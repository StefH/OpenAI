using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSearch.Redis.Database.Models;
using PdfSearch.SQL.Math;

namespace PdfSearch.SQL.Models;

public class CosineSearchContext : DbContext
{
    private readonly ILogger<CosineSearchContext> _logger;

    public CosineSearchContext(ILogger<CosineSearchContext> logger)
    {
        _logger = logger;
    }

    //public CosineSearchContext(DbContextOptions<CosineSearchContext> options)
    //    : base(options)
    //{
    //}

    public virtual DbSet<TextFragment> TextFragments { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if MSSQL
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CosineSearch;Trusted_Connection=True;");
#elif SQLITE
        optionsBuilder.UseSqlite("Data Source=../../../CosineSearch.db");
#else
        throw new Exception("No database provider specified");
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TextFragment>(entity =>
        {
#if MSSQL
            entity.HasKey(e => e.Id).HasName("PK__HashEntr__3214EC0723A933CF");
#elif SQLITE
            entity.HasKey(e => e.Id);
#else
            throw new Exception("No database provider specified");
#endif

            entity.Property(e => e.Prefix).HasMaxLength(128);
        });

        // OnModelCreatingPartial(modelBuilder);
    }

    // partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<IReadOnlyList<VectorDocument>> SearchAsync(string prefix, float[] questionAsVector)
    {
        _logger.LogInformation("Doing a search for prefix {prefix}", prefix);

        var questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();
        var (questionVector, questionMagnitude) = MathHelper.GetQuestionVectorData(questionAsBytes);

        var hashEntries = await TextFragments
            .AsNoTracking()
            .Where(h => h.Prefix == prefix)
            .ToArrayAsync();

        var searchResult = new ConcurrentBag<(TextFragment HashEntry, double CosineSimilarity)>();
        Parallel.ForEach(hashEntries, hashEntry =>
        {
            var embeddingVectorAsFloats = MemoryMarshal.Cast<byte, float>(hashEntry.EmbeddingAsBinary).ToArray();
            var embeddingVector = Vector<float>.Build.DenseOfArray(embeddingVectorAsFloats);

            var cosineSimilarity = MathHelper.CalculateCosineSimilarity(questionVector, questionMagnitude, embeddingVector);

            searchResult.Add((hashEntry, cosineSimilarity));
        });

        var sortedDocuments = searchResult
            .OrderByDescending(x => x.CosineSimilarity)
            .Take(5)
            .Select(document => new VectorDocument
            (
                Idx: document.HashEntry.Index,
                Text: document.HashEntry.Text,
                TokenLength: document.HashEntry.Tokens,
                Score: document.CosineSimilarity
            ))
            .OrderByDescending(document => document.Score)
            .ToArray();

        return sortedDocuments;
    }

    public async Task InsertAsync(
        string prefix,
        IReadOnlyList<string> textFragments,
        Func<string, Task<float[]>> embeddingFunc,
        Func<string, Task<IReadOnlyList<int>>> tokenFunc
    )
    {
        foreach (var item in textFragments.Select((textFragment, idx) => new { idx, textFragment }))
        {
            _logger.LogInformation("Inserting {idx}/{count}", item.idx, textFragments.Count);

            var embeddingTask = embeddingFunc(item.textFragment);
            var tokenTask = tokenFunc(item.textFragment);

            await Task.WhenAll(embeddingTask, tokenTask);

            var embeddings = await embeddingTask;
            var tokens = await tokenTask;

            var embeddingAsBinary = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

            var hashEntry = new TextFragment
            {
                Prefix = prefix,
                Index = item.idx,
                Text = item.textFragment,
                Tokens = tokens.Count,
                EmbeddingAsBinary = embeddingAsBinary
            };

            await TextFragments.AddAsync(hashEntry);

            if (item.idx % 10 == 0)
            {
                await SaveChangesAsync();
            }
        };

        await SaveChangesAsync();
    }
}

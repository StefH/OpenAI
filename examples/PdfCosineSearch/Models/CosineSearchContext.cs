using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using LangChain.Example.Redis.Models;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.EntityFrameworkCore;
using PdfCosineSearch.Math;

namespace PdfCosineSearch.Models;

public class CosineSearchContext : DbContext
{
    //public CosineSearchContext()
    //{
    //}

    //public CosineSearchContext(DbContextOptions<CosineSearchContext> options)
    //    : base(options)
    //{
    //}

    public virtual DbSet<HashEntry> HashEntries { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CosineSearch;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HashEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HashEntr__3214EC0723A933CF");

            entity.Property(e => e.Prefix).HasMaxLength(128);
        });

        // OnModelCreatingPartial(modelBuilder);
    }

    // partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<IReadOnlyList<VectorDocument>> SearchAsync(string prefix, float[] questionAsVector)
    {
        var questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();
        var (questionVector, questionMagnitude) = MathHelper.GetQuestionVectorData(questionAsBytes);

        var hashEntries = await HashEntries.Where(h => h.Prefix == prefix).ToArrayAsync();

        var searchResult = new ConcurrentBag<(HashEntry HashEntry, double CosineSimilarity)>();
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
        IReadOnlyList<string> parts,
        Func<string, Task<float[]>> embeddingFunc,
        Func<string, Task<IReadOnlyList<int>>> tokenFunc
    )
    {
        var items = parts.Select((part, idx) => new { idx, part }).ToArray();

        foreach (var item in items)
        {
            Console.WriteLine("{0}/{1}", item.idx, parts.Count);

            var embeddingTask = embeddingFunc(item.part);
            var tokenTask = tokenFunc(item.part);

            await Task.WhenAll(embeddingTask, tokenTask);

            var embeddings = await embeddingTask;
            var tokens = await tokenTask;

            var hashEntry = new HashEntry
            {
                Prefix = prefix,
                Index = item.idx,
                Text = item.part,
                Tokens = tokens.Count,
                EmbeddingAsBinary = MemoryMarshal.Cast<float, byte>(embeddings).ToArray()
            };

            await HashEntries.AddAsync(hashEntry);

            if (item.idx % 10 == 0)
            {
                await SaveChangesAsync();
            }
        };

        await SaveChangesAsync();
    }
}

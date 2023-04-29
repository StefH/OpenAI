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

    public virtual DbSet<HashEntry> HashEntries { get; set; }

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

        var searchResult = new List<(HashEntry HashEntry, double CosineSimilarity)>();
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

        foreach (var x in items)
        {
            Console.WriteLine("{0}/{1}", x.idx, parts.Count);

            var embeddingTask = embeddingFunc(x.part);
            var tokenTask = tokenFunc(x.part);

            await Task.WhenAll(embeddingTask, tokenTask);

            var embeddings = await embeddingTask;
            var tokens = await tokenTask;
            var byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

            var hashEntry = new HashEntry
            {
                Prefix = prefix,
                Index = x.idx,
                Text = x.part,
                Tokens = tokens.Count,
                EmbeddingAsBinary = byteArray
            };

            await HashEntries.AddAsync(hashEntry);

            if (x.idx % 10 == 0)
            {
                await SaveChangesAsync();
            }
        };

        await SaveChangesAsync();
    }
}

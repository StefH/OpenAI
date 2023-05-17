using PdfSearch.Redis.Database.Models;

namespace PdfSearch.Redis.Database;

internal interface IRedisDatabaseService
{
    bool DoesDataExists(string prefix);

    Task InsertAsync(string indexName, string prefix, IReadOnlyList<string> textFragments, Func<string, Task<float[]>> embeddingFunc, Func<string, Task<IReadOnlyList<int>>> tokenFunc);

    Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vectorAsBytes);
}
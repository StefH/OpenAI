using LangChain.Example.Redis.Models;

namespace LangChain.Example.Redis;

internal interface IRedisDatabaseService
{
    bool DoesDataExists(string prefix);

    Task InsertAsync(string indexName, string prefix, IReadOnlyList<string> parts, Func<string, Task<float[]>> embeddingFunc, Func<string, Task<IReadOnlyList<int>>> tokenFunc);

    Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vector);
}
using LangChain.Example.Redis.Models;

namespace LangChain.Example.Redis;

internal interface IRedisDatabaseService
{
    bool DoesDataExists(string prefix);

    Task InsertAsync(string indexName, string prefix, IReadOnlyList<string> parts, Func<string, Task<float[]>> func);

    Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vector);
}
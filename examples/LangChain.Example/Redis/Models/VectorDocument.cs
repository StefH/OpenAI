namespace LangChain.Example.Redis.Models;

public record VectorDocument(int Idx, string Text, float Score)
{
}
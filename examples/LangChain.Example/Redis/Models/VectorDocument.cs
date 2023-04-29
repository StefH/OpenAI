namespace LangChain.Example.Redis.Models;

public record VectorDocument(int Idx, string Text, int TokenLength, double Score);
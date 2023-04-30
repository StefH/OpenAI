namespace PdfSearch.Redis.Database.Models;

public record VectorDocument(int Idx, string Text, int TokenLength, double Score);
﻿namespace PdfSearch.SQL.Models;

public class TextFragment
{
    public int Id { get; set; }

    public string Prefix { get; set; } = null!;

    public int Index { get; set; }

    public string Text { get; set; } = null!;

    public int Tokens { get; set; }

    public byte[] EmbeddingAsBinary { get; set; } = null!;
}

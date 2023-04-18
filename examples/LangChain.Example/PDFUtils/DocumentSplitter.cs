using System.Text;
using LangChain.Example.TextUtils;
using UglyToad.PdfPig;

namespace LangChain.Example.PDFUtils;

public class DocumentSplitter : IDocumentSplitter
{
    public IReadOnlyList<string> Split(string filePath)
    {
        var stringBuilder = new StringBuilder();
        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages().Where(p => !string.IsNullOrWhiteSpace(p.Text)))
            {
                stringBuilder.Append(page.Text);
            }
        }

        return new RecursiveCharacterTextSplitter().SplitText(stringBuilder.ToString());
    }
}
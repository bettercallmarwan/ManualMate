using System.Text;
using UglyToad.PdfPig;

namespace ManualMate.Services
{
    public static class PdfExtractor
    {
        public static string ExtractTextFromPdf(string path)
        {
            using var document = PdfDocument.Open(path);
            var text = new StringBuilder();

            foreach(var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
    }
}

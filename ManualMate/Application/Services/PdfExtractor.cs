using ManualMate.API.Controllers.Responses;
using System.Text;
using UglyToad.PdfPig;

namespace ManualMate.Application.Services
{
    public static class PdfExtractor
    {
        public static Result<string> ExtractTextFromPdf(string path)
        {
            try
            {
                using var document = PdfDocument.Open(path);
                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return Result<string>.Ok(text.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

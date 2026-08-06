using System.Net;
using System.Text;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Responses;
using UglyToad.PdfPig;

namespace ManualMate.Infrastructure.Services
{
    public class PdfTextExtractor : IPdfTextExtractor
    {
        public Result<string> ExtractTextFromPdf(string path)
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
            catch (Exception ex)
            {
                return Result<string>.Fail("Unexpected error while extracting text from uploaded file: " + ex.Message, HttpStatusCode.InternalServerError);
            }      
        }
    }
}

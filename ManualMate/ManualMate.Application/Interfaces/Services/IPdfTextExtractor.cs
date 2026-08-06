using ManualMate.Application.Responses;

namespace ManualMate.Application.Interfaces.Services;

public interface IPdfTextExtractor
{
    Result<string> ExtractTextFromPdf(string path);
}
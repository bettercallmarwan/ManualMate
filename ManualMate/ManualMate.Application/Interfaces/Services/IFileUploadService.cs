using ManualMate.Application.Responses;
using Microsoft.AspNetCore.Http;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IFileUploadService
    {
        Task<Result<string>> UploadItemFileAsync(Guid itemId, IFormFile file, CancellationToken cancellationToken = default);
        Task<Result<string>> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}

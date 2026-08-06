using System.Net;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManualMate.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf"
        };

        private const long MaxFileSizeInBytes = 50 * 1024 * 1024;

        private readonly IApplicationDbContext _dbContext;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string _filesDirectory;

        public FileUploadService(IApplicationDbContext dbContext,
            IHostEnvironment environment,
            ILogger<FileUploadService> logger)
        {
            _dbContext = dbContext;
            _environment = environment;
            _logger = logger;
            _filesDirectory = Path.Combine(_environment.ContentRootPath, "wwwroot", "Files");
        }

        public async Task<Result<string>> UploadItemFileAsync(Guid itemId, IFormFile file, CancellationToken cancellationToken = default)
        {
            var validationError = ValidateFile(file);
            if (validationError is not null)
            {
                return Result<string>.Fail(validationError.Value.Error, validationError.Value.StatusCode);
            }

            var item = await _dbContext.Items.FindAsync(new object?[] { itemId }, cancellationToken);
            if (item is null)
            {
                return Result<string>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{itemId}_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(_filesDirectory, fileName);
            var oldFilePath = item.FilePath;

            try
            {
                await SaveFileAsync(file, filePath, cancellationToken);

                item.FilePath = filePath;
                item.LastUpdated = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                await DeleteFileBestEffortAsync(oldFilePath, cancellationToken);

                return Result<string>.Ok(filePath);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await DeleteFileBestEffortAsync(filePath, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await DeleteFileBestEffortAsync(filePath, cancellationToken);
                _logger.LogError(ex, "Failed to upload file for item {ItemId}", itemId);
                return Result<string>.Fail("Cannot upload file", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var validationError = ValidateFile(file);
            if (validationError is not null)
            {
                return Result<string>.Fail(validationError.Value.Error, validationError.Value.StatusCode);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var fileName = $"{safeName}_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(_filesDirectory, fileName);

            try
            {
                return Result<string>.Ok(await SaveFileAsync(file, filePath, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await DeleteFileBestEffortAsync(filePath, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await DeleteFileBestEffortAsync(filePath, cancellationToken);
                _logger.LogError(ex, "Failed to upload file {FileName}", file.FileName);
                return Result<string>.Fail("Cannot upload file", HttpStatusCode.InternalServerError);
            }
        }

        private (string Error, HttpStatusCode StatusCode)? ValidateFile(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return ("File Is Empty or Null", HttpStatusCode.BadRequest);
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                return ($"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / (1024 * 1024)} MB",
                    HttpStatusCode.BadRequest);
            }

            var fileExtension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return ($"File type {fileExtension} is not allowed", HttpStatusCode.BadRequest);
            }

            return null;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string filePath, CancellationToken cancellationToken)
        {
            EnsureDirectoryExists(_filesDirectory);

            await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return filePath;
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private async Task DeleteFileBestEffortAsync(string? filePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {FilePath}", filePath);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "file";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(safeName) ? "file" : safeName;
        }
    }
}

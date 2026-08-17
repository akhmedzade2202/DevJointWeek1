using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Infrastructure.Services;

/// <summary>
/// Stores uploaded files on the local file system.
/// Validates MIME type / extension and maximum size before saving.
/// </summary>
public class LocalFileService : IFileService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly HashSet<string> _allowedExtensions;
    private readonly ILogger<LocalFileService> _logger;

    public LocalFileService(IConfiguration configuration, ILogger<LocalFileService> logger)
    {
        _logger = logger;

        _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads";
        _maxFileSizeBytes = configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 5_242_880); // 5 MB default

        var extensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                         ?? new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        _allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

        // Ensure the upload directory exists.
        Directory.CreateDirectory(_uploadPath);
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was provided or the file is empty.");

        // --- Size validation ---
        if (file.Length > _maxFileSizeBytes)
            throw new InvalidOperationException(
                $"File size {file.Length / 1024} KB exceeds the maximum allowed {_maxFileSizeBytes / 1024} KB.");

        // --- Extension validation ---
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

        // --- Generate a unique file name to prevent collisions and path traversal ---
        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadPath, uniqueName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("File uploaded: {OriginalName} → {StoredName} ({Size} bytes)",
            file.FileName, uniqueName, file.Length);

        return uniqueName;
    }

    /// <inheritdoc/>
    public string GetFilePath(string fileName)
    {
        // Prevent path traversal: strip any directory components from the file name.
        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_uploadPath, safeName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File '{safeName}' was not found.");

        return fullPath;
    }

    /// <inheritdoc/>
    public bool Delete(string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_uploadPath, safeName);

        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        _logger.LogInformation("File deleted: {FileName}", safeName);
        return true;
    }
}

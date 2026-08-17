using Microsoft.AspNetCore.Http;

namespace LibraryApi.Application.Interfaces.Services;

/// <summary>
/// Handles file upload and download operations for book cover images or documents.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Validates and saves the uploaded file. Returns the stored file name.
    /// </summary>
    Task<string> UploadAsync(IFormFile file);

    /// <summary>
    /// Returns the absolute path of a stored file. Throws <see cref="FileNotFoundException"/> if not found.
    /// </summary>
    string GetFilePath(string fileName);

    /// <summary>
    /// Deletes a stored file. Returns false if the file did not exist.
    /// </summary>
    bool Delete(string fileName);
}

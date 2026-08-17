using LibraryApi.Application.DTOs.Book;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

/// <summary>
/// Manages library books: CRUD operations, file uploads, and file downloads.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IFileService _fileService;

    public BooksController(IBookService bookService, IFileService fileService)
    {
        _bookService = bookService;
        _fileService = fileService;
    }

    // ──────────────────────────────────────────────────────────────
    // CRUD
    // ──────────────────────────────────────────────────────────────

    /// <summary>Returns all books. Response is served from cache when available.</summary>
    /// <response code="200">List of books (may be cached).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    /// <summary>Returns a single book by its id. Response is served from cache when available.</summary>
    /// <param name="id">Book identifier.</param>
    /// <response code="200">The requested book.</response>
    /// <response code="404">Book not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return Ok(book);
    }

    /// <summary>Creates a new book. Requires Admin role. Invalidates the books cache.</summary>
    /// <response code="201">Book created successfully.</response>
    /// <response code="400">Validation error (duplicate ISBN, invalid author, etc.).</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        try
        {
            var created = await _bookService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Updates an existing book. Requires Admin role. Invalidates the books cache.</summary>
    /// <param name="id">Book identifier.</param>
    /// <param name="dto">Updated book data.</param>
    /// <response code="204">Book updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Book not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateBookDto dto)
    {
        try
        {
            var updated = await _bookService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a book. Requires Admin role. Invalidates the books cache.</summary>
    /// <param name="id">Book identifier.</param>
    /// <response code="204">Book deleted successfully.</response>
    /// <response code="404">Book not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ──────────────────────────────────────────────────────────────
    // FILE UPLOAD / DOWNLOAD
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Uploads a file (e.g. book cover image or PDF) for a specific book.
    /// Allowed types: .pdf, .jpg, .jpeg, .png. Maximum size: 5 MB (dev: 10 MB).
    /// Requires Admin role.
    /// </summary>
    /// <param name="id">Book identifier the file belongs to.</param>
    /// <param name="file">Multipart form-data file.</param>
    /// <response code="200">Upload successful. Returns the stored file name.</response>
    /// <response code="400">File validation failed (wrong type, too large, empty).</response>
    /// <response code="404">Book not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadFile(int id, IFormFile file)
    {
        // Verify the book exists before attaching a file.
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound(new { message = $"Book {id} not found." });

        try
        {
            var storedName = await _fileService.UploadAsync(file);
            return Ok(new FileUploadResponseDto
            {
                BookId = id,
                OriginalFileName = file.FileName,
                StoredFileName = storedName,
                FileSizeBytes = file.Length,
                DownloadUrl = Url.Action(nameof(DownloadFile), "Books", new { id, fileName = storedName }, Request.Scheme)!
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Downloads a previously uploaded file for a specific book.
    /// </summary>
    /// <param name="id">Book identifier.</param>
    /// <param name="fileName">Stored file name returned by the upload endpoint.</param>
    /// <response code="200">Binary file stream.</response>
    /// <response code="404">Book or file not found.</response>
    [HttpGet("{id:int}/download/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(int id, string fileName)
    {
        // Verify the book exists.
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound(new { message = $"Book {id} not found." });

        try
        {
            var filePath = _fileService.GetFilePath(fileName);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = GetContentType(extension);

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = $"File '{fileName}' not found." });
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private static string GetContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };
}

/// <summary>Response returned after a successful file upload.</summary>
public class FileUploadResponseDto
{
    /// <summary>The book this file is associated with.</summary>
    public int BookId { get; set; }

    /// <summary>Original file name provided by the client.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Unique file name used for storage (use this for downloads).</summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Direct URL to download this file.</summary>
    public string DownloadUrl { get; set; } = string.Empty;
}

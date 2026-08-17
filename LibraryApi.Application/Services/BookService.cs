using LibraryApi.Application.DTOs.Book;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Services;

/// <summary>
/// Handles book business logic with in-memory caching on read operations.
/// Cache is invalidated on any write (create/update/delete).
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cache;

    // Cache key constants — centralised so invalidation is consistent.
    private const string CacheKeyAll = "books:all";
    private const string CacheKeyByIdPrefix = "books:id:";

    public BookService(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        ICategoryRepository categoryRepository,
        ICacheService cache)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    /// <summary>
    /// Returns all books. Result is cached; subsequent calls skip the database.
    /// </summary>
    public Task<IEnumerable<BookDto>> GetAllAsync()
    {
        return _cache.GetOrCreateAsync(CacheKeyAll, async () =>
        {
            var books = _bookRepository.Query().Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Isbn = b.Isbn,
                PublishedYear = b.PublishedYear,
                AuthorId = b.AuthorId,
                AuthorFullName = b.Author.FirstName + " " + b.Author.LastName,
                Categories = b.Categories.Select(c => c.Name).ToList()
            });
            return (IEnumerable<BookDto>)await Task.FromResult(books.ToList());
        });
    }

    /// <summary>
    /// Returns a single book by id. Result is cached per-id.
    /// </summary>
    public Task<BookDto?> GetByIdAsync(int id)
    {
        return _cache.GetOrCreateAsync($"{CacheKeyByIdPrefix}{id}", async () =>
        {
            var book = await _bookRepository.GetByIdWithDetailsAsync(id);
            return book == null ? null : MapToDto(book);
        });
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId);
        if (!authorExists)
            throw new InvalidOperationException($"Author with id {dto.AuthorId} not found.");

        var isbnExists = await _bookRepository.IsbnExistsAsync(dto.Isbn);
        if (isbnExists)
            throw new InvalidOperationException($"Book with ISBN {dto.Isbn} already exists.");

        var categories = await _categoryRepository.GetByIdsAsync(dto.CategoryIds);

        var book = new Book
        {
            Title = dto.Title,
            Isbn = dto.Isbn,
            PublishedYear = dto.PublishedYear,
            AuthorId = dto.AuthorId,
            Categories = categories
        };

        var created = await _bookRepository.AddAsync(book);
        var withDetails = await _bookRepository.GetByIdWithDetailsAsync(created.Id);

        // Invalidate the "all books" list cache so the new entry appears on the next read.
        _cache.Remove(CacheKeyAll);

        return MapToDto(withDetails!);
    }

    public async Task<bool> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId);
        if (!authorExists)
            throw new InvalidOperationException($"Author with id {dto.AuthorId} not found.");

        var isbnExists = await _bookRepository.IsbnExistsAsync(dto.Isbn, id);
        if (isbnExists)
            throw new InvalidOperationException($"Book with ISBN {dto.Isbn} already exists.");

        book.Title = dto.Title;
        book.Isbn = dto.Isbn;
        book.PublishedYear = dto.PublishedYear;
        book.AuthorId = dto.AuthorId;

        await _bookRepository.UpdateAsync(book);

        // Invalidate both the list cache and the specific-item cache.
        _cache.Remove(CacheKeyAll);
        _cache.Remove($"{CacheKeyByIdPrefix}{id}");

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        await _bookRepository.DeleteAsync(book);

        // Invalidate both the list cache and the specific-item cache.
        _cache.Remove(CacheKeyAll);
        _cache.Remove($"{CacheKeyByIdPrefix}{id}");

        return true;
    }

    private static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Isbn = book.Isbn,
        PublishedYear = book.PublishedYear,
        AuthorId = book.AuthorId,
        AuthorFullName = book.Author != null ? $"{book.Author.FirstName} {book.Author.LastName}" : null,
        Categories = book.Categories?.Select(c => c.Name).ToList() ?? new List<string>()
    };
}

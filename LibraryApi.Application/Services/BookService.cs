using LibraryApi.Application.DTOs.Book;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;

    public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllAsync()
    {
        var books = _bookRepository.Query().Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Isbn = b.Isbn,
            PublishedYear = b.PublishedYear,
            AuthorId = b.AuthorId,
            AuthorFullName = b.Author.FirstName + " " + b.Author.LastName
        });
        return await Task.FromResult(books.ToList());
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdWithDetailsAsync(id);
        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId);
        if (!authorExists)
            throw new InvalidOperationException($"Author with id {dto.AuthorId} not found.");

        var isbnExists = await _bookRepository.IsbnExistsAsync(dto.Isbn);
        if (isbnExists)
            throw new InvalidOperationException($"Book with ISBN {dto.Isbn} already exists.");

        var book = new Book
        {
            Title = dto.Title,
            Isbn = dto.Isbn,
            PublishedYear = dto.PublishedYear,
            AuthorId = dto.AuthorId
        };
        var created = await _bookRepository.AddAsync(book);
        var withDetails = await _bookRepository.GetByIdWithDetailsAsync(created.Id);
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
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        await _bookRepository.DeleteAsync(book);
        return true;
    }

    private static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Isbn = book.Isbn,
        PublishedYear = book.PublishedYear,
        AuthorId = book.AuthorId,
        AuthorFullName = book.Author != null ? $"{book.Author.FirstName} {book.Author.LastName}" : null
    };
}
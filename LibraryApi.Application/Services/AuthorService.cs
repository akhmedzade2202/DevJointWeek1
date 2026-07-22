using LibraryApi.Application.DTOs.Author;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<IEnumerable<AuthorDto>> GetAllAsync()
    {
        var authors = await _authorRepository.GetAllAsync();
        return authors.Select(MapToDto);
    }

    public async Task<AuthorDto?> GetByIdAsync(int id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        return author == null ? null : MapToDto(author);
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate
        };
        var created = await _authorRepository.AddAsync(author);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateAuthorDto dto)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null) return false;

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.BirthDate = dto.BirthDate;

        await _authorRepository.UpdateAsync(author);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null) return false;

        await _authorRepository.DeleteAsync(author);
        return true;
    }

    private static AuthorDto MapToDto(Author author) => new()
    {
        Id = author.Id,
        FirstName = author.FirstName,
        LastName = author.LastName,
        BirthDate = author.BirthDate
    };
}
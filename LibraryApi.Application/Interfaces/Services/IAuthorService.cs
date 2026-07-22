using LibraryApi.Application.DTOs.Author;

namespace LibraryApi.Application.Interfaces.Services;

public interface IAuthorService
{
    Task<IEnumerable<AuthorDto>> GetAllAsync();
    Task<AuthorDto?> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(CreateAuthorDto dto);
    Task<bool> UpdateAsync(int id, UpdateAuthorDto dto);
    Task<bool> DeleteAsync(int id);
}
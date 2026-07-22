using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Repositories;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIdWithDetailsAsync(int id);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null);
}
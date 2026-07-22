using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Repositories;

public interface IAuthorRepository : IGenericRepository<Author>
{
    Task<Author?> GetByIdWithBooksAsync(int id);
}
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetByIdsAsync(List<int> ids);
}
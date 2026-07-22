using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Repositories;

public interface IMemberRepository : IGenericRepository<Member>
{
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
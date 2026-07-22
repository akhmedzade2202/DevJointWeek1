using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Repositories;

public class MemberRepository : GenericRepository<Member>, IMemberRepository
{
    public MemberRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
        await _dbSet.AnyAsync(m => m.Email == email && (!excludeId.HasValue || m.Id != excludeId));
}
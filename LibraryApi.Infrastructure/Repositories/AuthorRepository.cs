using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Repositories;

public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
{
    public AuthorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Author?> GetByIdWithBooksAsync(int id) =>
        await _dbSet.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
}
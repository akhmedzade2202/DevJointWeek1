using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Book?> GetByIdWithDetailsAsync(int id) =>
        await _dbSet
            .Include(b => b.Author)
            .Include(b => b.Categories) 
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null) =>
        await _dbSet.AnyAsync(b => b.Isbn == isbn && (!excludeId.HasValue || b.Id != excludeId));
}
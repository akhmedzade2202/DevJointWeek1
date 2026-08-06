using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetByIdsAsync(List<int> ids)
    {
        return await _context.Categories.Where(c => ids.Contains(c.Id)).ToListAsync();
    }
}
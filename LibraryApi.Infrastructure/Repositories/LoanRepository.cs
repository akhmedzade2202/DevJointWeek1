using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Repositories;

public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    public LoanRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Loan?> GetByIdWithDetailsAsync(int id) =>
        await _dbSet.Include(l => l.Book).Include(l => l.Member).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Loan>> GetActiveLoansAsync() =>
        await _dbSet.Include(l => l.Book).Include(l => l.Member)
            .Where(l => l.ReturnDate == null).ToListAsync();
}
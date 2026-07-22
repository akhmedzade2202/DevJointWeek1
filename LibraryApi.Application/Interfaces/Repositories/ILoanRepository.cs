using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Interfaces.Repositories;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<Loan?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Loan>> GetActiveLoansAsync();
}
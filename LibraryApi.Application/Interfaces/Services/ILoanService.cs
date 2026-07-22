using LibraryApi.Application.DTOs.Loan;

namespace LibraryApi.Application.Interfaces.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllAsync();
    Task<LoanDto?> GetByIdAsync(int id);
    Task<LoanDto> CreateAsync(CreateLoanDto dto);
    Task<bool> ReturnAsync(int id, ReturnLoanDto dto);
    Task<bool> DeleteAsync(int id);
}
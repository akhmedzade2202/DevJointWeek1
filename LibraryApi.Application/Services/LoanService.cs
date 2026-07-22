using LibraryApi.Application.DTOs.Loan;
using LibraryApi.Application.Interfaces.Repositories;
using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Domain.Entities;

namespace LibraryApi.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;

    public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository, IMemberRepository memberRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<LoanDto>> GetAllAsync()
    {
        var loans = _loanRepository.Query().Select(l => new LoanDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            ReturnDate = l.ReturnDate,
            BookId = l.BookId,
            BookTitle = l.Book.Title,
            MemberId = l.MemberId,
            MemberFullName = l.Member.FirstName + " " + l.Member.LastName
        });
        return await Task.FromResult(loans.ToList());
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _loanRepository.GetByIdWithDetailsAsync(id);
        return loan == null ? null : MapToDto(loan);
    }

    public async Task<LoanDto> CreateAsync(CreateLoanDto dto)
    {
        var bookExists = await _bookRepository.ExistsAsync(dto.BookId);
        if (!bookExists)
            throw new InvalidOperationException($"Book with id {dto.BookId} not found.");

        var memberExists = await _memberRepository.ExistsAsync(dto.MemberId);
        if (!memberExists)
            throw new InvalidOperationException($"Member with id {dto.MemberId} not found.");

        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = dto.MemberId,
            LoanDate = DateTime.UtcNow,
            ReturnDate = null
        };
        var created = await _loanRepository.AddAsync(loan);
        var withDetails = await _loanRepository.GetByIdWithDetailsAsync(created.Id);
        return MapToDto(withDetails!);
    }

    public async Task<bool> ReturnAsync(int id, ReturnLoanDto dto)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null) return false;

        loan.ReturnDate = dto.ReturnDate;
        await _loanRepository.UpdateAsync(loan);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null) return false;

        await _loanRepository.DeleteAsync(loan);
        return true;
    }

    private static LoanDto MapToDto(Loan loan) => new()
    {
        Id = loan.Id,
        LoanDate = loan.LoanDate,
        ReturnDate = loan.ReturnDate,
        BookId = loan.BookId,
        BookTitle = loan.Book?.Title,
        MemberId = loan.MemberId,
        MemberFullName = loan.Member != null ? $"{loan.Member.FirstName} {loan.Member.LastName}" : null
    };
}
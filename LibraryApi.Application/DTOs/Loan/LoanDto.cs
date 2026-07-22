namespace LibraryApi.Application.DTOs.Loan;

public class LoanDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    public int MemberId { get; set; }
    public string? MemberFullName { get; set; }
}

public class CreateLoanDto
{
    public int BookId { get; set; }
    public int MemberId { get; set; }
}

public class ReturnLoanDto
{
    public DateTime ReturnDate { get; set; }
}
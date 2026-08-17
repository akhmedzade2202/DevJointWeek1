using LibraryApi.Application.DTOs.Loan;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

/// <summary>
/// Manages book loans: borrow, return, and tracking.
/// Async email notifications are fired without blocking the response.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>Returns all loans.</summary>
    /// <response code="200">List of all loans.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
    {
        var loans = await _loanService.GetAllAsync();
        return Ok(loans);
    }

    /// <summary>Returns a single loan by id.</summary>
    /// <param name="id">Loan identifier.</param>
    /// <response code="200">The requested loan.</response>
    /// <response code="404">Loan not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        if (loan == null) return NotFound();
        return Ok(loan);
    }

    /// <summary>
    /// Creates a new loan. Triggers an asynchronous email confirmation (fire-and-forget).
    /// Requires Admin role.
    /// </summary>
    /// <param name="dto">Book and member identifiers.</param>
    /// <response code="201">Loan created; confirmation email sent asynchronously.</response>
    /// <response code="400">Book or member not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> Create(CreateLoanDto dto)
    {
        try
        {
            var created = await _loanService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Marks a loan as returned. Requires Admin role.</summary>
    /// <param name="id">Loan identifier.</param>
    /// <param name="dto">Return date.</param>
    /// <response code="204">Loan returned successfully.</response>
    /// <response code="404">Loan not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Return(int id, ReturnLoanDto dto)
    {
        var updated = await _loanService.ReturnAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    /// <summary>Deletes a loan record. Requires Admin role.</summary>
    /// <param name="id">Loan identifier.</param>
    /// <response code="204">Loan deleted successfully.</response>
    /// <response code="404">Loan not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _loanService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

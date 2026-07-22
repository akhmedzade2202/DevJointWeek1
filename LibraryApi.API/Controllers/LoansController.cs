using LibraryApi.Application.DTOs.Loan;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
    {
        var loans = await _loanService.GetAllAsync();
        return Ok(loans);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        if (loan == null) return NotFound();
        return Ok(loan);
    }

    [HttpPost]
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

    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> Return(int id, ReturnLoanDto dto)
    {
        var updated = await _loanService.ReturnAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _loanService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
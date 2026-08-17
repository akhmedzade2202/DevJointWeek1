using LibraryApi.Application.DTOs.Member;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

/// <summary>
/// Manages library members (borrowers).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    /// <summary>Returns all members.</summary>
    /// <response code="200">List of all members.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAll()
    {
        var members = await _memberService.GetAllAsync();
        return Ok(members);
    }

    /// <summary>Returns a single member by id.</summary>
    /// <param name="id">Member identifier.</param>
    /// <response code="200">The requested member.</response>
    /// <response code="404">Member not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();
        return Ok(member);
    }

    /// <summary>Creates a new member. Requires Admin role.</summary>
    /// <param name="dto">Member data.</param>
    /// <response code="201">Member created successfully.</response>
    /// <response code="400">Email already in use.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        try
        {
            var created = await _memberService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Updates an existing member. Requires Admin role.</summary>
    /// <param name="id">Member identifier.</param>
    /// <param name="dto">Updated member data.</param>
    /// <response code="204">Member updated successfully.</response>
    /// <response code="400">Email already in use.</response>
    /// <response code="404">Member not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateMemberDto dto)
    {
        try
        {
            var updated = await _memberService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a member. Requires Admin role.</summary>
    /// <param name="id">Member identifier.</param>
    /// <response code="204">Member deleted successfully.</response>
    /// <response code="404">Member not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _memberService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

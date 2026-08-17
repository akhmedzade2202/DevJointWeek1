using LibraryApi.Application.DTOs.Author;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

/// <summary>
/// Manages library authors.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    /// <summary>Returns all authors.</summary>
    /// <response code="200">List of authors.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAll()
    {
        var authors = await _authorService.GetAllAsync();
        return Ok(authors);
    }

    /// <summary>Returns a single author by id.</summary>
    /// <param name="id">Author identifier.</param>
    /// <response code="200">The requested author.</response>
    /// <response code="404">Author not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorDto>> GetById(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author == null) return NotFound();
        return Ok(author);
    }

    /// <summary>Creates a new author. Requires Admin role.</summary>
    /// <param name="dto">Author data.</param>
    /// <response code="201">Author created successfully.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthorDto>> Create(CreateAuthorDto dto)
    {
        var created = await _authorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing author. Requires Admin role.</summary>
    /// <param name="id">Author identifier.</param>
    /// <param name="dto">Updated author data.</param>
    /// <response code="204">Author updated successfully.</response>
    /// <response code="404">Author not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateAuthorDto dto)
    {
        var updated = await _authorService.UpdateAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    /// <summary>Deletes an author. Requires Admin role.</summary>
    /// <param name="id">Author identifier.</param>
    /// <response code="204">Author deleted successfully.</response>
    /// <response code="404">Author not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _authorService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

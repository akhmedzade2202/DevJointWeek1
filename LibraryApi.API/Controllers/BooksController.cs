using LibraryApi.Application.DTOs.Book;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LibraryApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return Ok(book);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        try
        {
            var created = await _bookService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBookDto dto)
    {
        try
        {
            var updated = await _bookService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using API_web.Models;

namespace API_web.Controllers;

/// <summary>
/// Manages todo items with role-based access control
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class TodoItemsController : ControllerBase
{
    private readonly TodoContext _context;

    public TodoItemsController(TodoContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all todo items
    /// </summary>
    /// <returns>A list of all todo items</returns>
    /// <response code="200">Returns the list of todo items</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    [HttpGet]
    [Authorize(Roles = "user,admin")]
    [ProducesResponseType(typeof(IEnumerable<TodoItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        var items = await _context.TodoItems.Find(_ => true).ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Get a specific todo item by ID
    /// </summary>
    /// <param name="id">The ID of the todo item to retrieve</param>
    /// <returns>The requested todo item</returns>
    /// <response code="200">Returns the requested todo item</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="404">Todo item not found</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "user,admin")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> GetTodoItem(string id)
    {
        var todoItem = await _context.TodoItems.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (todoItem == null)
        {
            return NotFound();
        }

        return todoItem;
    }

    /// <summary>
    /// Create a new todo item
    /// </summary>
    /// <param name="todoItem">The todo item to create</param>
    /// <returns>The created todo item</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/todoitems
    ///     {
    ///        "name": "Buy groceries",
    ///        "isComplete": false
    ///     }
    /// </remarks>
    /// <response code="201">Todo item successfully created</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
        await _context.TodoItems.InsertOneAsync(todoItem);
        return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
    }

    /// <summary>
    /// Update an existing todo item
    /// </summary>
    /// <param name="id">The ID of the todo item to update</param>
    /// <param name="todoItem">The updated todo item data</param>
    /// <returns>No content on success</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/todoitems/507f1f77bcf86cd799439011
    ///     {
    ///        "id": "507f1f77bcf86cd799439011",
    ///        "name": "Buy groceries and cook dinner",
    ///        "isComplete": true
    ///     }
    /// </remarks>
    /// <response code="204">Todo item successfully updated</response>
    /// <response code="400">Bad request - ID mismatch</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">Todo item not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutTodoItem(string id, TodoItem todoItem)
    {
        if (id != todoItem.Id)
        {
            return BadRequest();
        }

        var result = await _context.TodoItems.ReplaceOneAsync(x => x.Id == id, todoItem);

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a todo item
    /// </summary>
    /// <param name="id">The ID of the todo item to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Todo item successfully deleted</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">Todo item not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodoItem(string id)
    {
        var result = await _context.TodoItems.DeleteOneAsync(x => x.Id == id);

        if (result.DeletedCount == 0)
        {
            return NotFound();
        }

        return NoContent();
    }
}

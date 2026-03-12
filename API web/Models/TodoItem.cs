using System.ComponentModel.DataAnnotations;

namespace API_web.Models;

/// <summary>
/// Represents a todo item in the system
/// </summary>
public class TodoItem
{
    /// <summary>
    /// The unique identifier for the todo item
    /// </summary>
    /// <example>1</example>
    public long Id { get; set; }

    /// <summary>
    /// The name or description of the todo item
    /// </summary>
    /// <example>Buy groceries</example>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Indicates whether the todo item has been completed
    /// </summary>
    /// <example>false</example>
    public bool IsComplete { get; set; }
}

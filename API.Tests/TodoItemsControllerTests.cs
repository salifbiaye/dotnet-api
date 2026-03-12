using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using API_web.Models;

namespace API.Tests;

public class TodoItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TodoItemsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTodoItems_ReturnsOkStatus_WithJsonArray()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        Assert.NotNull(items);
        // Items list may or may not be empty depending on test execution order
    }

    [Fact]
    public async Task GetTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostTodoItem_CreatesNewItem_ReturnsCreatedWithLocation()
    {
        // Arrange
        var newItem = new TodoItem { Name = "Test Task", IsComplete = false };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        
        var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(createdItem);
        Assert.Equal("Test Task", createdItem.Name);
        Assert.False(createdItem.IsComplete);
        Assert.True(createdItem.Id > 0);
    }

    [Fact]
    public async Task PutTodoItem_UpdatesExistingItem_ReturnsNoContent()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem { Name = "Original Task", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(createdItem);
        
        // Update the item
        createdItem.Name = "Updated Task";
        createdItem.IsComplete = true;
        
        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{createdItem.Id}", createdItem);
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the update
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
        var updatedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(updatedItem);
        Assert.Equal("Updated Task", updatedItem.Name);
        Assert.True(updatedItem.IsComplete);
    }

    [Fact]
    public async Task PutTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var item = new TodoItem { Id = 999, Name = "Non-existent", IsComplete = false };
        
        // Act
        var response = await _client.PutAsJsonAsync("/api/todoitems/999", item);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTodoItem_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var item = new TodoItem { Id = 5, Name = "Task", IsComplete = false };
        
        // Act
        var response = await _client.PutAsJsonAsync("/api/todoitems/10", item);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodoItem_RemovesItem_ReturnsNoContent()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem { Name = "Task to Delete", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(createdItem);
        
        // Act
        var response = await _client.DeleteAsync($"/api/todoitems/{createdItem.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todoitems/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTodoItems_ReturnsAllItems_AfterMultipleCreations()
    {
        // Arrange - Create multiple items
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Name = "Task 1", IsComplete = false });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Name = "Task 2", IsComplete = true });
        await _client.PostAsJsonAsync("/api/todoitems", new TodoItem { Name = "Task 3", IsComplete = false });
        
        // Act
        var response = await _client.GetAsync("/api/todoitems");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        Assert.NotNull(items);
        Assert.True(items.Count >= 3);
    }
}


// Custom factory to ensure each test gets a fresh database
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-for-unit-tests-minimum-32-characters-long");
        builder.UseSetting("JWT_EXPIRATION_MINUTES", "60");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TodoContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new DbContext with a unique database name for this factory instance
            services.AddDbContext<TodoContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}

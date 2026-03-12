# Implementation Plan: TodoItems API

## Overview

This plan implements a RESTful TodoItems API using ASP.NET Core 9.0 with Entity Framework Core and an in-memory database. The implementation follows a controller-based approach with proper HTTP status codes, OpenAPI documentation, and dependency injection.

## Tasks

- [x] 1. Set up project structure and core models
  - Create ASP.NET Core Web API project with .NET 9.0
  - Add required NuGet packages (EF Core InMemory, OpenAPI)
  - Create Models folder and TodoItem entity class
  - Enable nullable reference types in project configuration
  - _Requirements: 6.1, 6.3_

- [ ] 2. Implement database context
  - [x] 2.1 Create TodoContext DbContext class
    - Implement constructor accepting DbContextOptions<TodoContext>
    - Define TodoItems DbSet property
    - _Requirements: 6.1, 6.2_
  
  - [x] 2.2 Configure DbContext in Program.cs
    - Register TodoContext with dependency injection
    - Configure InMemory database provider with name "TodoList"
    - _Requirements: 6.2, 6.3, 6.4_


- [ ] 3. Implement TodoItemsController with GET endpoints
  - [x] 3.1 Create TodoItemsController class
    - Add [Route("api/[controller]")] and [ApiController] attributes
    - Inject TodoContext via constructor
    - _Requirements: 1.1, 2.1_
  
  - [x] 3.2 Implement GET all todo items endpoint
    - Create GetTodoItems() method with [HttpGet] attribute
    - Return all TodoItems from database using async/await
    - Return 200 OK with JSON collection (including empty collections)
    - _Requirements: 1.1, 1.2, 1.3, 1.4_
  
  - [x] 3.3 Implement GET todo item by ID endpoint
    - Create GetTodoItem(long id) method with [HttpGet("{id}")] attribute
    - Query database for TodoItem by ID using async/await
    - Return 200 OK with TodoItem if found
    - Return 404 Not Found if ID doesn't exist
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 4. Checkpoint - Verify GET endpoints
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 5. Implement POST endpoint for creating todo items
  - [x] 5.1 Create PostTodoItem endpoint
    - Create PostTodoItem(TodoItem todoItem) method with [HttpPost] attribute
    - Add TodoItem to database context and save changes using async/await
    - Return 201 Created with Location header pointing to new resource
    - Return created TodoItem in response body
    - Handle invalid payloads with 400 Bad Request (automatic via model binding)
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_
  
  - [ ]* 5.2 Write unit tests for POST endpoint
    - Test successful creation with valid payload
    - Test Location header contains correct URI
    - Test invalid payload returns 400
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [ ] 6. Implement PUT endpoint for updating todo items
  - [x] 6.1 Create PutTodoItem endpoint
    - Create PutTodoItem(long id, TodoItem todoItem) method with [HttpPut("{id}")] attribute
    - Validate ID in URL matches ID in payload (return 400 if mismatch)
    - Check if TodoItem exists in database (return 404 if not found)
    - Update TodoItem properties and save changes using async/await
    - Handle DbUpdateConcurrencyException appropriately
    - Return 204 No Content on success
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_
  
  - [ ]* 6.2 Write unit tests for PUT endpoint
    - Test successful update with valid ID and payload
    - Test 404 when ID doesn't exist
    - Test 400 when URL ID doesn't match payload ID
    - Test 400 with invalid payload
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_


- [ ] 7. Implement DELETE endpoint for removing todo items
  - [x] 7.1 Create DeleteTodoItem endpoint
    - Create DeleteTodoItem(long id) method with [HttpDelete("{id}")] attribute
    - Check if TodoItem exists in database (return 404 if not found)
    - Remove TodoItem from database and save changes using async/await
    - Return 204 No Content on success
    - _Requirements: 5.1, 5.2, 5.3_
  
  - [ ]* 7.2 Write unit tests for DELETE endpoint
    - Test successful deletion with valid ID
    - Test 404 when ID doesn't exist
    - Test item is actually removed from database
    - _Requirements: 5.1, 5.2, 5.3_

- [x] 8. Checkpoint - Verify all CRUD operations
  - Ensure all tests pass, ask the user if questions arise.


- [x] 9. Configure OpenAPI and Swagger documentation
  - [x] 9.1 Add OpenAPI services in Program.cs
    - Add AddEndpointsApiExplorer() service
    - Add AddSwaggerGen() service
    - _Requirements: 7.1, 7.4_
  
  - [x] 9.2 Configure Swagger middleware
    - Add UseSwagger() middleware for OpenAPI document endpoint
    - Add UseSwaggerUI() middleware for interactive UI (Development only)
    - Ensure OpenAPI document available at /swagger/v1/swagger.json
    - _Requirements: 7.2, 7.3, 8.4_
  
  - [ ]* 9.3 Verify OpenAPI documentation completeness
    - Check all endpoints documented with correct HTTP methods
    - Verify request/response schemas included
    - Test Swagger UI functionality
    - _Requirements: 7.1, 7.2, 7.3, 7.4_


- [x] 10. Configure API middleware and settings
  - [x] 10.1 Add Controllers service
    - Add AddControllers() in service configuration
    - _Requirements: 8.3_
  
  - [x] 10.2 Configure middleware pipeline
    - Add UseHttpsRedirection() for HTTPS enforcement
    - Add UseCors() to enable cross-origin requests
    - Add UseAuthorization() for future auth support
    - Add MapControllers() to map controller endpoints
    - _Requirements: 8.1, 8.2, 8.3_
  
  - [x] 10.3 Verify middleware order
    - Ensure middleware configured in correct order
    - Verify HTTPS redirection works
    - Verify CORS headers present in responses
    - _Requirements: 8.1, 8.2, 8.3_

- [~] 11. Final checkpoint and integration verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- The API uses async/await throughout for better scalability
- InMemory database is suitable for development; can be swapped for persistent storage later
- All endpoints follow RESTful conventions with proper HTTP status codes

# Implementation Plan: API Security and Deployment

## Overview

This implementation plan extends the existing TodoItems API with JWT-based authentication, role-based authorization, and Docker deployment capabilities. The tasks build incrementally, starting with user management and authentication, then adding authorization to existing endpoints, and finally containerizing the application.

## Tasks

- [x] 1. Set up authentication infrastructure and user management
  - [x] 1.1 Create User model and update TodoContext
    - Add User.cs model with Id, Username, PasswordHash, and Role properties
    - Add Users DbSet to TodoContext
    - _Requirements: 8.2, 8.3, 8.6_
  
  - [x] 1.2 Create password hashing service
    - Implement IPasswordHasher interface
    - Implement PasswordHasher class using BCrypt or PBKDF2
    - Register service in Program.cs dependency injection
    - _Requirements: 8.2_
  
  - [x] 1.3 Create JWT token service
    - Implement IJwtTokenService interface
    - Implement JwtTokenService with GenerateToken and ValidateToken methods
    - Load JWT_SECRET_KEY and JWT_EXPIRATION_MINUTES from configuration
    - Include user ID, username, and role claims in tokens
    - _Requirements: 1.2, 1.4, 1.5, 1.6, 5.1, 5.2_

- [x] 2. Implement authentication endpoints
  - [x] 2.1 Create AuthController with registration endpoint
    - Implement POST /api/auth/register endpoint
    - Validate username uniqueness
    - Hash password before storing
    - Return appropriate status codes (201 Created, 400 Bad Request)
    - _Requirements: 8.1, 8.2, 8.4_
  
  - [x] 2.2 Create login endpoint in AuthController
    - Implement POST /api/auth/login endpoint
    - Validate credentials against stored users
    - Generate and return JWT token on success
    - Return 401 Unauthorized for invalid credentials with error message
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [ ]* 2.3 Write unit tests for AuthController
    - Test successful registration
    - Test duplicate username rejection
    - Test successful login with valid credentials
    - Test login failure with invalid credentials
    - _Requirements: 1.1, 1.3, 8.1, 8.4_

- [x] 3. Configure JWT authentication middleware
  - [x] 3.1 Add JWT authentication to Program.cs
    - Install Microsoft.AspNetCore.Authentication.JwtBearer NuGet package
    - Configure authentication services with JWT bearer options
    - Set token validation parameters (issuer signing key, validate lifetime)
    - Add UseAuthentication() middleware before UseAuthorization()
    - _Requirements: 2.1, 2.2, 2.3, 2.5, 5.1_
  
  - [x] 3.2 Implement configuration validation on startup
    - Check JWT_SECRET_KEY is configured
    - Fail application startup with descriptive error if missing
    - Log configuration errors
    - _Requirements: 5.4_
  
  - [ ]* 3.3 Write integration tests for JWT validation
    - Test valid token acceptance
    - Test expired token rejection (401)
    - Test malformed token rejection (401)
    - Test missing Authorization header rejection (401)
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 4. Checkpoint - Ensure authentication works
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Apply authorization to TodoItems endpoints
  - [x] 5.1 Add authorization attributes to read endpoints
    - Add [Authorize(Roles = "user,admin")] to GET /api/todoitems
    - Add [Authorize(Roles = "user,admin")] to GET /api/todoitems/{id}
    - _Requirements: 3.1, 3.2, 3.3, 3.4_
  
  - [x] 5.2 Add authorization attributes to write endpoints
    - Add [Authorize(Roles = "admin")] to POST /api/todoitems
    - Add [Authorize(Roles = "admin")] to PUT /api/todoitems/{id}
    - Add [Authorize(Roles = "admin")] to DELETE /api/todoitems/{id}
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_
  
  - [ ]* 5.3 Write integration tests for role-based authorization
    - Test admin can perform all CRUD operations
    - Test user can read but gets 403 on write operations
    - Test unauthenticated requests get 401
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_

- [x] 6. Implement security headers and HTTPS enforcement
  - [x] 6.1 Add security headers middleware
    - Add Strict-Transport-Security header (max-age=31536000)
    - Add X-Content-Type-Options: nosniff
    - Add X-Frame-Options: DENY
    - Add X-XSS-Protection: 1; mode=block
    - _Requirements: 9.2, 9.3, 9.4, 9.5_
  
  - [x] 6.2 Configure HTTPS redirection for production
    - Add HTTPS redirection middleware with production environment check
    - _Requirements: 9.1_
  
  - [x] 6.3 Configure CORS with whitelist
    - Load allowed origins from configuration (CORS_ALLOWED_ORIGINS)
    - Configure CORS policy with restricted origins
    - _Requirements: 9.6_

- [x] 7. Implement logging for authentication and authorization
  - [x] 7.1 Add structured logging to AuthController
    - Log successful authentication with username and timestamp
    - Log failed authentication with username and reason
    - _Requirements: 10.1, 10.2_
  
  - [x] 7.2 Add authorization failure logging
    - Create custom authorization handler or middleware
    - Log 403 Forbidden events with username, endpoint, and required role
    - Log invalid JWT token validation failures
    - _Requirements: 10.3, 10.4_
  
  - [x] 7.3 Configure global exception handling
    - Add exception handling middleware
    - Log unhandled exceptions with stack traces
    - Configure log levels in appsettings.json
    - Ensure logs write to stdout for container compatibility
    - _Requirements: 10.5, 10.6, 10.7_

- [x] 8. Implement health check endpoints
  - [x] 8.1 Add health check endpoints
    - Install Microsoft.Extensions.Diagnostics.HealthChecks NuGet package
    - Implement /health endpoint returning 200 OK
    - Implement /ready endpoint with database connectivity check
    - Return 503 Service Unavailable when database unavailable
    - Configure 5-second timeout
    - Exclude health endpoints from authentication
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_
  
  - [ ]* 8.2 Write tests for health check endpoints
    - Test /health returns 200
    - Test /ready returns 200 when database available
    - Test /ready returns 503 when database unavailable
    - _Requirements: 11.1, 11.2, 11.3_

- [x] 9. Update Swagger documentation
  - [x] 9.1 Configure Swagger for JWT authentication
    - Add security definition for JWT bearer tokens
    - Add security requirement to Swagger configuration
    - Document authentication instructions
    - Add JWT token input mechanism in Swagger UI
    - _Requirements: 12.1, 12.2, 12.4, 12.5_
  
  - [x] 9.2 Document all endpoints with schemas
    - Ensure all endpoints have request/response schemas
    - Indicate required authentication and roles for each endpoint
    - _Requirements: 12.3, 12.4_

- [x] 10. Checkpoint - Ensure all API features work
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Create Docker configuration
  - [x] 11.1 Create Dockerfile with multi-stage build
    - Use mcr.microsoft.com/dotnet/sdk:9.0 for build stage
    - Use mcr.microsoft.com/dotnet/aspnet:9.0 for runtime stage
    - Copy and restore dependencies
    - Build and publish application
    - Expose ports 8080 (HTTP) and 8081 (HTTPS)
    - _Requirements: 6.1, 6.2, 6.3, 6.4_
  
  - [x] 11.2 Create docker-compose.yml
    - Define API service with environment variables
    - Map ports 8080 and 8081
    - Configure volume mounting for development
    - Set environment variables for JWT_SECRET_KEY, JWT_EXPIRATION_MINUTES, CONNECTION_STRING
    - _Requirements: 6.5, 6.6, 6.7_
  
  - [x] 11.3 Create .dockerignore file
    - Exclude bin, obj, .git, and other unnecessary files
    - Optimize build context size
  
  - [ ]* 11.4 Test Docker build and run
    - Build Docker image locally
    - Run container with docker-compose
    - Verify API is accessible on configured ports
    - _Requirements: 6.7_

- [x] 12. Create environment configuration files
  - [x] 12.1 Update appsettings.json with configuration structure
    - Add JWT configuration section
    - Add CORS configuration section
    - Add logging configuration
    - _Requirements: 5.5_
  
  - [x] 12.2 Create appsettings.Production.json
    - Configure production-specific settings
    - Reference environment variables for sensitive values
    - _Requirements: 5.5_
  
  - [x] 12.3 Create .env.example file
    - Document all required environment variables
    - Provide example values for local development
    - Include JWT_SECRET_KEY, JWT_EXPIRATION_MINUTES, CONNECTION_STRING, CORS_ALLOWED_ORIGINS
    - _Requirements: 5.1, 5.2, 5.3, 9.6_

- [x] 13. Create basic GitHub repository setup
  - [x] 13.1 Create .gitignore file
    - Exclude bin, obj, .env, appsettings.*.json (except examples)
    - Exclude user-specific IDE files
  
  - [x] 13.2 Create README.md with setup instructions
    - Document how to run locally
    - Document how to run with Docker
    - Document environment variable configuration
    - Document authentication flow and API usage
    - _Requirements: 5.1, 5.2, 5.3, 6.5, 6.6_

- [x] 14. Final checkpoint - Complete integration test
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- The implementation builds on the existing TodoItems API without breaking changes
- Configuration management uses environment variables for security
- Docker setup is simplified without external registries or complex CI/CD
- Focus is on core authentication, authorization, and containerization features

# TodoItems API

A secure ASP.NET Core 9.0 REST API for managing todo items with JWT authentication and role-based authorization.

## Features

- JWT-based authentication
- Role-based authorization (admin/user roles)
- In-memory database (easily configurable for SQL Server)
- Swagger/OpenAPI documentation
- Docker support with multi-stage builds
- Health check endpoints
- Comprehensive logging and error handling
- Security headers and HTTPS enforcement

## Quick Start

### Prerequisites

- .NET 9.0 SDK (for local development)
- Docker and Docker Compose (for containerized deployment)

### Running Locally

1. **Clone the repository**

```bash
git clone <repository-url>
cd <repository-directory>
```

2. **Configure environment variables**

Copy the example environment file and configure it:

```bash
cp .env.example .env
```

Edit `.env` and set your JWT secret key (minimum 32 characters recommended):

```env
JWT_SECRET_KEY=your-secure-secret-key-at-least-32-characters-long
JWT_EXPIRATION_MINUTES=60
ASPNETCORE_ENVIRONMENT=Development
```

3. **Restore dependencies and run**

```bash
cd "API web"
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

### Running with Docker

1. **Configure environment variables**

Ensure you have a `.env` file in the root directory with your configuration (see `.env.example`).

2. **Build and run with Docker Compose**

```bash
docker-compose up --build
```

The API will be available at:
- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8081`
- Swagger UI: `http://localhost:8080/swagger`

3. **Stop the containers**

```bash
docker-compose down
```

### Running with Docker (without Docker Compose)

```bash
# Build the image
docker build -t todoitems-api .

# Run the container
docker run -d \
  -p 8080:8080 \
  -p 8081:8081 \
  -e JWT_SECRET_KEY=your-secure-secret-key-at-least-32-characters-long \
  -e JWT_EXPIRATION_MINUTES=60 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name todoitems-api \
  todoitems-api
```

## Environment Variable Configuration

The API can be configured using environment variables or `appsettings.json`. Environment variables take precedence.

### Required Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `JWT_SECRET_KEY` | Secret key for signing JWT tokens (REQUIRED) | None | `your-secret-key-min-32-chars` |

### Optional Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `JWT_EXPIRATION_MINUTES` | JWT token expiration time in minutes | `60` | `120` |
| `CONNECTION_STRING` | Database connection string | InMemory | `Server=localhost;Database=TodoDb;...` |
| `CORS_ALLOWED_ORIGINS` | Comma-separated list of allowed CORS origins | `http://localhost:3000,http://localhost:5173` | `https://example.com,https://app.example.com` |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` | `Development`, `Staging`, `Production` |
| `Logging__LogLevel__Default` | Default logging level | `Information` | `Debug`, `Warning`, `Error` |

### Configuration Files

The API supports environment-specific configuration files:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

Example `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

## Authentication Flow and API Usage

### 1. Register a New User

Create a new user account with a username, password, and role.

**Endpoint:** `POST /api/auth/register`

**Request Body:**

```json
{
  "username": "john_doe",
  "password": "SecurePassword123!",
  "role": "user"
}
```

**Valid Roles:**
- `user` - Can read todo items
- `admin` - Can read, create, update, and delete todo items

**Response (201 Created):**

```json
{
  "id": 1,
  "username": "john_doe",
  "role": "user"
}
```

**Example with curl:**

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"john_doe","password":"SecurePassword123!","role":"user"}'
```

### 2. Login and Obtain JWT Token

Authenticate with your credentials to receive a JWT token.

**Endpoint:** `POST /api/auth/login`

**Request Body:**

```json
{
  "username": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john_doe",
  "role": "user"
}
```

**Example with curl:**

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john_doe","password":"SecurePassword123!"}'
```

### 3. Use JWT Token for Protected Endpoints

Include the JWT token in the `Authorization` header with the `Bearer` scheme for all protected endpoints.

**Header Format:**

```
Authorization: Bearer <your-jwt-token>
```

### API Endpoints

#### Authentication Endpoints (No authentication required)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | None |
| POST | `/api/auth/login` | Login and obtain JWT token | None |

#### TodoItems Endpoints (Authentication required)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/api/todoitems` | Get all todo items | `user` or `admin` |
| GET | `/api/todoitems/{id}` | Get a specific todo item | `user` or `admin` |
| POST | `/api/todoitems` | Create a new todo item | `admin` only |
| PUT | `/api/todoitems/{id}` | Update a todo item | `admin` only |
| DELETE | `/api/todoitems/{id}` | Delete a todo item | `admin` only |

#### Health Check Endpoints (No authentication required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Basic health check (always returns 200 OK) |
| GET | `/ready` | Readiness check (verifies database connectivity) |

### Example API Usage

#### Get All Todo Items (User or Admin)

```bash
curl -X GET http://localhost:8080/api/todoitems \
  -H "Authorization: Bearer <your-jwt-token>"
```

#### Create a Todo Item (Admin Only)

```bash
curl -X POST http://localhost:8080/api/todoitems \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Buy groceries","isComplete":false}'
```

#### Update a Todo Item (Admin Only)

```bash
curl -X PUT http://localhost:8080/api/todoitems/1 \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"id":1,"name":"Buy groceries","isComplete":true}'
```

#### Delete a Todo Item (Admin Only)

```bash
curl -X DELETE http://localhost:8080/api/todoitems/1 \
  -H "Authorization: Bearer <your-jwt-token>"
```

### Authorization Responses

- **401 Unauthorized** - Missing or invalid JWT token
- **403 Forbidden** - Valid token but insufficient permissions (e.g., user trying to create/update/delete)

## Interactive API Documentation

The API includes Swagger/OpenAPI documentation for interactive testing.

### Accessing Swagger UI

When running in Development mode, navigate to:
- Local: `http://localhost:5000/swagger`
- Docker: `http://localhost:8080/swagger`

### Using Swagger UI with Authentication

1. Click the "Authorize" button (lock icon) at the top right
2. Register a user via `POST /api/auth/register`
3. Login via `POST /api/auth/login` to obtain a JWT token
4. Copy the token value from the response
5. Click "Authorize" again and paste the token (without "Bearer " prefix)
6. Click "Authorize" to save
7. You can now test protected endpoints directly from Swagger UI

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Project

```bash
dotnet test API.Tests/API.Tests.csproj
```

## Project Structure

```
.
├── API web/                    # Main API project
│   ├── Controllers/            # API controllers
│   ├── Models/                 # Data models and DTOs
│   ├── Services/               # Business logic services
│   ├── Middleware/             # Custom middleware
│   ├── HealthChecks/           # Health check implementations
│   ├── appsettings.json        # Configuration
│   └── Program.cs              # Application entry point
├── API.Tests/                  # Test project
├── Dockerfile                  # Docker image definition
├── docker-compose.yml          # Docker Compose configuration
├── .env.example                # Example environment variables
└── README.md                   # This file
```

## Security Considerations

### Production Deployment

1. **JWT Secret Key**: Generate a strong, random secret key (minimum 32 characters)
   ```bash
   # Generate a secure random key (Linux/macOS)
   openssl rand -base64 32
   ```

2. **HTTPS**: Always use HTTPS in production. The API automatically redirects HTTP to HTTPS in production mode.

3. **CORS**: Configure `CORS_ALLOWED_ORIGINS` to only include trusted domains.

4. **Environment Variables**: Never commit `.env` files or `appsettings.Production.json` with secrets to version control.

5. **Database**: Replace the in-memory database with a persistent database (SQL Server, PostgreSQL, etc.) by setting `CONNECTION_STRING`.

### Security Headers

The API automatically sets the following security headers in production:

- `Strict-Transport-Security`: Enforces HTTPS
- `X-Content-Type-Options`: Prevents MIME sniffing
- `X-Frame-Options`: Prevents clickjacking
- `X-XSS-Protection`: Enables XSS filtering

## Logging and Monitoring

### Log Levels

Configure logging levels via environment variables:

```env
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning
```

Available levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

### Logged Events

The API logs the following security-relevant events:

- Successful authentication (username, timestamp)
- Failed authentication attempts (username, reason)
- Authorization failures (username, endpoint, required role)
- Invalid JWT token validation failures
- Unhandled exceptions with stack traces

### Container Logs

When running in Docker, logs are written to stdout and can be viewed with:

```bash
docker-compose logs -f api
```

## Health Checks

### Basic Health Check

```bash
curl http://localhost:8080/health
```

Returns `200 OK` if the API is running.

### Readiness Check

```bash
curl http://localhost:8080/ready
```

Returns:
- `200 OK` if the API is ready and database is accessible
- `503 Service Unavailable` if the database is unavailable

Response format:

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": null,
      "duration": 1.234
    }
  ]
}
```

## Troubleshooting

### API fails to start with "JWT_SECRET_KEY is not configured"

**Solution**: Set the `JWT_SECRET_KEY` environment variable or add it to `appsettings.json`:

```bash
export JWT_SECRET_KEY=your-secure-secret-key-at-least-32-characters-long
```

### 401 Unauthorized on protected endpoints

**Causes**:
- Missing `Authorization` header
- Expired JWT token (default expiration: 60 minutes)
- Invalid JWT token format

**Solution**: Obtain a new token via `/api/auth/login` and include it in the header:

```
Authorization: Bearer <your-jwt-token>
```

### 403 Forbidden on write operations

**Cause**: User role lacks permissions (only `admin` can create/update/delete)

**Solution**: Register or login with an admin account:

```json
{
  "username": "admin_user",
  "password": "SecurePassword123!",
  "role": "admin"
}
```

### Docker container fails to start

**Solution**: Check logs for errors:

```bash
docker-compose logs api
```

Ensure all required environment variables are set in `.env` or `docker-compose.yml`.

## License

[Your License Here]

## Contributing

[Your Contributing Guidelines Here]
#   d o t n e t - a p i  
 
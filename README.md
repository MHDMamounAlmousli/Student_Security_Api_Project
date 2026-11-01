# Student API

Student API is an ASP.NET Core Web API for managing students and authentication. It uses JWT access tokens, refresh tokens, role-based authorization, SQL Server stored procedures, and Swagger for API exploration.

## Technology

- .NET 10
- ASP.NET Core Web API
- SQL Server with `Microsoft.Data.SqlClient`
- JWT Bearer authentication
- BCrypt password hashing
- Swagger/OpenAPI
- ASP.NET Core rate limiting

## Solution Structure

- `StudentsApi` - HTTP API, controllers, authentication, authorization, Swagger, and application startup.
- `StudentBusiness` - Student and refresh-token business models and JWT token generation.
- `StudentDataAccess` - SQL Server access and stored-procedure integration.

## Requirements

- .NET 10 SDK
- SQL Server
- The database schema and stored procedures expected by `StudentDataAccess`

## Configuration

Configuration is stored in ASP.NET Core user secrets rather than committed `appsettings` files. The API project already contains a `UserSecretsId`.

From the `StudentsApi` directory, initialize user secrets if needed:

```bash
dotnet user-secrets init
```

Set the JWT values and database connection string:

```bash
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret"
dotnet user-secrets set "Jwt:Issuer" "StudentApi"
dotnet user-secrets set "Jwt:Audience" "StudentApiUsers"
dotnet user-secrets set "Jwt:DurationInMinutes" "30"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=StudentsDb;User Id=sa;Password=your-password;TrustServerCertificate=True;"
```

Do not commit `secrets.json`, passwords, JWT keys, or other credentials to source control.

## Running the API

From the repository root:

```bash
dotnet restore StudentSystem.slnx
dotnet build StudentSystem.slnx
dotnet run --project StudentsApi/StudentsApi.csproj
```

The configured development URLs are:

- Swagger UI: <https://localhost:7189/swagger>
- HTTP API: <http://localhost:5284>
- HTTPS API: <https://localhost:7189>

The API loads user secrets automatically when running in the Development environment.

## Authentication

Login and refresh endpoints return JWT access tokens and refresh tokens. Send the access token with protected requests:

```http
Authorization: Bearer <access-token>
```

The JWT contains the student ID, email, and role claims. The `admin` role is required for student creation, updates, deletion, and retrieving all students. The owner-or-admin policy protects individual student records.

## Main Endpoints

### Authentication

- `POST /api/Auth/login` - Authenticate with email and password.
- `POST /api/Auth/refresh` - Exchange a valid refresh token for new tokens.
- `POST /api/Auth/logout` - Revoke a refresh token.

### Students

- `GET /api/Students/All` - Get all students. Requires the `admin` role.
- `GET /api/Students/Passed` - Get students who passed.
- `GET /api/Students/AverageGrade` - Get the average grade.
- `GET /api/Students/{id}` - Get a student by ID. Requires the student owner or an admin.
- `POST /api/Students` - Add a student. Requires the `admin` role.
- `PUT /api/Students/{id}` - Update a student. Requires the `admin` role.
- `DELETE /api/Students/{id}` - Delete a student. Requires the `admin` role.
- `POST /api/Students/Upload` - Upload an image.
- `GET /api/Students/getImage/{filename}` - Retrieve an uploaded image.

Authentication endpoints are protected by a sliding-window rate limit of five requests per minute per IP address.

## Security Controls and Threat Mitigations

The project applies the following security controls to reduce common API threats:

- **JWT authentication** - Access tokens are signed with a secret key and validated for issuer, audience, lifetime, and signing key.
- **Secure secret storage** - JWT configuration and database credentials are stored in ASP.NET Core user secrets instead of source-controlled configuration files.
- **Password protection** - Student passwords are hashed with BCrypt before they are stored or compared.
- **Refresh-token protection** - Refresh tokens are stored as BCrypt hashes, have an expiry time, and can be revoked during logout or token rotation.
- **Role-based authorization** - Administrative operations require the `admin` role.
- **Resource ownership authorization** - Individual student records are protected by the `StudentOwnerOrAdmin` policy.
- **Rate limiting** - Login and refresh endpoints use a sliding-window limit of five requests per minute per client IP to reduce brute-force and credential-stuffing attacks.
- **CORS restrictions** - Cross-origin requests are limited to the configured frontend development origins.
- **Input validation** - Student input is checked for required values and valid numeric ranges before database operations.
- **SQL stored procedures** - Database operations use named stored procedures through `SqlCommand` rather than dynamically constructed SQL statements.
- **HTTPS redirection** - HTTP requests are redirected to HTTPS when the API is running.
- **Controlled error responses** - Authentication and authorization failures return appropriate `401`, `403`, `404`, `429`, or `500` responses without exposing database implementation details.

## Logging and Auditing

The API uses ASP.NET Core `ILogger` for operational logging and security auditing. Logged events include:

- Failed login attempts, including the email and client IP address.
- Invalid, expired, revoked, or mismatched refresh-token attempts.
- Successful authentication and refresh activity, including user ID and client IP address.
- Administrative student actions such as create, update, and delete operations.
- Blocked or forbidden requests, including user ID, request path, and client IP address.

These logs provide an audit trail for authentication events, authorization failures, and sensitive administrative actions. Logs should be protected from unauthorized access and retained according to the deployment environment's security policy. Do not log passwords, JWT signing keys, database credentials, or raw refresh tokens in production.

## Database

The data-access project calls stored procedures for student and refresh-token operations, including procedures such as:

- `SP_GetAllStudents`
- `SP_GetStudentById`
- `SP_AddStudent`
- `SP_UpdateStudent`
- `SP_DeleteStudent`
- `SP_SaveRefreshToken`
- `SP_GetRefreshTokenByHash`
- `SP_RevokeRefreshToken`

The connection string is read from `ConnectionStrings:DefaultConnection` at application startup.

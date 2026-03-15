# 🐢 NinjaTurtlesFahrSchule — Donatello Service

> A driving school (Fahrschule) customer database management system and gRPC communication exercise.

---

## 📌 Project Purpose

This project addresses two core learning goals simultaneously:

1. **Driving School Customer Database** — A fully structured data layer for managing students, license courses, enrollments, and payments.
2. **gRPC Service Communication Exercise** — Practicing strongly-typed, HTTP/2-based inter-service communication using Protocol Buffers and `Grpc.AspNetCore`.

The project follows a Ninja Turtles naming theme: each service is named after a turtle. This service is **Donatello** — representing data and technology.

---

## 🗂️ Project Structure

```
NinjaTurtlesFahrSchule/
└── Donatello/
    ├── Donatello.API/            # gRPC service endpoints, proto files, middleware
    ├── Donatello.Core/           # Domain models, interfaces, enums (dependency-free layer)
    ├── Donatello.Infrastructure/ # EF Core, repository implementations, migrations
    └── Donatello.Tests/          # Unit tests with xUnit + NSubstitute
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 |
| gRPC Framework | `Grpc.AspNetCore` + Protocol Buffers (proto3) |
| Web Server | Kestrel (HTTP/2, port 5215) |
| Database | PostgreSQL |
| ORM | Entity Framework Core + Npgsql |
| Logging | Serilog (Console sink; PostgreSQL sink prepared, commented out) |
| Testing | xUnit + FluentAssertions + NSubstitute |
| gRPC Reflection | `Grpc.AspNetCore.Server.Reflection` (development environment) |

---

## 🧩 Design Patterns Applied

### 1. Repository Pattern + Generic Base Repository
The `IBaseRepository<T>` interface standardizes `GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, and `SoftDeleteAsync` methods for all entities. `BaseRepository<T>` provides the concrete implementation, while entity-specific repositories (`StudentRepository`, `CourseRepository`, etc.) extend it with domain-specific queries.

```
IBaseRepository<T>  ←  BaseRepository<T>  ←  StudentRepository
                                           ←  CourseRepository
                                           ←  EnrollmentRepository
                                           ←  PaymentRepository
                                           ←  UserRepository
```

### 2. Unit of Work Pattern
`IUnitOfWork` / `UnitOfWork` aggregates all repositories under a single transaction umbrella. Atomic database operations are guaranteed through `BeginTransactionAsync`, `CommitTransactionAsync`, and `RollbackTransactionAsync`. Repositories are lazy-loaded to avoid unnecessary object instantiation.

### 3. Soft Delete Pattern
The `IsDeleted` flag on `BaseEntity`, combined with EF Core's `HasQueryFilter`, ensures records are never physically removed — they are only marked as deleted. All queries pass through this filter automatically and globally.

### 4. Layered Architecture (Clean Architecture)
- **Core** layer: Only domain models and interfaces; zero external dependencies.
- **Infrastructure** layer: Implements Core interfaces; depends on EF Core.
- **API** layer: Hosts gRPC services; depends on Core interfaces, not Infrastructure directly.

This ensures dependencies flow in the correct direction (Dependency Inversion Principle).

### 5. Middleware Pattern — Global Exception Handler
The `GlobalExceptionHandler` middleware centrally catches all unhandled exceptions, logs them via Serilog, and maps them to appropriate HTTP status codes: `ArgumentException → 400`, `KeyNotFoundException → 404`, `UnauthorizedAccessException → 401`. For gRPC-specific errors, `RpcException` is thrown with the appropriate `StatusCode`.

### 6. Startup / Bootstrapper Pattern
The `Startup` class separates application configuration (`ConfigureServices` and `Configure`) from `Program.cs`, improving readability and testability.

---

## 📡 gRPC Services & Proto Definitions

Four gRPC services are defined using `proto3` syntax:

### `StudentService`
Student CRUD operations, paginated listing, and querying students with their enrollment details.

### `CourseService`
Course management by license category (A, A1, A2, B, BE, B1), including active/inactive filtering and category-based queries.

### `EnrollmentService`
Student-course enrollment management with status transitions (`PreRegistered → Active → Completed / Cancelled`) and queries including payment details.

### `PaymentService`
Partial payment support, pending payment listing, and a `ProcessPayment` RPC for gateway integration flow.

---

## 🗄️ Data Model

```
User (1) ──── (N) Student (1) ──── (N) Enrollment (1) ──── (N) Payment
                                          │
                                         (N)
                                        Course
```

- **User**: Protected by unique indexes on email and TC Identity Number (Turkish national ID).
- **Student**: Auto-generated `StudentNumber` (e.g. `STD20251234`), emergency contact fields.
- **Course**: `LicenseCategory` enum, theory/practice hours, duration (days), price. Seeded with Class A and Class B courses out of the box.
- **Enrollment**: Tracks total amount and paid amount, completion date.
- **Payment**: Payment type, gateway info, transaction ID.


---

## ✅ Test Structure

The `Donatello.Tests` project contains comprehensive unit tests for `StudentGrpcService`:

- **NSubstitute** mocks `IUnitOfWork` — no real database connection required.
- **FluentAssertions** provides readable assertions.
- Covered scenarios: valid student creation, invalid date format, record not found, invalid GUID format, soft delete success/failure, and paginated listing.

---

## 🚀 Running the Project

### Prerequisites
- .NET 8 SDK
- PostgreSQL

### Connection Setup
Update the `DefaultConnection` string in `Donatello.API/appsettings.json` to point to your PostgreSQL instance.

### Start the Service

```bash
cd Donatello/Donatello.API
dotnet run
```

The application runs on `localhost:5215` over HTTP/2. In development mode, EF Core migrations are applied automatically and gRPC Reflection is enabled (testable via Postman or `grpcurl`).

### Run Tests

```bash
cd Donatello/Donatello.Tests
dotnet test
```

---

## 🔭 Planned Improvements

TODOs throughout the project:

- Add a business service layer (`IStudentService`, `ICourseService`, etc.)
- Implement proper password hashing (replacing `temp_hash` with BCrypt/Argon2)
- Enable Serilog's PostgreSQL sink for persistent log storage
- Activate health check endpoints
- Integrate with Future Ninja Turtle services (Leonardo, Raphael, Michelangelo)

---

## 📄 License

MIT — see the `LICENSE` file for details.

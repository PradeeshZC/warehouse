# Project Overview

This repository contains a production-ready enterprise skeleton for a Supply Chain & Warehouse Management System built on .NET 8 using Razor / MVC patterns. The solution is implemented with Clean Architecture principles and is prepared for enterprise-grade features including CQRS (MediatR), EF Core (SQL Server), ASP.NET Core Identity (JWT + refresh tokens), repository/unit-of-work patterns, Serilog logging, FluentValidation, and AutoMapper.

---

# System Architecture

- Presentation: ASP.NET Core MVC / Razor views
- Application: CQRS (Commands & Queries) implemented with MediatR; Services and Handlers
- Domain: Entities, Enums, Value Objects
- Infrastructure: EF Core DbContext, Repositories (Generic + Module-specific), Identity stores, external integrations

Architecture diagram (high level):

```
+-----------+     +------------+     +------------------+
|  Browser  | --> | MVC / Razor| --> |  Controllers ---> |  (IMediator)  
+-----------+     +------------+     +------------------+
											   |
											   v
										 +-------------+
										 |  MediatR    |
										 | Commands/   |
										 | Queries     |
										 +-------------+
											  /  \
											 /    \
											v      v
								   +----------------+  +----------------+
								   |  Handlers      |  |  Handlers      |
								   +----------------+  +----------------+
											|                 |
											v                 v
									+--------------------------------+
									|  Services / Repositories       |
									+--------------------------------+
											  |
											  v
									   +-------------------+
									   | ApplicationDbContext|
									   | (EF Core + SQL Server)|
									   +-------------------+
```

---

# Technology Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8, C# 12 |
| Web | ASP.NET Core MVC, Razor Runtime Compilation |
| Data | Entity Framework Core 8 + SQL Server |
| Authentication | ASP.NET Core Identity, JWT Bearer |
| CQRS/Mediator | MediatR |
| Mapping | AutoMapper (AutoMapper.Extensions.Microsoft.DependencyInjection) |
| Validation | FluentValidation.AspNetCore |
| Logging | Serilog (Console + File sinks) |
| Security | BCrypt.Net-Next (password hashing if direct hashing needed), JWT refresh tokens |
| API Docs | Swashbuckle / Swagger |

---

# Folder Structure

Root folders created and used in this project:

- /Controllers
  - /Controllers/Auth
  - /Controllers/Admin
- /Models
  - /Models/Entities
  - /Models/DTOs
  - /Models/ViewModels
  - /Models/Identity
  - /Models/Enums
- /Data
  - /Data/Configurations
  - /Data/Seeders
  - ApplicationDbContext.cs
- /CQRS
  - /Commands
  - /Queries
  - /Handlers
- /Repositories
  - /Interfaces
  - /Implementations
- /Services
  - /Interfaces
  - /Implementations
- /Middleware
- /Helpers
- /Validators
- /Mapping
- /Modules (module-specific structure)
- /wwwroot
- /Views

This matches Clean Architecture boundaries and keeps UI, application, domain, and infrastructure organized.

---

# Database Design

Key tables (entities) implemented:

- Roles, Users (Identity + ApplicationUser)
- Categories
- Products
- Warehouses
- Bins
- InventoryStocks
- Suppliers
- PurchaseOrders
- PurchaseOrderItems
- Orders
- OrderItems
- Shipments
- RefreshTokens

Important database constraints and features:
- SKU (planned) must be unique (ensure when adding SKU column)
- Email and UserName are unique
- Bin code is unique per warehouse (composite unique index)
- Decimal columns use precision: decimal(18,4)
- Soft-delete implemented via BaseEntity.IsDeleted with global query filters
- Timestamps: CreatedAt and UpdatedAt managed in ApplicationDbContext.SaveChanges overrides

---

# CQRS Architecture

- MediatR is the mediator used for command/query dispatch.
- Commands and Queries are separated under /CQRS with strongly typed responses (BaseResponse<T> wrapper).
- Handlers interact with repository and unit-of-work abstractions (no direct DbContext usage in controllers).
- Write operations use Commands handled by IRequestHandler<TCommand, TResult> implementations.
- Read operations use Queries returning DTO collections or single DTOs.

Example flow:
- Controller -> IMediator.Send(CreateProductCommand) -> CreateProductHandler -> IRepository<Product>.AddAsync -> IUnitOfWorkAsync.CompleteAsync

---

# Authentication Flow

- ASP.NET Core Identity is used with ApplicationUser (extends IdentityUser) and EF stores.
- JWT access tokens are issued by IJwtService, signed with HMAC using a secure secret configured in appsettings.json.
- Refresh tokens are long random strings stored in RefreshTokens table and tied to user ID; refresh flow issues new access + refresh tokens and revokes the previous refresh token.
- Access tokens are short lived (configurable via JwtSettings: AccessTokenExpirationMinutes).
- Refresh tokens have longer TTL (e.g., 7 days) and are revoked on refresh or logout.

Security controls:
- Secure cookies (HttpOnly, Secure, SameSite=Strict) used to store refresh tokens if desired.
- Password policy enforced by Identity (length, upper, lower, digit, etc.).
- No plaintext passwords stored. Identity manages hashing.

---

# Authorization Flow

- Role-based: Roles created (Admin, Manager, Worker, Viewer). Use [Authorize(Roles = "Admin")] or policy-based attributes.
- Policy-based: Example policy "RequireAdminRole" configured in Program.cs.
- Claims-based: Claims are read from Identity and added to JWT access tokens to support fine-grained authorization.
- Filters and middleware: StatusCodePages redirects 401/403 to Unauthorized / AccessDenied views.

Permissions mapping (recommended):

| Role | Permissions |
|---|---|
| Admin | Full access to all modules |
| Manager | Product, Warehouse, Procurement, Orders |
| Worker | Inventory operations, Shipment processing |
| Viewer | Read-only access across modules |

---

# Entity Relationships

- Role (1) -> Users (many)
- Category (1) -> Products (many)
- Warehouse (1) -> Bins (many)
- InventoryStock links Product, Warehouse, optional Bin
- Supplier (1) -> PurchaseOrders (many)
- PurchaseOrder (1) -> PurchaseOrderItems (many)
- Product (1) -> PurchaseOrderItems (many)
- Order (1) -> OrderItems (many)
- Product (1) -> OrderItems (many)
- Order (1) -> Shipments (many)

ER flow (simplified):

```
Product --< InventoryStock >-- Warehouse
Supplier --< PurchaseOrder --< PurchaseOrderItem >-- Product
Order --< OrderItem >-- Product
Order --< Shipment
```

---

# Module Explanations

Each module follows the same enterprise structure:

- Entity (Domain model)
- DTOs (Data Transfer Objects)
- ViewModels (for views)
- CQRS Commands & Queries
- Handlers (MediatR)
- Repository integration
- Service layer
- Controller and Views
- Validation (FluentValidation)
- Authorization attributes (policy/role)
- AutoMapper profiles

Modules present or scaffolded: Products, Categories, Warehouses, Bins, Inventory, Suppliers, PurchaseOrders, Orders, Shipments, Users.

---

# Repository Pattern

- Generic repository interfaces (IGenericRepository<T>, IRepository<T>) provide CRUD and pagination operations.
- Implementations (GenericRepository, Repository) use EF Core DbSet<T> and are async.
- Repositories contain no business logic — only data access.
- Repositories are injected into handlers/services via DI.

---

# Unit of Work

- IUnitOfWork and IUnitOfWorkAsync provide Commit/Transaction management.
- UnitOfWorkAsync supports BeginTransactionAsync, CommitAsync, RollbackAsync backed by EF Core IDbContextTransaction.
- Services coordinate multiple repository operations and commit through the unit of work.

---

# MediatR Usage

- MediatR registered in Program.cs: builder.Services.AddMediatR(...)
- Commands and Queries implement IRequest<TResponse>. Handlers implement IRequestHandler<TRequest, TResponse>.
- Handlers use repositories and unit-of-work; controllers only use IMediator.

---

# Validation Strategy

- FluentValidation integrated via FluentValidation.AspNetCore.
- Validators implemented per command or ViewModel to enforce business rules and input validation before handlers run.
- Validation errors are returned through ModelState in MVC views and through standardized BaseResponse<T> in APIs.

---

# AutoMapper Strategy

- AutoMapper registered (AddAutoMapper) and used to map Entities <-> DTOs/ViewModels.
- Create profiles per module (ProductsProfile, WarehouseProfile) for maintainability.

Example profile registration:

```csharp
public class ProductsProfile : Profile
{
	public ProductsProfile()
	{
		CreateMap<Product, ProductDto>();
		CreateMap<CreateProductCommand, Product>();
	}
}
```

---

# Logging Strategy

- Serilog configured in Program.cs with Console and Rolling File sinks.
- Structured logging is used (JSON-friendly) and enriched with context.
- Sensitive data should be excluded from logs.

---

# Middleware

- ExceptionHandlingMiddleware: global exception handler to return consistent JSON for API errors and log unhandled exceptions.
- StatusCodePages: redirects unauthorized and forbidden responses to UI pages.
- Session middleware, Authentication and Authorization middleware included.

---

# Error Handling

- Global exception middleware returns generic error message for production and logs stack traces.
- MVC Error view and ErrorController provided for friendly UI error pages.
- Validation errors surfaced back to views via ModelState.

---

# Security Features

- ASP.NET Core Identity for secure password storage and management.
- JWT for stateless auth with short-lived access tokens and refresh tokens.
- Refresh tokens stored server-side and revoked on use and logout.
- Secure cookie settings (HttpOnly, Secure, SameSite=Strict).
- Soft delete prevents immediate data loss.
- Password policy and account lockout should be configured for production.

---

# MVC UI Structure

- Controllers use IMediator to dispatch commands/queries.
- Views use strongly typed ViewModels and anti-forgery tokens on write operations.
- Layout (/Views/Shared/_Layout.cshtml) and shared Error view are present.

---

# Workflow Diagrams

Order Processing Flow (simplified):

```
Customer Order -> OrdersController -> CreateOrderCommand -> Handler
 -> Validate inventory -> Reserve stock (InventoryStock) -> Commit Transaction
 -> Create Shipment(s) -> Update Order Status
```

Inventory Flow:

```
PurchaseOrder Received -> Update InventoryStocks (by Warehouse/Bin)
Inventory adjustment -> InventoryStock records change, UpdatedAt timestamp
```

Shipment Flow:

```
Order ready -> Create Shipment -> Update ShipmentStatus -> Delivery confirmation -> Update Order status
```

Procurement Flow:

```
Create PurchaseOrder -> Supplier confirmation -> Receive (PurchaseOrderItem -> InventoryStock) -> Update PO status
```

---

# Setup Instructions

1. Prerequisites
   - .NET 8 SDK
   - SQL Server instance (LocalDB or full SQL Server)
   - Optional: Visual Studio 2026 or VS Code

2. Configuration
   - Open appsettings.json and set `ConnectionStrings:DefaultConnection` to your SQL Server connection string.
   - Set `JwtSettings:Secret` to a secure, long random string. Example:

```json
"JwtSettings": {
  "Issuer": "WarehouseIssuer",
  "Audience": "WarehouseAudience",
  "Secret": "<long-secret-string>",
  "AccessTokenExpirationMinutes": "15"
}
```

3. Restore and build

```bash
dotnet restore
dotnet build
```

4. Add EF tools (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

5. Create initial migration and update database

```bash
dotnet ef migrations add Initial -o Data/Migrations
dotnet ef database update
```

(Or in Visual Studio Package Manager Console)

```powershell
Add-Migration Initial -OutputDir Data\Migrations
Update-Database
```

6. Run the app

```bash
dotnet run
```

7. Default Admin (development only)

- Email: admin@warehouse.local
- Password: Admin@12345

> Replace default credentials in production. Use user secrets / environment variables for secrets.

---

# NuGet Packages

| Package | Purpose |
|---|---|
| Microsoft.EntityFrameworkCore 8.0.0 | EF Core runtime |
| Microsoft.EntityFrameworkCore.SqlServer 8.0.0 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Tools 8.0.0 | Migrations tooling |
| Microsoft.EntityFrameworkCore.Design 8.0.0 | Design-time helpers |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0 | Identity EF stores |
| Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0 | JWT authentication |
| AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.0 | AutoMapper DI support |
| FluentValidation.AspNetCore 11.3.1 | FluentValidation integration |
| BCrypt.Net-Next 4.0.2 | BCrypt hashing (optional) |
| Swashbuckle.AspNetCore 6.6.1 | Swagger/OpenAPI |
| MediatR 11.1.0 | Mediator / CQRS-ish bus |
| MediatR.Extensions.Microsoft.DependencyInjection 11.1.0 | MediatR DI integration |
| Serilog.AspNetCore 7.0.0 | Serilog integration |
| Serilog.Sinks.Console 4.1.0 | Console sink |
| Serilog.Sinks.File 5.0.0 | File sink |

---

# Database Migration Commands

- Create migration:

```bash
dotnet ef migrations add Initial -o Data/Migrations
```

- Apply migration:

```bash
dotnet ef database update
```

- Remove last migration (if needed):

```bash
dotnet ef migrations remove
```

---

# Run Instructions

- Run locally via CLI:

```bash
dotnet run --project warehouse.csproj
```

- Or open the solution in Visual Studio 2026 and run (IIS Express or Project).

- Swagger UI available during development at: https://localhost:{port}/swagger

---

# Default Admin Credentials

- Email: admin@warehouse.local
- Password: Admin@12345

> These credentials are created by a development seeder. Replace them immediately for production and store secrets securely.

---

# Future Scalability Plan

- Separate read and write models (true CQRS) and scale read side horizontally (caching / read replicas).
- Move handlers into separate worker services or microservices for heavy processing (shipments, reporting).
- Add async messaging (RabbitMQ, Azure Service Bus) for decoupled integration.
- Add distributed caching (Redis) for high-performance lookups.
- Add observability: distributed tracing (OpenTelemetry), metrics (Prometheus), centralized logging (Seq/ELK).

---

# Enterprise Enhancements

- Multi-tenancy support (schema or database separation).
- Role management UI and permission matrix (RBAC/ABAC).
- Stronger security: rotate keys, HSM, refresh token rotation strategy, device sessions.
- Rate limiting and API gateway for external integrations.

---

# Known Constraints

- Seeded admin password is insecure in production.
- SKU uniqueness not enforced until SKU property is added in Product entity.
- Some modules are scaffolded and require full implementation (Views, validators, AutoMapper profiles).

---

# Recommended Improvements

- Add integration and unit tests for handlers, services, and repositories.
- Use AutoMapper profiles and remove reflection mapping patterns from handlers.
- Implement account lockout and advanced Identity features.
- Move seeding to deterministic migrations or idempotent startup tasks with better lifecycle handling.

---

If you want, I can now:

- Generate the initial EF migration and provide the SQL script.
- Implement complete Product module end-to-end (DTOs, validators, handlers, AutoMapper, controller, views).
- Harden Identity security (lockout, email confirmation) and move secrets to user secrets.

Select the next action and I will proceed.

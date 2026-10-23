# FrameRent - Audiovisual Equipment Rental System

FrameRent is an enterprise-grade web platform designed to streamline audiovisual equipment rentals for photographers, small production studios, and content creators. The solution replaces manual spreadsheet and messaging tracking with an automated, auditable system adhering to **Clean Architecture** principles in .NET 10.

---

## 🚀 Technologies Used

- **Runtime & Language**: .NET 10 (C# 13)
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation: Web & API)
- **Database**: PostgreSQL with Entity Framework Core 10 (Code First & Migrations)
- **Authentication & Authorization**: ASP.NET Core Identity & JWT (JSON Web Tokens)
- **API Documentation**: OpenAPI / Swagger UI
- **Presentation Layer**: ASP.NET Core MVC (Razor Views + Bootstrap 5 + Bootstrap Icons)

---

## 🏛️ Clean Architecture Structure

```text
simulacro/
 ├── src/
 │    ├── FrameRent.Domain/         # Core business entities, Enums, Domain Exceptions, Repository Interfaces
 │    ├── FrameRent.Application/    # Use cases, DTOs, Application Service contracts & implementations
 │    ├── FrameRent.Infrastructure/ # EF Core DbContext, Identity, Npgsql, JWT generation, Repositories, DataSeeder
 │    ├── FrameRent.Api/            # REST API controllers, Bearer JWT Auth, Global Exception Middleware, Swagger
 │    └── FrameRent.Web/            # Customer-facing MVC presentation layer (Catalog, Availability, Rentals)
 ├── FrameRent.slnx                 # Solution configuration
 └── README.md
```

---

## 🔐 Default Staff Account (Seeded on First Run)

As required, FrameRent comes with a pre-configured Staff administrator account automatically initialized upon first application startup:

- **Email**: `staff@framerent.com`
- **Password**: `Staff@123456`
- **Role**: `Staff`

> An additional demo client account is also pre-seeded:
> - **Email**: `client@framerent.com`
> - **Password**: `Client@123456`
> - **Role**: `Client`

---

## ⚙️ Setup and Execution Guide

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- PostgreSQL server running (default config: `Host=localhost;Port=5433;Database=framerent_db;Username=postgres;Password=postgres`).
  - *If your PostgreSQL runs on another port or user, adjust `ConnectionStrings:DefaultConnection` in `src/FrameRent.Api/appsettings.json` and `src/FrameRent.Web/appsettings.json`.*

### 2. Database Migrations
Run the migrations to create the database schema:
```bash
dotnet ef database update --project src/FrameRent.Infrastructure --startup-project src/FrameRent.Api
```

### 3. Run the REST API
```bash
dotnet run --project src/FrameRent.Api --urls "http://localhost:5150"
```
- Access **Swagger UI** at: [http://localhost:5150/swagger](http://localhost:5150/swagger)

### 4. Run the Client Web Application (Presentation Layer)
In another terminal, start the customer portal:
```bash
dotnet run --project src/FrameRent.Web --urls "http://localhost:5002"
```
- Open your browser at: [http://localhost:5002](http://localhost:5002)

---

## 📋 API Endpoints Reference

### Authentication
- `POST /api/v1/auth/register` - Registers a new customer (Role: `Client`).
- `POST /api/v1/auth/login` - Authenticates and issues a signed JWT token.

### Equipment
- `GET /api/v1/equipment` - Public list of all active equipment.
- `GET /api/v1/equipment/{id}` - Public equipment details.
- `GET /api/v1/equipment/{id}/availability?from=&to=` - Check available units in date range.
- `POST /api/v1/equipment` - *(Staff only)* Create equipment.
- `PUT /api/v1/equipment/{id}` - *(Staff only)* Update equipment.
- `PATCH /api/v1/equipment/{id}/deactivate` - *(Staff only)* Deactivate equipment (blocked if active rentals exist).

### Rentals
- `POST /api/v1/rentals` - *(Client)* Request a new rental (validates 15-day limit, active rental limit <= 3, and date availability).
- `GET /api/v1/rentals/mine` - *(Client)* List authenticated client's rentals.
- `GET /api/v1/rentals/{id}` - *(Client owner or Staff)* View rental details.
- `PATCH /api/v1/rentals/{id}/cancel` - *(Client owner)* Cancel rental (only allowed before start date and if Pending/Approved).
- `GET /api/v1/rentals?status=` - *(Staff)* List all rentals with optional status filter.
- `PATCH /api/v1/rentals/{id}/approve` - *(Staff)* Approve rental (re-validates inventory availability).
- `PATCH /api/v1/rentals/{id}/reject` - *(Staff)* Reject rental.
- `PATCH /api/v1/rentals/{id}/deliver` - *(Staff)* Register physical handover of equipment.
- `PATCH /api/v1/rentals/{id}/return` - *(Staff)* Register return and automatically compute late fee penalty if overdue:
  $$\text{LateFee} = \text{OverdueDays} \times \text{DailyRate} \times \text{Quantity} \times 1.5$$

### Reports
- `GET /api/v1/reports/top-equipment` - *(Staff)* Ranking of most rented equipment.

---

## 👤 Author

- **Full Name**: Sebastian Mendoza Viloria
- **Clan**: Coder C#/.net
- **Email**: smendozaviloria9@gmail.com

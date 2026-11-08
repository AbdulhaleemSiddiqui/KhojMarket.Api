# KhojMarket API

KhojMarket is a requirement-based B2B marketplace for Pakistan. Buyers publish what they need, administrators review and verify requirements, verified sellers respond with inquiries or quotations, and buyers choose a supplier to proceed with a deal.

This repository contains the ASP.NET Core Web API and its persistence, authentication, marketplace workflows, and supporting services.

## How the marketplace works

```text
Buyer
  |
  | Register / sign in, verify phone, post requirement
  v
Requirement submitted
  |
  v
Admin reviews requirement
  |----------------------|
  | Approve              | Reject
  v                      v
Verified / public        Buyer can revise or follow up
requirement
  |
  v
Verified seller browses requirement
  |
  | Sends inquiry / quotation (uses seller credits)
  v
Buyer reviews seller responses
  |
  | Selects a supplier
  v
Deal created
  |
  | Buyer marks the requirement/deal complete
  v
Deal completed
  |
  v
Buyer and seller can review each other
```

Seller onboarding has a separate admin verification step: a seller submits business details, an administrator approves or rejects the seller, and seller inquiry submission is restricted to approved sellers.

## Features

- **Authentication and authorization:** registration and login endpoints issue JWT bearer tokens; role-based authorization is used for buyer, seller, and admin operations.
- **Phone OTP verification:** OTPs are generated, stored as hashes, expire after five minutes, and are invalidated after successful use or too many failed attempts. The currently registered phone number is used.
- **Buyer requirements:** create, update, list, view, complete, delete, manage activity, and upload requirement images. Buyers can also favorite requirements.
- **Admin requirement review:** administrators can view pending requirements and approve or reject them.
- **Seller verification:** sellers submit business name, city, and category for admin review. Admin endpoints support pending lists, seller details, approval, and rejection.
- **Seller inquiries and quotations:** sellers can respond to requirements; buyer-facing endpoints expose responses. The data model prevents more than one inquiry from the same seller for a requirement.
- **Seller wallet and credits:** seller credit balances and transaction history are available. Inquiry submission uses credits; administrators can manage seller credits.
- **Deals:** a buyer can select a seller response and proceed through the deal lifecycle, including completion.
- **Reviews and ratings:** participants can review each other after a completed deal; public user rating and review endpoints are available.
- **AI-assisted requirement drafting:** `POST /api/ai/requirements/generate` converts free-form text into an editable structured requirement. The default implementation is a deterministic development mock. An optional OpenAI-backed implementation can be enabled explicitly.
- **Health endpoint:** `GET /health` is mapped for health checks.
- **API documentation:** Swagger UI is enabled in the Development environment.

### Important integration notes

- The configured SMS implementation is currently `DevelopmentSmsService` in both Development and non-Development environments. It logs OTP values; a production SMS provider has not yet been integrated. Do not treat this as production-ready phone delivery.
- OpenAI integration is opt-in. Keep it disabled until an API key, supported model, rate limiting, and operational safeguards are configured.
- The AI mock uses limited deterministic parsing and is intended for development and flow validation, not equivalent semantic AI quality.
- The API currently uses the configured CORS allow-list. Set it to the actual frontend origins for each environment.

## Tech stack

| Area | Technology |
|---|---|
| API framework | ASP.NET Core on .NET 10 |
| Language | C# |
| ORM and migrations | Entity Framework Core 10 |
| Database provider | SQL Server |
| Authentication | JWT Bearer authentication |
| API documentation | Swagger / Swashbuckle |
| Health checks | ASP.NET Core Health Checks with SQL Server health-check package |
| AI integration | Development mock by default; optional OpenAI Responses API integration |
| Configuration | ASP.NET Core configuration, environment variables, and .NET User Secrets |

## Architecture

The API uses a **layered architecture with a service layer**:

- Controllers handle HTTP routes, request binding, authorization attributes, and HTTP responses.
- Services contain application and marketplace workflow logic.
- DTOs define request and response contracts.
- Models represent persisted domain data.
- Entity Framework Core's DbContext maps models to SQL Server.
- Middleware provides cross-cutting HTTP exception handling.

This is not described as Clean Architecture; the project is organized around its existing controllers, services, models, and data context.

## Project structure

| Path | Responsibility |
|---|---|
| `Controllers/` | HTTP API endpoints for authentication, OTP, requirements, admin operations, seller inquiries and wallet, AI requirement generation, deals, and reviews |
| `Services/` | Application logic for users, JWT, OTP, requirements, verification, seller credits/inquiries, deals, reviews, notifications, SMS, and AI |
| `Models/` | Entity classes persisted by Entity Framework Core |
| `DTOs/` | API request and response models |
| `Data/` | `KhojMarketDbContext` and EF Core entity configuration |
| `Middleware/` | Global exception middleware |
| `Migrations/` | EF Core database migrations and model snapshot |
| `docs/` | Supporting technical notes and review documentation |
| `wwwroot/uploads/` | Uploaded requirement images served as static files |
| `Program.cs` | Dependency registration, configuration, authentication, CORS, middleware pipeline, and endpoint mapping |
| `KhojMarket.Api.csproj` | Target framework and NuGet package references |

## Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server instance accessible from the API
- Entity Framework Core CLI tool (`dotnet-ef`) for applying migrations

### 1. Clone and open the API project

```bash
git clone https://github.com/AbdulhaleemSiddiqui/KhojMarket.Api.git
cd KhojMarket.Api
```

### 2. Configure local secrets

The project has a User Secrets ID configured. Store the database connection string and JWT signing key in User Secrets rather than committing them to `appsettings*.json`.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-sql-server-connection-string>"
dotnet user-secrets set "Jwt:Key" "<your-long-random-signing-key>"
```

The application also requires `Jwt:Issuer` and `Jwt:Audience`. These are present in the checked-in `appsettings.json` defaults; override them with User Secrets or environment-specific configuration if your deployment requires different values.

If the EF Core CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

### 3. Apply database migrations

Run this from the directory containing `KhojMarket.Api.csproj`:

```bash
dotnet ef database update
```

This applies the migrations in the project to the database specified by `ConnectionStrings:DefaultConnection`.

### 4. Run the API

```bash
dotnet run
```

When running in the Development environment, Swagger middleware is enabled. Open the Swagger URL printed by the application at startup. The API also maps a health endpoint at `/health`.

## Configuration

Configuration is read through the standard ASP.NET Core configuration system. User Secrets are suitable for local development; use a secure secret store or environment variables in deployed environments.

| Key | Required | Purpose / behavior |
|---|---:|---|
| `ConnectionStrings:DefaultConnection` | Yes | SQL Server connection used by Entity Framework Core |
| `Jwt:Key` | Yes | Symmetric signing key used to issue and validate JWTs; keep it secret |
| `Jwt:Issuer` | Yes | Expected JWT issuer |
| `Jwt:Audience` | Yes | Expected JWT audience |
| `Jwt:ExpiryMinutes` | Optional | Token expiry setting present in application configuration |
| `Cors:AllowedOrigins` | Environment-specific | Array of frontend origins allowed by the `Frontend` CORS policy |
| `MarketplaceCredits:InitialSellerCredits` | Optional | Initial seller credit setting present in application configuration |
| `MarketplaceCredits:InquiryCost` | Optional | Credit cost setting for seller inquiries |
| `OpenAI:Enabled` | Optional; defaults to `false` | Set to `true` to register the OpenAI-backed requirement service; otherwise the development mock is registered |
| `OpenAI:ApiKey` | Only when OpenAI is enabled | OpenAI API key; provide through User Secrets or a secure deployment configuration, never commit it |
| `OpenAI:Model` | Optional when OpenAI is enabled | Model name sent to the OpenAI Responses API; the service has a code-level fallback if this is not supplied |

Example optional local OpenAI configuration (only if you intentionally want to use the paid provider):

```bash
dotnet user-secrets set "OpenAI:Enabled" "true"
dotnet user-secrets set "OpenAI:ApiKey" "<your-openai-api-key>"
dotnet user-secrets set "OpenAI:Model" "<model-supported-by-your-account>"
```

Leave `OpenAI:Enabled` unset or set it to `false` to use the mock implementation.

## Roadmap

The following are future improvements, not claims about functionality already delivered:

- Add automated unit and integration test projects for authentication, OTP, requirement moderation, seller verification, wallet/credit deductions, inquiries, deals, and reviews.
- Add CI workflows for restore, build, tests, and migration checks.
- Integrate a real SMS provider and production-grade OTP delivery, throttling, and monitoring.
- Add production safeguards for AI generation, including rate limiting, provider failure handling, and usage monitoring.
- Prepare and document cloud deployment, environment configuration, secret management, database migrations, logging, and monitoring.

## Security and secrets

- Never commit database connection strings, JWT signing keys, OpenAI API keys, OTP values, or other credentials.
- Use .NET User Secrets for local development and a managed secret store or protected environment variables for deployment.
- The current development SMS implementation logs OTPs. Replace it with a real provider and review logging behavior before production use.
- Keep real OpenAI usage explicitly disabled until billing, access controls, and request-abuse protections are ready.

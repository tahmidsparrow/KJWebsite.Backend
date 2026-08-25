# KJWebsite.Backend - Project Context

## Overview
KJWebsite.Backend is a .NET 10 microservices backend for the KJ website.
It currently includes gateway routing, public content APIs, CTA submission APIs, and auth/registration APIs.

## Tech Stack
- .NET SDK: 10.0.300
- ASP.NET Core Minimal APIs
- YARP reverse proxy (gateway)
- EF Core 10
- SQLite (current persistence provider for auth + cta services)

## Repository Structure
- `src/Services/ApiGateway`
- `src/Services/ContentService`
- `src/Services/CtaSubmissionService`
- `src/Services/AuthIdentityService`
- `src/BuildingBlocks/KJWebsite.BuildingBlocks`
- `openapi.v1.yaml` (aggregated API contract)
- `PROJECT_CONTEXT.md` (this file)

## Service Ports (Development)
- ApiGateway: `http://localhost:7000`
- ContentService: `http://localhost:7001`
- CtaSubmissionService: `http://localhost:7002`
- AuthIdentityService: `http://localhost:7003`

## API Gateway Routing
Gateway forwards:
- `/api/v1/content/*` -> ContentService
- `/api/v1/cta/*` -> CtaSubmissionService
- `/api/v1/auth/*` -> AuthIdentityService
- `/api/v1/admin/content/*` -> ContentService

## Auth Rules
- Public website content endpoints are intentionally unauthenticated.
- CTA submission endpoint is currently unauthenticated.
- Auth endpoints handle login/refresh/logout/me and registration.
- Content admin endpoints in ContentService expect bearer token `dev-admin-token` in current v1 implementation.

## Implemented v1 Endpoints

### ContentService (public)
- `GET /api/v1/content/projects?lang=&status=`
- `GET /api/v1/content/projects/{id}?lang=`
- `GET /api/v1/content/news?lang=&limit=&offset=`
- `GET /api/v1/content/news/{id}?lang=`

### ContentService (admin)
- `POST /api/v1/admin/content/projects`
- `PUT /api/v1/admin/content/projects/{id}`
- `DELETE /api/v1/admin/content/projects/{id}`
- `POST /api/v1/admin/content/news`
- `PUT /api/v1/admin/content/news/{id}`
- `DELETE /api/v1/admin/content/news/{id}`

### CtaSubmissionService
- `POST /api/v1/cta/submissions`

### AuthIdentityService
- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/me`

## Data Persistence (EF Core 10)

### AuthIdentityService
DbContext: `AuthDbContext`
- Table: `users`
- Table: `refresh_tokens`

Legacy registration/profile fields from old system are mapped into user columns (FirstName, LastName, Gender, ReasonForJoining, etc.).

### CtaSubmissionService
DbContext: `CtaDbContext`
- Table: `cta_submissions`

Stores raw `values` as JSON plus extracted legacy awaiting-user fields:
- FirstName, LastName, Gender, ReasonForJoining, PresentOrganization, VolunteeingExperience, DateOfBirth, CityOfResidence, CountryOfResidence.

## Migrations
Initial migrations exist in:
- `src/Services/AuthIdentityService/Migrations`
- `src/Services/CtaSubmissionService/Migrations`

Both services call `Database.Migrate()` at startup.

## Legacy Import Notes
Two legacy snapshots were imported with history preserved via git subtree:
- `kj-registration/` (from `master`)
- `kj-registration-AwaitingUserFeatures/` (from `AwaitingUserFeatures`)

Important: these folders are historical references and should be treated as read-only unless explicitly requested.

## OpenAPI
- Consolidated spec file: `openapi.v1.yaml`
- Covers gateway-level v1 routes and shared schemas.

## Local Run Notes
If `dotnet` is not on PATH in a shell, use:
- `"C:\Program Files\dotnet\dotnet.exe"`

Build command:
- `dotnet build .\KJWebsite.Backend.slnx -c Debug`

## Current Constraints / Known Gaps
- Auth tokens are currently in-memory access tokens; refresh tokens are persisted.
- Passwords are plain in current dev implementation; production should use secure hashing.
- SQLite is used for quick local development; consider PostgreSQL for production.
- No centralized background job/event bus yet for notifications/analytics.

## Suggested Next Steps
1. Add password hashing + secure auth hardening.
2. Move from in-memory access token tracking to signed JWT validation flow.
3. Add integration tests for auth + cta + gateway routes.
4. Add Docker setup for service + DB orchestration.
5. Add PostgreSQL provider and migration strategy for production.

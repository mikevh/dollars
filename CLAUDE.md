# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

Full-stack personal finance app with a .NET 10 backend and React/TypeScript frontend. Four main projects:

- **Dollars.Api** — ASP.NET Core 10 REST API, JWT auth, EF Core + SQL Server
- **Dollars.React** — React 19 + TypeScript + Vite + Tailwind + shadcn/ui (primary frontend)
- **Dollars.Shared** — Shared C# models and Dapper-based repositories
- **Dollars.Sync** — Console app that pulls financial data from Plaid and SimpleFin

## Frontend (Dollars.React)

```bash
npm run dev        # dev server at localhost:5173
npm run build      # tsc + vite build
npm run lint       # ESLint
npm run format     # Prettier
npm run typecheck  # tsc --noEmit
```

Vite proxies `/api/*` → `http://localhost:5062` during dev. Path alias `@/` maps to `src/`.

Key files:
- `src/lib/auth.tsx` — AuthContext with JWT stored in localStorage
- `src/lib/api.ts` — `apiFetch()` wrapper that injects `Authorization: Bearer` header
- `src/App.tsx` — Router: `/login` and protected `/` routes
- `src/components/protected-route.tsx` — Redirects unauthenticated users to `/login`

shadcn/ui components live in `src/components/ui/`. Add new ones with `npx shadcn@latest add <component>`.

## Backend (Dollars.Api)

```bash
cd Dollars.Api
dotnet run             # API on http://localhost:5062
dotnet ef database update  # apply EF migrations
```

Endpoints are defined as minimal API handler classes in `Features/` (e.g., `LoginUser.cs`, `RegisterUser.cs`). Each feature class has a static `Map()` method called from `Program.cs`.

JWT config and admin seed credentials come from user secrets:
```bash
dotnet user-secrets set "Jwt:SecretKey" "<key>"
dotnet user-secrets set "Admin:User" "<email>"
dotnet user-secrets set "Admin:Password" "<password>"
```

## Sync Service (Dollars.Sync)

```bash
cd Dollars.Sync
dotnet run
```

Pulls transactions from Plaid and SimpleFin APIs. Connection strings and API tokens configured via user secrets. Logs to console, file, and SQL Server via Serilog.

## Database

SQL Server. Schema defined in `Dollars.Sync/db.sql`. EF Core migrations in `Dollars.Api/Migrations/`. `Dollars.Shared` contains `AccountsRepo` and `LogsRepo` using Dapper for raw queries.

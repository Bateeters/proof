# Proof

Proof is a sophisticated cocktail companion web app built with React, TypeScript, and C# / ASP.NET Core. Discover cocktails tailored to your taste profile, season, and available ingredients. Save recipes to your personal cookbook and swap ingredients by taste, cost, or availability.

This is a learning-focused, paired-programming portfolio project. See [docs/](./docs) for full architecture, data model, API design, and roadmap:

- [ARCHITECTURE.md](./docs/ARCHITECTURE.md) — system design and rationale
- [DATA_MODEL.md](./docs/DATA_MODEL.md) — entities and relationships
- [API_DESIGN.md](./docs/API_DESIGN.md) — endpoint reference
- [DESIGN.md](./docs/DESIGN.md) — brand/visual direction (implemented in Phase 10)
- [ROADMAP.md](./docs/ROADMAP.md) — phased build plan
- [PROGRESS.md](./docs/PROGRESS.md) — session-by-session log

## Stack

- **Client**: React + TypeScript (Vite)
- **Server**: C# / ASP.NET Core Web API
- **Database**: PostgreSQL (Docker)
- **External data**: [TheCocktailDB](https://www.thecocktaildb.com/)

## Running locally

Requires: Node.js, .NET SDK, Docker.

```bash
# 1. Start Postgres
docker compose up -d

# 2. Start the API
cd server/Proof.Api
dotnet run

# 3. Start the client (separate terminal)
cd client
npm install
npm run dev
```

Client dev server: http://localhost:5173
API: see console output for the assigned port (Swagger UI available in development mode).

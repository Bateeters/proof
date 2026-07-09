# Proof — Architecture

## What this document is

A map of how Proof is built and *why*, so anyone (including future-us) can get oriented without re-deriving decisions. See also: [DATA_MODEL.md](./DATA_MODEL.md), [API_DESIGN.md](./API_DESIGN.md), [ROADMAP.md](./ROADMAP.md), [PROGRESS.md](./PROGRESS.md).

## System overview

Proof is a monorepo with two independently-runnable pieces talking over HTTP:

```
client (React + TS, Vite dev server, port 5173)
   |  fetch() calls, JWT bearer auth
   v
server (ASP.NET Core Web API, port 5xxx)
   |  EF Core
   v
PostgreSQL (Docker container, port 5432)

server also talks outbound to TheCocktailDB (free public API) to seed/refresh
its local cocktail + ingredient cache.
```

There's no server-side rendering and no BFF layer — the React app is a pure SPA that calls the API directly. That's the simplest shape for a two-person (well, one-person) learning project, and it's a completely standard portfolio pattern.

## Why these technology choices

- **React + TypeScript, Vite**: matches the brief's learning goals. Vite over Create React App because CRA is effectively unmaintained; Vite is the current default and has a much faster dev loop, which matters when you're iterating and learning.
- **ASP.NET Core Web API**: brief's specified backend. It's a mature, well-documented framework — good for learning REST + ORM fundamentals with strong tooling (built-in DI, model binding, Swagger).
- **PostgreSQL in Docker**: free, matches real-world usage (Postgres is extremely common in industry), and Docker means no messy local install — `docker compose up` gives you a disposable, resettable database. Good habit to build early.
- **EF Core**: the standard .NET ORM. We'll write migrations by hand-reviewing what EF generates rather than blindly trusting it, so the SQL underneath isn't a black box.
- **Hand-rolled auth (not ASP.NET Identity)**: Identity is powerful but does a lot of "magic" — table creation, claims, cookie/token plumbing — that would hide the exact mechanics that are valuable to learn here (password hashing, token issuing, middleware). We build a minimal version ourselves: `Account` table + BCrypt hashing + JWT bearer tokens. This is a deliberate scope trade-off — Identity would be the pragmatic choice on a real production team.

## Why a single API project, not layered/Clean Architecture

A "proper" enterprise .NET solution often splits into `Domain` / `Application` / `Infrastructure` / `Api` projects. We're **not** doing that yet. Reasons:

- You're still learning C# syntax and ASP.NET Core basics — adding project-reference boundaries and abstraction layers on top of that is extra cognitive load with no immediate payoff.
- Premature layering for a project this size is over-engineering — YAGNI applies to architecture, not just code.
- The folder structure inside the single project (`Controllers/`, `Models/`, `Data/`, `Services/`, `DTOs/`) already gives you separation of concerns without solution-level ceremony.

We can graduate to a layered structure later as an explicit, deliberate refactor exercise once the fundamentals are second nature — that's actually a great "here's why this pattern exists" lesson to do *after* you've felt the pain layering solves.

## The TheCocktailDB problem, and how we solve it

TheCocktailDB's free tier is generous for single-ingredient lookups but paywalls **multi-ingredient filtering** — which is exactly what the "What Can I Make?" feature needs (recipes matching *several* on-hand ingredients at once).

**Our approach: sync, don't proxy.** Instead of calling TheCocktailDB live on every discovery/filter request, we run a one-time (then periodically-refreshable) sync job that walks the free `search.php?f=<letter>` endpoint A–Z, pulls every cocktail's full detail (ingredients, measures, instructions, image), and writes it into our own Postgres tables (`Cocktail`, `Ingredient`, `CocktailIngredient`).

Once that cache exists, "What Can I Make?" is just **our own SQL query** — no external API call, no paywall, and it's fast. This also means:
- Search/browse is instant and doesn't depend on TheCocktailDB's uptime.
- We fully control ranking/filtering logic (taste profile, season, ingredients-on-hand) since it's all relational data we own.
- If we ever want the paid key for production, the sync job is the *only* thing that changes — everything downstream is unaffected.

The trade-off: our cache can go stale if TheCocktailDB's data changes upstream. Acceptable for a portfolio project; a production version would run the sync on a schedule.

## Auth flow (MVP)

1. `POST /api/auth/register` — Account created, password hashed with BCrypt, stored.
2. `POST /api/auth/login` — credentials verified, short-lived JWT issued.
3. Client stores the JWT **in memory** (React context/state), not `localStorage`. This is deliberate: `localStorage` is readable by any JS on the page, so a successful XSS attack anywhere in the app (or a compromised dependency) can steal every logged-in user's token. In-memory means a page refresh logs you out — that's an accepted trade-off for MVP. A production-grade fix (httpOnly refresh cookie + short-lived access token) is a documented stretch goal, not required for the portfolio version.
4. Each API request sends `Authorization: Bearer <token>`; middleware validates it and attaches the identity to the request.

## Accounts vs. Profiles

This distinction matters and is easy to conflate:

- **Account** = the login. Email + password. One per person signing up.
- **Profile** = a Netflix-style sub-user under an Account. Taste preferences, cookbook, and substitution history all belong to a *Profile*, not the Account directly — because the brief requires "recommendations are always aware of who is drinking," and a household shares an Account but not a palate.

Every feature endpoint after auth operates in the context of an *active profile*, not just an authenticated account.

## Where things live

See the root README for how to run everything locally. Folder structure is documented in [ROADMAP.md](./ROADMAP.md) phase 0 and reflected directly in the repo.

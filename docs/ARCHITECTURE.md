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

## Auth flow

1. `POST /api/auth/register` — Account created, password hashed with BCrypt, stored.
2. `POST /api/auth/login` — credentials verified, a short-lived (60 min) JWT **access token** is returned in the response body, and a long-lived (30 day) **refresh token** is issued as an httpOnly, Secure, `SameSite=Lax` cookie scoped to `/api/auth`.
3. Client keeps the access token **in memory only** (React context/state), never `localStorage` — `localStorage` is readable by any JS on the page, so an XSS attack or a compromised dependency could steal every logged-in user's token. The refresh token accomplishes the same "stay logged in" goal *without* that risk, since `httpOnly` makes it invisible to JavaScript entirely — this is the standard access-token/refresh-token split (updated 2026-09-30; the original MVP version just accepted "refresh logs you out" as a trade-off and flagged this as a stretch goal — it stopped being acceptable once real users were expected to hit it).
4. On app load, the client silently calls `POST /api/auth/refresh` (browser sends the cookie automatically) to restore the session — a page refresh no longer logs anyone out. The refresh token **rotates** on every use (old one revoked, new one issued): if an already-revoked refresh token is ever presented again, that's treated as a theft signal and every active refresh token on the account is revoked, forcing a real login everywhere. Only the SHA-256 hash of a refresh token is ever stored server-side (`RefreshToken` table), same principle as password hashing.
5. `POST /api/auth/logout` revokes the current refresh token server-side and clears the cookie.
6. Each API request sends `Authorization: Bearer <access token>`; middleware validates it and attaches the identity to the request. (This part is unaffected by the refresh flow — the access token still travels as a header, only the refresh token is cookie-based.)

## Admin authorization

Added 2026-09-28, when `AdminController`'s three data-sync endpoints (`sync-cocktails`, `tag-ingredient-flavors`, `tag-cocktail-flavors`) moved from "any logged-in account" to a real `Admin`-only gate — those endpoints can wipe and rebuild core tables, which is fine for a solo-dev MVP but not once this is treated as a product other people's accounts exist on.

- `Account.IsAdmin` (a plain `bool`, defaults `false`) is the source of truth. No roles table — one admin/non-admin distinction doesn't justify one.
- `TokenService` adds a `"role": "Admin"` claim to the JWT **only** when `IsAdmin` is true (not `"role": "User"` for everyone else — keeps ordinary tokens minimal).
- `Program.cs`'s JWT setup sets `RoleClaimType = "role"` explicitly. This is required because `MapInboundClaims = false` (set for the `"sub"` claim, see below) also disables ASP.NET Core's automatic short-name-to-long-URI mapping for `"role"` — without this line, `[Authorize(Roles = "Admin")]` would silently never match.
- `AdminController` uses `[Authorize(Roles = "Admin")]`, not plain `[Authorize]`. Verified: admin token → `200`, logged-in non-admin token → `403`, no token → `401`.
- **Becoming an admin is deliberately not self-service and not seeded in code.** There's no endpoint, no `DataSeedService` entry, no hardcoded admin account anywhere in source. A hardcoded default-admin-with-known-password is itself a common real-world vulnerability (an OWASP-flagged pattern), so the first admin account is granted by a one-off direct database update instead (`UPDATE "Accounts" SET "IsAdmin" = true WHERE "Email" = '...'`). This is a standard, appropriately-scoped bootstrap approach for a project this size — a proper admin-invite flow would be over-engineering right now.
- **Known trade-off:** the role claim is baked into the JWT at login time. If an account's `IsAdmin` is later revoked, any already-issued token keeps working as an admin token until it naturally expires. Bounded by `Jwt:ExpiryMinutes` (currently 60), so the exposure window is small — but worth knowing if `IsAdmin` is ever flipped off for an account that's actively logged in.

## Accounts vs. Profiles

This distinction matters and is easy to conflate:

- **Account** = the login. Email + password. One per person signing up.
- **Profile** = a Netflix-style sub-user under an Account. Taste preferences, cookbook, and substitution history all belong to a *Profile*, not the Account directly — because the brief requires "recommendations are always aware of who is drinking," and a household shares an Account but not a palate.

Every feature endpoint after auth operates in the context of an *active profile*, not just an authenticated account.

## Standing rule: controllers never return entities directly

Established in Phase 1 (`AccountsController`): every endpoint returns a **DTO** (`DTOs/`), never an EF Core entity straight from `Models/`. The entity is the database's shape; the DTO is what's safe to hand to a client. This isn't optional per-endpoint judgment — `Account` has `PasswordHash` on it today, and any future entity could end up with its own field that shouldn't leave the server, so the rule is applied uniformly rather than decided case-by-case. Pattern: query entities via the `DbContext`, project to a DTO (typically via LINQ `.Select(...)`), return the DTO.

## Reference-data seeding

`Spirit` and `FlavorTag` (Phase 6) are curated lookup lists, not user- or sync-generated data. Rather than EF Core's migration-embedded `HasData()` (which requires deterministic ids baked into the migration itself and hasn't been needed anywhere else in this project), seeding happens in `DataSeedService`, run once at app startup in `Program.cs` via a manually-created DI scope (`app.Services.CreateScope()` — startup code runs before any HTTP request exists to hang a scoped `DbContext` off of, so one has to be created by hand). Each seed method checks which names already exist (one query) and only adds the missing ones — not a whole-table `AnyAsync()` skip, which would silently stop new seed values (like `"Spiced"`, added later) from ever being inserted once the table already had older rows in it. Re-running (e.g. every local `dotnet run`) is a no-op once the data exists — verified by restarting the server and confirming row counts didn't change.

## Where things live

See the root README for how to run everything locally. Folder structure is documented in [ROADMAP.md](./ROADMAP.md) phase 0 and reflected directly in the repo.

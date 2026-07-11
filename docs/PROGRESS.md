# Proof — Progress Log

Running session-by-session log. Newest entry on top. Purpose: let any session (especially a fresh one with no memory of prior conversations) pick up exactly where we left off — what's done, what's decided, what's next.

---

## 2026-07-09 — Kickoff & architecture

**Did:**
- Reviewed project brief v1.1, clarified scope via Q&A with Brian.
- Locked in architecture decisions: TheCocktailDB sync-to-local-cache strategy (solves the multi-ingredient-filter paywall), hand-rolled JWT auth (not ASP.NET Identity), Account/Profile split, single-project API structure (no Clean Architecture layering yet), IsCustom flag on Cocktail for future custom-recipe support.
- Wrote `docs/ARCHITECTURE.md`, `docs/DATA_MODEL.md`, `docs/API_DESIGN.md`, `docs/ROADMAP.md`.
- Confirmed with Brian: near-beginner in both React and C#, rule-based substitution engine for MVP (AI swap-in deferred), hints-first for logic/algorithms only (setup/config handed over directly), Postgres via Docker, project-embedded challenge briefs only (no generic warm-ups), monorepo layout.

**Decided:**
- See "Open architectural calls" in `docs/ARCHITECTURE.md` for full rationale on each.

- Scaffolded `/client` (Vite + React + TS, default template) and `/server/Proof.Api` (ASP.NET Core Web API, default template, `net10.0`).
- Added `docker-compose.yml` for local Postgres 16, root `.gitignore`, and rewrote `README.md` with run instructions.
- Fixed a high-severity advisory (GHSA-v5pm-xwqc-g5wc, transitive `Microsoft.OpenApi` 2.0.0) surfaced by the default web API template — pinned an explicit `Microsoft.OpenApi` 2.10.0 reference in `Proof.Api.csproj` to override it.
- Verified end-to-end: `docker compose up` starts Postgres cleanly; `dotnet run` serves the default sample endpoint (200 OK); `npm run dev` serves the default Vite page (200 OK). All torn back down afterward — nothing left running.

**Next:**
- Phase 1: first EF Core entity (`Account`) + DbContext + migration + one real endpoint. This is where Brian starts driving with hints-first support.

---

## 2026-07-11 — Phase 1: first entity, DbContext, migration, endpoint

**Did:**
- Regenerated `server/Proof.Api` with the controller-based template (`-controllers` flag) instead of the minimal-API default, matching `ARCHITECTURE.md`'s planned folder structure.
- Added EF Core + Npgsql provider packages, `dotnet-ef` CLI tool.
- Established the folder-matches-namespace convention (`Proof.Api.Models`, `.Data`, `.Controllers`, `.DTOs`) across all new files.
- Brian wrote `Models/Account.cs` (first entity) and `Data/ProofDbContext.cs` (first DbContext) with hints-first support — covered properties vs. methods, type casing, nullable reference types / `required`, value vs. reference types (and why that explains EF Core's `DbSet<T>` nullable-warning exemption, verified empirically), inheritance + constructor base-call syntax, and class accessibility consistency (`CS0051`, caused by `internal`-by-default classes referenced from `public` signatures).
- I wired `ProofDbContext` into `Program.cs` via `AddDbContext` (DI + connection string), generated and applied the `InitialCreate` migration — `Accounts` table now exists in Postgres, verified via `psql`.
- Brian built `Controllers/AccountsController.cs` (`GET /api/accounts`) — first real endpoint, verified end-to-end against real Postgres.
- Caught and fixed a real security gap before calling it done: the naive controller returned the full `Account` entity, including `PasswordHash`, in the JSON response. Introduced `DTOs/AccountDto.cs` and a `.Select()` projection to fix it — verified with a throwaway test row that the response now excludes `PasswordHash` while the DB query (correctly) still reads it. Documented this as a standing rule in `ARCHITECTURE.md`: controllers never return entities directly.
- Fixed a build lock issue (rebuild appeared to hang — actually a still-running `dotnet run` locking the output DLL) — now a known troubleshooting step for both of us.

**Mentoring calibration (important for future sessions):** Brian gave explicit feedback mid-session that blank-file + verbal-description tasks weren't working for new ASP.NET Core/EF Core patterns — he has real programming experience but these are genuinely new framework conventions (DI, attribute routing, DbContext lifecycle), not a general-beginner situation. Shifted to skeleton-file-with-marked-blanks for new patterns, plain/simple ("8th grade") language in explanations. Saved to persistent memory (not just this doc) since it should apply beyond this project. See memory entries `proof-mentoring-style` and `feedback-mentoring-scaffolding-level` if picking this up in a future session without this conversation's context.

**Next:**
- Phase 2: React frontend shell that calls `GET /api/accounts` and renders the result — first frontend slice, hooks/fetch/TS types.

---

<!-- Add new entries above this line, newest on top. -->

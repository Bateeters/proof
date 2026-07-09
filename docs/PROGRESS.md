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

<!-- Add new entries above this line, newest on top. -->

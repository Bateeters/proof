# Proof — Roadmap

Phased build order. Each phase = a concept explanation, a scaffolding step (done directly, explained), and — where the logic is meaty — a challenge brief for Brian to implement with hints-first support. Status is tracked here; running session-by-session notes live in [PROGRESS.md](./PROGRESS.md).

| # | Phase | What it teaches | Status |
|---|---|---|---|
| 0 | Docs + skeleton scaffolding | Repo/tooling layout, Docker basics | ✅ done |
| 1 | Backend foundations (first entity, DbContext, one endpoint) | REST fundamentals, EF Core basics, migrations | ✅ done |
| 2 | Frontend foundations (React shell calling the endpoint) | Components, hooks, fetch, TS types | ✅ done |
| 3 | Auth (register/login, BCrypt, JWT, middleware, React auth context) | Password hashing, token auth, protected routes | ✅ done |
| 4 | Profiles (multi-profile CRUD, active-profile switching) | Nested resource design, frontend context/state | ✅ done |
| 5 | TheCocktailDB sync + discovery (browse/search, seasonal toggle) | External API integration, background jobs, caching | ⬜ not started |
| 6 | Taste preferences + taste-based ranking *(challenge brief)* | Lookup tables/join entities, scoring/ranking algorithms | ⬜ not started |
| 7 | Personal cookbook (save/list/remove) | CRUD on a relationship, ownership/privacy checks | ⬜ not started |
| 8 | Substitution engine *(challenge brief)* | Rule-lookup systems, conditional branching UX | ⬜ not started |
| 9 | "What Can I Make?" *(challenge brief)* | Set logic, SQL joins, ranking by closeness | ⬜ not started |
| 10 | Polish (theme, empty states, responsive pass) | CSS systems, copywriting-as-UX | ⬜ not started |
| 11 | Stretch: LLM-backed substitution | Prompting, provider integration | ⬜ not started |

## Phase 0 scope (current)

- `docs/` populated (this file + ARCHITECTURE, DATA_MODEL, API_DESIGN, PROGRESS)
- Root `README.md` with project overview + local run instructions
- `docker-compose.yml` — Postgres container for local dev
- `/client` — Vite + React + TypeScript scaffold (default template, no custom UI yet)
- `/server` — ASP.NET Core Web API scaffold (default template, no custom entities yet)
- Verify: Postgres container starts, `dotnet run` serves the default endpoint, `npm run dev` serves the default page

Feature work (Phase 1 onward) starts in the next session.

## Post-MVP ideas (not scheduled)

From the original project brief:
- Custom recipe creation (data model already supports this — `Cocktail.IsCustom`/`OwnerProfileId` — UI deferred)
- Shared cookbook — optional sharing between household profiles
- Rating and notes on saved recipes
- Mobile bartending mode — step-by-step instructions, scaled measurements
- Expand mythology/topic as a separate future project (Norse, Egyptian, Roman)
- Social/sharing features

Added during Phase 4 (2026-07-17), from a design discussion with Brian about why Account/Profile are split:
- **"Shared" / crowd-pleaser mode** — a way to get recommendations spanning *all* profiles on an account at once (e.g. "find something everyone here would like"), as opposed to the MVP's one-active-profile-at-a-time model. Explicitly V2+, not MVP — flagged so it isn't confused with or accidentally built into the Phase 6 ranking work, which is scoped to a single active profile only.

# Proof — Progress Log

Running session-by-session log. Newest entry on top. Purpose: let any session (especially a fresh one with no memory of prior conversations) pick up exactly where we left off — what's done, what's decided, what's next.

---

## 2026-07-16 to 2026-07-22 — Phase 4: Profiles

**Did:**
- Scoped Phase 4 down to profile CRUD + active-profile switching only; moved taste preferences (spirits/flavors/allergens — multiple lookup/join tables) to bundle with Phase 6's ranking work instead of building them with nothing yet consuming the data. Documented in `ROADMAP.md`/`API_DESIGN.md`.
- Brian built the `Profile` entity — first use of a real foreign key + navigation property (`AccountId` + `Account`). Learned why the navigation property can't be `required` (`= null!` instead) and confirmed via the generated migration that it's what makes EF Core emit a real FK constraint (`FK_Profiles_Accounts_AccountId`, cascade delete) rather than just a bare column.
- I proactively fixed a real ASP.NET Core gotcha before it could cause a confusing debugging session: `options.MapInboundClaims = false` in `Program.cs`, since the framework silently remaps JWT claim names (like `"sub"`) to legacy URIs by default, which would've made `User.FindFirst("sub")` mysteriously fail.
- Brian built `ProfilesController` (`GET`/`POST /api/profiles`) — first time reading identity from JWT claims (`User.FindFirst(JwtRegisteredClaimNames.Sub)`) to scope a query to "my account only." Caught his own bug: `ProfileDto` construction was missing `Id`/`CreatedAt` (compiled fine since neither was `required`, but would've returned `Guid.Empty` for every profile's id — a real bug since nothing else in this phase would've caught it before it hit the frontend).
- Verified end-to-end via curl: created two profiles under one account (one with an explicit `avatarColor`, one omitted to confirm the server-side `"gray"` default), confirmed both scoped correctly to the authenticated account via the generated SQL's `WHERE` clause.
- Frontend: `Profile.ts` type, `ProfileContext.tsx` (a context that depends on another context — calls `useAuth()` internally; reacts to login/logout via `token` in its `useEffect` dependency array), `ProfileSwitcher.tsx` (profile list with click-to-switch and active-profile highlighting, plus a create-profile form).
- Design discussion with Brian (captured in `ROADMAP.md`'s Post-MVP ideas): confirmed the Account/Profile split's actual justification is the shared-device UX (switching whose profile is active without a full logout/login, e.g. a phone passed around while making drinks together), not shared billing, which doesn't apply since Proof has no subscription concept. Brian proposed a V2 "crowd pleaser" mode spanning all profiles on an account — explicitly deferred, documented so it isn't confused with the single-active-profile Phase 6 ranking scope.

**Real bugs hit and fixed this phase:**
- `ProfileDto` missing `Id`/`CreatedAt` in construction — compiled clean (neither was `required`) but would've silently returned wrong data (`Guid.Empty` ids) to every consumer. Brian caught the *pattern* himself after this was pointed out once.
- Object reference-equality bug in `ProfileSwitcher`'s active-profile check (`activeProfile == profile`) — objects compare by reference in JS, not by value, so this would've broken the moment `profiles` got re-fetched (new object instances, same data). Fixed by comparing `.id` (a string, i.e. a real value) on both sides instead of whole objects.
- Missing `Authorization` header on `ProfileContext`'s `createProfile` POST (had `Content-Type` but not the auth header) — would've 401'd the moment anyone tried to create a profile.
- A genuinely obscure one: bare `SubmitEvent` in `ProfileSwitcher.tsx` (missing the `import type { SubmitEvent } from "react"` that `LoginForm`/`RegisterForm` both have) silently resolved to an unrelated *global DOM* type with the same name instead of erroring "not found" — produced a confusing structural-mismatch error rather than a clear missing-import one.
- `onChange` on the new-profile input was calling `createProfile()` (a server POST) on every keystroke instead of updating local state — Brian caught and fixed this himself while addressing the `SubmitEvent` issue, without it being pointed out separately.

**Mentoring calibration (important for future sessions):** Brian gave explicit corrective feedback this phase: even for steps he's already done before (e.g. destructuring a hook's return value — he'd done this 3+ times), don't paste the literal line of code, describe it in words instead. Updated `feedback-mentoring-scaffolding-level` memory with this refinement. The prior "skeleton for new patterns, blank slate for repeated ones" calibration still holds — this refines *how* to hint at repeated patterns specifically (words, not snippets), not when.

**Next:**
- Phase 5: TheCocktailDB sync + discovery (browse/search, seasonal toggle).

---

## 2026-07-13 to 2026-07-15 — Phase 3: Auth

**Did:**
- Backend: added `BCrypt.Net-Next` for password hashing and `Microsoft.AspNetCore.Authentication.JwtBearer` for token issuing/validation.
- Brian built `RegisterRequestDto`/`LoginRequestDto` (input DTOs — first time distinguishing input vs. output DTOs) and `AuthResponseDto` (`{ token, account }`).
- Brian built `POST /api/auth/register` (hash password, save `Account`, stage/save via EF Core's `Add`+`SaveChangesAsync` — first DB write) and `POST /api/auth/login` (`FirstOrDefaultAsync` lookup + `BCrypt.Verify`, identical `401` for "no such email" vs. "wrong password" to avoid account enumeration — Brian identified this requirement himself before being told).
- I built `TokenService` (JWT generation) and wired JWT validation middleware into `Program.cs` — pure framework/crypto ceremony, handled directly per our usual split.
- Verified end-to-end repeatedly via curl: real bcrypt hash in Postgres (not plaintext), correct-credentials login returns a token, wrong-password and nonexistent-email both return identical `401`s.
- Caught a real gap via self-review (same pattern as the Phase 1 `PasswordHash` catch): `Register` wasn't actually returning a token despite `API_DESIGN.md` documenting auto-login — fixed to match.
- Frontend: `AuthContext.tsx` (React Context — first use of `createContext`/`useContext`/Provider pattern), `LoginForm.tsx`/`RegisterForm.tsx` (first controlled-input forms), wired `AuthProvider` around `App` in `main.tsx`, `App.tsx` now conditionally renders forms vs. logged-in view based on auth state.
- Closed the loop: added `[Authorize]` to `AccountsController` (Brian's call, via explicit prompt — endpoint was previously wide open despite auth now existing) and updated `AccountsList.tsx` to send the `Authorization: Bearer` header, including adding `token` to its `useEffect` dependency array.
- Brian requested and got a design-direction doc (`docs/DESIGN.md`) — light "Marble" (white/gray/gold) and dark "Midnight Sunset" (indigo/purple/pink/orange/gold) themes with a toggle, gold single-line logo. Not implemented until Phase 10; captured now so it isn't lost.

**Real bugs hit and fixed this phase (good reference if similar ones recur):**
- `BCrypt.Net-Next`'s namespace (`BCrypt.Net`) and class (`BCrypt`) share a name — bare `BCrypt.HashPassword(...)` even with `using BCrypt.Net;` resolves to the namespace, not the class. Fix: fully qualify as `BCrypt.Net.BCrypt.HashPassword(...)`.
- .NET 10 added `System.Linq.AsyncEnumerable` (already in scope via the implicit `System.Linq` using), which has its own same-named `FirstOrDefaultAsync` — without `using Microsoft.EntityFrameworkCore;` in a file, calls silently resolve to the wrong one and produce a confusing `CS0411` type-inference error rather than a clean "not found."
- React's `FormEvent` type is deprecated (its own JSDoc says "doesn't actually exist") — use `SubmitEvent` for form `onSubmit` handlers instead. Brian caught this one himself from an IDE hint.
- Classic `.then()` chaining bug in `AuthContext`: chained `.then()` calls each receive the *previous* `.then()`'s return value, not the original data — `setAccount`/`setToken` return nothing, so a third `.then()` tried to read `.token` off `void`. Resolved by switching to `async`/`await`.

**Mentoring notes:** Brian is picking up the skeleton-with-blanks pattern well and increasingly needs less scaffolding on repeated patterns (RegisterForm from LoginForm, LoginRequestDto from RegisterRequestDto) — good sign the calibration from [[feedback-mentoring-scaffolding-level]] is working as intended. He's also proactively asking "why" on design decisions (DTO duplication vs. reuse, enumeration protection) rather than just accepting instructions, and catching things independently (IDE deprecation hints, the missing auto-login token).

**Next:**
- Phase 4: Profiles (multi-profile CRUD under an account, active-profile switching on the frontend).

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

## 2026-07-12 — Phase 2: frontend foundations

**Did:**
- Brian wrote `client/src/types/Account.ts` (TS shape matching `AccountDto`: id/email/createdAt, all strings — caught and fixed a `Date` vs `string` mismatch, since JSON has no native date type; also caught a missing `export` via a real `tsc` failure, not just told about it).
- Brian built `client/src/components/AccountsList.tsx` — first component combining `useState`, `useEffect`, and `fetch`. Given a skeleton with the two non-obvious lines (`useState<Account[]>([])`, the empty `useEffect` dependency array) pre-explained and the fetch/render logic left as blanks, per the scaffolding-level adjustment from last session. Got the fetch chain, state update, and list rendering (including the `key` prop) right on the first attempt.
- I handled the cross-cutting config: `client/.env.development` + `vite-env.d.ts` for `VITE_API_BASE_URL` (client didn't know the API's actual port — `server/Proof.Api/Properties/launchSettings.json` has it running on `5168`, not the `5099` I'd been using for ad hoc verification), and CORS middleware in `Program.cs` (`AddCors`/`UseCors`) allowing `http://localhost:5173`. Cleared out the Vite starter template's demo content from `App.tsx` and wired in `AccountsList`.
- Verified end-to-end via `tsc --noEmit`, a curl with a spoofed `Origin` header confirming the `Access-Control-Allow-Origin` response header, and both `App.tsx`/`AccountsList.tsx` transforming cleanly through Vite. Brian had his own `dotnet run`/`npm run dev` going in his own terminals by the end, so real-browser confirmation happened on his side.

**Next:**
- Phase 3: Auth (register/login, BCrypt hashing, JWT issuing/validation, React auth context). First real "logic-heavy" backend work since Phase 1, plus the first meaningful frontend state-management pattern (auth context).

---

<!-- Add new entries above this line, newest on top. -->

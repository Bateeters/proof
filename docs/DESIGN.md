# Proof — Design Direction

Visual/brand direction. Not implemented yet — this is reference for Phase 10 (Polish), captured now so it isn't lost. See [ROADMAP.md](./ROADMAP.md) for where that fits in the build order.

## Brand personality

From the original project brief: a high-end cocktail bar that knows how to have a good time. Elevated and sophisticated, never cold or pretentious. Playful undertone lives in copy, empty states, loading messages, and micro-interactions — the visual design itself stays clean, sharp, and luxurious rather than cartoonish.

Example copy tone (from brief):
- Empty cookbook state: "Nothing here yet. Let's fix that."
- No results state: "We've got nothing for that. Try loosening the filters."

## Typography

- Serif for headings
- Clean sans-serif for UI/body text

## Color themes — light/dark toggle (added 2026-07-15)

Two full themes, user-toggleable, both sharing the same gold accent and logo treatment.

**Light theme — "Marble"**
- White/off-white base with gray "marbled" texture throughout
- Gold accents

**Dark theme — "Midnight Sunset"**
- Primarily dark purple/indigo
- Pinks, reds, and oranges mixed in as accent/gradient tones
- Gold accents (same as light theme — gold is the constant across both themes)

**Logo**: a simple outline / single-line-style mark, gold, consistent across both themes.

## Open questions for Phase 10

- Exact hex values / palette scale for both themes (marble grays, midnight sunset gradient stops) — TBD when we actually build the theme system
- Whether the toggle is a simple light/dark switch or something more visually thematic given the "marble" vs "midnight sunset" framing
- How the gold accent is implemented so it stays visually consistent (same gold value) across both themes' otherwise-different palettes

# Current Task: Rustweaver saved-state load and HUD sync audit

## Objective
Perform a focused audit and fix pass for all Rustweaver saved-state loading and synchronization, with special attention to loading on startup/join instead of only during casting.

## Hard scope boundary
This is an audit + correction pass for save-state loading and sync timing only.

This task must not redesign:
- HUD layout
- corruption math
- spell data
- Tome UI
- gameplay systems

This task must not add new features, recipes, particles, or Harmony patches.

## Required outcome
1. Brand new Rustweavers start at `0 / 200`.
2. Existing Rustweavers preserve their saved corruption exactly across reconnect/load.
3. The HUD visual and number show the correct saved value immediately after join/startup.
4. The HUD must not wait for a spell cast to refresh from authoritative state.
5. A player at `200 / 200` must visually show `200 / 200` immediately after join.
6. Hard lock behavior must still work.
7. No join/load path may overwrite existing current corruption with `0` or `200` incorrectly.

## Notes
- Separate default initialization from persisted-state loading
- Hydrate the HUD from authoritative state on join/startup
- Keep AGENTS.md unchanged

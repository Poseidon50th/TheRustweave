# Current Task: Rustweaver's Tome placeholder interaction

## Objective
Make the Rustweaver's Tome usable as a Rustweaver-only placeholder item.

## Hard scope boundary
This task must only add a minimal interaction for the Rustweaver's Tome.

This task must not add:
- HUD elements
- spellcasting systems
- corruption mechanics
- spell learning
- GUI screens
- recipes
- Harmony patches

## Required behavior
1. When a player right-clicks or otherwise uses the Rustweaver's Tome:
   - if the player is the Rustweaver class, show a simple success message
   - if the player is not the Rustweaver class, show a simple rejection message
2. Keep the logic minimal and clean.
3. Use the existing Rustweaver class identifier already added by the mod.
4. Do not implement any real spells yet.

## Messages
- Rustweaver success: `The Rust answers your call.`
- Non-Rustweaver rejection: `You do not understand the tome.`

## Files to inspect and update as needed
- `TheRustweave source files`
- `TheRustweave/assets/therustweave/itemtypes/rustweaverstome.json`
- Any registration code required for the tome's custom behavior/class

## Notes
- Keep the existing item code, textures, shape, transforms, creative tab placement, and attributes intact
- Prefer a clean item class or behavior attachment approach consistent with Vintage Story modding
- Do not add networking unless absolutely required
- Do not add persistent save data yet unless absolutely required

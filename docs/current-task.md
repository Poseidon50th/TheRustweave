# Current Task

Expand the Rustweave spell framework so every currently recognized target/effect type is executable, validated, and documented.

## Expected file changes

- `TheRustweave/RustweaveSpells.cs`
- `TheRustweave/RustweaveScaffold.cs`
- `TheRustweave/RustweaveUi.cs` only if labels/details need new fields
- `TheRustweave/assets/therustweave/config/spells.json` for hidden validation spells
- `TheRustweave/assets/therustweave/lang/en.json` only if labels are needed
- `docs/spell-framework.md`
- `AGENTS.md` for the persistent spell-framework rule update

## Scope

- Canonical school spelling is `Twining`.
- `Twinning` must remain a backwards-compatible alias.
- Existing target types must keep working:
  - `self`
  - `heldItem`
  - `inventory`
  - `lookEntity`
  - `lookPlayer`
  - `lookNonPlayerEntity`
  - `lookDroppedItem`
  - `lookBlock`
  - `lookBlockEntity`
  - `lookContainer`
  - `lookPosition`
  - `selfArea`
  - `lookArea`
- All currently recognized effect types must validate and execute with a real runtime outcome.

## Runtime systems to preserve/add

- Persistent active area registry
- Temporal history tracking
- General active effect registry for purge/cancel/counter/reflect/ward-like systems
- Safe teleport helpers
- Summon tracking and expiry
- Protection and PvP checks

## Validation rules

- Look targets require positive range.
- Area targets require positive radius.
- Timed effects require positive duration.
- Block-changing effects must respect claims/protection.
- Harmful player-target effects must obey PvP restrictions.
- Unsupported future effects must fail clearly.
- No silent no-ops.

## Preview rules

- Keep world-space previews lightweight and stable.
- Do not reintroduce tooltip/HUD target indicators.
- Preserve caster preview, server-locked preview, and targeted-player warning separation.

## Build/test checklist

1. Build the project.
2. Confirm `spells.json` loads.
3. Confirm hidden validation spells are recognized.
4. Confirm supported target/effect types resolve.
5. Confirm existing spells still cast.
6. Confirm block/protection/PvP validation fails cleanly where expected.

# AGENTS.md

This repository is for **TheRustweave**, a Vintage Story mod.

## Baseline requirements
- **Target game version:** Vintage Story **1.22.0**
- **Target framework:** **.NET 10** (`net10.0`)
- Do not downgrade framework or widen the game dependency unless explicitly instructed.
- Preserve the mod domain and identity:
  - `modid`: `therustweave`
  - asset domain: `therustweave`

## Project structure
- Main mod project: `TheRustweave/`
- Main code entrypoint: `TheRustweave/TheRustweaveModSystem.cs`
- Mod manifest: `TheRustweave/modinfo.json`
- Assets root: `TheRustweave/assets/therustweave/`
- Language file: `TheRustweave/assets/therustweave/lang/en.json`

## Implementation rules
- Favor **minimal, targeted changes**.
- Do not add features outside the current task.
- Do not rename files, namespaces, mod IDs, or asset domains unless explicitly asked.
- Keep changes compatible with Vintage Story 1.22.0 content/code loading.
- Use existing Vintage Story conventions for assets, config files, and localization keys.
- For class/trait content, prefer **data-driven JSON assets** over C# when possible.
- Only add C# when the requested feature cannot be achieved cleanly with content assets alone.

## Character class content rules
- Custom classes must be added via:
  - `TheRustweave/assets/therustweave/config/characterclasses.json`
- Custom traits must be added via:
  - `TheRustweave/assets/therustweave/config/traits.json`
- Localized class and trait text belongs in:
  - `TheRustweave/assets/therustweave/lang/en.json`
- When defining trait attribute text, use Vintage Story lang key format:
  - `game:charattribute-<attribute>-<value>`
- For negative numeric values, expect the double-dash format in lang keys, e.g.:
  - `game:charattribute-hungerrate--0.25`

## Scope control
Unless the task document says otherwise, do **not**:
- add HUD systems
- add spellbooks or spell logic
- add corruption systems
- add particles, shaders, or GUI work
- add Harmony patches
- refactor unrelated files
- touch build scripts beyond what the task strictly requires

## Task document policy
- The current prompt-specific instructions live in:
  - `docs/current-task.md`
- This file is intended to be **replaced on each new prompt** unless the user explicitly asks to keep or extend a previous task document.
- If a future prompt requires additional persistent repository instructions, update `AGENTS.md`.
- If a future prompt only changes implementation scope, replace `docs/current-task.md` and leave `AGENTS.md` intact.

## Output expectations
When completing a task, prefer to:
1. edit the smallest set of files necessary
2. preserve buildability
3. preserve clear localization entries
4. note any required add/remove/replace actions if the user asked for a delta instead of full replacement

## Current Implementation Priority: Data-Driven Spell Executor

When modifying The Rustweave, prioritize the current spell-system architecture task:

- Spell definitions should come from JSON, preferably `spells.json`.
- C# should execute spell behavior through a server-authoritative spell effect executor.
- Spells must support an `effects[]` list, not only one hardcoded effect.
- Unknown `effectType` or `targetType` values should be rejected during spell validation/loading.
- Gameplay effects must run server-side.
- Client-side code may display HUD, cast bar, particles, and sounds, but must not apply gameplay effects.
- Corruption cost is charged on successful cast completion only.
- Failed casts should not add corruption and should not trigger cooldown.
- Add basic per-player, per-spell cooldowns.
- Initial target types are:
  - `self`
  - `heldItem`
  - `lookEntity`
- Initial effect types are:
  - `none`
  - `repairHeldItem`
  - `addHealth`
  - `damageEntity`
  - `teleportForward`
  - `ventCorruption`
  - `spawnParticles`
  - `playSound`

Do not attempt unrelated polish fixes during this task unless required for the executor to work. Known deferred issues:
- HUD does not immediately sync saved corruption when joining old worlds/servers.
- Rustweaver's Tome GUI background is transparent.

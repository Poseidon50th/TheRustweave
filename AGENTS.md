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

## Spell framework policy
- Spell framework work is only in scope when `docs/current-task.md` explicitly asks for it.
- All currently recognized spell effect types are expected to be executable, validated, and have a runtime outcome.
- Future new effect types must not be added to `SpellEffectTypes.Supported` unless they also have validation and real runtime behavior.
- No silent no-ops: if an effect cannot execute safely, it must fail clearly during validation or runtime.
- Block-changing effects must respect protection and claims.
- Temporal/history tracking must stay bounded and avoid log spam.
- Loreweave or lore-flavored spells must only gain mechanical behavior when the spell JSON explicitly assigns it and the current task authorizes that behavior; do not rely on a blanket cosmetic-only allowlist in framework work.

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

## Vintage Story GUI Rule

For Rustweaver Tome work, do not use `SingleComposer = composer` inside rebuild, refresh, tab-switch, or button-callback flows.

The current crash path is caused by assigning `SingleComposer` while `GuiDialog` internal composer storage is null or unsafe.

Use a stable `GuiDialog` lifecycle:
- constructor must call `base(capi)`
- build composer once on open where possible
- use `Composers["main"]` or a known-safe Vintage Story GUI pattern
- do not reassign composers from active mouse callbacks
- prefer updating text/state elements over full recomposition
- if rebuild is unavoidable, close/dispose/recreate the dialog safely outside the callback

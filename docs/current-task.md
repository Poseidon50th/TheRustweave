# Current Task: Rustweaver Tome title and tier label fix

## Objective
Apply a focused Rustweaver Tome UI fix only:
1. Move the Tome title and exit/close button down further.
2. Fix Learned tab tier labels so they display readable text like `Initiate Spell`, not asset/lang keys.

## Scope
This task is only for Tome GUI layout and Tome-local tier label display.

Do not add:
- spells
- rituals
- glyph mechanics
- NPCs
- assets
- Harmony patches
- unrelated systems

Do not change:
- spell loading
- spell casting
- corruption
- mentor learning
- discovery items
- Rust Tablet behavior
- saved player data

## Required behavior
1. Move the title and close button downward.
2. Keep the Learned tab row to spell name, tier label, and Prep only.
3. Tier labels must display readable text such as `Initiate Spell`.
4. Preserve all current Tome behavior.

## Validation requirements
After the change:
1. The project builds successfully.
2. The Tome opens without new errors.
3. The title and close button are moved downward.
4. Learned rows show readable tier labels.
5. Learned tab does not display raw localization keys.
6. No unrelated systems were changed.


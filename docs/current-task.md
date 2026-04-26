# Current Task: Rustplane Sage mentor learning backend

## Objective
Implement backend support for Rustplane Sage mentor learning, including persistent studies, configurable payment, study completion, and admin/debug commands.

## Scope
This task is limited to mentor backend/config/commands.

Do not add:
- Tome UI work
- rituals
- player-made scrolls
- Rustbound Magic dependencies
- hostile/corrupted mentors
- school specialization
- prerequisite chains

## Required behavior
1. Rustplane Sage mentor studies must be stored per player and persist across logout/world reload.
2. Mentors must offer only enabled, non-hidden, non-Loreweave spells.
3. Locked/learned validation must happen before study starts.
4. Study timers must continue using world/calendar time when possible.
5. Payment must be configurable by tier.
6. Completing a study must call `LearnSpell(player, spellCode, "mentor:rustplane-sage")`.
7. Admin/debug commands must exist for listing, starting, checking, and completing mentor studies.

## Validation requirements
After the repair:
1. Game starts without errors.
2. Mentor config loads.
3. `/rustweave mentor list` shows teachable spells.
4. `/rustweave mentor study <spellcode>` starts a paid study.
5. `/rustweave mentor status` reports active studies.
6. Due studies complete on login/tick and unlock the spell.
7. Studying persists across logout/world reload.
8. Invalid spell codes fail cleanly.
9. Loreweave spells are never taught.

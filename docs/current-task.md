# Current Task: Rustweaver Tome UI stabilization and prepared-slot flow

## Objective
Fix the Rustweaver's Tome UI so the Learned and Prepared pages work reliably, the dialog stays fixed-position, and prepared spells are stored and cast through prepared slots only.

## Hard scope boundary
This task is only for the Tome UI, tab switching, prepared-slot flow, and the Tome item interaction entrypoint.

This task must not add:
- new gameplay systems
- glyphs or rituals
- new spells
- corruption redesigns
- HUD redesigns
- draggable Tome behavior
- unrelated refactors

## Required behavior
1. The Tome must open as a fixed-position dialog.
2. Dragging/moving the Tome must be disabled.
3. The Tome background must be visible, opaque or mostly opaque, and larger than the content area.
4. The Tome must have two tabs:
   - Learned Spells
   - Prepared Spells
5. Both tabs must open reliably.
6. Tab switching must safely rebuild or refresh the dialog.
7. Interactive buttons must not overlap.
8. Button labels must fit cleanly.
9. Preparing a spell must store the spell code into a prepared slot.
10. Preparing a spell must not directly make that spell the cast target.
11. Casting must use the active prepared slot only.
12. The Tome item interaction must open the rebuilt dialog cleanly and not create duplicate dialogs.

## Prepare flow
1. If a prepared slot is selected, Prepare must use that slot.
2. If no prepared slot is selected, Prepare must use the first empty slot.
3. The same spell cannot be prepared in multiple slots.
4. If all slots are full and no slot is selected, show `No empty spell slots.`
5. After preparing, refresh the prepared-slot state immediately.

## Prepared slots
1. There must be exactly 9 prepared slots.
2. Prepared spells must be stored by spell code.
3. Prepared Spells must show all 9 slots.
4. Each slot must show:
   - slot number
   - spell name or `Empty`
   - active/selected indicator
   - Clear control
5. Clicking a slot must make it the active casting slot.
6. Clearing the active slot must leave that slot selected but empty.
7. Invalid saved spell codes must not crash the Tome.

## Learned spells
1. Show only learned/default-learned enabled spells.
2. Sort by:
   - school
   - tier
   - name
3. Each learned spell row should allow selection and preparation.
4. Selecting a learned spell should show a details panel.

## Logging requirements
Add focused debug logs for:
- Tome opened
- Active tab changed
- Prepared page opened
- Prepare clicked with spell code
- Target slot chosen
- Slot before/after prepare
- Duplicate prepare rejected
- No empty slot rejected
- Slot selected
- Slot cleared
- Cast requested from active slot
- Loaded mod origin/path if available

## Validation requirements
After the repair:
1. The Tome opens with a visible non-transparent background.
2. Learned Spells opens.
3. Prepared Spells opens.
4. Buttons do not overlap.
5. Button text fits.
6. The Tome does not crash from dragging because moving is disabled.
7. Rust Mend can be prepared into slot 1 when no slot is selected.
8. Selecting slot 5 and preparing Ruststep stores it in slot 5.
9. Duplicate prepares are rejected.
10. Clearing removes the spell from a slot.
11. Casting uses the selected prepared slot only.

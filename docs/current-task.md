# Current Task: Rustweaver Tome tabbed UI and prepared-slot fix

## Objective
Rebuild the Rustweaver's Tome UI into a tabbed interface and fix spell preparation so prepared spells are stored in prepared slots instead of becoming directly castable.

## Hard scope boundary
This task is only for the Tome UI, learned spell display, prepared spell slot handling, and the direct prepare/cast flow.

This task must not add:
- new spells
- glyphs or rituals
- corruption redesigns
- HUD sync redesigns
- GUI redesigns outside the Tome dialog
- unrelated refactors

## Required behavior
1. The Tome must use tabs/pages.
2. The Tome must have at least:
   - Learned Spells
   - Prepared Spells
3. Learned spells must be shown from `SpellRegistry` and sorted by:
   - school
   - tier
   - name
4. Only learned/default-learned enabled spells should be shown.
5. Clicking a learned spell name must show spell details.
6. Clicking Prepare must store the spell code into a prepared spell slot.
7. Casting must use the currently selected prepared slot, not the last prepared spell.
8. The Tome background must be visible and non-transparent.
9. Invalid/missing prepared spell codes must not crash the Tome.
10. Prepared slots must remain 9.
11. The active prepared slot should be saved if the existing save system supports it.

## Prepare flow rules
1. If a prepared slot is selected, Prepare must use that slot.
2. If no prepared slot is selected, Prepare must use the first empty slot.
3. The same spell cannot be prepared in multiple slots.
4. If all slots are full and no slot is selected, show `No empty spell slots.`
5. Preparing a spell must not cast it.
6. Prepared spells must be stored by registry code, not display name.

## Cast flow rules
1. Cast must resolve the active prepared slot only.
2. An empty active slot must fail cleanly with a clear message.
3. The last spell clicked in Learned Spells must not determine what casts.
4. The server must remain authoritative for cast validation and gameplay execution.

## UI requirements
### Learned Spells tab
- Show only learned/default-learned enabled spells.
- Display spell names in a scrollable list.
- Each row should have:
  - spell name
  - Prepare button
- Selecting a spell name should update a details panel.
- The details panel should show:
  - name
  - description
  - school
  - tier
  - corruption cost
  - cast time
  - cooldown
  - target type
  - effect summary if easy

### Prepared Spells tab
- Show all 9 prepared slots.
- Each slot should show:
  - slot number
  - prepared spell name or `Empty`
  - selected/active state
  - Clear button
- Clicking a slot should select it and make it active.
- Clearing the active slot should keep the slot selected but empty if possible.

## Logging requirements
Add clear logs for:
- Tome opened
- learned spell count exposed to the Tome
- prepare request received
- target slot chosen
- spell stored in slot
- duplicate prepare rejected
- slot cleared
- active slot changed
- cast attempted from active slot

## Validation requirements
After the repair:
1. The Tome has a visible background.
2. The Tome has tabs/pages.
3. Learned Spells lists all enabled learned/default-learned spells.
4. Clicking a spell name shows full details.
5. Clicking Prepare places the spell into the selected prepared slot if one is selected.
6. If no slot is selected, Prepare places it into the first empty slot.
7. Preparing the same spell twice is rejected.
8. Prepared Spells shows 9 slots.
9. Clicking a prepared slot makes it active.
10. Clear clears that slot.
11. Casting uses the active prepared slot.
12. Empty active slot cannot cast.
13. Invalid saved spell codes do not crash the Tome.
14. Prepared slots persist if the existing prepared-spell persistence supports it.

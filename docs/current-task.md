# Current Task: Rustweaver discovery items and loot generation

## Objective
Add lootable Rustweave discovery items that can teach random spells from configured school/tier pools.

## Scope
This pass covers discovery-item backend, item assets, loot generation hooks, and localization.

Do not add:
- mentors
- rituals
- crafted scrolls
- practice unlocks
- spell executor rewrites
- unrelated UI systems

## Required behavior
1. Add the discovery items:
   - Forgotten Book
   - Arcane Notes
   - Rustplane Prism
   - Ancient Codex
   - Scroll from the Rust
2. Each item must have matching model, texture, itemtype, and localization assets.
3. Discovery items must unlock random eligible spells from configured pools.
4. Discovery items must not unlock Loreweave spells.
5. Discovery items must not unlock hidden, disabled, invalid, or already learned spells unless fallback research behavior applies.
6. Forgotten Book and Rustplane Prism must be single-use per player and not consumed.
7. Arcane Notes, Ancient Codex, and Scroll from the Rust must be consumed on successful use.
8. Loot hooks should add discovery items to ruins, chests, bookshelves, traders, and dungeon loot where feasible.

## Validation
- No missing texture or model errors for the new items.
- All five discovery items appear in creative inventory with correct names.
- Items teach eligible spells and update the Tome learned tab.
- Invalid or empty pools fail cleanly.
- Loot patches apply without crashing.

# Current Task: Rust Tablet passive decay creative inventory crash fix

## Objective
Fix the server-side `NullReferenceException` thrown by passive Rust Tablet decay scanning creative inventories during join/startup and server ticks.

## Hard scope boundary
This task is only for passive Rust Tablet decay inventory scanning and startup timing.

This task must not add:
- new gameplay systems
- corruption or HUD redesigns
- spellcasting changes
- new recipes
- particles, shaders, or Harmony patches
- unrelated refactors

## Required behavior
1. Passive Rust Tablet decay must no longer throw when scanning player inventories.
2. Creative inventories must be skipped entirely.
3. Null inventories must be skipped safely.
4. Inventories whose `ClassName` or `InventoryID` indicates creative inventory must be skipped.
5. A bad inventory must not crash the server tick.
6. Join/startup tablet decay scanning must be deferred until inventories are ready.
7. Login-time decay, if needed, must run after a short delay instead of immediately on `PlayerJoin`.
8. Normal Rust Tablet decay must still work for valid survival inventories and valid containers.

## Audit targets
Review the full passive tablet decay flow for:
- `OnServerPlayerJoin`
- `OnServerPlayerNowPlaying`
- `ProcessPassiveTabletDecay`
- `ProcessTabletDecayForInventories`
- `ProcessTabletDecayForInventory`

For each path, verify:
- creative inventories are excluded
- null inventories are skipped
- `inventory.Count` access is defensive
- one bad inventory cannot crash the tick loop
- join/startup decay is deferred until the player inventory is ready

## Implementation rules
- Keep the changes minimal and robust.
- Prefer explicit skip checks over broad catch-all handling.
- Use a one-time log when an inventory is skipped, but do not spam every tick.
- Preserve normal decay behavior for valid inventories and containers.

## Validation requirements
After the repair:
1. Creative worlds no longer crash.
2. Joining a creative-building save no longer throws `InventoryPlayerCreative.get_Count()`.
3. Passive tablet decay still works for actual Rust Tablets in normal player inventory and container slots.
4. The server tick cannot be killed by one bad inventory.

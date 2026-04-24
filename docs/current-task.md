# Current Task: Rust Tablet decay pass

## Objective
Implement passive Rust Tablet decay over world time, staged tablet state labels, and inert reuse behavior.

## Hard scope boundary
This task is only for first-pass Rust Tablet decay.

This task must not add:
- attraction logic
- entity luring
- entity consumption
- damage or health buffs
- a recipe
- a tablet vault block implementation
- visual or icon changes
- a new GUI panel
- spell system changes

## Required design rules
1. Rust Tablet remains one item with per-item stored corruption state.
2. Decay rate is 4 corruption per in-game day.
3. Decay continues while carried.
4. Valid implemented contexts for this task:
   - player inventory
   - vertical rack
   - world-placed tablet
5. Future tablet vault support may be left as a clean extension point if practical, but do not implement the block/container now.
6. Offline time counts. Use world time timestamps so decay progresses sensibly even while unloaded.
7. No visual difference between stages in first pass. Tooltip only.
8. Inert condition: stored corruption reaches 0.
9. Inert tablets can be used again for venting without needing conversion to a separate item.

## Stage labels
Use tooltip-only stage labels based on current stored corruption:
- inert: 0
- faded: 1 to 99
- stabilized: 100 to 400

## Data rules
- never let stored corruption go below 0
- preserve per-tablet corruption through moves, relogs, saves, and context changes
- update decay on access, load, or tick rather than requiring constant active ticking everywhere
- do not duplicate truth sources

## Notes
- The Rust Tablet already uses per-item stack attributes for stored corruption
- Keep AGENTS.md unchanged
- If direct rack integration is awkward, implement the cleanest compatible lookup possible and document the approach in code comments or the implementation summary

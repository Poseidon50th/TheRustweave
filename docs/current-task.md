# Current Task

Implement and verify the four non-testing Warping initiates now present in the Tome data:
`Warped Reach`, `Unseen Offset`, `Arranged Step`, and `Wardwarp I`.

## What changed

- The four Warping initiates are enabled, non-hidden, non-testing spells and remain discoverable through the existing Forgotten Book and mentor rules.
- `Warped Reach` is currently a timed Rustweave range buff; safe vanilla reach hooks are engine-dependent.
- `Unseen Offset` clears hostile creature aggro in range and is canceled by later Rustweave spell casts.
- `Arranged Step` is motion-based rather than a teleport.
- `Wardwarp I` is a block-bound rift suppression ward with a six-second cleanup scan fallback when no direct temporal-rift spawn hook is available.

## Files expected to change

- `TheRustweave/RustweaveScaffold.cs`
- `TheRustweave/RustweaveSpells.cs`
- `TheRustweave/RustweaveUi.cs` only if a state sync path touches UI readiness
- `docs/current-task.md`
- `docs/spell-framework.md`

## Required behavior

- The four Warping initiates must stay visible as normal spells, not testing-only entries.
- `Warped Reach` must apply the Rustweave targeting-range bonus while active.
- `Unseen Offset` must continue to skip players and only affect hostile creatures.
- `Arranged Step` must remain collision-authoritative and not behave like a teleport.
- `Wardwarp I` must persist as a block-bound ward and clean up natural rifts without log spam.
- Existing worlds with saved Rustweave state must load safely.
- Bad or old Rustweave player state must be repaired or reset, not applied raw.
- Rustweave must not write unsafe vanilla watched attributes during early join.
- Prepared slots remain SlotIds `1..9`.
- Client HUD/Tome state must not be touched before the client player entity is ready.

## Logging

- Log world-state load and sanitization once per world load.
- Log repaired player state with the player UID and world identifier.
- Log when a server state sync is queued and when it is actually applied.
- Log when the client defers and later applies Rustweave state hydration.

## Validation checklist

- Fresh worlds load without crash.
- Existing worlds with saved Rustweave state load without crash.
- Corrupt/old prepared-slot state is repaired or reset safely.
- Client own-player-data receive no longer crashes in `HudMouseTools`.
- The four Warping initiates appear in Tome listings after discovery/mentor unlock.
- `Warped Reach` extends Rustweave spell range while active.
- `Wardwarp I` suppresses natural rifts and respects the player/chunk caps already enforced by the runtime.
- Prepared slots, corruption, and learned spells still persist in valid worlds.
- Build succeeds cleanly.

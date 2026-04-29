# Current Task

Redo the runtime behavior for Still Thread and Stable Sense.

## What changed

- Still Thread now resolves `self` directly from the caster before any raycast or look-target logic.
- Still Thread no longer fails with a generic “no valid target” message when cast from prepared slot 1 or any other slot.
- Stable Sense now reports exact in-game hours until the next Temporal Storm when timing data is available.
- Stable Sense now uses Light / Medium / Heavy intensity bands derived from forecast timing instead of relying on unavailable vanilla storm-strength data.
- Foundational Fabric is unchanged in this pass.

## Files expected to change

- `TheRustweave/RustweaveSpells.cs`
- `TheRustweave/RustweaveScaffold.cs`
- `TheRustweave/assets/therustweave/config/spells.json`
- `TheRustweave/assets/therustweave/lang/en.json`
- `docs/current-task.md`
- `docs/spell-framework.md`

## Required behavior

- `still-thread` must use `targetType: self`, `range: 0`, `radius: 0`, and `requiresLineOfSight: false`.
- `freezeTemporalStabilityLoss` must accept a self/caster target.
- Self-target resolution must happen before raycast or look-target validation.
- Stable Sense must cost 0 corruption and still go on cooldown.
- Stable Sense must report forecast hours using the best available runtime timing data.
- Stable Sense must not depend on unavailable vanilla storm-strength access.

## Logging

- Still Thread should log the resolved self target and clear failure reasons when the caster is unavailable.
- Stable Sense should log whether forecast timing was available, the hour count, the intensity band, and the source used.
- Do not add noisy per-tick or per-cast reflection warnings.

## Validation checklist

- Still Thread casts successfully while looking at nothing.
- Still Thread casts successfully from prepared slot 1.
- Still Thread applies its temporal stability effect to the caster.
- Stable Sense reports an exact in-game hour value when forecast timing is available.
- Stable Sense falls back cleanly when timing cannot be read.
- Stable Sense displays intensity as Light, Medium, or Heavy.
- Build succeeds cleanly.

# Current Task

Fix three Warping spell runtime issues:

- Still Thread self-target resolution
- Stable Sense exact Temporal Storm timing
- Foundational Fabric deferred-cost repayment

## What changed

- Still Thread now resolves `self` through the caster directly and no longer depends on look-target lookup.
- Stable Sense now probes the runtime more aggressively for Temporal Storm timing and reports an exact in-game hour value when available.
- Foundational Fabric debt repayment is logged and repaid through the world-scoped player state path, including offline expiry handling and death cleanup.

## Files expected to change

- `TheRustweave/RustweaveSpells.cs`
- `TheRustweave/RustweaveScaffold.cs`
- `TheRustweave/assets/therustweave/lang/en.json`
- `docs/current-task.md`
- `docs/spell-framework.md`

## Required behavior

- `still-thread` must always target the caster when `targetType` is `self`.
- `freezeTemporalStabilityLoss` must not be rejected by target resolution.
- Stable Sense must report exact in-game hours to the next Temporal Storm when the runtime exposes that data.
- Stable Sense must fall back cleanly when storm data is unavailable.
- Foundational Fabric must reduce later base corruption costs by 10%, round deferred debt up, and repay accumulated debt on expiry.
- Foundational Fabric debt must persist through logout/unload and clear on death.
- Foundational Fabric must not stack with itself.

## Logging

- Still Thread self-target resolution logs the caster name.
- Stable Sense logs whether storm data was available, the hour count, and the severity label.
- Foundational Fabric logs each discount, repayment, and death cleanup.

## Validation checklist

- Still Thread casts successfully from every prepared slot, including slot 1.
- Stable Sense costs 0 corruption and still completes its cooldown.
- Stable Sense sends either the exact timing message or the unavailable fallback.
- Foundational Fabric adds deferred debt during discounted casts.
- Foundational Fabric repays deferred debt on expiry or next join after offline expiry.
- Foundational Fabric clears on death and does not repay afterward.
- Build succeeds cleanly.

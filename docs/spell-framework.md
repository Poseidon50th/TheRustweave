# Rustweave Spell Framework

This document is the persistent reference for Rustweave spell vocabulary, validation, and runtime behavior.

## Canonical schools

- `Loreweave`
- `Tabby`
- `Warping`
- `Wefting`
- `Shedding`
- `Picking`
- `Beating`
- `Twining`
- `Darning`
- `Backstitching`
- `Hemming`
- `Carding`
- `Scouring`
- `Fulling`
- `Spinning`
- `Grafting`
- `Scutching`
- `Tensioning`

## School alias

- `Twining` is the canonical spelling.
- `Twinning` remains a backwards-compatible alias and normalizes to `Twining` while loading spell JSON.

## Target types

| Target type | Behavior |
| --- | --- |
| `self` | Caster self-target. |
| `heldItem` | Caster active held item. |
| `inventory` | Caster inventory. |
| `lookEntity` | Raycast/lock any valid entity. |
| `lookPlayer` | Raycast/lock only another player. |
| `lookNonPlayerEntity` | Raycast/lock only a non-player living entity. |
| `lookDroppedItem` | Raycast/lock only a dropped item entity. |
| `lookBlock` | Raycast/lock a block position. |
| `lookBlockEntity` | Raycast/lock a block position only if a block entity exists. |
| `lookContainer` | Raycast/lock a block position only if the block entity exposes a container/inventory. |
| `lookPosition` | Raycast/lock a position or hit point. |
| `selfArea` | Area centered on the caster. Requires radius > 0. |
| `lookArea` | Area centered on the looked-at block/position. Requires radius > 0. |

## Target rules

- Look targets require positive range.
- Area targets require positive radius.
- `lookBlockEntity` and `lookContainer` must fail cleanly if the block entity is missing or not container-capable.
- `lookDroppedItem` must use safe entity detection and must not crash on unknown entity types.
- Harmful player-target effects still obey PvP validation.
- Non-player entity targets must not be blocked by player-only PvP rules.

## Effect rules

- Any effect added to `SpellEffectTypes.Supported` must have validation and a real runtime result.
- No silent no-ops: unsupported future effects must fail clearly during validation.
- Block-changing effects must respect land claims/protection.
- Loreweave or lore-flavored spells should only gain mechanical behavior when the spell JSON explicitly assigns it and the current task authorizes that behavior; do not rely on a blanket cosmetic-only allowlist in framework work.

## Runtime systems

- Persistent active area registry
- Temporal history tracker
- Active spell-effect registry
- Summon tracking and expiry
- Safe teleport helpers

## Active area effects

Active area records are persisted in mod config and ticked on the server. They support:

- wards
- barriers
- containment areas
- boundary lines
- anti-spread areas
- stabilization/corruption modifiers
- temperature/environment pressure fields
- rift markers

They are queryable by purge/cancel/counter/identify effects and expire automatically.

## Temporal history

- Recent entity positions are sampled on the server at a bounded cadence.
- The history window is limited so rewind effects can safely restore a recent position without log spam or unbounded memory growth.

## Supported effect types

### Core

| Effect type | Behavior |
| --- | --- |
| `none` | No gameplay effect. |
| `healSelf` | Direct self-healing. |
| `healTarget` | Direct targeted healing. |
| `healArea` | Area healing around the resolved target point. |
| `repairHeldItem` | Repairs the held item. |
| `repairInventoryItem` | Repairs an inventory item. |
| `damageRayEntity` | Direct entity damage. |
| `damageArea` | Area damage. |
| `shieldSelf` | Applies a defensive shield to the caster. |
| `shieldTarget` | Applies a defensive shield to a target entity. |
| `slowTarget` | Applies timed movement slow. |
| `rootTarget` | Applies stronger movement lock. |
| `speedBuff` | Applies a timed speed buff. |
| `damageOverTime` | Applies timed damage ticks. |
| `stunTarget` | Applies a short stun/interrupt. |
| `knockbackEntity` | Pushes an entity away. |
| `pullEntity` | Pulls an entity toward the effect center. |
| `weakenTarget` | Applies timed weakening. |
| `projectileEntity` | Emits a projectile-like spell effect. |
| `corruptionTransfer` | Transfers corruption. |
| `teleportForward` | Teleports the caster to a safe forward destination. |
| `spawnParticles` | Cosmetic particle emission. |
| `playSound` | Cosmetic sound emission. |

### Warping

| Effect type | Behavior |
| --- | --- |
| `modifyTemporalStability` | Timed stability/corruption modifier. |
| `stabilizeArea` | Timed stabilization field. |
| `anchorEntity` | Prevents displacement on an entity. |
| `anchorBlock` | Marks a block as anchored for Rustweave interactions. |
| `preventDisplacement` | Blocks teleport/push/pull/swap displacement. |
| `modifyCorruptionGain` | Timed corruption gain modifier. |

### Wefting

| Effect type | Behavior |
| --- | --- |
| `teleportToTarget` | Teleports the caster to the resolved safe target position. |
| `teleportEntityToCaster` | Teleports a target entity toward the caster. |
| `teleportEntityToPosition` | Teleports a target entity to a resolved position. |
| `swapPositions` | Swaps the caster and target entity positions. |
| `moveDroppedItem` | Moves a dropped item to the caster inventory when possible. |
| `moveBlockEntityContents` | Moves container contents to the caster inventory. |
| `pushEntity` | Pushes an entity away from the caster/effect center. |

### Shedding

| Effect type | Behavior |
| --- | --- |
| `releaseTarget` | Removes Rustweave bindings/restraints from the target. |
| `breakBinding` | Stronger binding removal. |
| `openLock` | Attempts to unlock a lockable container/block entity safely. |
| `openPassage` | Temporarily opens a passable passage with snapshots for restoration. |
| `createRift` | Creates a temporary rift/visual marker field. |
| `separateTarget` | Breaks links and separates bound targets. |

### Picking

| Effect type | Behavior |
| --- | --- |
| `markTarget` | Applies a timed mark for follow-up effects. |
| `weakPointStrike` | Precise targeted damage. |
| `precisionBlockStrike` | Precise block strike. |

### Beating

| Effect type | Behavior |
| --- | --- |
| `forcePulse` | Short-range force burst. |
| `shockwave` | Radial force plus stagger/damage. |
| `pressureBlast` | Heavier directional or radial force attack. |
| `staggerArea` | Brief area stagger/interrupt. |

### Twining

| Effect type | Behavior |
| --- | --- |
| `tetherEntity` | Tethers an entity to the caster or area center. |
| `linkEntities` | Links entities for later transfer/mirroring. |
| `bindEntityToArea` | Prevents an entity from leaving an area. |
| `charmEntity` | Creature-only pacify/follow behavior. |
| `commandEntity` | Creature-only simple command behavior. |

### Darning

| Effect type | Behavior |
| --- | --- |
| `repairBlock` | Repairs or restores a targeted block. |
| `repairBlockArea` | Repairs multiple blocks in an area. |
| `closeRift` | Closes or weakens an active rift effect. |
| `restoreStructure` | Restores stored passage/block snapshots when available. |
| `healOverTime` | Applies timed healing. |

### Backstitching

| Effect type | Behavior |
| --- | --- |
| `counterNextHostileEffect` | Counters the next hostile effect on the target. |
| `reflectProjectile` | Reflects or neutralizes the next projectile-like effect. |
| `rewindEntityPosition` | Restores an entity to a recent sampled position. |
| `undoRecentEffect` | Cancels or reverses a recent compatible Rustweave effect. |
| `cancelActiveEffect` | Cancels a currently active Rustweave effect. |

### Hemming

| Effect type | Behavior |
| --- | --- |
| `createWardArea` | Creates a protective ward area. |
| `createBarrier` | Creates a blocking barrier or barrier field. |
| `createContainmentArea` | Creates an entity containment area. |
| `createBoundaryLine` | Creates a line-shaped boundary field. |
| `createAntiSpreadArea` | Prevents spread-like Rustweave effects. |

### Carding

| Effect type | Behavior |
| --- | --- |
| `detectBlocks` | Reveals matching blocks using particles. |
| `detectEntities` | Reveals matching entities using particles. |
| `detectRustTraces` | Reveals active Rustweave traces/effects using particles. |
| `identifyActiveEffects` | Reveals active Rustweave effects. |
| `readGlyphs` | Reveals active glyph-like markers. |
| `alignNextSpell` | Buffs the next Rustweave spell. |

### Scouring

| Effect type | Behavior |
| --- | --- |
| `purgeTimedEffects` | Removes timed negative Rustweave effects. |
| `stripEntityBuffs` | Removes timed positive Rustweave buffs. |
| `cleanseContamination` | Reduces Rustweave contamination/corruption. |
| `unravelDamage` | Reverses or mitigates recent Rustweave damage where possible. |
| `lifestealEntity` | Damages the target and heals the caster. |
| `destroyCorruptedMatter` | Removes or weakens corrupted matter when safe. |

### Fulling

| Effect type | Behavior |
| --- | --- |
| `convertBlock` | Converts a block into a configured result block. |
| `convertHeldItem` | Converts the held item into a configured result item. |
| `hardenMaterial` | Hardens or fortifies a material. |
| `heatMaterial` | Heats or warms a material. |
| `coolMaterial` | Cools or chills a material. |
| `accelerateCraftState` | Advances a craft/block-entity process when possible. |

### Spinning

| Effect type | Behavior |
| --- | --- |
| `summonTemporaryEntity` | Spawns a temporary entity with expiry. |
| `summonTemporaryItem` | Spawns or grants a temporary item. |
| `summonTemporaryProjectile` | Spawns a temporary projectile-like effect. |
| `summonTemporaryConstruct` | Spawns a temporary construct/block effect. |

### Grafting

| Effect type | Behavior |
| --- | --- |
| `modifyCropGrowth` | Adjusts crop growth. |
| `modifyFarmlandNutrients` | Adjusts farmland nutrients. |
| `modifySatiety` | Adjusts player satiety/hunger. |
| `createTemporaryFood` | Creates temporary food. |
| `modifyAnimalFertility` | Adjusts animal fertility/breeding readiness. |
| `vitalityOverTime` | Applies a living vitality modifier over time. |

### Scutching

| Effect type | Behavior |
| --- | --- |
| `mineBlock` | Mines a single block with normal drops. |
| `excavateBlocks` | Mines multiple blocks with normal drops. |
| `extractOre` | Extracts ore-like blocks with normal drops. |
| `harvestBlocks` | Harvests crop/plant blocks with normal drops. |

### Tensioning

| Effect type | Behavior |
| --- | --- |
| `changeWeather` | Changes local/world weather when available, otherwise uses safe fallback simulation. |
| `changeTemperatureArea` | Applies a temperature area field. |
| `callLightning` | Calls real lightning if available, otherwise a safe fallback. |
| `changeEnvironmentalPressure` | Applies environmental pressure. |
| `stormPulse` | Creates a storm-like pulse field. |

## Target/effect compatibility rules

- Entity-only effects use `lookEntity`, `lookPlayer`, `lookNonPlayerEntity`, or `self` where explicitly allowed.
- Creature-only control effects (`charmEntity`, `commandEntity`) never affect players.
- Dropped-item movement uses `lookDroppedItem`.
- Container effects use `lookContainer`.
- Block effects use `lookBlock`, `lookBlockEntity`, `lookContainer`, `lookArea`, or `selfArea` where appropriate.
- Area effects use `selfArea`, `lookArea`, `lookBlock`, or `lookPosition` when a radius is provided.
- Teleport effects always search for a nearest safe standing position and stay in the same dimension.

## Preview rules

- Entity/NPC preview, player warning, block preview, area preview, location preview, self preview, and projectile/line preview must remain visually distinct.
- Preview visuals stay world-space only.
- Tooltip/HUD target indicators remain removed.

## Validation and runtime notes

- Recognized effect names are accepted by normalization.
- Supported effect names have validation and runtime behavior.
- Effects that need persistence use the active area registry.
- Effects that need recent movement use the temporal history tracker.
- Detection/read effects are particle-based rather than chat-based.
- Farming/grafting uses real Vintage Story behavior where available and safe fallbacks otherwise.

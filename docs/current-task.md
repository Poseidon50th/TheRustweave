# Current Task: Rustweaver class and traits only

## Objective
Implement the **Rustweaver** as a new Vintage Story character class for TheRustweave.

This task is intentionally narrow.

## Hard scope boundary
This task must **only** add:
- the Rustweaver class definition
- the Rustweaver starting gear
- the Rustweaver custom traits
- the required English localization entries for the class, traits, and trait attribute text

This task must **not** add:
- spellbooks
- spell systems
- HUD elements
- corruption mechanics
- casting logic
- custom GUI
- networking
- save data
- Harmony patches
- extra starter items beyond those listed below

## Files to add or update
### Create
- `TheRustweave/assets/therustweave/config/characterclasses.json`
- `TheRustweave/assets/therustweave/config/traits.json`

### Update
- `TheRustweave/assets/therustweave/lang/en.json`

## Rustweaver class data
Add one enabled class with code:
- `rustweaver`

Use these traits on the class:
- `temporal-speed`
- `rustwoven-armor`
- `planar-satiety`
- `rustplane-sapped`

Use exactly this starting gear list:
1. `game:clothes-foot-forgotten`
2. `game:clothes-hand-forgotten`
3. `game:clothes-lowerbody-forgotten`
4. `game:clothes-upperbody-forgotten`
5. `game:clothes-upperbodyover-forgotten`
6. `game:clothes-neck-gear-amulet-rusty`

Represent each as standard Vintage Story gear entries:
```json
{ "type": "item", "code": "..." }
```

## Localization entries
Add these exact class localization entries to `en.json`:
- `game:characterclass-rustweaver`: `Rustweaver`
- `game:characterdesc-rustweaver`: `<font color="#99c9f9"><i>The world has been corrupted long before you were here by people like you. You feel the strands of reality, of time, ebbing and flowing in unnatural ways, ripe for manipulation by your hand. It is power incarnate, a tool that could be used for the greater good or greater evil. The world looks upon you as an anomaly to blame for its instability yet only you and people like you can correct it...if you choose to.\n</i></font><font color="#c69c29"><br>You are a Rustweaver. Manipulate the Rust how you wish though remember, there is always a cost.</i></font><br><br>`

## Trait definitions
Create the following custom traits in `traits.json`.

### 1) Temporal Speed
- code: `temporal-speed`
- type: `positive`
- attributes:
  - `walkspeed`: `0.10`
  - `armorWalkSpeedAffectedness`: `-0.20`

### 2) Rustwoven Armor
- code: `rustwoven-armor`
- type: `positive`
- attributes:
  - `armorDurabilityLoss`: `-0.35`

### 3) Planar Satiety
- code: `planar-satiety`
- type: `positive`
- attributes:
  - `hungerrate`: `-0.25`

### 4) Rustplane Sapped
- code: `rustplane-sapped`
- type: `negative`
- attributes:
  - `bowDrawingStrength`: `-0.20`
  - `miningSpeedMul`: `-0.10`
  - `meleeWeaponsDamage`: `-0.10`
  - `rangedWeaponsDamage`: `-0.10`

## Trait localization
Add trait display entries in `en.json` for all four traits:
- `game:trait-temporal-speed`
- `game:traitname-temporal-speed`
- `game:trait-rustwoven-armor`
- `game:traitname-rustwoven-armor`
- `game:trait-planar-satiety`
- `game:traitname-planar-satiety`
- `game:trait-rustplane-sapped`
- `game:traitname-rustplane-sapped`

Use sensible display text that matches the requested trait names.

For `game:trait-*` entries, format them like standard trait bullet lines, with positive traits in green and the negative trait in red.

## Required character attribute lang keys
Add any missing English lang keys needed so the class screen shows readable text for all non-vanilla attribute/value pairs used by these traits.

This task should include keys for:
- `walkspeed` with value `0.1`
- `armorWalkSpeedAffectedness` with value `-0.2`
- `armorDurabilityLoss` with value `-0.35`
- `hungerrate` with value `-0.25`
- `bowDrawingStrength` with value `-0.2`
- `miningSpeedMul` with value `-0.1`
- `meleeWeaponsDamage` with value `-0.1`
- `rangedWeaponsDamage` with value `-0.1`

Use user-friendly English descriptions for each attribute modifier.

## Important notes
- Keep the existing `hello` lang entry unless there is a strong reason to remove it.
- Do not modify `modinfo.json` for this task, since it already targets game version 1.22.0.
- Do not change `TheRustweave.csproj` target framework unless it no longer targets `net10.0`.
- Do not add any C# for this task unless absolutely required; this should be implemented as content assets plus localization.

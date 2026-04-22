# Current Task: Rustweaver's Tome local texture paths

## Objective
Fix the Rustweaver's Tome asset setup so it uses local mod texture files that can be added by drag and drop.

## Hard scope boundary
This task must only change the Rustweaver's Tome texture pipeline and folder structure.

This task must not add:
- gameplay behavior
- spell systems
- GUIs
- book reading systems
- HUD work
- networking
- recipes
- Harmony patches

## Files to inspect and update as needed
- `TheRustweave/assets/therustweave/itemtypes/rustweaverstome.json`
- `TheRustweave/assets/therustweave/shapes/item/rustweaverstome.json`
- `TheRustweave/assets/therustweave/lang/en.json`

## Files/folders to create
- `TheRustweave/assets/therustweave/textures/item/tome/`
- `TheRustweave/assets/therustweave/textures/item/tome/.gitkeep`

## Required texture paths
The tome should resolve these local mod textures:
- `therustweave:textures/item/tome/brown-front.png`
- `therustweave:textures/item/tome/pages-aged.png`

## Requirements
- Keep the Rustweaver's Tome item code stable
- Keep the current creative inventory setup intact
- Keep custom Rustweave tag data intact
- Preserve the current closed-book shape concept
- Use the local texture paths in the shape/item setup, not vanilla book paths

## Validation goal
After the change, I should be able to place these files here:
- `TheRustweave/assets/therustweave/textures/item/tome/brown-front.png`
- `TheRustweave/assets/therustweave/textures/item/tome/pages-aged.png`

and the item should render without missing-texture warnings for those paths.

## Notes
- Prefer pure JSON/content changes
- Do not modify `modinfo.json` or `TheRustweave.csproj`
- Do not add C# unless absolutely necessary

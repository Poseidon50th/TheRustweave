# Current Task

Implement the next spell system iteration for TheRustweave:
- Remove spell effect aliases
- Add support for `healTarget`, `healArea`, `shieldSelf`, `shieldTarget`, `stunTarget`, `projectileEntity`, `lookBlock`, `lookPosition`, and `corruptionTransfer`
- Keep existing spells loading by migrating current JSON spell effects to the new names
- Preserve Tome lifecycle rules and avoid unsafe `SingleComposer` reassignment

Focus only on spell effect/target validation and runtime execution.

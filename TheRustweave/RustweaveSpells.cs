using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace TheRustweave
{
    internal static class SpellTargetTypes
    {
        public const string Self = "self";
        public const string HeldItem = "heldItem";
        public const string LookEntity = "lookEntity";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Self,
            HeldItem,
            LookEntity
        };
    }

    internal static class SpellEffectTypes
    {
        public const string None = "none";
        public const string RepairHeldItem = "repairHeldItem";
        public const string AddHealth = "addHealth";
        public const string DamageEntity = "damageEntity";
        public const string TeleportForward = "teleportForward";
        public const string VentCorruption = "ventCorruption";
        public const string SpawnParticles = "spawnParticles";
        public const string PlaySound = "playSound";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            None,
            RepairHeldItem,
            AddHealth,
            DamageEntity,
            TeleportForward,
            VentCorruption,
            SpawnParticles,
            PlaySound
        };
    }

    internal sealed class SpellEffectDefinition
    {
        public string Type { get; set; } = string.Empty;

        public int DurabilityAmount { get; set; }

        public bool AllowBrokenItems { get; set; }

        public float HealthAmount { get; set; }

        public float DamageAmount { get; set; }

        public float TeleportDistance { get; set; }

        public int CorruptionAmount { get; set; }

        public string ParticleCode { get; set; } = string.Empty;

        public int Count { get; set; }

        public string Sound { get; set; } = string.Empty;
    }

    internal sealed class SpellDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public string School { get; set; } = string.Empty;

        public int Tier { get; set; }

        public double CastTimeSeconds { get; set; }

        public double CooldownSeconds { get; set; }

        public int CorruptionCost { get; set; }

        public string TargetType { get; set; } = string.Empty;

        public double Range { get; set; }

        public double Radius { get; set; }

        public bool RequiresLineOfSight { get; set; }

        public bool RequiresTome { get; set; } = true;

        public bool RequiresHeldFocus { get; set; }

        public List<string> RequiredGlyphs { get; set; } = new();

        public List<string> AllowedGlyphs { get; set; } = new();

        public int GlyphSlots { get; set; }

        public List<SpellEffectDefinition> Effects { get; set; } = new();

        public string Icon { get; set; } = string.Empty;
    }

    internal sealed class SpellExecutionPlan
    {
        public int CorruptionDelta { get; set; }

        public List<Func<bool>> Actions { get; } = new();
    }

    internal sealed class SpellRegistry
    {
        private readonly Dictionary<string, SpellDefinition> byCode;

        public SpellRegistry(IReadOnlyList<SpellDefinition> spells)
        {
            Spells = spells;
            byCode = spells
                .Where(spell => !string.IsNullOrWhiteSpace(spell.Code))
                .ToDictionary(spell => spell.Code, StringComparer.OrdinalIgnoreCase);
            StarterSpellCode = spells.FirstOrDefault()?.Code ?? RustweaveConstants.DummySpellCode;
        }

        public IReadOnlyList<SpellDefinition> Spells { get; }

        public IReadOnlyDictionary<string, SpellDefinition> ByCode => byCode;

        public string StarterSpellCode { get; }

        public static SpellRegistry CreateFallback()
        {
            return new SpellRegistry(new[]
            {
                new SpellDefinition
                {
                    Code = RustweaveConstants.DummySpellCode,
                    Name = "Dummy Rustcall",
                    Description = "A harmless placeholder spell used for scaffold testing.",
                    Enabled = true,
                    School = "scaffold",
                    Tier = 1,
                    CastTimeSeconds = 2.5,
                    CooldownSeconds = 0,
                    CorruptionCost = 25,
                    TargetType = SpellTargetTypes.Self,
                    Range = 0,
                    Radius = 0,
                    RequiresLineOfSight = false,
                    RequiresTome = true,
                    RequiresHeldFocus = false,
                    RequiredGlyphs = new List<string>(),
                    AllowedGlyphs = new List<string>(),
                    GlyphSlots = 0,
                    Effects = new List<SpellEffectDefinition>
                    {
                        new()
                        {
                            Type = SpellEffectTypes.None
                        }
                    },
                    Icon = string.Empty
                }
            });
        }

        public static SpellRegistry Load(ICoreAPI api)
        {
            try
            {
                var asset = api.Assets.TryGet(new AssetLocation(RustweaveConstants.SpellRegistryAsset));
                var loaded = asset?.ToObject<List<SpellDefinition>>() ?? new List<SpellDefinition>();
                var validated = Validate(api, loaded);

                if (validated.Count == 0)
                {
                    api.Logger.Warning("TheRustweave spell registry contained no valid enabled spells. Falling back to the built-in dummy spell.");
                    return CreateFallback();
                }

                api.Logger.Notification("TheRustweave loaded {0} enabled spell definition(s): {1}", validated.Count, string.Join(", ", validated.Select(spell => spell.Code)));
                return new SpellRegistry(validated);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("TheRustweave failed to load spell registry, falling back to the built-in dummy spell: {0}", exception.Message);
                return CreateFallback();
            }
        }

        public bool TryGetSpell(string code, out SpellDefinition? spell)
        {
            spell = null;

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return byCode.TryGetValue(code, out spell);
        }

        public bool TryGetEnabledSpell(string code, out SpellDefinition? spell)
        {
            return TryGetSpell(code, out spell);
        }

        public IReadOnlyList<SpellDefinition> GetAllSpells()
        {
            return Spells;
        }

        public IReadOnlyList<SpellDefinition> GetEnabledSpells()
        {
            return Spells;
        }

        private static List<SpellDefinition> Validate(ICoreAPI api, IReadOnlyList<SpellDefinition> loaded)
        {
            var validated = new List<SpellDefinition>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < loaded.Count; index++)
            {
                var raw = loaded[index];
                if (raw == null)
                {
                    api.Logger.Warning("TheRustweave spell registry entry #{0} is null and was ignored.", index);
                    continue;
                }

                if (!TryNormalizeSpell(raw, out var normalized, out var validationError))
                {
                    api.Logger.Warning("TheRustweave spell registry entry #{0} was ignored: {1}", index, validationError);
                    continue;
                }

                if (!seenCodes.Add(normalized.Code))
                {
                    api.Logger.Warning("TheRustweave spell registry code '{0}' was duplicated and the duplicate was ignored.", normalized.Code);
                    continue;
                }

                validated.Add(normalized);
            }

            return validated;
        }

        private static bool TryNormalizeSpell(SpellDefinition raw, out SpellDefinition normalized, out string validationError)
        {
            normalized = raw;
            validationError = string.Empty;

            normalized.Code = (normalized.Code ?? string.Empty).Trim();
            normalized.Name = (normalized.Name ?? string.Empty).Trim();
            normalized.Description = (normalized.Description ?? string.Empty).Trim();
            normalized.School = (normalized.School ?? string.Empty).Trim();
            normalized.TargetType = (normalized.TargetType ?? string.Empty).Trim();
            normalized.Icon = (normalized.Icon ?? string.Empty).Trim();
            normalized.RequiredGlyphs = NormalizeStringList(normalized.RequiredGlyphs);
            normalized.AllowedGlyphs = NormalizeStringList(normalized.AllowedGlyphs);
            normalized.Effects = normalized.Effects?.Where(effect => effect != null).ToList() ?? new List<SpellEffectDefinition>();

            if (string.IsNullOrWhiteSpace(normalized.Code))
            {
                validationError = "missing a code";
                return false;
            }

            if (!normalized.Enabled)
            {
                validationError = "the spell is disabled";
                return false;
            }

            if (normalized.CastTimeSeconds < 0)
            {
                validationError = "castTimeSeconds cannot be negative";
                return false;
            }

            if (normalized.CooldownSeconds < 0)
            {
                validationError = "cooldownSeconds cannot be negative";
                return false;
            }

            if (normalized.CorruptionCost < 0)
            {
                validationError = "corruptionCost cannot be negative";
                return false;
            }

            if (normalized.Range < 0)
            {
                validationError = "range cannot be negative";
                return false;
            }

            if (normalized.Radius < 0)
            {
                validationError = "radius cannot be negative";
                return false;
            }

            if (normalized.GlyphSlots < 0)
            {
                validationError = "glyphSlots cannot be negative";
                return false;
            }

            if (!SpellTargetTypes.Supported.Contains(normalized.TargetType))
            {
                validationError = $"unknown target type '{normalized.TargetType}'";
                return false;
            }

            if (normalized.Effects.Count == 0)
            {
                validationError = "no effects were defined";
                return false;
            }

            for (var effectIndex = 0; effectIndex < normalized.Effects.Count; effectIndex++)
            {
                var effect = normalized.Effects[effectIndex];
                if (!TryNormalizeEffect(effect, normalized.TargetType, out var effectError))
                {
                    validationError = $"effect #{effectIndex} was ignored: {effectError}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryNormalizeEffect(SpellEffectDefinition effect, string targetType, out string validationError)
        {
            validationError = string.Empty;
            effect.Type = (effect.Type ?? string.Empty).Trim();
            effect.ParticleCode = (effect.ParticleCode ?? string.Empty).Trim();
            effect.Sound = (effect.Sound ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(effect.Type))
            {
                validationError = "missing effect type";
                return false;
            }

            var normalizedEffectType = NormalizeEffectType(effect.Type);
            if (normalizedEffectType == null)
            {
                validationError = $"unknown effect type '{effect.Type}'";
                return false;
            }

            effect.Type = normalizedEffectType;

            switch (effect.Type)
            {
                case SpellEffectTypes.None:
                    return true;
                case SpellEffectTypes.RepairHeldItem:
                    if (!string.Equals(targetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "repairHeldItem requires targetType heldItem";
                        return false;
                    }

                    if (effect.DurabilityAmount <= 0)
                    {
                        validationError = "durabilityAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.AddHealth:
                    if (!string.Equals(targetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "addHealth requires targetType self";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DamageEntity:
                    if (!string.Equals(targetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "damageEntity requires targetType lookEntity";
                        return false;
                    }

                    if (effect.DamageAmount <= 0)
                    {
                        validationError = "damageAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.TeleportForward:
                    if (!string.Equals(targetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "teleportForward requires targetType self";
                        return false;
                    }

                    if (effect.TeleportDistance <= 0)
                    {
                        validationError = "teleportDistance must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.VentCorruption:
                    if (!string.Equals(targetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "ventCorruption requires targetType self";
                        return false;
                    }

                    if (effect.CorruptionAmount <= 0)
                    {
                        validationError = "corruptionAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.SpawnParticles:
                    if (string.IsNullOrWhiteSpace(effect.ParticleCode))
                    {
                        validationError = "particleCode is required for spawnParticles";
                        return false;
                    }

                    if (effect.Count <= 0)
                    {
                        validationError = "count must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.PlaySound:
                    if (string.IsNullOrWhiteSpace(effect.Sound))
                    {
                        validationError = "sound is required for playSound";
                        return false;
                    }

                    return true;
                default:
                    validationError = $"unsupported effect type '{effect.Type}'";
                    return false;
            }
        }

        private static List<string> NormalizeStringList(IReadOnlyCollection<string>? values)
        {
            if (values == null || values.Count == 0)
            {
                return new List<string>();
            }

            return values
                .Select(value => (value ?? string.Empty).Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();
        }

        private static string? NormalizeEffectType(string? effectType)
        {
            if (string.IsNullOrWhiteSpace(effectType))
            {
                return null;
            }

            var trimmed = effectType.Trim();
            return SpellEffectTypes.Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal sealed class SpellEffectExecutor
    {
        private readonly ICoreServerAPI sapi;

        public SpellEffectExecutor(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public bool TryBuildPlan(IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, out SpellExecutionPlan? plan, out string failureReason)
        {
            plan = null;
            failureReason = string.Empty;

            if (caster?.Entity == null || !caster.Entity.Alive)
            {
                failureReason = "caster is not available";
                return false;
            }

            if (state == null)
            {
                failureReason = "player state is missing";
                return false;
            }

            if (spell == null)
            {
                failureReason = "spell is missing";
                return false;
            }

            if (spell.RequiresTome && !RustweaveStateService.IsHoldingTome(caster))
            {
                failureReason = "the caster is not holding the Tome";
                return false;
            }

            if (!TryResolveTarget(caster, spell, out var target, out failureReason))
            {
                return false;
            }

            var gameplayActions = new List<Func<bool>>();
            var ventActions = new List<Func<bool>>();
            var visualActions = new List<Func<bool>>();
            var executionPlan = new SpellExecutionPlan();
            var corruptionDelta = 0;

            foreach (var effect in spell.Effects)
            {
                if (!TryAppendEffect(gameplayActions, ventActions, visualActions, caster, state, spell, target, effect, ref corruptionDelta, out failureReason))
                {
                    return false;
                }
            }

            executionPlan.Actions.AddRange(gameplayActions);
            executionPlan.Actions.AddRange(ventActions);
            executionPlan.Actions.AddRange(visualActions);
            executionPlan.CorruptionDelta = corruptionDelta;
            plan = executionPlan;
            return true;
        }

        public bool TryExecutePlan(IServerPlayer caster, RustweavePlayerStateData state, SpellExecutionPlan plan, out string failureReason)
        {
            failureReason = string.Empty;

            if (plan == null)
            {
                failureReason = "spell execution plan is missing";
                return false;
            }

            try
            {
                foreach (var action in plan.Actions)
                {
                    if (!action())
                    {
                        failureReason = "one spell effect failed while executing";
                        sapi.Logger.Warning("[TheRustweave] Spell execution failed for player '{0}' because one effect returned false.", caster?.PlayerUID ?? string.Empty);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                failureReason = exception.Message;
                sapi.Logger.Warning("[TheRustweave] Failed to execute spell plan for player '{0}': {1}", caster?.PlayerUID ?? string.Empty, exception);
                return false;
            }
        }

        private bool TryResolveTarget(IServerPlayer caster, SpellDefinition spell, out SpellTargetContext target, out string failureReason)
        {
            target = new SpellTargetContext();
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            target.Position = entity.Pos.XYZ;

            switch (spell.TargetType)
            {
                case SpellTargetTypes.Self:
                    target.Entity = entity;
                    return true;
                case SpellTargetTypes.HeldItem:
                    target.ItemSlot = caster.InventoryManager?.ActiveHotbarSlot;
                    if (target.ItemSlot?.Itemstack == null)
                    {
                        failureReason = "held item target is unavailable";
                        return false;
                    }

                    target.Entity = entity;
                    return true;
                case SpellTargetTypes.LookEntity:
                    var selection = caster.CurrentEntitySelection?.Entity;
                    if (selection == null)
                    {
                        failureReason = "no entity is currently targeted";
                        return false;
                    }

                    if (spell.Range > 0 && entity.Pos.SquareDistanceTo(selection.Pos) > spell.Range * spell.Range)
                    {
                        failureReason = "the targeted entity is out of range";
                        return false;
                    }

                    target.Entity = selection;
                    target.Position = selection.Pos.XYZ;
                    return true;
                default:
                    failureReason = $"unknown target type '{spell.TargetType}'";
                    sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed because its target type '{1}' is unsupported at runtime.", spell.Code, spell.TargetType);
                    return false;
            }
        }

        private bool TryAppendEffect(List<Func<bool>> gameplayActions, List<Func<bool>> ventActions, List<Func<bool>> visualActions, IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, ref int corruptionDelta, out string failureReason)
        {
            failureReason = string.Empty;

            var normalizedEffectType = NormalizeEffectType(effect.Type);
            if (normalizedEffectType == null)
            {
                failureReason = $"unsupported effect type '{effect.Type}'";
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed because effect type '{1}' is unsupported at runtime.", spell.Code, effect.Type);
                return false;
            }

            effect.Type = normalizedEffectType;

            switch (effect.Type)
            {
                case SpellEffectTypes.None:
                    return true;
                case SpellEffectTypes.RepairHeldItem:
                    return TryAppendRepairHeldItem(gameplayActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.AddHealth:
                    return TryAppendAddHealth(gameplayActions, caster, effect, out failureReason);
                case SpellEffectTypes.DamageEntity:
                    return TryAppendDamageEntity(gameplayActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.TeleportForward:
                    return TryAppendTeleportForward(gameplayActions, caster, effect, out failureReason);
                case SpellEffectTypes.VentCorruption:
                    return TryAppendVentCorruption(ventActions, state, effect, ref corruptionDelta, out failureReason);
                case SpellEffectTypes.SpawnParticles:
                    return TryAppendSpawnParticles(visualActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.PlaySound:
                    return TryAppendPlaySound(visualActions, caster, target, effect, out failureReason);
                default:
                    failureReason = $"unsupported effect type '{effect.Type}'";
                    sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed because effect type '{1}' is unsupported at runtime.", spell.Code, effect.Type);
                    return false;
            }
        }

        private bool TryAppendRepairHeldItem(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var slot = target.ItemSlot;
            var stack = slot?.Itemstack;

            if (slot == null || stack == null)
            {
                failureReason = "no held item is available";
                sapi.Logger.Warning("[TheRustweave] Repair effect failed because the held item was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var collectible = stack.Collectible;
            var maxDurability = collectible.GetMaxDurability(stack);
            if (maxDurability <= 0)
            {
                failureReason = "held item is not damageable";
                sapi.Logger.Warning("[TheRustweave] Repair effect failed because the held item is not damageable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var remainingDurability = collectible.GetRemainingDurability(stack);
            if (remainingDurability <= 0 && !effect.AllowBrokenItems)
            {
                failureReason = "broken items are not allowed by this spell";
                sapi.Logger.Warning("[TheRustweave] Repair effect failed because the held item is broken and allowBrokenItems is false for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var repairAmount = Math.Min(effect.DurabilityAmount, maxDurability - remainingDurability);
            if (repairAmount <= 0)
            {
                failureReason = "the held item is already fully repaired";
                sapi.Logger.Warning("[TheRustweave] Repair effect failed because the held item is already fully repaired for player '{0}'.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                collectible.DamageItem(sapi.World, caster.Entity, slot, -repairAmount, false);
                slot.MarkDirty();
                return true;
            });

            return true;
        }

        private bool TryAppendAddHealth(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var health = caster.Entity?.GetBehavior<EntityBehaviorHealth>();
            if (health == null)
            {
                failureReason = "caster does not have a health behavior";
                sapi.Logger.Warning("[TheRustweave] addHealth failed because player '{0}' has no health behavior.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                health.Health = Math.Min(health.MaxHealth, health.Health + effect.HealthAmount);
                return true;
            });

            return true;
        }

        private bool TryAppendDamageEntity(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            dynamic targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] damageEntity failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var damageSource = new DamageSource
            {
                Source = EnumDamageSource.Player,
                SourceEntity = caster.Entity,
                CauseEntity = caster.Entity,
                KnockbackStrength = 0f
            };

            if (!targetEntity.ShouldReceiveDamage(damageSource, effect.DamageAmount))
            {
                failureReason = "the targeted entity cannot receive that damage";
                sapi.Logger.Warning("[TheRustweave] damageEntity failed because the target entity refused damage for player '{0}'.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                return targetEntity.ReceiveDamage(damageSource, effect.DamageAmount);
            });

            return true;
        }

        private bool TryAppendTeleportForward(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            if (!TryFindTeleportDestination(caster, effect.TeleportDistance, out var destination))
            {
                failureReason = "no safe teleport destination could be found";
                sapi.Logger.Warning("[TheRustweave] teleportForward failed because no safe destination was found for player '{0}'.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                caster.Entity?.TeleportTo(destination);
                return caster.Entity != null;
            });

            return true;
        }

        private bool TryAppendVentCorruption(List<Func<bool>> ventActions, RustweavePlayerStateData state, SpellEffectDefinition effect, ref int corruptionDelta, out string failureReason)
        {
            failureReason = string.Empty;
            corruptionDelta -= effect.CorruptionAmount;

            ventActions.Add(() =>
            {
                state.CurrentTemporalCorruption = Math.Max(0, state.CurrentTemporalCorruption - effect.CorruptionAmount);
                return true;
            });

            return true;
        }

        private bool TryAppendSpawnParticles(List<Func<bool>> visualActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var center = target.Position;
            var color = GetParticleColor(effect.ParticleCode);
            var minPos = new Vec3d(center.X - 0.12, center.Y - 0.12, center.Z - 0.12);
            var maxPos = new Vec3d(center.X + 0.12, center.Y + 0.12, center.Z + 0.12);
            var minVelocity = new Vec3f(-0.02f, 0.02f, -0.02f);
            var maxVelocity = new Vec3f(0.02f, 0.08f, 0.02f);

            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(effect.Count, color, minPos, maxPos, minVelocity, maxVelocity, 0.6f, 0f, 1f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendPlaySound(List<Func<bool>> visualActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var soundLocation = new AssetLocation(effect.Sound);
            if (!soundLocation.Valid)
            {
                failureReason = $"invalid sound location '{effect.Sound}'";
                sapi.Logger.Warning("[TheRustweave] playSound failed because '{0}' is not a valid asset location for player '{1}'.", effect.Sound, caster.PlayerUID);
                return false;
            }

            var center = target.Position;
            visualActions.Add(() =>
            {
                sapi.World.PlaySoundAt(soundLocation, center.X, center.Y, center.Z, caster, true, 32f, 1f);
                return true;
            });

            return true;
        }

        private bool TryFindTeleportDestination(IServerPlayer caster, float distance, out Vec3d destination)
        {
            destination = caster.Entity?.Pos.XYZ ?? new Vec3d();
            var entity = caster.Entity;
            if (entity == null || entity.World == null)
            {
                return false;
            }

            var viewVector = entity.Pos.GetViewVector();
            var basePosition = entity.Pos.XYZ;
            var desired = new Vec3d(
                basePosition.X + (viewVector.X * distance),
                basePosition.Y + (viewVector.Y * distance),
                basePosition.Z + (viewVector.Z * distance));

            var searchStartY = Math.Floor(desired.Y);
            for (var offset = 0; offset <= 3; offset++)
            {
                var candidate = new Vec3d(desired.X, searchStartY + offset, desired.Z);
                if (IsTeleportPositionSafe(entity.World, entity, candidate))
                {
                    destination = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool IsTeleportPositionSafe(IWorldAccessor world, object entity, Vec3d candidate)
        {
            dynamic typedEntity = entity;
            var dim = typedEntity.Pos.Dimension;
            var feetPos = new BlockPos((int)Math.Floor(candidate.X), (int)Math.Floor(candidate.Y), (int)Math.Floor(candidate.Z), dim);
            var headHeight = 1.7;
            var headPos = new BlockPos((int)Math.Floor(candidate.X), (int)Math.Floor(candidate.Y + headHeight), (int)Math.Floor(candidate.Z), dim);

            return IsSpaceClear(world, feetPos) && IsSpaceClear(world, headPos);
        }

        private static bool IsSpaceClear(IWorldAccessor world, BlockPos pos)
        {
            var block = world.BlockAccessor.GetBlock(pos);
            return block == null || block.CollisionBoxes == null || block.CollisionBoxes.Length == 0;
        }

        private static int GetParticleColor(string particleCode)
        {
            if (particleCode.IndexOf("rust", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return unchecked((int)0xFFC58A4A);
            }

            return unchecked((int)0xFFD6D0B0);
        }

        private static string? NormalizeEffectType(string? effectType)
        {
            if (string.IsNullOrWhiteSpace(effectType))
            {
                return null;
            }

            var trimmed = effectType.Trim();
            return SpellEffectTypes.Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class SpellTargetContext
        {
            public object? Entity { get; set; }

            public ItemSlot? ItemSlot { get; set; }

            public Vec3d Position { get; set; } = new Vec3d();
        }
    }
}

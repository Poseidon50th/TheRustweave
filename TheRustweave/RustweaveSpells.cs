using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace TheRustweave
{
    internal static class SpellSchoolTypes
    {
        public const string Loreweave = "Loreweave";
        public const string Tabby = "Tabby";
        public const string Warping = "Warping";
        public const string Wefting = "Wefting";
        public const string Shedding = "Shedding";
        public const string Picking = "Picking";
        public const string Beating = "Beating";
        public const string Twinning = "Twinning";
        public const string Darning = "Darning";
        public const string Backstitching = "Backstitching";
        public const string Hemming = "Hemming";
        public const string Carding = "Carding";
        public const string Scouring = "Scouring";
        public const string Fulling = "Fulling";
        public const string Spinning = "Spinning";
        public const string Grafting = "Grafting";
        public const string Scutching = "Scutching";
        public const string Tensioning = "Tensioning";

        public static readonly HashSet<string> Recognized = new(StringComparer.OrdinalIgnoreCase)
        {
            Loreweave,
            Tabby,
            Warping,
            Wefting,
            Shedding,
            Picking,
            Beating,
            Twinning,
            Darning,
            Backstitching,
            Hemming,
            Carding,
            Scouring,
            Fulling,
            Spinning,
            Grafting,
            Scutching,
            Tensioning
        };
    }

    internal static class SpellTargetTypes
    {
        public const string Self = "self";
        public const string HeldItem = "heldItem";
        public const string Inventory = "inventory";
        public const string LookEntity = "lookEntity";
        public const string LookBlock = "lookBlock";
        public const string LookPosition = "lookPosition";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Self,
            HeldItem,
            Inventory,
            LookEntity,
            LookBlock,
            LookPosition
        };
    }

    internal static class SpellPreviewModes
    {
        public const string None = "none";
        public const string Self = "self";
        public const string Entity = "entity";
        public const string Block = "block";
        public const string Position = "position";
        public const string Area = "area";
        public const string Projectile = "projectile";
        public const string Line = "line";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            None,
            Self,
            Entity,
            Block,
            Position,
            Area,
            Projectile,
            Line
        };

        public static string Normalize(string? previewMode)
        {
            if (string.IsNullOrWhiteSpace(previewMode))
            {
                return string.Empty;
            }

            var trimmed = previewMode.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }
    }

    internal static class SpellPreviewColorClasses
    {
        public const string Harmful = "harmful";
        public const string Beneficial = "beneficial";
        public const string Neutral = "neutral";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Harmful,
            Beneficial,
            Neutral
        };

        public static string Normalize(string? colorClass)
        {
            if (string.IsNullOrWhiteSpace(colorClass))
            {
                return string.Empty;
            }

            var trimmed = colorClass.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }
    }

    internal static class SpellPreviewMarkerStyles
    {
        public const string Ring = "ring";
        public const string Halo = "halo";
        public const string Column = "column";
        public const string Arrow = "arrow";
        public const string Trail = "trail";
        public const string Strip = "strip";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Ring,
            Halo,
            Column,
            Arrow,
            Trail,
            Strip
        };

        public static string Normalize(string? markerStyle)
        {
            if (string.IsNullOrWhiteSpace(markerStyle))
            {
                return string.Empty;
            }

            var trimmed = markerStyle.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }
    }

    internal static class SpellEffectTypes
    {
        public const string None = "none";
        public const string HealTarget = "healTarget";
        public const string HealArea = "healArea";
        public const string HealSelf = "healSelf";
        public const string RepairHeldItem = "repairHeldItem";
        public const string RepairInventoryItem = "repairInventoryItem";
        public const string DamageRayEntity = "damageRayEntity";
        public const string DamageArea = "damageArea";
        public const string ShieldSelf = "shieldSelf";
        public const string ShieldTarget = "shieldTarget";
        public const string SlowTarget = "slowTarget";
        public const string RootTarget = "rootTarget";
        public const string SpeedBuff = "speedBuff";
        public const string DamageOverTime = "damageOverTime";
        public const string StunTarget = "stunTarget";
        public const string KnockbackEntity = "knockbackEntity";
        public const string PullEntity = "pullEntity";
        public const string WeakenTarget = "weakenTarget";
        public const string ProjectileEntity = "projectileEntity";
        public const string CorruptionTransfer = "corruptionTransfer";
        public const string TeleportForward = "teleportForward";
        public const string SpawnParticles = "spawnParticles";
        public const string PlaySound = "playSound";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            None,
            HealTarget,
            HealArea,
            HealSelf,
            RepairHeldItem,
            RepairInventoryItem,
            DamageRayEntity,
            DamageArea,
            ShieldSelf,
            ShieldTarget,
            SlowTarget,
            RootTarget,
            SpeedBuff,
            DamageOverTime,
            StunTarget,
            KnockbackEntity,
            PullEntity,
            WeakenTarget,
            ProjectileEntity,
            CorruptionTransfer,
            TeleportForward,
            SpawnParticles,
            PlaySound
        };

        public static string? Normalize(string? effectType)
        {
            if (string.IsNullOrWhiteSpace(effectType))
            {
                return null;
            }

            var trimmed = effectType.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal sealed class SpellEffectDefinition
    {
        public string Type { get; set; } = string.Empty;

        public double Range { get; set; }

        public double Radius { get; set; }

        public bool IncludeCaster { get; set; }

        public bool AllowNoTargets { get; set; }

        public int MaxTargets { get; set; }

        public int DurabilityAmount { get; set; }

        public bool AllowBrokenItems { get; set; }

        public float HealthAmount { get; set; }

        public float DamageAmount { get; set; }

        public float TeleportDistance { get; set; }

        public int CorruptionAmount { get; set; }

        public double DurationSeconds { get; set; }

        public float SpeedMultiplier { get; set; }

        public float DamageMultiplier { get; set; }

        public float IncomingDamageMultiplier { get; set; }

        public float Force { get; set; }

        public float DamagePerTick { get; set; }

        public double TickIntervalSeconds { get; set; }

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

        public bool Hidden { get; set; }

        public string School { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

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

        public bool IsLoreSpell { get; set; }

        public string UnlockHint { get; set; } = string.Empty;

        public string PreviewMode { get; set; } = string.Empty;

        public string PreviewColorClass { get; set; } = string.Empty;

        public double PreviewRadius { get; set; }

        public double PreviewWidth { get; set; }

        public double PreviewLength { get; set; }

        public bool PreviewUsesGravity { get; set; }

        public bool PreviewShowImpactPoint { get; set; } = true;

        public string PreviewMarkerStyle { get; set; } = string.Empty;

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

    internal sealed class SpellPreviewInfo
    {
        public string Mode { get; set; } = SpellPreviewModes.None;

        public string ColorClass { get; set; } = SpellPreviewColorClasses.Neutral;

        public double Radius { get; set; }

        public double Width { get; set; }

        public double Length { get; set; }

        public bool UsesGravity { get; set; }

        public bool ShowImpactPoint { get; set; } = true;

        public string MarkerStyle { get; set; } = SpellPreviewMarkerStyles.Ring;
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
                    Hidden = true,
                    School = "scaffold",
                    Category = string.Empty,
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
                    IsLoreSpell = false,
                    UnlockHint = string.Empty,
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
            normalized.Category = (normalized.Category ?? string.Empty).Trim();
            normalized.TargetType = (normalized.TargetType ?? string.Empty).Trim();
            normalized.Icon = (normalized.Icon ?? string.Empty).Trim();
            normalized.UnlockHint = (normalized.UnlockHint ?? string.Empty).Trim();
            normalized.PreviewMode = SpellPreviewModes.Normalize(normalized.PreviewMode);
            normalized.PreviewColorClass = SpellPreviewColorClasses.Normalize(normalized.PreviewColorClass);
            normalized.PreviewMarkerStyle = SpellPreviewMarkerStyles.Normalize(normalized.PreviewMarkerStyle);
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

            if (normalized.Tier < 1 || normalized.Tier > 6)
            {
                validationError = "tier must be between 1 and 6";
                return false;
            }

            if (!SpellTargetTypes.Supported.Contains(normalized.TargetType))
            {
                validationError = $"unknown target type '{normalized.TargetType}'";
                return false;
            }

            if ((string.Equals(normalized.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
                && GetEffectiveLookTargetRange(normalized) <= 0)
            {
                validationError = "range must be greater than zero for look targets";
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
                if (!TryNormalizeEffect(effect, normalized, out var effectError))
                {
                    validationError = $"effect #{effectIndex} was ignored: {effectError}";
                    return false;
                }
            }

            if (string.Equals(normalized.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
            {
                var effectiveRange = GetEffectiveLookEntityRange(normalized);
                if (effectiveRange <= 0)
                {
                    validationError = "range must be greater than zero for lookEntity or a damageRayEntity effect must define range";
                    return false;
                }
            }

            return true;
        }

        private static bool TryNormalizeEffect(SpellEffectDefinition effect, SpellDefinition spell, out string validationError)
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

            var normalizedEffectType = SpellEffectTypes.Normalize(effect.Type);
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
                case SpellEffectTypes.HealTarget:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "healTarget requires targetType self or lookEntity";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.HealArea:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "healArea requires targetType self, lookEntity, lookBlock, or lookPosition";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    if (GetEffectiveRadius(spell, effect) <= 0)
                    {
                        validationError = "radius must be greater than zero for healArea";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.HealSelf:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "healSelf requires targetType self";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.RepairHeldItem:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
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
                case SpellEffectTypes.RepairInventoryItem:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "repairInventoryItem requires targetType inventory";
                        return false;
                    }

                    if (effect.DurabilityAmount <= 0)
                    {
                        validationError = "durabilityAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DamageRayEntity:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "damageRayEntity requires targetType lookEntity";
                        return false;
                    }

                    if (effect.DamageAmount <= 0)
                    {
                        validationError = "damageAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DamageArea:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "damageArea requires targetType self, lookEntity, lookBlock, or lookPosition";
                        return false;
                    }

                    if (effect.DamageAmount <= 0)
                    {
                        validationError = "damageAmount must be greater than zero";
                        return false;
                    }

                    if (GetEffectiveRadius(spell, effect) <= 0)
                    {
                        validationError = "radius must be greater than zero for damageArea";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ShieldSelf:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "shieldSelf requires targetType self";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ShieldTarget:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "shieldTarget requires targetType self or lookEntity";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.SlowTarget:
                    if (string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "slowTarget requires an entity target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.SpeedMultiplier <= 0)
                    {
                        validationError = "speedMultiplier must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.RootTarget:
                    if (string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "rootTarget requires an entity target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.SpeedMultiplier <= 0)
                    {
                        effect.SpeedMultiplier = 0.05f;
                    }

                    return true;
                case SpellEffectTypes.SpeedBuff:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "speedBuff requires targetType self";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.SpeedMultiplier <= 0)
                    {
                        validationError = "speedMultiplier must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DamageOverTime:
                    if (string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "damageOverTime requires an entity target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.TickIntervalSeconds <= 0)
                    {
                        validationError = "tickIntervalSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.DamagePerTick <= 0)
                    {
                        validationError = "damagePerTick must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.StunTarget:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "stunTarget requires targetType lookEntity";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.KnockbackEntity:
                case SpellEffectTypes.PullEntity:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = $"{effect.Type} requires targetType lookEntity";
                        return false;
                    }

                    if (effect.Force <= 0)
                    {
                        validationError = "force must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ProjectileEntity:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "projectileEntity requires targetType lookEntity, lookBlock, or lookPosition";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.WeakenTarget:
                    if (string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "weakenTarget requires an entity target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.DamageMultiplier <= 0 && effect.IncomingDamageMultiplier <= 0)
                    {
                        validationError = "damageMultiplier or incomingDamageMultiplier must be greater than zero";
                        return false;
                    }

                    if (effect.DamageMultiplier <= 0)
                    {
                        effect.DamageMultiplier = effect.IncomingDamageMultiplier;
                    }

                    return true;
                case SpellEffectTypes.TeleportForward:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
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
                case SpellEffectTypes.CorruptionTransfer:
                    if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "corruptionTransfer requires targetType self or lookEntity";
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

        internal static double GetEffectiveLookEntityRange(SpellDefinition spell)
        {
            return GetEffectiveLookTargetRange(spell);
        }

        internal static double GetEffectiveLookTargetRange(SpellDefinition spell)
        {
            if (spell == null)
            {
                return 0d;
            }

            var effectRange = spell.Effects
                .Where(effect => effect != null)
                .Select(effect => Math.Max(0d, effect.Range))
                .DefaultIfEmpty(0d)
                .Max();

            return Math.Max(0d, Math.Max(spell.Range, effectRange));
        }

        internal static double GetEffectiveRadius(SpellDefinition spell, SpellEffectDefinition effect)
        {
            if (spell == null || effect == null)
            {
                return 0d;
            }

            return Math.Max(0d, effect.Radius > 0d ? effect.Radius : spell.Radius);
        }

        internal static SpellPreviewInfo GetPreviewInfo(SpellDefinition spell)
        {
            var preview = new SpellPreviewInfo();
            if (spell == null)
            {
                return preview;
            }

            preview.Mode = ResolvePreviewMode(spell);
            preview.ColorClass = ResolvePreviewColorClass(spell);
            preview.MarkerStyle = ResolvePreviewMarkerStyle(spell, preview.Mode);
            preview.Radius = GetEffectivePreviewRadius(spell);
            preview.Width = GetEffectivePreviewWidth(spell);
            preview.Length = GetEffectivePreviewLength(spell);
            preview.UsesGravity = ResolvePreviewUsesGravity(spell);
            preview.ShowImpactPoint = ResolvePreviewShowImpactPoint(spell, preview.Mode);
            return preview;
        }

        internal static string ResolvePreviewMode(SpellDefinition spell)
        {
            if (spell == null)
            {
                return SpellPreviewModes.None;
            }

            var explicitMode = SpellPreviewModes.Normalize(spell.PreviewMode);
            if (!string.IsNullOrWhiteSpace(explicitMode))
            {
                return explicitMode;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase))
            {
                return SpellPreviewModes.Self;
            }

            var hasProjectile = spell.Effects.Any(effect => effect != null && string.Equals(effect.Type, SpellEffectTypes.ProjectileEntity, StringComparison.OrdinalIgnoreCase));
            var hasArea = GetEffectivePreviewRadius(spell) > 0d
                || spell.Effects.Any(effect => effect != null && (string.Equals(effect.Type, SpellEffectTypes.DamageArea, StringComparison.OrdinalIgnoreCase) || string.Equals(effect.Type, SpellEffectTypes.HealArea, StringComparison.OrdinalIgnoreCase)));

            if (hasProjectile)
            {
                return SpellPreviewModes.Projectile;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
            {
                return hasArea ? SpellPreviewModes.Area : SpellPreviewModes.Entity;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase))
            {
                return hasArea ? SpellPreviewModes.Area : SpellPreviewModes.Block;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
            {
                if (hasArea)
                {
                    return SpellPreviewModes.Area;
                }

                if (spell.PreviewLength > 0d || spell.PreviewWidth > 0d || !string.IsNullOrWhiteSpace(spell.PreviewMarkerStyle))
                {
                    return SpellPreviewModes.Line;
                }

                return SpellPreviewModes.Position;
            }

            return SpellPreviewModes.None;
        }

        internal static string ResolvePreviewColorClass(SpellDefinition spell)
        {
            if (spell == null)
            {
                return SpellPreviewColorClasses.Neutral;
            }

            var explicitColorClass = SpellPreviewColorClasses.Normalize(spell.PreviewColorClass);
            if (!string.IsNullOrWhiteSpace(explicitColorClass))
            {
                return explicitColorClass;
            }

            if (spell.Effects.Any(effect => effect != null && IsHarmfulEffectType(effect.Type)))
            {
                return SpellPreviewColorClasses.Harmful;
            }

            if (spell.Effects.Any(effect => effect != null && IsBeneficialEffectType(effect.Type)))
            {
                return SpellPreviewColorClasses.Beneficial;
            }

            return SpellPreviewColorClasses.Neutral;
        }

        internal static string ResolvePreviewMarkerStyle(SpellDefinition spell, string previewMode)
        {
            if (spell == null)
            {
                return SpellPreviewMarkerStyles.Ring;
            }

            var explicitMarkerStyle = SpellPreviewMarkerStyles.Normalize(spell.PreviewMarkerStyle);
            if (!string.IsNullOrWhiteSpace(explicitMarkerStyle))
            {
                return explicitMarkerStyle;
            }

            return previewMode switch
            {
                SpellPreviewModes.Entity => SpellPreviewMarkerStyles.Halo,
                SpellPreviewModes.Block => SpellPreviewMarkerStyles.Column,
                SpellPreviewModes.Area => SpellPreviewMarkerStyles.Ring,
                SpellPreviewModes.Projectile => SpellPreviewMarkerStyles.Trail,
                SpellPreviewModes.Line => SpellPreviewMarkerStyles.Strip,
                SpellPreviewModes.Position => SpellPreviewMarkerStyles.Ring,
                _ => SpellPreviewMarkerStyles.Ring
            };
        }

        internal static double GetEffectivePreviewRadius(SpellDefinition spell)
        {
            if (spell == null)
            {
                return 0d;
            }

            var effectRadius = spell.Effects
                .Where(effect => effect != null)
                .Select(effect => Math.Max(0d, effect.Radius))
                .DefaultIfEmpty(0d)
                .Max();

            return Math.Max(0d, Math.Max(spell.PreviewRadius, Math.Max(spell.Radius, effectRadius)));
        }

        internal static double GetEffectivePreviewWidth(SpellDefinition spell)
        {
            return spell == null ? 0d : Math.Max(0d, spell.PreviewWidth);
        }

        internal static double GetEffectivePreviewLength(SpellDefinition spell)
        {
            if (spell == null)
            {
                return 0d;
            }

            var effectRange = spell.Effects
                .Where(effect => effect != null)
                .Select(effect => Math.Max(0d, effect.Range))
                .DefaultIfEmpty(0d)
                .Max();

            return Math.Max(0d, Math.Max(spell.PreviewLength, Math.Max(spell.Range, effectRange)));
        }

        internal static bool ResolvePreviewUsesGravity(SpellDefinition spell)
        {
            return spell != null && spell.PreviewUsesGravity;
        }

        internal static bool ResolvePreviewShowImpactPoint(SpellDefinition spell, string previewMode)
        {
            if (spell == null)
            {
                return true;
            }

            if (spell.PreviewShowImpactPoint)
            {
                return true;
            }

            return string.Equals(previewMode, SpellPreviewModes.Projectile, StringComparison.OrdinalIgnoreCase)
                || string.Equals(previewMode, SpellPreviewModes.Line, StringComparison.OrdinalIgnoreCase)
                || string.Equals(previewMode, SpellPreviewModes.Block, StringComparison.OrdinalIgnoreCase)
                || string.Equals(previewMode, SpellPreviewModes.Area, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHarmfulEffectType(string? effectType)
        {
            return string.Equals(effectType, SpellEffectTypes.DamageRayEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.DamageArea, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.DamageOverTime, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.KnockbackEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.PullEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.WeakenTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.StunTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.ProjectileEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.CorruptionTransfer, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBeneficialEffectType(string? effectType)
        {
            return string.Equals(effectType, SpellEffectTypes.HealTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.HealArea, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.HealSelf, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.ShieldSelf, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.ShieldTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.SpeedBuff, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.RepairHeldItem, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.RepairInventoryItem, StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeEffectType(string? effectType)
        {
            return SpellEffectTypes.Normalize(effectType);
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
            return TryBuildPlan(caster, state, spell, null, out plan, out failureReason);
        }

        public bool TryBuildPlan(IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext? lockedTarget, out SpellExecutionPlan? plan, out string failureReason)
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

            if (lockedTarget == null)
            {
                if (!TryResolveTarget(caster, spell, out lockedTarget, out failureReason))
                {
                    return false;
                }
            }

            var gameplayActions = new List<Func<bool>>();
            var ventActions = new List<Func<bool>>();
            var visualActions = new List<Func<bool>>();
            var executionPlan = new SpellExecutionPlan();
            var corruptionDelta = 0;

            foreach (var effect in spell.Effects)
            {
                if (!TryAppendEffect(gameplayActions, ventActions, visualActions, caster, state, spell, lockedTarget, effect, ref corruptionDelta, out failureReason))
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

        internal bool TryResolveTarget(IServerPlayer caster, SpellDefinition spell, out SpellTargetContext target, out string failureReason)
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
                    target.TargetName = entity.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.HeldItem:
                    target.ItemSlot = caster.InventoryManager?.ActiveHotbarSlot;
                    if (target.ItemSlot?.Itemstack == null)
                    {
                        failureReason = "held item target is unavailable";
                        return false;
                    }

                    target.Entity = entity;
                    target.TargetName = entity.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.Inventory:
                    var repairEffect = spell.Effects.FirstOrDefault(effect => string.Equals(effect.Type, SpellEffectTypes.RepairInventoryItem, StringComparison.OrdinalIgnoreCase));
                    if (repairEffect == null)
                    {
                        failureReason = "No damaged item to mend.";
                        return false;
                    }

                    if (!TryFindBestInventoryRepairTarget(caster, spell, repairEffect, out var repairSlot, out failureReason))
                    {
                        return false;
                    }

                    target.ItemSlot = repairSlot;
                    target.Entity = entity;
                    target.TargetName = repairSlot.Itemstack?.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.LookEntity:
                    if (!TryResolveLookEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var selection, out failureReason))
                    {
                        return false;
                    }

                    target.Entity = selection.Entity;
                    target.Position = selection.HitPosition;
                    target.TargetName = selection.Entity?.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.LookBlock:
                    if (!TryResolveLookBlockTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var blockPos, out var blockPosition, out failureReason))
                    {
                        return false;
                    }

                    target.Position = blockPosition;
                    target.BlockPos = blockPos ?? new BlockPos((int)Math.Floor(blockPosition.X), (int)Math.Floor(blockPosition.Y), (int)Math.Floor(blockPosition.Z), entity.Pos.Dimension);
                    target.TargetName = GetBlockDisplayName(target.BlockPos);
                    return true;
                case SpellTargetTypes.LookPosition:
                    if (!TryResolveLookPositionTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var lookPosition, out failureReason))
                    {
                        return false;
                    }

                    target.Position = lookPosition;
                    target.TargetName = spell.Code;
                    return true;
                default:
                    failureReason = $"unknown target type '{spell.TargetType}'";
                    sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed because its target type '{1}' is unsupported at runtime.", spell.Code, spell.TargetType);
                    return false;
            }
        }

        internal bool TryResolveLockedTarget(IServerPlayer caster, SpellDefinition spell, RustweaveCastStateData castState, out SpellTargetContext target, out string failureReason)
        {
            target = new SpellTargetContext();
            failureReason = string.Empty;

            if (caster?.Entity == null || !caster.Entity.Alive)
            {
                failureReason = "caster is not available";
                return false;
            }

            if (spell == null)
            {
                failureReason = "spell is missing";
                return false;
            }

            if (castState == null)
            {
                failureReason = "locked target is missing";
                return false;
            }

            target.Position = caster.Entity.Pos.XYZ;

            switch (spell.TargetType)
            {
                case SpellTargetTypes.Self:
                    target.Entity = caster.Entity;
                    target.TargetName = caster.Entity.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.HeldItem:
                    target.ItemSlot = caster.InventoryManager?.ActiveHotbarSlot;
                    if (target.ItemSlot?.Itemstack == null)
                    {
                        failureReason = "held item target is unavailable";
                        return false;
                    }

                    target.Entity = caster.Entity;
                    target.TargetName = caster.Entity.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.Inventory:
                    var repairEffect = spell.Effects.FirstOrDefault(effect => string.Equals(effect.Type, SpellEffectTypes.RepairInventoryItem, StringComparison.OrdinalIgnoreCase));
                    if (repairEffect == null)
                    {
                        failureReason = "No damaged item to mend.";
                        return false;
                    }

                    if (!TryFindBestInventoryRepairTarget(caster, spell, repairEffect, out var repairSlot, out failureReason))
                    {
                        return false;
                    }

                    target.ItemSlot = repairSlot;
                    target.Entity = caster.Entity;
                    target.TargetName = repairSlot.Itemstack?.GetName() ?? spell.Code;
                    return true;
                case SpellTargetTypes.LookEntity:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var entity = sapi.World.GetEntityById(castState.LockedEntityId);
                    if (entity == null || !entity.Alive)
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var effectiveRange = SpellRegistry.GetEffectiveLookTargetRange(spell);
                    if (effectiveRange > 0)
                    {
                        var casterPos = caster.Entity.Pos.XYZ;
                        var targetPos = entity.Pos.XYZ;
                        var dx = casterPos.X - targetPos.X;
                        var dy = casterPos.Y - targetPos.Y;
                        var dz = casterPos.Z - targetPos.Z;
                        if ((dx * dx) + (dy * dy) + (dz * dz) > effectiveRange * effectiveRange)
                        {
                            failureReason = "The locked target moved out of range.";
                            return false;
                        }
                    }

                    if (spell.RequiresLineOfSight && !TryHasLineOfSight(caster, entity.Pos.XYZ, entity.EntityId, out failureReason))
                    {
                        return false;
                    }

                    target.Entity = entity;
                    target.Position = entity.Pos.XYZ;
                    target.TargetName = entity.GetName() ?? spell.Code;
                    return true;
                }
                case SpellTargetTypes.LookBlock:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked block target is no longer valid.";
                        return false;
                    }

                    var pos = new BlockPos(castState.LockedBlockX, castState.LockedBlockY, castState.LockedBlockZ, caster.Entity.Pos.Dimension);
                    var block = sapi.World.BlockAccessor.GetBlock(pos);
                    if (block == null)
                    {
                        failureReason = "The locked block target is no longer valid.";
                        return false;
                    }

                    var targetPos = new Vec3d(castState.LockedPosX, castState.LockedPosY, castState.LockedPosZ);
                    var blockRange = SpellRegistry.GetEffectiveLookTargetRange(spell);
                    if (blockRange > 0)
                    {
                        var casterPos = caster.Entity.Pos.XYZ;
                        var dx = casterPos.X - targetPos.X;
                        var dy = casterPos.Y - targetPos.Y;
                        var dz = casterPos.Z - targetPos.Z;
                        if ((dx * dx) + (dy * dy) + (dz * dz) > blockRange * blockRange)
                        {
                            failureReason = "The locked block target moved out of range.";
                            return false;
                        }
                    }

                    target.Position = targetPos;
                    target.BlockPos = pos;
                    target.TargetName = !string.IsNullOrWhiteSpace(castState.LockedTargetName) ? castState.LockedTargetName : GetBlockDisplayName(pos);
                    return true;
                }
                case SpellTargetTypes.LookPosition:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var targetPos = new Vec3d(castState.LockedPosX, castState.LockedPosY, castState.LockedPosZ);
                    var positionRange = SpellRegistry.GetEffectiveLookTargetRange(spell);
                    if (positionRange > 0)
                    {
                        var casterPos = caster.Entity.Pos.XYZ;
                        var dx = casterPos.X - targetPos.X;
                        var dy = casterPos.Y - targetPos.Y;
                        var dz = casterPos.Z - targetPos.Z;
                        if ((dx * dx) + (dy * dy) + (dz * dz) > positionRange * positionRange)
                        {
                            failureReason = "The locked target moved out of range.";
                            return false;
                        }
                    }

                    target.Position = targetPos;
                    target.TargetName = !string.IsNullOrWhiteSpace(castState.LockedTargetName) ? castState.LockedTargetName : spell.Code;
                    return true;
                }
                default:
                    return TryResolveTarget(caster, spell, out target, out failureReason);
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
            sapi.Logger.Debug("[TheRustweave] Spell '{0}' processing effect '{1}'.", spell.Code, effect.Type);

            switch (effect.Type)
            {
                case SpellEffectTypes.None:
                    return true;
                case SpellEffectTypes.HealTarget:
                    return TryAppendHealTarget(gameplayActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.HealArea:
                    return TryAppendHealArea(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.RepairHeldItem:
                    return TryAppendRepairHeldItem(gameplayActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.RepairInventoryItem:
                    return TryAppendRepairInventoryItem(gameplayActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.HealSelf:
                    return TryAppendHealSelf(gameplayActions, caster, effect, out failureReason);
                case SpellEffectTypes.DamageRayEntity:
                    return TryAppendDamageRayEntity(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.DamageArea:
                    return TryAppendDamageArea(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.ShieldSelf:
                    return TryAppendShieldSelf(gameplayActions, caster, spell, effect, out failureReason);
                case SpellEffectTypes.ShieldTarget:
                    return TryAppendShieldTarget(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.SlowTarget:
                    return TryAppendTimedMovementModifier(gameplayActions, caster, target, spell, effect, effect.SpeedMultiplier, "walkspeed", out failureReason, effect.Type);
                case SpellEffectTypes.RootTarget:
                    return TryAppendTimedMovementModifier(gameplayActions, caster, target, spell, effect, effect.SpeedMultiplier <= 0 ? 0.05f : effect.SpeedMultiplier, "walkspeed", out failureReason, effect.Type);
                case SpellEffectTypes.SpeedBuff:
                    return TryAppendTimedMovementModifier(gameplayActions, caster, target, spell, effect, effect.SpeedMultiplier, "walkspeed", out failureReason, effect.Type, applyToCaster: true);
                case SpellEffectTypes.DamageOverTime:
                    return TryAppendDamageOverTime(gameplayActions, caster, target, spell, effect, out failureReason);
                case SpellEffectTypes.StunTarget:
                    return TryAppendStunTarget(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.KnockbackEntity:
                    return TryAppendKnockbackEntity(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.PullEntity:
                    return TryAppendPullEntity(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.WeakenTarget:
                    return TryAppendWeakenTarget(gameplayActions, caster, target, spell, effect, out failureReason);
                case SpellEffectTypes.ProjectileEntity:
                    return TryAppendProjectileEntity(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.TeleportForward:
                    return TryAppendTeleportForward(gameplayActions, caster, effect, out failureReason);
                case SpellEffectTypes.CorruptionTransfer:
                    return TryAppendCorruptionTransfer(ventActions, caster, spell, target, effect, ref corruptionDelta, out failureReason);
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

        private bool TryAppendRepairInventoryItem(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var slot = target.ItemSlot;
            var stack = slot?.Itemstack;

            if (slot == null || stack == null)
            {
                failureReason = "No damaged item to mend.";
                sapi.Logger.Warning("[TheRustweave] repairInventoryItem failed because the selected inventory slot was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var collectible = stack.Collectible;
            var maxDurability = collectible.GetMaxDurability(stack);
            if (maxDurability <= 0)
            {
                failureReason = "No damaged item to mend.";
                sapi.Logger.Warning("[TheRustweave] repairInventoryItem failed because the selected inventory item is not damageable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var remainingDurability = collectible.GetRemainingDurability(stack);
            if (remainingDurability <= 0 && !effect.AllowBrokenItems)
            {
                failureReason = "No damaged item to mend.";
                sapi.Logger.Warning("[TheRustweave] repairInventoryItem failed because the selected inventory item is broken and allowBrokenItems is false for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var repairAmount = Math.Min(effect.DurabilityAmount, maxDurability - remainingDurability);
            if (repairAmount <= 0)
            {
                failureReason = "No damaged item to mend.";
                sapi.Logger.Warning("[TheRustweave] repairInventoryItem failed because the selected inventory item is already fully repaired for player '{0}'.", caster.PlayerUID);
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

        private bool TryAppendHealSelf(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var health = caster.Entity?.GetBehavior<EntityBehaviorHealth>();
            if (health == null)
            {
                failureReason = "caster does not have a health behavior";
                sapi.Logger.Warning("[TheRustweave] healSelf failed because player '{0}' has no health behavior.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                health.Health = Math.Min(health.MaxHealth, health.Health + effect.HealthAmount);
                return true;
            });

            return true;
        }

        private bool TryAppendHealTarget(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] healTarget failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var health = targetEntity.GetBehavior<EntityBehaviorHealth>();
            if (health == null)
            {
                failureReason = "the targeted entity does not have a health behavior";
                sapi.Logger.Warning("[TheRustweave] healTarget failed because the target entity has no health behavior for player '{0}'.", caster.PlayerUID);
                return false;
            }

            if (effect.HealthAmount <= 0)
            {
                failureReason = "healthAmount must be greater than zero";
                return false;
            }

            gameplayActions.Add(() =>
            {
                health.Health = Math.Min(health.MaxHealth, health.Health + effect.HealthAmount);
                sapi.Logger.Debug("[TheRustweave] Spell healTarget restored {0} health on entity '{1}'.", effect.HealthAmount, targetEntity.GetName());
                return true;
            });

            return true;
        }

        private bool TryAppendHealArea(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var radius = SpellRegistry.GetEffectiveRadius(spell, effect);
            if (radius <= 0)
            {
                failureReason = "radius must be greater than zero";
                return false;
            }

            var center = target.Entity?.Pos?.XYZ ?? target.Position;
            var entities = sapi.World.GetEntitiesAround(
                center,
                (float)radius,
                (float)radius,
                candidate =>
                {
                    if (candidate == null || !candidate.Alive)
                    {
                        return false;
                    }

                    if (!effect.IncludeCaster && candidate == caster.Entity)
                    {
                        return false;
                    }

                    return candidate.GetBehavior<EntityBehaviorHealth>() != null;
                });

            if ((entities == null || entities.Length == 0) && !effect.AllowNoTargets)
            {
                failureReason = "No target struck.";
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' healArea found no targets within radius {1}.", spell.Code, radius);
                return false;
            }

            var limitedTargets = entities?
                .Where(entity => entity != null && entity.Alive && entity.GetBehavior<EntityBehaviorHealth>() != null)
                .Take(effect.MaxTargets > 0 ? effect.MaxTargets : int.MaxValue)
                .ToArray() ?? Array.Empty<Entity>();

            if (limitedTargets.Length == 0 && !effect.AllowNoTargets)
            {
                failureReason = "No target struck.";
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' healArea found no valid healing targets within radius {1}.", spell.Code, radius);
                return false;
            }

            gameplayActions.Add(() =>
            {
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applying healArea for {1} target(s) with {2} healing.", spell.Code, limitedTargets.Length, effect.HealthAmount);
                foreach (var entity in limitedTargets)
                {
                    var health = entity.GetBehavior<EntityBehaviorHealth>();
                    if (health == null)
                    {
                        continue;
                    }

                    health.Health = Math.Min(health.MaxHealth, health.Health + effect.HealthAmount);
                }

                return true;
            });

            return true;
        }

        private bool TryAppendDamageRayEntity(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "No target struck.";
                sapi.Logger.Warning("[TheRustweave] damageRayEntity failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var damageSource = new DamageSource
            {
                Source = EnumDamageSource.Internal,
                SourceEntity = caster.Entity,
                CauseEntity = caster.Entity,
                KnockbackStrength = 0f
            };

            if (!targetEntity.ShouldReceiveDamage(damageSource, effect.DamageAmount))
            {
                failureReason = "the targeted entity cannot receive that damage";
                sapi.Logger.Warning("[TheRustweave] damageRayEntity failed because the target entity refused damage for player '{0}'.", caster.PlayerUID);
                return false;
            }

            gameplayActions.Add(() =>
            {
                return ApplySpellDamage(caster, spell, effect.Type, targetEntity, damageSource, effect.DamageAmount);
            });

            return true;
        }

        private bool TryAppendDamageArea(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var radius = SpellRegistry.GetEffectiveRadius(spell, effect);
            if (radius <= 0)
            {
                failureReason = "radius must be greater than zero";
                return false;
            }

            var center = target.Entity?.Pos?.XYZ ?? target.Position;
            var damageSource = new DamageSource
            {
                Source = EnumDamageSource.Internal,
                SourceEntity = caster.Entity,
                CauseEntity = caster.Entity,
                KnockbackStrength = 0f
            };

            var entities = sapi.World.GetEntitiesAround(
                center,
                (float)radius,
                (float)radius,
                candidate =>
                {
                    if (candidate == null || !candidate.Alive)
                    {
                        return false;
                    }

                    if (!candidate.IsCreature)
                    {
                        return false;
                    }

                    if (!effect.IncludeCaster && candidate == caster.Entity)
                    {
                        return false;
                    }

                    if (!sapi.Server.Config.AllowPvP && candidate is EntityPlayer && candidate != caster.Entity)
                    {
                        return false;
                    }

                    return true;
                });

            if ((entities == null || entities.Length == 0) && !effect.AllowNoTargets)
            {
                failureReason = "No target struck.";
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' damageArea found no targets within radius {1}.", spell.Code, radius);
                return false;
            }

            var limitedTargets = entities?
                .Where(entity => entity != null && entity.Alive)
                .Where(entity => entity != null && entity.ShouldReceiveDamage(damageSource, effect.DamageAmount))
                .Take(effect.MaxTargets > 0 ? effect.MaxTargets : int.MaxValue)
                .ToArray() ?? Array.Empty<Entity>();

            if (limitedTargets.Length == 0 && !effect.AllowNoTargets)
            {
                failureReason = "No target struck.";
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' damageArea found no valid damage targets within radius {1}.", spell.Code, radius);
                return false;
            }

            gameplayActions.Add(() =>
            {
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applying damageArea for {1} target(s) with {2} damage.", spell.Code, limitedTargets.Length, effect.DamageAmount);
                foreach (var entity in limitedTargets)
                {
                    if (entity == null || !entity.Alive)
                    {
                        continue;
                    }

                    if (!entity.ShouldReceiveDamage(damageSource, effect.DamageAmount))
                    {
                        continue;
                    }

                    ApplySpellDamage(caster, spell, effect.Type, entity, damageSource, effect.DamageAmount);
                }

                return true;
            });

            return true;
        }

        private bool TryAppendShieldSelf(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            return TryAppendShield(gameplayActions, caster, caster.Entity, spell, effect, out failureReason, "shieldSelf");
        }

        private bool TryAppendShieldTarget(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            return TryAppendShield(gameplayActions, caster, target.Entity, spell, effect, out failureReason, "shieldTarget");
        }

        private bool TryAppendShield(List<Func<bool>> gameplayActions, IServerPlayer caster, Entity? targetEntity, SpellDefinition spell, SpellEffectDefinition effect, out string failureReason, string effectLabel)
        {
            failureReason = string.Empty;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] {0} failed because the target entity was unavailable for player '{1}'.", effectLabel, caster.PlayerUID);
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var shieldAmount = effect.HealthAmount;
            if (shieldAmount <= 0)
            {
                failureReason = "healthAmount must be greater than zero";
                return false;
            }

            var shieldCode = BuildTimedEffectCode(spell.Code, effectLabel, targetEntity.EntityId);

            gameplayActions.Add(() =>
            {
                if (RustweaveRuntime.Server?.TryRegisterShield(targetEntity, shieldCode, shieldAmount, durationMilliseconds, spell.Code, effectLabel) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] {0} failed to register for player '{1}' on entity '{2}'.", effectLabel, caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied {1} shield of {2} to entity {3} for {4} ms.", spell.Code, effectLabel, shieldAmount, targetEntity.EntityId, durationMilliseconds);
                return true;
            });

            return true;
        }

        private bool TryAppendStunTarget(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] stunTarget failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            gameplayActions.Add(() =>
            {
                var movementCode = BuildTimedEffectCode(spell.Code, effect.Type + "-walkspeed", targetEntity.EntityId);
                var attackCode = BuildTimedEffectCode(spell.Code, effect.Type + "-attackpower", targetEntity.EntityId);

                if (RustweaveRuntime.Server?.TryRegisterTimedStatModifier(targetEntity, "walkspeed", movementCode, -0.95f, durationMilliseconds, spell.Code, effect.Type) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] stunTarget failed to register movement stun for player '{0}' on entity '{1}'.", caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                if (RustweaveRuntime.Server?.TryRegisterTimedStatModifier(targetEntity, "attackpower", attackCode, -1f, durationMilliseconds, spell.Code, effect.Type) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] stunTarget failed to register attack reduction for player '{0}' on entity '{1}'.", caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied stunTarget to entity {1} for {2} ms.", spell.Code, targetEntity.EntityId, durationMilliseconds);
                return true;
            });

            return true;
        }

        private bool TryAppendProjectileEntity(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            var targetPosition = target.Position;
            var hasGameplayEffect = false;

            if (targetEntity != null && targetEntity.Alive)
            {
                if (effect.DamageAmount > 0)
                {
                    hasGameplayEffect = true;
                    var damageSource = new DamageSource
                    {
                        Source = EnumDamageSource.Internal,
                        SourceEntity = caster.Entity,
                        CauseEntity = caster.Entity,
                        KnockbackStrength = 0f
                    };

                    if (!targetEntity.ShouldReceiveDamage(damageSource, effect.DamageAmount))
                    {
                        failureReason = "the targeted entity cannot receive that damage";
                        sapi.Logger.Warning("[TheRustweave] projectileEntity failed because the target entity refused damage for player '{0}'.", caster.PlayerUID);
                        return false;
                    }

                    gameplayActions.Add(() =>
                    {
                        return ApplySpellDamage(caster, spell, effect.Type, targetEntity, damageSource, effect.DamageAmount);
                    });
                }
                else if (effect.HealthAmount > 0)
                {
                    hasGameplayEffect = true;
                    var health = targetEntity.GetBehavior<EntityBehaviorHealth>();
                    if (health == null)
                    {
                        failureReason = "the targeted entity does not have a health behavior";
                        sapi.Logger.Warning("[TheRustweave] projectileEntity failed because the target entity has no health behavior for player '{0}'.", caster.PlayerUID);
                        return false;
                    }

                    gameplayActions.Add(() =>
                    {
                        health.Health = Math.Min(health.MaxHealth, health.Health + effect.HealthAmount);
                        sapi.Logger.Debug("[TheRustweave] Spell projectileEntity restored {0} health on entity '{1}'.", effect.HealthAmount, targetEntity.GetName());
                        return true;
                    });
                }
            }

            if (effect.Count > 0 || !string.IsNullOrWhiteSpace(effect.ParticleCode))
            {
                var color = GetParticleColor(effect.ParticleCode);
                var minPos = new Vec3d(targetPosition.X - 0.12, targetPosition.Y - 0.12, targetPosition.Z - 0.12);
                var maxPos = new Vec3d(targetPosition.X + 0.12, targetPosition.Y + 0.12, targetPosition.Z + 0.12);
                var minVelocity = new Vec3f(-0.02f, 0.02f, -0.02f);
                var maxVelocity = new Vec3f(0.02f, 0.08f, 0.02f);

                visualActions.Add(() =>
                {
                    sapi.World.SpawnParticles(effect.Count > 0 ? effect.Count : 1, color, minPos, maxPos, minVelocity, maxVelocity, 0.6f, 0f, 1f, EnumParticleModel.Quad, caster);
                    return true;
                });
            }

            if (!hasGameplayEffect && effect.Count <= 0 && string.IsNullOrWhiteSpace(effect.ParticleCode))
            {
                sapi.Logger.Debug("[TheRustweave] projectileEntity resolved for spell '{0}' without a direct gameplay effect.", spell.Code);
            }

            return true;
        }

        private bool TryAppendTimedMovementModifier(
            List<Func<bool>> gameplayActions,
            IServerPlayer caster,
            SpellTargetContext target,
            SpellDefinition spell,
            SpellEffectDefinition effect,
            float multiplier,
            string statCategory,
            out string failureReason,
            string effectLabel,
            bool applyToCaster = false)
        {
            failureReason = string.Empty;

            var targetEntity = applyToCaster ? caster.Entity : target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] {0} failed because the target entity was unavailable for player '{1}'.", effectLabel, caster.PlayerUID);
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var modifierDelta = multiplier - 1f;
            var modifierCode = BuildTimedEffectCode(spell.Code, effectLabel, targetEntity.EntityId);

            gameplayActions.Add(() =>
            {
                if (RustweaveRuntime.Server?.TryRegisterTimedStatModifier(targetEntity, statCategory, modifierCode, modifierDelta, durationMilliseconds, spell.Code, effectLabel) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] {0} failed to register for player '{1}' on entity '{2}'.", effectLabel, caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied timed {1} modifier {2} to entity {3} for {4} ms.", spell.Code, statCategory, modifierDelta, targetEntity.EntityId, durationMilliseconds);
                return true;
            });

            return true;
        }

        private bool TryAppendWeakenTarget(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellDefinition spell, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] weakenTarget failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var damageMultiplier = effect.DamageMultiplier > 0 ? effect.DamageMultiplier : effect.IncomingDamageMultiplier;
            if (damageMultiplier <= 0)
            {
                failureReason = "damageMultiplier must be greater than zero";
                return false;
            }

            var damageDelta = damageMultiplier - 1f;
            var damageCode = BuildTimedEffectCode(spell.Code, effect.Type + "-damage", targetEntity.EntityId);
            var attackCode = BuildTimedEffectCode(spell.Code, effect.Type + "-attackpower", targetEntity.EntityId);

            gameplayActions.Add(() =>
            {
                if (RustweaveRuntime.Server?.TryRegisterTimedStatModifier(targetEntity, "damage", damageCode, damageDelta, durationMilliseconds, spell.Code, effect.Type) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] weakenTarget failed to register damage reduction for player '{0}' on entity '{1}'.", caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                if (RustweaveRuntime.Server?.TryRegisterTimedStatModifier(targetEntity, "attackpower", attackCode, damageDelta, durationMilliseconds, spell.Code, effect.Type) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] weakenTarget failed to register attackpower reduction for player '{0}' on entity '{1}'.", caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied weakenTarget to entity {1} for {2} ms with damage delta {3}.", spell.Code, targetEntity.EntityId, durationMilliseconds, damageDelta);
                return true;
            });

            return true;
        }

        private bool TryAppendDamageOverTime(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellTargetContext target, SpellDefinition spell, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] damageOverTime failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            if (effect.DamagePerTick <= 0 || effect.TickIntervalSeconds <= 0 || effect.DurationSeconds <= 0)
            {
                failureReason = "damage over time parameters are invalid";
                return false;
            }

            var durationMilliseconds = (long)Math.Round(effect.DurationSeconds * 1000d);
            var tickIntervalMilliseconds = (long)Math.Round(effect.TickIntervalSeconds * 1000d);
            if (durationMilliseconds <= 0 || tickIntervalMilliseconds <= 0)
            {
                failureReason = "damage over time parameters are invalid";
                return false;
            }

            var effectCode = BuildTimedEffectCode(spell.Code, effect.Type, targetEntity.EntityId);
            gameplayActions.Add(() =>
            {
                if (RustweaveRuntime.Server?.TryRegisterDamageOverTime(targetEntity, caster.Entity, effectCode, effect.DamagePerTick, tickIntervalMilliseconds, durationMilliseconds, spell.Code, effect.Type) != true)
                {
                    sapi.Logger.Warning("[TheRustweave] damageOverTime failed to register for player '{0}' on entity '{1}'.", caster.PlayerUID, targetEntity.EntityId);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied damageOverTime to entity {1} for {2} ms at {3} damage/tick.", spell.Code, targetEntity.EntityId, durationMilliseconds, effect.DamagePerTick);
                return true;
            });

            return true;
        }

        private bool TryAppendKnockbackEntity(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive || targetEntity == caster.Entity)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] knockbackEntity failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var force = Math.Max(0f, effect.Force);
            if (force <= 0)
            {
                failureReason = "force must be greater than zero";
                return false;
            }

            gameplayActions.Add(() =>
            {
                var casterPos = caster.Entity?.Pos.XYZ ?? targetEntity.Pos.XYZ;
                var awayVector = new Vec3d(targetEntity.Pos.X - casterPos.X, targetEntity.Pos.Y - casterPos.Y, targetEntity.Pos.Z - casterPos.Z);
                var awayLength = Math.Sqrt((awayVector.X * awayVector.X) + (awayVector.Y * awayVector.Y) + (awayVector.Z * awayVector.Z));
                if (awayLength <= 0)
                {
                    var viewVector = caster.Entity?.Pos.GetViewVector() ?? new Vec3f(0, 0, 1);
                    awayVector = new Vec3d(viewVector.X, viewVector.Y, viewVector.Z);
                    awayLength = Math.Sqrt((awayVector.X * awayVector.X) + (awayVector.Y * awayVector.Y) + (awayVector.Z * awayVector.Z));
                }

                if (awayLength > 0)
                {
                    awayVector = new Vec3d(awayVector.X / awayLength, awayVector.Y / awayLength, awayVector.Z / awayLength);
                }

                targetEntity.Pos.Motion = targetEntity.Pos.Motion.Add(awayVector.X * force, Math.Max(0.15, awayVector.Y * 0.35 + 0.2), awayVector.Z * force);
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied knockbackEntity force {1} to entity '{2}'.", spell.Code, force, targetEntity.GetName());
                return true;
            });

            return true;
        }

        private bool TryAppendPullEntity(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive || targetEntity == caster.Entity)
            {
                failureReason = "the targeted entity is unavailable";
                sapi.Logger.Warning("[TheRustweave] pullEntity failed because the target entity was unavailable for player '{0}'.", caster.PlayerUID);
                return false;
            }

            var force = Math.Max(0f, effect.Force);
            if (force <= 0)
            {
                failureReason = "force must be greater than zero";
                return false;
            }

            gameplayActions.Add(() =>
            {
                var casterPos = caster.Entity?.Pos.XYZ ?? targetEntity.Pos.XYZ;
                var towardVector = new Vec3d(casterPos.X - targetEntity.Pos.X, casterPos.Y - targetEntity.Pos.Y, casterPos.Z - targetEntity.Pos.Z);
                var towardLength = Math.Sqrt((towardVector.X * towardVector.X) + (towardVector.Y * towardVector.Y) + (towardVector.Z * towardVector.Z));
                if (towardLength <= 0)
                {
                    var viewVector = caster.Entity?.Pos.GetViewVector() ?? new Vec3f(0, 0, 1);
                    towardVector = new Vec3d(-viewVector.X, -viewVector.Y, -viewVector.Z);
                    towardLength = Math.Sqrt((towardVector.X * towardVector.X) + (towardVector.Y * towardVector.Y) + (towardVector.Z * towardVector.Z));
                }

                if (towardLength > 0)
                {
                    towardVector = new Vec3d(towardVector.X / towardLength, towardVector.Y / towardLength, towardVector.Z / towardLength);
                }

                targetEntity.Pos.Motion = targetEntity.Pos.Motion.Add(towardVector.X * force, Math.Max(0.1, towardVector.Y * 0.25), towardVector.Z * force);
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied pullEntity force {1} to entity '{2}'.", spell.Code, force, targetEntity.GetName());
                return true;
            });

            return true;
        }

        private static string BuildTimedEffectCode(string spellCode, string effectType, long entityId)
        {
            return $"therustweave.{spellCode}.{effectType}.{entityId}";
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

        private bool TryAppendCorruptionTransfer(List<Func<bool>> ventActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, ref int corruptionDelta, out string failureReason)
        {
            failureReason = string.Empty;
            var corruptionAmount = Math.Max(0, effect.CorruptionAmount);
            if (corruptionAmount <= 0)
            {
                failureReason = "corruptionAmount must be greater than zero";
                return false;
            }

            corruptionDelta -= corruptionAmount;

            var targetPlayer = target.Entity is EntityPlayer targetEntityPlayer && !string.IsNullOrWhiteSpace(targetEntityPlayer.PlayerUID)
                ? sapi.World.PlayerByUid(targetEntityPlayer.PlayerUID) as IServerPlayer
                : null;

            ventActions.Add(() =>
            {
                if (!RustweaveRuntime.Server?.TryTransferCorruption(caster, targetPlayer, corruptionAmount, spell.Code, effect.Type) ?? true)
                {
                    sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed to transfer {1} corruption for player '{2}'.", spell.Code, corruptionAmount, caster.PlayerUID);
                    return false;
                }

                sapi.Logger.Debug("[TheRustweave] Spell '{0}' transferred {1} corruption.", spell.Code, corruptionAmount);
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

        private bool ApplySpellDamage(IServerPlayer caster, SpellDefinition spell, string effectType, Entity targetEntity, DamageSource damageSource, float damageAmount)
        {
            if (targetEntity == null || !targetEntity.Alive || damageAmount <= 0)
            {
                return false;
            }

            if (RustweaveRuntime.Server?.TryApplySpellDamage(targetEntity, damageSource, damageAmount, spell.Code, effectType) == true)
            {
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied {1} damage to entity '{2}' via {3}.", spell.Code, damageAmount, targetEntity.GetName(), effectType);
                return true;
            }

            if (RustweaveRuntime.Server != null)
            {
                return false;
            }

            if (!targetEntity.ShouldReceiveDamage(damageSource, damageAmount))
            {
                return false;
            }

            targetEntity.ReceiveDamage(damageSource, damageAmount);
            sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied {1} fallback damage to entity '{2}' via {3}.", spell.Code, damageAmount, targetEntity.GetName(), effectType);
            return true;
        }

        private bool TryResolveLookEntityTarget(IServerPlayer caster, double range, out EntitySelection selection, out string failureReason)
        {
            selection = new EntitySelection();
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            var fromPos = entity.Pos.XYZ;
            var eyePos = entity.LocalEyePos;
            fromPos = new Vec3d(entity.Pos.X + eyePos.X, entity.Pos.Y + eyePos.Y, entity.Pos.Z + eyePos.Z);

            var viewVector = entity.Pos.GetViewVector();
            var rayRange = (float)Math.Max(0d, range);
            var toPos = new Vec3d(
                fromPos.X + (viewVector.X * rayRange),
                fromPos.Y + (viewVector.Y * rayRange),
                fromPos.Z + (viewVector.Z * rayRange));

            BlockSelection? blockSelection = null;
            EntitySelection? entitySelection = null;
            sapi.World.RayTraceForSelection(
                fromPos,
                toPos,
                ref blockSelection,
                ref entitySelection,
                null,
                candidate => candidate != null
                    && candidate != caster.Entity
                    && candidate.Alive
                    && (sapi.Server.Config.AllowPvP || candidate is not EntityPlayer));

            if (entitySelection?.Entity == null)
            {
                failureReason = "No target struck.";
                sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} hit no entity.", rayRange);
                return false;
            }

            selection = entitySelection;
            sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} hit entity '{1}'.", rayRange, entitySelection.Entity.GetName());
            return true;
        }

        private bool TryResolveLookBlockTarget(IServerPlayer caster, double range, out BlockPos? blockPos, out Vec3d position, out string failureReason)
        {
            blockPos = null;
            position = new Vec3d();
            failureReason = string.Empty;

            if (!TryResolveLookSelection(caster, range, out var blockSelection, out var entitySelection, out failureReason))
            {
                return false;
            }

            if (blockSelection == null)
            {
                failureReason = "No target struck.";
                return false;
            }

            blockPos = blockSelection.Position;
            position = blockSelection.HitPosition;
            sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} hit block at {1}.", range, position);
            return true;
        }

        private bool TryResolveLookPositionTarget(IServerPlayer caster, double range, out Vec3d position, out string failureReason)
        {
            position = new Vec3d();
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            var eyePos = entity.LocalEyePos;
            var fromPos = new Vec3d(entity.Pos.X + eyePos.X, entity.Pos.Y + eyePos.Y, entity.Pos.Z + eyePos.Z);
            var viewVector = entity.Pos.GetViewVector();
            var rayRange = (float)Math.Max(0d, range);
            var toPos = new Vec3d(
                fromPos.X + (viewVector.X * rayRange),
                fromPos.Y + (viewVector.Y * rayRange),
                fromPos.Z + (viewVector.Z * rayRange));

            BlockSelection? blockSelection = null;
            EntitySelection? entitySelection = null;
            sapi.World.RayTraceForSelection(
                fromPos,
                toPos,
                ref blockSelection,
                ref entitySelection,
                null,
                candidate => candidate != null
                    && candidate != caster.Entity
                    && candidate.Alive
                    && (sapi.Server.Config.AllowPvP || candidate is not EntityPlayer));

            if (entitySelection?.Entity != null)
            {
                position = entitySelection.HitPosition;
                sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} resolved entity position {1}.", range, position);
                return true;
            }

            if (blockSelection != null)
            {
                position = blockSelection.HitPosition;
                sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} resolved block position {1}.", range, position);
                return true;
            }

            position = toPos;
            sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} resolved end position {1}.", range, position);
            return true;
        }

        private bool TryResolveLookSelection(IServerPlayer caster, double range, out BlockSelection? blockSelection, out EntitySelection? entitySelection, out string failureReason)
        {
            blockSelection = null;
            entitySelection = null;
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            var eyePos = entity.LocalEyePos;
            var fromPos = new Vec3d(entity.Pos.X + eyePos.X, entity.Pos.Y + eyePos.Y, entity.Pos.Z + eyePos.Z);
            var viewVector = entity.Pos.GetViewVector();
            var rayRange = (float)Math.Max(0d, range);
            var toPos = new Vec3d(
                fromPos.X + (viewVector.X * rayRange),
                fromPos.Y + (viewVector.Y * rayRange),
                fromPos.Z + (viewVector.Z * rayRange));

            sapi.World.RayTraceForSelection(
                fromPos,
                toPos,
                ref blockSelection,
                ref entitySelection,
                null,
                candidate => candidate != null
                    && candidate != caster.Entity
                    && candidate.Alive
                    && (sapi.Server.Config.AllowPvP || candidate is not EntityPlayer));

            if (blockSelection == null && entitySelection?.Entity == null)
            {
                failureReason = "No target struck.";
                return false;
            }

            return true;
        }

        private bool TryHasLineOfSight(IServerPlayer caster, Vec3d targetPos, long? targetEntityId, out string failureReason)
        {
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            var eyePos = entity.LocalEyePos;
            var fromPos = new Vec3d(entity.Pos.X + eyePos.X, entity.Pos.Y + eyePos.Y, entity.Pos.Z + eyePos.Z);
            BlockSelection? blockSelection = null;
            EntitySelection? entitySelection = null;
            sapi.World.RayTraceForSelection(
                fromPos,
                targetPos,
                ref blockSelection,
                ref entitySelection,
                null,
                candidate => candidate != null
                    && candidate.Alive
                    && candidate != caster.Entity
                    && (sapi.Server.Config.AllowPvP || candidate is not EntityPlayer));

            if (targetEntityId.HasValue && entitySelection?.Entity != null && entitySelection.Entity.EntityId == targetEntityId.Value)
            {
                return true;
            }

            if (targetEntityId.HasValue)
            {
                failureReason = "The locked target is no longer visible.";
                return false;
            }

            if (blockSelection != null || entitySelection?.Entity != null)
            {
                failureReason = "The locked target is no longer visible.";
                return false;
            }

            return true;
        }

        private bool TryFindBestInventoryRepairTarget(IServerPlayer caster, SpellDefinition spell, SpellEffectDefinition repairEffect, out ItemSlot? selectedSlot, out string failureReason)
        {
            selectedSlot = null;
            failureReason = string.Empty;

            var inventories = caster.InventoryManager?.InventoriesOrdered;
            if (inventories == null)
            {
                failureReason = "No damaged item to mend.";
                return false;
            }

            ItemSlot? bestSlot = null;
            int bestMissingDurability = 0;

            foreach (var inventory in inventories)
            {
                if (!IsInventoryRepairCandidate(inventory))
                {
                    continue;
                }

                if (!TryGetInventoryCount(inventory, out var inventoryCount))
                {
                    continue;
                }

                for (var index = 0; index < inventoryCount; index++)
                {
                    var slot = inventory[index];
                    var stack = slot?.Itemstack;
                    if (stack == null || IsRustweaverTomeStack(stack))
                    {
                        continue;
                    }

                    var collectible = stack.Collectible;
                    var maxDurability = collectible.GetMaxDurability(stack);
                    if (maxDurability <= 0)
                    {
                        continue;
                    }

                    var remainingDurability = collectible.GetRemainingDurability(stack);
                    if (remainingDurability <= 0 && !repairEffect.AllowBrokenItems)
                    {
                        continue;
                    }

                    var missingDurability = maxDurability - remainingDurability;
                    if (missingDurability <= 0)
                    {
                        continue;
                    }

                    if (missingDurability > bestMissingDurability)
                    {
                        bestMissingDurability = missingDurability;
                        bestSlot = slot;
                    }
                }
            }

            if (bestSlot == null)
            {
                failureReason = "No damaged item to mend.";
                sapi.Logger.Debug("[TheRustweave] Spell '{0}' could not find a damaged inventory item to repair for player '{1}'.", spell.Code, caster.PlayerUID);
                return false;
            }

            selectedSlot = bestSlot;
            return true;
        }

        private static bool TryGetInventoryCount(IInventory inventory, out int count)
        {
            try
            {
                count = inventory.Count;
                return true;
            }
            catch
            {
                count = 0;
                return false;
            }
        }

        private static bool IsInventoryRepairCandidate(IInventory inventory)
        {
            var typeName = inventory.GetType().Name;
            if (typeName.IndexOf("Creative", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            if (typeName.IndexOf("Player", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (typeName.IndexOf("Hotbar", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (typeName.IndexOf("Backpack", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsRustweaverTomeStack(ItemStack stack)
        {
            var collectible = stack?.Collectible;
            return collectible != null
                && (collectible is ItemRustweaverTome
                    || string.Equals(collectible.Code?.Path, RustweaveConstants.TomeItemCode, StringComparison.OrdinalIgnoreCase));
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

        private string GetBlockDisplayName(BlockPos? pos)
        {
            if (pos == null)
            {
                return string.Empty;
            }

            var block = sapi.World?.BlockAccessor?.GetBlock(pos);
            if (block?.Code != null)
            {
                return block.Code.ToShortString();
            }

            return $"Block {pos.X}, {pos.Y}, {pos.Z}";
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

        internal sealed class SpellTargetContext
        {
            public Entity? Entity { get; set; }

            public ItemSlot? ItemSlot { get; set; }

            public BlockPos? BlockPos { get; set; }

            public string TargetName { get; set; } = string.Empty;

            public Vec3d Position { get; set; } = new Vec3d();
        }
    }
}

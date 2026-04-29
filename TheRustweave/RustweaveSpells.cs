using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
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
        public const string Twining = "Twining";
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
            Twining,
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

        public static string Normalize(string? school)
        {
            if (string.IsNullOrWhiteSpace(school))
            {
                return string.Empty;
            }

            var trimmed = school.Trim();
            if (string.Equals(trimmed, Twinning, StringComparison.OrdinalIgnoreCase))
            {
                return Twining;
            }

            return Recognized.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? trimmed;
        }
    }

    internal static class SpellTargetTypes
    {
        public const string Self = "self";
        public const string HeldItem = "heldItem";
        public const string Inventory = "inventory";
        public const string LookEntity = "lookEntity";
        public const string LookBlock = "lookBlock";
        public const string LookPosition = "lookPosition";
        public const string LookPlayer = "lookPlayer";
        public const string LookNonPlayerEntity = "lookNonPlayerEntity";
        public const string LookDroppedItem = "lookDroppedItem";
        public const string LookBlockEntity = "lookBlockEntity";
        public const string LookContainer = "lookContainer";
        public const string SelfArea = "selfArea";
        public const string LookArea = "lookArea";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Self,
            HeldItem,
            Inventory,
            LookEntity,
            LookBlock,
            LookPosition,
            LookPlayer,
            LookNonPlayerEntity,
            LookDroppedItem,
            LookBlockEntity,
            LookContainer,
            SelfArea,
            LookArea
        };

        public static string Normalize(string? targetType)
        {
            if (string.IsNullOrWhiteSpace(targetType))
            {
                return string.Empty;
            }

            var trimmed = targetType.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? trimmed;
        }

        public static string GetDisplayName(string targetType)
        {
            return targetType switch
            {
                Self => "Self",
                HeldItem => "Held Item",
                Inventory => "Inventory",
                LookEntity => "Entity",
                LookPlayer => "Player",
                LookNonPlayerEntity => "Creature/NPC",
                LookDroppedItem => "Dropped Item",
                LookBlock => "Block",
                LookBlockEntity => "Block Entity",
                LookContainer => "Container",
                SelfArea => "Area Around Self",
                LookArea => "Targeted Area",
                LookPosition => "Position",
                _ => targetType
            };
        }

        public static bool IsEntityTarget(string? targetType)
        {
            return string.Equals(targetType, LookEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookPlayer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookDroppedItem, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLivingEntityTarget(string? targetType)
        {
            return string.Equals(targetType, LookEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookPlayer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPlayerTarget(string? targetType)
        {
            return string.Equals(targetType, LookPlayer, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNonPlayerEntityTarget(string? targetType)
        {
            return string.Equals(targetType, LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDroppedItemTarget(string? targetType)
        {
            return string.Equals(targetType, LookDroppedItem, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBlockTarget(string? targetType)
        {
            return string.Equals(targetType, LookBlock, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookBlockEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookContainer, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAreaTarget(string? targetType)
        {
            return string.Equals(targetType, SelfArea, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookArea, StringComparison.OrdinalIgnoreCase);
        }

        public static bool RequiresLookRange(string? targetType)
        {
            return string.Equals(targetType, LookEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookPlayer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookDroppedItem, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookBlock, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookBlockEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookContainer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookPosition, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, LookArea, StringComparison.OrdinalIgnoreCase);
        }
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

    internal static class SpellWarningTypes
    {
        public const string Neutral = "neutral";
        public const string Hostile = "hostile";
        public const string Beneficial = "beneficial";

        public static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            Neutral,
            Hostile,
            Beneficial
        };

        public static string Normalize(string? warningType)
        {
            if (string.IsNullOrWhiteSpace(warningType))
            {
                return Neutral;
            }

            var trimmed = warningType.Trim();
            return Supported.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase)) ?? Neutral;
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
        // Planned Warping effects. Add to Supported only when the runtime can execute them safely.
        public const string ModifyTemporalStability = "modifyTemporalStability";
        public const string StabilizeArea = "stabilizeArea";
        public const string AnchorEntity = "anchorEntity";
        public const string AnchorBlock = "anchorBlock";
        public const string PreventDisplacement = "preventDisplacement";
        public const string ModifyCorruptionGain = "modifyCorruptionGain";
        // Planned Wefting effects.
        public const string TeleportToTarget = "teleportToTarget";
        public const string TeleportEntityToCaster = "teleportEntityToCaster";
        public const string TeleportEntityToPosition = "teleportEntityToPosition";
        public const string SwapPositions = "swapPositions";
        public const string MoveDroppedItem = "moveDroppedItem";
        public const string MoveBlockEntityContents = "moveBlockEntityContents";
        public const string PushEntity = "pushEntity";
        // Planned Shedding effects.
        public const string ReleaseTarget = "releaseTarget";
        public const string BreakBinding = "breakBinding";
        public const string OpenLock = "openLock";
        public const string OpenPassage = "openPassage";
        public const string CreateRift = "createRift";
        public const string FreezeTemporalStabilityLoss = "freezeTemporalStabilityLoss";
        public const string BraceNextDisplacement = "braceNextDisplacement";
        public const string SenseTemporalStorm = "senseTemporalStorm";
        public const string DeferSpellCorruptionCost = "deferSpellCorruptionCost";
        public const string SeparateTarget = "separateTarget";
        // Planned Picking effects.
        public const string MarkTarget = "markTarget";
        public const string WeakPointStrike = "weakPointStrike";
        public const string PrecisionBlockStrike = "precisionBlockStrike";
        // Planned Beating effects.
        public const string ForcePulse = "forcePulse";
        public const string Shockwave = "shockwave";
        public const string PressureBlast = "pressureBlast";
        public const string StaggerArea = "staggerArea";
        // Planned Twining effects.
        public const string TetherEntity = "tetherEntity";
        public const string LinkEntities = "linkEntities";
        public const string BindEntityToArea = "bindEntityToArea";
        public const string CharmEntity = "charmEntity";
        public const string CommandEntity = "commandEntity";
        // Planned Darning effects.
        public const string RepairBlock = "repairBlock";
        public const string RepairBlockArea = "repairBlockArea";
        public const string CloseRift = "closeRift";
        public const string RestoreStructure = "restoreStructure";
        public const string HealOverTime = "healOverTime";
        // Planned Backstitching effects.
        public const string CounterNextHostileEffect = "counterNextHostileEffect";
        public const string ReflectProjectile = "reflectProjectile";
        public const string RewindEntityPosition = "rewindEntityPosition";
        public const string UndoRecentEffect = "undoRecentEffect";
        public const string CancelActiveEffect = "cancelActiveEffect";
        // Planned Hemming effects.
        public const string CreateWardArea = "createWardArea";
        public const string CreateBarrier = "createBarrier";
        public const string CreateContainmentArea = "createContainmentArea";
        public const string CreateBoundaryLine = "createBoundaryLine";
        public const string CreateAntiSpreadArea = "createAntiSpreadArea";
        // Planned Carding effects.
        public const string DetectBlocks = "detectBlocks";
        public const string DetectEntities = "detectEntities";
        public const string DetectRustTraces = "detectRustTraces";
        public const string IdentifyActiveEffects = "identifyActiveEffects";
        public const string ReadGlyphs = "readGlyphs";
        public const string AlignNextSpell = "alignNextSpell";
        // Planned Scouring effects.
        public const string PurgeTimedEffects = "purgeTimedEffects";
        public const string StripEntityBuffs = "stripEntityBuffs";
        public const string CleanseContamination = "cleanseContamination";
        public const string UnravelDamage = "unravelDamage";
        public const string LifestealEntity = "lifestealEntity";
        public const string DestroyCorruptedMatter = "destroyCorruptedMatter";
        // Planned Fulling effects.
        public const string ConvertBlock = "convertBlock";
        public const string ConvertHeldItem = "convertHeldItem";
        public const string HardenMaterial = "hardenMaterial";
        public const string HeatMaterial = "heatMaterial";
        public const string CoolMaterial = "coolMaterial";
        public const string AccelerateCraftState = "accelerateCraftState";
        // Planned Spinning effects.
        public const string SummonTemporaryEntity = "summonTemporaryEntity";
        public const string SummonTemporaryItem = "summonTemporaryItem";
        public const string SummonTemporaryProjectile = "summonTemporaryProjectile";
        public const string SummonTemporaryConstruct = "summonTemporaryConstruct";
        // Planned Grafting effects.
        public const string ModifyCropGrowth = "modifyCropGrowth";
        public const string ModifyFarmlandNutrients = "modifyFarmlandNutrients";
        public const string ModifySatiety = "modifySatiety";
        public const string CreateTemporaryFood = "createTemporaryFood";
        public const string ModifyAnimalFertility = "modifyAnimalFertility";
        public const string VitalityOverTime = "vitalityOverTime";
        // Planned Scutching effects.
        public const string MineBlock = "mineBlock";
        public const string ExcavateBlocks = "excavateBlocks";
        public const string ExtractOre = "extractOre";
        public const string HarvestBlocks = "harvestBlocks";
        // Planned Tensioning effects.
        public const string ChangeWeather = "changeWeather";
        public const string ChangeTemperatureArea = "changeTemperatureArea";
        public const string CallLightning = "callLightning";
        public const string ChangeEnvironmentalPressure = "changeEnvironmentalPressure";
        public const string StormPulse = "stormPulse";

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
            PlaySound,
            ModifyTemporalStability,
            StabilizeArea,
            AnchorEntity,
            AnchorBlock,
            PreventDisplacement,
            ModifyCorruptionGain,
            TeleportToTarget,
            TeleportEntityToCaster,
            TeleportEntityToPosition,
            SwapPositions,
            MoveDroppedItem,
            MoveBlockEntityContents,
            PushEntity,
            ReleaseTarget,
            BreakBinding,
            OpenLock,
            OpenPassage,
            CreateRift,
            FreezeTemporalStabilityLoss,
            BraceNextDisplacement,
            SenseTemporalStorm,
            DeferSpellCorruptionCost,
            SeparateTarget,
            MarkTarget,
            WeakPointStrike,
            PrecisionBlockStrike,
            ForcePulse,
            Shockwave,
            PressureBlast,
            StaggerArea,
            TetherEntity,
            LinkEntities,
            BindEntityToArea,
            CharmEntity,
            CommandEntity,
            RepairBlock,
            RepairBlockArea,
            CloseRift,
            RestoreStructure,
            HealOverTime,
            CounterNextHostileEffect,
            ReflectProjectile,
            RewindEntityPosition,
            UndoRecentEffect,
            CancelActiveEffect,
            CreateWardArea,
            CreateBarrier,
            CreateContainmentArea,
            CreateBoundaryLine,
            CreateAntiSpreadArea,
            DetectBlocks,
            DetectEntities,
            DetectRustTraces,
            IdentifyActiveEffects,
            ReadGlyphs,
            AlignNextSpell,
            PurgeTimedEffects,
            StripEntityBuffs,
            CleanseContamination,
            UnravelDamage,
            LifestealEntity,
            DestroyCorruptedMatter,
            ConvertBlock,
            ConvertHeldItem,
            HardenMaterial,
            HeatMaterial,
            CoolMaterial,
            AccelerateCraftState,
            SummonTemporaryEntity,
            SummonTemporaryItem,
            SummonTemporaryProjectile,
            SummonTemporaryConstruct,
            ModifyCropGrowth,
            ModifyFarmlandNutrients,
            ModifySatiety,
            CreateTemporaryFood,
            ModifyAnimalFertility,
            VitalityOverTime,
            MineBlock,
            ExcavateBlocks,
            ExtractOre,
            HarvestBlocks,
            ChangeWeather,
            ChangeTemperatureArea,
            CallLightning,
            ChangeEnvironmentalPressure,
            StormPulse
        };

        public static readonly HashSet<string> Recognized = new(StringComparer.OrdinalIgnoreCase)
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
            PlaySound,
            ModifyTemporalStability,
            StabilizeArea,
            AnchorEntity,
            AnchorBlock,
            PreventDisplacement,
            ModifyCorruptionGain,
            TeleportToTarget,
            TeleportEntityToCaster,
            TeleportEntityToPosition,
            SwapPositions,
            MoveDroppedItem,
            MoveBlockEntityContents,
            PushEntity,
            ReleaseTarget,
            BreakBinding,
            OpenLock,
            OpenPassage,
            CreateRift,
            FreezeTemporalStabilityLoss,
            BraceNextDisplacement,
            SenseTemporalStorm,
            DeferSpellCorruptionCost,
            SeparateTarget,
            MarkTarget,
            WeakPointStrike,
            PrecisionBlockStrike,
            ForcePulse,
            Shockwave,
            PressureBlast,
            StaggerArea,
            TetherEntity,
            LinkEntities,
            BindEntityToArea,
            CharmEntity,
            CommandEntity,
            RepairBlock,
            RepairBlockArea,
            CloseRift,
            RestoreStructure,
            HealOverTime,
            CounterNextHostileEffect,
            ReflectProjectile,
            RewindEntityPosition,
            UndoRecentEffect,
            CancelActiveEffect,
            CreateWardArea,
            CreateBarrier,
            CreateContainmentArea,
            CreateBoundaryLine,
            CreateAntiSpreadArea,
            DetectBlocks,
            DetectEntities,
            DetectRustTraces,
            IdentifyActiveEffects,
            ReadGlyphs,
            AlignNextSpell,
            PurgeTimedEffects,
            StripEntityBuffs,
            CleanseContamination,
            UnravelDamage,
            LifestealEntity,
            DestroyCorruptedMatter,
            ConvertBlock,
            ConvertHeldItem,
            HardenMaterial,
            HeatMaterial,
            CoolMaterial,
            AccelerateCraftState,
            SummonTemporaryEntity,
            SummonTemporaryItem,
            SummonTemporaryProjectile,
            SummonTemporaryConstruct,
            ModifyCropGrowth,
            ModifyFarmlandNutrients,
            ModifySatiety,
            CreateTemporaryFood,
            ModifyAnimalFertility,
            VitalityOverTime,
            MineBlock,
            ExcavateBlocks,
            ExtractOre,
            HarvestBlocks,
            ChangeWeather,
            ChangeTemperatureArea,
            CallLightning,
            ChangeEnvironmentalPressure,
            StormPulse
        };

        public static bool IsSupported(string? effectType)
        {
            return !string.IsNullOrWhiteSpace(effectType) && Supported.Contains(effectType);
        }

        public static string? Normalize(string? effectType)
        {
            if (string.IsNullOrWhiteSpace(effectType))
            {
                return null;
            }

            var trimmed = effectType.Trim();
            return Recognized.FirstOrDefault(known => string.Equals(known, trimmed, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal sealed class SpellEffectDefinition
    {
        public string Type { get; set; } = string.Empty;

        public double Range { get; set; }

        public double Radius { get; set; }

        public float Amount { get; set; }

        public float SecondaryAmount { get; set; }

        public string Mode { get; set; } = string.Empty;

        public string StatCategory { get; set; } = string.Empty;

        public string ModifierCode { get; set; } = string.Empty;

        public string StatusCode { get; set; } = string.Empty;

        public string BlockCode { get; set; } = string.Empty;

        public string ResultBlockCode { get; set; } = string.Empty;

        public string ItemCode { get; set; } = string.Empty;

        public string EntityCode { get; set; } = string.Empty;

        public string WeatherType { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public List<string> BlockCodes { get; set; } = new();

        public List<string> EntityCodes { get; set; } = new();

        public List<string> ItemCodes { get; set; } = new();

        public int MaxBlocks { get; set; }

        public int MaxEntities { get; set; }

        public bool IncludeCaster { get; set; }

        public bool AllowNoTargets { get; set; }

        public int MaxTargets { get; set; }

        public int DurabilityAmount { get; set; }

        public bool AllowBrokenItems { get; set; }

        public bool PreserveDrops { get; set; }

        public bool ReplaceExisting { get; set; }

        public bool AllowPlayers { get; set; }

        public bool AllowHostiles { get; set; } = true;

        public bool AllowPassives { get; set; } = true;

        public bool RequireContainer { get; set; }

        public bool RequireBlockEntity { get; set; }

        public bool ClearPositive { get; set; }

        public bool ClearNegative { get; set; }

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

        public double DespawnAfterSeconds { get; set; }

        public double TemperatureDelta { get; set; }

        public double StabilityAmount { get; set; }
    }

    internal sealed class SpellDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Hidden { get; set; }

        public bool TestingOnly { get; set; }

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
            var unsupportedEffectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    if (string.Equals(validationError, "testing-only spell skipped", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (TryCollectUnsupportedEffectTypes(validationError, unsupportedEffectTypes))
                    {
                        continue;
                    }

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

            if (unsupportedEffectTypes.Count > 0)
            {
                api.Logger.Warning("[TheRustweave] Loaded spells referenced unsupported effect type(s): {0}", string.Join(", ", unsupportedEffectTypes.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)));
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
            normalized.School = SpellSchoolTypes.Normalize(normalized.School);
            normalized.Category = (normalized.Category ?? string.Empty).Trim();
            normalized.TargetType = SpellTargetTypes.Normalize(normalized.TargetType);
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

            if (normalized.TestingOnly && !RustweaveConstants.AllSpellsLearnedByDefaultForTesting)
            {
                validationError = "testing-only spell skipped";
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

            if (SpellTargetTypes.RequiresLookRange(normalized.TargetType)
                && GetEffectiveLookTargetRange(normalized) <= 0)
            {
                validationError = "range must be greater than zero for look targets";
                return false;
            }

            if (string.Equals(normalized.TargetType, SpellTargetTypes.SelfArea, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized.TargetType, SpellTargetTypes.LookArea, StringComparison.OrdinalIgnoreCase))
            {
                var normalizedRadius = Math.Max(0d, normalized.Radius);
                if (normalized.Effects.Count > 0)
                {
                    normalizedRadius = Math.Max(normalizedRadius, normalized.Effects.Where(effect => effect != null).Select(effect => Math.Max(0d, effect.Radius)).DefaultIfEmpty(0d).Max());
                }

                if (normalizedRadius <= 0)
                {
                    validationError = "radius must be greater than zero for area targets";
                    return false;
                }
            }

            if (normalized.Effects.Count == 0)
            {
                validationError = "no effects were defined";
                return false;
            }

            var unsupportedEffectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var effectIndex = 0; effectIndex < normalized.Effects.Count; effectIndex++)
            {
                var effect = normalized.Effects[effectIndex];
                if (!TryNormalizeEffect(effect, normalized, out var effectError))
                {
                    if (TryGetUnsupportedEffectType(effectError, out var unsupportedEffectType))
                    {
                        unsupportedEffectTypes.Add(unsupportedEffectType);
                        continue;
                    }

                    validationError = $"effect #{effectIndex} was ignored: {effectError}";
                    return false;
                }
            }

            if (unsupportedEffectTypes.Count > 0)
            {
                validationError = $"unsupported effect type(s): {string.Join(", ", unsupportedEffectTypes.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))}";
                return false;
            }

            if (string.Equals(normalized.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase))
            {
                var effectiveRange = GetEffectiveLookEntityRange(normalized);
                if (effectiveRange <= 0)
                {
                    validationError = "range must be greater than zero for lookEntity, lookPlayer, lookNonPlayerEntity, lookDroppedItem, lookBlock, lookBlockEntity, lookContainer, lookPosition, or lookArea";
                    return false;
                }
            }

            return true;
        }

        private static bool TryNormalizeEffect(SpellEffectDefinition effect, SpellDefinition spell, out string validationError)
        {
            validationError = string.Empty;
            effect.Type = (effect.Type ?? string.Empty).Trim();
            effect.Mode = (effect.Mode ?? string.Empty).Trim();
            effect.StatCategory = (effect.StatCategory ?? string.Empty).Trim();
            effect.ModifierCode = (effect.ModifierCode ?? string.Empty).Trim();
            effect.StatusCode = (effect.StatusCode ?? string.Empty).Trim();
            effect.BlockCode = (effect.BlockCode ?? string.Empty).Trim();
            effect.ResultBlockCode = (effect.ResultBlockCode ?? string.Empty).Trim();
            effect.ItemCode = (effect.ItemCode ?? string.Empty).Trim();
            effect.EntityCode = (effect.EntityCode ?? string.Empty).Trim();
            effect.WeatherType = (effect.WeatherType ?? string.Empty).Trim();
            effect.Message = (effect.Message ?? string.Empty).Trim();
            effect.ParticleCode = (effect.ParticleCode ?? string.Empty).Trim();
            effect.Sound = (effect.Sound ?? string.Empty).Trim();
            effect.BlockCodes = NormalizeStringList(effect.BlockCodes);
            effect.EntityCodes = NormalizeStringList(effect.EntityCodes);
            effect.ItemCodes = NormalizeStringList(effect.ItemCodes);

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

            if (!SpellEffectTypes.IsSupported(effect.Type))
            {
                validationError = $"effect type '{effect.Type}' is not implemented yet";
                return false;
            }

            switch (effect.Type)
            {
                case SpellEffectTypes.None:
                    return true;
                case SpellEffectTypes.HealTarget:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "healTarget requires targetType self, lookEntity, lookPlayer, or lookNonPlayerEntity";
                        return false;
                    }

                    if (effect.HealthAmount <= 0)
                    {
                        validationError = "healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.HealArea:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition))
                    {
                        validationError = "healArea requires a self, entity, block, position, or area target";
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "damageRayEntity requires targetType lookEntity, lookPlayer, or lookNonPlayerEntity";
                        return false;
                    }

                    if (effect.DamageAmount <= 0)
                    {
                        validationError = "damageAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DamageArea:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition))
                    {
                        validationError = "damageArea requires a self, entity, block, position, or area target";
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "shieldTarget requires targetType self, lookEntity, lookPlayer, or lookNonPlayerEntity";
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "stunTarget requires targetType lookEntity, lookPlayer, or lookNonPlayerEntity";
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = $"{effect.Type} requires targetType lookEntity, lookPlayer, or lookNonPlayerEntity";
                        return false;
                    }

                    if (effect.Force <= 0)
                    {
                        validationError = "force must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ProjectileEntity:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.LookDroppedItem, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea))
                    {
                        validationError = "projectileEntity requires an entity, block, position, or area target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.WeakenTarget:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
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
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea))
                    {
                        validationError = "teleportForward requires targetType self, lookBlock, lookPosition, or area";
                        return false;
                    }

                    if (effect.TeleportDistance <= 0)
                    {
                        validationError = "teleportDistance must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.CorruptionTransfer:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "corruptionTransfer requires targetType self, lookEntity, lookPlayer, or lookNonPlayerEntity";
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
                case SpellEffectTypes.CreateRift:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.LookArea, SpellTargetTypes.SelfArea))
                    {
                        validationError = "createRift requires a block, position, or area target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (GetEffectiveRadius(spell, effect) <= 0)
                    {
                        validationError = "radius must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.FreezeTemporalStabilityLoss:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
                    {
                        validationError = "freezeTemporalStabilityLoss requires targetType self";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.BraceNextDisplacement:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
                    {
                        validationError = "braceNextDisplacement requires targetType self";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.SenseTemporalStorm:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
                    {
                        validationError = "senseTemporalStorm requires targetType self";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DeferSpellCorruptionCost:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self))
                    {
                        validationError = "deferSpellCorruptionCost requires targetType self";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.Amount <= 0f || effect.Amount >= 1f)
                    {
                        validationError = "amount must be greater than zero and less than one";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ModifyTemporalStability:
                case SpellEffectTypes.ModifyCorruptionGain:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookPosition, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = $"{effect.Type} requires a self, area, block, position, or entity target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.Amount == 0f && effect.StabilityAmount == 0d && effect.CorruptionAmount == 0)
                    {
                        validationError = "amount, stabilityAmount, or corruptionAmount is required";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.StabilizeArea:
                case SpellEffectTypes.CreateWardArea:
                case SpellEffectTypes.CreateBarrier:
                case SpellEffectTypes.CreateContainmentArea:
                case SpellEffectTypes.CreateBoundaryLine:
                case SpellEffectTypes.CreateAntiSpreadArea:
                case SpellEffectTypes.ChangeTemperatureArea:
                case SpellEffectTypes.ChangeEnvironmentalPressure:
                case SpellEffectTypes.StormPulse:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookPosition, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer))
                    {
                        validationError = $"{effect.Type} requires a self area, targeted area, or block target";
                        return false;
                    }

                    if (!RequireRadius(spell, effect, out validationError) && !string.Equals(effect.Type, SpellEffectTypes.ChangeTemperatureArea, StringComparison.OrdinalIgnoreCase) && !string.Equals(effect.Type, SpellEffectTypes.ChangeEnvironmentalPressure, StringComparison.OrdinalIgnoreCase) && !string.Equals(effect.Type, SpellEffectTypes.StormPulse, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.CallLightning:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookPosition, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer))
                    {
                        validationError = "callLightning requires a self area, targeted area, or block target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.AnchorEntity:
                case SpellEffectTypes.PreventDisplacement:
                case SpellEffectTypes.CounterNextHostileEffect:
                case SpellEffectTypes.ReflectProjectile:
                case SpellEffectTypes.RewindEntityPosition:
                case SpellEffectTypes.UndoRecentEffect:
                case SpellEffectTypes.CancelActiveEffect:
                case SpellEffectTypes.ReleaseTarget:
                case SpellEffectTypes.BreakBinding:
                case SpellEffectTypes.SeparateTarget:
                case SpellEffectTypes.PurgeTimedEffects:
                case SpellEffectTypes.StripEntityBuffs:
                case SpellEffectTypes.CleanseContamination:
                case SpellEffectTypes.UnravelDamage:
                case SpellEffectTypes.IdentifyActiveEffects:
                case SpellEffectTypes.AlignNextSpell:
                case SpellEffectTypes.HealOverTime:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.LookArea, SpellTargetTypes.SelfArea, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition))
                    {
                        validationError = $"{effect.Type} requires a self, entity, block, position, or area target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.MarkTarget:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea))
                    {
                        validationError = "markTarget requires a valid entity, block, position, or area target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.WeakPointStrike:
                case SpellEffectTypes.LifestealEntity:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = $"{effect.Type} requires an entity target";
                        return false;
                    }

                    if (effect.DamageAmount <= 0 && effect.HealthAmount <= 0)
                    {
                        validationError = "damageAmount or healthAmount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.TetherEntity:
                case SpellEffectTypes.BindEntityToArea:
                case SpellEffectTypes.CharmEntity:
                case SpellEffectTypes.CommandEntity:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.Self))
                    {
                        validationError = $"{effect.Type} requires a creature or entity target";
                        return false;
                    }

                    if (spell.TargetType.Equals(SpellTargetTypes.LookPlayer, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = $"{effect.Type} cannot target players";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.Radius <= 0 && spell.TargetType.Equals(SpellTargetTypes.LookArea, StringComparison.OrdinalIgnoreCase))
                    {
                        validationError = "radius must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.LinkEntities:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "linkEntities requires targetType lookEntity, lookPlayer, or lookNonPlayerEntity";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.VitalityOverTime:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookEntity, SpellTargetTypes.LookNonPlayerEntity))
                    {
                        validationError = "vitalityOverTime requires a living target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    if (effect.HealthAmount <= 0 && effect.Amount <= 0f)
                    {
                        validationError = "healthAmount or amount must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.DetectBlocks:
                case SpellEffectTypes.DetectEntities:
                case SpellEffectTypes.DetectRustTraces:
                case SpellEffectTypes.ReadGlyphs:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookPosition, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer))
                    {
                        validationError = $"{effect.Type} requires a self, area, or block target";
                        return false;
                    }

                    if (effect.Radius <= 0 && !HasTargetType(spell.TargetType, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer))
                    {
                        validationError = "radius must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ModifyCropGrowth:
                case SpellEffectTypes.ModifyFarmlandNutrients:
                case SpellEffectTypes.ModifyAnimalFertility:
                    if (string.Equals(effect.Type, SpellEffectTypes.ModifyAnimalFertility, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookNonPlayerEntity))
                        {
                            validationError = "modifyAnimalFertility requires a creature target";
                            return false;
                        }

                        if (effect.DurationSeconds <= 0)
                        {
                            validationError = "durationSeconds must be greater than zero";
                            return false;
                        }

                        if (effect.Amount == 0f && effect.SecondaryAmount == 0f)
                        {
                            validationError = "amount is required";
                            return false;
                        }

                        return true;
                    }

                    if (string.Equals(effect.Type, SpellEffectTypes.CreateTemporaryFood, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.HeldItem, SpellTargetTypes.Inventory))
                        {
                            validationError = "createTemporaryFood requires targetType self, heldItem, or inventory";
                            return false;
                        }

                        if (!RequireCodeField(effect.ItemCode, "itemCode", out validationError))
                        {
                            return false;
                        }

                        if (effect.Amount < 0f)
                        {
                            validationError = "amount must be nonnegative";
                            return false;
                        }

                        return true;
                    }

                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea, SpellTargetTypes.LookPosition, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer))
                    {
                        validationError = $"{effect.Type} requires a block, position, or area target";
                        return false;
                    }

                    if (effect.DurationSeconds <= 0)
                    {
                        validationError = "durationSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.SummonTemporaryEntity:
                case SpellEffectTypes.SummonTemporaryProjectile:
                case SpellEffectTypes.SummonTemporaryConstruct:
                    if (!RequireCodeField(effect.EntityCode, "entityCode", out validationError) && string.IsNullOrWhiteSpace(effect.BlockCode))
                    {
                        return false;
                    }

                    if (effect.DespawnAfterSeconds <= 0)
                    {
                        validationError = "despawnAfterSeconds must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.SummonTemporaryItem:
                case SpellEffectTypes.ConvertHeldItem:
                    if (!RequireCodeField(effect.ItemCode, "itemCode", out validationError))
                    {
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ModifySatiety:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookEntity))
                    {
                        validationError = "modifySatiety requires targetType self, lookPlayer, or lookEntity";
                        return false;
                    }

                    if (effect.Amount == 0f)
                    {
                        validationError = "amount must be nonzero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.MineBlock:
                case SpellEffectTypes.ExcavateBlocks:
                case SpellEffectTypes.ExtractOre:
                case SpellEffectTypes.HarvestBlocks:
                case SpellEffectTypes.RepairBlock:
                case SpellEffectTypes.RepairBlockArea:
                case SpellEffectTypes.CloseRift:
                case SpellEffectTypes.RestoreStructure:
                case SpellEffectTypes.ConvertBlock:
                case SpellEffectTypes.HardenMaterial:
                case SpellEffectTypes.HeatMaterial:
                case SpellEffectTypes.CoolMaterial:
                case SpellEffectTypes.AccelerateCraftState:
                case SpellEffectTypes.PrecisionBlockStrike:
                case SpellEffectTypes.OpenPassage:
                case SpellEffectTypes.OpenLock:
                case SpellEffectTypes.AnchorBlock:
                case SpellEffectTypes.MoveBlockEntityContents:
                case SpellEffectTypes.DestroyCorruptedMatter:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.LookArea, SpellTargetTypes.SelfArea))
                    {
                        validationError = $"{effect.Type} requires a block, container, or area target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.MoveDroppedItem:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookDroppedItem))
                    {
                        validationError = "moveDroppedItem requires targetType lookDroppedItem";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.TeleportToTarget:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.Self, SpellTargetTypes.LookBlock, SpellTargetTypes.LookBlockEntity, SpellTargetTypes.LookContainer, SpellTargetTypes.LookPosition, SpellTargetTypes.SelfArea, SpellTargetTypes.LookArea))
                    {
                        validationError = "teleportToTarget requires a self, block, position, or area target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.TeleportEntityToCaster:
                case SpellEffectTypes.TeleportEntityToPosition:
                case SpellEffectTypes.SwapPositions:
                case SpellEffectTypes.PushEntity:
                    if (!HasTargetType(spell.TargetType, SpellTargetTypes.LookEntity, SpellTargetTypes.LookPlayer, SpellTargetTypes.LookNonPlayerEntity, SpellTargetTypes.Self))
                    {
                        validationError = $"{effect.Type} requires an entity target";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ForcePulse:
                case SpellEffectTypes.Shockwave:
                case SpellEffectTypes.PressureBlast:
                case SpellEffectTypes.StaggerArea:
                    if (GetEffectiveRadius(spell, effect) <= 0)
                    {
                        validationError = "radius must be greater than zero";
                        return false;
                    }

                    return true;
                case SpellEffectTypes.ChangeWeather:
                    if (string.IsNullOrWhiteSpace(effect.WeatherType) && string.IsNullOrWhiteSpace(effect.Mode))
                    {
                        validationError = "weatherType or mode is required";
                        return false;
                    }

                    return true;
                default:
                    validationError = $"unsupported effect type '{effect.Type}'";
                    return false;
            }
        }

        private static bool TryGetUnsupportedEffectType(string validationError, out string effectType)
        {
            effectType = string.Empty;
            if (string.IsNullOrWhiteSpace(validationError))
            {
                return false;
            }

            const string marker = "unsupported effect type '";
            var markerIndex = validationError.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return false;
            }

            var startIndex = markerIndex + marker.Length;
            var endIndex = validationError.IndexOf('\'', startIndex);
            if (endIndex <= startIndex)
            {
                return false;
            }

            effectType = validationError.Substring(startIndex, endIndex - startIndex).Trim();
            return !string.IsNullOrWhiteSpace(effectType);
        }

        private static bool TryCollectUnsupportedEffectTypes(string validationError, ISet<string> collection)
        {
            if (string.IsNullOrWhiteSpace(validationError) || collection == null)
            {
                return false;
            }

            const string prefix = "unsupported effect type(s):";
            if (!validationError.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var remainder = validationError.Substring(prefix.Length).Trim();
            if (string.IsNullOrWhiteSpace(remainder))
            {
                return true;
            }

            foreach (var value in remainder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = value.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    collection.Add(trimmed);
                }
            }

            return true;
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

        private static bool HasTargetType(string targetType, params string[] allowedTargetTypes)
        {
            return allowedTargetTypes.Any(allowedTargetType => string.Equals(targetType, allowedTargetType, StringComparison.OrdinalIgnoreCase));
        }

        private static bool RequireDuration(SpellEffectDefinition effect, out string validationError)
        {
            if (effect.DurationSeconds <= 0)
            {
                validationError = "durationSeconds must be greater than zero";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        private static bool RequireRadius(SpellDefinition spell, SpellEffectDefinition effect, out string validationError)
        {
            if (GetEffectiveRadius(spell, effect) <= 0)
            {
                validationError = "radius must be greater than zero";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        private static bool RequireAmount(SpellEffectDefinition effect, out string validationError)
        {
            if (Math.Abs(effect.Amount) <= 0f && Math.Abs(effect.SecondaryAmount) <= 0f && Math.Abs(effect.StabilityAmount) <= 0d && Math.Abs(effect.CorruptionAmount) <= 0 && Math.Abs(effect.DamageAmount) <= 0f && Math.Abs(effect.HealthAmount) <= 0f && Math.Abs(effect.DamagePerTick) <= 0f && Math.Abs(effect.Force) <= 0f)
            {
                validationError = "an amount, damage, health, stability, or force value is required";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        private static bool RequireCodeField(string value, string label, out string validationError)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                validationError = $"{label} is required";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        private static bool ValidateAllowedTargets(SpellDefinition spell, string errorMessage, out string validationError, params string[] allowedTargets)
        {
            if (!HasTargetType(spell.TargetType, allowedTargets))
            {
                validationError = errorMessage;
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        private static bool RequireCreatureTarget(string targetType, out string validationError)
        {
            if (string.Equals(targetType, SpellTargetTypes.LookPlayer, StringComparison.OrdinalIgnoreCase))
            {
                validationError = "that spell requires a creature or NPC target";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        internal static bool IsDroppedItemEntity(Entity? entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity is EntityItem)
            {
                return true;
            }

            var type = entity.GetType();
            var typeName = type.Name ?? string.Empty;
            var fullName = type.FullName ?? string.Empty;
            return typeName.Contains("Item", StringComparison.OrdinalIgnoreCase)
                && typeName.Contains("Entity", StringComparison.OrdinalIgnoreCase)
                || fullName.Contains("EntityItem", StringComparison.OrdinalIgnoreCase)
                || fullName.Contains("DroppedItem", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsPlayerEntity(Entity? entity)
        {
            return entity is EntityPlayer;
        }

        internal static bool TryGetBlockEntityInventory(BlockEntity? blockEntity, out IInventory? inventory)
        {
            inventory = null;
            if (blockEntity == null)
            {
                return false;
            }

            var type = blockEntity.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var memberName in new[] { "Inventory", "inventory" })
            {
                var property = type.GetProperty(memberName, flags);
                if (property?.GetValue(blockEntity) is IInventory propertyInventory)
                {
                    inventory = propertyInventory;
                    return true;
                }

                var field = type.GetField(memberName, flags);
                if (field?.GetValue(blockEntity) is IInventory fieldInventory)
                {
                    inventory = fieldInventory;
                    return true;
                }
            }

            return false;
        }

        internal static string GetBlockEntityDisplayName(BlockEntity? blockEntity, BlockPos? blockPos)
        {
            if (blockEntity?.Block?.Code != null)
            {
                var path = blockEntity.Block.Code.Path;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return path;
                }
            }

            if (blockEntity != null)
            {
                var typeName = blockEntity.GetType().Name;
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    return typeName;
                }
            }

            return blockPos != null ? "Block Entity" : "Block Entity";
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

            if (string.Equals(spell.TargetType, SpellTargetTypes.SelfArea, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookArea, StringComparison.OrdinalIgnoreCase))
            {
                return SpellPreviewModes.Area;
            }

            if (hasProjectile)
            {
                return SpellPreviewModes.Projectile;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.LookEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookPlayer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookDroppedItem, StringComparison.OrdinalIgnoreCase))
            {
                return hasArea ? SpellPreviewModes.Area : SpellPreviewModes.Entity;
            }

            if (string.Equals(spell.TargetType, SpellTargetTypes.LookBlock, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookBlockEntity, StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.TargetType, SpellTargetTypes.LookContainer, StringComparison.OrdinalIgnoreCase))
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

        internal static string ResolveTargetedWarningType(SpellDefinition spell)
        {
            if (spell == null)
            {
                return SpellWarningTypes.Neutral;
            }

            if (spell.Effects.Any(effect => effect != null && IsHarmfulEffectType(effect.Type)))
            {
                return SpellWarningTypes.Hostile;
            }

            if (spell.Effects.Any(effect => effect != null && IsBeneficialEffectType(effect.Type)))
            {
                return SpellWarningTypes.Beneficial;
            }

            return SpellWarningTypes.Neutral;
        }

        internal static string GetSpellSchoolCategoryDisplayName(SpellDefinition? spell)
        {
            if (spell == null)
            {
                return string.Empty;
            }

            static string NormalizeDisplayName(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return string.Empty;
                }

                var text = value.Trim().Replace('_', ' ').Replace('-', ' ');
                var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return string.Empty;
                }

                return string.Join(" ", parts.Select(part => part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
            }

            var school = NormalizeDisplayName(spell.School);
            var category = NormalizeDisplayName(spell.Category);
            if (!string.IsNullOrWhiteSpace(school) && !string.IsNullOrWhiteSpace(category) && !string.Equals(school, category, StringComparison.OrdinalIgnoreCase))
            {
                return $"{school} / {category}";
            }

            return !string.IsNullOrWhiteSpace(school) ? school : category;
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
                || string.Equals(effectType, SpellEffectTypes.SlowTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, SpellEffectTypes.RootTarget, StringComparison.OrdinalIgnoreCase)
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
                sapi.Logger.Warning("[TheRustweave] Self target resolution failed for spell '{0}' caster '{1}' targetType '{2}': caster is not available", spell.Code, caster?.PlayerUID ?? "unknown", SpellTargetTypes.Self);
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
                if (string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryResolveSelfTarget(caster, spell, out lockedTarget, out failureReason))
                    {
                        return false;
                    }
                }
                else if (!TryResolveTarget(caster, spell, out lockedTarget, out failureReason))
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

        private bool TryResolveSelfTarget(IServerPlayer caster, SpellDefinition spell, out SpellTargetContext target, out string failureReason)
        {
            target = new SpellTargetContext();
            failureReason = string.Empty;

            if (caster?.Entity == null || !caster.Entity.Alive)
            {
                failureReason = "caster is not available";
                return false;
            }

            if (string.Equals(spell.Code, "still-thread", StringComparison.OrdinalIgnoreCase))
            {
                sapi.Logger.Debug("[TheRustweave] Resolving self target for spell still-thread: caster={0}", caster.PlayerName ?? caster.PlayerUID);
            }

            target.Entity = caster.Entity;
            target.Position = caster.Entity.Pos.XYZ;
            target.TargetName = caster.Entity.GetName() ?? spell.Code;
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
                    return TryResolveSelfTarget(caster, spell, out target, out failureReason);
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

                    if (repairSlot == null || repairSlot.Itemstack == null)
                    {
                        failureReason = "No valid repair target was found.";
                        return false;
                    }

                    target.ItemSlot = repairSlot;
                    target.Entity = entity;
                    target.TargetName = repairSlot.Itemstack.GetName() ?? spell.Code;
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
                case SpellTargetTypes.LookPlayer:
                    if (!TryResolveLookEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var playerSelection, out failureReason, candidate => candidate is EntityPlayer))
                    {
                        return false;
                    }

                    target.Entity = playerSelection.Entity;
                    target.Position = playerSelection.HitPosition;
                    target.TargetName = playerSelection.Entity?.GetName() ?? "Player";
                    return true;
                case SpellTargetTypes.LookNonPlayerEntity:
                    if (!TryResolveLookEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var nonPlayerSelection, out failureReason, candidate => candidate != null && !SpellRegistry.IsPlayerEntity(candidate) && !SpellRegistry.IsDroppedItemEntity(candidate)))
                    {
                        return false;
                    }

                    target.Entity = nonPlayerSelection.Entity;
                    target.Position = nonPlayerSelection.HitPosition;
                    target.TargetName = nonPlayerSelection.Entity?.GetName() ?? "Creature/NPC";
                    return true;
                case SpellTargetTypes.LookDroppedItem:
                    if (!TryResolveLookEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var droppedItemSelection, out failureReason, candidate => SpellRegistry.IsDroppedItemEntity(candidate)))
                    {
                        return false;
                    }

                    target.Entity = droppedItemSelection.Entity;
                    target.Position = droppedItemSelection.HitPosition;
                    target.TargetName = droppedItemSelection.Entity?.GetName() ?? "Dropped Item";
                    return true;
                case SpellTargetTypes.LookBlock:
                    if (!TryResolveLookBlockTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var blockPos, out var blockPosition, out failureReason))
                    {
                        return false;
                    }

                    target.Position = blockPosition;
                    target.BlockPos = blockPos ?? new BlockPos((int)Math.Floor(blockPosition.X), (int)Math.Floor(blockPosition.Y - 1d), (int)Math.Floor(blockPosition.Z), entity.Pos.Dimension);
                    target.TargetName = GetBlockDisplayName(target.BlockPos);
                    return true;
                case SpellTargetTypes.LookBlockEntity:
                    if (!TryResolveLookBlockEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), false, out var blockEntityPos, out var blockEntityPosition, out var blockEntity, out failureReason))
                    {
                        return false;
                    }

                    target.Position = blockEntityPosition;
                    target.BlockPos = blockEntityPos;
                    target.BlockEntity = blockEntity;
                    target.TargetName = SpellRegistry.GetBlockEntityDisplayName(blockEntity, blockEntityPos);
                    return true;
                case SpellTargetTypes.LookContainer:
                    if (!TryResolveLookBlockEntityTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), true, out var containerPos, out var containerPosition, out var containerBlockEntity, out failureReason))
                    {
                        return false;
                    }

                    target.Position = containerPosition;
                    target.BlockPos = containerPos;
                    target.BlockEntity = containerBlockEntity;
                    target.TargetName = SpellRegistry.GetBlockEntityDisplayName(containerBlockEntity, containerPos);
                    return true;
                case SpellTargetTypes.LookPosition:
                    if (!TryResolveLookPositionTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var lookPosition, out failureReason))
                    {
                        return false;
                    }

                    target.Position = lookPosition;
                    target.TargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookPosition);
                    return true;
                case SpellTargetTypes.SelfArea:
                    target.Entity = entity;
                    target.Position = entity.Pos.XYZ;
                    target.TargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.SelfArea);
                    return true;
                case SpellTargetTypes.LookArea:
                    if (!TryResolveLookAreaTarget(caster, SpellRegistry.GetEffectiveLookTargetRange(spell), out var areaPosition, out failureReason))
                    {
                        return false;
                    }

                    target.Position = areaPosition;
                    target.BlockPos = new BlockPos((int)Math.Floor(areaPosition.X), (int)Math.Floor(areaPosition.Y), (int)Math.Floor(areaPosition.Z), entity.Pos.Dimension);
                    target.TargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookArea);
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
                    return TryResolveSelfTarget(caster, spell, out target, out failureReason);
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

                    if (repairSlot == null || repairSlot.Itemstack == null)
                    {
                        failureReason = "No valid repair target was found.";
                        return false;
                    }

                    target.ItemSlot = repairSlot;
                    target.Entity = caster.Entity;
                    target.TargetName = repairSlot.Itemstack.GetName() ?? spell.Code;
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
                case SpellTargetTypes.LookPlayer:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookPlayer, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var entity = sapi.World.GetEntityById(castState.LockedEntityId);
                    if (entity is not EntityPlayer playerEntity || !playerEntity.Alive)
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var effectiveRange = SpellRegistry.GetEffectiveLookTargetRange(spell);
                    if (effectiveRange > 0)
                    {
                        var casterPos = caster.Entity.Pos.XYZ;
                        var targetPos = playerEntity.Pos.XYZ;
                        var dx = casterPos.X - targetPos.X;
                        var dy = casterPos.Y - targetPos.Y;
                        var dz = casterPos.Z - targetPos.Z;
                        if ((dx * dx) + (dy * dy) + (dz * dz) > effectiveRange * effectiveRange)
                        {
                            failureReason = "The locked target moved out of range.";
                            return false;
                        }
                    }

                    if (spell.RequiresLineOfSight && !TryHasLineOfSight(caster, playerEntity.Pos.XYZ, playerEntity.EntityId, out failureReason))
                    {
                        return false;
                    }

                    target.Entity = playerEntity;
                    target.Position = playerEntity.Pos.XYZ;
                    target.TargetName = playerEntity.GetName() ?? spell.Code;
                    return true;
                }
                case SpellTargetTypes.LookNonPlayerEntity:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookNonPlayerEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var entity = sapi.World.GetEntityById(castState.LockedEntityId);
                    if (entity == null || !entity.Alive || SpellRegistry.IsPlayerEntity(entity) || SpellRegistry.IsDroppedItemEntity(entity))
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
                case SpellTargetTypes.LookDroppedItem:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookDroppedItem, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = "The locked target is no longer valid.";
                        return false;
                    }

                    var entity = sapi.World.GetEntityById(castState.LockedEntityId);
                    if (!SpellRegistry.IsDroppedItemEntity(entity))
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

                    target.Entity = entity;
                    target.Position = entity.Pos.XYZ;
                    target.TargetName = entity.GetName() ?? "Dropped Item";
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
                case SpellTargetTypes.LookBlockEntity:
                case SpellTargetTypes.LookContainer:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, spell.TargetType, StringComparison.OrdinalIgnoreCase))
                    {
                        failureReason = spell.TargetType == SpellTargetTypes.LookContainer
                            ? "The locked container target is no longer valid."
                            : "The locked block entity target is no longer valid.";
                        return false;
                    }

                    var pos = new BlockPos(castState.LockedBlockX, castState.LockedBlockY, castState.LockedBlockZ, caster.Entity.Pos.Dimension);
                    var blockEntity = sapi.World.BlockAccessor.GetBlockEntity(pos);
                    if (blockEntity == null)
                    {
                        failureReason = spell.TargetType == SpellTargetTypes.LookContainer
                            ? "The locked container target is no longer valid."
                            : "The locked block entity target is no longer valid.";
                        return false;
                    }

                    if (string.Equals(spell.TargetType, SpellTargetTypes.LookContainer, StringComparison.OrdinalIgnoreCase) && !SpellRegistry.TryGetBlockEntityInventory(blockEntity, out _))
                    {
                        failureReason = "That target requires a container.";
                        return false;
                    }

                    target.BlockPos = pos;
                    target.BlockEntity = blockEntity;
                    target.Position = new Vec3d(castState.LockedPosX, castState.LockedPosY, castState.LockedPosZ);
                    target.TargetName = !string.IsNullOrWhiteSpace(castState.LockedTargetName) ? castState.LockedTargetName : SpellRegistry.GetBlockEntityDisplayName(blockEntity, pos);
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
                    target.TargetName = !string.IsNullOrWhiteSpace(castState.LockedTargetName) ? castState.LockedTargetName : SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookPosition);
                    return true;
                }
                case SpellTargetTypes.SelfArea:
                {
                    target.Entity = caster.Entity;
                    target.Position = caster.Entity.Pos.XYZ;
                    target.TargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.SelfArea);
                    return true;
                }
                case SpellTargetTypes.LookArea:
                {
                    if (!castState.HasLockedTarget || !string.Equals(castState.LockedTargetType, SpellTargetTypes.LookArea, StringComparison.OrdinalIgnoreCase))
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
                    target.TargetName = !string.IsNullOrWhiteSpace(castState.LockedTargetName) ? castState.LockedTargetName : SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookArea);
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
                    return TryAppendTeleportForward(gameplayActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.CorruptionTransfer:
                    return TryAppendCorruptionTransfer(ventActions, caster, spell, target, effect, ref corruptionDelta, out failureReason);
                case SpellEffectTypes.SpawnParticles:
                    return TryAppendSpawnParticles(visualActions, caster, target, effect, out failureReason);
                case SpellEffectTypes.PlaySound:
                    return TryAppendPlaySound(visualActions, caster, target, effect, out failureReason);
                default:
                    return TryAppendPlannedEffect(gameplayActions, ventActions, visualActions, caster, state, spell, target, effect, ref corruptionDelta, out failureReason);
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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "damageRayEntity", out failureReason))
            {
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

                      if (candidate is not EntityPlayer && !candidate.IsCreature)
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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "stunTarget", out failureReason))
            {
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
                    if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "projectileEntity", out failureReason))
                    {
                        return false;
                    }

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

              if (!applyToCaster && IsPlayerTargetBlockedByPvp(caster, targetEntity, effectLabel, out failureReason))
              {
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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "weakenTarget", out failureReason))
            {
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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "damageOverTime", out failureReason))
            {
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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "knockbackEntity", out failureReason))
            {
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
                if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, targetEntity))
                {
                    return true;
                }

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

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, "pullEntity", out failureReason))
            {
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
                if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, targetEntity))
                {
                    return true;
                }

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

        private bool TryAppendTeleportForward(List<Func<bool>> gameplayActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;

            if (!TryFindTeleportDestination(caster, target, effect.TeleportDistance, out var destination))
            {
                failureReason = "no safe teleport destination could be found";
                sapi.Logger.Warning("[TheRustweave] teleportForward failed because no safe destination was found for player '{0}' and spell '{1}'.", caster.PlayerUID, spell.Code);
                return false;
            }

            gameplayActions.Add(() =>
            {
                caster.Entity?.TeleportTo(destination);
                return caster.Entity != null;
            });

            return true;
        }

        private bool TryFindTeleportDestination(IServerPlayer caster, SpellTargetContext target, float distance, out Vec3d destination)
        {
            destination = caster.Entity?.Pos.XYZ ?? new Vec3d();
            var entity = caster.Entity;
            if (entity == null || entity.World == null)
            {
                return false;
            }

            Vec3d desired;
            if (target.BlockPos != null)
            {
                desired = new Vec3d(target.BlockPos.X + 0.5, target.BlockPos.Y + 1.0, target.BlockPos.Z + 0.5);
            }
            else if (target.Position != default)
            {
                desired = target.Position;
            }
            else
            {
                var viewVector = entity.Pos.GetViewVector();
                var basePosition = entity.Pos.XYZ;
                desired = new Vec3d(
                    basePosition.X + (viewVector.X * distance),
                    basePosition.Y + (viewVector.Y * distance),
                    basePosition.Z + (viewVector.Z * distance));
            }

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

        private bool TryAppendWeakPointStrike(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            var damage = Math.Max(1f, effect.DamageAmount > 0f ? effect.DamageAmount : effect.Amount);
            if (effect.DamageAmount <= 0f)
            {
                effect.DamageAmount = damage;
            }

            var result = TryAppendDamageRayEntity(gameplayActions, caster, spell, target, effect, out failureReason);
            if (!result)
            {
                return false;
            }

            visualActions.Add(() =>
            {
                var center = target.Entity?.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF8A5137), new Vec3d(center.X - 0.1, center.Y + 0.9, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 1.2, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.35f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendLifeSteal(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!TryAppendDamageRayEntity(gameplayActions, caster, spell, target, effect, out failureReason))
            {
                return false;
            }

            var healAmount = Math.Max(1f, effect.HealthAmount > 0f ? effect.HealthAmount : effect.DamageAmount > 0f ? effect.DamageAmount : effect.Amount);
            gameplayActions.Add(() =>
            {
                return TryHealEntity(caster.Entity, healAmount);
            });

            visualActions.Add(() =>
            {
                var center = target.Entity?.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF6ACB71), new Vec3d(center.X - 0.1, center.Y + 0.9, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 1.2, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.35f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendControlledEntityField(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            return TryAppendEntityFieldEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
        }

        private bool TryAppendLinkEntitiesEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (target.Entity == null || !target.Entity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                return false;
            }

            if (effect.DurationSeconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            if (IsPlayerTargetBlockedByPvp(caster, target.Entity, effect.Type, out failureReason))
            {
                return false;
            }

            var durationMilliseconds = (long)Math.Round(effect.DurationSeconds * 1000d);
            var tickIntervalMilliseconds = Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d));
            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, target.Entity.EntityId);
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "link",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = target.Entity is EntityPlayer targetPlayer ? targetPlayer.PlayerUID ?? string.Empty : string.Empty,
                TargetEntityId = target.Entity.EntityId,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = target.Entity.Pos?.XYZ.X ?? target.Position.X,
                CenterY = target.Entity.Pos?.XYZ.Y ?? target.Position.Y,
                CenterZ = target.Entity.Pos?.XYZ.Z ?? target.Position.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                Radius = Math.Max(1d, effect.Radius > 0d ? effect.Radius : 4d),
                Amount = effect.Amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + durationMilliseconds,
                StartedAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d),
                ExpiresAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d) + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = tickIntervalMilliseconds,
                NextTickAtMilliseconds = sapi.World.ElapsedMilliseconds + tickIntervalMilliseconds,
                PersistAcrossRestart = true,
                IsArea = false,
                IsHostile = false,
                IsBeneficial = false,
                IsBlocking = false
            };

            if (caster.Entity?.EntityId > 0 && !record.AffectedEntityIds.Contains(caster.Entity.EntityId))
            {
                record.AffectedEntityIds.Add(caster.Entity.EntityId);
            }

            if (!record.AffectedEntityIds.Contains(target.Entity.EntityId))
            {
                record.AffectedEntityIds.Add(target.Entity.EntityId);
            }

            gameplayActions.Add(() => RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true);
            visualActions.Add(() =>
            {
                var start = caster.Entity?.Pos?.XYZ ?? new Vec3d(record.OriginX, record.OriginY, record.OriginZ);
                var end = target.Entity.Pos?.XYZ ?? new Vec3d(record.CenterX, record.CenterY, record.CenterZ);
                var color = record.IsHostile ? unchecked((int)0xFFC94A4A) : unchecked((int)0xFF6ACB71);
                var segments = 5;
                for (var i = 0; i <= segments; i++)
                {
                    var t = segments == 0 ? 0d : i / (double)segments;
                    var point = new Vec3d(start.X + ((end.X - start.X) * t), start.Y + ((end.Y - start.Y) * t), start.Z + ((end.Z - start.Z) * t));
                    sapi.World.SpawnParticles(1, color, new Vec3d(point.X - 0.03, point.Y - 0.03, point.Z - 0.03), new Vec3d(point.X + 0.03, point.Y + 0.03, point.Z + 0.03), new Vec3f(0f, 0.01f, 0f), new Vec3f(0f, 0.02f, 0f), 0.05f, 0f, 0.15f, EnumParticleModel.Quad, caster);
                }

                return true;
            });

            return true;
        }

        private bool TryAppendTeleportToTarget(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (caster.Entity == null)
            {
                failureReason = "the caster is unavailable";
                return false;
            }

            var desired = target.BlockPos != null ? new Vec3d(target.BlockPos.X + 0.5, target.BlockPos.Y + 1.0, target.BlockPos.Z + 0.5) : target.Position;
            gameplayActions.Add(() => TryTeleportEntitySafely(caster.Entity, desired));
            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(desired.X - 0.15, desired.Y + 0.05, desired.Z - 0.15), new Vec3d(desired.X + 0.15, desired.Y + 0.2, desired.Z + 0.15), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendTeleportEntityToCaster(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (target.Entity == null)
            {
                failureReason = "the target entity is unavailable";
                return false;
            }

            if (IsPlayerTargetBlockedByPvp(caster, target.Entity, effect.Type, out failureReason))
            {
                return false;
            }

            var desired = caster.Entity?.Pos?.XYZ ?? target.Position;
            gameplayActions.Add(() =>
            {
                if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, target.Entity))
                {
                    return true;
                }

                return TryTeleportEntitySafely(target.Entity, desired);
            });
            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(desired.X - 0.12, desired.Y + 0.05, desired.Z - 0.12), new Vec3d(desired.X + 0.12, desired.Y + 0.18, desired.Z + 0.12), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendTeleportEntityToPosition(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (target.Entity == null)
            {
                failureReason = "the target entity is unavailable";
                return false;
            }

            if (IsPlayerTargetBlockedByPvp(caster, target.Entity, effect.Type, out failureReason))
            {
                return false;
            }

            var desired = target.BlockPos != null ? new Vec3d(target.BlockPos.X + 0.5, target.BlockPos.Y + 1.0, target.BlockPos.Z + 0.5) : target.Position;
            gameplayActions.Add(() =>
            {
                if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, target.Entity))
                {
                    return true;
                }

                return TryTeleportEntitySafely(target.Entity, desired);
            });
            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(desired.X - 0.12, desired.Y + 0.05, desired.Z - 0.12), new Vec3d(desired.X + 0.12, desired.Y + 0.18, desired.Z + 0.12), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendForceEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var radius = SpellRegistry.GetEffectiveRadius(spell, effect);
            var center = target.Position;
            if (effect.Type == SpellEffectTypes.PushEntity && target.Entity != null)
            {
                var force = Math.Max(0.5f, effect.Force > 0f ? effect.Force : effect.Amount);
                if (IsPlayerTargetBlockedByPvp(caster, target.Entity, effect.Type, out failureReason))
                {
                    return false;
                }

                gameplayActions.Add(() =>
                {
                    var source = caster.Entity?.Pos?.XYZ ?? center;
                    var direction = target.Entity.Pos.XYZ - source;
                    direction.Y = Math.Max(0.2, direction.Y);
                    direction.Normalize();
                    if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, target.Entity))
                    {
                        return true;
                    }

                    target.Entity.Pos.Motion.Add(direction.X * force, direction.Y * force, direction.Z * force);
                    return true;
                });
            }
            else
            {
                if (radius <= 0)
                {
                    failureReason = "radius must be greater than zero";
                    return false;
                }

                gameplayActions.Add(() =>
                {
                    var entities = sapi.World.GetEntitiesAround(center, (float)radius, (float)radius);
                    if (entities == null)
                    {
                        return true;
                    }

                    var source = caster.Entity?.Pos?.XYZ ?? center;
                    foreach (var entity in entities.Where(entity => entity != null && entity.Alive))
                    {
                        if (entity is EntityPlayer && IsPlayerTargetBlockedByPvp(caster, entity, effect.Type, out _))
                        {
                            continue;
                        }

                        if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, entity))
                        {
                            continue;
                        }

                        var direction = entity.Pos.XYZ - source;
                        if (direction.Length() <= 0.001)
                        {
                            direction = new Vec3d(0.01, 0.1, 0.01);
                        }

                        direction.Normalize();
                        var force = Math.Max(0.35f, effect.Force > 0f ? effect.Force : Math.Max(0.5f, effect.Amount));
                        entity.Pos.Motion.Add(direction.X * force, direction.Y * force, direction.Z * force);
                    }

                    return true;
                });
            }

            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.18, center.Z + 0.1), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendMoveDroppedItem(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!SpellRegistry.IsDroppedItemEntity(target.Entity))
            {
                failureReason = "the target is not a dropped item";
                return false;
            }

            var itemEntity = target.Entity as EntityItem;
            if (itemEntity?.Itemstack == null)
            {
                failureReason = "the dropped item is unavailable";
                return false;
            }

            gameplayActions.Add(() =>
            {
                var moved = TryGiveItemStackToCaster(caster, itemEntity.Itemstack);
                if (moved)
                {
                    itemEntity.Itemstack = null;
                    return true;
                }

                return false;
            });

            var visualEntity = target.Entity;
            if (visualEntity == null)
            {
                failureReason = "that spell requires an entity target";
                return false;
            }

            visualActions.Add(() =>
            {
                var center = visualEntity.Pos.XYZ;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendMoveBlockEntityContents(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!SpellRegistry.TryGetBlockEntityInventory(target.BlockEntity, out var inventory) || inventory == null)
            {
                failureReason = "that spell requires a container";
                return false;
            }

            gameplayActions.Add(() =>
            {
                var movedAny = false;
                for (var slotIndex = 0; slotIndex < inventory.Count; slotIndex++)
                {
                    var slot = inventory[slotIndex];
                    if (slot == null)
                    {
                        continue;
                    }

                    var stack = slot.Itemstack;
                    if (stack == null)
                    {
                        continue;
                    }

                    if (TryGiveItemStackToCaster(caster, stack))
                    {
                        slot.Itemstack = null;
                        slot.MarkDirty();
                        inventory.MarkSlotDirty(slotIndex);
                        movedAny = true;
                    }
                }

                return movedAny;
            });

            visualActions.Add(() =>
            {
                var center = target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendBlockEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!TryGetTargetBlockPos(target, out var pos))
            {
                failureReason = "no valid block target";
                return false;
            }

            if (!RustweaveRuntime.Server?.CanModifyBlockAt(pos, caster.PlayerUID) ?? true)
            {
                failureReason = "that block is protected";
                return false;
            }

            var world = caster.Entity?.World ?? sapi.World;
            switch (effect.Type)
            {
                case SpellEffectTypes.MineBlock:
                case SpellEffectTypes.ExtractOre:
                case SpellEffectTypes.HarvestBlocks:
                case SpellEffectTypes.PrecisionBlockStrike:
                    gameplayActions.Add(() => TryBreakBlockWithLoot(world, pos, caster));
                    break;
                case SpellEffectTypes.ExcavateBlocks:
                    gameplayActions.Add(() =>
                    {
                        var removed = 0;
                        var radius = Math.Max(1, effect.MaxBlocks > 0 ? effect.MaxBlocks : 1);
                        for (var dx = -1; dx <= 1 && removed < radius; dx++)
                        {
                            for (var dy = 0; dy <= 1 && removed < radius; dy++)
                            {
                                for (var dz = -1; dz <= 1 && removed < radius; dz++)
                                {
                                var p = new BlockPos(pos.X + dx, pos.Y + dy, pos.Z + dz);
                                    if (!RustweaveRuntime.Server?.CanModifyBlockAt(p, caster.PlayerUID) ?? true)
                                    {
                                        continue;
                                    }

                                    if (TryBreakBlockWithLoot(world, p, caster))
                                    {
                                        removed++;
                                    }
                                }
                            }
                        }

                        return removed > 0;
                    });
                    break;
                case SpellEffectTypes.ConvertBlock:
                case SpellEffectTypes.RepairBlock:
                    gameplayActions.Add(() =>
                    {
                        var blockCode = !string.IsNullOrWhiteSpace(effect.ResultBlockCode) ? effect.ResultBlockCode : effect.BlockCode;
                        if (string.IsNullOrWhiteSpace(blockCode))
                        {
                            return false;
                        }

                        var block = world.GetBlock(new AssetLocation(blockCode));
                        if (block == null)
                        {
                            return false;
                        }

                        world.BlockAccessor.SetBlock(block.BlockId, pos);
                        return true;
                    });
                    break;
                case SpellEffectTypes.RepairBlockArea:
                    gameplayActions.Add(() =>
                    {
                        var repaired = 0;
                        var radius = Math.Max(1, (int)Math.Round(SpellRegistry.GetEffectiveRadius(spell, effect)));
                        for (var dx = -radius; dx <= radius; dx++)
                        {
                            for (var dz = -radius; dz <= radius; dz++)
                            {
                                var p = new BlockPos(pos.X + dx, pos.Y, pos.Z + dz);
                                if (!RustweaveRuntime.Server?.CanModifyBlockAt(p, caster.PlayerUID) ?? true)
                                {
                                    continue;
                                }

                                var blockCode = !string.IsNullOrWhiteSpace(effect.ResultBlockCode) ? effect.ResultBlockCode : effect.BlockCode;
                                if (string.IsNullOrWhiteSpace(blockCode))
                                {
                                    continue;
                                }

                                var block = world.GetBlock(new AssetLocation(blockCode));
                                if (block == null)
                                {
                                    continue;
                                }

                                world.BlockAccessor.SetBlock(block.BlockId, p);
                                repaired++;
                            }
                        }

                        return repaired > 0;
                    });
                    break;
                case SpellEffectTypes.OpenPassage:
                case SpellEffectTypes.CreateRift:
                    gameplayActions.Add(() =>
                    {
                        var snapshots = new List<RustweaveBlockSnapshot>();
                        var dimension = caster.Entity?.Pos?.Dimension ?? 0;
                        for (var dy = 0; dy < 2; dy++)
                        {
                            var p = new BlockPos(pos.X, pos.Y + dy, pos.Z, dimension);
                            if (!RustweaveRuntime.Server?.CanModifyBlockAt(p, caster.PlayerUID) ?? true)
                            {
                                continue;
                            }

                            var block = world.BlockAccessor.GetBlock(p);
                            if (block == null || block.BlockId == 0)
                            {
                                continue;
                            }

                            snapshots.Add(new RustweaveBlockSnapshot
                            {
                                X = p.X,
                                Y = p.Y,
                                Z = p.Z,
                                Dimension = dimension,
                                BlockId = block.BlockId,
                                BlockCode = block.Code?.ToString() ?? string.Empty
                            });
                            world.BlockAccessor.SetBlock(0, p);
                        }

                        if (snapshots.Count == 0)
                        {
                            return false;
                        }

                        var record = BuildAreaEffectRecord(caster, spell, target, effect, new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5));
                        record.RecordKind = string.Equals(effect.Type, SpellEffectTypes.CreateRift, StringComparison.OrdinalIgnoreCase) ? "rift" : "passage";
                        record.BlockSnapshots = snapshots;
                        record.IsBlocking = true;
                        if (record.DurationMilliseconds <= 0)
                        {
                            record.DurationMilliseconds = 10000;
                            record.ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + record.DurationMilliseconds;
                        }
                        return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
                    });
                    break;
                case SpellEffectTypes.CloseRift:
                    gameplayActions.Add(() =>
                    {
                        var nearby = RustweaveRuntime.Server?.GetActiveEffectsNear(target.Position, Math.Max(2d, SpellRegistry.GetEffectiveRadius(spell, effect))) ?? Array.Empty<RustweaveActiveEffectRecord>();
                        var latest = nearby.LastOrDefault(record => string.Equals(record.EffectType, SpellEffectTypes.CreateRift, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.OpenPassage, StringComparison.OrdinalIgnoreCase) || string.Equals(record.RecordKind, "rift", StringComparison.OrdinalIgnoreCase) || string.Equals(record.RecordKind, "passage", StringComparison.OrdinalIgnoreCase));
                        if (latest == null)
                        {
                            return false;
                        }

                        return RustweaveRuntime.Server?.TryRemoveActiveEffect(latest.EffectId, true) == true;
                    });
                    break;
                case SpellEffectTypes.OpenLock:
                    gameplayActions.Add(() =>
                    {
                        var blockEntity = world.BlockAccessor.GetBlockEntity(pos);
                        if (blockEntity == null)
                        {
                            return false;
                        }

                        var unlocked = TrySetBooleanMember(blockEntity, false, "Locked", "IsLocked", "locked", "isLocked");
                        if (unlocked)
                        {
                            TryMarkBlockEntityDirty(blockEntity);
                        }

                        return unlocked;
                    });
                    break;
                case SpellEffectTypes.DestroyCorruptedMatter:
                    gameplayActions.Add(() => TryBreakBlockWithLoot(world, pos, caster));
                    break;
                case SpellEffectTypes.HardenMaterial:
                case SpellEffectTypes.HeatMaterial:
                case SpellEffectTypes.CoolMaterial:
                case SpellEffectTypes.AccelerateCraftState:
                    gameplayActions.Add(() =>
                    {
                        var blockEntity = world.BlockAccessor.GetBlockEntity(pos);
                        if (blockEntity == null)
                        {
                            return false;
                        }

                        var changed = false;
                        var amount = effect.Amount != 0f ? effect.Amount : 1f;
                        if (string.Equals(effect.Type, SpellEffectTypes.HardenMaterial, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(effect.ResultBlockCode))
                        {
                            var block = world.GetBlock(new AssetLocation(effect.ResultBlockCode));
                            if (block != null)
                            {
                                world.BlockAccessor.SetBlock(block.BlockId, pos);
                                changed = true;
                            }
                        }

                        if (string.Equals(effect.Type, SpellEffectTypes.HeatMaterial, StringComparison.OrdinalIgnoreCase))
                        {
                            changed |= TryAdjustNumericMember(blockEntity, amount, "Temperature", "temperature", "Heat", "heat", "TemperatureLevel", "temperatureLevel");
                        }
                        else if (string.Equals(effect.Type, SpellEffectTypes.CoolMaterial, StringComparison.OrdinalIgnoreCase))
                        {
                            changed |= TryAdjustNumericMember(blockEntity, -Math.Abs(amount), "Temperature", "temperature", "Heat", "heat", "TemperatureLevel", "temperatureLevel");
                        }
                        else if (string.Equals(effect.Type, SpellEffectTypes.AccelerateCraftState, StringComparison.OrdinalIgnoreCase))
                        {
                            changed |= TryAdjustNumericMember(blockEntity, Math.Max(0.1d, Math.Abs(amount)), "Progress", "progress", "CraftProgress", "craftProgress", "CookProgress", "cookProgress", "GrowthProgress", "growthProgress");
                        }

                        if (changed)
                        {
                            TryMarkBlockEntityDirty(blockEntity);
                        }

                        return changed;
                    });
                    break;
                case SpellEffectTypes.AnchorBlock:
                case SpellEffectTypes.CreateWardArea:
                case SpellEffectTypes.CreateBarrier:
                case SpellEffectTypes.CreateContainmentArea:
                case SpellEffectTypes.CreateBoundaryLine:
                case SpellEffectTypes.CreateAntiSpreadArea:
                case SpellEffectTypes.StabilizeArea:
                case SpellEffectTypes.ChangeTemperatureArea:
                case SpellEffectTypes.ChangeEnvironmentalPressure:
                case SpellEffectTypes.StormPulse:
                    gameplayActions.Add(() =>
                    {
                        var record = BuildAreaEffectRecord(caster, spell, target, effect, new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5));
                        return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
                    });
                    break;
            }

            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(pos.X + 0.25, pos.Y + 0.05, pos.Z + 0.25), new Vec3d(pos.X + 0.75, pos.Y + 0.15, pos.Z + 0.75), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendHeldOrInventoryEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            switch (effect.Type)
            {
                case SpellEffectTypes.ConvertHeldItem:
                    gameplayActions.Add(() =>
                    {
                        var slot = target.ItemSlot;
                        var itemCode = effect.ItemCode;
                        if (slot == null || string.IsNullOrWhiteSpace(itemCode))
                        {
                            return false;
                        }

                        var collectible = sapi.World.GetItem(new AssetLocation(itemCode));
                        if (collectible == null)
                        {
                            return false;
                        }

                        var stackSize = Math.Max(1, slot.Itemstack?.StackSize ?? 1);
                        slot.Itemstack = new ItemStack(collectible, stackSize);
                        slot.MarkDirty();
                        return true;
                    });
                    break;
                case SpellEffectTypes.SummonTemporaryItem:
                case SpellEffectTypes.CreateTemporaryFood:
                    gameplayActions.Add(() =>
                    {
                        var stack = CreateStack(effect.ItemCode, effect.Amount > 0 ? (int)Math.Round(effect.Amount) : 1);
                        if (stack == null)
                        {
                            return false;
                        }

                        if (target.ItemSlot != null)
                        {
                            target.ItemSlot.Itemstack = stack;
                            target.ItemSlot.MarkDirty();
                            return true;
                        }

                        if (TryGiveItemStackToCaster(caster, stack))
                        {
                            return true;
                        }

                        return TrySpawnItemEntityByReflection(caster.Entity?.World ?? sapi.World, stack, caster.Entity?.Pos?.XYZ ?? target.Position, caster);
                    });
                    break;
            }

            visualActions.Add(() =>
            {
                var center = caster.Entity?.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendSummonEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (effect.DespawnAfterSeconds <= 0)
            {
                failureReason = "despawnAfterSeconds must be greater than zero";
                return false;
            }

            if (effect.Type == SpellEffectTypes.SummonTemporaryItem)
            {
                gameplayActions.Add(() =>
                {
                    var stack = CreateStack(effect.ItemCode, effect.Amount > 0 ? (int)Math.Round(effect.Amount) : 1);
                    if (stack == null)
                    {
                        return false;
                    }

                    var placed = TryGiveItemStackToCaster(caster, stack);
                    if (!placed && target.ItemSlot != null)
                    {
                        target.ItemSlot.Itemstack = stack;
                        target.ItemSlot.MarkDirty();
                        placed = true;
                    }

                    if (!placed)
                    {
                        return false;
                    }

                    var record = BuildAreaEffectRecord(caster, spell, target, effect, caster.Entity?.Pos?.XYZ ?? target.Position);
                    record.RecordKind = "item";
                    record.ItemCode = effect.ItemCode;
                    record.TargetEntityId = caster.Entity?.EntityId ?? -1;
                    record.Amount = effect.Amount;
                    record.DurationMilliseconds = (long)Math.Round(effect.DespawnAfterSeconds * 1000d);
                    record.ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + record.DurationMilliseconds;
                    return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
                });
            }
            else if (effect.Type == SpellEffectTypes.SummonTemporaryEntity || effect.Type == SpellEffectTypes.SummonTemporaryProjectile)
            {
                gameplayActions.Add(() =>
                {
                    var summonPosition = target.BlockPos != null ? new Vec3d(target.BlockPos.X + 0.5, target.BlockPos.Y + 1.0, target.BlockPos.Z + 0.5) : target.Position;
                    if (!TrySpawnEntityByCode(effect.EntityCode, summonPosition, caster, out var spawnedEntity))
                    {
                        return false;
                    }

                    var record = BuildAreaEffectRecord(caster, spell, target, effect, summonPosition);
                    record.RecordKind = effect.Type == SpellEffectTypes.SummonTemporaryProjectile ? "projectile" : "summon";
                    record.EntityCode = effect.EntityCode;
                    record.TargetEntityId = spawnedEntity?.EntityId ?? -1;
                    record.AffectedEntityIds = spawnedEntity?.EntityId > 0 ? new List<long> { spawnedEntity.EntityId } : new List<long>();
                    record.Amount = effect.Amount;
                    record.DurationMilliseconds = (long)Math.Round(effect.DespawnAfterSeconds * 1000d);
                    record.ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + record.DurationMilliseconds;
                    return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
                });
            }
            else if (effect.Type == SpellEffectTypes.SummonTemporaryConstruct)
            {
                gameplayActions.Add(() =>
                {
                    if (!TryGetTargetBlockPos(target, out var pos))
                    {
                        return false;
                    }

                    var dimension = caster.Entity?.Pos?.Dimension ?? 0;
                    if (!RustweaveRuntime.Server?.CanModifyBlockAt(pos, caster.PlayerUID) ?? true)
                    {
                        return false;
                    }

                    var blockCode = !string.IsNullOrWhiteSpace(effect.BlockCode) ? effect.BlockCode : effect.ResultBlockCode;
                    if (string.IsNullOrWhiteSpace(blockCode))
                    {
                        return false;
                    }

                    var block = sapi.World.GetBlock(new AssetLocation(blockCode));
                    if (block == null)
                    {
                        return false;
                    }

                    var snapshot = new RustweaveBlockSnapshot
                    {
                        X = pos.X,
                        Y = pos.Y,
                        Z = pos.Z,
                        Dimension = dimension,
                        BlockId = sapi.World.BlockAccessor.GetBlock(pos)?.BlockId ?? 0,
                        BlockCode = sapi.World.BlockAccessor.GetBlock(pos)?.Code?.ToString() ?? string.Empty
                    };

                    sapi.World.BlockAccessor.SetBlock(block.BlockId, pos);
                    var record = BuildAreaEffectRecord(caster, spell, target, effect, new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5));
                    record.RecordKind = "construct";
                    record.BlockCode = blockCode;
                    record.BlockSnapshots = new List<RustweaveBlockSnapshot> { snapshot };
                    record.DurationMilliseconds = (long)Math.Round(effect.DespawnAfterSeconds * 1000d);
                    record.ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + record.DurationMilliseconds;
                    return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
                });
            }

            visualActions.Add(() =>
            {
                var center = target.Position;
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendDetectionEffect(List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            visualActions.Add(() =>
            {
                var center = target.Position;
                sapi.World.SpawnParticles(8, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.2, center.Y + 0.1, center.Z - 0.2), new Vec3d(center.X + 0.2, center.Y + 0.5, center.Z + 0.2), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendGraftingEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (effect.Type == SpellEffectTypes.ModifySatiety)
            {
                var targetEntity = target.Entity as EntityPlayer;
                if (targetEntity == null)
                {
                    if (string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
                    {
                        targetEntity = caster.Entity as EntityPlayer;
                    }
                    else
                    {
                        failureReason = "that spell requires a player target";
                        return false;
                    }
                }

                if (targetEntity == null)
                {
                    failureReason = "that spell requires a player target";
                    return false;
                }

                gameplayActions.Add(() => TryModifySatiety(targetEntity, effect.Amount, out _));
                visualActions.Add(() =>
                {
                    var center = targetEntity.Pos?.XYZ ?? caster.Entity?.Pos?.XYZ ?? target.Position;
                    sapi.World.SpawnParticles(4, unchecked((int)0xFF6ACB71), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                    return true;
                });

                return true;
            }

            if (effect.Type == SpellEffectTypes.ModifyCropGrowth || effect.Type == SpellEffectTypes.ModifyFarmlandNutrients)
            {
                var center = target.BlockPos != null
                    ? new Vec3d(target.BlockPos.X + 0.5, target.BlockPos.Y + 0.5, target.BlockPos.Z + 0.5)
                    : target.Position;
                var record = BuildAreaEffectRecord(caster, spell, target, effect, center);
                record.RecordKind = "crop";
                record.TickIntervalMilliseconds = 1000;
                record.NextTickAtMilliseconds = sapi.World.ElapsedMilliseconds + 1000;
                record.IsArea = true;
                record.IsBeneficial = effect.Amount >= 0f;
                record.IsHostile = effect.Amount < 0f;
                gameplayActions.Add(() => RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true);
                visualActions.Add(() =>
                {
                    var color = effect.Amount >= 0f ? unchecked((int)0xFF6ACB71) : unchecked((int)0xFFC94A4A);
                    sapi.World.SpawnParticles(6, color, new Vec3d(center.X - 0.15, center.Y + 0.05, center.Z - 0.15), new Vec3d(center.X + 0.15, center.Y + 0.2, center.Z + 0.15), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.05f, 0.01f), 0.3f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                    return true;
                });
                return true;
            }

            if (effect.Type == SpellEffectTypes.ModifyAnimalFertility)
            {
                if (target.Entity == null)
                {
                    failureReason = "the target entity is unavailable";
                    return false;
                }

                var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
                var record = new RustweaveActiveEffectRecord
                {
                    EffectId = BuildTimedEffectCode(spell.Code, effect.Type, target.Entity.EntityId),
                    EffectType = effect.Type,
                    RecordKind = "entity",
                    SpellCode = spell.Code,
                    CasterPlayerUid = caster.PlayerUID,
                    CasterEntityId = caster.Entity?.EntityId ?? -1,
                    TargetPlayerUid = target.Entity is EntityPlayer targetPlayer ? targetPlayer.PlayerUID ?? string.Empty : string.Empty,
                    TargetEntityId = target.Entity.EntityId,
                    TargetType = spell.TargetType,
                    Mode = effect.Mode,
                    CenterX = target.Entity.Pos?.XYZ.X ?? target.Position.X,
                    CenterY = target.Entity.Pos?.XYZ.Y ?? target.Position.Y,
                    CenterZ = target.Entity.Pos?.XYZ.Z ?? target.Position.Z,
                    OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                    OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                    OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                    Radius = Math.Max(1d, effect.Radius > 0d ? effect.Radius : 1d),
                    Amount = effect.Amount,
                    SecondaryAmount = effect.SecondaryAmount,
                    DurationMilliseconds = durationMilliseconds,
                    StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                    ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + durationMilliseconds,
                    StartedAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d),
                    ExpiresAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d) + (Math.Max(1d, effect.DurationSeconds) / 24d),
                    TickIntervalMilliseconds = Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                    NextTickAtMilliseconds = sapi.World.ElapsedMilliseconds + Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                    PersistAcrossRestart = true,
                    IsArea = false,
                    IsHostile = false,
                    IsBeneficial = true
                };

                gameplayActions.Add(() => RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true);
                visualActions.Add(() =>
                {
                    var center = target.Entity.Pos?.XYZ ?? target.Position;
                    sapi.World.SpawnParticles(8, unchecked((int)0xFF6ACB71), new Vec3d(center.X - 0.12, center.Y + 0.1, center.Z - 0.12), new Vec3d(center.X + 0.12, center.Y + 1.2, center.Z + 0.12), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.05f, 0.01f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                    return true;
                });
                return true;
            }

            failureReason = $"unsupported grafting effect '{effect.Type}'";
            return false;
        }

        private bool TryAppendWeatherEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (effect.Type == SpellEffectTypes.CallLightning)
            {
                gameplayActions.Add(() => TryCallLightningAt(caster, spell, target.Position, effect));
                visualActions.Add(() =>
                {
                    var center = target.Position;
                    sapi.World.SpawnParticles(8, unchecked((int)0xFFD9D0FF), new Vec3d(center.X - 0.15, center.Y + 0.2, center.Z - 0.15), new Vec3d(center.X + 0.15, center.Y + 0.9, center.Z + 0.15), new Vec3f(-0.02f, 0.04f, -0.02f), new Vec3f(0.02f, 0.08f, 0.02f), 0.25f, 0f, 0.5f, EnumParticleModel.Quad, caster);
                    return true;
                });
                return true;
            }

            gameplayActions.Add(() =>
            {
                if (string.Equals(effect.Type, SpellEffectTypes.ChangeWeather, StringComparison.OrdinalIgnoreCase))
                {
                    TryApplyWeatherChange(target.Position, effect);
                }

                var record = BuildAreaEffectRecord(caster, spell, target, effect, target.Position);
                record.RecordKind = effect.Type == SpellEffectTypes.ChangeWeather ? "weather" : "weather-area";
                record.WeatherType = effect.WeatherType;
                if (string.Equals(effect.Type, SpellEffectTypes.ChangeWeather, StringComparison.OrdinalIgnoreCase) && record.DurationMilliseconds <= 0)
                {
                    record.DurationMilliseconds = 10000;
                    record.ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + record.DurationMilliseconds;
                }
                return RustweaveRuntime.Server?.TryRegisterActiveEffect(record) == true;
            });

            visualActions.Add(() =>
            {
                var center = target.Position;
                sapi.World.SpawnParticles(8, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.2, center.Y + 0.1, center.Z - 0.2), new Vec3d(center.X + 0.2, center.Y + 0.6, center.Z + 0.2), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryCallLightningAt(IServerPlayer caster, SpellDefinition spell, Vec3d position, SpellEffectDefinition effect)
        {
            var radius = Math.Max(1d, SpellRegistry.GetEffectiveRadius(spell, effect));
            var damage = Math.Max(2f, effect.DamageAmount > 0f ? effect.DamageAmount : Math.Abs(effect.Amount) > 0f ? Math.Abs(effect.Amount) : 4f);
            var entities = sapi.World.GetEntitiesAround(position, (float)radius, (float)radius);
            if (entities != null)
            {
                foreach (var entity in entities.Where(entity => entity != null && entity.Alive))
                {
                    if (entity is EntityPlayer && IsPlayerTargetBlockedByPvp(caster, entity, effect.Type, out _))
                    {
                        continue;
                    }

                    ApplySpellDamage(caster, spell, effect.Type, entity, new DamageSource
                    {
                        Source = EnumDamageSource.Internal,
                        SourceEntity = caster.Entity,
                        CauseEntity = caster.Entity,
                        DamageTier = 2,
                        KnockbackStrength = 0.35f
                    }, damage);
                }
            }

            return true;
        }

        private bool TryApplyWeatherChange(Vec3d position, SpellEffectDefinition effect)
        {
            try
            {
                var world = sapi.World;
                foreach (var method in world.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!method.Name.Contains("Weather", StringComparison.OrdinalIgnoreCase) && !method.Name.Contains("Rain", StringComparison.OrdinalIgnoreCase) && !method.Name.Contains("Storm", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0 || parameters.Length > 4)
                    {
                        continue;
                    }

                    var args = new object?[parameters.Length];
                    var usable = true;
                    for (var index = 0; index < parameters.Length; index++)
                    {
                        var parameterType = parameters[index].ParameterType;
                        if (parameterType == typeof(string))
                        {
                            args[index] = !string.IsNullOrWhiteSpace(effect.WeatherType) ? effect.WeatherType : effect.Mode;
                        }
                        else if (typeof(Vec3d).IsAssignableFrom(parameterType))
                        {
                            args[index] = position;
                        }
                        else if (parameterType == typeof(bool))
                        {
                            args[index] = true;
                        }
                        else if (parameterType == typeof(float) || parameterType == typeof(double))
                        {
                            args[index] = Math.Max(0f, effect.Amount);
                        }
                        else if (parameterType.IsValueType)
                        {
                            args[index] = Activator.CreateInstance(parameterType);
                        }
                        else
                        {
                            usable = false;
                            break;
                        }
                    }

                    if (!usable)
                    {
                        continue;
                    }

                    try
                    {
                        method.Invoke(world, args);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            sapi.World.SpawnParticles(8, unchecked((int)0xFF8C6A4A), new Vec3d(position.X - 0.4, position.Y + 0.2, position.Z - 0.4), new Vec3d(position.X + 0.4, position.Y + 1.0, position.Z + 0.4), new Vec3f(-0.02f, 0.03f, -0.02f), new Vec3f(0.02f, 0.07f, 0.02f), 0.28f, 0f, 0.45f, EnumParticleModel.Quad, null);
            return true;
        }

        private bool TryAppendCleanupEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var entity = target.Entity ?? caster.Entity;
            gameplayActions.Add(() =>
            {
                if (entity == null)
                {
                    return false;
                }

                var effects = RustweaveRuntime.Server?.GetActiveEffectsForEntity(entity.EntityId) ?? Array.Empty<RustweaveActiveEffectRecord>();
                var removed = false;
                foreach (var record in effects)
                {
                    if (record == null)
                    {
                        continue;
                    }

                    if (string.Equals(effect.Type, SpellEffectTypes.PurgeTimedEffects, StringComparison.OrdinalIgnoreCase) && record.IsHostile)
                    {
                        removed |= RustweaveRuntime.Server?.TryRemoveActiveEffect(record.EffectId, false) == true;
                    }
                    else if (string.Equals(effect.Type, SpellEffectTypes.StripEntityBuffs, StringComparison.OrdinalIgnoreCase) && record.IsBeneficial)
                    {
                        removed |= RustweaveRuntime.Server?.TryRemoveActiveEffect(record.EffectId, false) == true;
                    }
                    else if (string.Equals(effect.Type, SpellEffectTypes.CleanseContamination, StringComparison.OrdinalIgnoreCase) && string.Equals(record.EffectType, SpellEffectTypes.ModifyCorruptionGain, StringComparison.OrdinalIgnoreCase))
                    {
                        removed |= RustweaveRuntime.Server?.TryRemoveActiveEffect(record.EffectId, false) == true;
                    }
                    else if (string.Equals(effect.Type, SpellEffectTypes.UnravelDamage, StringComparison.OrdinalIgnoreCase) && string.Equals(record.EffectType, SpellEffectTypes.DamageOverTime, StringComparison.OrdinalIgnoreCase))
                    {
                        removed |= RustweaveRuntime.Server?.TryRemoveActiveEffect(record.EffectId, false) == true;
                    }
                }

                return removed;
            });

            visualActions.Add(() =>
            {
                var center = entity.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF6ACB71), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.3, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.25f, 0f, 0.4f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryConsumeDisplacementBraceIfNeeded(IServerPlayer caster, SpellDefinition spell, string effectType, Entity? targetEntity)
        {
            return targetEntity != null
                && RustweaveRuntime.Server?.TryConsumeDisplacementBrace(targetEntity, caster, spell.Code, effectType, out _) == true;
        }

        private bool TryAppendFreezeTemporalStabilityLoss(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase))
            {
                failureReason = "freezeTemporalStabilityLoss requires targetType self";
                return false;
            }

            var targetEntity = caster.Entity as EntityPlayer;
            if (targetEntity == null)
            {
                sapi.Logger.Warning("[TheRustweave] freezeTemporalStabilityLoss failed for spell '{0}' because caster '{1}' could not resolve a self target.", spell.Code, caster.PlayerUID);
                failureReason = Lang.Get("game:rustweave-still-thread-fail");
                return false;
            }

            if (!RustweaveStateService.TryReadTemporalStabilityValue(targetEntity, out var baseline))
            {
                sapi.Logger.Warning("[TheRustweave] freezeTemporalStabilityLoss failed because temporal stability could not be read for player '{0}'.", caster.PlayerUID);
                failureReason = Lang.Get("game:rustweave-still-thread-fail");
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, targetEntity.EntityId);
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "self",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = targetEntity.PlayerUID ?? string.Empty,
                TargetEntityId = targetEntity.EntityId,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = targetEntity.Pos?.XYZ.X ?? target.Position.X,
                CenterY = targetEntity.Pos?.XYZ.Y ?? target.Position.Y,
                CenterZ = targetEntity.Pos?.XYZ.Z ?? target.Position.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                Radius = Math.Max(1d, effect.Radius),
                Amount = effect.Amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = nowMilliseconds,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                StartedAtTotalDays = nowDays,
                ExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                NextTickAtMilliseconds = nowMilliseconds + Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                PersistAcrossRestart = true,
                IsArea = false,
                IsHostile = false,
                IsBeneficial = true,
                IsBlocking = false
            };

            gameplayActions.Add(() =>
            {
                RustweaveRuntime.Server?.TryRemoveActiveEffectsForEntityAndType(targetEntity, SpellEffectTypes.FreezeTemporalStabilityLoss, spell.Code);
                if (RustweaveRuntime.Server?.TryRegisterActiveEffect(record) != true)
                {
                    return false;
                }

                state.FreezeTemporalStabilityLossStartedAtTotalDays = nowDays;
                state.FreezeTemporalStabilityLossExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d);
                state.FreezeTemporalStabilityLossBaseline = baseline;
                state.FreezeTemporalStabilityLossSourceSpellCode = spell.Code;
                caster.SendMessage(0, Lang.Get("game:rustweave-still-thread-active"), EnumChatType.Notification, null);
                return true;
            });

            visualActions.Add(() =>
            {
                var center = targetEntity.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(8, unchecked((int)0xFF6FCBD3), new Vec3d(center.X - 0.12, center.Y + 0.05, center.Z - 0.12), new Vec3d(center.X + 0.12, center.Y + 1.3, center.Z + 0.12), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.05f, 0.01f), 0.2f, 0f, 0.35f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendBraceNextDisplacement(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity as EntityPlayer ?? caster.Entity as EntityPlayer;
            if (targetEntity == null)
            {
                failureReason = "that spell requires a player target";
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, targetEntity.EntityId);
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "self",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = targetEntity.PlayerUID ?? string.Empty,
                TargetEntityId = targetEntity.EntityId,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = targetEntity.Pos?.XYZ.X ?? target.Position.X,
                CenterY = targetEntity.Pos?.XYZ.Y ?? target.Position.Y,
                CenterZ = targetEntity.Pos?.XYZ.Z ?? target.Position.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                Radius = Math.Max(1d, effect.Radius),
                Amount = effect.Amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = nowMilliseconds,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                StartedAtTotalDays = nowDays,
                ExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = Math.Max(500, (long)Math.Round(Math.Max(0.5d, effect.TickIntervalSeconds) * 1000d)),
                NextTickAtMilliseconds = nowMilliseconds + Math.Max(500, (long)Math.Round(Math.Max(0.5d, effect.TickIntervalSeconds) * 1000d)),
                PersistAcrossRestart = true,
                IsArea = false,
                IsHostile = false,
                IsBeneficial = true,
                IsBlocking = true
            };

            gameplayActions.Add(() =>
            {
                RustweaveRuntime.Server?.TryRemoveActiveEffectsForEntityAndType(targetEntity, SpellEffectTypes.BraceNextDisplacement, spell.Code);
                if (RustweaveRuntime.Server?.TryRegisterActiveEffect(record) != true)
                {
                    return false;
                }

                state.BraceNextDisplacementStartedAtTotalDays = nowDays;
                state.BraceNextDisplacementExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d);
                state.BraceNextDisplacementSourceSpellCode = spell.Code;
                return true;
            });

            visualActions.Add(() =>
            {
                var center = targetEntity.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(8, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.14, center.Y + 0.05, center.Z - 0.14), new Vec3d(center.X + 0.14, center.Y + 0.35, center.Z + 0.14), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.22f, 0f, 0.35f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendSenseTemporalStorm(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            gameplayActions.Add(() =>
            {
                if (TryReadTemporalStormForecast(sapi.World, out var hoursUntilStorm, out var intensity, out var intensitySource))
                {
                    var clampedHours = Math.Max(0d, hoursUntilStorm);
                    sapi.Logger.Debug("[TheRustweave] Stable Sense forecast: timingAvailable={0}, hoursUntil={1}, intensity={2}, intensitySource={3}", true, clampedHours.ToString("0.0", CultureInfo.InvariantCulture), intensity, intensitySource);
                    if (clampedHours <= 0.01d)
                    {
                        caster.SendMessage(0, Lang.Get("game:rustweave-stable-sense-active", intensity), EnumChatType.Notification, null);
                    }
                    else
                    {
                        caster.SendMessage(0, Lang.Get("game:rustweave-stable-sense-result", clampedHours.ToString("0.0", CultureInfo.InvariantCulture), intensity), EnumChatType.Notification, null);
                    }
                }
                else
                {
                    sapi.Logger.Debug("[TheRustweave] Stable Sense forecast: timingAvailable={0}, hoursUntil={1}, intensity={2}, intensitySource={3}", false, "0.0", "Unknown", "unavailable");
                    caster.SendMessage(0, Lang.Get("game:rustweave-stable-sense-unavailable"), EnumChatType.Notification, null);
                }

                return true;
            });

            visualActions.Add(() =>
            {
                var center = caster.Entity?.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(4, unchecked((int)0xFF6FCBD3), new Vec3d(center.X - 0.1, center.Y + 0.05, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.35, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.18f, 0f, 0.3f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendDeferSpellCorruptionCost(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, RustweavePlayerStateData state, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity as EntityPlayer ?? caster.Entity as EntityPlayer;
            if (targetEntity == null)
            {
                failureReason = "that spell requires a player target";
                return false;
            }

            if (RustweaveStateService.HasActiveFoundationalFabric(state, sapi.World.Calendar?.TotalDays ?? 0d))
            {
                failureReason = Lang.Get("game:rustweave-foundational-fabric-active");
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, targetEntity.EntityId);
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            var reductionPercent = Math.Max(0d, Math.Min(0.99d, effect.Amount));
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "self",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = targetEntity.PlayerUID ?? string.Empty,
                TargetEntityId = targetEntity.EntityId,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = targetEntity.Pos?.XYZ.X ?? target.Position.X,
                CenterY = targetEntity.Pos?.XYZ.Y ?? target.Position.Y,
                CenterZ = targetEntity.Pos?.XYZ.Z ?? target.Position.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                Radius = Math.Max(1d, effect.Radius),
                Amount = (float)reductionPercent,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = nowMilliseconds,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                StartedAtTotalDays = nowDays,
                ExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = Math.Max(1000, (long)Math.Round(Math.Max(1d, effect.TickIntervalSeconds) * 1000d)),
                NextTickAtMilliseconds = nowMilliseconds + Math.Max(1000, (long)Math.Round(Math.Max(1d, effect.TickIntervalSeconds) * 1000d)),
                PersistAcrossRestart = true,
                IsArea = false,
                IsHostile = false,
                IsBeneficial = true,
                IsBlocking = false
            };

            gameplayActions.Add(() =>
            {
                RustweaveRuntime.Server?.TryRemoveActiveEffectsForEntityAndType(targetEntity, SpellEffectTypes.DeferSpellCorruptionCost, spell.Code);
                if (RustweaveRuntime.Server?.TryRegisterActiveEffect(record) != true)
                {
                    return false;
                }

                state.FoundationalFabricStartedAtTotalDays = nowDays;
                state.FoundationalFabricExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d);
                state.FoundationalFabricSourceSpellCode = spell.Code;
                state.FoundationalFabricReductionPercent = reductionPercent;
                state.FoundationalFabricDebtAmount = 0;
                return true;
            });

            visualActions.Add(() =>
            {
                var center = targetEntity.Pos?.XYZ ?? target.Position;
                sapi.World.SpawnParticles(6, unchecked((int)0xFFC69C29), new Vec3d(center.X - 0.1, center.Y + 0.05, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.28, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.04f, 0.01f), 0.2f, 0f, 0.3f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private static bool TryReadNumericMemberValue(object target, out double value, params string[] memberNames)
        {
            value = 0d;
            if (target == null || memberNames == null || memberNames.Length == 0)
            {
                return false;
            }

            var type = target.GetType();
            foreach (var memberName in memberNames.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.CanRead == true)
                {
                    try
                    {
                        var current = property.GetValue(target);
                        if (current is int currentInt)
                        {
                            value = currentInt;
                            return true;
                        }

                        if (current is long currentLong)
                        {
                            value = currentLong;
                            return true;
                        }

                        if (current is float currentFloat)
                        {
                            value = currentFloat;
                            return true;
                        }

                        if (current is double currentDouble)
                        {
                            value = currentDouble;
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }

                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    try
                    {
                        var current = field.GetValue(target);
                        if (current is int currentInt)
                        {
                            value = currentInt;
                            return true;
                        }

                        if (current is long currentLong)
                        {
                            value = currentLong;
                            return true;
                        }

                        if (current is float currentFloat)
                        {
                            value = currentFloat;
                            return true;
                        }

                        if (current is double currentDouble)
                        {
                            value = currentDouble;
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        private static bool TryReadStringMemberValue(object target, out string value, params string[] memberNames)
        {
            value = string.Empty;
            if (target == null || memberNames == null || memberNames.Length == 0)
            {
                return false;
            }

            var type = target.GetType();
            foreach (var memberName in memberNames.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.CanRead == true)
                {
                    try
                    {
                        var current = property.GetValue(target)?.ToString();
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            value = current!;
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }

                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    try
                    {
                        var current = field.GetValue(target)?.ToString();
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            value = current!;
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        private bool TryReadTemporalStormForecast(IWorldAccessor world, out double hoursUntilStorm, out string intensity, out string intensitySource)
        {
            hoursUntilStorm = 0d;
            intensity = "Unknown";
            intensitySource = "unavailable";
            if (world == null)
            {
                return false;
            }

            var probes = new List<object>();
            probes.Add(world);
            if (world.Calendar != null)
            {
                probes.Add(world.Calendar);
            }

            if (world.Api != null)
            {
                probes.Add(world.Api);
                if (world.Api.ModLoader != null)
                {
                    probes.Add(world.Api.ModLoader);
                    if (TryGetTemporalStabilitySystem(world, out var stormSystem) && stormSystem != null)
                    {
                        probes.Add(stormSystem);
                    }
                }
            }

            foreach (var probe in probes.ToArray())
            {
                TryCollectNestedStormProbes(probe, probes);
            }

            var relativeHourNames = new[]
            {
                "NextTemporalStormHours",
                "NextTemporalStormInHours",
                "HoursUntilNextTemporalStorm",
                "HoursUntilNextStorm",
                "NextStormHours",
                "TemporalStormHoursRemaining",
                "TemporalStormHours",
                "StormHoursRemaining",
                "StormHours",
                "HoursToNextStorm",
                "HoursUntilStorm",
                "HoursRemainingUntilNextStorm",
                "StormHoursLeft",
                "nextStormHours",
                "nextStormTime",
                "stormTimeRemaining",
                "stormHoursRemaining",
                "nextStormRemainingHours"
            };

            var absoluteHourNames = new[]
            {
                "NextTemporalStormTotalHours",
                "NextTemporalStormAtTotalHours",
                "NextStormTotalHours",
                "NextStormStartTotalHours",
                "StormStartTotalHours",
                "StormTotalHours",
                "nextStormTotalHours",
                "stormStartTotalHours",
                "nextStormAtTotalHours"
            };

            var dayNames = new[]
            {
                "NextTemporalStormDays",
                "NextTemporalStormInDays",
                "DaysUntilNextTemporalStorm",
                "DaysUntilNextStorm",
                "NextStormDays",
                "TemporalStormDaysRemaining",
                "StormDaysRemaining",
                "StormDays",
                "DaysToNextStorm",
                "DaysUntilStorm"
            };

            var absoluteDayNames = new[]
            {
                "NextTemporalStormTotalDays",
                "NextTemporalStormAtTotalDays",
                "NextStormTotalDays",
                "NextStormStartTotalDays",
                "StormStartTotalDays",
                "StormTotalDays",
                "nextStormTotalDays",
                "stormStartTotalDays",
                "nextStormAtTotalDays"
            };

            var currentHoursKnown = TryReadCurrentInGameHours(world, out var currentHours);
            foreach (var probe in probes)
            {
                if (TryReadNumericMemberValue(probe, out var hoursValue, relativeHourNames))
                {
                    hoursUntilStorm = hoursValue;
                    goto got_time;
                }

                if (TryReadNumericMemberValue(probe, out var absoluteHoursValue, absoluteHourNames) && currentHoursKnown)
                {
                    hoursUntilStorm = absoluteHoursValue - currentHours;
                    goto got_time;
                }

                if (TryReadNumericMemberValue(probe, out var daysValue, dayNames))
                {
                    hoursUntilStorm = daysValue * 24d;
                    goto got_time;
                }

                if (TryReadNumericMemberValue(probe, out var absoluteDaysValue, absoluteDayNames) && currentHoursKnown)
                {
                    hoursUntilStorm = absoluteDaysValue * 24d - currentHours;
                    goto got_time;
                }
            }

            return false;

        got_time:
            if (hoursUntilStorm < 0d)
            {
                hoursUntilStorm = 0d;
            }

            intensity = GetStormIntensityLabel(hoursUntilStorm);
            intensitySource = "time-band";
            return true;
        }

        private static bool TryReadCurrentInGameHours(IWorldAccessor world, out double currentHours)
        {
            currentHours = 0d;
            if (world?.Calendar == null)
            {
                return false;
            }

            if (TryReadNumericMemberValue(world.Calendar, out currentHours, "TotalHours", "TotalWorldHours", "TotalGameHours"))
            {
                return true;
            }

            if (TryReadNumericMemberValue(world.Calendar, out var totalDays, "TotalDays", "TotalWorldDays"))
            {
                currentHours = totalDays * 24d;
                return true;
            }

            return false;
        }

        private bool TryGetTemporalStabilitySystem(IWorldAccessor world, out object? system)
        {
            system = null;
            var modLoader = world?.Api?.ModLoader;
            if (modLoader == null)
            {
                return false;
            }

            var systemType = Type.GetType("Vintagestory.GameContent.SystemTemporalStability, Vintagestory.GameContent", false);
            if (systemType == null)
            {
                return false;
            }

            var getModSystemMethod = modLoader.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method => string.Equals(method.Name, "GetModSystem", StringComparison.Ordinal) && method.IsGenericMethodDefinition && method.GetParameters().Length == 0);
            if (getModSystemMethod == null)
            {
                return false;
            }

            try
            {
                var genericMethod = getModSystemMethod.MakeGenericMethod(systemType);
                system = genericMethod.Invoke(modLoader, Array.Empty<object>());
                return system != null;
            }
            catch
            {
                system = null;
                return false;
            }
        }

        private static void TryCollectNestedStormProbes(object probe, ICollection<object> probes)
        {
            if (probe == null || probes == null)
            {
                return;
            }

            var type = probe.GetType();
            var memberNames = new[]
            {
                "TemporalStorm",
                "Storm",
                "StormData",
                "NextStorm",
                "CurrentStorm",
                "StormState",
                "TemporalStormData",
                "StormInfo",
                "nextStorm",
                "stormStart",
                "temporalStorm",
                "stormForecast",
                "stormSchedule"
            };

            foreach (var memberName in memberNames)
            {
                try
                {
                    var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var current = property?.CanRead == true ? property.GetValue(probe) : null;
                    if (current != null && !probes.Contains(current))
                    {
                        probes.Add(current);
                        continue;
                    }

                    var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    current = field?.GetValue(probe);
                    if (current != null && !probes.Contains(current))
                    {
                        probes.Add(current);
                    }
                }
                catch
                {
                }
            }
        }

        private static string GetStormIntensityLabel(double hoursUntilStorm)
        {
            if (hoursUntilStorm > 72d)
            {
                return "Light";
            }

            if (hoursUntilStorm > 24d)
            {
                return "Medium";
            }

            return "Heavy";
        }

        private RustweaveActiveEffectRecord BuildAreaEffectRecord(IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, Vec3d center)
        {
            var radius = Math.Max(0d, spell.Radius > 0d ? spell.Radius : effect.Radius);
            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            return new RustweaveActiveEffectRecord
            {
                EffectId = $"{spell.Code}:{effect.Type}:{caster.Entity?.EntityId ?? 0}:{Guid.NewGuid():N}",
                EffectType = effect.Type,
                RecordKind = "area",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = target.Entity is EntityPlayer targetPlayer ? targetPlayer.PlayerUID ?? string.Empty : string.Empty,
                TargetEntityId = target.Entity?.EntityId ?? -1,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = center.X,
                CenterY = center.Y,
                CenterZ = center.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? center.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? center.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? center.Z,
                Radius = radius,
                Amount = effect.Amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = nowMilliseconds,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                StartedAtTotalDays = nowDays,
                ExpiresAtTotalDays = nowDays + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                NextTickAtMilliseconds = nowMilliseconds + Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                PersistAcrossRestart = true,
                IsArea = true,
                IsHostile = effect.Amount < 0f || effect.Type is SpellEffectTypes.ChangeTemperatureArea or SpellEffectTypes.ChangeEnvironmentalPressure or SpellEffectTypes.StormPulse,
                IsBeneficial = effect.Amount > 0f || effect.Type is SpellEffectTypes.StabilizeArea or SpellEffectTypes.CreateWardArea or SpellEffectTypes.CreateBarrier or SpellEffectTypes.CreateContainmentArea or SpellEffectTypes.CreateBoundaryLine or SpellEffectTypes.CreateAntiSpreadArea
            };
        }

        private bool TryHealEntity(Entity? entity, float amount)
        {
            if (entity == null || amount <= 0f)
            {
                return false;
            }

            var health = entity.GetBehavior<EntityBehaviorHealth>();
            if (health == null)
            {
                return false;
            }

            health.Health = Math.Min(health.MaxHealth, health.Health + amount);
            return true;
        }

        private bool TryTeleportEntitySafely(Entity entity, Vec3d desired)
        {
            if (entity == null)
            {
                return false;
            }

            if (RustweaveRuntime.Server?.TryFindNearestSafeTeleportPosition(entity.World, entity, desired, out var safePosition) == true)
            {
                entity.TeleportTo(safePosition);
                return true;
            }

            return false;
        }

        private static bool TryGetTargetBlockPos(SpellTargetContext target, out BlockPos pos)
        {
            if (target.BlockPos != null)
            {
                pos = target.BlockPos;
                return true;
            }

            pos = new BlockPos((int)Math.Floor(target.Position.X), (int)Math.Floor(target.Position.Y), (int)Math.Floor(target.Position.Z), target.Entity?.Pos?.Dimension ?? 0);
            return true;
        }

        private bool TryGiveItemStackToCaster(IServerPlayer caster, ItemStack stack)
        {
            if (caster?.InventoryManager?.InventoriesOrdered == null || stack == null)
            {
                return false;
            }

            foreach (var inventory in caster.InventoryManager.InventoriesOrdered)
            {
                for (var slotIndex = 0; slotIndex < inventory.Count; slotIndex++)
                {
                    var slot = inventory[slotIndex];
                    if (slot == null)
                    {
                        continue;
                    }

                    if (slot.Itemstack == null)
                    {
                        slot.Itemstack = stack.Clone();
                        slot.MarkDirty();
                        inventory.MarkSlotDirty(slotIndex);
                        return true;
                    }

                    if (slot.Itemstack.Collectible?.Code != null && stack.Collectible?.Code != null && slot.Itemstack.Collectible.Code.Path == stack.Collectible.Code.Path)
                    {
                        slot.Itemstack.StackSize = Math.Min(slot.Itemstack.StackSize + stack.StackSize, slot.Itemstack.Collectible.MaxStackSize);
                        slot.MarkDirty();
                        inventory.MarkSlotDirty(slotIndex);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TrySpawnItemEntityByReflection(IWorldAccessor world, ItemStack stack, Vec3d position, IServerPlayer caster)
        {
            if (world == null || stack == null)
            {
                return false;
            }

            foreach (var method in world.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(candidate => string.Equals(candidate.Name, "SpawnItemEntity", StringComparison.OrdinalIgnoreCase)))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0 || parameters.Length > 5)
                {
                    continue;
                }

                var args = new object?[parameters.Length];
                var usable = true;
                for (var index = 0; index < parameters.Length; index++)
                {
                    var parameterType = parameters[index].ParameterType;
                    if (typeof(ItemStack).IsAssignableFrom(parameterType))
                    {
                        args[index] = stack.Clone();
                    }
                    else if (typeof(Vec3d).IsAssignableFrom(parameterType))
                    {
                        args[index] = position;
                    }
                    else if (typeof(Vec3f).IsAssignableFrom(parameterType))
                    {
                        args[index] = new Vec3f((float)position.X, (float)position.Y, (float)position.Z);
                    }
                    else if (typeof(IPlayer).IsAssignableFrom(parameterType))
                    {
                        args[index] = caster;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        args[index] = true;
                    }
                    else if (parameterType.IsValueType)
                    {
                        args[index] = Activator.CreateInstance(parameterType);
                    }
                    else
                    {
                        usable = false;
                        break;
                    }
                }

                if (!usable)
                {
                    continue;
                }

                try
                {
                    var result = method.Invoke(world, args);
                    if (result != null)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private bool TryBreakBlockWithLoot(IWorldAccessor world, BlockPos pos, IServerPlayer caster)
        {
            if (world == null || pos == null)
            {
                return false;
            }

            var accessor = world.BlockAccessor;
            var accessorType = accessor.GetType();
            var method = accessorType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, "BreakBlock", StringComparison.OrdinalIgnoreCase) || string.Equals(candidate.Name, "BreakBlockWithDrops", StringComparison.OrdinalIgnoreCase));

            if (method != null)
            {
                var parameters = method.GetParameters();
                var args = new object?[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    if (typeof(BlockPos).IsAssignableFrom(parameterType))
                    {
                        args[i] = pos;
                    }
                    else if (typeof(IPlayer).IsAssignableFrom(parameterType))
                    {
                        args[i] = caster;
                    }
                    else if (parameterType == typeof(float) || parameterType == typeof(double))
                    {
                        args[i] = 1f;
                    }
                    else
                    {
                        args[i] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
                    }
                }

                try
                {
                    method.Invoke(accessor, args);
                    return true;
                }
                catch
                {
                }
            }

            var block = world.BlockAccessor.GetBlock(pos);
            if (block == null)
            {
                return false;
            }

            world.BlockAccessor.SetBlock(0, pos);
            return true;
        }

        private ItemStack? CreateStack(string itemCode, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemCode) || quantity <= 0)
            {
                return null;
            }

            var item = sapi.World.GetItem(new AssetLocation(itemCode));
            if (item == null)
            {
                return null;
            }

            return new ItemStack(item, quantity);
        }

        private bool TryModifySatiety(Entity? targetEntity, float amount, out string failureReason)
        {
            failureReason = string.Empty;
            if (targetEntity == null || Math.Abs(amount) <= 0f)
            {
                failureReason = "that spell requires a player target";
                return false;
            }

            if (!SpellRegistry.IsPlayerEntity(targetEntity))
            {
                failureReason = "that spell requires a player target";
                return false;
            }

            if (TryAdjustNumericMember(targetEntity, amount, "Satiety", "satiety", "Saturation", "saturation", "Hunger", "hunger"))
            {
                return true;
            }

            if (TryAdjustWatchedAttributeValue(targetEntity, amount, "satiety", "saturation", "hunger"))
            {
                return true;
            }

            failureReason = "the hunger API is unavailable";
            return false;
        }

        private static bool TryAdjustWatchedAttributeValue(Entity entity, double delta, params string[] keys)
        {
            if (entity == null || keys == null || keys.Length == 0 || delta == 0d)
            {
                return false;
            }

            var watchedAttributes = entity.GetType().GetProperty("WatchedAttributes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entity);
            if (watchedAttributes == null)
            {
                return false;
            }

            var type = watchedAttributes.GetType();
            foreach (var key in keys.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                foreach (var getterName in new[] { "GetFloat", "GetDouble", "GetInt" })
                {
                    var getter = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(method => string.Equals(method.Name, getterName, StringComparison.OrdinalIgnoreCase)
                            && method.GetParameters().Length >= 1
                            && method.GetParameters()[0].ParameterType == typeof(string));
                    if (getter == null)
                    {
                        continue;
                    }

                    object? currentValue;
                    try
                    {
                        var getterArgs = getter.GetParameters().Length == 1
                            ? new object[] { key }
                            : new object[] { key, 0 };
                        currentValue = getter.Invoke(watchedAttributes, getterArgs);
                    }
                    catch
                    {
                        continue;
                    }

                    if (currentValue == null)
                    {
                        continue;
                    }

                    var updated = currentValue switch
                    {
                        float currentFloat => currentFloat + (float)delta,
                        double currentDouble => currentDouble + delta,
                        int currentInt => currentInt + (int)Math.Round(delta),
                        _ => (double?)null
                    };

                    if (updated == null)
                    {
                        continue;
                    }

                    var setter = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(method => string.Equals(method.Name, getterName.Replace("Get", "Set"), StringComparison.OrdinalIgnoreCase)
                            && method.GetParameters().Length == 2
                            && method.GetParameters()[0].ParameterType == typeof(string));
                    if (setter == null)
                    {
                        continue;
                    }

                    try
                    {
                        var parameterType = setter.GetParameters()[1].ParameterType;
                        object converted = parameterType == typeof(float)
                            ? (object)(float)updated.Value
                            : parameterType == typeof(int)
                                ? (object)(int)Math.Round(updated.Value)
                                : (object)updated.Value;
                        setter.Invoke(watchedAttributes, new[] { key, converted });
                        return true;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return false;
        }

        private static bool TryAdjustNumericMember(object target, double delta, params string[] memberNames)
        {
            if (target == null || memberNames == null || memberNames.Length == 0 || delta == 0d)
            {
                return false;
            }

            var type = target.GetType();
            foreach (var memberName in memberNames.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.CanRead == true && property.CanWrite == true)
                {
                    var current = property.GetValue(target);
                    if (current is int currentInt)
                    {
                        property.SetValue(target, currentInt + (int)Math.Round(delta));
                        return true;
                    }

                    if (current is long currentLong)
                    {
                        property.SetValue(target, currentLong + (long)Math.Round(delta));
                        return true;
                    }

                    if (current is float currentFloat)
                    {
                        property.SetValue(target, currentFloat + (float)delta);
                        return true;
                    }

                    if (current is double currentDouble)
                    {
                        property.SetValue(target, currentDouble + delta);
                        return true;
                    }
                }

                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    var current = field.GetValue(target);
                    if (current is int currentInt)
                    {
                        field.SetValue(target, currentInt + (int)Math.Round(delta));
                        return true;
                    }

                    if (current is long currentLong)
                    {
                        field.SetValue(target, currentLong + (long)Math.Round(delta));
                        return true;
                    }

                    if (current is float currentFloat)
                    {
                        field.SetValue(target, currentFloat + (float)delta);
                        return true;
                    }

                    if (current is double currentDouble)
                    {
                        field.SetValue(target, currentDouble + delta);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TrySetBooleanMember(object target, bool value, params string[] memberNames)
        {
            if (target == null || memberNames == null || memberNames.Length == 0)
            {
                return false;
            }

            var type = target.GetType();
            foreach (var memberName in memberNames.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.CanRead == true && property.CanWrite == true && property.PropertyType == typeof(bool))
                {
                    property.SetValue(target, value);
                    return true;
                }

                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(target, value);
                    return true;
                }
            }

            return false;
        }

        private static void TryMarkBlockEntityDirty(BlockEntity? blockEntity)
        {
            if (blockEntity == null)
            {
                return;
            }

            var methods = blockEntity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var method = methods.FirstOrDefault(candidate => string.Equals(candidate.Name, "MarkDirty", StringComparison.OrdinalIgnoreCase));
            if (method == null)
            {
                return;
            }

            try
            {
                var parameters = method.GetParameters();
                var args = new object?[parameters.Length];
                for (var index = 0; index < parameters.Length; index++)
                {
                    var parameterType = parameters[index].ParameterType;
                    if (parameterType == typeof(bool))
                    {
                        args[index] = true;
                    }
                    else
                    {
                        args[index] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
                    }
                }

                method.Invoke(blockEntity, args);
            }
            catch
            {
            }
        }

        private bool TrySpawnEntityByCode(string entityCode, Vec3d position, IServerPlayer caster, out Entity? spawnedEntity)
        {
            spawnedEntity = null;
            if (string.IsNullOrWhiteSpace(entityCode) || caster?.Entity == null)
            {
                return false;
            }

            var world = caster.Entity.World ?? sapi.World;
            var beforeEntities = new HashSet<long>(world.GetEntitiesAround(position, 4f, 4f)?.Select(entity => entity?.EntityId ?? -1).Where(entityId => entityId > 0) ?? Array.Empty<long>());

            foreach (var method in world.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!string.Equals(method.Name, "SpawnEntity", StringComparison.OrdinalIgnoreCase) && !string.Equals(method.Name, "SpawnEntityAt", StringComparison.OrdinalIgnoreCase) && !string.Equals(method.Name, "SpawnEntityWithCallback", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 0 || parameters.Length > 6)
                {
                    continue;
                }

                var args = new object?[parameters.Length];
                var supported = true;
                for (var index = 0; index < parameters.Length; index++)
                {
                    var parameterType = parameters[index].ParameterType;
                    if (parameterType == typeof(string))
                    {
                        args[index] = entityCode;
                    }
                    else if (parameterType == typeof(AssetLocation))
                    {
                        args[index] = new AssetLocation(entityCode);
                    }
                    else if (typeof(Vec3d).IsAssignableFrom(parameterType))
                    {
                        args[index] = position;
                    }
                    else if (typeof(Vec3f).IsAssignableFrom(parameterType))
                    {
                        args[index] = new Vec3f((float)position.X, (float)position.Y, (float)position.Z);
                    }
                    else if (typeof(IPlayer).IsAssignableFrom(parameterType))
                    {
                        args[index] = caster;
                    }
                    else if (parameterType == typeof(bool))
                    {
                        args[index] = true;
                    }
                    else if (parameterType.IsValueType)
                    {
                        args[index] = Activator.CreateInstance(parameterType);
                    }
                    else
                    {
                        args[index] = null;
                    }
                }

                try
                {
                    var result = method.Invoke(world, args);
                    if (result is Entity entityResult)
                    {
                        spawnedEntity = entityResult;
                        return true;
                    }
                }
                catch
                {
                    supported = false;
                }

                if (!supported)
                {
                    continue;
                }

                var afterEntities = world.GetEntitiesAround(position, 4f, 4f);
                if (afterEntities == null)
                {
                    continue;
                }

                var spawned = afterEntities
                    .Where(entity => entity != null && entity.EntityId > 0 && !beforeEntities.Contains(entity.EntityId))
                    .FirstOrDefault(entity => string.IsNullOrWhiteSpace(entityCode) || entity.Code?.ToString().Contains(entityCode, StringComparison.OrdinalIgnoreCase) == true);
                if (spawned != null)
                {
                    spawnedEntity = spawned;
                    return true;
                }
            }

            return false;
        }

        private bool TryAppendPlannedEffect(
            List<Func<bool>> gameplayActions,
            List<Func<bool>> ventActions,
            List<Func<bool>> visualActions,
            IServerPlayer caster,
            RustweavePlayerStateData state,
            SpellDefinition spell,
            SpellTargetContext target,
            SpellEffectDefinition effect,
            ref int corruptionDelta,
            out string failureReason)
        {
            failureReason = string.Empty;

            switch (effect.Type)
            {
                case SpellEffectTypes.ModifyTemporalStability:
                    return TryAppendAreaCorruptionEffect(gameplayActions, visualActions, caster, spell, target, effect, ref corruptionDelta, false, out failureReason);
                case SpellEffectTypes.StabilizeArea:
                    return TryAppendAreaCorruptionEffect(gameplayActions, visualActions, caster, spell, target, effect, ref corruptionDelta, true, out failureReason);
                case SpellEffectTypes.ModifyCorruptionGain:
                    return TryAppendCorruptionGainField(gameplayActions, visualActions, caster, spell, target, effect, ref corruptionDelta, out failureReason);
                case SpellEffectTypes.FreezeTemporalStabilityLoss:
                    return TryAppendFreezeTemporalStabilityLoss(gameplayActions, visualActions, caster, state, spell, target, effect, out failureReason);
                case SpellEffectTypes.BraceNextDisplacement:
                    return TryAppendBraceNextDisplacement(gameplayActions, visualActions, caster, state, spell, target, effect, out failureReason);
                case SpellEffectTypes.SenseTemporalStorm:
                    return TryAppendSenseTemporalStorm(gameplayActions, visualActions, caster, state, spell, target, effect, out failureReason);
                case SpellEffectTypes.DeferSpellCorruptionCost:
                    return TryAppendDeferSpellCorruptionCost(gameplayActions, visualActions, caster, state, spell, target, effect, out failureReason);
                case SpellEffectTypes.AnchorEntity:
                case SpellEffectTypes.PreventDisplacement:
                case SpellEffectTypes.CounterNextHostileEffect:
                case SpellEffectTypes.ReflectProjectile:
                case SpellEffectTypes.MarkTarget:
                case SpellEffectTypes.ReleaseTarget:
                case SpellEffectTypes.BreakBinding:
                case SpellEffectTypes.SeparateTarget:
                case SpellEffectTypes.IdentifyActiveEffects:
                case SpellEffectTypes.AlignNextSpell:
                case SpellEffectTypes.HealOverTime:
                case SpellEffectTypes.VitalityOverTime:
                    return TryAppendEntityFieldEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.LinkEntities:
                    return TryAppendLinkEntitiesEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.RewindEntityPosition:
                    return TryAppendRewindEntity(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.UndoRecentEffect:
                case SpellEffectTypes.CancelActiveEffect:
                    return TryAppendCancelActiveEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.WeakPointStrike:
                    return TryAppendWeakPointStrike(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.LifestealEntity:
                    return TryAppendLifeSteal(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.TetherEntity:
                case SpellEffectTypes.BindEntityToArea:
                case SpellEffectTypes.CharmEntity:
                case SpellEffectTypes.CommandEntity:
                    return TryAppendControlledEntityField(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.TeleportToTarget:
                case SpellEffectTypes.TeleportForward:
                    return TryAppendTeleportToTarget(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.TeleportEntityToCaster:
                case SpellEffectTypes.SwapPositions:
                    return TryAppendTeleportEntityToCaster(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.TeleportEntityToPosition:
                    return TryAppendTeleportEntityToPosition(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.PushEntity:
                case SpellEffectTypes.ForcePulse:
                case SpellEffectTypes.Shockwave:
                case SpellEffectTypes.PressureBlast:
                case SpellEffectTypes.StaggerArea:
                    return TryAppendForceEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.MoveDroppedItem:
                    return TryAppendMoveDroppedItem(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.MoveBlockEntityContents:
                    return TryAppendMoveBlockEntityContents(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.OpenLock:
                case SpellEffectTypes.OpenPassage:
                case SpellEffectTypes.CreateRift:
                case SpellEffectTypes.CloseRift:
                case SpellEffectTypes.RepairBlock:
                case SpellEffectTypes.RepairBlockArea:
                case SpellEffectTypes.ConvertBlock:
                case SpellEffectTypes.DestroyCorruptedMatter:
                case SpellEffectTypes.PrecisionBlockStrike:
                case SpellEffectTypes.AnchorBlock:
                case SpellEffectTypes.HardenMaterial:
                case SpellEffectTypes.HeatMaterial:
                case SpellEffectTypes.CoolMaterial:
                case SpellEffectTypes.AccelerateCraftState:
                case SpellEffectTypes.MineBlock:
                case SpellEffectTypes.ExcavateBlocks:
                case SpellEffectTypes.ExtractOre:
                case SpellEffectTypes.HarvestBlocks:
                    return TryAppendBlockEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.ConvertHeldItem:
                case SpellEffectTypes.SummonTemporaryItem:
                case SpellEffectTypes.CreateTemporaryFood:
                    return TryAppendHeldOrInventoryEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.SummonTemporaryEntity:
                case SpellEffectTypes.SummonTemporaryProjectile:
                case SpellEffectTypes.SummonTemporaryConstruct:
                    return TryAppendSummonEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.DetectBlocks:
                case SpellEffectTypes.DetectEntities:
                case SpellEffectTypes.DetectRustTraces:
                case SpellEffectTypes.ReadGlyphs:
                    return TryAppendDetectionEffect(visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.ModifyCropGrowth:
                case SpellEffectTypes.ModifyFarmlandNutrients:
                case SpellEffectTypes.ModifyAnimalFertility:
                case SpellEffectTypes.ModifySatiety:
                    return TryAppendGraftingEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.ChangeWeather:
                case SpellEffectTypes.ChangeTemperatureArea:
                case SpellEffectTypes.ChangeEnvironmentalPressure:
                case SpellEffectTypes.CallLightning:
                case SpellEffectTypes.StormPulse:
                    return TryAppendWeatherEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                case SpellEffectTypes.PurgeTimedEffects:
                case SpellEffectTypes.StripEntityBuffs:
                case SpellEffectTypes.CleanseContamination:
                case SpellEffectTypes.UnravelDamage:
                    return TryAppendCleanupEffect(gameplayActions, visualActions, caster, spell, target, effect, out failureReason);
                default:
                    failureReason = $"unsupported effect type '{effect.Type}'";
                    sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed because effect type '{1}' is unsupported at runtime.", spell.Code, effect.Type);
                    return false;
            }
        }

        private bool TryAppendAreaCorruptionEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, ref int corruptionDelta, bool stabilize, out string failureReason)
        {
            failureReason = string.Empty;
            var radius = SpellRegistry.GetEffectiveRadius(spell, effect);
            if (radius <= 0)
            {
                failureReason = "radius must be greater than zero";
                return false;
            }

            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0)
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var amount = effect.Amount != 0f ? effect.Amount : (float)effect.StabilityAmount;
            if (amount == 0f && effect.CorruptionAmount != 0)
            {
                amount = effect.CorruptionAmount;
            }

            var center = target.Position;
            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, caster.Entity?.EntityId ?? 0);
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "area",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = target.Entity is EntityPlayer targetPlayer ? targetPlayer.PlayerUID ?? string.Empty : string.Empty,
                TargetEntityId = target.Entity?.EntityId ?? -1,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = center.X,
                CenterY = center.Y,
                CenterZ = center.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? center.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? center.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? center.Z,
                Radius = radius,
                Amount = stabilize ? Math.Abs(amount) : amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                ExpiresAtMilliseconds = sapi.World.ElapsedMilliseconds + durationMilliseconds,
                StartedAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d),
                ExpiresAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d) + (durationMilliseconds / 86400000d),
                TickIntervalMilliseconds = 1000,
                NextTickAtMilliseconds = sapi.World.ElapsedMilliseconds + 1000,
                PersistAcrossRestart = true,
                IsArea = true,
                IsHostile = !stabilize && amount < 0,
                IsBeneficial = stabilize || amount > 0,
                IsBlocking = string.Equals(effect.Type, SpellEffectTypes.CreateWardArea, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Type, SpellEffectTypes.CreateBarrier, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Type, SpellEffectTypes.CreateContainmentArea, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Type, SpellEffectTypes.CreateBoundaryLine, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Type, SpellEffectTypes.CreateAntiSpreadArea, StringComparison.OrdinalIgnoreCase)
            };

            gameplayActions.Add(() =>
            {
                RustweaveRuntime.Server?.TryRegisterActiveEffect(record);
                return true;
            });

            visualActions.Add(() =>
            {
                var color = stabilize || amount > 0 ? unchecked((int)0xFF67D46A) : unchecked((int)0xFFC95A4A);
                sapi.World.SpawnParticles(8, color, new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.2, center.Z + 0.1), new Vec3f(-0.02f, 0.03f, -0.02f), new Vec3f(0.02f, 0.08f, 0.02f), 0.35f, 0f, 0.5f, EnumParticleModel.Quad, caster);
                return true;
            });

            if (!stabilize)
            {
                corruptionDelta += (int)Math.Round(effect.CorruptionAmount != 0 ? effect.CorruptionAmount : effect.Amount);
            }

            return true;
        }

        private bool TryAppendCorruptionGainField(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, ref int corruptionDelta, out string failureReason)
        {
            failureReason = string.Empty;
            var radius = SpellRegistry.GetEffectiveRadius(spell, effect);
            if (radius <= 0)
            {
                failureReason = "radius must be greater than zero";
                return false;
            }

            if (!TryAppendAreaCorruptionEffect(gameplayActions, visualActions, caster, spell, target, effect, ref corruptionDelta, effect.Amount < 0f || effect.StabilityAmount < 0d, out failureReason))
            {
                return false;
            }

            return true;
        }

        private bool TryAppendEntityFieldEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                return false;
            }

            if (targetEntity is EntityPlayer && (string.Equals(effect.Type, SpellEffectTypes.CharmEntity, StringComparison.OrdinalIgnoreCase) || string.Equals(effect.Type, SpellEffectTypes.CommandEntity, StringComparison.OrdinalIgnoreCase)))
            {
                failureReason = "that spell cannot target players";
                return false;
            }

            if (IsPlayerTargetBlockedByPvp(caster, targetEntity, effect.Type, out failureReason))
            {
                return false;
            }

            var visualTargetEntity = targetEntity;
            var visualTargetCenter = visualTargetEntity.Pos.XYZ;
            var durationMilliseconds = (long)Math.Round(Math.Max(0d, effect.DurationSeconds) * 1000d);
            if (durationMilliseconds <= 0 && !string.Equals(effect.Type, SpellEffectTypes.CounterNextHostileEffect, StringComparison.OrdinalIgnoreCase) && !string.Equals(effect.Type, SpellEffectTypes.ReflectProjectile, StringComparison.OrdinalIgnoreCase) && !string.Equals(effect.Type, SpellEffectTypes.UndoRecentEffect, StringComparison.OrdinalIgnoreCase) && !string.Equals(effect.Type, SpellEffectTypes.CancelActiveEffect, StringComparison.OrdinalIgnoreCase))
            {
                failureReason = "durationSeconds must be greater than zero";
                return false;
            }

            var effectId = BuildTimedEffectCode(spell.Code, effect.Type, targetEntity.EntityId);
            var record = new RustweaveActiveEffectRecord
            {
                EffectId = effectId,
                EffectType = effect.Type,
                RecordKind = "entity",
                SpellCode = spell.Code,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity?.EntityId ?? -1,
                TargetPlayerUid = targetEntity is EntityPlayer targetPlayer ? targetPlayer.PlayerUID ?? string.Empty : string.Empty,
                TargetEntityId = visualTargetEntity.EntityId,
                TargetType = spell.TargetType,
                Mode = effect.Mode,
                CenterX = visualTargetCenter.X,
                CenterY = visualTargetCenter.Y,
                CenterZ = visualTargetCenter.Z,
                OriginX = caster.Entity?.Pos?.XYZ.X ?? target.Position.X,
                OriginY = caster.Entity?.Pos?.XYZ.Y ?? target.Position.Y,
                OriginZ = caster.Entity?.Pos?.XYZ.Z ?? target.Position.Z,
                Radius = Math.Max(1d, effect.Radius),
                Amount = string.Equals(effect.Type, SpellEffectTypes.HealOverTime, StringComparison.OrdinalIgnoreCase) || string.Equals(effect.Type, SpellEffectTypes.VitalityOverTime, StringComparison.OrdinalIgnoreCase)
                    ? -Math.Abs(effect.Amount != 0f ? effect.Amount : effect.HealthAmount)
                    : effect.Amount,
                SecondaryAmount = effect.SecondaryAmount,
                DurationMilliseconds = durationMilliseconds,
                StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                ExpiresAtMilliseconds = durationMilliseconds > 0 ? sapi.World.ElapsedMilliseconds + durationMilliseconds : sapi.World.ElapsedMilliseconds + 1000,
                StartedAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d),
                ExpiresAtTotalDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d) + (Math.Max(1d, effect.DurationSeconds) / 24d),
                TickIntervalMilliseconds = Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                NextTickAtMilliseconds = sapi.World.ElapsedMilliseconds + Math.Max(250, (long)Math.Round(Math.Max(0.25d, effect.TickIntervalSeconds) * 1000d)),
                PersistAcrossRestart = true,
                IsArea = false,
                IsHostile = effect.Type is SpellEffectTypes.AnchorEntity or SpellEffectTypes.PreventDisplacement or SpellEffectTypes.TetherEntity or SpellEffectTypes.BindEntityToArea or SpellEffectTypes.CharmEntity or SpellEffectTypes.CommandEntity,
                IsBeneficial = effect.Type is SpellEffectTypes.MarkTarget or SpellEffectTypes.CounterNextHostileEffect or SpellEffectTypes.ReflectProjectile or SpellEffectTypes.ReleaseTarget or SpellEffectTypes.BreakBinding or SpellEffectTypes.PurgeTimedEffects or SpellEffectTypes.StripEntityBuffs or SpellEffectTypes.CleanseContamination or SpellEffectTypes.HealOverTime or SpellEffectTypes.VitalityOverTime or SpellEffectTypes.AlignNextSpell,
                IsBlocking = effect.Type is SpellEffectTypes.AnchorEntity or SpellEffectTypes.PreventDisplacement or SpellEffectTypes.TetherEntity or SpellEffectTypes.BindEntityToArea
            };

            gameplayActions.Add(() =>
            {
                RustweaveRuntime.Server?.TryRegisterActiveEffect(record);
                return true;
            });

            visualActions.Add(() =>
            {
                var color = record.IsHostile ? unchecked((int)0xFFC94A4A) : unchecked((int)0xFF6ACB71);
                sapi.World.SpawnParticles(10, color, new Vec3d(visualTargetEntity.Pos.XYZ.X - 0.15, visualTargetEntity.Pos.XYZ.Y + 0.1, visualTargetEntity.Pos.XYZ.Z - 0.15), new Vec3d(visualTargetEntity.Pos.XYZ.X + 0.15, visualTargetEntity.Pos.XYZ.Y + 1.4, visualTargetEntity.Pos.XYZ.Z + 0.15), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.05f, 0.01f), 0.3f, 0f, 0.45f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendRewindEntity(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity;
            if (targetEntity == null || !targetEntity.Alive)
            {
                failureReason = "the targeted entity is unavailable";
                return false;
            }

            var historyPosition = new Vec3d();
            var hasHistory = RustweaveRuntime.Server != null
                && RustweaveRuntime.Server.TryGetHistoricalPosition(targetEntity.EntityId, Math.Max(1, (int)Math.Round(Math.Max(1d, effect.DurationSeconds * 2d))), out historyPosition);
            if (!hasHistory)
            {
                failureReason = "the entity has no recent history to rewind";
                return false;
            }

            gameplayActions.Add(() =>
            {
                if (RustweaveRuntime.Server?.TryFindNearestSafeTeleportPosition(targetEntity.World, targetEntity, historyPosition, out var safePosition) != true)
                {
                    return false;
                }

                if (TryConsumeDisplacementBraceIfNeeded(caster, spell, effect.Type, targetEntity))
                {
                    return true;
                }

                targetEntity.TeleportTo(safePosition);
                return true;
            });

            visualActions.Add(() =>
            {
                sapi.World.SpawnParticles(8, unchecked((int)0xFF8C6A4A), new Vec3d(historyPosition.X - 0.2, historyPosition.Y + 0.1, historyPosition.Z - 0.2), new Vec3d(historyPosition.X + 0.2, historyPosition.Y + 0.5, historyPosition.Z + 0.2), new Vec3f(-0.02f, 0.02f, -0.02f), new Vec3f(0.02f, 0.06f, 0.02f), 0.35f, 0f, 0.5f, EnumParticleModel.Quad, caster);
                return true;
            });

            return true;
        }

        private bool TryAppendCancelActiveEffect(List<Func<bool>> gameplayActions, List<Func<bool>> visualActions, IServerPlayer caster, SpellDefinition spell, SpellTargetContext target, SpellEffectDefinition effect, out string failureReason)
        {
            failureReason = string.Empty;
            var targetEntity = target.Entity;
            if (targetEntity == null && target.BlockPos == null)
            {
                failureReason = "no valid target";
                return false;
            }

            gameplayActions.Add(() =>
            {
                if (targetEntity != null)
                {
                    var activeEffects = RustweaveRuntime.Server?.GetActiveEffectsForEntity(targetEntity.EntityId) ?? Array.Empty<RustweaveActiveEffectRecord>();
                    var latest = activeEffects.LastOrDefault(effectRecord => string.Equals(effectRecord.CasterPlayerUid, caster.PlayerUID, StringComparison.OrdinalIgnoreCase) || string.Equals(effectRecord.TargetPlayerUid, caster.PlayerUID, StringComparison.OrdinalIgnoreCase));
                    if (latest != null)
                    {
                        RustweaveRuntime.Server?.TryRemoveActiveEffect(latest.EffectId, true);
                        return true;
                    }
                }

                return true;
            });

            visualActions.Add(() =>
            {
                var center = target.Position;
                sapi.World.SpawnParticles(6, unchecked((int)0xFF8C6A4A), new Vec3d(center.X - 0.1, center.Y + 0.1, center.Z - 0.1), new Vec3d(center.X + 0.1, center.Y + 0.2, center.Z + 0.1), new Vec3f(-0.01f, 0.02f, -0.01f), new Vec3f(0.01f, 0.05f, 0.01f), 0.25f, 0f, 0.35f, EnumParticleModel.Quad, caster);
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

            if (!sapi.Server.Config.AllowPvP && targetEntity is EntityPlayer && targetEntity != caster.Entity)
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

        private bool IsPlayerTargetBlockedByPvp(IServerPlayer caster, Entity? targetEntity, string effectLabel, out string failureReason)
        {
            failureReason = string.Empty;
            if (targetEntity is EntityPlayer && targetEntity != caster.Entity && !sapi.Server.Config.AllowPvP)
            {
                failureReason = "PvP is disabled on this server.";
                sapi.Logger.Warning("[TheRustweave] {0} failed because PvP is disabled for player '{1}' targeting entity '{2}'.", effectLabel, caster.PlayerUID, targetEntity.EntityId);
                return true;
            }

            return false;
        }

        private bool TryResolveLookEntityTarget(IServerPlayer caster, double range, out EntitySelection selection, out string failureReason, System.Func<Entity, bool>? entityFilter = null)
        {
            selection = new EntitySelection();
            failureReason = string.Empty;

            var entity = caster.Entity;
            if (entity == null)
            {
                failureReason = "caster entity is missing";
                return false;
            }

            if (!TryResolveLookSelection(caster, range, out var blockSelection, out var entitySelection, out failureReason, entityFilter))
            {
                return false;
            }

            if (entitySelection?.Entity == null)
            {
                failureReason = "No target struck.";
                return false;
            }

            selection = entitySelection;
            sapi.Logger.Debug("[TheRustweave] Spell look ray range {0} hit entity '{1}'.", range, entitySelection.Entity.GetName());
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
            position = new Vec3d(blockSelection.Position.X + 0.5, blockSelection.Position.Y + 1.0, blockSelection.Position.Z + 0.5);
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
                    && candidate.Alive);

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

        private bool TryResolveLookSelection(IServerPlayer caster, double range, out BlockSelection? blockSelection, out EntitySelection? entitySelection, out string failureReason, System.Func<Entity, bool>? entityFilter = null)
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
                    && (entityFilter == null || entityFilter(candidate)));

            if (blockSelection == null && entitySelection?.Entity == null)
            {
                failureReason = "No target struck.";
                return false;
            }

            return true;
        }

        private bool TryResolveLookBlockEntityTarget(IServerPlayer caster, double range, bool requireContainer, out BlockPos? blockPos, out Vec3d position, out BlockEntity? blockEntity, out string failureReason)
        {
            blockPos = null;
            position = new Vec3d();
            blockEntity = null;
            failureReason = string.Empty;

            if (!TryResolveLookBlockTarget(caster, range, out blockPos, out position, out failureReason))
            {
                return false;
            }

            if (blockPos == null)
            {
                failureReason = "No target struck.";
                return false;
            }

            blockEntity = sapi.World.BlockAccessor.GetBlockEntity(blockPos);
            if (blockEntity == null)
            {
                failureReason = requireContainer ? "That target requires a container." : "That target requires a block entity.";
                return false;
            }

            if (requireContainer && !SpellRegistry.TryGetBlockEntityInventory(blockEntity, out _))
            {
                failureReason = "That target requires a container.";
                return false;
            }

            return true;
        }

        private bool TryResolveLookAreaTarget(IServerPlayer caster, double range, out Vec3d position, out string failureReason)
        {
            position = new Vec3d();
            failureReason = string.Empty;

            if (!TryResolveLookSelection(caster, range, out var blockSelection, out var entitySelection, out failureReason))
            {
                return false;
            }

            if (blockSelection != null)
            {
                position = new Vec3d(blockSelection.Position.X + 0.5, blockSelection.Position.Y + 1.0, blockSelection.Position.Z + 0.5);
                return true;
            }

            if (entitySelection?.Entity != null)
            {
                position = entitySelection.HitPosition;
                return true;
            }

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
            position = new Vec3d(
                fromPos.X + (viewVector.X * rayRange),
                fromPos.Y + (viewVector.Y * rayRange),
                fromPos.Z + (viewVector.Z * rayRange));
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
                    && candidate != caster.Entity);

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
            var supportPos = new BlockPos((int)Math.Floor(candidate.X), (int)Math.Floor(candidate.Y) - 1, (int)Math.Floor(candidate.Z), dim);

            return IsSpaceClear(world, feetPos) && IsSpaceClear(world, headPos) && IsTeleportSupportSafe(world, supportPos);
        }

        private static bool IsSpaceClear(IWorldAccessor world, BlockPos pos)
        {
            var block = world.BlockAccessor.GetBlock(pos);
            return block == null || block.CollisionBoxes == null || block.CollisionBoxes.Length == 0;
        }

        private static bool IsTeleportSupportSafe(IWorldAccessor world, BlockPos pos)
        {
            var block = world.BlockAccessor.GetBlock(pos);
            return block != null && block.CollisionBoxes != null && block.CollisionBoxes.Length > 0;
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

            public BlockEntity? BlockEntity { get; set; }

            public string TargetName { get; set; } = string.Empty;

            public Vec3d Position { get; set; } = new Vec3d();
        }
    }
}

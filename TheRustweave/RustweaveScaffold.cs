using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using ProtoBuf;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using SpellRegistryType = TheRustweave.SpellRegistry;

namespace TheRustweave
{
    internal static class RustweaveConstants
    {
        public const string ModId = "therustweave";
        public const string NetworkChannelName = "therustweave";
        public const string ProgressionConfigFileName = "therustweave-progression.json";
        public const string MentorConfigFileName = "therustweave-mentor.json";
        public const string DiscoveryConfigFileName = "therustweave-discovery.json";
        public const string ActiveEffectConfigFileName = "therustweave-activeeffects.json";
        public const string DiscoveryConfigAsset = "therustweave:config/therustweave-discovery.json";
        public const string TomeItemCode = "rustweaverstome";
        public const string TomeItemClass = "ItemRustweaverTome";
        public const string TabletItemCode = "rusttablet";
        public const string TabletItemClass = "ItemRustTablet";
        public const string DiscoveryItemClass = "ItemRustweaveDiscoveryItem";
        public const string SpellRegistryAsset = "therustweave:config/spells.json";
        public const string PlayerStateModDataKey = "therustweave:playerstate";
        public const string CastStateModDataKey = "therustweave:caststate";
        public const string WatchedPlayerStateKey = "therustweave:playerstate";
        public const string WatchedCastStateKey = "therustweave:caststate";
        public const string TabletStoredCorruptionKey = "therustweave:tabletcorruption";
        public const string TabletLastDecayTotalDaysKey = "therustweave:tabletdecaydays";
        public const string DummySpellCode = "dummy-rustcall";
        public const string ForgottenBookItemCode = "forgotten-book";
        public const string ArcaneNotesItemCode = "arcane-notes";
        public const string RustplanePrismItemCode = "rustplane-prism";
        public const string AncientCodexItemCode = "ancient-codex";
        public const string ScrollFromTheRustItemCode = "scroll-from-the-rust";
        public const int DefaultCorruption = 0;
        public const int DefaultThreshold = 200;
        public const int DefaultCap = 1000;
        public const int TabletCapacity = 400;
        public const int TabletVentingRatePerSecond = 10;
        public const int TabletDecayPerDay = 4;
        public const long OverloadDurationMilliseconds = 120000;
        public const int PreparedSlotCount = 9;
        public const string RustMendSpellCode = "rust-mend";
        public const string MentorStudySource = RustweaveMentorTypes.RustplaneSageStudySource;

        // Change this list when adding or removing starter spells.
        public static readonly string[] StarterSpellCodes = { RustMendSpellCode };

        public static readonly string[] DiscoveryItemCodes =
        {
            ForgottenBookItemCode,
            ArcaneNotesItemCode,
            RustplanePrismItemCode,
            AncientCodexItemCode,
            ScrollFromTheRustItemCode
        };

        public static bool AllSpellsLearnedByDefaultForTesting => RustweaveRuntime.ProgressionConfig?.AllSpellsLearnedByDefaultForTesting ?? false;
    }

    internal sealed class RustweaveProgressionConfig
    {
        /// <summary>
        /// Set true only while testing spell effects and Tome slot behavior.
        /// Set false for survival progression testing.
        /// </summary>
        public bool AllSpellsLearnedByDefaultForTesting { get; set; } = false;
    }

    internal sealed class RustweaveDiscoveryConfig
    {
        [JsonProperty("discoveryItems")]
        public Dictionary<string, RustweaveDiscoveryItemDefinition> DiscoveryItems { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class RustweaveDiscoveryItemDefinition
    {
        [JsonProperty("allowedSchools")]
        public List<string> AllowedSchools { get; set; } = new();

        [JsonProperty("allowedTiers")]
        public List<int> AllowedTiers { get; set; } = new();

        [JsonProperty("consumeOnSuccess")]
        public bool ConsumeOnSuccess { get; set; }

        [JsonProperty("singleUsePerPlayer")]
        public bool SingleUsePerPlayer { get; set; }
    }

    internal static class RustweaveKnowledgeRanks
    {
        private static readonly string[] Names =
        {
            "Rustweave Initiate",
            "Rustweave Practitioner",
            "Adept Rustweaver",
            "Rustweave Scholar",
            "Master Rustweaver",
            "Ascendant Rustweaver"
        };

        public static string GetName(int index)
        {
            if (index < 0)
            {
                return Names[0];
            }

            return index >= Names.Length ? Names[Names.Length - 1] : Names[index];
        }
    }

    [ProtoContract]
    internal enum RustweaveActionType
    {
        [ProtoEnum]
        RequestStartCast = 0,

        [ProtoEnum]
        RequestCancelCast = 1,

        [ProtoEnum]
        RequestSelectPrepared = 2,

        [ProtoEnum]
        RequestPrepareSpell = 3,

        [ProtoEnum]
        RequestUnprepareSpell = 4
    }

    [ProtoContract]
    internal sealed class RustweaveActionPacket
    {
        [ProtoMember(1)]
        public RustweaveActionType Action { get; set; }

        [ProtoMember(2)]
        public string SpellCode { get; set; } = string.Empty;

        [ProtoMember(3)]
        public int SlotIndex { get; set; } = -1;

        [ProtoMember(4)]
        public int Delta { get; set; }
    }

    [ProtoContract]
    internal sealed class RustweaveTargetLockPacket
    {
        [ProtoMember(1)]
        public bool IsActive { get; set; }

        [ProtoMember(2)]
        public string SpellCode { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string SpellName { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string TargetType { get; set; } = string.Empty;

        [ProtoMember(5)]
        public string TargetName { get; set; } = string.Empty;

        [ProtoMember(6)]
        public long TargetEntityId { get; set; } = -1;

        [ProtoMember(7)]
        public double TargetX { get; set; }

        [ProtoMember(8)]
        public double TargetY { get; set; }

        [ProtoMember(9)]
        public double TargetZ { get; set; }

        [ProtoMember(10)]
        public long CastDurationMs { get; set; }

        [ProtoMember(11)]
        public long CastStartedAtMs { get; set; }

        public RustweaveTargetLockPacket Clone()
        {
            return new RustweaveTargetLockPacket
            {
                IsActive = IsActive,
                SpellCode = SpellCode,
                SpellName = SpellName,
                TargetType = TargetType,
                TargetName = TargetName,
                TargetEntityId = TargetEntityId,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                CastDurationMs = CastDurationMs,
                CastStartedAtMs = CastStartedAtMs
            };
        }
    }

    [ProtoContract]
    internal sealed class RustweaveTargetPreviewPacket
    {
        [ProtoMember(1)]
        public bool IsActive { get; set; }

        [ProtoMember(2)]
        public bool IsLocked { get; set; }

        [ProtoMember(3)]
        public string CasterPlayerUid { get; set; } = string.Empty;

        [ProtoMember(4)]
        public long CasterEntityId { get; set; } = -1;

        [ProtoMember(5)]
        public string SpellCode { get; set; } = string.Empty;

        [ProtoMember(6)]
        public string SpellName { get; set; } = string.Empty;

        [ProtoMember(7)]
        public string PreviewMode { get; set; } = string.Empty;

        [ProtoMember(8)]
        public string TargetType { get; set; } = string.Empty;

        [ProtoMember(9)]
        public string ColorClass { get; set; } = string.Empty;

        [ProtoMember(10)]
        public string TargetName { get; set; } = string.Empty;

        [ProtoMember(11)]
        public long TargetEntityId { get; set; } = -1;

        [ProtoMember(12)]
        public double SourceX { get; set; }

        [ProtoMember(13)]
        public double SourceY { get; set; }

        [ProtoMember(14)]
        public double SourceZ { get; set; }

        [ProtoMember(15)]
        public double TargetX { get; set; }

        [ProtoMember(16)]
        public double TargetY { get; set; }

        [ProtoMember(17)]
        public double TargetZ { get; set; }

        [ProtoMember(18)]
        public double ImpactX { get; set; }

        [ProtoMember(19)]
        public double ImpactY { get; set; }

        [ProtoMember(20)]
        public double ImpactZ { get; set; }

        [ProtoMember(21)]
        public double Radius { get; set; }

        [ProtoMember(22)]
        public double Width { get; set; }

        [ProtoMember(23)]
        public double Length { get; set; }

        [ProtoMember(24)]
        public bool UsesGravity { get; set; }

        [ProtoMember(25)]
        public bool ShowImpactPoint { get; set; }

        [ProtoMember(26)]
        public string MarkerStyle { get; set; } = string.Empty;

        [ProtoMember(27)]
        public long UpdatedAtMs { get; set; }

        public RustweaveTargetPreviewPacket Clone()
        {
            return new RustweaveTargetPreviewPacket
            {
                IsActive = IsActive,
                IsLocked = IsLocked,
                CasterPlayerUid = CasterPlayerUid,
                CasterEntityId = CasterEntityId,
                SpellCode = SpellCode,
                SpellName = SpellName,
                PreviewMode = PreviewMode,
                TargetType = TargetType,
                ColorClass = ColorClass,
                TargetName = TargetName,
                TargetEntityId = TargetEntityId,
                SourceX = SourceX,
                SourceY = SourceY,
                SourceZ = SourceZ,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                ImpactX = ImpactX,
                ImpactY = ImpactY,
                ImpactZ = ImpactZ,
                Radius = Radius,
                Width = Width,
                Length = Length,
                UsesGravity = UsesGravity,
                ShowImpactPoint = ShowImpactPoint,
                MarkerStyle = MarkerStyle,
                UpdatedAtMs = UpdatedAtMs
            };
        }
    }

    [ProtoContract]
    internal sealed class RustweaveTargetedWarningPacket
    {
        [ProtoMember(1)]
        public bool IsActive { get; set; }

        [ProtoMember(2)]
        public string CastId { get; set; } = string.Empty;

        [ProtoMember(3)]
        public long CasterEntityId { get; set; } = -1;

        [ProtoMember(4)]
        public long TargetEntityId { get; set; } = -1;

        [ProtoMember(5)]
        public string TargetPlayerUid { get; set; } = string.Empty;

        [ProtoMember(6)]
        public string SpellSchool { get; set; } = string.Empty;

        [ProtoMember(7)]
        public string WarningType { get; set; } = string.Empty;

        [ProtoMember(8)]
        public string TargetType { get; set; } = string.Empty;

        [ProtoMember(9)]
        public double OriginX { get; set; }

        [ProtoMember(10)]
        public double OriginY { get; set; }

        [ProtoMember(11)]
        public double OriginZ { get; set; }

        [ProtoMember(12)]
        public double TargetX { get; set; }

        [ProtoMember(13)]
        public double TargetY { get; set; }

        [ProtoMember(14)]
        public double TargetZ { get; set; }

        [ProtoMember(15)]
        public long StartedAtMs { get; set; }

        [ProtoMember(16)]
        public long ExpectedEndAtMs { get; set; }

        [ProtoMember(17)]
        public long ExpiresAtMs { get; set; }

        [ProtoMember(18)]
        public bool IsPersonalWarning { get; set; }

        public RustweaveTargetedWarningPacket Clone()
        {
            return new RustweaveTargetedWarningPacket
            {
                IsActive = IsActive,
                CastId = CastId,
                CasterEntityId = CasterEntityId,
                TargetEntityId = TargetEntityId,
                TargetPlayerUid = TargetPlayerUid,
                SpellSchool = SpellSchool,
                WarningType = WarningType,
                TargetType = TargetType,
                OriginX = OriginX,
                OriginY = OriginY,
                OriginZ = OriginZ,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                StartedAtMs = StartedAtMs,
                ExpectedEndAtMs = ExpectedEndAtMs,
                ExpiresAtMs = ExpiresAtMs,
                IsPersonalWarning = IsPersonalWarning
            };
        }
    }

    internal sealed class RustweaveActiveCastRecord
    {
        public string CastId { get; set; } = string.Empty;

        public string CasterPlayerUid { get; set; } = string.Empty;

        public long CasterEntityId { get; set; } = -1;

        public string TargetPlayerUid { get; set; } = string.Empty;

        public long TargetEntityId { get; set; } = -1;

        public string SpellCode { get; set; } = string.Empty;

        public string SpellSchool { get; set; } = string.Empty;

        public string WarningType { get; set; } = SpellWarningTypes.Neutral;

        public string TargetType { get; set; } = string.Empty;

        public long StartedAtMs { get; set; }

        public long ExpectedEndAtMs { get; set; }

        public long ExpiresAtMs { get; set; }

        public double OriginX { get; set; }

        public double OriginY { get; set; }

        public double OriginZ { get; set; }

        public double TargetX { get; set; }

        public double TargetY { get; set; }

        public double TargetZ { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPersonalWarning { get; set; }

        public List<string> ObserverPlayerUids { get; set; } = new();

        public RustweaveTargetedWarningPacket ToPacket()
        {
            return new RustweaveTargetedWarningPacket
            {
                IsActive = IsActive,
                CastId = CastId,
                CasterEntityId = CasterEntityId,
                TargetEntityId = TargetEntityId,
                TargetPlayerUid = TargetPlayerUid,
                SpellSchool = SpellSchool,
                WarningType = WarningType,
                TargetType = TargetType,
                OriginX = OriginX,
                OriginY = OriginY,
                OriginZ = OriginZ,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                StartedAtMs = StartedAtMs,
                ExpectedEndAtMs = ExpectedEndAtMs,
                ExpiresAtMs = ExpiresAtMs,
                IsPersonalWarning = IsPersonalWarning
            };
        }

        public RustweaveActiveCastRecord Clone()
        {
            return new RustweaveActiveCastRecord
            {
                CastId = CastId,
                CasterPlayerUid = CasterPlayerUid,
                CasterEntityId = CasterEntityId,
                TargetPlayerUid = TargetPlayerUid,
                TargetEntityId = TargetEntityId,
                SpellCode = SpellCode,
                SpellSchool = SpellSchool,
                WarningType = WarningType,
                TargetType = TargetType,
                StartedAtMs = StartedAtMs,
                ExpectedEndAtMs = ExpectedEndAtMs,
                ExpiresAtMs = ExpiresAtMs,
                OriginX = OriginX,
                OriginY = OriginY,
                OriginZ = OriginZ,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                IsActive = IsActive,
                IsPersonalWarning = IsPersonalWarning,
                ObserverPlayerUids = ObserverPlayerUids?.ToList() ?? new List<string>()
            };
        }
    }

    internal sealed class RustweaveBlockSnapshot
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }

        [JsonProperty("dimension")]
        public int Dimension { get; set; } = -1;

        [JsonProperty("blockCode")]
        public string BlockCode { get; set; } = string.Empty;

        [JsonProperty("blockId")]
        public int BlockId { get; set; } = 0;
    }

    internal sealed class RustweaveActiveEffectRecord
    {
        [JsonProperty("effectId")]
        public string EffectId { get; set; } = string.Empty;

        [JsonProperty("effectType")]
        public string EffectType { get; set; } = string.Empty;

        [JsonProperty("recordKind")]
        public string RecordKind { get; set; } = string.Empty;

        [JsonProperty("spellCode")]
        public string SpellCode { get; set; } = string.Empty;

        [JsonProperty("casterPlayerUid")]
        public string CasterPlayerUid { get; set; } = string.Empty;

        [JsonProperty("casterEntityId")]
        public long CasterEntityId { get; set; } = -1;

        [JsonProperty("targetPlayerUid")]
        public string TargetPlayerUid { get; set; } = string.Empty;

        [JsonProperty("targetEntityId")]
        public long TargetEntityId { get; set; } = -1;

        [JsonProperty("targetType")]
        public string TargetType { get; set; } = string.Empty;

        [JsonProperty("mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonProperty("centerX")]
        public double CenterX { get; set; }

        [JsonProperty("centerY")]
        public double CenterY { get; set; }

        [JsonProperty("centerZ")]
        public double CenterZ { get; set; }

        [JsonProperty("originX")]
        public double OriginX { get; set; }

        [JsonProperty("originY")]
        public double OriginY { get; set; }

        [JsonProperty("originZ")]
        public double OriginZ { get; set; }

        [JsonProperty("targetX")]
        public double TargetX { get; set; }

        [JsonProperty("targetY")]
        public double TargetY { get; set; }

        [JsonProperty("targetZ")]
        public double TargetZ { get; set; }

        [JsonProperty("radius")]
        public double Radius { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("amount")]
        public float Amount { get; set; }

        [JsonProperty("secondaryAmount")]
        public float SecondaryAmount { get; set; }

        [JsonProperty("durationMilliseconds")]
        public long DurationMilliseconds { get; set; }

        [JsonProperty("startedAtMilliseconds")]
        public long StartedAtMilliseconds { get; set; }

        [JsonProperty("expiresAtMilliseconds")]
        public long ExpiresAtMilliseconds { get; set; }

        [JsonProperty("startedAtTotalDays")]
        public double StartedAtTotalDays { get; set; }

        [JsonProperty("expiresAtTotalDays")]
        public double ExpiresAtTotalDays { get; set; }

        [JsonProperty("tickIntervalMilliseconds")]
        public long TickIntervalMilliseconds { get; set; }

        [JsonProperty("nextTickAtMilliseconds")]
        public long NextTickAtMilliseconds { get; set; }

        [JsonProperty("persistAcrossRestart")]
        public bool PersistAcrossRestart { get; set; }

        [JsonProperty("isArea")]
        public bool IsArea { get; set; }

        [JsonProperty("isHostile")]
        public bool IsHostile { get; set; }

        [JsonProperty("isBeneficial")]
        public bool IsBeneficial { get; set; }

        [JsonProperty("isBlocking")]
        public bool IsBlocking { get; set; }

        [JsonProperty("blockSnapshots")]
        public List<RustweaveBlockSnapshot> BlockSnapshots { get; set; } = new();

        [JsonProperty("affectedEntityIds")]
        public List<long> AffectedEntityIds { get; set; } = new();

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonProperty("blockCode")]
        public string BlockCode { get; set; } = string.Empty;

        [JsonProperty("resultBlockCode")]
        public string ResultBlockCode { get; set; } = string.Empty;

        [JsonProperty("itemCode")]
        public string ItemCode { get; set; } = string.Empty;

        [JsonProperty("entityCode")]
        public string EntityCode { get; set; } = string.Empty;

        [JsonProperty("weatherType")]
        public string WeatherType { get; set; } = string.Empty;

        public RustweaveActiveEffectRecord Clone()
        {
            return new RustweaveActiveEffectRecord
            {
                EffectId = EffectId,
                EffectType = EffectType,
                RecordKind = RecordKind,
                SpellCode = SpellCode,
                CasterPlayerUid = CasterPlayerUid,
                CasterEntityId = CasterEntityId,
                TargetPlayerUid = TargetPlayerUid,
                TargetEntityId = TargetEntityId,
                TargetType = TargetType,
                Mode = Mode,
                CenterX = CenterX,
                CenterY = CenterY,
                CenterZ = CenterZ,
                OriginX = OriginX,
                OriginY = OriginY,
                OriginZ = OriginZ,
                TargetX = TargetX,
                TargetY = TargetY,
                TargetZ = TargetZ,
                Radius = Radius,
                Width = Width,
                Length = Length,
                Amount = Amount,
                SecondaryAmount = SecondaryAmount,
                DurationMilliseconds = DurationMilliseconds,
                StartedAtMilliseconds = StartedAtMilliseconds,
                ExpiresAtMilliseconds = ExpiresAtMilliseconds,
                StartedAtTotalDays = StartedAtTotalDays,
                ExpiresAtTotalDays = ExpiresAtTotalDays,
                TickIntervalMilliseconds = TickIntervalMilliseconds,
                NextTickAtMilliseconds = NextTickAtMilliseconds,
                PersistAcrossRestart = PersistAcrossRestart,
                IsArea = IsArea,
                IsHostile = IsHostile,
                IsBeneficial = IsBeneficial,
                IsBlocking = IsBlocking,
                BlockSnapshots = BlockSnapshots?.Select(snapshot => new RustweaveBlockSnapshot
                {
                    X = snapshot.X,
                    Y = snapshot.Y,
                    Z = snapshot.Z,
                    Dimension = snapshot.Dimension,
                    BlockCode = snapshot.BlockCode,
                    BlockId = snapshot.BlockId
                }).ToList() ?? new List<RustweaveBlockSnapshot>(),
                AffectedEntityIds = AffectedEntityIds?.ToList() ?? new List<long>(),
                StatusCode = StatusCode,
                BlockCode = BlockCode,
                ResultBlockCode = ResultBlockCode,
                ItemCode = ItemCode,
                EntityCode = EntityCode,
                WeatherType = WeatherType
            };
        }
    }

    internal sealed class RustweaveEntityHistorySample
    {
        [JsonProperty("entityId")]
        public long EntityId { get; set; }

        [JsonProperty("playerUid")]
        public string PlayerUid { get; set; } = string.Empty;

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

        [JsonProperty("yaw")]
        public float Yaw { get; set; }

        [JsonProperty("pitch")]
        public float Pitch { get; set; }

        [JsonProperty("sampledAtMilliseconds")]
        public long SampledAtMilliseconds { get; set; }
    }

    internal sealed class RustweaveActiveEffectRegistryConfig
    {
        [JsonProperty("activeEffects")]
        public List<RustweaveActiveEffectRecord> ActiveEffects { get; set; } = new();
    }

    internal sealed class RustweavePlayerStateData
    {
        public int SpellProgressionVersion { get; set; }

        public int CurrentTemporalCorruption { get; set; } = RustweaveConstants.DefaultCorruption;

        public int EffectiveTemporalCorruptionThreshold { get; set; } = RustweaveConstants.DefaultThreshold;

        public int AbsoluteTemporalCorruptionCap { get; set; } = RustweaveConstants.DefaultCap;

        public float CastSpeedMultiplier { get; set; } = 1f;

        public List<string> LearnedSpellCodes { get; set; } = new();

        public Dictionary<string, int> SpellCastCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public int KnowledgeRankIndex { get; set; }

        public List<string> PreparedSpellCodes { get; set; } = new();

        public int SelectedPreparedSpellIndex { get; set; } = -1;

        public long TemporalOverloadStartedAtMilliseconds { get; set; }

        public Dictionary<string, int> DiscoveryResearchProgress { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, int> DiscoveryItemUseCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public List<RustweaveMentorStudyData> MentorStudies { get; set; } = new();

        [JsonIgnore]
        public string SelectedPreparedSpellCode => RustweaveStateService.GetPreparedSpellCode(this, SelectedPreparedSpellIndex);

        public RustweavePlayerStateData Clone()
        {
            return new RustweavePlayerStateData
            {
                CurrentTemporalCorruption = CurrentTemporalCorruption,
                EffectiveTemporalCorruptionThreshold = EffectiveTemporalCorruptionThreshold,
                AbsoluteTemporalCorruptionCap = AbsoluteTemporalCorruptionCap,
                CastSpeedMultiplier = CastSpeedMultiplier,
                SpellProgressionVersion = SpellProgressionVersion,
                LearnedSpellCodes = LearnedSpellCodes.ToList(),
                SpellCastCounts = SpellCastCounts?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                KnowledgeRankIndex = KnowledgeRankIndex,
                PreparedSpellCodes = PreparedSpellCodes.ToList(),
                SelectedPreparedSpellIndex = SelectedPreparedSpellIndex,
                TemporalOverloadStartedAtMilliseconds = TemporalOverloadStartedAtMilliseconds,
                DiscoveryResearchProgress = DiscoveryResearchProgress?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                DiscoveryItemUseCounts = DiscoveryItemUseCounts?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                MentorStudies = MentorStudies?.Select(study => study?.Clone()).Where(study => study != null).Cast<RustweaveMentorStudyData>().ToList() ?? new List<RustweaveMentorStudyData>()
            };
        }
    }

    internal sealed class RustweaveCastStateData
    {
        public bool IsCasting { get; set; }

        public string SpellCode { get; set; } = string.Empty;

        public int PreparedSpellIndex { get; set; } = -1;

        public long StartedAtMilliseconds { get; set; }

        public long DurationMilliseconds { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public int CorruptionCost { get; set; }

        public double BaseCastTimeSeconds { get; set; }

        public int StartingTemporalCorruption { get; set; }

        public bool HasLockedTarget { get; set; }

        public string LockedTargetType { get; set; } = string.Empty;

        public long LockedEntityId { get; set; } = -1;

        public int LockedBlockX { get; set; } = -1;

        public int LockedBlockY { get; set; } = -1;

        public int LockedBlockZ { get; set; } = -1;

        public double LockedPosX { get; set; }

        public double LockedPosY { get; set; }

        public double LockedPosZ { get; set; }

        public string LockedTargetName { get; set; } = string.Empty;

        public string ActiveCastId { get; set; } = string.Empty;

        [JsonIgnore]
        public double Progress => DurationMilliseconds <= 0 ? 0 : Math.Min(1d, (double)ElapsedMilliseconds / DurationMilliseconds);

        [JsonIgnore]
        public double RemainingSeconds => Math.Max(0d, (DurationMilliseconds - ElapsedMilliseconds) / 1000d);

        public RustweaveCastStateData Clone()
        {
            return new RustweaveCastStateData
            {
                IsCasting = IsCasting,
                SpellCode = SpellCode,
                PreparedSpellIndex = PreparedSpellIndex,
                StartedAtMilliseconds = StartedAtMilliseconds,
                DurationMilliseconds = DurationMilliseconds,
                ElapsedMilliseconds = ElapsedMilliseconds,
                CorruptionCost = CorruptionCost,
                BaseCastTimeSeconds = BaseCastTimeSeconds,
                StartingTemporalCorruption = StartingTemporalCorruption
                ,
                HasLockedTarget = HasLockedTarget,
                LockedTargetType = LockedTargetType,
                LockedEntityId = LockedEntityId,
                LockedBlockX = LockedBlockX,
                LockedBlockY = LockedBlockY,
                LockedBlockZ = LockedBlockZ,
                LockedPosX = LockedPosX,
                LockedPosY = LockedPosY,
                LockedPosZ = LockedPosZ,
                LockedTargetName = LockedTargetName,
                ActiveCastId = ActiveCastId
            };
        }
    }

    internal static class RustweaveRuntime
    {
        public static ICoreAPI? CommonApi { get; private set; }

        public static ICoreServerAPI? ServerApi { get; private set; }

        public static ICoreClientAPI? ClientApi { get; private set; }

        public static RustweaveProgressionConfig ProgressionConfig { get; private set; } = new();

        public static RustweaveMentorConfig MentorConfig { get; private set; } = new();

        public static RustweaveDiscoveryConfig DiscoveryConfig { get; private set; } = new();

        public static SpellRegistryType SpellRegistry { get; private set; } = SpellRegistryType.CreateFallback();

        public static RustweaveServerController? Server { get; private set; }

        public static RustweaveClientController? Client { get; private set; }

        public static void LoadSpellRegistry(ICoreAPI api)
        {
            CommonApi = api;
            LoadProgressionConfig(api);
            LoadMentorConfig(api);
            SpellRegistry = SpellRegistryType.Load(api);
            LoadDiscoveryConfig(api);

            if (RustweaveConstants.AllSpellsLearnedByDefaultForTesting)
            {
                var enabledSpells = SpellRegistry.GetEnabledSpells();
                if (enabledSpells.Count == 0)
                {
                    api.Logger.Warning("TheRustweave spell registry loaded zero enabled spells, so the Tome has nothing to expose for testing.");
                }
                else
                {
                    api.Logger.Notification("TheRustweave is exposing {0} spell(s) to the Tome as learned-by-default for testing: {1}", enabledSpells.Count, string.Join(", ", enabledSpells.Select(spell => spell.Code)));
                }
            }
        }

        private static void LoadProgressionConfig(ICoreAPI api)
        {
            try
            {
                ProgressionConfig = api.LoadModConfig<RustweaveProgressionConfig>(RustweaveConstants.ProgressionConfigFileName) ?? new RustweaveProgressionConfig();
            }
            catch (Exception exception)
            {
                ProgressionConfig = new RustweaveProgressionConfig();
                api.Logger.Warning("[TheRustweave] Failed to load progression config, using defaults: {0}", exception.Message);
            }

            try
            {
                api.StoreModConfig(ProgressionConfig, RustweaveConstants.ProgressionConfigFileName);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("[TheRustweave] Failed to store progression config defaults: {0}", exception.Message);
            }

            api.Logger.Notification("[TheRustweave] Progression config loaded: AllSpellsLearnedByDefaultForTesting={0}", ProgressionConfig.AllSpellsLearnedByDefaultForTesting);
        }

        private static void LoadMentorConfig(ICoreAPI api)
        {
            try
            {
                MentorConfig = api.LoadModConfig<RustweaveMentorConfig>(RustweaveConstants.MentorConfigFileName) ?? new RustweaveMentorConfig();
            }
            catch (Exception exception)
            {
                MentorConfig = new RustweaveMentorConfig();
                api.Logger.Warning("[TheRustweave] Failed to load mentor config, using defaults: {0}", exception.Message);
            }

            MentorConfig.DefaultStudyHoursByTier ??= new Dictionary<int, double>();
            MentorConfig.CostsByTier ??= new Dictionary<int, RustweaveMentorTierCostConfig>();

            foreach (var tier in Enumerable.Range(1, 6))
            {
                if (!MentorConfig.DefaultStudyHoursByTier.ContainsKey(tier))
                {
                    MentorConfig.DefaultStudyHoursByTier[tier] = tier switch
                    {
                        1 => 6d,
                        2 => 12d,
                        3 => 24d,
                        4 => 48d,
                        5 => 72d,
                        6 => 120d,
                        _ => 24d
                    };
                }

                if (!MentorConfig.CostsByTier.ContainsKey(tier))
                {
                    MentorConfig.CostsByTier[tier] = new RustweaveMentorTierCostConfig
                    {
                        ItemCode = "game:gear-temporal",
                        Quantity = Math.Max(1, tier)
                    };
                }
            }

            try
            {
                api.StoreModConfig(MentorConfig, RustweaveConstants.MentorConfigFileName);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("[TheRustweave] Failed to store mentor config defaults: {0}", exception.Message);
            }

            api.Logger.Notification(
                "[TheRustweave] Mentor config loaded: enableRustplaneSageMentors={0}, allowMentorsToBypassLootRarity={1}",
                MentorConfig.EnableRustplaneSageMentors,
                MentorConfig.AllowMentorsToBypassLootRarity);
        }

        private static void LoadDiscoveryConfig(ICoreAPI api)
        {
            RustweaveDiscoveryConfig? loadedConfig = null;
            try
            {
                loadedConfig = api.LoadModConfig<RustweaveDiscoveryConfig>(RustweaveConstants.DiscoveryConfigFileName);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("[TheRustweave] Failed to load discovery config, using defaults: {0}", exception.Message);
            }

            if (loadedConfig == null || loadedConfig.DiscoveryItems == null || loadedConfig.DiscoveryItems.Count == 0)
            {
                try
                {
                    var asset = api.Assets.TryGet(new AssetLocation(RustweaveConstants.DiscoveryConfigAsset));
                    loadedConfig = asset?.ToObject<RustweaveDiscoveryConfig>();
                    if (loadedConfig != null)
                    {
                        api.Logger.Notification("[TheRustweave] Discovery config loaded from asset defaults.");
                    }
                }
                catch (Exception exception)
                {
                    api.Logger.Warning("[TheRustweave] Failed to load asset discovery config, using defaults: {0}", exception.Message);
                }
            }

            DiscoveryConfig = loadedConfig ?? new RustweaveDiscoveryConfig();
            DiscoveryConfig.DiscoveryItems ??= new Dictionary<string, RustweaveDiscoveryItemDefinition>(StringComparer.OrdinalIgnoreCase);
            EnsureDefaultDiscoveryConfig();

            try
            {
                api.StoreModConfig(DiscoveryConfig, RustweaveConstants.DiscoveryConfigFileName);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("[TheRustweave] Failed to store discovery config defaults: {0}", exception.Message);
            }

            api.Logger.Notification("[TheRustweave] Discovery config loaded with {0} item definition(s).", DiscoveryConfig.DiscoveryItems.Count);
        }

        public static bool TryGetDiscoveryItemDefinition(string itemCode, out RustweaveDiscoveryItemDefinition? definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return false;
            }

            if (DiscoveryConfig?.DiscoveryItems == null)
            {
                return false;
            }

            return DiscoveryConfig.DiscoveryItems.TryGetValue(itemCode, out definition);
        }

        private static void EnsureDefaultDiscoveryConfig()
        {
            var defaults = CreateDefaultDiscoveryConfig();
            foreach (var pair in defaults.DiscoveryItems)
            {
                if (!DiscoveryConfig.DiscoveryItems.ContainsKey(pair.Key))
                {
                    DiscoveryConfig.DiscoveryItems[pair.Key] = pair.Value;
                }
            }
        }

        private static RustweaveDiscoveryConfig CreateDefaultDiscoveryConfig()
        {
            var allTiers = new[] { 1, 2, 3, 4, 5, 6 }.ToList();

            return new RustweaveDiscoveryConfig
            {
                DiscoveryItems = new Dictionary<string, RustweaveDiscoveryItemDefinition>(StringComparer.OrdinalIgnoreCase)
                {
                    [RustweaveConstants.ForgottenBookItemCode] = new RustweaveDiscoveryItemDefinition
                    {
                        AllowedSchools = new List<string> { SpellSchoolTypes.Tabby, SpellSchoolTypes.Warping, SpellSchoolTypes.Wefting },
                        AllowedTiers = new List<int> { 1, 2 },
                        ConsumeOnSuccess = false,
                        SingleUsePerPlayer = true
                    },
                    [RustweaveConstants.ArcaneNotesItemCode] = new RustweaveDiscoveryItemDefinition
                    {
                        AllowedSchools = new List<string> { SpellSchoolTypes.Darning, SpellSchoolTypes.Backstitching, SpellSchoolTypes.Hemming },
                        AllowedTiers = new List<int> { 1, 2, 3 },
                        ConsumeOnSuccess = true,
                        SingleUsePerPlayer = false
                    },
                    [RustweaveConstants.RustplanePrismItemCode] = new RustweaveDiscoveryItemDefinition
                    {
                        AllowedSchools = new List<string> { SpellSchoolTypes.Tensioning, SpellSchoolTypes.Spinning, SpellSchoolTypes.Grafting },
                        AllowedTiers = new List<int> { 2, 3, 4 },
                        ConsumeOnSuccess = false,
                        SingleUsePerPlayer = true
                    },
                    [RustweaveConstants.AncientCodexItemCode] = new RustweaveDiscoveryItemDefinition
                    {
                        AllowedSchools = new List<string> { SpellSchoolTypes.Scutching, SpellSchoolTypes.Fulling, SpellSchoolTypes.Scouring, SpellSchoolTypes.Carding },
                        AllowedTiers = new List<int> { 3, 4, 5 },
                        ConsumeOnSuccess = true,
                        SingleUsePerPlayer = false
                    },
                    [RustweaveConstants.ScrollFromTheRustItemCode] = new RustweaveDiscoveryItemDefinition
                    {
                        AllowedSchools = new List<string>(),
                        AllowedTiers = allTiers,
                        ConsumeOnSuccess = true,
                        SingleUsePerPlayer = false
                    }
                }
            };
        }

        public static void InitializeServer(ICoreServerAPI api)
        {
            ServerApi = api;
            Server = new RustweaveServerController(api);
            Server.Start();
        }

        public static void InitializeClient(ICoreClientAPI api)
        {
            ClientApi = api;
            Client = new RustweaveClientController(api);
            Client.Start();
        }
    }

    internal static class RustweaveStateService
    {
        private const int CurrentProgressionVersion = 1;

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly Dictionary<string, RustweavePlayerStateData> CachedServerStates = new(StringComparer.OrdinalIgnoreCase);

        public static string GetClassCode(IPlayer? player)
        {
            if (player?.Entity?.WatchedAttributes == null)
            {
                return string.Empty;
            }

            return player.Entity.WatchedAttributes.GetString("characterClass", string.Empty)
                ?? player.Entity.WatchedAttributes.GetString("characterclass", string.Empty)
                ?? player.Entity.WatchedAttributes.GetString("class", string.Empty)
                ?? string.Empty;
        }

        public static bool IsRustweaver(IPlayer? player)
        {
            return string.Equals(GetClassCode(player), "rustweaver", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRustweaver(EntityPlayer? entityPlayer)
        {
            if (entityPlayer?.WatchedAttributes == null)
            {
                return false;
            }

            var classCode =
                entityPlayer.WatchedAttributes.GetString("characterClass", string.Empty)
                ?? entityPlayer.WatchedAttributes.GetString("characterclass", string.Empty)
                ?? entityPlayer.WatchedAttributes.GetString("class", string.Empty)
                ?? string.Empty;

            return string.Equals(classCode, "rustweaver", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHoldingTome(IPlayer? player)
        {
            var collectible = player?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible;
            if (collectible == null)
            {
                return false;
            }

            return collectible is ItemRustweaverTome || string.Equals(collectible.Code?.Path, RustweaveConstants.TomeItemCode, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHoldingTablet(IPlayer? player)
        {
            var collectible = player?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible;
            if (collectible == null)
            {
                return false;
            }

            return collectible is ItemRustTablet || string.Equals(collectible.Code?.Path, RustweaveConstants.TabletItemCode, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTabletStack(ItemStack? stack)
        {
            var collectible = stack?.Collectible;
            if (collectible == null)
            {
                return false;
            }

            return collectible is ItemRustTablet || string.Equals(collectible.Code?.Path, RustweaveConstants.TabletItemCode, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLoreweaveSpell(SpellDefinition? spell)
        {
            return spell != null && (
                spell.IsLoreSpell
                || string.Equals(spell.Category, "Lore", StringComparison.OrdinalIgnoreCase)
                || string.Equals(spell.School, SpellSchoolTypes.Loreweave, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsHiddenSpell(SpellDefinition? spell)
        {
            return spell?.Hidden == true;
        }

        public static bool ShouldIncludeHiddenSpellsInTome()
        {
            return RustweaveConstants.AllSpellsLearnedByDefaultForTesting;
        }

        public static bool IsStarterSpell(string? spellCode)
        {
            return !string.IsNullOrWhiteSpace(spellCode)
                && RustweaveConstants.StarterSpellCodes.Any(code => string.Equals(code, spellCode, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsIntrinsicLearnedSpell(SpellDefinition? spell)
        {
            return IsLoreweaveSpell(spell) || IsStarterSpell(spell?.Code);
        }

        public static int GetTabletStoredCorruption(ItemStack? stack)
        {
            if (!IsTabletStack(stack) || stack?.Attributes == null)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(RustweaveConstants.TabletCapacity, stack.Attributes.GetInt(RustweaveConstants.TabletStoredCorruptionKey, 0)));
        }

        public static void SetTabletStoredCorruption(ItemStack? stack, int storedCorruption)
        {
            SetTabletStoredCorruption(stack, storedCorruption, null);
        }

        public static void SetTabletStoredCorruption(ItemStack? stack, int storedCorruption, IWorldAccessor? world)
        {
            if (!IsTabletStack(stack) || stack?.Attributes == null)
            {
                return;
            }

            var clamped = Math.Max(0, Math.Min(RustweaveConstants.TabletCapacity, storedCorruption));
            stack.Attributes.SetInt(RustweaveConstants.TabletStoredCorruptionKey, clamped);
            if (clamped > 0 && world?.Calendar != null)
            {
                TouchTabletDecayTimestamp(stack, world);
            }
        }

        public static double GetTabletLastDecayTotalDays(ItemStack? stack)
        {
            if (!IsTabletStack(stack) || stack?.Attributes == null)
            {
                return 0d;
            }

            return Math.Max(0d, stack.Attributes.GetDouble(RustweaveConstants.TabletLastDecayTotalDaysKey, 0d));
        }

        public static void TouchTabletDecayTimestamp(ItemStack? stack, IWorldAccessor? world)
        {
            if (!IsTabletStack(stack) || stack?.Attributes == null || world?.Calendar == null)
            {
                return;
            }

            stack.Attributes.SetDouble(RustweaveConstants.TabletLastDecayTotalDaysKey, Math.Max(0d, world.Calendar.TotalDays));
        }

        public static int GetTabletDisplayedCorruption(IWorldAccessor? world, ItemStack? stack)
        {
            if (!IsTabletStack(stack))
            {
                return 0;
            }

            var storedCorruption = GetTabletStoredCorruption(stack);
            if (storedCorruption <= 0 || world?.Calendar == null)
            {
                return storedCorruption;
            }

            var lastDecayTotalDays = GetTabletLastDecayTotalDays(stack);
            if (lastDecayTotalDays <= 0d)
            {
                return storedCorruption;
            }

            var elapsedDays = world.Calendar.TotalDays - lastDecayTotalDays;
            if (elapsedDays <= 0d)
            {
                return storedCorruption;
            }

            var decayAmount = (int)Math.Floor(elapsedDays * RustweaveConstants.TabletDecayPerDay);
            return Math.Max(0, storedCorruption - decayAmount);
        }

        public static string GetTabletStageLabel(IWorldAccessor? world, ItemStack? stack)
        {
            return GetTabletStageLabel(GetTabletDisplayedCorruption(world, stack));
        }

        public static string GetTabletStageLabel(int storedCorruption)
        {
            if (storedCorruption <= 0)
            {
                return Lang.Get("game:rusttablet-stage-inert");
            }

            if (storedCorruption < 100)
            {
                return Lang.Get("game:rusttablet-stage-faded");
            }

            return Lang.Get("game:rusttablet-stage-stabilized");
        }

        public static bool ApplyTabletPassiveDecay(IWorldAccessor? world, ItemSlot? slot)
        {
            if (slot?.Itemstack == null)
            {
                return false;
            }

            var changed = ApplyTabletPassiveDecay(world, slot.Itemstack);
            if (changed)
            {
                slot.MarkDirty();
            }

            return changed;
        }

        public static bool ApplyTabletPassiveDecay(IWorldAccessor? world, ItemStack? stack)
        {
            if (!IsTabletStack(stack) || stack?.Attributes == null || world?.Calendar == null)
            {
                return false;
            }

            var currentDays = Math.Max(0d, world.Calendar.TotalDays);
            var storedCorruption = GetTabletStoredCorruption(stack);
            var lastDecayTotalDays = GetTabletLastDecayTotalDays(stack);

            if (storedCorruption <= 0)
            {
                if (lastDecayTotalDays <= 0d)
                {
                    stack.Attributes.SetDouble(RustweaveConstants.TabletLastDecayTotalDaysKey, currentDays);
                    return true;
                }

                return false;
            }

            if (lastDecayTotalDays <= 0d || currentDays <= lastDecayTotalDays)
            {
                stack.Attributes.SetDouble(RustweaveConstants.TabletLastDecayTotalDaysKey, currentDays);
                return true;
            }

            var elapsedDays = currentDays - lastDecayTotalDays;
            var decayAmount = (int)Math.Floor(elapsedDays * RustweaveConstants.TabletDecayPerDay);
            if (decayAmount <= 0)
            {
                return false;
            }

            var newStoredCorruption = Math.Max(0, storedCorruption - decayAmount);
            if (newStoredCorruption == storedCorruption)
            {
                return false;
            }

            stack.Attributes.SetInt(RustweaveConstants.TabletStoredCorruptionKey, newStoredCorruption);
            stack.Attributes.SetDouble(
                RustweaveConstants.TabletLastDecayTotalDaysKey,
                newStoredCorruption <= 0
                    ? currentDays
                    : lastDecayTotalDays + (decayAmount / (double)RustweaveConstants.TabletDecayPerDay));
            return true;
        }

        public static void UpdateOverloadState(RustweavePlayerStateData state, long nowMilliseconds)
        {
            if (state.CurrentTemporalCorruption >= state.EffectiveTemporalCorruptionThreshold)
            {
                if (state.TemporalOverloadStartedAtMilliseconds <= 0)
                {
                    state.TemporalOverloadStartedAtMilliseconds = nowMilliseconds;
                }

                return;
            }

            state.TemporalOverloadStartedAtMilliseconds = 0;
        }

        public static bool IsOverloadExpired(RustweavePlayerStateData state, long nowMilliseconds)
        {
            return state.TemporalOverloadStartedAtMilliseconds > 0
                && nowMilliseconds - state.TemporalOverloadStartedAtMilliseconds >= RustweaveConstants.OverloadDurationMilliseconds;
        }

        public static RustweavePlayerStateData CreateFreshState()
        {
            var state = new RustweavePlayerStateData
            {
                CurrentTemporalCorruption = RustweaveConstants.DefaultCorruption,
                EffectiveTemporalCorruptionThreshold = RustweaveConstants.DefaultThreshold,
                AbsoluteTemporalCorruptionCap = RustweaveConstants.DefaultCap,
                CastSpeedMultiplier = 1f,
                SpellProgressionVersion = CurrentProgressionVersion,
                LearnedSpellCodes = new List<string>(),
                SpellCastCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                KnowledgeRankIndex = 0,
                PreparedSpellCodes = new List<string>(RustweaveConstants.PreparedSlotCount),
                SelectedPreparedSpellIndex = -1
            };

            while (state.PreparedSpellCodes.Count < RustweaveConstants.PreparedSlotCount)
            {
                state.PreparedSpellCodes.Add(string.Empty);
            }

            return state;
        }

        public static RustweavePlayerStateData CreateDefaultState()
        {
            return CreateFreshState();
        }

        public static RustweavePlayerStateData NormalizeState(RustweavePlayerStateData? state)
        {
            state ??= CreateFreshState();

            state.CurrentTemporalCorruption = Math.Max(RustweaveConstants.DefaultCorruption, state.CurrentTemporalCorruption);
            state.EffectiveTemporalCorruptionThreshold = Math.Max(1, state.EffectiveTemporalCorruptionThreshold);
            state.AbsoluteTemporalCorruptionCap = Math.Max(state.EffectiveTemporalCorruptionThreshold, state.AbsoluteTemporalCorruptionCap);
            state.CurrentTemporalCorruption = Math.Min(state.CurrentTemporalCorruption, state.AbsoluteTemporalCorruptionCap);
            state.CastSpeedMultiplier = state.CastSpeedMultiplier <= 0 ? 1f : state.CastSpeedMultiplier;
            if (state.CurrentTemporalCorruption < state.EffectiveTemporalCorruptionThreshold)
            {
                state.TemporalOverloadStartedAtMilliseconds = 0;
            }

            state.LearnedSpellCodes ??= new List<string>();
            state.SpellCastCounts ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.PreparedSpellCodes ??= new List<string>();
            state.DiscoveryResearchProgress ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.DiscoveryItemUseCounts ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.MentorStudies ??= new List<RustweaveMentorStudyData>();

            if (state.SpellProgressionVersion < CurrentProgressionVersion)
            {
                state.LearnedSpellCodes.Clear();
                state.SpellCastCounts.Clear();
                state.SpellProgressionVersion = CurrentProgressionVersion;
            }

            CleanInvalidLearnedSpells(state);
            state.KnowledgeRankIndex = GetKnowledgeRankIndex(state);

            var normalizedPrepared = new List<string>(RustweaveConstants.PreparedSlotCount);
            var usedSpellCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < RustweaveConstants.PreparedSlotCount; index++)
            {
                var code = index < state.PreparedSpellCodes.Count ? state.PreparedSpellCodes[index] ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    normalizedPrepared.Add(string.Empty);
                    continue;
                }

                if (!IsSpellLearned(code, state) || !usedSpellCodes.Add(code))
                {
                    normalizedPrepared.Add(string.Empty);
                    continue;
                }

                if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(code, out _))
                {
                    normalizedPrepared.Add(string.Empty);
                    continue;
                }

                normalizedPrepared.Add(code);
            }

            while (normalizedPrepared.Count < RustweaveConstants.PreparedSlotCount)
            {
                normalizedPrepared.Add(string.Empty);
            }

            state.PreparedSpellCodes = normalizedPrepared;
            state.SelectedPreparedSpellIndex = IsValidSlotIndex(state.SelectedPreparedSpellIndex)
                ? state.SelectedPreparedSpellIndex
                : -1;

            state.DiscoveryResearchProgress = state.DiscoveryResearchProgress
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
                .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Max(pair => pair.Value), StringComparer.OrdinalIgnoreCase);

            state.DiscoveryItemUseCounts = state.DiscoveryItemUseCounts
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
                .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Max(pair => pair.Value), StringComparer.OrdinalIgnoreCase);

            RustweaveMentorService.NormalizeMentorStudies(state);

            return state;
        }

        public static bool IsSpellLearned(string spellCode, RustweavePlayerStateData state)
        {
            if (string.IsNullOrWhiteSpace(spellCode) || state == null)
            {
                return false;
            }

            if (RustweaveConstants.AllSpellsLearnedByDefaultForTesting)
            {
                return RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out _);
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) || spell == null)
            {
                return false;
            }

            if (IsIntrinsicLearnedSpell(spell))
            {
                return true;
            }

            return state.LearnedSpellCodes.Any(code => string.Equals(code, spellCode, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsSpellLearned(IPlayer? player, string spellCode)
        {
            if (player is IServerPlayer serverPlayer)
            {
                return IsSpellLearned(spellCode, LoadServerState(serverPlayer));
            }

            if (player is IClientPlayer clientPlayer && TryGetClientState(clientPlayer, out var state))
            {
                return IsSpellLearned(spellCode, state);
            }

            return false;
        }

        public static int NormalizeSelectedIndex(int selectedIndex, IReadOnlyList<string> preparedSpellCodes)
        {
            if (selectedIndex >= 0 && selectedIndex < preparedSpellCodes.Count)
            {
                return selectedIndex;
            }

            var firstPrepared = GetFirstPreparedSlotIndex(preparedSpellCodes);
            return firstPrepared >= 0 ? firstPrepared : -1;
        }

        public static int GetFirstPreparedSlotIndex(IReadOnlyList<string> preparedSpellCodes)
        {
            for (var index = 0; index < preparedSpellCodes.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(preparedSpellCodes[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public static string GetPreparedSpellCode(RustweavePlayerStateData state, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= state.PreparedSpellCodes.Count)
            {
                return string.Empty;
            }

            return state.PreparedSpellCodes[slotIndex] ?? string.Empty;
        }

        public static string GetPreparedSlotDisplayText(RustweavePlayerStateData state, int slotIndex)
        {
            var spellCode = GetPreparedSpellCode(state, slotIndex);
            return string.IsNullOrWhiteSpace(spellCode) ? "Empty" : GetSpellDisplayName(spellCode);
        }

        public static string GetSelectedPreparedSpellCode(RustweavePlayerStateData state)
        {
            return GetPreparedSpellCode(state, state.SelectedPreparedSpellIndex);
        }

        public static int FindPreparedSpellSlot(RustweavePlayerStateData state, string spellCode)
        {
            for (var index = 0; index < state.PreparedSpellCodes.Count; index++)
            {
                if (string.Equals(state.PreparedSpellCodes[index], spellCode, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        public static int CycleSelection(RustweavePlayerStateData state, int delta)
        {
            var occupiedSlots = Enumerable.Range(0, state.PreparedSpellCodes.Count)
                .Where(index => !string.IsNullOrWhiteSpace(state.PreparedSpellCodes[index]))
                .ToList();

            if (occupiedSlots.Count == 0)
            {
                return -1;
            }

            var currentIndex = NormalizeSelectedIndex(state.SelectedPreparedSpellIndex, state.PreparedSpellCodes);
            if (currentIndex < 0)
            {
                return occupiedSlots[0];
            }

            var currentPosition = occupiedSlots.IndexOf(currentIndex);
            if (currentPosition < 0)
            {
                return occupiedSlots[0];
            }

            var nextPosition = (currentPosition + delta) % occupiedSlots.Count;
            if (nextPosition < 0)
            {
                nextPosition += occupiedSlots.Count;
            }

            return occupiedSlots[nextPosition];
        }

        public static int ResolvePrepareTargetSlot(RustweavePlayerStateData state, int targetSlotIndex)
        {
            if (IsValidSlotIndex(targetSlotIndex))
            {
                return targetSlotIndex;
            }

            if (IsValidSlotIndex(state.SelectedPreparedSpellIndex))
            {
                return state.SelectedPreparedSpellIndex;
            }

            return GetFirstEmptyPreparedSlotIndex(state.PreparedSpellCodes);
        }

        public static bool TryPrepareSpell(RustweavePlayerStateData state, string spellCode, int targetSlotIndex)
        {
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return false;
            }

            var existingSlot = FindPreparedSpellSlot(state, spellCode);
            var slotIndex = ResolvePrepareTargetSlot(state, targetSlotIndex);

            if (slotIndex < 0)
            {
                return false;
            }

            if (existingSlot >= 0 && existingSlot != slotIndex)
            {
                return false;
            }

            if (existingSlot == slotIndex && string.Equals(state.PreparedSpellCodes[slotIndex], spellCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            state.PreparedSpellCodes[slotIndex] = spellCode;
            return true;
        }

        public static bool TryUnprepareSpell(RustweavePlayerStateData state, int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(state.PreparedSpellCodes[slotIndex]))
            {
                return false;
            }

            state.PreparedSpellCodes[slotIndex] = string.Empty;

            return true;
        }

        public static bool TrySelectPreparedSpell(RustweavePlayerStateData state, int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            state.SelectedPreparedSpellIndex = slotIndex;
            return true;
        }

        public static bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < RustweaveConstants.PreparedSlotCount;
        }

        public static int ToDisplaySlotNumber(int slotIndex)
        {
            return slotIndex + 1;
        }

        public static int ToInternalSlotIndex(int displaySlotNumber)
        {
            return displaySlotNumber - 1;
        }

        public static string DescribePreparedSlot(int slotIndex)
        {
            return IsValidSlotIndex(slotIndex) ? ToDisplaySlotNumber(slotIndex).ToString(CultureInfo.InvariantCulture) : "none";
        }

        public static int GetFirstEmptyPreparedSlotIndex(IReadOnlyList<string> preparedSpellCodes)
        {
            for (var index = 0; index < preparedSpellCodes.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(preparedSpellCodes[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public static IReadOnlyList<string> GetLearnedSpellCodes(RustweavePlayerStateData state)
        {
            return state.LearnedSpellCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetLearnedSpells(IPlayer? player)
        {
            var includeHidden = ShouldIncludeHiddenSpellsInTome();
            var spells = RustweaveRuntime.SpellRegistry.GetEnabledSpells();
            if (player == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            return spells
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && (includeHidden || !IsHiddenSpell(spell)) && !IsLoreweaveSpell(spell) && IsSpellLearned(player, spell.Code))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetLockedSpells(IPlayer? player)
        {
            if (player == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            var includeHidden = ShouldIncludeHiddenSpellsInTome();
            return RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && (includeHidden || !IsHiddenSpell(spell)) && !IsLoreweaveSpell(spell) && !IsSpellLearned(player, spell.Code))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetLoreweaveSpells(IPlayer? player)
        {
            if (player == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            var includeHidden = ShouldIncludeHiddenSpellsInTome();
            return RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && (includeHidden || !IsHiddenSpell(spell)) && IsLoreweaveSpell(spell))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetAllVisibleSpells(IPlayer? player)
        {
            if (player == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            var includeHidden = ShouldIncludeHiddenSpellsInTome();
            return RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && (includeHidden || !IsHiddenSpell(spell)))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetDiscoveryPoolCandidates(IPlayer? player, string discoveryItemCode)
        {
            if (player == null || string.IsNullOrWhiteSpace(discoveryItemCode))
            {
                return Array.Empty<SpellDefinition>();
            }

            if (!RustweaveRuntime.TryGetDiscoveryItemDefinition(discoveryItemCode, out var definition) || definition == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            var includeHidden = ShouldIncludeHiddenSpellsInTome();
            return RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => IsEligibleDiscoverySpell(spell, definition, includeHidden))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<SpellDefinition> GetDiscoveryEligibleSpells(IPlayer? player, string discoveryItemCode)
        {
            if (player == null || string.IsNullOrWhiteSpace(discoveryItemCode))
            {
                return Array.Empty<SpellDefinition>();
            }

            if (!RustweaveRuntime.TryGetDiscoveryItemDefinition(discoveryItemCode, out var definition) || definition == null)
            {
                return Array.Empty<SpellDefinition>();
            }

            return GetDiscoveryPoolCandidates(player, discoveryItemCode)
                .Where(spell => !IsSpellLearned(player, spell.Code))
                .ToList();
        }

        public static bool IsEligibleDiscoverySpell(SpellDefinition? spell, RustweaveDiscoveryItemDefinition? definition, bool includeHidden)
        {
            if (spell == null || definition == null || string.IsNullOrWhiteSpace(spell.Code) || !spell.Enabled)
            {
                return false;
            }

            if (!includeHidden && IsHiddenSpell(spell))
            {
                return false;
            }

            if (IsLoreweaveSpell(spell))
            {
                return false;
            }

            if (definition.AllowedSchools != null && definition.AllowedSchools.Count > 0)
            {
                var school = spell.School ?? string.Empty;
                if (!definition.AllowedSchools.Any(allowed => string.Equals(allowed, school, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            if (definition.AllowedTiers != null && definition.AllowedTiers.Count > 0 && !definition.AllowedTiers.Contains(spell.Tier))
            {
                return false;
            }

            return true;
        }

        public static int GetDiscoveryItemUseCount(RustweavePlayerStateData state, string discoveryItemCode)
        {
            if (state?.DiscoveryItemUseCounts == null || string.IsNullOrWhiteSpace(discoveryItemCode))
            {
                return 0;
            }

            return state.DiscoveryItemUseCounts.TryGetValue(discoveryItemCode, out var count) ? count : 0;
        }

        public static bool HasUsedDiscoveryItem(RustweavePlayerStateData state, string discoveryItemCode)
        {
            return GetDiscoveryItemUseCount(state, discoveryItemCode) > 0;
        }

        public static bool MarkDiscoveryItemUsed(RustweavePlayerStateData state, string discoveryItemCode)
        {
            if (state == null || string.IsNullOrWhiteSpace(discoveryItemCode))
            {
                return false;
            }

            state.DiscoveryItemUseCounts ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.DiscoveryItemUseCounts[discoveryItemCode] = Math.Max(1, GetDiscoveryItemUseCount(state, discoveryItemCode) + 1);
            return true;
        }

        public static int GetDiscoveryResearchProgress(RustweavePlayerStateData? state, string discoveryItemCode)
        {
            if (state?.DiscoveryResearchProgress == null || string.IsNullOrWhiteSpace(discoveryItemCode))
            {
                return 0;
            }

            return state.DiscoveryResearchProgress.TryGetValue(discoveryItemCode, out var progress) ? progress : 0;
        }

        public static int IncrementDiscoveryResearchProgress(RustweavePlayerStateData state, string discoveryItemCode, int delta = 1)
        {
            if (state == null || string.IsNullOrWhiteSpace(discoveryItemCode) || delta <= 0)
            {
                return GetDiscoveryResearchProgress(state, discoveryItemCode);
            }

            state.DiscoveryResearchProgress ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.DiscoveryResearchProgress[discoveryItemCode] = Math.Max(0, GetDiscoveryResearchProgress(state, discoveryItemCode) + delta);
            return state.DiscoveryResearchProgress[discoveryItemCode];
        }

        public static int GetKnowledgeRankIndex(RustweavePlayerStateData state)
        {
            if (state == null)
            {
                return 0;
            }

            var learnedCount = RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && !IsLoreweaveSpell(spell) && IsSpellLearned(spell.Code, state))
                .Count();

            return Math.Min(5, learnedCount / 2);
        }

        public static string GetKnowledgeRankName(RustweavePlayerStateData state)
        {
            return RustweaveKnowledgeRanks.GetName(GetKnowledgeRankIndex(state));
        }

        public static string GetKnowledgeRankName(IPlayer? player)
        {
            return player is IServerPlayer serverPlayer
                ? GetKnowledgeRankName(LoadServerState(serverPlayer))
                : player is IClientPlayer clientPlayer && TryGetClientState(clientPlayer, out var state)
                    ? GetKnowledgeRankName(state)
                    : RustweaveKnowledgeRanks.GetName(0);
        }

        public static int GetKnowledgeRank(IPlayer? player)
        {
            return player is IServerPlayer serverPlayer
                ? GetKnowledgeRankIndex(LoadServerState(serverPlayer))
                : player is IClientPlayer clientPlayer && TryGetClientState(clientPlayer, out var state)
                    ? GetKnowledgeRankIndex(state)
                    : 0;
        }

        public static bool IncrementSpellCastCount(RustweavePlayerStateData state, string spellCode)
        {
            if (state == null || string.IsNullOrWhiteSpace(spellCode))
            {
                return false;
            }

            state.SpellCastCounts ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.SpellCastCounts.TryGetValue(spellCode, out var current);
            state.SpellCastCounts[spellCode] = current + 1;
            return true;
        }

        public static int GetSpellCastCount(RustweavePlayerStateData state, string spellCode)
        {
            if (state?.SpellCastCounts == null || string.IsNullOrWhiteSpace(spellCode))
            {
                return 0;
            }

            return state.SpellCastCounts.TryGetValue(spellCode, out var count) ? count : 0;
        }

        public static bool CleanInvalidLearnedSpells(RustweavePlayerStateData state)
        {
            if (state == null)
            {
                return false;
            }

            var learnedSpellCodes = state.LearnedSpellCodes ?? new List<string>();
            var cleaned = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var changed = false;

            foreach (var spellCode in learnedSpellCodes)
            {
                if (string.IsNullOrWhiteSpace(spellCode))
                {
                    changed = true;
                    continue;
                }

                if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out var spell) || spell == null)
                {
                    changed = true;
                    continue;
                }

                if (IsIntrinsicLearnedSpell(spell))
                {
                    changed = true;
                    continue;
                }

                if (!seen.Add(spell.Code))
                {
                    changed = true;
                    continue;
                }

                cleaned.Add(spell.Code);
            }

            if (changed || cleaned.Count != learnedSpellCodes.Count)
            {
                state.LearnedSpellCodes = cleaned;
                return true;
            }

            state.LearnedSpellCodes = cleaned;
            return false;
        }

        public static bool LearnSpell(RustweavePlayerStateData state, string spellCode, string source, out string failureReason)
        {
            failureReason = string.Empty;
            if (state == null || string.IsNullOrWhiteSpace(spellCode))
            {
                failureReason = "Invalid spell code.";
                return false;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out var spell) || spell == null)
            {
                failureReason = "Spell is missing or disabled.";
                return false;
            }

            if (IsIntrinsicLearnedSpell(spell) || IsSpellLearned(spellCode, state))
            {
                failureReason = "Spell is already learned.";
                return false;
            }

            state.LearnedSpellCodes ??= new List<string>();
            if (!state.LearnedSpellCodes.Any(code => string.Equals(code, spellCode, StringComparison.OrdinalIgnoreCase)))
            {
                state.LearnedSpellCodes.Add(spell.Code);
            }

            state.KnowledgeRankIndex = GetKnowledgeRankIndex(state);
            return true;
        }

        public static bool LearnSpell(IServerPlayer player, string spellCode, string source, out string failureReason)
        {
            var state = LoadServerState(player);
            var changed = LearnSpell(state, spellCode, source, out failureReason);
            if (changed)
            {
                SaveServerState(player, NormalizeState(state));
            }

            return changed;
        }

        public static bool ForgetSpell(RustweavePlayerStateData state, string spellCode, string source, out string failureReason)
        {
            failureReason = string.Empty;
            if (state == null || string.IsNullOrWhiteSpace(spellCode))
            {
                failureReason = "Invalid spell code.";
                return false;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) || spell == null)
            {
                failureReason = "Spell is missing or disabled.";
                return false;
            }

            if (IsIntrinsicLearnedSpell(spell))
            {
                failureReason = "Starter and Loreweave spells cannot be forgotten.";
                return false;
            }

            var removed = false;
            for (var index = state.LearnedSpellCodes.Count - 1; index >= 0; index--)
            {
                if (!string.Equals(state.LearnedSpellCodes[index], spell.Code, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                state.LearnedSpellCodes.RemoveAt(index);
                removed = true;
            }

            if (!removed)
            {
                failureReason = "Spell was not learned.";
                return false;
            }

            state.KnowledgeRankIndex = GetKnowledgeRankIndex(state);
            return true;
        }

        public static bool ForgetSpell(IServerPlayer player, string spellCode, string source, out string failureReason)
        {
            var state = LoadServerState(player);
            var changed = ForgetSpell(state, spellCode, source, out failureReason);
            if (changed)
            {
                NormalizeState(state);
                SaveServerState(player, state);
            }

            return changed;
        }

        public static bool ResetSpellProgression(RustweavePlayerStateData state, out string failureReason)
        {
            failureReason = string.Empty;
            if (state == null)
            {
                failureReason = "Player state was unavailable.";
                return false;
            }

            state.LearnedSpellCodes = new List<string>();
            state.SpellCastCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.DiscoveryResearchProgress = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.DiscoveryItemUseCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            state.MentorStudies = new List<RustweaveMentorStudyData>();
            state.SpellProgressionVersion = CurrentProgressionVersion;
            state.KnowledgeRankIndex = GetKnowledgeRankIndex(state);
            return true;
        }

        public static bool ResetSpellProgression(IServerPlayer player, out string failureReason)
        {
            var state = LoadServerState(player);
            if (!ResetSpellProgression(state, out failureReason))
            {
                return false;
            }

            SaveServerState(player, NormalizeState(state));
            return true;
        }

        public static bool LearnAllEnabledSpells(RustweavePlayerStateData state, out string failureReason)
        {
            failureReason = string.Empty;
            if (state == null)
            {
                failureReason = "Player state was unavailable.";
                return false;
            }

            state.LearnedSpellCodes ??= new List<string>();
            var changed = false;
            foreach (var spell in RustweaveRuntime.SpellRegistry.GetEnabledSpells())
            {
                if (spell == null || string.IsNullOrWhiteSpace(spell.Code) || IsIntrinsicLearnedSpell(spell))
                {
                    continue;
                }

                if (state.LearnedSpellCodes.Any(code => string.Equals(code, spell.Code, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                state.LearnedSpellCodes.Add(spell.Code);
                changed = true;
            }

            state.KnowledgeRankIndex = GetKnowledgeRankIndex(state);
            return changed;
        }

        public static bool LearnAllEnabledSpells(IServerPlayer player, out string failureReason)
        {
            var state = LoadServerState(player);
            var changed = LearnAllEnabledSpells(state, out failureReason);
            if (changed)
            {
                SaveServerState(player, NormalizeState(state));
            }

            return changed;
        }

        public static string SerializePlayerState(RustweavePlayerStateData state)
        {
            return JsonConvert.SerializeObject(state, JsonSettings);
        }

        public static RustweavePlayerStateData DeserializePlayerState(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateFreshState();
            }

            try
            {
                return NormalizeState(JsonConvert.DeserializeObject<RustweavePlayerStateData>(json, JsonSettings));
            }
            catch
            {
                return CreateFreshState();
            }
        }

        public static bool TryDeserializePlayerState(string? json, out RustweavePlayerStateData state)
        {
            state = CreateFreshState();
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                state = NormalizeState(JsonConvert.DeserializeObject<RustweavePlayerStateData>(json, JsonSettings));
                return true;
            }
            catch
            {
                state = CreateFreshState();
                return false;
            }
        }

        public static string SerializeCastState(RustweaveCastStateData state)
        {
            return JsonConvert.SerializeObject(state, JsonSettings);
        }

        public static RustweaveCastStateData DeserializeCastState(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new RustweaveCastStateData();
            }

            try
            {
                return JsonConvert.DeserializeObject<RustweaveCastStateData>(json, JsonSettings) ?? new RustweaveCastStateData();
            }
            catch
            {
                return new RustweaveCastStateData();
            }
        }

        public static void SaveServerState(IServerPlayer player, RustweavePlayerStateData state)
        {
            var bytes = Encoding.UTF8.GetBytes(SerializePlayerState(state));
            player.SetModdata(RustweaveConstants.PlayerStateModDataKey, bytes);
            SyncWatchedPlayerState(player, state);
            player.BroadcastPlayerData(false);
        }

        public static RustweavePlayerStateData LoadServerState(IServerPlayer player)
        {
            if (CachedServerStates.TryGetValue(player.PlayerUID, out var cached))
            {
                var cachedJson = SerializePlayerState(cached);
                var watchedCachedStateJson = player.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedPlayerStateKey, string.Empty) ?? string.Empty;
                if (!string.Equals(watchedCachedStateJson, cachedJson, StringComparison.Ordinal))
                {
                    SyncWatchedPlayerState(player, cached);
                }

                return cached;
            }

            var raw = player.GetModdata(RustweaveConstants.PlayerStateModDataKey);
            var isNewState = raw == null || raw.Length == 0;
            var rawJson = raw == null ? string.Empty : Encoding.UTF8.GetString(raw);
            var state = isNewState
                ? CreateFreshState()
                : DeserializePlayerState(rawJson);

            state = NormalizeState(state);
            CachedServerStates[player.PlayerUID] = state;
            var normalizedJson = SerializePlayerState(state);
            if (isNewState || !string.Equals(rawJson, normalizedJson, StringComparison.Ordinal))
            {
                player.SetModdata(RustweaveConstants.PlayerStateModDataKey, Encoding.UTF8.GetBytes(normalizedJson));
            }

            var watchedStateJson = player.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedPlayerStateKey, string.Empty) ?? string.Empty;
            if (!string.Equals(watchedStateJson, normalizedJson, StringComparison.Ordinal))
            {
                SyncWatchedPlayerState(player, state);
            }

            return state;
        }

        public static void CacheServerState(IServerPlayer player, RustweavePlayerStateData state)
        {
            CachedServerStates[player.PlayerUID] = state;
        }

        public static void SyncWatchedPlayerState(IPlayer player, RustweavePlayerStateData state)
        {
            if (player.Entity?.WatchedAttributes == null)
            {
                return;
            }

            var json = SerializePlayerState(state);
            player.Entity.WatchedAttributes.SetString(RustweaveConstants.WatchedPlayerStateKey, json);
            player.Entity.WatchedAttributes.MarkPathDirty(RustweaveConstants.WatchedPlayerStateKey);
        }

        public static void SyncWatchedCastState(IPlayer player, RustweaveCastStateData state)
        {
            if (player.Entity?.WatchedAttributes == null)
            {
                return;
            }

            var json = SerializeCastState(state);
            player.Entity.WatchedAttributes.SetString(RustweaveConstants.WatchedCastStateKey, json);
            player.Entity.WatchedAttributes.MarkPathDirty(RustweaveConstants.WatchedCastStateKey);
        }

        public static bool TryGetClientState(IClientPlayer player, out RustweavePlayerStateData state)
        {
            var json = player?.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedPlayerStateKey, string.Empty);
            return TryDeserializePlayerState(json, out state);
        }

        public static bool TryGetClientCastState(IClientPlayer player, out RustweaveCastStateData state)
        {
            var json = player?.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedCastStateKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                state = new RustweaveCastStateData();
                return false;
            }

            state = DeserializeCastState(json);
            return true;
        }

        public static string FormatSeconds(double seconds)
        {
            return seconds.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string GetSpellDisplayName(string spellCode)
        {
            return RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell is { Name: { Length: > 0 } name }
                ? name
                : spellCode;
        }

        public static string GetSpellDescription(string spellCode)
        {
            return RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell != null
                ? spell.Description
                : string.Empty;
        }
    }

    internal sealed class RustweaveServerController
    {
        private readonly ICoreServerAPI sapi;
        private readonly SpellEffectExecutor spellExecutor;
        private readonly Dictionary<string, RustweaveCastStateData> activeCasts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustTabletVentingSession> activeTabletVents = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> pendingTabletDecayScans = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> skippedTabletDecayInventoryLogs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveTimedStatModifier> activeTimedStatModifiers = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveTimedDamageOverTime> activeDamageOverTimeEffects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveTimedShield> activeSpellShields = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, long>> spellCooldowns = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveTargetPreviewPacket> activeTargetPreviews = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveActiveCastRecord> activeTargetedCastRecords = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveActiveEffectRecord> activeEffectRecords = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<long, LinkedList<RustweaveEntityHistorySample>> entityHistory = new();
        private readonly Dictionary<long, long> nextHistorySampleMilliseconds = new();
        private IServerNetworkChannel? channel;
        private long tickListenerId;
        private long nextTabletDecayScanMilliseconds;
        private long nextActiveEffectPersistenceSaveMilliseconds;
        private bool activeEffectRegistryDirty;
        private RustweaveActiveEffectRegistryConfig activeEffectRegistry = new();

        public RustweaveServerController(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
            spellExecutor = new SpellEffectExecutor(sapi);
        }

        public void Start()
        {
            channel = sapi.Network.RegisterChannel(RustweaveConstants.NetworkChannelName);
            channel.RegisterMessageType(typeof(RustweaveActionPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetLockPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetPreviewPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetedWarningPacket));
            channel.SetMessageHandler<RustweaveActionPacket>(OnClientPacket);
            channel.SetMessageHandler<RustweaveTargetPreviewPacket>(OnClientPreviewPacket);
            LoadActiveEffectRegistry();
            sapi.Event.PlayerJoin += OnServerPlayerJoin;
            sapi.Event.PlayerNowPlaying += OnServerPlayerNowPlaying;
            sapi.Event.PlayerLeave += OnServerPlayerLeave;
            RegisterCommands();
            tickListenerId = sapi.Event.RegisterGameTickListener(OnServerTick, 50, 50);
        }

        private void LoadActiveEffectRegistry()
        {
            try
            {
                activeEffectRegistry = sapi.LoadModConfig<RustweaveActiveEffectRegistryConfig>(RustweaveConstants.ActiveEffectConfigFileName) ?? new RustweaveActiveEffectRegistryConfig();
            }
            catch (Exception exception)
            {
                activeEffectRegistry = new RustweaveActiveEffectRegistryConfig();
                sapi.Logger.Warning("[TheRustweave] Failed to load active effect registry, using defaults: {0}", exception.Message);
            }

            activeEffectRegistry.ActiveEffects ??= new List<RustweaveActiveEffectRecord>();
            activeEffectRecords.Clear();

            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            foreach (var record in activeEffectRegistry.ActiveEffects.Where(entry => entry != null))
            {
                var normalized = record!.Clone();
                if (string.IsNullOrWhiteSpace(normalized.EffectId))
                {
                    normalized.EffectId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                }

                if (normalized.ExpiresAtTotalDays > 0d && normalized.ExpiresAtTotalDays < nowDays)
                {
                    continue;
                }

                activeEffectRecords[normalized.EffectId] = normalized;
            }

            activeEffectRegistry.ActiveEffects = activeEffectRecords.Values.Select(effect => effect.Clone()).ToList();
            try
            {
                sapi.StoreModConfig(activeEffectRegistry, RustweaveConstants.ActiveEffectConfigFileName);
            }
            catch (Exception exception)
            {
                sapi.Logger.Warning("[TheRustweave] Failed to store active effect registry defaults: {0}", exception.Message);
            }
        }

        private void SaveActiveEffectRegistry()
        {
            try
            {
                activeEffectRegistry.ActiveEffects = activeEffectRecords.Values.Select(effect => effect.Clone()).ToList();
                sapi.StoreModConfig(activeEffectRegistry, RustweaveConstants.ActiveEffectConfigFileName);
                activeEffectRegistryDirty = false;
                nextActiveEffectPersistenceSaveMilliseconds = sapi.World.ElapsedMilliseconds + 5000;
            }
            catch (Exception exception)
            {
                sapi.Logger.Warning("[TheRustweave] Failed to store active effect registry: {0}", exception.Message);
            }
        }

        private void RegisterCommands()
        {
            var root = sapi.ChatCommands.Create("rustweave")
                .WithDescription("Rustweave admin spell progression commands")
                .RequiresPrivilege("controlserver");

            root.BeginSubCommand("learn")
                .WithDescription("Learn a spell code for the target player")
                .WithArgs(sapi.ChatCommands.Parsers.Word("spellcode"))
                .HandleWith(args => HandleLearnCommand(args))
                .EndSubCommand();

            root.BeginSubCommand("forget")
                .WithDescription("Forget a learned spell code for the target player")
                .WithArgs(sapi.ChatCommands.Parsers.Word("spellcode"))
                .HandleWith(args => HandleForgetCommand(args))
                .EndSubCommand();

            root.BeginSubCommand("learnall")
                .WithDescription("Learn every enabled spell for the target player")
                .HandleWith(args => HandleLearnAllCommand(args))
                .EndSubCommand();

            root.BeginSubCommand("resetspells")
                .WithDescription("Reset spell progression to starter spells")
                .HandleWith(args => HandleResetSpellsCommand(args))
                .EndSubCommand();

            root.BeginSubCommand("rank")
                .WithDescription("Show the target player's knowledge rank")
                .HandleWith(args => HandleRankCommand(args))
                .EndSubCommand();

            root.BeginSubCommand("casts")
                .WithDescription("Show successful cast count for a spell code")
                .WithArgs(sapi.ChatCommands.Parsers.Word("spellcode"))
                .HandleWith(args => HandleCastsCommand(args))
                .EndSubCommand();

            var mentor = root.BeginSubCommand("mentor")
                .WithDescription("Rustplane Sage mentor study commands");

            mentor.BeginSubCommand("list")
                .WithDescription("List teachable Rustplane Sage spells")
                .HandleWith(args => HandleMentorListCommand(args))
                .EndSubCommand();

            mentor.BeginSubCommand("study")
                .WithDescription("Start a Rustplane Sage study for a spell code")
                .WithArgs(sapi.ChatCommands.Parsers.Word("spellcode"))
                .HandleWith(args => HandleMentorStudyCommand(args))
                .EndSubCommand();

            mentor.BeginSubCommand("status")
                .WithDescription("Show active Rustplane Sage studies")
                .HandleWith(args => HandleMentorStatusCommand(args))
                .EndSubCommand();

            mentor.BeginSubCommand("complete")
                .WithDescription("Force-complete a Rustplane Sage study")
                .WithArgs(sapi.ChatCommands.Parsers.Word("spellcode"))
                .HandleWith(args => HandleMentorCompleteCommand(args))
                .EndSubCommand();

            mentor.EndSubCommand();
        }

        private TextCommandResult HandleLearnCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var spellCode = args[0] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return TextCommandResult.Error("Usage: /rustweave learn <spellcode>");
            }

            if (RustweaveStateService.LearnSpell(player, spellCode, "admin-command", out var failureReason))
            {
                sapi.Logger.Debug("[TheRustweave] Admin learned spell '{0}' for player '{1}'.", spellCode, player.PlayerUID);
                return TextCommandResult.Success($"Learned {spellCode}.");
            }

            return failureReason.IndexOf("already learned", StringComparison.OrdinalIgnoreCase) >= 0
                ? TextCommandResult.Success(failureReason)
                : TextCommandResult.Error(string.IsNullOrWhiteSpace(failureReason) ? "Unable to learn spell." : failureReason);
        }

        private TextCommandResult HandleForgetCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var spellCode = args[0] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return TextCommandResult.Error("Usage: /rustweave forget <spellcode>");
            }

            if (RustweaveStateService.ForgetSpell(player, spellCode, "admin-command", out var failureReason))
            {
                sapi.Logger.Debug("[TheRustweave] Admin forgot spell '{0}' for player '{1}'.", spellCode, player.PlayerUID);
                return TextCommandResult.Success($"Forgot {spellCode}.");
            }

            return failureReason.IndexOf("was not learned", StringComparison.OrdinalIgnoreCase) >= 0
                ? TextCommandResult.Success(failureReason)
                : TextCommandResult.Error(string.IsNullOrWhiteSpace(failureReason) ? "Unable to forget spell." : failureReason);
        }

        private TextCommandResult HandleLearnAllCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            if (RustweaveStateService.LearnAllEnabledSpells(player, out var failureReason))
            {
                sapi.Logger.Debug("[TheRustweave] Admin learned all enabled spells for player '{0}'.", player.PlayerUID);
                return TextCommandResult.Success("All enabled spells are now learned.");
            }

            return string.IsNullOrWhiteSpace(failureReason)
                ? TextCommandResult.Success("No changes made.")
                : TextCommandResult.Error(failureReason);
        }

        private TextCommandResult HandleResetSpellsCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            if (RustweaveStateService.ResetSpellProgression(player, out var failureReason))
            {
                sapi.Logger.Debug("[TheRustweave] Admin reset spell progression for player '{0}'.", player.PlayerUID);
                return TextCommandResult.Success("Spell progression reset to starter spells.");
            }

            return TextCommandResult.Error(string.IsNullOrWhiteSpace(failureReason) ? "Unable to reset spell progression." : failureReason);
        }

        private TextCommandResult HandleRankCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var state = GetState(player);
            var rankName = RustweaveStateService.GetKnowledgeRankName(state);
            var rankIndex = RustweaveStateService.GetKnowledgeRankIndex(state);
            var learnedCount = RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Count(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && !RustweaveStateService.IsLoreweaveSpell(spell) && RustweaveStateService.IsSpellLearned(spell.Code, state));

            return TextCommandResult.Success($"{rankName} (rank {rankIndex + 1}) with {learnedCount} learned non-Loreweave spell(s).");
        }

        private TextCommandResult HandleCastsCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var spellCode = args[0] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return TextCommandResult.Error("Usage: /rustweave casts <spellcode>");
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) || spell == null || !spell.Enabled)
            {
                return TextCommandResult.Error($"Unknown or disabled spell: {spellCode}");
            }

            var state = GetState(player);
            var count = RustweaveStateService.GetSpellCastCount(state, spellCode);
            return TextCommandResult.Success($"{spellCode}: {count} successful cast(s).");
        }

        private TextCommandResult HandleMentorListCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            if (!RustweaveMentorService.IsEnabled())
            {
                return TextCommandResult.Error(Lang.Get("game:rustweave-mentor-disabled"));
            }

            var teachables = RustweaveMentorService.GetTeachableSpells(player);
            if (teachables.Count == 0)
            {
                return TextCommandResult.Success(Lang.Get("game:rustweave-mentor-no-spells"));
            }

            var lines = teachables
                .Select(spell =>
                {
                    var studyHours = RustweaveMentorService.GetStudyHoursForTier(spell.Tier);
                    var costEntries = RustweaveMentorService.BuildCostEntries(spell.Tier);
                    return $"{spell.Code} - {RustweaveStateService.GetSpellDisplayName(spell.Code)} (tier {spell.Tier}, {spell.School}, study {RustweaveMentorService.FormatStudyTime(studyHours)}h, cost {RustweaveMentorService.FormatCostEntries(costEntries)})";
                })
                .ToList();

            return TextCommandResult.Success($"Teachable spell(s):\n{string.Join("\n", lines)}");
        }

        private TextCommandResult HandleMentorStudyCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var spellCode = args[0] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return TextCommandResult.Error("Usage: /rustweave mentor study <spellcode>");
            }

            if (!RustweaveMentorService.TryValidateMentorSpell(player, spellCode, out var spell, out var failureReason) || spell == null)
            {
                if (string.Equals(failureReason, "You already know this spell.", StringComparison.OrdinalIgnoreCase))
                {
                    return TextCommandResult.Error(Lang.Get("game:rustweave-mentor-already-known"));
                }

                return TextCommandResult.Error(string.IsNullOrWhiteSpace(failureReason) ? "Unable to begin study." : failureReason);
            }

            var state = GetState(player);
            RustweaveMentorService.NormalizeMentorStudies(state);

            if (state.MentorStudies.Any(study => string.Equals(study.SpellCode, spell.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return TextCommandResult.Error("You are already studying that working.");
            }

            var costEntries = RustweaveMentorService.BuildCostEntries(spell.Tier);
            if (!RustweaveMentorService.HasCost(player, costEntries))
            {
                sapi.Logger.Debug("[TheRustweave] Mentor study rejected for player '{0}' because they could not pay {1}.", player.PlayerUID, RustweaveMentorService.FormatCostEntries(costEntries));
                player.SendMessage(0, Lang.Get("game:rustweave-mentor-insufficient-payment"), EnumChatType.Notification, null);
                return TextCommandResult.Error("You lack the required offering.");
            }

            if (!RustweaveMentorService.ConsumeCost(player, costEntries))
            {
                sapi.Logger.Warning("[TheRustweave] Mentor cost consumption failed for player '{0}' and spell '{1}'.", player.PlayerUID, spell.Code);
                return TextCommandResult.Error("You lack the required offering.");
            }

            var nowHours = GetWorldHours(player);
            var studyHours = RustweaveMentorService.GetStudyHoursForTier(spell.Tier);
            state.MentorStudies.Add(new RustweaveMentorStudyData
            {
                SpellCode = spell.Code,
                MentorType = RustweaveMentorTypes.RustplaneSage,
                StartTime = nowHours,
                FinishTime = nowHours + studyHours,
                PaidCost = costEntries.Select(entry => entry.Clone()).ToList()
            });

            SaveAndSyncState(player, state);
            sapi.Logger.Debug("[TheRustweave] Mentor study started for player '{0}' on spell '{1}' (finish at {2:0.0}h).", player.PlayerUID, spell.Code, nowHours + studyHours);
            player.SendMessage(0, Lang.Get("game:rustweave-mentor-started", RustweaveStateService.GetSpellDisplayName(spell.Code)), EnumChatType.Notification, null);
            return TextCommandResult.Success($"The Rustplane Sage begins your study of {spell.Code}.");
        }

        private TextCommandResult HandleMentorStatusCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            ProcessMentorStudies(player);

            var state = GetState(player);
            RustweaveMentorService.NormalizeMentorStudies(state);
            var studies = RustweaveMentorService.GetActiveStudies(state);
            if (studies.Count == 0)
            {
                return TextCommandResult.Success("No active mentor studies.");
            }

            var nowHours = GetWorldHours(player);
            var lines = studies
                .OrderBy(study => study.FinishTime)
                .ThenBy(study => study.SpellCode, StringComparer.OrdinalIgnoreCase)
                .Select(study =>
                {
                    var displayName = RustweaveStateService.GetSpellDisplayName(study.SpellCode);
                    var remainingHours = Math.Max(0d, study.FinishTime - nowHours);
                    return $"{study.SpellCode} - {displayName} ({RustweaveMentorService.FormatStudyTime(remainingHours)}h remaining, paid {RustweaveMentorService.FormatCostEntries(study.PaidCost)})";
                })
                .ToList();

            return TextCommandResult.Success($"Active mentor studies:\n{string.Join("\n", lines)}");
        }

        private TextCommandResult HandleMentorCompleteCommand(TextCommandCallingArgs args)
        {
            if (args.Caller?.Player is not IServerPlayer player)
            {
                return TextCommandResult.Error("A player caller is required for this command.");
            }

            var spellCode = args[0] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return TextCommandResult.Error("Usage: /rustweave mentor complete <spellcode>");
            }

            if (TryCompleteMentorStudy(player, spellCode, true, out var message))
            {
                if (string.Equals(message, "invalid", StringComparison.OrdinalIgnoreCase))
                {
                    return TextCommandResult.Error("The Sage cannot teach that working.");
                }

                return TextCommandResult.Success(string.IsNullOrWhiteSpace(message) ? $"Completed study for {spellCode}." : message);
            }

            return TextCommandResult.Error(string.IsNullOrWhiteSpace(message) ? "Unable to complete mentor study." : message);
        }

        private bool TryCompleteMentorStudy(IServerPlayer player, string spellCode, bool force, out string message)
        {
            message = string.Empty;
            if (player == null || string.IsNullOrWhiteSpace(spellCode))
            {
                message = "Invalid spell code.";
                return false;
            }

            var state = GetState(player);
            RustweaveMentorService.NormalizeMentorStudies(state);
            var study = state.MentorStudies.FirstOrDefault(entry => string.Equals(entry.SpellCode, spellCode, StringComparison.OrdinalIgnoreCase));
            if (study == null)
            {
                message = "No active mentor study found for that spell.";
                return false;
            }

            var changed = TryFinalizeMentorStudy(player, state, study, force, out var learnedNow, out var statusMessage);
            if (!changed)
            {
                message = "The study is not complete yet.";
                return false;
            }

            SaveAndSyncState(player, state);

            if (learnedNow)
            {
                message = string.IsNullOrWhiteSpace(statusMessage)
                    ? $"Your study is complete. You learned {RustweaveStateService.GetSpellDisplayName(spellCode)}."
                    : statusMessage;
            }
            else if (string.Equals(statusMessage, "already learned", StringComparison.OrdinalIgnoreCase))
            {
                message = "You already know this spell.";
            }
            else if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                message = statusMessage;
            }

            return true;
        }

        private bool TryFinalizeMentorStudy(IServerPlayer player, RustweavePlayerStateData state, RustweaveMentorStudyData study, bool force, out bool learnedNow, out string statusMessage)
        {
            learnedNow = false;
            statusMessage = string.Empty;

            if (player == null || state == null || study == null)
            {
                statusMessage = "Invalid mentor study.";
                return false;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(study.SpellCode, out var spell) || spell == null || RustweaveStateService.IsHiddenSpell(spell) || RustweaveStateService.IsLoreweaveSpell(spell))
            {
                state.MentorStudies.Remove(study);
                sapi.Logger.Warning("[TheRustweave] Mentor study for player '{0}' removed because spell '{1}' is invalid.", player.PlayerUID, study.SpellCode);
                player.SendMessage(0, Lang.Get("game:rustweave-mentor-invalid"), EnumChatType.Notification, null);
                statusMessage = "invalid";
                return true;
            }

            var nowHours = GetWorldHours(player);
            if (!force && nowHours < study.FinishTime)
            {
                return false;
            }

            if (RustweaveStateService.IsSpellLearned(player, study.SpellCode))
            {
                state.MentorStudies.Remove(study);
                sapi.Logger.Debug("[TheRustweave] Mentor study for player '{0}' removed because spell '{1}' was already learned.", player.PlayerUID, study.SpellCode);
                statusMessage = "already learned";
                return true;
            }

            if (RustweaveStateService.LearnSpell(state, study.SpellCode, RustweaveMentorTypes.RustplaneSageStudySource, out var failureReason))
            {
                state.MentorStudies.Remove(study);
                learnedNow = true;
                sapi.Logger.Debug("[TheRustweave] Mentor study for player '{0}' completed spell '{1}'.", player.PlayerUID, study.SpellCode);
                player.SendMessage(0, Lang.Get("game:rustweave-mentor-complete", RustweaveStateService.GetSpellDisplayName(study.SpellCode)), EnumChatType.Notification, null);
                statusMessage = $"Your study is complete. You learned {RustweaveStateService.GetSpellDisplayName(study.SpellCode)}.";
                return true;
            }

            state.MentorStudies.Remove(study);
            sapi.Logger.Warning("[TheRustweave] Mentor study for player '{0}' failed for spell '{1}': {2}", player.PlayerUID, study.SpellCode, failureReason);
            statusMessage = string.IsNullOrWhiteSpace(failureReason) ? "Unable to complete mentor study." : failureReason;
            return true;
        }

        private double GetWorldHours(IServerPlayer player)
        {
            return player?.Entity?.World?.Calendar?.TotalHours
                ?? sapi.World?.Calendar?.TotalHours
                ?? 0d;
        }

        private sealed class RustTabletVentingSession
        {
            public long StartedAtMilliseconds { get; set; }

            public int TransferredCorruption { get; set; }
        }

        private sealed class RustweaveTimedStatModifier
        {
            public long TargetEntityId { get; set; }

            public string StatCategory { get; set; } = string.Empty;

            public string ModifierCode { get; set; } = string.Empty;

            public float Delta { get; set; }

            public long ExpiresAtMilliseconds { get; set; }

            public string SpellCode { get; set; } = string.Empty;

            public string EffectType { get; set; } = string.Empty;
        }

        private sealed class RustweaveTimedDamageOverTime
        {
            public long TargetEntityId { get; set; }

            public long SourceEntityId { get; set; }

            public string EffectCode { get; set; } = string.Empty;

            public float DamagePerTick { get; set; }

            public long TickIntervalMilliseconds { get; set; }

            public long NextTickAtMilliseconds { get; set; }

            public long ExpiresAtMilliseconds { get; set; }

            public string SpellCode { get; set; } = string.Empty;

            public string EffectType { get; set; } = string.Empty;
        }

        private sealed class RustweaveTimedShield
        {
            public long TargetEntityId { get; set; }

            public string ShieldCode { get; set; } = string.Empty;

            public float RemainingAmount { get; set; }

            public long ExpiresAtMilliseconds { get; set; }

            public string SpellCode { get; set; } = string.Empty;

            public string EffectType { get; set; } = string.Empty;
        }

        private void OnServerPlayerJoin(IServerPlayer player)
        {
            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                return;
            }

            GetState(player);
            ScheduleTabletDecayScan(player, 1500);
        }

        private void OnServerPlayerNowPlaying(IServerPlayer player)
        {
            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                return;
            }

            var state = GetState(player);
            SaveAndSyncState(player, state);
            ProcessMentorStudies(player, state);
            ScheduleTabletDecayScan(player, 1500);
        }

        private void OnServerPlayerLeave(IServerPlayer player)
        {
            if (player == null)
            {
                return;
            }

            activeCasts.Remove(player.PlayerUID);
            ClearCastState(player);
            ClearTargetedWarningState(casterPlayerUid: player.PlayerUID, targetPlayerUid: player.PlayerUID, sendInactive: true);
            ClearTargetPreviewState(player);
            RemoveActiveEffectsForPlayer(player.PlayerUID);
            activeTabletVents.Remove(player.PlayerUID);
            pendingTabletDecayScans.Remove(player.PlayerUID);
            spellCooldowns.Remove(player.PlayerUID);
        }

        public bool TryStartTabletVenting(EntityPlayer entityPlayer)
        {
            if (entityPlayer == null)
            {
                return false;
            }

            var player = GetOnlineServerPlayer(entityPlayer.PlayerUID);
            if (player == null || player.Entity == null || !player.Entity.Alive || !RustweaveStateService.IsRustweaver(player))
            {
                return false;
            }

            var state = GetState(player);
            if (state.CurrentTemporalCorruption > state.EffectiveTemporalCorruptionThreshold)
            {
                return false;
            }

            var tablet = player.InventoryManager?.ActiveHotbarSlot?.Itemstack;
            if (tablet == null || !RustweaveStateService.IsHoldingTablet(player))
            {
                return false;
            }

            RustweaveStateService.ApplyTabletPassiveDecay(player.Entity?.World ?? sapi.World, player.InventoryManager?.ActiveHotbarSlot);

            if (RustweaveStateService.GetTabletStoredCorruption(tablet) >= RustweaveConstants.TabletCapacity)
            {
                return false;
            }

            activeTabletVents[player.PlayerUID] = new RustTabletVentingSession
            {
                StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                TransferredCorruption = 0
            };

            return true;
        }

        public void StopTabletVenting(EntityPlayer entityPlayer)
        {
            if (entityPlayer == null)
            {
                return;
            }

            activeTabletVents.Remove(entityPlayer.PlayerUID);
        }

        public bool TryUseDiscoveryItem(EntityPlayer entityPlayer, ItemSlot? slot)
        {
            if (entityPlayer == null || slot?.Itemstack?.Collectible?.Code == null)
            {
                return false;
            }

            var player = GetOnlineServerPlayer(entityPlayer.PlayerUID);
            if (player == null || player.Entity == null || !player.Entity.Alive || !RustweaveStateService.IsRustweaver(player))
            {
                player?.SendMessage(0, Lang.Get("game:rustweave-discovery-reject"), EnumChatType.Notification, null);
                return false;
            }

            var itemCode = slot.Itemstack.Collectible.Code.Path ?? string.Empty;
            if (!RustweaveRuntime.TryGetDiscoveryItemDefinition(itemCode, out var definition) || definition == null)
            {
                sapi.Logger.Warning("[TheRustweave] Discovery item '{0}' has no configured spell pool.", itemCode);
                player.SendMessage(0, Lang.Get("game:rustweave-no-new-spell-from-this"), EnumChatType.Notification, null);
                return false;
            }

            var state = GetState(player);
            sapi.Logger.Debug("[TheRustweave] Discovery item used by player '{0}': {1}", player.PlayerUID, itemCode);

            if (definition.SingleUsePerPlayer && RustweaveStateService.HasUsedDiscoveryItem(state, itemCode))
            {
                sapi.Logger.Debug("[TheRustweave] Discovery item '{0}' was rejected for player '{1}' because it was already used once.", itemCode, player.PlayerUID);
                player.SendMessage(0, Lang.Get("game:rustweave-no-new-spell-from-this"), EnumChatType.Notification, null);
                return false;
            }

            var eligibleSpells = RustweaveStateService.GetDiscoveryEligibleSpells(player, itemCode);
            sapi.Logger.Debug("[TheRustweave] Discovery item '{0}' resolved {1} eligible spell(s) for player '{2}'.", itemCode, eligibleSpells.Count, player.PlayerUID);

            if (eligibleSpells.Count == 0)
            {
                var poolSpells = RustweaveStateService.GetDiscoveryPoolCandidates(player, itemCode);
                if (poolSpells.Count == 0)
                {
                    sapi.Logger.Warning("[TheRustweave] Discovery item '{0}' found no eligible spells for player '{1}'.", itemCode, player.PlayerUID);
                    player.SendMessage(0, Lang.Get("game:rustweave-no-new-spell-from-this"), EnumChatType.Notification, null);
                    return false;
                }

                var researchProgress = RustweaveStateService.IncrementDiscoveryResearchProgress(state, itemCode, 1);
                SaveAndSyncState(player, state);
                sapi.Logger.Debug("[TheRustweave] Discovery item '{0}' granted research progress {1} for player '{2}'.", itemCode, researchProgress, player.PlayerUID);
                player.SendMessage(0, Lang.Get("game:rustweave-discovery-fragments"), EnumChatType.Notification, null);
                return true;
            }

            var selectedSpell = eligibleSpells[sapi.World.Rand.Next(eligibleSpells.Count)];
            if (!RustweaveStateService.LearnSpell(player, selectedSpell.Code, $"discovery:{itemCode}", out var failureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Discovery item '{0}' failed to learn spell '{1}' for player '{2}': {3}", itemCode, selectedSpell.Code, player.PlayerUID, failureReason);
                player.SendMessage(0, Lang.Get("game:rustweave-no-new-spell-from-this"), EnumChatType.Notification, null);
                return false;
            }

            if (definition.SingleUsePerPlayer)
            {
                RustweaveStateService.MarkDiscoveryItemUsed(state, itemCode);
            }

            if (definition.ConsumeOnSuccess)
            {
                slot.Itemstack.StackSize = Math.Max(0, slot.Itemstack.StackSize - 1);
                if (slot.Itemstack.StackSize <= 0)
                {
                    slot.Itemstack = null;
                }

                slot.MarkDirty();
            }

            SaveAndSyncState(player, state);
            sapi.Logger.Debug("[TheRustweave] Discovery item '{0}' taught spell '{1}' to player '{2}' (consumed={3}).", itemCode, selectedSpell.Code, player.PlayerUID, definition.ConsumeOnSuccess);
            player.SendMessage(0, Lang.Get("game:rustweave-spell-learned-from-discovery", RustweaveStateService.GetSpellDisplayName(selectedSpell.Code)), EnumChatType.Notification, null);
            return true;
        }

        private void OnClientPacket(IServerPlayer fromPlayer, RustweaveActionPacket packet)
        {
            if (fromPlayer == null || packet == null)
            {
                return;
            }

            if (!RustweaveStateService.IsRustweaver(fromPlayer))
            {
                fromPlayer.SendMessage(0, Lang.Get("game:rustweave-tome-reject"), EnumChatType.Notification, null);
                return;
            }

            var state = GetState(fromPlayer);
            switch (packet.Action)
            {
                case RustweaveActionType.RequestStartCast:
                    StartCast(fromPlayer, state, packet.SlotIndex);
                    break;
                case RustweaveActionType.RequestCancelCast:
                    CancelCast(fromPlayer, state, Lang.Get("game:rustweave-cast-cancel"), true, true);
                    break;
                case RustweaveActionType.RequestSelectPrepared:
                    if (activeCasts.ContainsKey(fromPlayer.PlayerUID))
                    {
                        CancelCast(fromPlayer, state, Lang.Get("game:rustweave-cast-cancel"), true, true);
                    }

                    if (RustweaveStateService.TrySelectPreparedSpell(state, packet.SlotIndex))
                    {
                        var displaySlot = RustweaveStateService.ToDisplaySlotNumber(packet.SlotIndex);
                        sapi.Logger.Debug("[TheRustweave] Active prepared slot changed to {0} (internal index {1}) for player '{2}'.", displaySlot, packet.SlotIndex, fromPlayer.PlayerUID);
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-selected-slot", displaySlot, RustweaveStateService.GetPreparedSlotDisplayText(state, state.SelectedPreparedSpellIndex)), EnumChatType.Notification, null);
                    }
                    break;
                case RustweaveActionType.RequestPrepareSpell:
                    sapi.Logger.Debug("[TheRustweave] Prepare request received from player '{0}' for spell '{1}' (requested slot {2}).", fromPlayer.PlayerUID, packet.SpellCode, RustweaveStateService.DescribePreparedSlot(packet.SlotIndex));

                    if (string.IsNullOrWhiteSpace(packet.SpellCode) || !RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(packet.SpellCode, out var spell) || spell == null || !RustweaveStateService.IsSpellLearned(spell.Code, state))
                    {
                        sapi.Logger.Warning("[TheRustweave] Prepare request rejected for player '{0}' because spell '{1}' is unavailable or locked.", fromPlayer.PlayerUID, packet.SpellCode);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-unlearned"), EnumChatType.Notification, null);
                        break;
                    }

                    var chosenSlot = RustweaveStateService.ResolvePrepareTargetSlot(state, packet.SlotIndex);
                    sapi.Logger.Debug("[TheRustweave] Prepare target slot resolved to {0} (internal index {1}) for player '{2}'.", RustweaveStateService.DescribePreparedSlot(chosenSlot), chosenSlot, fromPlayer.PlayerUID);

                    if (chosenSlot < 0)
                    {
                        sapi.Logger.Warning("[TheRustweave] Prepare request rejected for player '{0}' because no prepared slot was available.", fromPlayer.PlayerUID);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-no-empty-spell-slots"), EnumChatType.Notification, null);
                        break;
                    }

                    var existingSlot = RustweaveStateService.FindPreparedSpellSlot(state, spell.Code);
                    if (existingSlot >= 0 && existingSlot != chosenSlot)
                    {
                        sapi.Logger.Warning("[TheRustweave] Prepare request rejected for player '{0}' because spell '{1}' is already prepared in slot {2}.", fromPlayer.PlayerUID, spell.Code, existingSlot);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-duplicate"), EnumChatType.Notification, null);
                        break;
                    }

                    var beforeValue = RustweaveStateService.GetPreparedSpellCode(state, chosenSlot);
                    if (RustweaveStateService.TryPrepareSpell(state, spell.Code, chosenSlot))
                    {
                        var afterValue = RustweaveStateService.GetPreparedSpellCode(state, chosenSlot);
                        sapi.Logger.Debug("[TheRustweave] Stored spell '{0}' in prepared slot {1} (internal index {2}) for player '{3}' (before='{4}', after='{5}').", spell.Code, RustweaveStateService.ToDisplaySlotNumber(chosenSlot), chosenSlot, fromPlayer.PlayerUID, beforeValue, afterValue);
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-prepared-slot", RustweaveStateService.GetSpellDisplayName(spell.Code), RustweaveStateService.ToDisplaySlotNumber(chosenSlot)), EnumChatType.Notification, null);
                    }
                    else
                    {
                        sapi.Logger.Warning("[TheRustweave] Prepare request rejected for player '{0}' because there were no available prepared slots.", fromPlayer.PlayerUID);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-no-empty-spell-slots"), EnumChatType.Notification, null);
                    }
                    break;
                case RustweaveActionType.RequestUnprepareSpell:
                    if (RustweaveStateService.TryUnprepareSpell(state, packet.SlotIndex))
                    {
                        sapi.Logger.Debug("[TheRustweave] Prepared slot {0} (internal index {1}) cleared for player '{2}'.", RustweaveStateService.ToDisplaySlotNumber(packet.SlotIndex), packet.SlotIndex, fromPlayer.PlayerUID);
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-cleared-prepared-slot", RustweaveStateService.ToDisplaySlotNumber(packet.SlotIndex)), EnumChatType.Notification, null);
                    }
                    break;
            }
        }

        private void OnServerTick(float dt)
        {
            ProcessPendingTabletDecayScans();
            ProcessPassiveTabletDecay();
            ProcessTimedSpellEffects();
            ProcessActiveEffectRecords();
            SampleEntityHistory();
            ProcessTargetPreviewStates();
            ProcessActiveTargetedWarningRecords();

            foreach (var onlinePlayer in sapi.World.AllOnlinePlayers)
            {
                if (onlinePlayer is not IServerPlayer serverPlayer)
                {
                    continue;
                }

                if (!RustweaveStateService.IsRustweaver(serverPlayer))
                {
                    if (activeCasts.ContainsKey(serverPlayer.PlayerUID))
                    {
                        var staleState = GetState(serverPlayer);
                        CancelCast(serverPlayer, staleState, Lang.Get("game:rustweave-cast-cancel"), true, true);
                    }

                    activeTabletVents.Remove(serverPlayer.PlayerUID);

                    continue;
                }

                if (serverPlayer.Entity == null || !serverPlayer.Entity.Alive)
                {
                    activeCasts.Remove(serverPlayer.PlayerUID);
                    ClearCastState(serverPlayer);
                    activeTabletVents.Remove(serverPlayer.PlayerUID);
                    continue;
                }

                var state = GetState(serverPlayer);
                ProcessMentorStudies(serverPlayer, state);
                ProcessTabletVenting(serverPlayer, state);
                RustweaveStateService.UpdateOverloadState(state, sapi.World.ElapsedMilliseconds);

                if (RustweaveStateService.IsOverloadExpired(state, sapi.World.ElapsedMilliseconds))
                {
                    TriggerOverloadExplosion(serverPlayer);
                    continue;
                }

                if (!activeCasts.TryGetValue(serverPlayer.PlayerUID, out var castState))
                {
                    continue;
                }

                castState.ElapsedMilliseconds = sapi.World.ElapsedMilliseconds - castState.StartedAtMilliseconds;
                if (!TryCanContinueCast(serverPlayer, castState, out var cancelMessage))
                {
                    CancelCast(serverPlayer, state, cancelMessage, true, true);
                    continue;
                }

                if (state.CurrentTemporalCorruption >= state.EffectiveTemporalCorruptionThreshold && castState.ElapsedMilliseconds < castState.DurationMilliseconds)
                {
                    CancelCast(serverPlayer, state, Lang.Get("game:rustweave-cast-locked"), true, false);
                    continue;
                }

                if (castState.ElapsedMilliseconds < castState.DurationMilliseconds)
                {
                    SyncCastState(serverPlayer, castState);
                    continue;
                }

                FinishCast(serverPlayer, state, castState);
            }

            if (activeEffectRegistryDirty && sapi.World.ElapsedMilliseconds >= nextActiveEffectPersistenceSaveMilliseconds)
            {
                SaveActiveEffectRegistry();
            }
        }

        private void SampleEntityHistory()
        {
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            if (nowMilliseconds <= 0)
            {
                return;
            }

            foreach (var onlinePlayer in sapi.World.AllOnlinePlayers.OfType<IServerPlayer>())
            {
                if (onlinePlayer.Entity == null || !onlinePlayer.Entity.Alive)
                {
                    continue;
                }

                SampleEntityHistory(onlinePlayer.Entity);
            }

            foreach (var effect in activeEffectRecords.Values)
            {
                if (effect == null || effect.TargetEntityId <= 0)
                {
                    continue;
                }

                var entity = sapi.World.GetEntityById(effect.TargetEntityId);
                if (entity != null && entity.Alive)
                {
                    SampleEntityHistory(entity);
                }
            }
        }

        private void SampleEntityHistory(Entity entity)
        {
            if (entity?.Pos == null)
            {
                return;
            }

            var entityId = entity.EntityId;
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            if (nextHistorySampleMilliseconds.TryGetValue(entityId, out var nextSampleMilliseconds) && nowMilliseconds < nextSampleMilliseconds)
            {
                return;
            }

            nextHistorySampleMilliseconds[entityId] = nowMilliseconds + 1000;
            if (!entityHistory.TryGetValue(entityId, out var samples))
            {
                samples = new LinkedList<RustweaveEntityHistorySample>();
                entityHistory[entityId] = samples;
            }

            samples.AddLast(new RustweaveEntityHistorySample
            {
                EntityId = entityId,
                PlayerUid = entity is EntityPlayer entityPlayer ? entityPlayer.PlayerUID ?? string.Empty : string.Empty,
                X = entity.Pos.XYZ.X,
                Y = entity.Pos.XYZ.Y,
                Z = entity.Pos.XYZ.Z,
                Yaw = entity.Pos.Yaw,
                Pitch = entity.Pos.Pitch,
                SampledAtMilliseconds = nowMilliseconds
            });

            while (samples.Count > 30)
            {
                samples.RemoveFirst();
            }
        }

        private void ProcessActiveEffectRecords()
        {
            if (activeEffectRecords.Count == 0)
            {
                return;
            }

            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            var nowDays = Math.Max(0d, sapi.World.Calendar?.TotalDays ?? 0d);
            var expired = new List<string>();

            foreach (var pair in activeEffectRecords.ToArray())
            {
                var record = pair.Value;
                if (record == null)
                {
                    expired.Add(pair.Key);
                    continue;
                }

                if (record.ExpiresAtMilliseconds > 0 && nowMilliseconds >= record.ExpiresAtMilliseconds)
                {
                    expired.Add(pair.Key);
                    continue;
                }

                if (record.ExpiresAtTotalDays > 0d && nowDays >= record.ExpiresAtTotalDays)
                {
                    expired.Add(pair.Key);
                    continue;
                }

                if (record.IsArea)
                {
                    ProcessActiveAreaEffect(record);
                }
                else if (!string.Equals(record.RecordKind, "summon", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessActiveEntityEffect(record);
                }
            }

            foreach (var effectId in expired)
            {
                TryRemoveActiveEffect(effectId, true);
            }
        }

        private void ProcessActiveAreaEffect(RustweaveActiveEffectRecord record)
        {
            var center = new Vec3d(record.CenterX, record.CenterY, record.CenterZ);
            var radius = Math.Max(0d, record.Radius);
            if (radius <= 0d)
            {
                return;
            }

            var entities = sapi.World.GetEntitiesAround(center, (float)radius, (float)radius);
            if (entities == null || entities.Length == 0)
            {
                return;
            }

            foreach (var entity in entities.Where(entity => entity != null && entity.Alive))
            {
                if (entity is EntityPlayer player && !string.IsNullOrWhiteSpace(record.TargetPlayerUid) && !string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                switch (record.EffectType)
                {
                    case SpellEffectTypes.StabilizeArea:
                        AdjustEntityCorruption(entity, -Math.Max(1, (int)Math.Round(Math.Max(1f, record.Amount))));
                        break;
                    case SpellEffectTypes.ModifyTemporalStability:
                        AdjustEntityCorruption(entity, -Math.Max(1, (int)Math.Round(record.Amount)));
                        break;
                    case SpellEffectTypes.ModifyCorruptionGain:
                        AdjustEntityCorruption(entity, (int)Math.Round(record.Amount));
                        break;
                    case SpellEffectTypes.ChangeTemperatureArea:
                    case SpellEffectTypes.ChangeEnvironmentalPressure:
                    case SpellEffectTypes.StormPulse:
                        ApplyEnvironmentalPulse(entity, record);
                        break;
                    case SpellEffectTypes.ModifyCropGrowth:
                        ApplyBlockEntityAreaPulse(record, record.Amount, "GrowthStage", "growthStage", "Growth", "growth", "GrowthProgress", "growthProgress", "Progress", "progress");
                        break;
                    case SpellEffectTypes.ModifyFarmlandNutrients:
                        ApplyBlockEntityAreaPulse(record, record.Amount, "Nutrients", "nutrients", "SoilNutrients", "soilNutrients", "Fertility", "fertility", "Moisture", "moisture");
                        break;
                    case SpellEffectTypes.CreateContainmentArea:
                    case SpellEffectTypes.CreateWardArea:
                    case SpellEffectTypes.CreateBarrier:
                    case SpellEffectTypes.CreateBoundaryLine:
                    case SpellEffectTypes.CreateAntiSpreadArea:
                    case SpellEffectTypes.CreateRift:
                        ApplyBarrierPulse(entity, record);
                        break;
                    case SpellEffectTypes.BindEntityToArea:
                        EnforceEntityWithinArea(entity, record);
                        break;
                    case SpellEffectTypes.TetherEntity:
                        EnforceEntityTether(entity, record);
                        break;
                    case SpellEffectTypes.CharmEntity:
                    case SpellEffectTypes.CommandEntity:
                        EnforceCharmedEntity(entity, record);
                        break;
                }
            }
        }

        private void ProcessActiveEntityEffect(RustweaveActiveEffectRecord record)
        {
            var entity = record.TargetEntityId > 0 ? sapi.World.GetEntityById(record.TargetEntityId) : null;
            if (entity == null || !entity.Alive)
            {
                return;
            }

            switch (record.EffectType)
            {
                case SpellEffectTypes.AnchorEntity:
                case SpellEffectTypes.PreventDisplacement:
                    entity.ServerPos.Motion.Set(0, 0, 0);
                    entity.Pos.Motion.Set(0, 0, 0);
                    break;
                case SpellEffectTypes.HealOverTime:
                case SpellEffectTypes.VitalityOverTime:
                    ApplyEnvironmentalPulse(entity, record);
                    break;
                case SpellEffectTypes.ModifyAnimalFertility:
                    if (entity is EntityPlayer)
                    {
                        return;
                    }

                    TryAdjustNumericMember(entity, record.Amount, "Fertility", "fertility", "BreedChance", "breedChance", "MateChance", "mateChance");
                    TrySetBooleanMember(entity, record.Amount >= 0d, "CanMate", "canMate", "ReadyToMate", "readyToMate", "Fertile", "fertile");
                    break;
                case SpellEffectTypes.TetherEntity:
                    EnforceEntityTether(entity, record);
                    break;
                case SpellEffectTypes.BindEntityToArea:
                    EnforceEntityWithinArea(entity, record);
                    break;
                case SpellEffectTypes.CharmEntity:
                case SpellEffectTypes.CommandEntity:
                    EnforceCharmedEntity(entity, record);
                    break;
            }
        }

        private void AdjustEntityCorruption(Entity entity, int delta)
        {
            if (entity is not EntityPlayer player || delta == 0)
            {
                return;
            }

            var serverPlayer = GetOnlineServerPlayer(player.PlayerUID);
            if (serverPlayer == null)
            {
                return;
            }

            var state = GetState(serverPlayer);
            state.CurrentTemporalCorruption = Math.Max(0, Math.Min(state.AbsoluteTemporalCorruptionCap, state.CurrentTemporalCorruption + delta));
            SaveAndSyncState(serverPlayer, state);
        }

        private void ApplyEnvironmentalPulse(Entity entity, RustweaveActiveEffectRecord record)
        {
            if (record.Amount > 0)
            {
                var damageSource = new DamageSource
                {
                    Source = EnumDamageSource.Internal,
                    SourceEntity = null,
                    CauseEntity = null,
                    DamageTier = 1,
                    KnockbackStrength = 0.2f
                };

                entity.ReceiveDamage(damageSource, record.Amount);
            }
            else if (entity.GetBehavior<EntityBehaviorHealth>() is { } health && record.Amount < 0)
            {
                health.Health = Math.Min(health.MaxHealth, health.Health - record.Amount);
            }
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

            var method = blockEntity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, "MarkDirty", StringComparison.OrdinalIgnoreCase));
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
                    args[index] = parameters[index].ParameterType == typeof(bool)
                        ? true
                        : parameters[index].ParameterType.IsValueType
                            ? Activator.CreateInstance(parameters[index].ParameterType)
                            : null;
                }

                method.Invoke(blockEntity, args);
            }
            catch
            {
            }
        }

        private void ApplyBarrierPulse(Entity entity, RustweaveActiveEffectRecord record)
        {
            if (entity == null || !entity.Alive)
            {
                return;
            }

            if (record.Radius > 0 && entity.Pos.XYZ.DistanceTo(new Vec3d(record.CenterX, record.CenterY, record.CenterZ)) > record.Radius)
            {
                return;
            }
        }

        private void EnforceEntityWithinArea(Entity entity, RustweaveActiveEffectRecord record)
        {
            if (entity?.Pos == null)
            {
                return;
            }

            var center = new Vec3d(record.CenterX, record.CenterY, record.CenterZ);
            var maxDistance = Math.Max(1d, record.Radius);
            if (entity.Pos.XYZ.DistanceTo(center) <= maxDistance)
            {
                return;
            }

            if (TryFindNearestSafeTeleportPosition(entity.World, entity, center, out var safePosition))
            {
                entity.TeleportTo(safePosition);
            }
        }

        private void EnforceEntityTether(Entity entity, RustweaveActiveEffectRecord record)
        {
            if (entity?.Pos == null)
            {
                return;
            }

            var origin = record.CasterEntityId > 0 ? sapi.World.GetEntityById(record.CasterEntityId)?.Pos?.XYZ ?? new Vec3d(record.OriginX, record.OriginY, record.OriginZ) : new Vec3d(record.OriginX, record.OriginY, record.OriginZ);
            var maxDistance = Math.Max(1d, record.Radius);
            if (entity.Pos.XYZ.DistanceTo(origin) <= maxDistance)
            {
                return;
            }

            if (TryFindNearestSafeTeleportPosition(entity.World, entity, origin, out var safePosition))
            {
                entity.TeleportTo(safePosition);
            }
        }

        private void EnforceCharmedEntity(Entity entity, RustweaveActiveEffectRecord record)
        {
            if (entity is EntityPlayer || entity is not EntityAgent creature || !creature.Alive)
            {
                return;
            }

            var caster = record.CasterEntityId > 0 ? sapi.World.GetEntityById(record.CasterEntityId) : null;
            var origin = caster?.Pos?.XYZ ?? new Vec3d(record.OriginX, record.OriginY, record.OriginZ);
            if (creature.Pos.XYZ.DistanceTo(origin) > 2.5d)
            {
                if (TryFindNearestSafeTeleportPosition(creature.World, creature, origin, out var safePosition))
                {
                    creature.TeleportTo(safePosition);
                }
            }
        }

        private void ApplyBlockEntityAreaPulse(RustweaveActiveEffectRecord record, double delta, params string[] memberNames)
        {
            var center = new Vec3d(record.CenterX, record.CenterY, record.CenterZ);
            var radius = Math.Max(1d, record.Radius);
            var minX = (int)Math.Floor(center.X - radius);
            var maxX = (int)Math.Ceiling(center.X + radius);
            var minY = (int)Math.Floor(center.Y) - 1;
            var maxY = (int)Math.Floor(center.Y) + 1;
            var minZ = (int)Math.Floor(center.Z - radius);
            var maxZ = (int)Math.Ceiling(center.Z + radius);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    for (var z = minZ; z <= maxZ; z++)
                    {
                        var pos = new BlockPos(x, y, z);
                        if (new Vec3d(x + 0.5, y + 0.5, z + 0.5).DistanceTo(center) > radius)
                        {
                            continue;
                        }

                        var blockEntity = sapi.World.BlockAccessor.GetBlockEntity(pos);
                        if (blockEntity == null)
                        {
                            continue;
                        }

                        if (TryAdjustNumericMember(blockEntity, delta, memberNames))
                        {
                            TryMarkBlockEntityDirty(blockEntity);
                        }
                    }
                }
            }
        }

        public bool TryRegisterActiveEffect(RustweaveActiveEffectRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.EffectId) || string.IsNullOrWhiteSpace(record.EffectType))
            {
                return false;
            }

            activeEffectRecords[record.EffectId] = record.Clone();
            activeEffectRegistryDirty = true;
            return true;
        }

        public bool TryRemoveActiveEffect(string effectId, bool restoreBlocks)
        {
            if (string.IsNullOrWhiteSpace(effectId) || !activeEffectRecords.TryGetValue(effectId, out var record))
            {
                return false;
            }

            if (restoreBlocks && record.BlockSnapshots.Count > 0)
            {
                RestoreBlockSnapshots(record);
            }

            if (string.Equals(record.RecordKind, "summon", StringComparison.OrdinalIgnoreCase)
                || string.Equals(record.RecordKind, "projectile", StringComparison.OrdinalIgnoreCase)
                || string.Equals(record.RecordKind, "item", StringComparison.OrdinalIgnoreCase)
                || string.Equals(record.RecordKind, "construct", StringComparison.OrdinalIgnoreCase))
            {
                CleanupSummonedRecord(record);
            }
            activeEffectRecords.Remove(effectId);
            activeEffectRegistryDirty = true;
            return true;
        }

        private void CleanupSummonedRecord(RustweaveActiveEffectRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (string.Equals(record.RecordKind, "item", StringComparison.OrdinalIgnoreCase))
            {
                RemoveTemporaryItemFromCaster(record.CasterPlayerUid, record.ItemCode);
            }

            foreach (var entityId in record.AffectedEntityIds.Concat(record.TargetEntityId > 0 ? new[] { record.TargetEntityId } : Array.Empty<long>()).Where(entityId => entityId > 0).Distinct())
            {
                var entity = sapi.World.GetEntityById(entityId);
                if (entity == null)
                {
                    continue;
                }

                TryDespawnEntity(entity);
            }
        }

        private void RemoveTemporaryItemFromCaster(string casterPlayerUid, string itemCode)
        {
            if (string.IsNullOrWhiteSpace(casterPlayerUid) || string.IsNullOrWhiteSpace(itemCode))
            {
                return;
            }

            var player = GetOnlineServerPlayer(casterPlayerUid);
            if (player?.InventoryManager?.InventoriesOrdered == null)
            {
                return;
            }

            foreach (var inventory in player.InventoryManager.InventoriesOrdered)
            {
                for (var slotIndex = 0; slotIndex < inventory.Count; slotIndex++)
                {
                    var slot = inventory[slotIndex];
                    var stack = slot?.Itemstack;
                    if (stack?.Collectible?.Code?.Path == null)
                    {
                        continue;
                    }

                    if (!string.Equals(stack.Collectible.Code.Path, itemCode.Split(':').LastOrDefault() ?? itemCode, StringComparison.OrdinalIgnoreCase) && !string.Equals(stack.Collectible.Code.ToString(), itemCode, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    slot.Itemstack = null;
                    slot.MarkDirty();
                    inventory.MarkSlotDirty(slotIndex);
                    return;
                }
            }
        }

        private static void TryDespawnEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            try
            {
                var despawnMethod = entity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate => string.Equals(candidate.Name, "Despawn", StringComparison.OrdinalIgnoreCase));
                if (despawnMethod != null && despawnMethod.GetParameters().Length == 0)
                {
                    despawnMethod.Invoke(entity, Array.Empty<object>());
                    return;
                }

                var dieMethod = entity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate => string.Equals(candidate.Name, "Die", StringComparison.OrdinalIgnoreCase));
                if (dieMethod != null)
                {
                    var parameters = dieMethod.GetParameters();
                    var args = new object?[parameters.Length];
                    for (var index = 0; index < parameters.Length; index++)
                    {
                        args[index] = parameters[index].ParameterType.IsValueType ? Activator.CreateInstance(parameters[index].ParameterType) : null;
                    }

                    dieMethod.Invoke(entity, args);
                }
            }
            catch
            {
            }
        }

        public IReadOnlyList<RustweaveActiveEffectRecord> GetActiveEffectsForEntity(long entityId)
        {
            if (entityId <= 0 || activeEffectRecords.Count == 0)
            {
                return Array.Empty<RustweaveActiveEffectRecord>();
            }

            return activeEffectRecords.Values.Where(record => record.TargetEntityId == entityId || record.AffectedEntityIds.Contains(entityId)).Select(record => record.Clone()).ToList();
        }

        public IReadOnlyList<RustweaveActiveEffectRecord> GetActiveEffectsNear(Vec3d position, double radius)
        {
            if (radius <= 0d || activeEffectRecords.Count == 0)
            {
                return Array.Empty<RustweaveActiveEffectRecord>();
            }

            return activeEffectRecords.Values
                .Where(record => record.IsArea && new Vec3d(record.CenterX, record.CenterY, record.CenterZ).DistanceTo(position) <= radius + Math.Max(0d, record.Radius))
                .Select(record => record.Clone())
                .ToList();
        }

        public bool TryGetHistoricalPosition(long entityId, int stepsBack, out Vec3d position)
        {
            position = new Vec3d();
            if (entityId <= 0 || stepsBack < 0 || !entityHistory.TryGetValue(entityId, out var samples) || samples.Count == 0)
            {
                return false;
            }

            var sample = samples.Last;
            for (var index = 0; index < stepsBack && sample?.Previous != null; index++)
            {
                sample = sample.Previous;
            }

            if (sample?.Value == null)
            {
                return false;
            }

            position = new Vec3d(sample.Value.X, sample.Value.Y, sample.Value.Z);
            return true;
        }

        public bool TryGetCounterTargetEffects(long targetEntityId, out IReadOnlyList<RustweaveActiveEffectRecord> effects)
        {
            effects = GetActiveEffectsForEntity(targetEntityId);
            return effects.Count > 0;
        }

        private void RestoreBlockSnapshots(RustweaveActiveEffectRecord record)
        {
            if (record.BlockSnapshots == null || record.BlockSnapshots.Count == 0)
            {
                return;
            }

            foreach (var snapshot in record.BlockSnapshots)
            {
                try
                {
                    var pos = new BlockPos(snapshot.X, snapshot.Y, snapshot.Z, snapshot.Dimension);
                    if (!CanModifyBlockAt(pos, record.CasterPlayerUid))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(snapshot.BlockCode))
                    {
                        var block = sapi.World.BlockAccessor.GetBlock(new AssetLocation(snapshot.BlockCode));
                        if (block != null)
                        {
                            sapi.World.BlockAccessor.SetBlock(block.BlockId, pos);
                            continue;
                        }
                    }

                    if (snapshot.BlockId > 0)
                    {
                        sapi.World.BlockAccessor.SetBlock(snapshot.BlockId, pos);
                    }
                }
                catch (Exception exception)
                {
                    sapi.Logger.Warning("[TheRustweave] Failed to restore block snapshot for effect '{0}': {1}", record.EffectType, exception.Message);
                }
            }
        }

        public bool CanModifyBlockAt(BlockPos pos, string casterPlayerUid)
        {
            if (pos == null)
            {
                return false;
            }

            try
            {
                var worldType = sapi.World.GetType();
                var claimsProperty = worldType.GetProperty("Claims", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var claims = claimsProperty?.GetValue(sapi.World);
                if (claims == null)
                {
                    return true;
                }

                var player = GetOnlineServerPlayer(casterPlayerUid);
                if (player == null)
                {
                    return false;
                }

                foreach (var method in claims.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!string.Equals(method.Name, "TryAccess", StringComparison.OrdinalIgnoreCase) && !string.Equals(method.Name, "TryAccessBlock", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    try
                    {
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(BlockPos))
                        {
                            var result = method.Invoke(claims, new object[] { pos, player });
                            if (result is bool allowed)
                            {
                                return allowed;
                            }
                        }
                        else if (parameters.Length == 3 && parameters[0].ParameterType == typeof(IWorldAccessor))
                        {
                            var result = method.Invoke(claims, new object[] { sapi.World, player, pos });
                            if (result is bool allowed)
                            {
                                return allowed;
                            }
                        }
                    }
                    catch
                    {
                        // fall back below
                    }
                }

                var center = new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5);
                var blockingEffects = GetActiveEffectsNear(center, 2d);
                if (blockingEffects.Any(record => record != null && record.IsBlocking && (string.Equals(record.EffectType, SpellEffectTypes.AnchorBlock, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateWardArea, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateBarrier, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateContainmentArea, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateBoundaryLine, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateAntiSpreadArea, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.CreateRift, StringComparison.OrdinalIgnoreCase) || string.Equals(record.EffectType, SpellEffectTypes.OpenPassage, StringComparison.OrdinalIgnoreCase))))
                {
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }

        public bool TryFindNearestSafeTeleportPosition(IWorldAccessor world, Entity entity, Vec3d desired, out Vec3d safePosition)
        {
            safePosition = entity?.Pos?.XYZ ?? desired;
            if (world == null || entity == null)
            {
                return false;
            }

            var dim = entity.Pos.Dimension;
            var baseY = Math.Floor(desired.Y);
            var offsets = new List<(int X, int Z)>();
            for (var radius = 0; radius <= 3; radius++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        if (Math.Abs(x) != radius && Math.Abs(z) != radius && radius > 0)
                        {
                            continue;
                        }

                        offsets.Add((x, z));
                    }
                }
            }

            foreach (var offset in offsets)
            {
                for (var vertical = 0; vertical <= 3; vertical++)
                {
                    var candidate = new Vec3d(desired.X + offset.X, baseY + vertical, desired.Z + offset.Z);
                    if (IsTeleportPositionSafe(world, entity, candidate, dim))
                    {
                        safePosition = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsTeleportPositionSafe(IWorldAccessor world, Entity entity, Vec3d candidate, int expectedDimension)
        {
            if (world == null || entity == null)
            {
                return false;
            }

            if (entity.Pos.Dimension != expectedDimension)
            {
                return false;
            }

            var feetPos = new BlockPos((int)Math.Floor(candidate.X), (int)Math.Floor(candidate.Y), (int)Math.Floor(candidate.Z), expectedDimension);
            var headPos = new BlockPos(feetPos.X, feetPos.Y + 1, feetPos.Z, expectedDimension);
            var supportPos = new BlockPos(feetPos.X, feetPos.Y - 1, feetPos.Z, expectedDimension);

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

        private void ProcessTabletVenting(IServerPlayer serverPlayer, RustweavePlayerStateData state)
        {
            if (!activeTabletVents.TryGetValue(serverPlayer.PlayerUID, out var ventSession))
            {
                return;
            }

            if (!RustweaveStateService.IsHoldingTablet(serverPlayer))
            {
                activeTabletVents.Remove(serverPlayer.PlayerUID);
                return;
            }

            var tablet = serverPlayer.InventoryManager?.ActiveHotbarSlot?.Itemstack;
            if (tablet == null)
            {
                activeTabletVents.Remove(serverPlayer.PlayerUID);
                return;
            }

            RustweaveStateService.ApplyTabletPassiveDecay(serverPlayer.Entity?.World ?? sapi.World, serverPlayer.InventoryManager?.ActiveHotbarSlot);

            if (state.CurrentTemporalCorruption > state.EffectiveTemporalCorruptionThreshold || state.CurrentTemporalCorruption <= 0)
            {
                activeTabletVents.Remove(serverPlayer.PlayerUID);
                return;
            }

            var storedCorruption = RustweaveStateService.GetTabletStoredCorruption(tablet);
            if (storedCorruption >= RustweaveConstants.TabletCapacity)
            {
                activeTabletVents.Remove(serverPlayer.PlayerUID);
                return;
            }

            var elapsedMilliseconds = sapi.World.ElapsedMilliseconds - ventSession.StartedAtMilliseconds;
            var desiredTransfer = (int)Math.Floor((elapsedMilliseconds / 1000d) * RustweaveConstants.TabletVentingRatePerSecond);
            var transfer = Math.Min(
                desiredTransfer - ventSession.TransferredCorruption,
                Math.Min(state.CurrentTemporalCorruption, RustweaveConstants.TabletCapacity - storedCorruption));

            if (transfer > 0)
            {
                state.CurrentTemporalCorruption -= transfer;
                storedCorruption += transfer;
                ventSession.TransferredCorruption += transfer;
                RustweaveStateService.SetTabletStoredCorruption(tablet, storedCorruption, serverPlayer.Entity?.World ?? sapi.World);
                serverPlayer.InventoryManager?.ActiveHotbarSlot?.MarkDirty();
                SaveAndSyncState(serverPlayer, state);
            }

            if (state.CurrentTemporalCorruption <= 0 || storedCorruption >= RustweaveConstants.TabletCapacity)
            {
                activeTabletVents.Remove(serverPlayer.PlayerUID);
                return;
            }
        }

        private void ProcessMentorStudies(IServerPlayer serverPlayer)
        {
            var state = GetState(serverPlayer);
            ProcessMentorStudies(serverPlayer, state);
        }

        private void ProcessMentorStudies(IServerPlayer serverPlayer, RustweavePlayerStateData state)
        {
            if (serverPlayer == null || state == null || !RustweaveStateService.IsRustweaver(serverPlayer))
            {
                return;
            }

            RustweaveMentorService.NormalizeMentorStudies(state);
            if (state.MentorStudies.Count == 0)
            {
                return;
            }

            var changed = false;
            foreach (var study in state.MentorStudies.ToList())
            {
                if (study == null || string.IsNullOrWhiteSpace(study.SpellCode))
                {
                    state.MentorStudies.Remove(study!);
                    changed = true;
                    continue;
                }

                if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(study.SpellCode, out var spell) || spell == null || RustweaveStateService.IsHiddenSpell(spell) || RustweaveStateService.IsLoreweaveSpell(spell))
                {
                    state.MentorStudies.Remove(study!);
                    changed = true;
                    sapi.Logger.Warning("[TheRustweave] Mentor study for player '{0}' was cleared because spell '{1}' became invalid.", serverPlayer.PlayerUID, study.SpellCode);
                    serverPlayer.SendMessage(0, Lang.Get("game:rustweave-mentor-invalid"), EnumChatType.Notification, null);
                    continue;
                }

                if (RustweaveStateService.IsSpellLearned(serverPlayer, study.SpellCode))
                {
                    state.MentorStudies.Remove(study!);
                    changed = true;
                    sapi.Logger.Debug("[TheRustweave] Mentor study for player '{0}' was cleared because spell '{1}' was already learned from another source.", serverPlayer.PlayerUID, study.SpellCode);
                    continue;
                }

                var currentHours = GetWorldHours(serverPlayer);
                if (currentHours < study.FinishTime)
                {
                    continue;
                }

                if (RustweaveStateService.LearnSpell(state, study.SpellCode, RustweaveMentorTypes.RustplaneSageStudySource, out var failureReason))
                {
                    state.MentorStudies.Remove(study!);
                    changed = true;
                    sapi.Logger.Debug("[TheRustweave] Mentor study completed for player '{0}' on spell '{1}'.", serverPlayer.PlayerUID, study.SpellCode);
                    serverPlayer.SendMessage(0, Lang.Get("game:rustweave-mentor-complete", RustweaveStateService.GetSpellDisplayName(study.SpellCode)), EnumChatType.Notification, null);
                    continue;
                }

                state.MentorStudies.Remove(study!);
                changed = true;
                sapi.Logger.Warning("[TheRustweave] Mentor study for player '{0}' failed to complete spell '{1}': {2}", serverPlayer.PlayerUID, study.SpellCode, failureReason);
                if (!string.IsNullOrWhiteSpace(failureReason))
                {
                    serverPlayer.SendMessage(0, Lang.Get("game:rustweave-mentor-invalid"), EnumChatType.Notification, null);
                }
            }

            if (changed)
            {
                SaveAndSyncState(serverPlayer, state);
            }
        }

        private void TriggerOverloadExplosion(IServerPlayer serverPlayer)
        {
            activeCasts.Remove(serverPlayer.PlayerUID);
            activeTabletVents.Remove(serverPlayer.PlayerUID);
            RemoveActiveEffectsForPlayer(serverPlayer.PlayerUID);

            if (serverPlayer.Entity == null || !serverPlayer.Entity.Alive)
            {
                return;
            }

            var explosionPos = serverPlayer.Entity.Pos.AsBlockPos;
            sapi.World.CreateExplosion(explosionPos, EnumBlastType.EntityBlast, 0, 5, 0f, serverPlayer.PlayerUID);
            serverPlayer.SendMessage(0, Lang.Get("game:rustweave-overload-detonated"), EnumChatType.Notification, null);

            var damageSource = new DamageSource
            {
                Source = EnumDamageSource.Explosion,
                Type = EnumDamageType.Crushing,
                SourceEntity = serverPlayer.Entity,
                CauseEntity = serverPlayer.Entity,
                DamageTier = 3,
                KnockbackStrength = 2f
            };

            serverPlayer.Entity.Die(EnumDespawnReason.Death, damageSource);
        }

        private RustweavePlayerStateData GetState(IServerPlayer player)
        {
            var state = RustweaveStateService.LoadServerState(player);
            state = RustweaveStateService.NormalizeState(state);
            RustweaveStateService.UpdateOverloadState(state, sapi.World.ElapsedMilliseconds);
            RustweaveStateService.CacheServerState(player, state);
            return state;
        }

        private void SaveAndSyncState(IServerPlayer player, RustweavePlayerStateData state)
        {
            state = RustweaveStateService.NormalizeState(state);
            RustweaveStateService.UpdateOverloadState(state, sapi.World.ElapsedMilliseconds);
            RustweaveStateService.CacheServerState(player, state);
            RustweaveStateService.SaveServerState(player, state);
        }

        private void ProcessTabletDecayForInventories(IServerPlayer player)
        {
            if (player?.InventoryManager?.InventoriesOrdered == null)
            {
                return;
            }

            foreach (var inventory in player.InventoryManager.InventoriesOrdered)
            {
                if (!CanProcessTabletDecayInventory(inventory))
                {
                    continue;
                }

                ProcessTabletDecayForInventory(inventory, player.Entity?.World ?? sapi.World);
            }
        }

        private void ProcessPassiveTabletDecay()
        {
            if (sapi.World.ElapsedMilliseconds < nextTabletDecayScanMilliseconds)
            {
                return;
            }

            nextTabletDecayScanMilliseconds = sapi.World.ElapsedMilliseconds + 1000;
            var processedPendingPlayers = ProcessPendingTabletDecayScans();

            foreach (var onlinePlayer in sapi.World.AllOnlinePlayers.OfType<IServerPlayer>())
            {
                if (processedPendingPlayers.Contains(onlinePlayer.PlayerUID) || ShouldDeferTabletDecayScan(onlinePlayer.PlayerUID))
                {
                    continue;
                }

                ProcessTabletDecayForInventories(onlinePlayer);
            }

            foreach (var chunkIndex in sapi.World.LoadedChunkIndices)
            {
                var chunk = sapi.World.BlockAccessor.GetChunk(chunkIndex);
                if (chunk?.BlockEntities == null)
                {
                    continue;
                }

                foreach (var blockEntity in chunk.BlockEntities.Values)
                {
                    if (blockEntity is not IBlockEntityContainer container || container.Inventory == null)
                    {
                        continue;
                    }

                    ProcessTabletDecayForInventory(container.Inventory, sapi.World);
                }
            }
        }

        private void ProcessTabletDecayForInventory(IInventory? inventory, IWorldAccessor? world)
        {
            if (inventory == null || world?.Calendar == null || !CanProcessTabletDecayInventory(inventory))
            {
                return;
            }

            if (!TryGetInventoryCount(inventory, out var inventoryCount))
            {
                return;
            }

            for (var slotIndex = 0; slotIndex < inventoryCount; slotIndex++)
            {
                var slot = inventory[slotIndex];
                if (slot?.Itemstack == null)
                {
                    continue;
                }

                if (RustweaveStateService.ApplyTabletPassiveDecay(world, slot))
                {
                    inventory.MarkSlotDirty(slotIndex);
                }
            }
        }

        public bool TryRegisterTimedStatModifier(Entity targetEntity, string statCategory, string modifierCode, float delta, long durationMilliseconds, string spellCode, string effectType)
        {
            if (targetEntity == null || !targetEntity.Alive || string.IsNullOrWhiteSpace(statCategory) || string.IsNullOrWhiteSpace(modifierCode) || durationMilliseconds <= 0)
            {
                return false;
            }

            if (targetEntity.Stats == null)
            {
                sapi.Logger.Warning("[TheRustweave] Timed stat modifier '{0}' failed because entity '{1}' has no stats.", effectType, targetEntity.EntityId);
                return false;
            }

            var expiresAtMilliseconds = sapi.World.ElapsedMilliseconds + durationMilliseconds;
            targetEntity.Stats.Set(statCategory, modifierCode, delta, false);
            activeTimedStatModifiers[modifierCode] = new RustweaveTimedStatModifier
            {
                TargetEntityId = targetEntity.EntityId,
                StatCategory = statCategory,
                ModifierCode = modifierCode,
                Delta = delta,
                ExpiresAtMilliseconds = expiresAtMilliseconds,
                SpellCode = spellCode,
                EffectType = effectType
            };

            sapi.Logger.Debug("[TheRustweave] Timed stat modifier '{0}' applied to entity '{1}' for {2} ms.", effectType, targetEntity.EntityId, durationMilliseconds);
            return true;
        }

        public bool TryRegisterDamageOverTime(Entity targetEntity, Entity? sourceEntity, string effectCode, float damagePerTick, long tickIntervalMilliseconds, long durationMilliseconds, string spellCode, string effectType)
        {
            if (targetEntity == null || !targetEntity.Alive || string.IsNullOrWhiteSpace(effectCode) || damagePerTick <= 0 || tickIntervalMilliseconds <= 0 || durationMilliseconds <= 0)
            {
                return false;
            }

            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            activeDamageOverTimeEffects[effectCode] = new RustweaveTimedDamageOverTime
            {
                TargetEntityId = targetEntity.EntityId,
                SourceEntityId = sourceEntity?.EntityId ?? 0,
                EffectCode = effectCode,
                DamagePerTick = damagePerTick,
                TickIntervalMilliseconds = tickIntervalMilliseconds,
                NextTickAtMilliseconds = nowMilliseconds + tickIntervalMilliseconds,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                SpellCode = spellCode,
                EffectType = effectType
            };

            sapi.Logger.Debug("[TheRustweave] Damage-over-time '{0}' applied to entity '{1}' for {2} ms.", effectType, targetEntity.EntityId, durationMilliseconds);
            return true;
        }

        public bool TryRegisterShield(Entity targetEntity, string shieldCode, float shieldAmount, long durationMilliseconds, string spellCode, string effectType)
        {
            if (targetEntity == null || !targetEntity.Alive || string.IsNullOrWhiteSpace(shieldCode) || shieldAmount <= 0 || durationMilliseconds <= 0)
            {
                return false;
            }

            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            activeSpellShields[shieldCode] = new RustweaveTimedShield
            {
                TargetEntityId = targetEntity.EntityId,
                ShieldCode = shieldCode,
                RemainingAmount = shieldAmount,
                ExpiresAtMilliseconds = nowMilliseconds + durationMilliseconds,
                SpellCode = spellCode,
                EffectType = effectType
            };

            sapi.Logger.Debug("[TheRustweave] Shield '{0}' applied to entity '{1}' for {2} ms with {3} remaining.", effectType, targetEntity.EntityId, durationMilliseconds, shieldAmount);
            return true;
        }

        public bool TryApplySpellDamage(Entity targetEntity, DamageSource damageSource, float damageAmount, string spellCode, string effectType)
        {
            if (targetEntity == null || !targetEntity.Alive || damageAmount <= 0)
            {
                return false;
            }

            var remainingDamage = damageAmount;
            var absorbedDamage = TryAbsorbShieldDamage(targetEntity, ref remainingDamage);
            if (absorbedDamage > 0)
            {
                sapi.Logger.Debug("[TheRustweave] Shield absorbed {0} damage from spell '{1}' on entity '{2}'.", absorbedDamage, spellCode, targetEntity.EntityId);
            }

            if (remainingDamage <= 0)
            {
                return true;
            }

            if (!targetEntity.ShouldReceiveDamage(damageSource, remainingDamage))
            {
                return false;
            }

            targetEntity.ReceiveDamage(damageSource, remainingDamage);
            sapi.Logger.Debug("[TheRustweave] Spell '{0}' applied {1} damage to entity '{2}' via {3}.", spellCode, remainingDamage, targetEntity.EntityId, effectType);
            return true;
        }

        public bool TryTransferCorruption(IServerPlayer sourcePlayer, IServerPlayer? targetPlayer, int amount, string spellCode, string effectType)
        {
            if (sourcePlayer == null || sourcePlayer.Entity == null || amount <= 0)
            {
                return false;
            }

            var sourceState = GetState(sourcePlayer);
            var sourceTransfer = Math.Min(amount, sourceState.CurrentTemporalCorruption);
            if (sourceTransfer <= 0)
            {
                sapi.Logger.Debug("[TheRustweave] Corruption transfer '{0}' for spell '{1}' had no source corruption to move for player '{2}'.", effectType, spellCode, sourcePlayer.PlayerUID);
                return true;
            }

            var transferred = sourceTransfer;
            if (targetPlayer != null && targetPlayer.Entity != null && !string.Equals(targetPlayer.PlayerUID, sourcePlayer.PlayerUID, StringComparison.OrdinalIgnoreCase) && RustweaveStateService.IsRustweaver(targetPlayer))
            {
                var targetState = GetState(targetPlayer);
                var targetCapacity = Math.Max(0, targetState.EffectiveTemporalCorruptionThreshold - targetState.CurrentTemporalCorruption);
                transferred = Math.Min(transferred, targetCapacity);
                if (transferred > 0)
                {
                    targetState.CurrentTemporalCorruption = Math.Min(targetState.EffectiveTemporalCorruptionThreshold, targetState.CurrentTemporalCorruption + transferred);
                    SaveAndSyncState(targetPlayer, targetState);
                }
            }

            if (transferred <= 0)
            {
                sapi.Logger.Debug("[TheRustweave] Corruption transfer '{0}' for spell '{1}' had no target capacity for player '{2}'.", effectType, spellCode, sourcePlayer.PlayerUID);
                return true;
            }

            sourceState.CurrentTemporalCorruption = Math.Max(0, sourceState.CurrentTemporalCorruption - transferred);
            SaveAndSyncState(sourcePlayer, sourceState);
            sapi.Logger.Debug("[TheRustweave] Corruption transfer '{0}' moved {1} corruption for spell '{2}' from '{3}'.", effectType, transferred, spellCode, sourcePlayer.PlayerUID);
            return true;
        }

        private void ProcessTimedSpellEffects()
        {
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;

            foreach (var entry in activeTimedStatModifiers.ToArray())
            {
                var effect = entry.Value;
                var entity = sapi.World.GetEntityById(effect.TargetEntityId);
                if (entity == null || !entity.Alive || nowMilliseconds >= effect.ExpiresAtMilliseconds)
                {
                    if (entity?.Stats != null)
                    {
                        entity.Stats.Remove(effect.StatCategory, effect.ModifierCode);
                    }

                    activeTimedStatModifiers.Remove(entry.Key);
                    sapi.Logger.Debug("[TheRustweave] Timed stat modifier '{0}' expired for entity '{1}'.", effect.EffectType, effect.TargetEntityId);
                    continue;
                }
            }

            foreach (var entry in activeDamageOverTimeEffects.ToArray())
            {
                var effect = entry.Value;
                var entity = sapi.World.GetEntityById(effect.TargetEntityId);
                if (entity == null || !entity.Alive)
                {
                    activeDamageOverTimeEffects.Remove(entry.Key);
                    sapi.Logger.Debug("[TheRustweave] Damage-over-time '{0}' ended because entity '{1}' is unavailable.", effect.EffectType, effect.TargetEntityId);
                    continue;
                }

                if (nowMilliseconds >= effect.ExpiresAtMilliseconds)
                {
                    activeDamageOverTimeEffects.Remove(entry.Key);
                    sapi.Logger.Debug("[TheRustweave] Damage-over-time '{0}' expired for entity '{1}'.", effect.EffectType, effect.TargetEntityId);
                    continue;
                }

                while (nowMilliseconds >= effect.NextTickAtMilliseconds && effect.NextTickAtMilliseconds < effect.ExpiresAtMilliseconds)
                {
                    var sourceEntity = effect.SourceEntityId > 0 ? sapi.World.GetEntityById(effect.SourceEntityId) : null;
                    var damageSource = new DamageSource
                    {
                        Source = EnumDamageSource.Internal,
                        SourceEntity = sourceEntity ?? entity,
                        CauseEntity = sourceEntity ?? entity,
                        KnockbackStrength = 0f
                    };

                    if (!entity.ShouldReceiveDamage(damageSource, effect.DamagePerTick))
                    {
                        break;
                    }

                    entity.ReceiveDamage(damageSource, effect.DamagePerTick);
                    effect.NextTickAtMilliseconds += effect.TickIntervalMilliseconds;
                    sapi.Logger.Debug("[TheRustweave] Damage-over-time '{0}' ticked entity '{1}' for {2} damage.", effect.EffectType, effect.TargetEntityId, effect.DamagePerTick);
                }

                activeDamageOverTimeEffects[entry.Key] = effect;
            }

            foreach (var entry in activeSpellShields.ToArray())
            {
                var shield = entry.Value;
                var entity = sapi.World.GetEntityById(shield.TargetEntityId);
                if (entity == null || !entity.Alive || nowMilliseconds >= shield.ExpiresAtMilliseconds || shield.RemainingAmount <= 0)
                {
                    activeSpellShields.Remove(entry.Key);
                    sapi.Logger.Debug("[TheRustweave] Shield '{0}' expired for entity '{1}'.", shield.EffectType, shield.TargetEntityId);
                }
            }
        }

        private float TryAbsorbShieldDamage(Entity targetEntity, ref float remainingDamage)
        {
            if (targetEntity == null || !targetEntity.Alive || remainingDamage <= 0)
            {
                return 0f;
            }

            var absorbedTotal = 0f;
            foreach (var entry in activeSpellShields.Where(pair => pair.Value.TargetEntityId == targetEntity.EntityId).OrderBy(pair => pair.Value.ExpiresAtMilliseconds).ToArray())
            {
                var shield = entry.Value;
                if (shield.RemainingAmount <= 0)
                {
                    activeSpellShields.Remove(entry.Key);
                    continue;
                }

                var absorbed = Math.Min(shield.RemainingAmount, remainingDamage);
                if (absorbed <= 0)
                {
                    continue;
                }

                shield.RemainingAmount -= absorbed;
                remainingDamage -= absorbed;
                absorbedTotal += absorbed;

                if (shield.RemainingAmount <= 0)
                {
                    activeSpellShields.Remove(entry.Key);
                }
                else
                {
                    activeSpellShields[entry.Key] = shield;
                }

                if (remainingDamage <= 0)
                {
                    break;
                }
            }

            return absorbedTotal;
        }

        private void ScheduleTabletDecayScan(IServerPlayer player, int delayMilliseconds)
        {
            if (player == null)
            {
                return;
            }

            pendingTabletDecayScans[player.PlayerUID] = sapi.World.ElapsedMilliseconds + Math.Max(1000, delayMilliseconds);
        }

        private HashSet<string> ProcessPendingTabletDecayScans()
        {
            var processedPlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nowMilliseconds = sapi.World.ElapsedMilliseconds;

            foreach (var entry in pendingTabletDecayScans.ToArray())
            {
                if (nowMilliseconds < entry.Value)
                {
                    continue;
                }

                pendingTabletDecayScans.Remove(entry.Key);

                var player = GetOnlineServerPlayer(entry.Key);
                if (player == null || player.Entity == null || !player.Entity.Alive || !RustweaveStateService.IsRustweaver(player))
                {
                    continue;
                }

                ProcessTabletDecayForInventories(player);
                processedPlayers.Add(entry.Key);
            }

            return processedPlayers;
        }

        private bool ShouldDeferTabletDecayScan(string playerUid)
        {
            return pendingTabletDecayScans.TryGetValue(playerUid, out var dueMilliseconds)
                && sapi.World.ElapsedMilliseconds < dueMilliseconds;
        }

        private bool TryGetInventoryCount(IInventory inventory, out int count)
        {
            try
            {
                count = inventory.Count;
                return true;
            }
            catch (Exception exception)
            {
                count = 0;
                LogSkippedTabletDecayInventory(inventory, $"count access failed: {exception.GetType().Name}");
                return false;
            }
        }

        private bool CanProcessTabletDecayInventory(IInventory? inventory)
        {
            if (inventory == null)
            {
                return false;
            }

            if (IsCreativeInventory(inventory))
            {
                LogSkippedTabletDecayInventory(inventory, "creative inventory");
                return false;
            }

            return true;
        }

        private static bool IsCreativeInventory(IInventory inventory)
        {
            var typeName = inventory.GetType().Name;
            if (typeName.IndexOf("Creative", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            var inventoryId = GetInventoryMetadataValue(inventory, "InventoryID");
            if (!string.IsNullOrWhiteSpace(inventoryId) && inventoryId.IndexOf("creative", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            var className = GetInventoryMetadataValue(inventory, "ClassName");
            if (!string.IsNullOrWhiteSpace(className) && className.IndexOf("creative", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static string? GetInventoryMetadataValue(IInventory inventory, string memberName)
        {
            var property = inventory.GetType().GetProperty(memberName);
            if (property?.PropertyType == typeof(string))
            {
                return property.GetValue(inventory) as string;
            }

            return null;
        }

        private void LogSkippedTabletDecayInventory(IInventory inventory, string reason)
        {
            var inventoryKey = $"{inventory.GetType().FullName}|{GetInventoryMetadataValue(inventory, "InventoryID") ?? string.Empty}|{GetInventoryMetadataValue(inventory, "ClassName") ?? string.Empty}|{reason}";
            if (!skippedTabletDecayInventoryLogs.Add(inventoryKey))
            {
                return;
            }

            sapi.Logger.Warning($"[TheRustweave] Skipping tablet decay inventory {inventoryKey}");
        }

        private IServerPlayer? GetOnlineServerPlayer(string playerUid)
        {
            return sapi.World.AllOnlinePlayers.OfType<IServerPlayer>().FirstOrDefault(player => string.Equals(player.PlayerUID, playerUid, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryGetCooldownRemaining(string playerUid, string spellCode, out long remainingMilliseconds)
        {
            remainingMilliseconds = 0;
            if (string.IsNullOrWhiteSpace(playerUid) || string.IsNullOrWhiteSpace(spellCode))
            {
                return false;
            }

            if (!spellCooldowns.TryGetValue(playerUid, out var cooldowns) || !cooldowns.TryGetValue(spellCode, out var expiresAtMilliseconds))
            {
                return false;
            }

            var nowMilliseconds = sapi.World.ElapsedMilliseconds;
            if (nowMilliseconds >= expiresAtMilliseconds)
            {
                cooldowns.Remove(spellCode);
                if (cooldowns.Count == 0)
                {
                    spellCooldowns.Remove(playerUid);
                }

                return false;
            }

            remainingMilliseconds = expiresAtMilliseconds - nowMilliseconds;
            return true;
        }

        private void SetSpellCooldown(string playerUid, SpellDefinition spell)
        {
            if (string.IsNullOrWhiteSpace(playerUid) || spell == null || spell.CooldownSeconds <= 0)
            {
                return;
            }

            var expiresAtMilliseconds = sapi.World.ElapsedMilliseconds + (long)Math.Round(spell.CooldownSeconds * 1000d);
            if (!spellCooldowns.TryGetValue(playerUid, out var cooldowns))
            {
                cooldowns = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                spellCooldowns[playerUid] = cooldowns;
            }

            cooldowns[spell.Code] = expiresAtMilliseconds;
        }

        private void SendSpellTargetFailure(IServerPlayer player, string failureReason)
        {
            if (string.Equals(failureReason, "No damaged item to mend.", StringComparison.Ordinal))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-damaged-item-to-mend"), EnumChatType.Notification, null);
                return;
            }

            if (string.Equals(failureReason, "No target struck.", StringComparison.Ordinal))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-target-struck"), EnumChatType.Notification, null);
                return;
            }

            player.SendMessage(0, Lang.Get("game:rustweave-spell-target-fail"), EnumChatType.Notification, null);
        }

        private bool TryCanContinueCast(IServerPlayer player, RustweaveCastStateData castState, out string cancelMessage)
        {
            cancelMessage = Lang.Get("game:rustweave-cast-cancel");
            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(castState.SpellCode, out var spell) || spell == null)
            {
                cancelMessage = Lang.Get("game:rustweave-cast-fail");
                return false;
            }

            if (spell.RequiresTome && !RustweaveStateService.IsHoldingTome(player))
            {
                return false;
            }

            return true;
        }

        private void StartCast(IServerPlayer player, RustweavePlayerStateData state, int slotIndex)
        {
            if (state.CurrentTemporalCorruption >= state.EffectiveTemporalCorruptionThreshold)
            {
                player.SendMessage(0, Lang.Get("game:rustweave-cast-locked"), EnumChatType.Notification, null);
                return;
            }

            if (!RustweaveStateService.IsValidSlotIndex(slotIndex))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-spell-prepared"), EnumChatType.Notification, null);
                return;
            }

            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            sapi.Logger.Debug("[TheRustweave] Cast attempted from active prepared slot {0} (internal index {1}) for player '{2}' (spell '{3}').", RustweaveStateService.ToDisplaySlotNumber(slotIndex), slotIndex, player.PlayerUID, spellCode);
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-spell-in-slot", RustweaveStateService.ToDisplaySlotNumber(slotIndex)), EnumChatType.Notification, null);
                return;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out var spell) || spell == null)
            {
                sapi.Logger.Warning("[TheRustweave] Cast rejected for player '{0}' because spell '{1}' is missing or disabled.", player.PlayerUID, spellCode);
                RustweaveStateService.TryUnprepareSpell(state, slotIndex);
                SaveAndSyncState(player, state);
                player.SendMessage(0, Lang.Get("game:rustweave-cast-fail"), EnumChatType.Notification, null);
                return;
            }

            if (!RustweaveStateService.IsSpellLearned(spell.Code, state))
            {
                sapi.Logger.Warning("[TheRustweave] Cast rejected for player '{0}' because spell '{1}' is no longer learned.", player.PlayerUID, spell.Code);
                RustweaveStateService.TryUnprepareSpell(state, slotIndex);
                SaveAndSyncState(player, state);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-unlearned"), EnumChatType.Notification, null);
                return;
            }

            if (TryGetCooldownRemaining(player.PlayerUID, spell.Code, out var remainingMilliseconds))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' was rejected for player '{1}' because it is on cooldown for {2} ms.", spell.Code, player.PlayerUID, remainingMilliseconds);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-cooldown", spell.Name, RustweaveStateService.FormatSeconds(remainingMilliseconds / 1000d)), EnumChatType.Notification, null);
                return;
            }

            sapi.Logger.Debug("[TheRustweave] Cast start requested by player '{0}' from prepared slot {1} (internal index {2}) for spell '{3}' with targetType '{4}'.", player.PlayerUID, RustweaveStateService.ToDisplaySlotNumber(slotIndex), slotIndex, spell.Code, spell.TargetType);
            if (!spellExecutor.TryResolveTarget(player, spell, out var lockedTarget, out var lockFailureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' could not resolve a cast-start target for player '{1}': {2}", spell.Code, player.PlayerUID, lockFailureReason);
                SendSpellTargetFailure(player, lockFailureReason);
                return;
            }

            if (!spellExecutor.TryBuildPlan(player, state, spell, lockedTarget, out var previewPlan, out var previewFailureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' could not start for player '{1}': {2}", spell.Code, player.PlayerUID, previewFailureReason);
                SendSpellTargetFailure(player, previewFailureReason);
                return;
            }

            var projectedCorruption = state.CurrentTemporalCorruption + previewPlan!.CorruptionDelta + spell.CorruptionCost;
            if (projectedCorruption > state.EffectiveTemporalCorruptionThreshold)
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' was rejected for player '{1}' because the corruption cap would be exceeded.", spell.Code, player.PlayerUID);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-capacity-fail"), EnumChatType.Notification, null);
                return;
            }

            var castSpell = spell;

            if (activeCasts.ContainsKey(player.PlayerUID))
            {
                return;
            }

            var duration = (long)Math.Round(castSpell.CastTimeSeconds * 1000d / Math.Max(0.0001f, state.CastSpeedMultiplier));
            if (duration <= 0)
            {
                duration = 1;
            }

            var castState = new RustweaveCastStateData
            {
                IsCasting = true,
                SpellCode = castSpell.Code,
                PreparedSpellIndex = slotIndex,
                StartedAtMilliseconds = sapi.World.ElapsedMilliseconds,
                DurationMilliseconds = duration,
                ElapsedMilliseconds = 0,
                CorruptionCost = castSpell.CorruptionCost,
                BaseCastTimeSeconds = castSpell.CastTimeSeconds,
                StartingTemporalCorruption = state.CurrentTemporalCorruption,
                ActiveCastId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)
            };

            ApplyLockedTarget(castState, spell, lockedTarget);
            sapi.Logger.Debug("[TheRustweave] Cast start locked target for player '{0}': hasLock={1}, lockType='{2}', targetName='{3}', entityId={4}.",
                player.PlayerUID, castState.HasLockedTarget, castState.LockedTargetType, castState.LockedTargetName, castState.LockedEntityId);

            activeCasts[player.PlayerUID] = castState;
            SyncCastState(player, castState);
            BroadcastTargetPreviewState(player, spell, lockedTarget, true, true);
            SendTargetLockState(player, castState, spell, true);
            RegisterTargetedWarningIfNeeded(player, spell, castState, lockedTarget);
        }

        private void FinishCast(IServerPlayer player, RustweavePlayerStateData state, RustweaveCastStateData castState)
        {
            if (!activeCasts.Remove(player.PlayerUID))
            {
                return;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(castState.SpellCode, out var spell) || spell == null)
            {
                player.SendMessage(0, Lang.Get("game:rustweave-cast-fail"), EnumChatType.Notification, null);
                ClearCastState(player);
                return;
            }

            sapi.Logger.Debug("[TheRustweave] Cast completion rebuilding target context for player '{0}' and spell '{1}'.", player.PlayerUID, spell.Code);
            if (TryGetCooldownRemaining(player.PlayerUID, spell.Code, out var remainingMilliseconds))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' was rejected at completion for player '{1}' because it is on cooldown for {2} ms.", spell.Code, player.PlayerUID, remainingMilliseconds);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-cooldown", spell.Name, RustweaveStateService.FormatSeconds(remainingMilliseconds / 1000d)), EnumChatType.Notification, null);
                ClearCastState(player);
                return;
            }

            if (!spellExecutor.TryResolveLockedTarget(player, spell, castState, out var lockedTarget, out var lockFailureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed at completion for player '{1}' because the locked target was invalid: {2}", spell.Code, player.PlayerUID, lockFailureReason);
                SendSpellTargetFailure(player, lockFailureReason);
                ClearCastState(player);
                return;
            }

            sapi.Logger.Debug("[TheRustweave] Cast completion rebuilt target context for player '{0}' and spell '{1}' as targetType '{2}' with target '{3}'.",
                player.PlayerUID, spell.Code, spell.TargetType, lockedTarget.TargetName);
            if (!spellExecutor.TryBuildPlan(player, state, spell, lockedTarget, out var executionPlan, out var failureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed at completion for player '{1}': {2}", spell.Code, player.PlayerUID, failureReason);
                SendSpellTargetFailure(player, failureReason);
                ClearCastState(player);
                return;
            }

            var projectedCorruption = state.CurrentTemporalCorruption + executionPlan!.CorruptionDelta + spell.CorruptionCost;
            if (projectedCorruption > state.EffectiveTemporalCorruptionThreshold)
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed at completion for player '{1}' because the corruption cap would be exceeded.", spell.Code, player.PlayerUID);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-capacity-fail"), EnumChatType.Notification, null);
                ClearCastState(player);
                return;
            }

            if (!spellExecutor.TryExecutePlan(player, state, executionPlan, out var executeFailureReason))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' failed while executing for player '{1}': {2}", spell.Code, player.PlayerUID, executeFailureReason);
                player.SendMessage(0, Lang.Get("game:rustweave-cast-fail"), EnumChatType.Notification, null);
                ClearCastState(player);
                return;
            }

            RustweaveStateService.IncrementSpellCastCount(state, spell.Code);
            state.CurrentTemporalCorruption = Math.Min(state.EffectiveTemporalCorruptionThreshold, state.CurrentTemporalCorruption + spell.CorruptionCost);
            SaveAndSyncState(player, state);
            SetSpellCooldown(player.PlayerUID, spell);
            ClearCastState(player);
            player.SendMessage(0, Lang.Get("game:rustweave-cast-success"), EnumChatType.Notification, null);
        }

        private void CancelCast(IServerPlayer player, RustweavePlayerStateData state, string message, bool notifyPlayer, bool addCancelCorruption)
        {
            if (activeCasts.Remove(player.PlayerUID))
            {
                if (addCancelCorruption)
                {
                    state.CurrentTemporalCorruption = Math.Min(state.EffectiveTemporalCorruptionThreshold, state.CurrentTemporalCorruption + 5);
                }

                SaveAndSyncState(player, state);
                ClearCastState(player);

                if (notifyPlayer)
                {
                    player.SendMessage(0, message, EnumChatType.Notification, null);
                }
            }
        }

        private void ClearCastState(IPlayer player)
        {
            var castState = new RustweaveCastStateData();
            if (player is IServerPlayer serverPlayer)
            {
                ClearTargetedWarningState(casterPlayerUid: serverPlayer.PlayerUID, sendInactive: true);
                SendTargetLockState(serverPlayer, castState, null, false);
                ClearTargetPreviewState(serverPlayer);
            }

            if (player.Entity?.WatchedAttributes == null)
            {
                return;
            }

            RustweaveStateService.SyncWatchedCastState(player, castState);
        }

        private void RemoveActiveEffectsForPlayer(string playerUid)
        {
            if (string.IsNullOrWhiteSpace(playerUid) || activeEffectRecords.Count == 0)
            {
                return;
            }

            foreach (var entry in activeEffectRecords.Where(pair => string.Equals(pair.Value.CasterPlayerUid, playerUid, StringComparison.OrdinalIgnoreCase) || string.Equals(pair.Value.TargetPlayerUid, playerUid, StringComparison.OrdinalIgnoreCase)).Select(pair => pair.Key).ToArray())
            {
                TryRemoveActiveEffect(entry, true);
            }
        }

        private void SyncCastState(IPlayer player, RustweaveCastStateData state)
        {
            if (player.Entity == null)
            {
                return;
            }

            RustweaveStateService.SyncWatchedCastState(player, state.Clone());
        }

        private void OnClientPreviewPacket(IServerPlayer fromPlayer, RustweaveTargetPreviewPacket packet)
        {
            if (fromPlayer == null || packet == null)
            {
                return;
            }

            if (!RustweaveStateService.IsRustweaver(fromPlayer))
            {
                return;
            }

            var playerUid = fromPlayer.PlayerUID;
            if (!packet.IsActive)
            {
                activeTargetPreviews.Remove(playerUid);
                BroadcastTargetPreviewPacket(packet, true);
                return;
            }

            if (activeCasts.ContainsKey(playerUid) && !packet.IsLocked)
            {
                return;
            }

            var normalized = packet.Clone();
            normalized.CasterPlayerUid = playerUid;
            normalized.CasterEntityId = fromPlayer.Entity?.EntityId ?? -1;
            normalized.UpdatedAtMs = sapi.World.ElapsedMilliseconds;
            activeTargetPreviews[playerUid] = normalized;

            BroadcastTargetPreviewPacket(normalized, false);
        }

        private void ProcessTargetPreviewStates()
        {
            if (activeTargetPreviews.Count == 0)
            {
                return;
            }

            var now = sapi.World.ElapsedMilliseconds;
            foreach (var entry in activeTargetPreviews.ToArray())
            {
                var playerUid = entry.Key;
                var preview = entry.Value;

                if (string.IsNullOrWhiteSpace(preview.CasterPlayerUid))
                {
                    activeTargetPreviews.Remove(playerUid);
                    continue;
                }

                if (!preview.IsActive)
                {
                    activeTargetPreviews.Remove(playerUid);
                    continue;
                }

                if (now - preview.UpdatedAtMs > 1200)
                {
                    ClearTargetPreviewState(playerUid);
                    continue;
                }

                if (preview.IsLocked && now - preview.UpdatedAtMs >= 150)
                {
                    preview.UpdatedAtMs = now;
                    activeTargetPreviews[playerUid] = preview;
                    BroadcastTargetPreviewPacket(preview, false);
                }
            }
        }

        private void ClearTargetPreviewState(IServerPlayer player)
        {
            if (player == null)
            {
                return;
            }

            ClearTargetPreviewState(player.PlayerUID);
        }

        private void ClearTargetPreviewState(string playerUid)
        {
            if (string.IsNullOrWhiteSpace(playerUid))
            {
                return;
            }

            if (!activeTargetPreviews.TryGetValue(playerUid, out var existing))
            {
                return;
            }

            activeTargetPreviews.Remove(playerUid);
            existing.IsActive = false;
            existing.IsLocked = false;
            existing.UpdatedAtMs = sapi.World.ElapsedMilliseconds;
            BroadcastTargetPreviewPacket(existing, true);
        }

        private void RegisterTargetedWarningIfNeeded(IServerPlayer caster, SpellDefinition spell, RustweaveCastStateData castState, SpellEffectExecutor.SpellTargetContext lockedTarget)
        {
            if (caster?.Entity == null || spell == null || castState == null || lockedTarget == null)
            {
                return;
            }

            if (lockedTarget.Entity is not EntityPlayer targetPlayer || string.IsNullOrWhiteSpace(targetPlayer.PlayerUID))
            {
                return;
            }

            var warningType = SpellRegistry.ResolveTargetedWarningType(spell);
            var record = new RustweaveActiveCastRecord
            {
                CastId = string.IsNullOrWhiteSpace(castState.ActiveCastId)
                    ? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)
                    : castState.ActiveCastId,
                CasterPlayerUid = caster.PlayerUID,
                CasterEntityId = caster.Entity.EntityId,
                TargetPlayerUid = targetPlayer.PlayerUID,
                TargetEntityId = targetPlayer.EntityId,
                SpellCode = spell.Code,
                SpellSchool = SpellRegistry.GetSpellSchoolCategoryDisplayName(spell),
                WarningType = warningType,
                TargetType = spell.TargetType,
                StartedAtMs = castState.StartedAtMilliseconds,
                ExpectedEndAtMs = castState.StartedAtMilliseconds + castState.DurationMilliseconds,
                ExpiresAtMs = castState.StartedAtMilliseconds + castState.DurationMilliseconds + 750,
                OriginX = caster.Entity.Pos.X + caster.Entity.LocalEyePos.X,
                OriginY = caster.Entity.Pos.Y + caster.Entity.LocalEyePos.Y,
                OriginZ = caster.Entity.Pos.Z + caster.Entity.LocalEyePos.Z,
                TargetX = targetPlayer.Pos.XYZ.X,
                TargetY = targetPlayer.Pos.XYZ.Y,
                TargetZ = targetPlayer.Pos.XYZ.Z,
                IsActive = true,
                IsPersonalWarning = true
            };

            if (string.Equals(record.WarningType, SpellWarningTypes.Neutral, StringComparison.OrdinalIgnoreCase))
            {
                activeTargetedCastRecords[record.CastId] = record.Clone();
                return;
            }

            var recipients = BuildTargetedWarningRecipients(record, targetPlayer);
            record.ObserverPlayerUids = recipients.Select(player => player.PlayerUID).ToList();
            activeTargetedCastRecords[record.CastId] = record.Clone();
            BroadcastTargetedWarningState(record, recipients);
        }

        private List<IServerPlayer> BuildTargetedWarningRecipients(RustweaveActiveCastRecord record, EntityPlayer? targetPlayer)
        {
            var recipients = new List<IServerPlayer>();
            if (record == null)
            {
                return recipients;
            }

            var targetServerPlayer = GetOnlineServerPlayer(record.TargetPlayerUid);
            if (targetServerPlayer != null)
            {
                recipients.Add(targetServerPlayer);
            }

            var targetPos = new Vec3d(record.TargetX, record.TargetY, record.TargetZ);
            foreach (var online in sapi.World.AllOnlinePlayers.OfType<IServerPlayer>())
            {
                if (online == null || online.Entity == null || !online.Entity.Alive)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(record.TargetPlayerUid) && string.Equals(online.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (targetPlayer != null && online.Entity.Pos.Dimension != targetPlayer.Pos.Dimension)
                {
                    continue;
                }

                var eyePos = online.Entity.LocalEyePos;
                var originPos = new Vec3d(online.Entity.Pos.X + eyePos.X, online.Entity.Pos.Y + eyePos.Y, online.Entity.Pos.Z + eyePos.Z);
                if (originPos.DistanceTo(targetPos) > 32d)
                {
                    continue;
                }

                if (!CanObserveTargetedWarning(online, targetPos, record.TargetEntityId))
                {
                    continue;
                }

                recipients.Add(online);
            }

            return recipients;
        }

        private bool CanObserveTargetedWarning(IServerPlayer observer, Vec3d targetPos, long targetEntityId)
        {
            if (observer?.Entity == null)
            {
                return false;
            }

            var eyePos = observer.Entity.LocalEyePos;
            var fromPos = new Vec3d(observer.Entity.Pos.X + eyePos.X, observer.Entity.Pos.Y + eyePos.Y, observer.Entity.Pos.Z + eyePos.Z);
            BlockSelection? blockSelection = null;
            EntitySelection? entitySelection = null;
            sapi.World.RayTraceForSelection(
                fromPos,
                targetPos,
                ref blockSelection,
                ref entitySelection,
                null,
                candidate => candidate != null && candidate.Alive && candidate != observer.Entity);

            if (targetEntityId >= 0)
            {
                return entitySelection?.Entity != null && entitySelection.Entity.EntityId == targetEntityId;
            }

            return blockSelection == null && entitySelection?.Entity == null;
        }

        private void BroadcastTargetedWarningState(RustweaveActiveCastRecord record, IReadOnlyList<IServerPlayer> recipients)
        {
            if (record == null || recipients == null || recipients.Count == 0 || channel == null)
            {
                return;
            }

            var packet = record.ToPacket();
            var targetRecipient = recipients.FirstOrDefault(player => string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase));
            if (targetRecipient != null)
            {
                var personalPacket = packet.Clone();
                personalPacket.IsPersonalWarning = true;
                channel.SendPacket(personalPacket, new[] { targetRecipient });
            }

            var observerRecipients = recipients
                .Where(player => player != null && !string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (observerRecipients.Length > 0)
            {
                var observerPacket = packet.Clone();
                observerPacket.IsPersonalWarning = false;
                channel.SendPacket(observerPacket, observerRecipients);
            }
        }

        private void ClearTargetedWarningState(string? casterPlayerUid = null, string? targetPlayerUid = null, string? castId = null, bool sendInactive = true)
        {
            if (activeTargetedCastRecords.Count == 0)
            {
                return;
            }

            var matches = activeTargetedCastRecords.Values
                .Where(record =>
                    record != null
                    && record.IsActive
                    && (string.IsNullOrWhiteSpace(castId) || string.Equals(record.CastId, castId, StringComparison.OrdinalIgnoreCase))
                    && (string.IsNullOrWhiteSpace(casterPlayerUid) || string.Equals(record.CasterPlayerUid, casterPlayerUid, StringComparison.OrdinalIgnoreCase))
                    && (string.IsNullOrWhiteSpace(targetPlayerUid) || string.Equals(record.TargetPlayerUid, targetPlayerUid, StringComparison.OrdinalIgnoreCase)))
                .Select(record => record.Clone())
                .ToList();

            foreach (var record in matches)
            {
                activeTargetedCastRecords.Remove(record.CastId);
                if (!sendInactive || channel == null)
                {
                    continue;
                }

                record.IsActive = false;
                var recipients = GetWarningRecipientsForClear(record);
                if (recipients.Count == 0)
                {
                    continue;
                }

                var packet = record.ToPacket();
                packet.IsActive = false;
                if (recipients.Any(player => string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase)))
                {
                    var targetRecipient = recipients.First(player => string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase));
                    var personalPacket = packet.Clone();
                    personalPacket.IsPersonalWarning = true;
                    channel.SendPacket(personalPacket, new[] { targetRecipient });
                }

                var observerRecipients = recipients
                    .Where(player => !string.Equals(player.PlayerUID, record.TargetPlayerUid, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (observerRecipients.Length > 0)
                {
                    var observerPacket = packet.Clone();
                    observerPacket.IsPersonalWarning = false;
                    channel.SendPacket(observerPacket, observerRecipients);
                }
            }
        }

        private List<IServerPlayer> GetWarningRecipientsForClear(RustweaveActiveCastRecord record)
        {
            var recipients = new List<IServerPlayer>();
            if (record == null)
            {
                return recipients;
            }

            if (!string.IsNullOrWhiteSpace(record.TargetPlayerUid))
            {
                var targetPlayer = GetOnlineServerPlayer(record.TargetPlayerUid);
                if (targetPlayer != null)
                {
                    recipients.Add(targetPlayer);
                }
            }

            foreach (var observerUid in record.ObserverPlayerUids ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(observerUid))
                {
                    continue;
                }

                var observerPlayer = GetOnlineServerPlayer(observerUid);
                if (observerPlayer != null && recipients.All(player => !string.Equals(player.PlayerUID, observerPlayer.PlayerUID, StringComparison.OrdinalIgnoreCase)))
                {
                    recipients.Add(observerPlayer);
                }
            }

            return recipients;
        }

        private void ProcessActiveTargetedWarningRecords()
        {
            if (activeTargetedCastRecords.Count == 0)
            {
                return;
            }

            var now = sapi.World.ElapsedMilliseconds;
            foreach (var entry in activeTargetedCastRecords.Values.ToArray())
            {
                if (entry == null)
                {
                    continue;
                }

                if (!entry.IsActive || now >= entry.ExpiresAtMs)
                {
                    ClearTargetedWarningState(entry.CasterPlayerUid, entry.TargetPlayerUid, entry.CastId, true);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.TargetPlayerUid) || GetOnlineServerPlayer(entry.TargetPlayerUid) == null)
                {
                    ClearTargetedWarningState(entry.CasterPlayerUid, entry.TargetPlayerUid, entry.CastId, true);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.CasterPlayerUid) || GetOnlineServerPlayer(entry.CasterPlayerUid) == null)
                {
                    ClearTargetedWarningState(entry.CasterPlayerUid, entry.TargetPlayerUid, entry.CastId, true);
                }
            }
        }

        public IReadOnlyList<RustweaveActiveCastRecord> GetActiveCastsTargetingPlayer(string targetPlayerUid)
        {
            if (string.IsNullOrWhiteSpace(targetPlayerUid) || activeTargetedCastRecords.Count == 0)
            {
                return Array.Empty<RustweaveActiveCastRecord>();
            }

            return activeTargetedCastRecords.Values
                .Where(record => record != null
                    && record.IsActive
                    && string.Equals(record.TargetPlayerUid, targetPlayerUid, StringComparison.OrdinalIgnoreCase))
                .Select(record => record.Clone())
                .ToArray();
        }

        public IReadOnlyList<RustweaveActiveCastRecord> GetActiveHostileCastsTargetingPlayer(string targetPlayerUid)
        {
            if (string.IsNullOrWhiteSpace(targetPlayerUid))
            {
                return Array.Empty<RustweaveActiveCastRecord>();
            }

            return GetActiveCastsTargetingPlayer(targetPlayerUid)
                .Where(record => string.Equals(record.WarningType, SpellWarningTypes.Hostile, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        public IReadOnlyList<RustweaveActiveCastRecord> GetActiveHostileCastsNearPosition(Vec3d position, double radius)
        {
            if (radius <= 0d || activeTargetedCastRecords.Count == 0)
            {
                return Array.Empty<RustweaveActiveCastRecord>();
            }

            var squaredRadius = radius * radius;
            return activeTargetedCastRecords.Values
                .Where(record => record != null
                    && record.IsActive
                    && string.Equals(record.WarningType, SpellWarningTypes.Hostile, StringComparison.OrdinalIgnoreCase)
                    && DistanceSquared(position, record.TargetX, record.TargetY, record.TargetZ) <= squaredRadius)
                .Select(record => record.Clone())
                .ToArray();
        }

        public bool TryGetActiveCastById(string castId, out RustweaveActiveCastRecord? record)
        {
            record = null;
            if (string.IsNullOrWhiteSpace(castId))
            {
                return false;
            }

            if (!activeTargetedCastRecords.TryGetValue(castId, out var existing) || existing == null)
            {
                return false;
            }

            if (!existing.IsActive)
            {
                return false;
            }

            record = existing.Clone();
            return true;
        }

        public bool TryCancelActiveCast(string castId, string reason)
        {
            if (string.IsNullOrWhiteSpace(castId) || !activeTargetedCastRecords.TryGetValue(castId, out var record) || record == null)
            {
                return false;
            }

            if (!record.IsActive)
            {
                return false;
            }

            var player = GetOnlineServerPlayer(record.CasterPlayerUid);
            if (player == null)
            {
                ClearTargetedWarningState(record.CasterPlayerUid, record.TargetPlayerUid, record.CastId, true);
                return true;
            }

            var state = GetState(player);
            CancelCast(player, state, string.IsNullOrWhiteSpace(reason) ? Lang.Get("game:rustweave-cast-cancel") : reason, true, true);
            return true;
        }

        private static double DistanceSquared(Vec3d position, double x, double y, double z)
        {
            var dx = position.X - x;
            var dy = position.Y - y;
            var dz = position.Z - z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        private void BroadcastTargetPreviewState(IServerPlayer player, SpellDefinition spell, SpellEffectExecutor.SpellTargetContext lockedTarget, bool isActive, bool isLocked)
        {
            if (player == null || spell == null)
            {
                return;
            }

            var packet = BuildTargetPreviewPacket(player, spell, lockedTarget, isActive, isLocked);
            if (packet == null)
            {
                if (!isActive)
                {
                    ClearTargetPreviewState(player);
                }

                return;
            }

            if (!isActive)
            {
                ClearTargetPreviewState(player);
                return;
            }

            activeTargetPreviews[player.PlayerUID] = packet;
            BroadcastTargetPreviewPacket(packet, true);
        }

        private RustweaveTargetPreviewPacket? BuildTargetPreviewPacket(IServerPlayer player, SpellDefinition spell, SpellEffectExecutor.SpellTargetContext lockedTarget, bool isActive, bool isLocked)
        {
            if (player?.Entity == null || spell == null || lockedTarget == null)
            {
                return null;
            }

            var previewInfo = SpellRegistry.GetPreviewInfo(spell);
            if (string.Equals(previewInfo.Mode, SpellPreviewModes.None, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var packet = new RustweaveTargetPreviewPacket
            {
                IsActive = isActive,
                IsLocked = isLocked,
                CasterPlayerUid = player.PlayerUID,
                CasterEntityId = player.Entity.EntityId,
                SpellCode = spell.Code,
                SpellName = spell.Name,
                PreviewMode = previewInfo.Mode,
                TargetType = spell.TargetType,
                ColorClass = previewInfo.ColorClass,
                TargetName = lockedTarget.TargetName,
                TargetEntityId = lockedTarget.Entity?.EntityId ?? -1,
                SourceX = player.Entity.Pos.X + player.Entity.LocalEyePos.X,
                SourceY = player.Entity.Pos.Y + player.Entity.LocalEyePos.Y,
                SourceZ = player.Entity.Pos.Z + player.Entity.LocalEyePos.Z,
                TargetX = lockedTarget.Entity?.Pos.XYZ.X ?? lockedTarget.Position.X,
                TargetY = lockedTarget.Entity?.Pos.XYZ.Y ?? lockedTarget.Position.Y,
                TargetZ = lockedTarget.Entity?.Pos.XYZ.Z ?? lockedTarget.Position.Z,
                ImpactX = lockedTarget.Position.X,
                ImpactY = lockedTarget.Position.Y,
                ImpactZ = lockedTarget.Position.Z,
                Radius = previewInfo.Radius,
                Width = previewInfo.Width,
                Length = previewInfo.Length,
                UsesGravity = previewInfo.UsesGravity,
                ShowImpactPoint = previewInfo.ShowImpactPoint,
                MarkerStyle = previewInfo.MarkerStyle,
                UpdatedAtMs = sapi.World.ElapsedMilliseconds
            };

            if (string.IsNullOrWhiteSpace(packet.TargetName))
            {
                packet.TargetName = ResolvePreviewTargetName(spell, lockedTarget);
            }

            return packet;
        }

        private void BroadcastTargetPreviewPacket(RustweaveTargetPreviewPacket packet, bool includeCaster)
        {
            if (packet == null || channel == null)
            {
                return;
            }

            var recipients = sapi.World.AllOnlinePlayers
                .OfType<IServerPlayer>()
                .Where(player => includeCaster || !string.Equals(player.PlayerUID, packet.CasterPlayerUid, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (recipients.Length == 0)
            {
                return;
            }

            channel.SendPacket(packet, recipients);
        }

        private string ResolvePreviewTargetName(SpellDefinition spell, SpellEffectExecutor.SpellTargetContext lockedTarget)
        {
            if (spell == null || lockedTarget == null)
            {
                return string.Empty;
            }

            return spell.TargetType switch
            {
                SpellTargetTypes.Self => "Self",
                SpellTargetTypes.HeldItem => "Held Item",
                SpellTargetTypes.Inventory => "Inventory",
                SpellTargetTypes.LookEntity => lockedTarget.Entity?.GetName() ?? lockedTarget.TargetName,
                SpellTargetTypes.LookPlayer => lockedTarget.Entity?.GetName() ?? "Player",
                SpellTargetTypes.LookNonPlayerEntity => lockedTarget.Entity?.GetName() ?? "Creature/NPC",
                SpellTargetTypes.LookDroppedItem => lockedTarget.Entity?.GetName() ?? "Dropped Item",
                SpellTargetTypes.LookBlock => lockedTarget.TargetName,
                SpellTargetTypes.LookBlockEntity => !string.IsNullOrWhiteSpace(lockedTarget.TargetName) ? lockedTarget.TargetName : "Block Entity",
                SpellTargetTypes.LookContainer => !string.IsNullOrWhiteSpace(lockedTarget.TargetName) ? lockedTarget.TargetName : "Container",
                SpellTargetTypes.SelfArea => SpellTargetTypes.GetDisplayName(SpellTargetTypes.SelfArea),
                SpellTargetTypes.LookArea => SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookArea),
                SpellTargetTypes.LookPosition => "Position",
                _ => lockedTarget.TargetName
            };
        }

        private void ApplyLockedTarget(RustweaveCastStateData castState, SpellDefinition spell, SpellEffectExecutor.SpellTargetContext lockedTarget)
        {
            castState.HasLockedTarget = false;
            castState.LockedTargetType = string.Empty;
            castState.LockedEntityId = -1;
            castState.LockedBlockX = -1;
            castState.LockedBlockY = -1;
            castState.LockedBlockZ = -1;
            castState.LockedPosX = 0;
            castState.LockedPosY = 0;
            castState.LockedPosZ = 0;
            castState.LockedTargetName = string.Empty;

            if (lockedTarget == null)
            {
                return;
            }

            switch (spell.TargetType)
            {
                case SpellTargetTypes.Self:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.Self;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = string.IsNullOrWhiteSpace(lockedTarget.TargetName) ? "Self" : lockedTarget.TargetName;
                    break;
                case SpellTargetTypes.HeldItem:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.HeldItem;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = string.IsNullOrWhiteSpace(lockedTarget.TargetName) ? "Held Item" : lockedTarget.TargetName;
                    break;
                case SpellTargetTypes.Inventory:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.Inventory;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = string.IsNullOrWhiteSpace(lockedTarget.TargetName) ? "Inventory" : lockedTarget.TargetName;
                    break;
                case SpellTargetTypes.LookEntity:
                    if (lockedTarget.Entity != null)
                    {
                        castState.HasLockedTarget = true;
                        castState.LockedTargetType = SpellTargetTypes.LookEntity;
                        castState.LockedEntityId = lockedTarget.Entity.EntityId;
                        castState.LockedPosX = lockedTarget.Position.X;
                        castState.LockedPosY = lockedTarget.Position.Y;
                        castState.LockedPosZ = lockedTarget.Position.Z;
                        castState.LockedTargetName = lockedTarget.TargetName;
                    }
                    break;
                case SpellTargetTypes.LookPlayer:
                case SpellTargetTypes.LookNonPlayerEntity:
                case SpellTargetTypes.LookDroppedItem:
                    if (lockedTarget.Entity != null)
                    {
                        castState.HasLockedTarget = true;
                        castState.LockedTargetType = spell.TargetType;
                        castState.LockedEntityId = lockedTarget.Entity.EntityId;
                        castState.LockedPosX = lockedTarget.Position.X;
                        castState.LockedPosY = lockedTarget.Position.Y;
                        castState.LockedPosZ = lockedTarget.Position.Z;
                        castState.LockedTargetName = lockedTarget.TargetName;
                    }
                    break;
                case SpellTargetTypes.LookBlock:
                    if (lockedTarget.BlockPos != null)
                    {
                        castState.HasLockedTarget = true;
                        castState.LockedTargetType = SpellTargetTypes.LookBlock;
                        castState.LockedBlockX = lockedTarget.BlockPos.X;
                        castState.LockedBlockY = lockedTarget.BlockPos.Y;
                        castState.LockedBlockZ = lockedTarget.BlockPos.Z;
                        castState.LockedPosX = lockedTarget.Position.X;
                        castState.LockedPosY = lockedTarget.Position.Y;
                        castState.LockedPosZ = lockedTarget.Position.Z;
                        castState.LockedTargetName = lockedTarget.TargetName;
                    }
                    break;
                case SpellTargetTypes.LookBlockEntity:
                case SpellTargetTypes.LookContainer:
                    if (lockedTarget.BlockPos != null)
                    {
                        castState.HasLockedTarget = true;
                        castState.LockedTargetType = spell.TargetType;
                        castState.LockedBlockX = lockedTarget.BlockPos.X;
                        castState.LockedBlockY = lockedTarget.BlockPos.Y;
                        castState.LockedBlockZ = lockedTarget.BlockPos.Z;
                        castState.LockedPosX = lockedTarget.Position.X;
                        castState.LockedPosY = lockedTarget.Position.Y;
                        castState.LockedPosZ = lockedTarget.Position.Z;
                        castState.LockedTargetName = lockedTarget.TargetName;
                    }
                    break;
                case SpellTargetTypes.LookPosition:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.LookPosition;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = lockedTarget.TargetName;
                    break;
                case SpellTargetTypes.SelfArea:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.SelfArea;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = lockedTarget.TargetName;
                    break;
                case SpellTargetTypes.LookArea:
                    castState.HasLockedTarget = true;
                    castState.LockedTargetType = SpellTargetTypes.LookArea;
                    castState.LockedPosX = lockedTarget.Position.X;
                    castState.LockedPosY = lockedTarget.Position.Y;
                    castState.LockedPosZ = lockedTarget.Position.Z;
                    castState.LockedTargetName = lockedTarget.TargetName;
                    break;
            }
        }

        private void SendTargetLockState(IServerPlayer player, RustweaveCastStateData? castState, SpellDefinition? spell, bool isActive)
        {
            if (player == null || channel == null)
            {
                sapi.Logger.Warning("[TheRustweave] Target lock packet could not be sent because the network channel or player was unavailable.");
                return;
            }

            var packet = new RustweaveTargetLockPacket
            {
                IsActive = isActive && castState?.HasLockedTarget == true && spell != null,
                SpellCode = spell?.Code ?? string.Empty,
                SpellName = spell?.Name ?? string.Empty,
                TargetType = castState?.LockedTargetType ?? string.Empty,
                TargetName = castState?.LockedTargetName ?? string.Empty,
                TargetEntityId = castState?.LockedEntityId ?? -1,
                TargetX = castState?.LockedPosX ?? 0,
                TargetY = castState?.LockedPosY ?? 0,
                TargetZ = castState?.LockedPosZ ?? 0,
                CastDurationMs = castState?.DurationMilliseconds ?? 0,
                CastStartedAtMs = castState?.StartedAtMilliseconds ?? 0
            };

            channel.SendPacket(packet, new[] { player });
        }
    }

    internal sealed class RustweaveClientController
    {
        private readonly ICoreClientAPI capi;
        private IClientNetworkChannel? channel;
        private long tickListenerId;
        private RustweavePlayerStateData currentState = RustweaveStateService.CreateFreshState();
        private RustweaveCastStateData currentCastState = new();
        private RustweaveTargetLockPacket currentTargetLock = new();
        private readonly Dictionary<string, RustweaveTargetPreviewPacket> activeTargetPreviews = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> activeTargetPreviewRenderTimes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> activeTargetPreviewSendTimes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustweaveTargetedWarningPacket> activeTargetedWarnings = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> activeTargetedWarningRenderTimes = new(StringComparer.OrdinalIgnoreCase);
        private int localSelectedPreparedSpellIndex = -1;
        private string lastStateJson = string.Empty;
        private string lastCastJson = string.Empty;
        private RustweaveCorruptionHud? corruptionHud;
        private RustweaveCastHud? castHud;
        private RustweaveTargetLockHud? targetLockHud;
        private RustweaveSpellPrepDialog? prepDialog;

        public RustweaveClientController(ICoreClientAPI capi)
        {
            this.capi = capi;
        }

        public void Start()
        {
            channel = capi.Network.RegisterChannel(RustweaveConstants.NetworkChannelName);
            channel.RegisterMessageType(typeof(RustweaveActionPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetLockPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetPreviewPacket));
            channel.RegisterMessageType(typeof(RustweaveTargetedWarningPacket));
            channel.SetMessageHandler<RustweaveTargetLockPacket>(OnTargetLockPacket);
            channel.SetMessageHandler<RustweaveTargetPreviewPacket>(OnTargetPreviewPacket);
            channel.SetMessageHandler<RustweaveTargetedWarningPacket>(OnTargetedWarningPacket);
            capi.Event.PlayerJoin += OnClientPlayerJoin;
            capi.Event.LevelFinalize += OnClientLevelFinalize;
            tickListenerId = capi.Event.RegisterGameTickListener(OnClientTick, 50, 50);
            capi.Event.MouseWheelMove += OnMouseWheelMove;
            HydrateFromSavedState();
        }

        public void OpenPreparationGui()
        {
            HydrateFromSavedState();
            if (!RustweaveStateService.IsRustweaver(capi.World.Player))
            {
                capi.ShowChatMessage(Lang.Get("game:rustweave-tome-reject"));
                return;
            }

            EnsurePrepDialog();
            prepDialog!.SetState(currentState, false);
            prepDialog.OpenDialog();
        }

        public bool IsCasting => currentCastState.IsCasting;

        public void RequestStartCast()
        {
            HydrateFromSavedState();
            if (currentState.CurrentTemporalCorruption >= currentState.EffectiveTemporalCorruptionThreshold)
            {
                capi.ShowChatMessage(Lang.Get("game:rustweave-cast-locked"));
                return;
            }

            var selectedSlotIndex = GetSelectedPreparedSlotIndex();
            var displaySlotNumber = RustweaveStateService.ToDisplaySlotNumber(selectedSlotIndex);
            capi.Logger.Debug("[TheRustweave] Cast requested from prepared slot {0} (internal index {1}).", displaySlotNumber, selectedSlotIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestStartCast,
                SlotIndex = selectedSlotIndex
            });
        }

        public void RequestCancelCast()
        {
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestCancelCast
            });
            RequestPreviewStop(true);
        }

        public void RequestPreviewUpdate(EntityAgent byEntity, BlockSelection? blockSel, EntitySelection? entitySel)
        {
            HydrateFromSavedState();
            var player = capi.World?.Player;
            if (player == null || byEntity == null || currentCastState.IsCasting || currentTargetLock.IsActive || !RustweaveStateService.IsRustweaver(player) || !RustweaveStateService.IsHoldingTome(player))
            {
                return;
            }

            if (!TryBuildLocalPreviewPacket(byEntity, blockSel, entitySel, out var packet))
            {
                RequestPreviewStop(false);
                return;
            }

            var previewKey = GetPreviewKey(packet);
            if (string.IsNullOrWhiteSpace(previewKey))
            {
                return;
            }

            activeTargetPreviews[previewKey] = packet;
            activeTargetPreviewRenderTimes[previewKey] = 0;
            if (ShouldSendPreviewPacket(packet))
            {
                SendPacket(packet.Clone());
            }
        }

        public void RequestPreviewStop(bool forceSend = false)
        {
            var keepLockedPreview = currentCastState.IsCasting;
            if (forceSend)
            {
                foreach (var key in activeTargetPreviews.Keys.ToArray())
                {
                    activeTargetPreviews.Remove(key);
                    activeTargetPreviewRenderTimes.Remove(key);
                }
            }
            else
            {
                foreach (var entry in activeTargetPreviews.ToArray())
                {
                    if (keepLockedPreview && entry.Value.IsLocked)
                    {
                        continue;
                    }

                    activeTargetPreviews.Remove(entry.Key);
                    activeTargetPreviewRenderTimes.Remove(entry.Key);
                }
            }

            if (!forceSend)
            {
                return;
            }

            var player = capi.World?.Player;
            if (player == null)
            {
                return;
            }

            SendPacket(new RustweaveTargetPreviewPacket
            {
                IsActive = false,
                IsLocked = false,
                CasterPlayerUid = player.PlayerUID,
                CasterEntityId = player.Entity?.EntityId ?? -1,
                UpdatedAtMs = capi.World?.ElapsedMilliseconds ?? 0
            });
        }

        public void RequestSelectPreparedSpell(int slotIndex)
        {
            RequestPreviewStop();
            if (RustweaveStateService.IsValidSlotIndex(slotIndex))
            {
                localSelectedPreparedSpellIndex = slotIndex;
                currentState.SelectedPreparedSpellIndex = slotIndex;
                prepDialog?.SetState(currentState);
            }

            capi.Logger.Debug("[TheRustweave] Client requested prepared slot selection: {0} (internal index {1}).", RustweaveStateService.ToDisplaySlotNumber(slotIndex), slotIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestSelectPrepared,
                SlotIndex = slotIndex
            });
        }

        public void RequestPrepareSpell(string spellCode, int targetSlotIndex)
        {
            capi.Logger.Debug("[TheRustweave] Client requested prepare for spell '{0}' with target slot {1}.", spellCode, RustweaveStateService.DescribePreparedSlot(targetSlotIndex));
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestPrepareSpell,
                SpellCode = spellCode,
                SlotIndex = targetSlotIndex
            });
        }

        public void RequestUnprepareSpell(int slotIndex)
        {
            RequestPreviewStop();
            if (RustweaveStateService.IsValidSlotIndex(slotIndex) && slotIndex < currentState.PreparedSpellCodes.Count)
            {
                currentState.PreparedSpellCodes[slotIndex] = string.Empty;
                prepDialog?.SetState(currentState);
            }

            capi.Logger.Debug("[TheRustweave] Client requested clear for prepared slot {0} (internal index {1}).", RustweaveStateService.ToDisplaySlotNumber(slotIndex), slotIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestUnprepareSpell,
                SlotIndex = slotIndex
            });
        }

        private void SendPacket(RustweaveActionPacket packet)
        {
            channel?.SendPacket(packet);
        }

        private void SendPacket(RustweaveTargetPreviewPacket packet)
        {
            channel?.SendPacket(packet);
        }

        private void OnClientPlayerJoin(IClientPlayer player)
        {
            if (player?.PlayerUID != capi.World?.Player?.PlayerUID)
            {
                return;
            }

            HydrateFromSavedState();
        }

        private void OnClientLevelFinalize()
        {
            HydrateFromSavedState();
            RequestPreviewStop();
            ClearTargetedWarnings();
        }

        private void OnTargetLockPacket(RustweaveTargetLockPacket packet)
        {
            currentTargetLock = packet?.Clone() ?? new RustweaveTargetLockPacket();
        }

        private void OnTargetPreviewPacket(RustweaveTargetPreviewPacket packet)
        {
            if (packet == null)
            {
                return;
            }

            var preview = packet.Clone();
            if (!preview.IsLocked && (currentCastState.IsCasting || currentTargetLock.IsActive))
            {
                return;
            }

            var key = GetPreviewKey(preview);
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (!preview.IsActive)
            {
                activeTargetPreviews.Remove(key);
                activeTargetPreviewRenderTimes.Remove(key);
                return;
            }

            activeTargetPreviews[key] = preview;
            activeTargetPreviewRenderTimes[key] = 0;
        }

        private void OnTargetedWarningPacket(RustweaveTargetedWarningPacket packet)
        {
            if (packet == null || string.IsNullOrWhiteSpace(packet.CastId))
            {
                return;
            }

            var warning = packet.Clone();
            if (!warning.IsActive)
            {
                activeTargetedWarnings.Remove(warning.CastId);
                activeTargetedWarningRenderTimes.Remove(warning.CastId);
                return;
            }

            activeTargetedWarnings[warning.CastId] = warning;
            activeTargetedWarningRenderTimes[warning.CastId] = 0;
        }

        private string GetPreviewKey(RustweaveTargetPreviewPacket packet)
        {
            if (packet == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(packet.CasterPlayerUid))
            {
                return packet.CasterPlayerUid;
            }

            return packet.CasterEntityId >= 0 ? packet.CasterEntityId.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private bool ShouldSendPreviewPacket(RustweaveTargetPreviewPacket packet)
        {
            if (packet == null)
            {
                return false;
            }

            var key = GetPreviewKey(packet);
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var now = capi.World?.ElapsedMilliseconds ?? 0;
            if (!activeTargetPreviewSendTimes.TryGetValue(key, out var lastSent))
            {
                activeTargetPreviewSendTimes[key] = now;
                return true;
            }

            if (now - lastSent >= 200)
            {
                activeTargetPreviewSendTimes[key] = now;
                return true;
            }

            return false;
        }

        private bool TryBuildLocalPreviewPacket(EntityAgent byEntity, BlockSelection? blockSel, EntitySelection? entitySel, out RustweaveTargetPreviewPacket packet)
        {
            packet = new RustweaveTargetPreviewPacket();

            var player = capi.World?.Player;
            if (player == null || byEntity == null || !RustweaveStateService.IsRustweaver(player) || !RustweaveStateService.IsHoldingTome(player))
            {
                return false;
            }

            var selectedSpellCode = RustweaveStateService.GetSelectedPreparedSpellCode(currentState);
            if (string.IsNullOrWhiteSpace(selectedSpellCode) || !RustweaveRuntime.SpellRegistry.TryGetSpell(selectedSpellCode, out var spell) || spell == null)
            {
                return false;
            }

            var preview = SpellRegistry.GetPreviewInfo(spell);
            if (string.Equals(preview.Mode, SpellPreviewModes.None, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var eyePos = byEntity.LocalEyePos;
            var fromPos = new Vec3d(byEntity.Pos.X + eyePos.X, byEntity.Pos.Y + eyePos.Y, byEntity.Pos.Z + eyePos.Z);
            var feetPos = byEntity.Pos.XYZ;
            var viewVector = byEntity.Pos.GetViewVector();
            var range = Math.Max(1d, SpellRegistry.GetEffectiveLookTargetRange(spell));
            var toPos = new Vec3d(
                fromPos.X + (viewVector.X * range),
                fromPos.Y + (viewVector.Y * range),
                fromPos.Z + (viewVector.Z * range));

            if (blockSel == null && entitySel == null && !string.Equals(spell.TargetType, SpellTargetTypes.Self, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(spell.TargetType, SpellTargetTypes.HeldItem, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(spell.TargetType, SpellTargetTypes.Inventory, StringComparison.OrdinalIgnoreCase))
            {
                BlockSelection? tracedBlock = null;
                EntitySelection? tracedEntity = null;
                capi.World.RayTraceForSelection(
                    fromPos,
                    toPos,
                    ref tracedBlock,
                    ref tracedEntity,
                null,
                candidate => candidate != null
                    && candidate != byEntity
                    && candidate.Alive);

                blockSel = tracedBlock;
                entitySel = tracedEntity;
            }

            var spellPreviewMode = preview.Mode;
            var packetTargetName = string.Empty;
            double targetX;
            double targetY;
            double targetZ;
            double impactX;
            double impactY;
            double impactZ;
            long targetEntityId = -1;

            switch (spell.TargetType)
            {
                case SpellTargetTypes.Self:
                case SpellTargetTypes.HeldItem:
                case SpellTargetTypes.Inventory:
                    targetX = feetPos.X;
                    targetY = feetPos.Y;
                    targetZ = feetPos.Z;
                    impactX = targetX;
                    impactY = targetY;
                    impactZ = targetZ;
                    packetTargetName = spell.TargetType switch
                    {
                        SpellTargetTypes.Self => "Self",
                        SpellTargetTypes.HeldItem => "Held Item",
                        SpellTargetTypes.Inventory => "Inventory",
                        _ => spell.Name
                    };
                    spellPreviewMode = SpellPreviewModes.Self;
                    break;
                case SpellTargetTypes.LookEntity:
                case SpellTargetTypes.LookPlayer:
                case SpellTargetTypes.LookNonPlayerEntity:
                case SpellTargetTypes.LookDroppedItem:
                    if (entitySel?.Entity == null || !entitySel.Entity.Alive)
                    {
                        return false;
                    }

                    targetEntityId = entitySel.Entity.EntityId;
                    targetX = entitySel.Entity.Pos.XYZ.X;
                    targetY = entitySel.Entity.Pos.XYZ.Y;
                    targetZ = entitySel.Entity.Pos.XYZ.Z;
                    impactX = entitySel.HitPosition.X;
                    impactY = entitySel.HitPosition.Y;
                    impactZ = entitySel.HitPosition.Z;
                    packetTargetName = entitySel.Entity.GetName() ?? spell.Name;
                    break;
                case SpellTargetTypes.LookBlock:
                case SpellTargetTypes.LookBlockEntity:
                case SpellTargetTypes.LookContainer:
                    if (blockSel == null)
                    {
                        return false;
                    }

                    var previewBlockEntity = capi.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                    if (spell.TargetType == SpellTargetTypes.LookBlockEntity && previewBlockEntity == null)
                    {
                        return false;
                    }

                    if (spell.TargetType == SpellTargetTypes.LookContainer && !TryGetBlockEntityInventory(previewBlockEntity, out _))
                    {
                        return false;
                    }

                    targetX = blockSel.Position.X + 0.5;
                    targetY = blockSel.Position.Y + 1.0;
                    targetZ = blockSel.Position.Z + 0.5;
                    impactX = blockSel.HitPosition.X;
                    impactY = blockSel.HitPosition.Y;
                    impactZ = blockSel.HitPosition.Z;
                    packetTargetName = spell.TargetType == SpellTargetTypes.LookBlock
                        ? GetBlockDisplayName(blockSel.Position)
                        : GetBlockEntityDisplayName(blockSel.Position);
                    spellPreviewMode = SpellPreviewModes.Block;
                    break;
                case SpellTargetTypes.SelfArea:
                    targetX = feetPos.X;
                    targetY = feetPos.Y;
                    targetZ = feetPos.Z;
                    impactX = targetX;
                    impactY = targetY;
                    impactZ = targetZ;
                    packetTargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.SelfArea);
                    spellPreviewMode = SpellPreviewModes.Area;
                    break;
                case SpellTargetTypes.LookArea:
                    if (blockSel?.HitPosition != null)
                    {
                        impactX = blockSel.HitPosition.X;
                        impactY = blockSel.HitPosition.Y;
                        impactZ = blockSel.HitPosition.Z;
                    }
                    else if (entitySel?.Entity != null)
                    {
                        impactX = entitySel.HitPosition.X;
                        impactY = entitySel.HitPosition.Y;
                        impactZ = entitySel.HitPosition.Z;
                    }
                    else
                    {
                        impactX = toPos.X;
                        impactY = toPos.Y;
                        impactZ = toPos.Z;
                    }

                    targetX = impactX;
                    targetY = impactY;
                    targetZ = impactZ;
                    packetTargetName = SpellTargetTypes.GetDisplayName(SpellTargetTypes.LookArea);
                    spellPreviewMode = SpellPreviewModes.Area;
                    break;
                case SpellTargetTypes.LookPosition:
                    if (blockSel?.HitPosition != null)
                    {
                        impactX = blockSel.HitPosition.X;
                        impactY = blockSel.HitPosition.Y;
                        impactZ = blockSel.HitPosition.Z;
                    }
                    else if (entitySel?.Entity != null)
                    {
                        impactX = entitySel.HitPosition.X;
                        impactY = entitySel.HitPosition.Y;
                        impactZ = entitySel.HitPosition.Z;
                    }
                    else
                    {
                        impactX = toPos.X;
                        impactY = toPos.Y;
                        impactZ = toPos.Z;
                    }

                    targetX = impactX;
                    targetY = impactY;
                    targetZ = impactZ;
                    packetTargetName = "Position";
                    break;
                default:
                    return false;
            }

            if (string.Equals(spellPreviewMode, SpellPreviewModes.Projectile, StringComparison.OrdinalIgnoreCase)
                && string.Equals(spell.TargetType, SpellTargetTypes.LookPosition, StringComparison.OrdinalIgnoreCase))
            {
                targetX = impactX;
                targetY = impactY;
                targetZ = impactZ;
            }

            packet = new RustweaveTargetPreviewPacket
            {
                IsActive = true,
                IsLocked = false,
                CasterPlayerUid = player.PlayerUID,
                CasterEntityId = byEntity.EntityId,
                SpellCode = spell.Code,
                SpellName = string.IsNullOrWhiteSpace(spell.Name) ? spell.Code : spell.Name,
                PreviewMode = spellPreviewMode,
                TargetType = spell.TargetType,
                ColorClass = preview.ColorClass,
                TargetName = packetTargetName,
                TargetEntityId = targetEntityId,
                SourceX = fromPos.X,
                SourceY = fromPos.Y,
                SourceZ = fromPos.Z,
                TargetX = targetX,
                TargetY = targetY,
                TargetZ = targetZ,
                ImpactX = impactX,
                ImpactY = impactY,
                ImpactZ = impactZ,
                Radius = preview.Radius,
                Width = preview.Width,
                Length = preview.Length,
                UsesGravity = preview.UsesGravity,
                ShowImpactPoint = preview.ShowImpactPoint,
                MarkerStyle = preview.MarkerStyle,
                UpdatedAtMs = capi.World.ElapsedMilliseconds
            };

            return true;
        }

        private void RenderTargetPreview(RustweaveTargetPreviewPacket packet)
        {
            if (packet == null || !packet.IsActive || capi.World == null)
            {
                return;
            }

            var color = GetPreviewColor(packet.ColorClass);
            var locked = packet.IsLocked;
            var mode = packet.PreviewMode;
            var center = new Vec3d(packet.TargetX, packet.TargetY, packet.TargetZ);
            var source = new Vec3d(packet.SourceX, packet.SourceY, packet.SourceZ);
            var impact = new Vec3d(packet.ImpactX, packet.ImpactY, packet.ImpactZ);

            switch (mode)
            {
                case SpellPreviewModes.Self:
                    RenderPreviewSelf(center, color, locked);
                    break;
                case SpellPreviewModes.Entity:
                    RenderPreviewEntity(center, color, locked);
                    break;
                case SpellPreviewModes.Block:
                    RenderPreviewBlock(center, color, locked);
                    break;
                case SpellPreviewModes.Area:
                    RenderPreviewArea(center, packet.Radius > 0 ? packet.Radius : 2d, color, locked);
                    break;
                case SpellPreviewModes.Projectile:
                    RenderPreviewProjectile(source, center, impact, color, packet.UsesGravity, packet.ShowImpactPoint, locked);
                    break;
                case SpellPreviewModes.Line:
                    RenderPreviewLine(source, center, impact, color, packet.Width > 0 ? packet.Width : 0.35d, packet.ShowImpactPoint, locked);
                    break;
                case SpellPreviewModes.Position:
                default:
                    RenderPreviewPosition(center, color, locked);
                    break;
            }
        }

        private void RenderPreviewSelf(Vec3d center, int color, bool locked)
        {
            RenderPreviewRing(new Vec3d(center.X, center.Y + 0.02, center.Z), locked ? 0.52d : 0.46d, color, locked ? 6 : 4, locked);
            SpawnPreviewParticle(new Vec3d(center.X, center.Y + 0.75, center.Z), color, locked ? 0.03f : 0.025f, locked ? 0.34f : 0.28f);
        }

        private void RenderPreviewEntity(Vec3d center, int color, bool locked)
        {
            RenderPreviewRing(new Vec3d(center.X, center.Y + 0.08, center.Z), locked ? 0.48d : 0.42d, color, locked ? 5 : 4, locked);
            SpawnPreviewParticle(new Vec3d(center.X - 0.14, center.Y + 0.55, center.Z), color, locked ? 0.03f : 0.025f, locked ? 0.3f : 0.25f);
            SpawnPreviewParticle(new Vec3d(center.X + 0.12, center.Y + 0.95, center.Z), color, locked ? 0.03f : 0.025f, locked ? 0.3f : 0.25f);
            SpawnPreviewParticle(new Vec3d(center.X, center.Y + 1.22, center.Z), color, locked ? 0.03f : 0.025f, locked ? 0.3f : 0.25f);
        }

        private void RenderPreviewPosition(Vec3d center, int color, bool locked)
        {
            var groundY = center.Y + 0.02;
            RenderPreviewRing(new Vec3d(center.X, groundY, center.Z), 0.2d, color, locked ? 6 : 4, locked);
            SpawnPreviewParticle(new Vec3d(center.X, groundY + 0.1, center.Z), color, locked ? 0.028f : 0.022f, locked ? 0.3f : 0.24f);
        }

        private void RenderPreviewRing(Vec3d center, double radius, int color, int points, bool locked)
        {
            if (radius <= 0d)
            {
                radius = 0.65d;
            }

            var count = Math.Max(4, points);
            for (var i = 0; i < count; i++)
            {
                var angle = (Math.PI * 2d * i) / count;
                var x = center.X + Math.Cos(angle) * radius;
                var z = center.Z + Math.Sin(angle) * radius;
                SpawnPreviewParticle(new Vec3d(x, center.Y + 0.05, z), color, locked ? 0.038f : 0.03f, locked ? 0.38f : 0.32f);
            }
        }

        private void RenderPreviewBlock(Vec3d topCenter, int color, bool locked)
        {
            var bottomY = topCenter.Y - 1d;
            var x0 = topCenter.X - 0.48d;
            var x1 = topCenter.X + 0.48d;
            var z0 = topCenter.Z - 0.48d;
            var z1 = topCenter.Z + 0.48d;

            var markers = new[]
            {
                new Vec3d(x0, bottomY + 0.02, z0),
                new Vec3d(x1, bottomY + 0.02, z0),
                new Vec3d(x1, bottomY + 0.02, z1),
                new Vec3d(x0, bottomY + 0.02, z1),
                new Vec3d(x0, topCenter.Y + 0.02, z0),
                new Vec3d(x1, topCenter.Y + 0.02, z0),
                new Vec3d(x1, topCenter.Y + 0.02, z1),
                new Vec3d(x0, topCenter.Y + 0.02, z1),
                new Vec3d(topCenter.X, topCenter.Y + 0.03, z0),
                new Vec3d(topCenter.X, topCenter.Y + 0.03, z1),
                new Vec3d(x0, topCenter.Y + 0.03, topCenter.Z),
                new Vec3d(x1, topCenter.Y + 0.03, topCenter.Z)
            };

            foreach (var marker in markers)
            {
                SpawnPreviewParticle(marker, color, locked ? 0.03f : 0.024f, locked ? 0.34f : 0.28f);
            }

            RenderPreviewRing(new Vec3d(topCenter.X, topCenter.Y + 0.03, topCenter.Z), 0.28d, color, locked ? 6 : 4, locked);
        }

        private void RenderPreviewArea(Vec3d center, double radius, int color, bool locked)
        {
            RenderPreviewRing(new Vec3d(center.X, center.Y + 0.02, center.Z), radius, color, locked ? 8 : 6, locked);
            var points = locked ? 8 : 6;
            for (var i = 0; i < points; i++)
            {
                var angle = (Math.PI * 2d * i) / points;
                var x = center.X + Math.Cos(angle) * radius;
                var z = center.Z + Math.Sin(angle) * radius;
                SpawnPreviewParticle(new Vec3d(x, center.Y + 0.22, z), color, locked ? 0.03f : 0.024f, locked ? 0.32f : 0.26f);
            }
        }

        private void RenderPreviewProjectile(Vec3d source, Vec3d target, Vec3d impact, int color, bool usesGravity, bool showImpactPoint, bool locked)
        {
            var distance = source.DistanceTo(target);
            var segments = Math.Max(4, (int)Math.Ceiling(distance / 1.25d));
            if (segments <= 0)
            {
                segments = 4;
            }

            for (var i = 0; i <= segments; i++)
            {
                var t = (double)i / segments;
                var x = source.X + ((target.X - source.X) * t);
                var y = source.Y + ((target.Y - source.Y) * t);
                var z = source.Z + ((target.Z - source.Z) * t);
                if (usesGravity)
                {
                    var arc = Math.Sin(Math.PI * t) * Math.Max(0.4d, distance * 0.15d);
                    y += arc;
                }

                if (i == 0 || i == segments || i % 2 == 0)
                {
                    SpawnPreviewParticle(new Vec3d(x, y, z), color, locked ? 0.032f : 0.026f, locked ? 0.3f : 0.24f);
                }
            }

            if (showImpactPoint)
            {
                RenderPreviewRing(new Vec3d(impact.X, impact.Y + 0.02, impact.Z), 0.16d, color, locked ? 5 : 4, locked);
            }
        }

        private void RenderPreviewLine(Vec3d source, Vec3d target, Vec3d impact, int color, double width, bool showImpactPoint, bool locked)
        {
            var distance = source.DistanceTo(target);
            var segments = Math.Max(4, (int)Math.Ceiling(distance / 1.25d));
            var dx = target.X - source.X;
            var dz = target.Z - source.Z;
            var length = Math.Sqrt((dx * dx) + (dz * dz));
            if (length <= 0.001d)
            {
                length = 1d;
            }

            var perpX = -dz / length;
            var perpZ = dx / length;
            var halfWidth = Math.Max(0.08d, width * 0.5d);
            var offsets = new[] { -halfWidth, 0d, halfWidth };

            foreach (var offset in offsets)
            {
                for (var i = 0; i <= segments; i++)
                {
                    var t = (double)i / segments;
                    var x = source.X + (dx * t) + (perpX * offset);
                    var y = source.Y + ((target.Y - source.Y) * t);
                    var z = source.Z + (dz * t) + (perpZ * offset);
                    if (i == 0 || i == segments || i % 3 == 0)
                    {
                        SpawnPreviewParticle(new Vec3d(x, y, z), color, locked ? 0.03f : 0.024f, locked ? 0.28f : 0.22f);
                    }
                }
            }

            if (showImpactPoint)
            {
                RenderPreviewRing(new Vec3d(impact.X, impact.Y + 0.02, impact.Z), Math.Max(0.14d, width * 0.35d), color, locked ? 5 : 4, locked);
            }
        }

        private void SpawnPreviewParticle(Vec3d pos, int color, float size, float lifeLength)
        {
            if (capi.World == null)
            {
                return;
            }

            var minPos = new Vec3d(pos.X - 0.03, pos.Y - 0.03, pos.Z - 0.03);
            var maxPos = new Vec3d(pos.X + 0.03, pos.Y + 0.03, pos.Z + 0.03);
            var minVelocity = new Vec3d(0, 0.01, 0);
            var maxVelocity = new Vec3d(0, 0.02, 0);
            capi.World.SpawnParticles(1, color, minPos, maxPos, new Vec3f((float)minVelocity.X, (float)minVelocity.Y, (float)minVelocity.Z), new Vec3f((float)maxVelocity.X, (float)maxVelocity.Y, (float)maxVelocity.Z), Math.Min(size, 0.045f), -0.01f, Math.Min(lifeLength, 0.38f), EnumParticleModel.Quad, capi.World.Player);
        }

        private string GetBlockDisplayName(BlockPos pos)
        {
            if (capi.World?.BlockAccessor == null || pos == null)
            {
                return "Block";
            }

            var block = capi.World.BlockAccessor.GetBlock(pos);
            if (block?.Code != null)
            {
                var code = block.Code.Path;
                if (!string.IsNullOrWhiteSpace(code))
                {
                    return code;
                }
            }

            return "Block";
        }

        private string GetBlockEntityDisplayName(BlockPos pos)
        {
            if (capi.World?.BlockAccessor == null || pos == null)
            {
                return "Block Entity";
            }

            var blockEntity = capi.World.BlockAccessor.GetBlockEntity(pos);
            if (blockEntity?.Block?.Code != null)
            {
                var code = blockEntity.Block.Code.Path;
                if (!string.IsNullOrWhiteSpace(code))
                {
                    return code;
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

            return "Block Entity";
        }

        private static bool TryGetBlockEntityInventory(BlockEntity? blockEntity, out IInventory? inventory)
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

        private static int GetPreviewColor(string colorClass)
        {
            if (string.Equals(colorClass, SpellPreviewColorClasses.Harmful, StringComparison.OrdinalIgnoreCase))
            {
                return unchecked((int)0xFFFF544D);
            }

            if (string.Equals(colorClass, SpellPreviewColorClasses.Beneficial, StringComparison.OrdinalIgnoreCase))
            {
                return unchecked((int)0xFF58E56B);
            }

            return unchecked((int)0xFFC58A4A);
        }

        private static int GetTargetedWarningColor(string warningType)
        {
            if (string.Equals(warningType, SpellWarningTypes.Hostile, StringComparison.OrdinalIgnoreCase))
            {
                return unchecked((int)0xFFFF4B4B);
            }

            if (string.Equals(warningType, SpellWarningTypes.Beneficial, StringComparison.OrdinalIgnoreCase))
            {
                return unchecked((int)0xFF4BE06F);
            }

            return unchecked((int)0xFFC58A4A);
        }

        private void UpdateTargetedWarningVisuals()
        {
            if (activeTargetedWarnings.Count == 0)
            {
                return;
            }

            var now = capi.World?.ElapsedMilliseconds ?? 0;
            foreach (var entry in activeTargetedWarnings.ToArray())
            {
                var key = entry.Key;
                var warning = entry.Value;
                if (warning == null || !warning.IsActive || now >= warning.ExpiresAtMs)
                {
                    activeTargetedWarnings.Remove(key);
                    activeTargetedWarningRenderTimes.Remove(key);
                    continue;
                }

                if (!activeTargetedWarningRenderTimes.TryGetValue(key, out var lastRender) || now - lastRender >= 220)
                {
                    RenderTargetedWarning(warning);
                    activeTargetedWarningRenderTimes[key] = now;
                }
            }
        }

          private void RenderTargetedWarning(RustweaveTargetedWarningPacket packet)
          {
              if (packet == null || capi.World == null)
              {
                  return;
              }

              var color = GetTargetedWarningColor(packet.WarningType);
              var personal = packet.IsPersonalWarning;
              var center = ResolveWarningTargetCenter(packet);
              var origin = new Vec3d(packet.OriginX, packet.OriginY, packet.OriginZ);
              var phase = ((capi.World.ElapsedMilliseconds - packet.StartedAtMs) / 220d) * Math.PI * 2d;
              RenderWarningOrbit(center, personal ? 0.64d : 0.52d, color, personal ? 10 : 6, personal, phase, 0.52d);
              if (personal)
              {
                  RenderWarningOrbit(center, 0.28d, color, 4, true, -phase * 0.8d, 1.08d);
              }
              RenderWarningDirectionMarker(center, origin, color, personal);
          }

        private Vec3d ResolveWarningTargetCenter(RustweaveTargetedWarningPacket packet)
        {
            if (packet.TargetEntityId >= 0)
            {
                var targetEntity = capi.World?.GetEntityById(packet.TargetEntityId);
                if (targetEntity != null && targetEntity.Alive)
                {
                    return targetEntity.Pos.XYZ;
                }
            }

            return new Vec3d(packet.TargetX, packet.TargetY, packet.TargetZ);
        }

          private void RenderWarningOrbit(Vec3d center, double radius, int color, int points, bool personal, double phase, double heightOffset)
          {
              var count = Math.Max(4, points);
              for (var i = 0; i < count; i++)
              {
                  var angle = phase + ((Math.PI * 2d * i) / count);
                  var x = center.X + Math.Cos(angle) * radius;
                  var z = center.Z + Math.Sin(angle) * radius;
                  var y = center.Y + heightOffset + ((i % 2 == 0) ? 0.03d : -0.01d);
                  SpawnWarningParticle(new Vec3d(x, y, z), color, personal ? 0.03f : 0.024f, personal ? 0.34f : 0.28f);
              }
          }

          private void RenderWarningDirectionMarker(Vec3d center, Vec3d origin, int color, bool personal)
          {
              var direction = new Vec3d(origin.X - center.X, 0, origin.Z - center.Z);
              var length = Math.Sqrt((direction.X * direction.X) + (direction.Z * direction.Z));
            if (length <= 0.001d)
            {
                return;
            }

              direction.X /= length;
              direction.Z /= length;
              var offset = personal ? 1.15d : 0.9d;
              var height = center.Y + 0.15;
              for (var i = 0; i < 2; i++)
              {
                  var t = (i + 1) / 2d;
                  var x = center.X + (direction.X * offset * t);
                  var z = center.Z + (direction.Z * offset * t);
                  SpawnWarningParticle(new Vec3d(x, height + (t * 0.05d), z), color, personal ? 0.028f : 0.022f, personal ? 0.28f : 0.22f);
              }
          }

          private void SpawnWarningParticle(Vec3d pos, int color, float size, float lifeLength)
          {
              SpawnPreviewParticle(pos, color, Math.Min(size, 0.035f), Math.Min(lifeLength, 0.34f));
          }

        private void UpdateTargetPreviewVisuals()
        {
            if (activeTargetPreviews.Count == 0)
            {
                return;
            }

            var now = capi.World?.ElapsedMilliseconds ?? 0;
            foreach (var entry in activeTargetPreviews.ToArray())
            {
                var key = entry.Key;
                var preview = entry.Value;
                if (!preview.IsActive || now - preview.UpdatedAtMs > 1200)
                {
                    activeTargetPreviews.Remove(key);
                    activeTargetPreviewRenderTimes.Remove(key);
                    activeTargetPreviewSendTimes.Remove(key);
                    continue;
                }

                if ((currentCastState.IsCasting || currentTargetLock.IsActive) && !preview.IsLocked)
                {
                    continue;
                }

                if (!activeTargetPreviewRenderTimes.TryGetValue(key, out var lastRender) || now - lastRender >= 200)
                {
                    RenderTargetPreview(preview);
                    activeTargetPreviewRenderTimes[key] = now;
                }
            }
        }

        private bool HydrateFromSavedState()
        {
            var player = capi.World?.Player;
            if (player == null)
            {
                return false;
            }

            if (!RustweaveStateService.TryGetClientState(player, out var syncedState))
            {
                return false;
            }

            var serialized = RustweaveStateService.SerializePlayerState(syncedState);
            if (string.Equals(serialized, lastStateJson, StringComparison.Ordinal))
            {
                return true;
            }

                lastStateJson = serialized;
                currentState = syncedState;
                ApplyLocalPreparedSelection();
            capi.Logger.Debug("[TheRustweave] Prepared slot state loaded: {0} slots, active slot {1}.", currentState.PreparedSpellCodes.Count, RustweaveStateService.DescribePreparedSlot(currentState.SelectedPreparedSpellIndex));
                corruptionHud?.SetState(currentState);
                prepDialog?.SetState(currentState);
                return true;
        }

        private void OnMouseWheelMove(MouseWheelEventArgs args)
        {
            HydrateFromSavedState();
            var player = capi.World.Player;
            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                return;
            }

            if (!RustweaveStateService.IsHoldingTome(player))
            {
                return;
            }

            if (player.Entity?.Controls?.ShiftKey != true)
            {
                return;
            }

            if (capi.OpenedGuis.OfType<GuiDialog>().Any(gui => gui != corruptionHud && gui != castHud && gui != prepDialog && gui.DialogType != EnumDialogType.HUD))
            {
                return;
            }

            var delta = args.delta > 0 ? 1 : args.delta < 0 ? -1 : 0;
            if (delta == 0)
            {
                return;
            }

            var nextSlot = RustweaveStateService.CycleSelection(currentState, delta);
            if (nextSlot < 0)
            {
                return;
            }

            RequestSelectPreparedSpell(nextSlot);
            capi.ShowChatMessage(Lang.Get("game:rustweave-selected-slot", RustweaveStateService.ToDisplaySlotNumber(nextSlot), RustweaveStateService.GetPreparedSlotDisplayText(currentState, nextSlot)));
            args.SetHandled(true);
        }

        private void OnClientTick(float dt)
        {
            var player = capi.World.Player;
            if (player == null)
            {
                CloseAllDialogs();
                return;
            }

            if (!RustweaveStateService.IsRustweaver(player))
            {
                CloseAllDialogs();
                return;
            }

            RefreshSnapshots(player);
            UpdateCorruptionHud();
            UpdateCastHud();
            UpdateTargetLockHud();
            UpdateTargetPreviewVisuals();
            UpdateTargetedWarningVisuals();
            CancelCastIfGuiOpened();
            CancelCastIfHoldingWrongItem(player);
        }

        private void RefreshSnapshots(IClientPlayer player)
        {
            var stateJson = player.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedPlayerStateKey, string.Empty) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(stateJson) && !string.Equals(stateJson, lastStateJson, StringComparison.Ordinal))
            {
                lastStateJson = stateJson;
                if (RustweaveStateService.TryDeserializePlayerState(stateJson, out var syncedState))
                {
                    currentState = syncedState;
                    ApplyLocalPreparedSelection();
                }
                corruptionHud?.SetState(currentState);
                prepDialog?.SetState(currentState);
            }

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                HydrateFromSavedState();
            }

            var castJson = player.Entity?.WatchedAttributes?.GetString(RustweaveConstants.WatchedCastStateKey, string.Empty) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(castJson) && !string.Equals(castJson, lastCastJson, StringComparison.Ordinal))
            {
                lastCastJson = castJson;
                if (RustweaveStateService.TryGetClientCastState(player, out var syncedCastState))
                {
                    currentCastState = syncedCastState;
                }
                castHud?.SetState(currentCastState, currentState);
            }
        }

        private void UpdateCorruptionHud()
        {
            EnsureCorruptionHud();
            var hud = corruptionHud;
            if (hud == null)
            {
                return;
            }

            if (!hud.IsOpened())
            {
                hud.TryOpen();
            }
        }

        private int GetSelectedPreparedSlotIndex()
        {
            if (RustweaveStateService.IsValidSlotIndex(localSelectedPreparedSpellIndex))
            {
                return localSelectedPreparedSpellIndex;
            }

            return currentState.SelectedPreparedSpellIndex;
        }

        private void ApplyLocalPreparedSelection()
        {
            if (RustweaveStateService.IsValidSlotIndex(localSelectedPreparedSpellIndex))
            {
                currentState.SelectedPreparedSpellIndex = localSelectedPreparedSpellIndex;
            }
        }

        private void UpdateTargetLockHud()
        {
            return;
        }

        private bool ShouldShowPreviewTargetLockHud()
        {
            return false;
        }

        private void UpdateCastHud()
        {
            EnsureCastHud();
            var hud = castHud;
            if (hud == null)
            {
                return;
            }

            if (currentCastState == null || !currentCastState.IsCasting)
            {
                if (hud.IsOpened())
                {
                    hud.TryClose();
                }

                return;
            }

            if (!hud.IsOpened())
            {
                hud.TryOpen();
            }
        }

        private void CancelCastIfGuiOpened()
        {
            if (!currentCastState.IsCasting)
            {
                return;
            }

            if (capi.OpenedGuis.OfType<GuiDialog>().Any(gui => gui != corruptionHud && gui != castHud && gui != targetLockHud && gui != prepDialog && gui.DialogType != EnumDialogType.HUD))
            {
                RequestCancelCast();
            }
        }

        private void CancelCastIfHoldingWrongItem(IClientPlayer player)
        {
            if (!currentCastState.IsCasting)
            {
                return;
            }

            if (!RustweaveStateService.IsHoldingTome(player))
            {
                RequestCancelCast();
            }
        }

        private void CloseAllDialogs()
        {
            currentTargetLock = new RustweaveTargetLockPacket();
            ClearTargetedWarnings();
            corruptionHud?.TryClose();
            castHud?.TryClose();
            prepDialog?.TryClose();
        }

        private void ClearTargetedWarnings()
        {
            activeTargetedWarnings.Clear();
            activeTargetedWarningRenderTimes.Clear();
        }

        private void EnsureCorruptionHud()
        {
            if (corruptionHud != null)
            {
                return;
            }

            corruptionHud = new RustweaveCorruptionHud(capi);
        }

        private void EnsureCastHud()
        {
            if (castHud != null)
            {
                return;
            }

            castHud = new RustweaveCastHud(capi);
        }

        private void EnsureTargetLockHud()
        {
            if (targetLockHud != null)
            {
                return;
            }

            targetLockHud = new RustweaveTargetLockHud(capi);
        }

        private void EnsurePrepDialog()
        {
            if (prepDialog != null)
            {
                return;
            }

            prepDialog = new RustweaveSpellPrepDialog(capi);
        }
    }
}

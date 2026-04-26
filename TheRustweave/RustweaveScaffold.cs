using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ProtoBuf;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using SpellRegistryType = TheRustweave.SpellRegistry;

namespace TheRustweave
{
    internal static class RustweaveConstants
    {
        public const string ModId = "therustweave";
        public const string NetworkChannelName = "therustweave";
        public const string ProgressionConfigFileName = "therustweave-progression.json";
        public const string DiscoveryConfigFileName = "therustweave-discovery.json";
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
                DiscoveryItemUseCounts = DiscoveryItemUseCounts?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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
            };
        }
    }

    internal static class RustweaveRuntime
    {
        public static ICoreAPI? CommonApi { get; private set; }

        public static ICoreServerAPI? ServerApi { get; private set; }

        public static ICoreClientAPI? ClientApi { get; private set; }

        public static RustweaveProgressionConfig ProgressionConfig { get; private set; } = new();

        public static RustweaveDiscoveryConfig DiscoveryConfig { get; private set; } = new();

        public static SpellRegistryType SpellRegistry { get; private set; } = SpellRegistryType.CreateFallback();

        public static RustweaveServerController? Server { get; private set; }

        public static RustweaveClientController? Client { get; private set; }

        public static void LoadSpellRegistry(ICoreAPI api)
        {
            CommonApi = api;
            LoadProgressionConfig(api);
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
        private readonly Dictionary<string, Dictionary<string, long>> spellCooldowns = new(StringComparer.OrdinalIgnoreCase);
        private IServerNetworkChannel? channel;
        private long tickListenerId;
        private long nextTabletDecayScanMilliseconds;

        public RustweaveServerController(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
            spellExecutor = new SpellEffectExecutor(sapi);
        }

        public void Start()
        {
            channel = sapi.Network.RegisterChannel(RustweaveConstants.NetworkChannelName);
            channel.RegisterMessageType(typeof(RustweaveActionPacket));
            channel.SetMessageHandler<RustweaveActionPacket>(OnClientPacket);
            sapi.Event.PlayerJoin += OnServerPlayerJoin;
            sapi.Event.PlayerNowPlaying += OnServerPlayerNowPlaying;
            sapi.Event.PlayerLeave += OnServerPlayerLeave;
            RegisterCommands();
            tickListenerId = sapi.Event.RegisterGameTickListener(OnServerTick, 50, 50);
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
            ScheduleTabletDecayScan(player, 1500);
        }

        private void OnServerPlayerLeave(IServerPlayer player)
        {
            if (player == null)
            {
                return;
            }

            activeCasts.Remove(player.PlayerUID);
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
                    if (RustweaveStateService.TrySelectPreparedSpell(state, packet.SlotIndex))
                    {
                        sapi.Logger.Debug("[TheRustweave] Active prepared slot changed to {0} for player '{1}'.", packet.SlotIndex, fromPlayer.PlayerUID);
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-selected-spell", RustweaveStateService.GetPreparedSpellCode(state, state.SelectedPreparedSpellIndex)), EnumChatType.Notification, null);
                    }
                    break;
                case RustweaveActionType.RequestPrepareSpell:
                    sapi.Logger.Debug("[TheRustweave] Prepare request received from player '{0}' for spell '{1}' (requested slot {2}).", fromPlayer.PlayerUID, packet.SpellCode, packet.SlotIndex);

                    if (string.IsNullOrWhiteSpace(packet.SpellCode) || !RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(packet.SpellCode, out var spell) || spell == null || !RustweaveStateService.IsSpellLearned(spell.Code, state))
                    {
                        sapi.Logger.Warning("[TheRustweave] Prepare request rejected for player '{0}' because spell '{1}' is unavailable or locked.", fromPlayer.PlayerUID, packet.SpellCode);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-unlearned"), EnumChatType.Notification, null);
                        break;
                    }

                    var chosenSlot = RustweaveStateService.ResolvePrepareTargetSlot(state, packet.SlotIndex);
                    sapi.Logger.Debug("[TheRustweave] Prepare target slot resolved to {0} for player '{1}'.", chosenSlot, fromPlayer.PlayerUID);

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
                        sapi.Logger.Debug("[TheRustweave] Stored spell '{0}' in prepared slot {1} for player '{2}' (before='{3}', after='{4}').", spell.Code, chosenSlot, fromPlayer.PlayerUID, beforeValue, afterValue);
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-prepared", RustweaveStateService.GetSpellDisplayName(spell.Code)), EnumChatType.Notification, null);
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
                        sapi.Logger.Debug("[TheRustweave] Prepared slot {0} cleared for player '{1}'.", packet.SlotIndex, fromPlayer.PlayerUID);
                        SaveAndSyncState(fromPlayer, state);
                    }
                    break;
            }
        }

        private void OnServerTick(float dt)
        {
            ProcessPendingTabletDecayScans();
            ProcessPassiveTabletDecay();
            ProcessTimedSpellEffects();

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
                    activeTabletVents.Remove(serverPlayer.PlayerUID);
                    continue;
                }

                var state = GetState(serverPlayer);
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

        private void TriggerOverloadExplosion(IServerPlayer serverPlayer)
        {
            activeCasts.Remove(serverPlayer.PlayerUID);
            activeTabletVents.Remove(serverPlayer.PlayerUID);

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
            sapi.Logger.Debug("[TheRustweave] Cast attempted from active prepared slot {0} for player '{1}' (spell '{2}').", slotIndex, player.PlayerUID, spellCode);
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-spell-prepared"), EnumChatType.Notification, null);
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

            if (!spellExecutor.TryBuildPlan(player, state, spell, out var previewPlan, out var previewFailureReason))
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
                StartingTemporalCorruption = state.CurrentTemporalCorruption
            };

            activeCasts[player.PlayerUID] = castState;
            SyncCastState(player, castState);
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

            if (TryGetCooldownRemaining(player.PlayerUID, spell.Code, out var remainingMilliseconds))
            {
                sapi.Logger.Warning("[TheRustweave] Spell '{0}' was rejected at completion for player '{1}' because it is on cooldown for {2} ms.", spell.Code, player.PlayerUID, remainingMilliseconds);
                player.SendMessage(0, Lang.Get("game:rustweave-spell-cooldown", spell.Name, RustweaveStateService.FormatSeconds(remainingMilliseconds / 1000d)), EnumChatType.Notification, null);
                ClearCastState(player);
                return;
            }

            if (!spellExecutor.TryBuildPlan(player, state, spell, out var executionPlan, out var failureReason))
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
            if (player.Entity?.WatchedAttributes == null)
            {
                return;
            }

            var castState = new RustweaveCastStateData();
            RustweaveStateService.SyncWatchedCastState(player, castState);
        }

        private void SyncCastState(IPlayer player, RustweaveCastStateData state)
        {
            if (player.Entity == null)
            {
                return;
            }

            RustweaveStateService.SyncWatchedCastState(player, state.Clone());
        }
    }

    internal sealed class RustweaveClientController
    {
        private readonly ICoreClientAPI capi;
        private IClientNetworkChannel? channel;
        private long tickListenerId;
        private RustweavePlayerStateData currentState = RustweaveStateService.CreateFreshState();
        private RustweaveCastStateData currentCastState = new();
        private string lastStateJson = string.Empty;
        private string lastCastJson = string.Empty;
        private RustweaveCorruptionHud? corruptionHud;
        private RustweaveCastHud? castHud;
        private RustweaveSpellPrepDialog? prepDialog;

        public RustweaveClientController(ICoreClientAPI capi)
        {
            this.capi = capi;
        }

        public void Start()
        {
            channel = capi.Network.RegisterChannel(RustweaveConstants.NetworkChannelName);
            channel.RegisterMessageType(typeof(RustweaveActionPacket));
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

        public void RequestStartCast()
        {
            HydrateFromSavedState();
            if (currentState.CurrentTemporalCorruption >= currentState.EffectiveTemporalCorruptionThreshold)
            {
                capi.ShowChatMessage(Lang.Get("game:rustweave-cast-locked"));
                return;
            }

            capi.Logger.Debug("[TheRustweave] Cast requested from active slot {0}.", currentState.SelectedPreparedSpellIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestStartCast,
                SlotIndex = currentState.SelectedPreparedSpellIndex
            });
        }

        public void RequestCancelCast()
        {
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestCancelCast
            });
        }

        public void RequestSelectPreparedSpell(int slotIndex)
        {
            if (RustweaveStateService.IsValidSlotIndex(slotIndex))
            {
                currentState.SelectedPreparedSpellIndex = slotIndex;
                prepDialog?.SetState(currentState);
            }

            capi.Logger.Debug("[TheRustweave] Client requested prepared slot selection: {0}.", slotIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestSelectPrepared,
                SlotIndex = slotIndex
            });
        }

        public void RequestPrepareSpell(string spellCode, int targetSlotIndex)
        {
            capi.Logger.Debug("[TheRustweave] Client requested prepare for spell '{0}' with target slot {1}.", spellCode, targetSlotIndex);
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestPrepareSpell,
                SpellCode = spellCode,
                SlotIndex = targetSlotIndex
            });
        }

        public void RequestUnprepareSpell(int slotIndex)
        {
            if (RustweaveStateService.IsValidSlotIndex(slotIndex) && slotIndex < currentState.PreparedSpellCodes.Count)
            {
                currentState.PreparedSpellCodes[slotIndex] = string.Empty;
                prepDialog?.SetState(currentState);
            }

            capi.Logger.Debug("[TheRustweave] Client requested clear for prepared slot {0}.", slotIndex);
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
            capi.Logger.Debug("[TheRustweave] Prepared slot state loaded: {0} slots, active slot {1}.", currentState.PreparedSpellCodes.Count, currentState.SelectedPreparedSpellIndex);
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
            capi.ShowChatMessage(Lang.Get("game:rustweave-selected-spell", RustweaveStateService.GetPreparedSpellCode(currentState, nextSlot)));
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

            if (capi.OpenedGuis.OfType<GuiDialog>().Any(gui => gui != corruptionHud && gui != castHud && gui != prepDialog && gui.DialogType != EnumDialogType.HUD))
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
            corruptionHud?.TryClose();
            castHud?.TryClose();
            prepDialog?.TryClose();
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

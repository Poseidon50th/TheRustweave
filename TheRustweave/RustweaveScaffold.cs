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
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace TheRustweave
{
    internal static class RustweaveConstants
    {
        public const string ModId = "therustweave";
        public const string NetworkChannelName = "therustweave";
        public const string TomeItemCode = "rustweaverstome";
        public const string TomeItemClass = "ItemRustweaverTome";
        public const string TabletItemCode = "rusttablet";
        public const string TabletItemClass = "ItemRustTablet";
        public const string SpellRegistryAsset = "therustweave:config/spells.json";
        public const string PlayerStateModDataKey = "therustweave:playerstate";
        public const string CastStateModDataKey = "therustweave:caststate";
        public const string WatchedPlayerStateKey = "therustweave:playerstate";
        public const string WatchedCastStateKey = "therustweave:caststate";
        public const string TabletStoredCorruptionKey = "therustweave:tabletcorruption";
        public const string TabletLastDecayTotalDaysKey = "therustweave:tabletdecaydays";
        public const string DummySpellCode = "dummy-rustcall";
        public const int DefaultCorruption = 0;
        public const int DefaultThreshold = 200;
        public const int DefaultCap = 1000;
        public const int TabletCapacity = 400;
        public const int TabletVentingRatePerSecond = 10;
        public const int TabletDecayPerDay = 4;
        public const long OverloadDurationMilliseconds = 120000;
        public const int PreparedSlotCount = 9;
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
        public int CurrentTemporalCorruption { get; set; } = RustweaveConstants.DefaultCorruption;

        public int EffectiveTemporalCorruptionThreshold { get; set; } = RustweaveConstants.DefaultThreshold;

        public int AbsoluteTemporalCorruptionCap { get; set; } = RustweaveConstants.DefaultCap;

        public float CastSpeedMultiplier { get; set; } = 1f;

        public List<string> LearnedSpellCodes { get; set; } = new();

        public List<string> PreparedSpellCodes { get; set; } = new();

        public int SelectedPreparedSpellIndex { get; set; } = -1;

        public long TemporalOverloadStartedAtMilliseconds { get; set; }

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
                LearnedSpellCodes = LearnedSpellCodes.ToList(),
                PreparedSpellCodes = PreparedSpellCodes.ToList(),
                SelectedPreparedSpellIndex = SelectedPreparedSpellIndex,
                TemporalOverloadStartedAtMilliseconds = TemporalOverloadStartedAtMilliseconds
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

        public static RustweaveSpellRegistry SpellRegistry { get; private set; } = RustweaveSpellRegistry.CreateFallback();

        public static RustweaveServerController? Server { get; private set; }

        public static RustweaveClientController? Client { get; private set; }

        public static void LoadSpellRegistry(ICoreAPI api)
        {
            CommonApi = api;
            SpellRegistry = RustweaveSpellRegistry.Load(api);
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
            var starterSpellCode = RustweaveRuntime.SpellRegistry.StarterSpellCode;
            var state = new RustweavePlayerStateData
            {
                CurrentTemporalCorruption = RustweaveConstants.DefaultCorruption,
                EffectiveTemporalCorruptionThreshold = RustweaveConstants.DefaultThreshold,
                AbsoluteTemporalCorruptionCap = RustweaveConstants.DefaultCap,
                CastSpeedMultiplier = 1f,
                LearnedSpellCodes = new List<string> { starterSpellCode },
                PreparedSpellCodes = new List<string>(RustweaveConstants.PreparedSlotCount)
                {
                    starterSpellCode
                },
                SelectedPreparedSpellIndex = 0
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
            state.PreparedSpellCodes ??= new List<string>();

            var normalizedLearned = new List<string>();
            var learnedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var code in state.LearnedSpellCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                if (!RustweaveRuntime.SpellRegistry.TryGetSpell(code, out _))
                {
                    continue;
                }

                if (!learnedCodes.Add(code))
                {
                    continue;
                }

                normalizedLearned.Add(code);
            }

            if (normalizedLearned.Count == 0)
            {
                normalizedLearned.Add(RustweaveRuntime.SpellRegistry.StarterSpellCode);
            }

            state.LearnedSpellCodes = normalizedLearned;

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

                if (!state.LearnedSpellCodes.Any(learned => string.Equals(learned, code, StringComparison.OrdinalIgnoreCase)) || !usedSpellCodes.Add(code))
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
            state.SelectedPreparedSpellIndex = NormalizeSelectedIndex(state.SelectedPreparedSpellIndex, state.PreparedSpellCodes);

            if (state.SelectedPreparedSpellIndex < 0)
            {
                state.SelectedPreparedSpellIndex = GetFirstPreparedSlotIndex(state.PreparedSpellCodes);
            }

            return state;
        }

        public static int NormalizeSelectedIndex(int selectedIndex, IReadOnlyList<string> preparedSpellCodes)
        {
            if (selectedIndex >= 0 && selectedIndex < preparedSpellCodes.Count && !string.IsNullOrWhiteSpace(preparedSpellCodes[selectedIndex]))
            {
                return selectedIndex;
            }

            return GetFirstPreparedSlotIndex(preparedSpellCodes);
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

        public static bool TryPrepareSpell(RustweavePlayerStateData state, string spellCode, int targetSlotIndex)
        {
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return false;
            }

            if (!state.LearnedSpellCodes.Any(code => string.Equals(code, spellCode, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out _))
            {
                return false;
            }

            var existingSlot = FindPreparedSpellSlot(state, spellCode);
            if (existingSlot >= 0)
            {
                state.SelectedPreparedSpellIndex = existingSlot;
                return true;
            }

            var slotIndex = IsValidSlotIndex(targetSlotIndex) && string.IsNullOrWhiteSpace(state.PreparedSpellCodes[targetSlotIndex])
                ? targetSlotIndex
                : GetFirstEmptyPreparedSlotIndex(state.PreparedSpellCodes);

            if (slotIndex < 0)
            {
                return false;
            }

            state.PreparedSpellCodes[slotIndex] = spellCode;
            state.SelectedPreparedSpellIndex = slotIndex;
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

            if (state.SelectedPreparedSpellIndex == slotIndex)
            {
                state.SelectedPreparedSpellIndex = NormalizeSelectedIndex(state.SelectedPreparedSpellIndex, state.PreparedSpellCodes);
            }

            return true;
        }

        public static bool TrySelectPreparedSpell(RustweavePlayerStateData state, int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(state.PreparedSpellCodes[slotIndex]))
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
        private readonly Dictionary<string, RustweaveCastStateData> activeCasts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RustTabletVentingSession> activeTabletVents = new(StringComparer.OrdinalIgnoreCase);
        private IServerNetworkChannel? channel;
        private long tickListenerId;
        private long nextTabletDecayScanMilliseconds;

        public RustweaveServerController(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public void Start()
        {
            channel = sapi.Network.RegisterChannel(RustweaveConstants.NetworkChannelName);
            channel.RegisterMessageType(typeof(RustweaveActionPacket));
            channel.SetMessageHandler<RustweaveActionPacket>(OnClientPacket);
            sapi.Event.PlayerJoin += OnServerPlayerJoin;
            sapi.Event.PlayerNowPlaying += OnServerPlayerNowPlaying;
            sapi.Event.PlayerLeave += OnServerPlayerLeave;
            tickListenerId = sapi.Event.RegisterGameTickListener(OnServerTick, 50, 50);
        }

        private sealed class RustTabletVentingSession
        {
            public long StartedAtMilliseconds { get; set; }

            public int TransferredCorruption { get; set; }
        }

        private void OnServerPlayerJoin(IServerPlayer player)
        {
            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                return;
            }

            GetState(player);
            ProcessTabletDecayForInventories(player);
        }

        private void OnServerPlayerNowPlaying(IServerPlayer player)
        {
            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                return;
            }

            var state = GetState(player);
            SaveAndSyncState(player, state);
            ProcessTabletDecayForInventories(player);
        }

        private void OnServerPlayerLeave(IServerPlayer player)
        {
            if (player == null)
            {
                return;
            }

            activeCasts.Remove(player.PlayerUID);
            activeTabletVents.Remove(player.PlayerUID);
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
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-selected-spell", RustweaveStateService.GetPreparedSpellCode(state, state.SelectedPreparedSpellIndex)), EnumChatType.Notification, null);
                    }
                    break;
                case RustweaveActionType.RequestPrepareSpell:
                    if (RustweaveStateService.TryPrepareSpell(state, packet.SpellCode, packet.SlotIndex))
                    {
                        SaveAndSyncState(fromPlayer, state);
                        fromPlayer.SendMessage(0, Lang.Get("game:rustweave-spell-prepared", RustweaveStateService.GetSpellDisplayName(packet.SpellCode)), EnumChatType.Notification, null);
                    }
                    break;
                case RustweaveActionType.RequestUnprepareSpell:
                    if (RustweaveStateService.TryUnprepareSpell(state, packet.SlotIndex))
                    {
                        SaveAndSyncState(fromPlayer, state);
                    }
                    break;
            }
        }

        private void OnServerTick(float dt)
        {
            ProcessPassiveTabletDecay();

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
                if (!RustweaveStateService.IsHoldingTome(serverPlayer))
                {
                    CancelCast(serverPlayer, state, Lang.Get("game:rustweave-cast-cancel"), true, true);
                    continue;
                }

                var progress = castState.DurationMilliseconds <= 0
                    ? 1d
                    : Math.Clamp((double)castState.ElapsedMilliseconds / castState.DurationMilliseconds, 0d, 1d);

                var corruptionTarget = Math.Min(
                    state.EffectiveTemporalCorruptionThreshold,
                    castState.StartingTemporalCorruption + (int)Math.Floor(castState.CorruptionCost * progress));

                if (state.CurrentTemporalCorruption < corruptionTarget)
                {
                    state.CurrentTemporalCorruption = corruptionTarget;
                    SaveAndSyncState(serverPlayer, state);
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

            foreach (var onlinePlayer in sapi.World.AllOnlinePlayers.OfType<IServerPlayer>())
            {
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
            if (inventory == null || world?.Calendar == null)
            {
                return;
            }

            for (var slotIndex = 0; slotIndex < inventory.Count; slotIndex++)
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

        private IServerPlayer? GetOnlineServerPlayer(string playerUid)
        {
            return sapi.World.AllOnlinePlayers.OfType<IServerPlayer>().FirstOrDefault(player => string.Equals(player.PlayerUID, playerUid, StringComparison.OrdinalIgnoreCase));
        }

        private void StartCast(IServerPlayer player, RustweavePlayerStateData state, int slotIndex)
        {
            if (state.CurrentTemporalCorruption >= state.EffectiveTemporalCorruptionThreshold)
            {
                player.SendMessage(0, Lang.Get("game:rustweave-cast-locked"), EnumChatType.Notification, null);
                return;
            }

            slotIndex = RustweaveStateService.NormalizeSelectedIndex(slotIndex, state.PreparedSpellCodes);
            if (!RustweaveStateService.IsValidSlotIndex(slotIndex))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-spell-prepared"), EnumChatType.Notification, null);
                return;
            }

            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                player.SendMessage(0, Lang.Get("game:rustweave-no-spell-prepared"), EnumChatType.Notification, null);
                return;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out var spell) || spell == null)
            {
                player.SendMessage(0, Lang.Get("game:rustweave-cast-fail"), EnumChatType.Notification, null);
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

            var finalCorruption = Math.Min(state.EffectiveTemporalCorruptionThreshold, castState.StartingTemporalCorruption + spell.CorruptionCost);
            if (state.CurrentTemporalCorruption < finalCorruption)
            {
                state.CurrentTemporalCorruption = finalCorruption;
            }

            SaveAndSyncState(player, state);
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
            prepDialog!.SetState(currentState);
            prepDialog.TryOpen();
        }

        public void RequestStartCast()
        {
            HydrateFromSavedState();
            if (currentState.CurrentTemporalCorruption >= currentState.EffectiveTemporalCorruptionThreshold)
            {
                capi.ShowChatMessage(Lang.Get("game:rustweave-cast-locked"));
                return;
            }

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
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestSelectPrepared,
                SlotIndex = slotIndex
            });
        }

        public void RequestPrepareSpell(string spellCode, int targetSlotIndex)
        {
            SendPacket(new RustweaveActionPacket
            {
                Action = RustweaveActionType.RequestPrepareSpell,
                SpellCode = spellCode,
                SlotIndex = targetSlotIndex
            });
        }

        public void RequestUnprepareSpell(int slotIndex)
        {
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

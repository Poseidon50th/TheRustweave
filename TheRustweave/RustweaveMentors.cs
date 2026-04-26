using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace TheRustweave
{
    internal static class RustweaveMentorTypes
    {
        public const string RustplaneSage = "rustplane-sage";
        public const string RustplaneSageDisplayName = "Rustplane Sage";
        public const string RustplaneSageStudySource = "mentor:rustplane-sage";
    }

    internal sealed class RustweaveMentorTierCostConfig
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; } = "game:gear-temporal";

        [JsonProperty("quantity")]
        public int Quantity { get; set; } = 1;

        public RustweaveMentorTierCostConfig Clone()
        {
            return new RustweaveMentorTierCostConfig
            {
                ItemCode = ItemCode,
                Quantity = Quantity
            };
        }
    }

    internal sealed class RustweaveMentorConfig
    {
        [JsonProperty("enableRustplaneSageMentors")]
        public bool EnableRustplaneSageMentors { get; set; } = true;

        [JsonProperty("allowMentorsToBypassLootRarity")]
        public bool AllowMentorsToBypassLootRarity { get; set; } = true;

        [JsonProperty("defaultStudyHoursByTier")]
        public Dictionary<int, double> DefaultStudyHoursByTier { get; set; } = new()
        {
            [1] = 6d,
            [2] = 12d,
            [3] = 24d,
            [4] = 48d,
            [5] = 72d,
            [6] = 120d
        };

        [JsonProperty("costsByTier")]
        public Dictionary<int, RustweaveMentorTierCostConfig> CostsByTier { get; set; } = new()
        {
            [1] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 1 },
            [2] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 2 },
            [3] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 3 },
            [4] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 4 },
            [5] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 5 },
            [6] = new RustweaveMentorTierCostConfig { ItemCode = "game:gear-temporal", Quantity = 6 }
        };
    }

    internal sealed class RustweaveMentorCostEntry
    {
        [JsonProperty("itemCode")]
        public string ItemCode { get; set; } = string.Empty;

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        public RustweaveMentorCostEntry Clone()
        {
            return new RustweaveMentorCostEntry
            {
                ItemCode = ItemCode,
                Quantity = Quantity
            };
        }
    }

    internal sealed class RustweaveMentorStudyData
    {
        [JsonProperty("spellCode")]
        public string SpellCode { get; set; } = string.Empty;

        [JsonProperty("mentorType")]
        public string MentorType { get; set; } = string.Empty;

        [JsonProperty("startTime")]
        public double StartTime { get; set; }

        [JsonProperty("finishTime")]
        public double FinishTime { get; set; }

        [JsonProperty("paidCost")]
        public List<RustweaveMentorCostEntry> PaidCost { get; set; } = new();

        public RustweaveMentorStudyData Clone()
        {
            return new RustweaveMentorStudyData
            {
                SpellCode = SpellCode,
                MentorType = MentorType,
                StartTime = StartTime,
                FinishTime = FinishTime,
                PaidCost = PaidCost?.Where(entry => entry != null).Select(entry => entry.Clone()).ToList() ?? new List<RustweaveMentorCostEntry>()
            };
        }
    }

    internal static class RustweaveMentorService
    {
        public static bool IsEnabled()
        {
            return RustweaveRuntime.MentorConfig?.EnableRustplaneSageMentors ?? true;
        }

        public static IReadOnlyList<SpellDefinition> GetTeachableSpells(IPlayer? player)
        {
            if (player == null || !IsEnabled() || !RustweaveStateService.IsRustweaver(player))
            {
                return Array.Empty<SpellDefinition>();
            }

            var teachables = RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null
                    && !string.IsNullOrWhiteSpace(spell.Code)
                    && spell.Enabled
                    && !RustweaveStateService.IsHiddenSpell(spell)
                    && !RustweaveStateService.IsLoreweaveSpell(spell)
                    && !RustweaveStateService.IsSpellLearned(player, spell.Code))
                .OrderBy(spell => spell.Tier)
                .ThenBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return teachables;
        }

        public static bool TryValidateMentorSpell(IPlayer? player, string spellCode, out SpellDefinition? spell, out string failureReason)
        {
            spell = null;
            failureReason = string.Empty;

            if (!IsEnabled())
            {
                failureReason = "The Rustplane Sage is not available right now.";
                return false;
            }

            if (player == null || !RustweaveStateService.IsRustweaver(player))
            {
                failureReason = "Only Rustweavers may study with the Sage.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(spellCode) || !RustweaveRuntime.SpellRegistry.TryGetEnabledSpell(spellCode, out spell) || spell == null)
            {
                failureReason = "The Sage cannot teach that working.";
                return false;
            }

            if (RustweaveStateService.IsHiddenSpell(spell) || RustweaveStateService.IsLoreweaveSpell(spell))
            {
                failureReason = "The Sage cannot teach that working.";
                return false;
            }

            if (RustweaveStateService.IsSpellLearned(player, spell.Code))
            {
                failureReason = "You already know this spell.";
                return false;
            }

            return true;
        }

        public static double GetStudyHoursForTier(int tier)
        {
            var config = RustweaveRuntime.MentorConfig?.DefaultStudyHoursByTier;
            if (config != null && config.TryGetValue(tier, out var hours) && hours > 0d)
            {
                return hours;
            }

            return tier switch
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

        public static RustweaveMentorTierCostConfig? GetCostForTier(int tier)
        {
            var config = RustweaveRuntime.MentorConfig?.CostsByTier;
            if (config != null && config.TryGetValue(tier, out var cost) && cost != null && !string.IsNullOrWhiteSpace(cost.ItemCode) && cost.Quantity > 0)
            {
                return cost.Clone();
            }

            return new RustweaveMentorTierCostConfig
            {
                ItemCode = "game:gear-temporal",
                Quantity = Math.Max(1, tier)
            };
        }

        public static IReadOnlyList<RustweaveMentorCostEntry> BuildCostEntries(int tier)
        {
            var cost = GetCostForTier(tier);
            if (cost == null)
            {
                return Array.Empty<RustweaveMentorCostEntry>();
            }

            return new[]
            {
                new RustweaveMentorCostEntry
                {
                    ItemCode = cost.ItemCode,
                    Quantity = cost.Quantity
                }
            };
        }

        public static string FormatCostEntries(IReadOnlyList<RustweaveMentorCostEntry> costEntries)
        {
            if (costEntries == null || costEntries.Count == 0)
            {
                return "none";
            }

            return string.Join(", ", costEntries.Select(entry => $"{entry.Quantity}x {entry.ItemCode}"));
        }

        public static string FormatStudyTime(double hours)
        {
            return hours.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string FormatStudyStatus(RustweaveMentorStudyData study, double currentHours)
        {
            var remainingHours = Math.Max(0d, study.FinishTime - currentHours);
            return $"{RustweaveStateService.GetSpellDisplayName(study.SpellCode)} - {FormatStudyTime(remainingHours)}h remaining";
        }

        public static bool HasCost(IServerPlayer player, IReadOnlyList<RustweaveMentorCostEntry> costEntries)
        {
            if (player?.InventoryManager?.InventoriesOrdered == null || costEntries == null || costEntries.Count == 0)
            {
                return false;
            }

            var required = costEntries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.ItemCode) && entry.Quantity > 0)
                .GroupBy(entry => entry.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Quantity), StringComparer.OrdinalIgnoreCase);

            if (required.Count == 0)
            {
                return false;
            }

            var available = CountItems(player, required.Keys.ToList());
            return required.All(pair => available.TryGetValue(pair.Key, out var count) && count >= pair.Value);
        }

        public static bool ConsumeCost(IServerPlayer player, IReadOnlyList<RustweaveMentorCostEntry> costEntries)
        {
            if (player?.InventoryManager?.InventoriesOrdered == null || costEntries == null || costEntries.Count == 0)
            {
                return false;
            }

            var required = costEntries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.ItemCode) && entry.Quantity > 0)
                .GroupBy(entry => entry.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Quantity), StringComparer.OrdinalIgnoreCase);

            if (required.Count == 0)
            {
                return false;
            }

            if (!HasCost(player, costEntries))
            {
                return false;
            }

            foreach (var pair in required.ToList())
            {
                var remaining = pair.Value;
                foreach (var inventory in player.InventoryManager.InventoriesOrdered)
                {
                    if (inventory == null || remaining <= 0)
                    {
                        continue;
                    }

                    if (!TryGetInventoryCount(inventory, out var inventoryCount))
                    {
                        continue;
                    }

                    for (var slotIndex = 0; slotIndex < inventoryCount && remaining > 0; slotIndex++)
                    {
                        var slot = inventory[slotIndex];
                        if (slot == null)
                        {
                            continue;
                        }

                        var stack = slot.Itemstack;
                        if (stack?.Collectible?.Code == null)
                        {
                            continue;
                        }

                        var collectibleCode = stack.Collectible?.Code;
                        if (collectibleCode == null)
                        {
                            continue;
                        }

                        var stackCode = collectibleCode.ToString();
                        if (!string.Equals(stackCode, pair.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var removed = Math.Min(remaining, stack.StackSize);
                        stack.StackSize -= removed;
                        remaining -= removed;

                        if (stack.StackSize <= 0)
                        {
                            slot.Itemstack = null;
                        }

                        slot.MarkDirty();
                        inventory.MarkSlotDirty(slotIndex);
                    }
                }

                if (remaining > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static List<RustweaveMentorStudyData> GetActiveStudies(RustweavePlayerStateData? state)
        {
            if (state?.MentorStudies == null)
            {
                return new List<RustweaveMentorStudyData>();
            }

            return state.MentorStudies
                .Where(study => study != null && !string.IsNullOrWhiteSpace(study.SpellCode) && !string.IsNullOrWhiteSpace(study.MentorType))
                .Select(study => study)
                .ToList();
        }

        public static bool NormalizeMentorStudies(RustweavePlayerStateData state)
        {
            if (state == null)
            {
                return false;
            }

            state.MentorStudies ??= new List<RustweaveMentorStudyData>();
            var originalCount = state.MentorStudies.Count;
            var normalized = new List<RustweaveMentorStudyData>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var changed = false;

            foreach (var study in state.MentorStudies)
            {
                if (study == null)
                {
                    changed = true;
                    continue;
                }

                study.SpellCode = (study.SpellCode ?? string.Empty).Trim();
                study.MentorType = (study.MentorType ?? string.Empty).Trim();
                study.PaidCost ??= new List<RustweaveMentorCostEntry>();
                study.PaidCost = study.PaidCost
                    .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.ItemCode) && entry.Quantity > 0)
                    .GroupBy(entry => entry.ItemCode, StringComparer.OrdinalIgnoreCase)
                    .Select(group => new RustweaveMentorCostEntry
                    {
                        ItemCode = group.Key,
                        Quantity = group.Sum(entry => entry.Quantity)
                    })
                    .ToList();

                if (string.IsNullOrWhiteSpace(study.SpellCode) || string.IsNullOrWhiteSpace(study.MentorType) || study.FinishTime < study.StartTime)
                {
                    changed = true;
                    continue;
                }

                var studyKey = $"{study.MentorType}|{study.SpellCode}";
                if (!seen.Add(studyKey))
                {
                    changed = true;
                    continue;
                }

                normalized.Add(study);
            }

            state.MentorStudies = normalized;
            return changed || normalized.Count != originalCount;
        }

        private static Dictionary<string, int> CountItems(IServerPlayer player, IReadOnlyList<string> itemCodes)
        {
            var counts = itemCodes.ToDictionary(code => code, _ => 0, StringComparer.OrdinalIgnoreCase);
            if (player?.InventoryManager?.InventoriesOrdered == null)
            {
                return counts;
            }

            foreach (var inventory in player.InventoryManager.InventoriesOrdered)
            {
                if (inventory == null)
                {
                    continue;
                }

                if (!TryGetInventoryCount(inventory, out var inventoryCount))
                {
                    continue;
                }

                for (var slotIndex = 0; slotIndex < inventoryCount; slotIndex++)
                {
                    var stack = inventory[slotIndex]?.Itemstack;
                    if (stack?.Collectible?.Code == null)
                    {
                        continue;
                    }

                    var stackCode = stack.Collectible.Code.ToString();
                    if (!counts.ContainsKey(stackCode))
                    {
                        continue;
                    }

                    counts[stackCode] += stack.StackSize;
                }
            }

            return counts;
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
    }
}

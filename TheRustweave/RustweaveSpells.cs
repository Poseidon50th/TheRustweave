using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace TheRustweave
{
    internal sealed class RustweaveSpellDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double CastTimeSeconds { get; set; }

        public int CorruptionCost { get; set; }

        public bool Enabled { get; set; } = true;

        public string School { get; set; } = string.Empty;

        public string TargetType { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;
    }

    internal sealed class RustweaveSpellRegistry
    {
        private readonly Dictionary<string, RustweaveSpellDefinition> byCode;

        public RustweaveSpellRegistry(IReadOnlyList<RustweaveSpellDefinition> spells)
        {
            Spells = spells;
            byCode = spells
                .Where(spell => !string.IsNullOrWhiteSpace(spell.Code))
                .ToDictionary(spell => spell.Code, StringComparer.OrdinalIgnoreCase);
            StarterSpellCode = spells.FirstOrDefault(spell => spell.Enabled)?.Code
                ?? spells.FirstOrDefault()?.Code
                ?? RustweaveConstants.DummySpellCode;
        }

        public IReadOnlyList<RustweaveSpellDefinition> Spells { get; }

        public IReadOnlyDictionary<string, RustweaveSpellDefinition> ByCode => byCode;

        public string StarterSpellCode { get; }

        public static RustweaveSpellRegistry CreateFallback()
        {
            return new RustweaveSpellRegistry(new[]
            {
                new RustweaveSpellDefinition
                {
                    Code = RustweaveConstants.DummySpellCode,
                    Name = "Dummy Rustcall",
                    Description = "A harmless placeholder spell used for scaffold testing.",
                    CastTimeSeconds = 2.5,
                    CorruptionCost = 25,
                    Enabled = true,
                    School = "scaffold",
                    TargetType = "self",
                    Icon = string.Empty
                }
            });
        }

        public static RustweaveSpellRegistry Load(ICoreAPI api)
        {
            try
            {
                var asset = api.Assets.TryGet(new AssetLocation(RustweaveConstants.SpellRegistryAsset));
                var loaded = asset?.ToObject<List<RustweaveSpellDefinition>>() ?? new List<RustweaveSpellDefinition>();
                var validated = Validate(api, loaded);

                if (validated.Count == 0)
                {
                    api.Logger.Warning("TheRustweave spell registry contained no valid entries. Falling back to the built-in dummy spell.");
                    return CreateFallback();
                }

                api.Logger.Notification("TheRustweave loaded {0} spell definition(s).", validated.Count);
                return new RustweaveSpellRegistry(validated);
            }
            catch (Exception exception)
            {
                api.Logger.Warning("TheRustweave failed to load spell registry, falling back to the built-in dummy spell: {0}", exception.Message);
                return CreateFallback();
            }
        }

        public bool TryGetSpell(string code, out RustweaveSpellDefinition? spell)
        {
            spell = null;

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return byCode.TryGetValue(code, out spell);
        }

        public bool TryGetEnabledSpell(string code, out RustweaveSpellDefinition? spell)
        {
            if (!TryGetSpell(code, out spell))
            {
                return false;
            }

            return spell != null && spell.Enabled;
        }

        private static List<RustweaveSpellDefinition> Validate(ICoreAPI api, IReadOnlyList<RustweaveSpellDefinition> loaded)
        {
            var validated = new List<RustweaveSpellDefinition>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < loaded.Count; index++)
            {
                var raw = loaded[index];
                if (raw == null)
                {
                    api.Logger.Warning("TheRustweave spell registry entry #{0} is null and was ignored.", index);
                    continue;
                }

                var code = (raw.Code ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(code))
                {
                    api.Logger.Warning("TheRustweave spell registry entry #{0} is missing a code and was ignored.", index);
                    continue;
                }

                if (!seenCodes.Add(code))
                {
                    api.Logger.Warning("TheRustweave spell registry code '{0}' was duplicated and the duplicate was ignored.", code);
                    continue;
                }

                if (raw.CastTimeSeconds < 0)
                {
                    api.Logger.Warning("TheRustweave spell '{0}' had a negative castTimeSeconds value and was ignored.", code);
                    continue;
                }

                if (raw.CorruptionCost < 0)
                {
                    api.Logger.Warning("TheRustweave spell '{0}' had a negative corruptionCost value and was ignored.", code);
                    continue;
                }

                raw.Code = code;
                raw.Name = (raw.Name ?? string.Empty).Trim();
                raw.Description = (raw.Description ?? string.Empty).Trim();
                raw.School = (raw.School ?? string.Empty).Trim();
                raw.TargetType = (raw.TargetType ?? string.Empty).Trim();
                raw.Icon = (raw.Icon ?? string.Empty).Trim();
                validated.Add(raw);
            }

            return validated;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace TheRustweave
{
    internal static class RustweaveHudLayout
    {
        public const float CorruptionHudWidth = 350f;
        public const float CorruptionHudHeight = 40f;
        public const float CastHudWidth = 260f;
        public const float CastHudHeight = 40f;
        public const float CastHudSpacing = 4f;

        public const float CorruptionOffsetX = 525f;
        public const float CorruptionOffsetY = -80f;

        public static Vec2d CorruptionOffset => new(CorruptionOffsetX, CorruptionOffsetY);

        public static Vec2d CastOffset => new(CorruptionOffsetX, CorruptionOffsetY - CorruptionHudHeight - CastHudSpacing);
    }

    internal sealed class RustweaveCorruptionHud : HudElement
    {
        private RustweavePlayerStateData state = RustweaveStateService.CreateDefaultState();
        private GuiElementDynamicText? corruptionLabel;

        public RustweaveCorruptionHud(ICoreClientAPI capi) : base(capi)
        {
        }

        public override bool Focusable => false;

        public override bool PrefersUngrabbedMouse => true;

        public override bool UnregisterOnClose => false;

        public override string ToggleKeyCombinationCode => string.Empty;

        public void SetState(RustweavePlayerStateData newState)
        {
            state = RustweaveStateService.NormalizeState(newState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            corruptionLabel?.SetNewText(GetCorruptionLabel(), false, true);
            if (IsOpened())
            {
                SingleComposer?.ReCompose();
            }
        }

        public void OpenHud()
        {
            EnsureComposer();
            TryOpen();
        }

        public new bool TryOpen()
        {
            EnsureComposer();
            return base.TryOpen();
        }

        private void EnsureComposer()
        {
            if (SingleComposer != null)
            {
                return;
            }

            var rootBounds = ElementBounds.FixedSize(RustweaveHudLayout.CorruptionHudWidth, RustweaveHudLayout.CorruptionHudHeight)
                .WithAlignment(EnumDialogArea.LeftBottom)
                .WithFixedOffset(GuiStyle.DialogToScreenPadding + RustweaveHudLayout.CorruptionOffset.X, RustweaveHudLayout.CorruptionOffset.Y);

            var barBounds = ElementBounds.Fixed(0, 18, RustweaveHudLayout.CorruptionHudWidth, 12);
            var titleBounds = ElementBounds.Fixed(0, 0, 194, 14);

            var composer = capi.Gui.CreateCompo("therustweave-corruption-hud", rootBounds);
            composer.AddStaticCustomDraw(barBounds, DrawCorruptionBar);
            composer.AddDynamicText(GetCorruptionLabel(), CairoFont.WhiteSmallText(), titleBounds, "corruptionlabel");
            composer.Compose();
            SingleComposer = composer;
            corruptionLabel = composer.GetDynamicText("corruptionlabel");
            corruptionLabel?.SetNewText(GetCorruptionLabel(), false, true);
        }

        private void DrawCorruptionBar(Context ctx, ImageSurface surface, ElementBounds bounds)
        {
            var x = bounds.drawX;
            var y = bounds.drawY;
            var width = bounds.InnerWidth;
            var height = bounds.InnerHeight;

            var ratio = state.EffectiveTemporalCorruptionThreshold <= 0
                ? 0
                : Math.Clamp((double)state.CurrentTemporalCorruption / state.EffectiveTemporalCorruptionThreshold, 0d, 1d);

            var warning = state.CurrentTemporalCorruption >= state.EffectiveTemporalCorruptionThreshold;
            var fillColor = warning ? new[] { 0.83, 0.21, 0.17, 0.92 } : new[] { 0.47, 0.76, 0.91, 0.92 };
            var bgColor = warning ? new[] { 0.22, 0.06, 0.05, 0.72 } : new[] { 0.1, 0.12, 0.13, 0.72 };
            var borderColor = warning ? new[] { 0.97, 0.44, 0.39, 0.95 } : new[] { 0.7, 0.84, 0.92, 0.95 };

            ctx.Save();
            ctx.SetSourceRGBA(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
            ctx.Rectangle(x, y, width, height);
            ctx.Fill();

            ctx.SetSourceRGBA(fillColor[0], fillColor[1], fillColor[2], fillColor[3]);
            ctx.Rectangle(x + 1, y + 1, Math.Max(0, (width - 2) * ratio), Math.Max(0, height - 2));
            ctx.Fill();

            ctx.SetSourceRGBA(borderColor[0], borderColor[1], borderColor[2], borderColor[3]);
            ctx.LineWidth = 1;
            ctx.Rectangle(x + 0.5, y + 0.5, width - 1, height - 1);
            ctx.Stroke();
            ctx.Restore();
        }

        private string GetCorruptionLabel()
        {
            return Lang.Get("game:rustweave-corruption-hud-value", state.CurrentTemporalCorruption, state.EffectiveTemporalCorruptionThreshold);
        }
    }

    internal sealed class RustweaveCastHud : HudElement
    {
        private RustweavePlayerStateData state = RustweaveStateService.CreateDefaultState();
        private RustweaveCastStateData castState = new();
        private GuiElementDynamicText? castLabel;
        private GuiElementDynamicText? castTimer;

        public RustweaveCastHud(ICoreClientAPI capi) : base(capi)
        {
        }

        public override bool Focusable => false;

        public override bool PrefersUngrabbedMouse => true;

        public override bool UnregisterOnClose => false;

        public override string ToggleKeyCombinationCode => string.Empty;

        public void SetState(RustweaveCastStateData newCastState, RustweavePlayerStateData newPlayerState)
        {
            castState = newCastState?.Clone() ?? new RustweaveCastStateData();
            state = RustweaveStateService.NormalizeState(newPlayerState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            castLabel?.SetNewText(GetSpellLabel(), false, true);
            castTimer?.SetNewText(GetElapsedLabel(), false, true);
            if (IsOpened())
            {
                SingleComposer?.ReCompose();
            }
        }

        public void OpenHud()
        {
            EnsureComposer();
            TryOpen();
        }

        public new bool TryOpen()
        {
            EnsureComposer();
            return base.TryOpen();
        }

        private void EnsureComposer()
        {
            if (SingleComposer != null)
            {
                return;
            }

            var rootBounds = ElementBounds.FixedSize(RustweaveHudLayout.CastHudWidth, RustweaveHudLayout.CastHudHeight)
                .WithAlignment(EnumDialogArea.LeftBottom)
                .WithFixedOffset(GuiStyle.DialogToScreenPadding + RustweaveHudLayout.CastOffset.X, RustweaveHudLayout.CastOffset.Y);

            var barBounds = ElementBounds.Fixed(0, 18, RustweaveHudLayout.CastHudWidth, 12);
            var titleBounds = ElementBounds.Fixed(0, 0, 180, 14);
            var timerBounds = ElementBounds.Fixed(184, 0, 76, 14);

            var composer = capi.Gui.CreateCompo("therustweave-cast-hud", rootBounds);
            composer.AddStaticCustomDraw(barBounds, DrawCastBar);
            composer.AddDynamicText(GetSpellLabel(), CairoFont.WhiteSmallText(), titleBounds, "castlabel");
            composer.AddDynamicText(GetElapsedLabel(), CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold), timerBounds, "casttimer");
            composer.Compose();
            SingleComposer = composer;
            castLabel = composer.GetDynamicText("castlabel");
            castTimer = composer.GetDynamicText("casttimer");
            castLabel?.SetNewText(GetSpellLabel(), false, true);
            castTimer?.SetNewText(GetElapsedLabel(), false, true);
        }

        private void DrawCastBar(Context ctx, ImageSurface surface, ElementBounds bounds)
        {
            var x = bounds.drawX;
            var y = bounds.drawY;
            var width = bounds.InnerWidth;
            var height = bounds.InnerHeight;

            var progress = Math.Clamp(castState.Progress, 0d, 1d);
            var fillColor = GetGradientColor(progress);
            var bgColor = new[] { 0.09, 0.08, 0.07, 0.72 };
            var borderColor = new[] { 0.93, 0.88, 0.73, 0.95 };

            ctx.Save();
            ctx.SetSourceRGBA(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
            ctx.Rectangle(x, y, width, height);
            ctx.Fill();

            ctx.SetSourceRGBA(fillColor[0], fillColor[1], fillColor[2], fillColor[3]);
            ctx.Rectangle(x + 1, y + 1, Math.Max(0, (width - 2) * progress), Math.Max(0, height - 2));
            ctx.Fill();

            ctx.SetSourceRGBA(borderColor[0], borderColor[1], borderColor[2], borderColor[3]);
            ctx.LineWidth = 1;
            ctx.Rectangle(x + 0.5, y + 0.5, width - 1, height - 1);
            ctx.Stroke();
            ctx.Restore();
        }

        private string GetSpellLabel()
        {
            var spellName = RustweaveStateService.GetSpellDisplayName(castState.SpellCode);
            if (string.IsNullOrWhiteSpace(spellName))
            {
                spellName = castState.SpellCode;
            }

            return spellName;
        }

        private string GetElapsedLabel()
        {
            return $"{RustweaveStateService.FormatSeconds(castState.ElapsedMilliseconds / 1000d)}s";
        }

        private static double[] GetGradientColor(double progress)
        {
            progress = Math.Clamp(progress, 0d, 1d);
            if (progress < 0.5d)
            {
                var t = progress / 0.5d;
                return new[] { 1d, t, 0d, 0.92d };
            }

            var u = (progress - 0.5d) / 0.5d;
            return new[] { 1d - u, 1d, 0d, 0.92d };
        }
    }

    internal sealed class RustweaveSpellPrepDialog : GuiDialog
    {
        private enum TomeTab
        {
            Learned,
            Prepared
        }

        private static bool warnedAboutEmptyRegistry;
        private RustweavePlayerStateData state = RustweaveStateService.CreateDefaultState();
        private TomeTab activeTab = TomeTab.Learned;
        private string selectedLearnedSpellCode = string.Empty;

        public RustweaveSpellPrepDialog(ICoreClientAPI capi) : base(capi)
        {
        }

        public override string ToggleKeyCombinationCode => string.Empty;

        public override bool Focusable => true;

        public override bool PrefersUngrabbedMouse => false;

        public override bool UnregisterOnClose => false;

        public override float ZSize => 500f;

        public void SetState(RustweavePlayerStateData newState)
        {
            state = RustweaveStateService.NormalizeState(newState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            EnsureSelectedLearnedSpell();
            if (IsOpened())
            {
                SingleComposer?.ReCompose();
            }
        }

        public void OpenDialog()
        {
            LogTomeOpen();
            EnsureComposer();
            TryOpen();
        }

        public override bool TryOpen()
        {
            EnsureComposer();
            return base.TryOpen();
        }

        private void LogTomeOpen()
        {
            var learnedSpells = GetLearnedSpells();
            capi.Logger.Debug("[TheRustweave] Tome opened; exposing {0} learned spell(s).", learnedSpells.Count);
            if (learnedSpells.Count == 0 && !warnedAboutEmptyRegistry)
            {
                warnedAboutEmptyRegistry = true;
                capi.Logger.Warning("[TheRustweave] The Tome opened while the spell registry had zero enabled spells.");
            }
        }

        private void EnsureComposer()
        {
            if (SingleComposer != null)
            {
                return;
            }

            EnsureSelectedLearnedSpell();

            var rootBounds = ElementBounds.FixedSize(760, 640).WithAlignment(EnumDialogArea.CenterMiddle);
            var headerBounds = ElementBounds.Fixed(16, 14, 728, 28);
            var contentBounds = ElementBounds.Fixed(12, 46, 736, 582);
            var tabLearnedBounds = ElementBounds.Fixed(16, 52, 130, 24);
            var tabPreparedBounds = ElementBounds.Fixed(152, 52, 130, 24);

            var composer = capi.Gui.CreateCompo("therustweave-spell-prep", rootBounds);
            composer.AddDialogBG(rootBounds, true, 0.93f);
            composer.AddDialogTitleBar(Lang.Get("game:rustweave-prep-title"), () => TryClose(), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(0, 0, 760, GuiStyle.TitleBarHeight));
            composer.AddInset(contentBounds, 5, 0.92f);
            composer.AddStaticText(Lang.Get("game:rustweave-tome-learned-tab"), CairoFont.WhiteSmallishText(), headerBounds, "tomeheader");
            AddTabButton(composer, tabLearnedBounds, TomeTab.Learned, "tablearned", Lang.Get("game:rustweave-tab-learned"));
            AddTabButton(composer, tabPreparedBounds, TomeTab.Prepared, "tabprepared", Lang.Get("game:rustweave-tab-prepared"));

            if (activeTab == TomeTab.Learned)
            {
                AddLearnedTab(composer);
            }
            else
            {
                AddPreparedTab(composer);
            }

            composer.Compose();
            SingleComposer = composer;
        }

        private void AddTabButton(GuiComposer composer, ElementBounds bounds, TomeTab tab, string name, string label)
        {
            var displayLabel = activeTab == tab ? $"> {label} <" : label;
            composer.AddButton(displayLabel, () =>
            {
                activeTab = tab;
                SingleComposer?.ReCompose();
                return true;
            }, bounds, EnumButtonStyle.Normal, name);
        }

        private void AddLearnedTab(GuiComposer composer)
        {
            var learnedSpells = GetLearnedSpells();
            var listBounds = ElementBounds.Fixed(18, 92, 332, 500);
            var detailsBounds = ElementBounds.Fixed(360, 92, 370, 500);

            composer.AddInset(listBounds, 3, 0.88f);
            composer.AddInset(detailsBounds, 3, 0.88f);
            composer.AddStaticText(Lang.Get("game:rustweave-learned-spells"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(26, 98, 300, 20), "learnedheader");
            composer.AddStaticText(Lang.Get("game:rustweave-spell-details"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(368, 98, 220, 20), "detailsheader");

            var listChildBounds = ElementBounds.Fixed(22, 122, 316, 454);
            composer.BeginChildElements(listChildBounds);
            if (learnedSpells.Count == 0)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), CairoFont.WhiteSmallText().WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, 0, 280, 18), "learned-empty");
            }
            else
            {
                for (var index = 0; index < learnedSpells.Count; index++)
                {
                    AddLearnedSpellRow(composer, learnedSpells[index], index);
                }
            }
            composer.EndChildElements();

            var selectedSpell = GetSelectedLearnedSpell();
            AddSpellDetailsPanel(composer, detailsBounds, selectedSpell);
        }

        private void AddLearnedSpellRow(GuiComposer composer, SpellDefinition spell, int index)
        {
            var code = spell.Code ?? string.Empty;
            var name = string.IsNullOrWhiteSpace(spell.Name) ? code : spell.Name;
            var rowTop = index * 26;
            var isSelected = string.Equals(code, selectedLearnedSpellCode, StringComparison.OrdinalIgnoreCase);
            var rowColor = isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DialogDefaultTextColor;
            var displayName = isSelected ? $"> {name} <" : name;

            composer.AddButton(displayName, () =>
            {
                selectedLearnedSpellCode = code;
                SingleComposer?.ReCompose();
                return true;
            }, ElementBounds.Fixed(0, rowTop, 220, 20), EnumButtonStyle.Normal, $"learned-name-{index}");

            composer.AddButton(Lang.Get("game:rustweave-prepare"), () =>
            {
                RustweaveRuntime.Client?.RequestPrepareSpell(code, state.SelectedPreparedSpellIndex);
                return true;
            }, ElementBounds.Fixed(228, rowTop, 68, 20), EnumButtonStyle.Normal, $"learned-prepare-{index}");

            composer.AddStaticText(spell.School ?? string.Empty, CairoFont.WhiteSmallText().WithColor(rowColor), ElementBounds.Fixed(0, rowTop + 18, 132, 12), $"learned-school-{index}");
            composer.AddStaticText($"T{spell.Tier}", CairoFont.WhiteSmallText().WithColor(rowColor), ElementBounds.Fixed(138, rowTop + 18, 32, 12), $"learned-tier-{index}");
        }

        private void AddSpellDetailsPanel(GuiComposer composer, ElementBounds detailsBounds, SpellDefinition? spell)
        {
            if (spell == null)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), CairoFont.WhiteSmallText().WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(372, 122, 330, 18), "details-empty");
                return;
            }

            var name = string.IsNullOrWhiteSpace(spell.Name) ? spell.Code : spell.Name;
            var description = string.IsNullOrWhiteSpace(spell.Description) ? Lang.Get("game:rustweave-slot-empty") : spell.Description;
            var effectsSummary = GetEffectsSummary(spell);

            composer.AddStaticText(name, CairoFont.WhiteSmallishText(), ElementBounds.Fixed(372, 122, 320, 20), "detail-name");
            composer.AddStaticTextAutoBoxSize(description, CairoFont.WhiteSmallText(), EnumTextOrientation.Left, ElementBounds.Fixed(372, 148, 320, 50), "detail-desc");
            composer.AddStaticText(Lang.Get("game:rustweave-school") + ": " + (spell.School ?? string.Empty), CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 206, 320, 18), "detail-school");
            composer.AddStaticText(Lang.Get("game:rustweave-tier") + ": " + spell.Tier, CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 226, 320, 18), "detail-tier");
            composer.AddStaticText(Lang.Get("game:rustweave-corruption-cost") + ": " + spell.CorruptionCost, CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 246, 320, 18), "detail-corruption");
            composer.AddStaticText(Lang.Get("game:rustweave-cast-time") + ": " + RustweaveStateService.FormatSeconds(spell.CastTimeSeconds) + "s", CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 266, 320, 18), "detail-cast");
            composer.AddStaticText(Lang.Get("game:rustweave-cooldown") + ": " + RustweaveStateService.FormatSeconds(spell.CooldownSeconds) + "s", CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 286, 320, 18), "detail-cooldown");
            composer.AddStaticText(Lang.Get("game:rustweave-target-type") + ": " + (spell.TargetType ?? string.Empty), CairoFont.WhiteSmallText(), ElementBounds.Fixed(372, 306, 320, 18), "detail-target");
            composer.AddStaticTextAutoBoxSize(Lang.Get("game:rustweave-effects") + ": " + effectsSummary, CairoFont.WhiteSmallText(), EnumTextOrientation.Left, ElementBounds.Fixed(372, 328, 320, 64), "detail-effects");
        }

        private void AddPreparedTab(GuiComposer composer)
        {
            composer.AddStaticText(Lang.Get("game:rustweave-prepared-spells"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(26, 98, 260, 20), "preparedheader");

            for (var slotIndex = 0; slotIndex < RustweaveConstants.PreparedSlotCount; slotIndex++)
            {
                AddPreparedSlotRow(composer, 26, 128 + (slotIndex * 44), slotIndex);
            }
        }

        private void AddPreparedSlotRow(GuiComposer composer, int left, int top, int slotIndex)
        {
            var isSelected = state.SelectedPreparedSpellIndex == slotIndex;
            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            var hasSpell = !string.IsNullOrWhiteSpace(spellCode);
            var spellExists = hasSpell && RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell != null;
            var spellName = !hasSpell
                ? Lang.Get("game:rustweave-slot-empty")
                : spellExists
                    ? RustweaveStateService.GetSpellDisplayName(spellCode)
                    : $"{spellCode} ({Lang.Get("game:rustweave-spell-invalid")})";
            var displaySlotText = isSelected ? $"> {Lang.Get("game:rustweave-prepared-slot", slotIndex + 1)}: {spellName} <" : $"{Lang.Get("game:rustweave-prepared-slot", slotIndex + 1)}: {spellName}";

            composer.AddButton(displaySlotText, () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                return true;
            }, ElementBounds.Fixed(left, top, 280, 20), EnumButtonStyle.Normal, $"prepared-slot-{slotIndex}");

            composer.AddStaticText(isSelected ? Lang.Get("game:rustweave-active-slot") : string.Empty, CairoFont.WhiteSmallText().WithColor(isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DisabledTextColor), ElementBounds.Fixed(left + 286, top + 2, 64, 16), $"prepared-active-{slotIndex}");
            composer.AddButton(Lang.Get("game:rustweave-unprepare"), () =>
            {
                RustweaveRuntime.Client?.RequestUnprepareSpell(slotIndex);
                return true;
            }, ElementBounds.Fixed(left + 350, top - 1, 54, 20), EnumButtonStyle.Normal, $"prepared-clear-{slotIndex}");
        }

        private void EnsureSelectedLearnedSpell()
        {
            var learnedSpells = GetLearnedSpells();
            if (learnedSpells.Count == 0)
            {
                selectedLearnedSpellCode = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedLearnedSpellCode) || learnedSpells.All(spell => !string.Equals(spell.Code, selectedLearnedSpellCode, StringComparison.OrdinalIgnoreCase)))
            {
                selectedLearnedSpellCode = learnedSpells[0].Code ?? string.Empty;
            }
        }

        private SpellDefinition? GetSelectedLearnedSpell()
        {
            var learnedSpells = GetLearnedSpells();
            if (learnedSpells.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(selectedLearnedSpellCode))
            {
                var selectedSpell = learnedSpells.FirstOrDefault(spell => string.Equals(spell.Code, selectedLearnedSpellCode, StringComparison.OrdinalIgnoreCase));
                if (selectedSpell != null)
                {
                    return selectedSpell;
                }
            }

            selectedLearnedSpellCode = learnedSpells[0].Code ?? string.Empty;
            return learnedSpells[0];
        }

        private List<SpellDefinition> GetLearnedSpells()
        {
            return RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Where(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && RustweaveStateService.IsSpellLearned(spell.Code, state))
                .OrderBy(spell => spell.School ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(spell => spell.Tier)
                .ThenBy(spell => spell.Name ?? spell.Code ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetEffectsSummary(SpellDefinition spell)
        {
            var effects = spell.Effects?
                .Select(effect => effect?.Type ?? string.Empty)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .ToList() ?? new List<string>();

            return effects.Count == 0 ? Lang.Get("game:rustweave-slot-empty") : string.Join(", ", effects);
        }
    }
}

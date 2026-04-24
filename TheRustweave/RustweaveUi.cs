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
        private bool rebuildPending;
        private bool rebuildInProgress;

        public RustweaveSpellPrepDialog(ICoreClientAPI capi) : base(capi)
        {
            capi.Gui.RegisterDialog(this);
        }

        public override string ToggleKeyCombinationCode => string.Empty;

        public override bool Focusable => true;

        public override bool PrefersUngrabbedMouse => false;

        public override bool UnregisterOnClose => false;

        public override float ZSize => 700f;

        public void SetState(RustweavePlayerStateData newState, bool refreshIfOpen = true)
        {
            state = RustweaveStateService.NormalizeState(newState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            EnsureSelectedLearnedSpell();
            if (refreshIfOpen && IsOpened())
            {
                SingleComposer?.ReCompose();
            }
        }

        public void OpenDialog()
        {
            LogTomeOpen();
            if (capi.World?.Player == null)
            {
                return;
            }

            if (IsOpened())
            {
                SingleComposer?.ReCompose();
                return;
            }

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

        private void LogPreparedPageOpen()
        {
            capi.Logger.Debug("[TheRustweave] Prepared page opened.");
        }

        private void RequestRebuild()
        {
            if (rebuildPending || rebuildInProgress)
            {
                return;
            }

            rebuildPending = true;
            capi.Event.EnqueueMainThreadTask(() =>
            {
                rebuildPending = false;
                if (rebuildInProgress)
                {
                    return;
                }

                rebuildInProgress = true;
                try
                {
                    RebuildDialog();
                }
                finally
                {
                    rebuildInProgress = false;
                }
            }, "therustweave-tome-rebuild");
        }

        private void SwitchTab(TomeTab tab)
        {
            if (activeTab == tab)
            {
                return;
            }

            activeTab = tab;
            capi.Logger.Debug("[TheRustweave] Active tab changed to {0}.", tab);
            if (tab == TomeTab.Prepared)
            {
                LogPreparedPageOpen();
            }

            RequestRebuild();
        }

        private void RebuildDialog()
        {
            if (rebuildInProgress)
            {
                return;
            }

            var wasOpened = IsOpened();
            if (wasOpened)
            {
                TryClose();
            }

            SingleComposer?.Dispose();
            SingleComposer = null;
            EnsureComposer();

            if (wasOpened)
            {
                TryOpen();
            }
        }

        private void EnsureComposer()
        {
            if (capi?.Gui == null || SingleComposer != null)
            {
                return;
            }

            EnsureSelectedLearnedSpell();

            var rootBounds = ElementBounds.FixedSize(860, 700).WithAlignment(EnumDialogArea.CenterMiddle);
            var backgroundBounds = ElementBounds.Fixed(0, 0, 860, 700);
            var titleBounds = ElementBounds.Fixed(26, 24, 500, 24);
            var closeBounds = ElementBounds.Fixed(794, 20, 42, 22);
            var tabLearnedBounds = ElementBounds.Fixed(26, 54, 140, 24);
            var tabPreparedBounds = ElementBounds.Fixed(172, 54, 140, 24);
            var contentBounds = ElementBounds.Fixed(24, 86, 812, 588);

            var composer = capi.Gui.CreateCompo("therustweave-spell-prep", rootBounds);
            composer.AddDialogBG(backgroundBounds, true, 1f);
            composer.AddInset(contentBounds, 5, 0.94f);
            composer.AddStaticText(Lang.Get("game:rustweave-prep-title"), CairoFont.WhiteSmallishText(), titleBounds, "tome-title");
            composer.AddButton("X", () =>
            {
                TryClose();
                return true;
            }, closeBounds, EnumButtonStyle.Normal, "tome-close");

            AddTabButton(composer, tabLearnedBounds, TomeTab.Learned, "tab-learned", "Learned");
            AddTabButton(composer, tabPreparedBounds, TomeTab.Prepared, "tab-prepared", "Prepared");

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
            var text = activeTab == tab ? $"[{label}]" : label;
            composer.AddButton(text, () =>
            {
                capi.Logger.Debug("[TheRustweave] Tome tab clicked: {0}", label);
                SwitchTab(tab);
                return true;
            }, bounds, EnumButtonStyle.Normal, name);
        }

        private void AddLearnedTab(GuiComposer composer)
        {
            var learnedSpells = GetLearnedSpells();
            var listPanelBounds = ElementBounds.Fixed(28, 94, 392, 562);
            var detailsPanelBounds = ElementBounds.Fixed(430, 94, 402, 562);
            var listHeaderBounds = ElementBounds.Fixed(36, 102, 240, 20);
            var detailsHeaderBounds = ElementBounds.Fixed(438, 102, 240, 20);

            composer.AddInset(listPanelBounds, 3, 0.9f);
            composer.AddInset(detailsPanelBounds, 3, 0.9f);
            composer.AddStaticText(Lang.Get("game:rustweave-learned-spells"), CairoFont.WhiteSmallishText(), listHeaderBounds, "learned-header");
            composer.AddStaticText(Lang.Get("game:rustweave-spell-details"), CairoFont.WhiteSmallishText(), detailsHeaderBounds, "details-header");

            var listBounds = ElementBounds.Fixed(36, 128, 372, 500);
            composer.BeginChildElements(listBounds);
            if (learnedSpells.Count == 0)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), CairoFont.WhiteSmallText().WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, 0, 300, 18), "learned-empty");
            }
            else
            {
                for (var index = 0; index < learnedSpells.Count; index++)
                {
                    AddLearnedSpellRow(composer, learnedSpells[index], index);
                }
            }
            composer.EndChildElements();

            AddSpellDetailsPanel(composer, GetSelectedLearnedSpell());
        }

        private void AddLearnedSpellRow(GuiComposer composer, SpellDefinition spell, int index)
        {
            var code = spell.Code ?? string.Empty;
            var name = string.IsNullOrWhiteSpace(spell.Name) ? code : spell.Name;
            var rowTop = index * 34;
            var isSelected = string.Equals(code, selectedLearnedSpellCode, StringComparison.OrdinalIgnoreCase);
            var displayName = isSelected ? $"> {name} <" : name;

            composer.AddButton(displayName, () =>
            {
                selectedLearnedSpellCode = code;
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(0, rowTop, 280, 22), EnumButtonStyle.Normal, $"learned-name-{index}");

            composer.AddButton("Prep", () =>
            {
                RustweaveRuntime.Client?.RequestPrepareSpell(code, -1);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(292, rowTop, 52, 22), EnumButtonStyle.Normal, $"learned-prep-{index}");
        }

        private void AddSpellDetailsPanel(GuiComposer composer, SpellDefinition? spell)
        {
            if (spell == null)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), CairoFont.WhiteSmallText().WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(438, 128, 360, 18), "details-empty");
                return;
            }

            var name = string.IsNullOrWhiteSpace(spell.Name) ? spell.Code : spell.Name;
            var description = string.IsNullOrWhiteSpace(spell.Description) ? Lang.Get("game:rustweave-slot-empty") : spell.Description;
            var effectsSummary = GetEffectsSummary(spell);

            composer.AddStaticText(name, CairoFont.WhiteSmallishText(), ElementBounds.Fixed(438, 126, 360, 20), "detail-name");
            composer.AddStaticTextAutoBoxSize(description, CairoFont.WhiteSmallText(), EnumTextOrientation.Left, ElementBounds.Fixed(438, 154, 360, 56), "detail-desc");
            composer.AddStaticText($"{Lang.Get("game:rustweave-school")}: {spell.School}", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 220, 360, 18), "detail-school");
            composer.AddStaticText($"{Lang.Get("game:rustweave-tier")}: {spell.Tier}", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 242, 360, 18), "detail-tier");
            composer.AddStaticText($"{Lang.Get("game:rustweave-corruption-cost")}: {spell.CorruptionCost}", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 264, 360, 18), "detail-corruption");
            composer.AddStaticText($"{Lang.Get("game:rustweave-cast-time")}: {RustweaveStateService.FormatSeconds(spell.CastTimeSeconds)}s", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 286, 360, 18), "detail-cast");
            composer.AddStaticText($"{Lang.Get("game:rustweave-cooldown")}: {RustweaveStateService.FormatSeconds(spell.CooldownSeconds)}s", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 308, 360, 18), "detail-cooldown");
            composer.AddStaticText($"{Lang.Get("game:rustweave-target-type")}: {spell.TargetType}", CairoFont.WhiteSmallText(), ElementBounds.Fixed(438, 330, 360, 18), "detail-target");
            composer.AddStaticTextAutoBoxSize($"{Lang.Get("game:rustweave-effects")}: {effectsSummary}", CairoFont.WhiteSmallText(), EnumTextOrientation.Left, ElementBounds.Fixed(438, 354, 360, 68), "detail-effects");
        }

        private void AddPreparedTab(GuiComposer composer)
        {
            var panelBounds = ElementBounds.Fixed(28, 94, 804, 562);
            composer.AddInset(panelBounds, 3, 0.9f);
            composer.AddStaticText(Lang.Get("game:rustweave-prepared-spells"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(36, 102, 250, 20), "prepared-header");

            var listBounds = ElementBounds.Fixed(36, 128, 772, 500);
            composer.BeginChildElements(listBounds);
            for (var slotIndex = 0; slotIndex < RustweaveConstants.PreparedSlotCount; slotIndex++)
            {
                AddPreparedSlotRow(composer, slotIndex);
            }
            composer.EndChildElements();
        }

        private void AddPreparedSlotRow(GuiComposer composer, int slotIndex)
        {
            var rowTop = slotIndex * 50;
            var isSelected = state.SelectedPreparedSpellIndex == slotIndex;
            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            var hasSpell = !string.IsNullOrWhiteSpace(spellCode);
            var spellExists = hasSpell && RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell != null;
            var spellName = !hasSpell
                ? Lang.Get("game:rustweave-slot-empty")
                : spellExists
                    ? RustweaveStateService.GetSpellDisplayName(spellCode)
                    : $"{spellCode} ({Lang.Get("game:rustweave-spell-invalid")})";
            var displayText = isSelected ? $"> {Lang.Get("game:rustweave-prepared-slot", slotIndex + 1)}: {spellName} <" : $"{Lang.Get("game:rustweave-prepared-slot", slotIndex + 1)}: {spellName}";

            composer.AddButton(displayText, () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(0, rowTop, 300, 24), EnumButtonStyle.Normal, $"prepared-slot-{slotIndex}");

            composer.AddButton("Select", () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(310, rowTop, 60, 24), EnumButtonStyle.Normal, $"prepared-select-{slotIndex}");

            composer.AddStaticText(isSelected ? Lang.Get("game:rustweave-active-slot") : string.Empty, CairoFont.WhiteSmallText().WithColor(isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DisabledTextColor), ElementBounds.Fixed(376, rowTop + 3, 54, 16), $"prepared-active-{slotIndex}");

            composer.AddButton("Clear", () =>
            {
                RustweaveRuntime.Client?.RequestUnprepareSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(436, rowTop, 54, 24), EnumButtonStyle.Normal, $"prepared-clear-{slotIndex}");
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

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
        private const int TomeWidth = 1140;
        private const int TomeHeight = 840;
        private const float TomeRootOffsetY = -68f;
        private const int TomeTabButtonWidth = 92;
        private const int TomeTabButtonHeight = 24;
        private const int TomeTabButtonSpacing = 14;
        private const int TomeTabRowX = 26;
        private const int TomeTabRowY = 96;

        private static CairoFont TomeTitleFont => CairoFont.WhiteSmallishText().WithFontSize(12.5f);
        private static CairoFont TomeButtonFont => CairoFont.WhiteSmallText().WithFontSize(9.5f);
        private static CairoFont TomeListFont => CairoFont.WhiteSmallText().WithFontSize(9.5f);
        private static CairoFont TomeDetailFont => CairoFont.WhiteSmallText().WithFontSize(9.5f);

        private enum TomeTab
        {
            All,
            Learned,
            Locked,
            Loreweave,
            Prepared
        }

        private static bool warnedAboutEmptyRegistry;
        private RustweavePlayerStateData state = RustweaveStateService.CreateDefaultState();
        private TomeTab activeTab = TomeTab.Learned;
        private string selectedSpellCode = string.Empty;
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

        public override float ZSize => 760f;

        public void SetState(RustweavePlayerStateData newState, bool refreshIfOpen = true)
        {
            state = RustweaveStateService.NormalizeState(newState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            NormalizeSelectedSpellForActiveTab();
            if (refreshIfOpen && IsOpened())
            {
                RequestRebuild();
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
                RequestRebuild();
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
            var player = capi.World?.Player;
            var visibleCount = RustweaveStateService.GetAllVisibleSpells(player).Count;
            var learnedCount = RustweaveStateService.GetLearnedSpells(player).Count;
            var lockedCount = RustweaveStateService.GetLockedSpells(player).Count;
            var loreCount = RustweaveStateService.GetLoreweaveSpells(player).Count;
            var rankName = RustweaveStateService.GetKnowledgeRankName(state);
            var rankIndex = RustweaveStateService.GetKnowledgeRankIndex(state) + 1;

            capi.Logger.Debug("[TheRustweave] Tome opened; visible={0}, learned={1}, locked={2}, lore={3}, rank={4} (#{5}).", visibleCount, learnedCount, lockedCount, loreCount, rankName, rankIndex);
            if (RustweaveConstants.AllSpellsLearnedByDefaultForTesting)
            {
                capi.Logger.Debug("[TheRustweave] Testing mode active: all enabled spells learned.");
            }

            if (visibleCount == 0 && !warnedAboutEmptyRegistry)
            {
                warnedAboutEmptyRegistry = true;
                capi.Logger.Warning("[TheRustweave] The Tome opened while the spell registry had zero visible enabled spells.");
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

            NormalizeSelectedSpellForActiveTab();
            RequestRebuild();
        }

        private void RebuildDialog()
        {
            var wasOpened = IsOpened();
            if (wasOpened)
            {
                TryClose();
            }

            ClearComposers();
            EnsureComposer();

            if (wasOpened)
            {
                TryOpen();
            }
        }

        private void EnsureComposer()
        {
            if (capi?.Gui == null || HasMainComposer())
            {
                return;
            }

            NormalizeSelectedSpellForActiveTab();

            var rootBounds = ElementBounds.FixedSize(TomeWidth, TomeHeight)
                .WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedOffset(0, TomeRootOffsetY);
            var backgroundBounds = ElementBounds.Fixed(0, 0, TomeWidth, TomeHeight);
            var titleBounds = ElementBounds.Fixed(26, 40, 760, 24);
            var closeBounds = ElementBounds.Fixed(1070, 40, 42, 22);
            var statusBounds = ElementBounds.Fixed(26, 68, 920, 38);
            var tabX = TomeTabRowX;
            var tabAllBounds = ElementBounds.Fixed(tabX, TomeTabRowY, TomeTabButtonWidth, TomeTabButtonHeight);
            tabX += TomeTabButtonWidth + TomeTabButtonSpacing;
            var tabLearnedBounds = ElementBounds.Fixed(tabX, TomeTabRowY, TomeTabButtonWidth, TomeTabButtonHeight);
            tabX += TomeTabButtonWidth + TomeTabButtonSpacing;
            var tabLockedBounds = ElementBounds.Fixed(tabX, TomeTabRowY, TomeTabButtonWidth, TomeTabButtonHeight);
            tabX += TomeTabButtonWidth + TomeTabButtonSpacing;
            var tabLoreBounds = ElementBounds.Fixed(tabX, TomeTabRowY, TomeTabButtonWidth, TomeTabButtonHeight);
            tabX += TomeTabButtonWidth + TomeTabButtonSpacing;
            var tabPreparedBounds = ElementBounds.Fixed(tabX, TomeTabRowY, TomeTabButtonWidth, TomeTabButtonHeight);
            var contentBounds = ElementBounds.Fixed(18, 126, 1104, 676);

            var composer = capi.Gui.CreateCompo("therustweave-spell-prep", rootBounds);
            composer.AddDialogBG(backgroundBounds, true, 1f);
            composer.AddInset(contentBounds, 6, 0.94f);
            composer.AddStaticText(Lang.Get("game:rustweave-prep-title"), TomeTitleFont, titleBounds, "tome-title");
            composer.AddStaticTextAutoBoxSize(GetStatusHeaderText(), TomeListFont, EnumTextOrientation.Left, statusBounds, "tome-status");
            composer.AddButton("X", () =>
            {
                TryClose();
                return true;
            }, closeBounds, TomeButtonFont, EnumButtonStyle.Normal, "tome-close");

            AddTabButton(composer, tabAllBounds, TomeTab.All, "tab-all", "All");
            AddTabButton(composer, tabLearnedBounds, TomeTab.Learned, "tab-learned", "Learned");
            AddTabButton(composer, tabLockedBounds, TomeTab.Locked, "tab-locked", "Locked");
            AddTabButton(composer, tabLoreBounds, TomeTab.Loreweave, "tab-lore", "Lore");
            AddTabButton(composer, tabPreparedBounds, TomeTab.Prepared, "tab-prepared", "Prepared");

            switch (activeTab)
            {
                case TomeTab.All:
                    AddSpellBrowseTab(composer, Lang.Get("game:rustweave-all-spells"), GetAllSpells(), allowPrepare: true, allowUnlockHint: true, showLoreNote: false, showStateLabel: true, showTierLabel: false, pageName: "all");
                    break;
                case TomeTab.Learned:
                    AddSpellBrowseTab(composer, Lang.Get("game:rustweave-learned-spells"), GetLearnedSpells(), allowPrepare: true, allowUnlockHint: false, showLoreNote: false, showStateLabel: false, showTierLabel: true, pageName: "learned");
                    break;
                case TomeTab.Locked:
                    AddSpellBrowseTab(composer, Lang.Get("game:rustweave-locked-spells"), GetLockedSpells(), allowPrepare: false, allowUnlockHint: true, showLoreNote: false, showStateLabel: true, showTierLabel: false, pageName: "locked");
                    break;
                case TomeTab.Loreweave:
                    AddSpellBrowseTab(composer, Lang.Get("game:rustweave-loreweave-spells"), GetLoreweaveSpells(), allowPrepare: true, allowUnlockHint: false, showLoreNote: true, showStateLabel: true, showTierLabel: false, pageName: "lore");
                    break;
                case TomeTab.Prepared:
                    AddPreparedTab(composer);
                    break;
            }

            composer.Compose();
            if (Composers == null)
            {
                capi.Logger.Error("[TheRustweave] Tome dialog composer map was null; cannot register main composer.");
                composer.Dispose();
                return;
            }

            Composers["main"] = composer;
        }

        private bool HasMainComposer()
        {
            return Composers != null && Composers.ContainsKey("main");
        }

        private void AddTabButton(GuiComposer composer, ElementBounds bounds, TomeTab tab, string name, string label)
        {
            var text = activeTab == tab ? $"[{label}]" : label;
            composer.AddButton(text, () =>
            {
                capi.Logger.Debug("[TheRustweave] Tome tab clicked: {0}", label);
                SwitchTab(tab);
                return true;
            }, bounds, TomeButtonFont, EnumButtonStyle.Normal, name);
        }

        private void AddSpellBrowseTab(GuiComposer composer, string pageTitle, List<SpellDefinition> spells, bool allowPrepare, bool allowUnlockHint, bool showLoreNote, bool showStateLabel, bool showTierLabel, string pageName)
        {
            var listPanelBounds = ElementBounds.Fixed(28, 142, 520, 580);
            var detailsPanelBounds = ElementBounds.Fixed(552, 142, 560, 580);
            composer.AddInset(listPanelBounds, 3, 0.9f);
            composer.AddInset(detailsPanelBounds, 3, 0.9f);
            composer.AddStaticText(pageTitle, TomeTitleFont, ElementBounds.Fixed(36, 146, 480, 20), $"{pageName}-header");

            if (showLoreNote)
            {
                composer.AddStaticTextAutoBoxSize(Lang.Get("game:rustweave-loreweave-note"), TomeListFont, EnumTextOrientation.Left, ElementBounds.Fixed(36, 170, 470, 32), $"{pageName}-note");
            }

            NormalizeSelectedSpellForActiveTab(spells);
            var selectedSpell = GetDisplayedSpell(spells);

            var listTop = showLoreNote ? 212 : 188;
            var listBounds = ElementBounds.Fixed(36, listTop, 500, 470);
            composer.BeginChildElements(listBounds);
            if (spells.Count == 0)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), TomeListFont.WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, 0, 340, 18), $"{pageName}-empty");
            }
            else
            {
                for (var index = 0; index < spells.Count; index++)
                {
                    AddSpellRow(composer, spells[index], index, allowPrepare, showStateLabel, showTierLabel, pageName);
                }
            }
            composer.EndChildElements();

            AddSpellDetailsPanel(composer, selectedSpell, allowUnlockHint, pageName);
        }

        private void AddSpellRow(GuiComposer composer, SpellDefinition spell, int index, bool allowPrepare, bool showStateLabel, bool showTierLabel, string pageName)
        {
            var code = spell.Code ?? string.Empty;
            var rowTop = index * 36;
            var isSelected = string.Equals(code, selectedSpellCode, StringComparison.OrdinalIgnoreCase);
            var name = string.IsNullOrWhiteSpace(spell.Name) ? code : spell.Name;
            var stateLabel = GetSpellStateLabel(spell);
            var tierLabel = GetTierDisplayLabel(spell.Tier);
            var displayName = isSelected ? $"> {name} <" : name;
            var canPrepare = allowPrepare && CanPrepareSpell(spell);
            var nameWidth = showStateLabel || showTierLabel ? 292 : 316;

            composer.AddButton(displayName, () =>
            {
                selectedSpellCode = code;
                capi.Logger.Debug("[TheRustweave] Spell selected: {0} ({1}).", code, stateLabel);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(0, rowTop, nameWidth, 21), TomeButtonFont, EnumButtonStyle.Normal, $"{pageName}-spell-{index}");

            if (showStateLabel)
            {
                composer.AddStaticText(stateLabel, TomeListFont, ElementBounds.Fixed(300, rowTop + 2, 118, 18), $"{pageName}-state-{index}");
            }
            else if (showTierLabel)
            {
                composer.AddStaticText(tierLabel, TomeListFont, ElementBounds.Fixed(300, rowTop + 2, 118, 18), $"{pageName}-tier-{index}");
            }

            if (canPrepare)
            {
                composer.AddButton("Prep", () =>
                {
                    RustweaveRuntime.Client?.RequestPrepareSpell(code, -1);
                    RequestRebuild();
                    return true;
                }, ElementBounds.Fixed(438, rowTop, 48, 21), TomeButtonFont, EnumButtonStyle.Normal, $"{pageName}-prep-{index}");
            }
        }

        private void AddSpellDetailsPanel(GuiComposer composer, SpellDefinition? spell, bool allowUnlockHint, string pageName)
        {
            if (spell == null)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), TomeDetailFont.WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(552, 172, 508, 18), $"{pageName}-details-empty");
                return;
            }

            var name = string.IsNullOrWhiteSpace(spell.Name) ? spell.Code : spell.Name;
            var description = string.IsNullOrWhiteSpace(spell.Description) ? Lang.Get("game:rustweave-slot-empty") : spell.Description;
            var effectsSummary = GetEffectsSummary(spell);
            var isLearned = RustweaveStateService.IsSpellLearned(spell.Code, state);
            var isLore = RustweaveStateService.IsLoreweaveSpell(spell);
            var isHidden = RustweaveStateService.IsHiddenSpell(spell);
            var stateText = isLore ? Lang.Get("game:rustweave-spell-state-lore") : isLearned ? Lang.Get("game:rustweave-spell-state-learned") : Lang.Get("game:rustweave-spell-state-locked");

            composer.AddStaticText(name, TomeTitleFont, ElementBounds.Fixed(552, 170, 520, 20), $"{pageName}-detail-name");
            composer.AddStaticTextAutoBoxSize(description, TomeDetailFont, EnumTextOrientation.Left, ElementBounds.Fixed(552, 198, 520, 52), $"{pageName}-detail-desc");
            composer.AddStaticText($"{Lang.Get("game:rustweave-school")}: {spell.School}", TomeDetailFont, ElementBounds.Fixed(552, 258, 520, 18), $"{pageName}-detail-school");

            if (!string.IsNullOrWhiteSpace(spell.Category))
            {
                composer.AddStaticText($"{Lang.Get("game:rustweave-category")}: {spell.Category}", TomeDetailFont, ElementBounds.Fixed(552, 280, 520, 18), $"{pageName}-detail-category");
            }

            composer.AddStaticText($"{Lang.Get("game:rustweave-tier")}: {spell.Tier}", TomeDetailFont, ElementBounds.Fixed(552, 302, 520, 18), $"{pageName}-detail-tier");
            composer.AddStaticText($"{Lang.Get("game:rustweave-corruption-cost")}: {spell.CorruptionCost}", TomeDetailFont, ElementBounds.Fixed(552, 324, 520, 18), $"{pageName}-detail-corruption");
            composer.AddStaticText($"{Lang.Get("game:rustweave-cast-time")}: {RustweaveStateService.FormatSeconds(spell.CastTimeSeconds)}s", TomeDetailFont, ElementBounds.Fixed(552, 346, 520, 18), $"{pageName}-detail-cast");
            composer.AddStaticText($"{Lang.Get("game:rustweave-cooldown")}: {RustweaveStateService.FormatSeconds(spell.CooldownSeconds)}s", TomeDetailFont, ElementBounds.Fixed(552, 368, 520, 18), $"{pageName}-detail-cooldown");
            composer.AddStaticText($"{Lang.Get("game:rustweave-target-type")}: {spell.TargetType}", TomeDetailFont, ElementBounds.Fixed(552, 390, 520, 18), $"{pageName}-detail-target");
            composer.AddStaticTextAutoBoxSize($"{Lang.Get("game:rustweave-effects")}: {effectsSummary}", TomeDetailFont, EnumTextOrientation.Left, ElementBounds.Fixed(552, 414, 520, 64), $"{pageName}-detail-effects");
            composer.AddStaticText($"{Lang.Get("game:rustweave-spell-state")}: {stateText}", TomeDetailFont, ElementBounds.Fixed(552, 484, 520, 18), $"{pageName}-detail-state");

            if (isLearned)
            {
                composer.AddStaticText($"{Lang.Get("game:rustweave-cast-count")}: {RustweaveStateService.GetSpellCastCount(state, spell.Code)}", TomeDetailFont, ElementBounds.Fixed(552, 506, 520, 18), $"{pageName}-detail-casts");
            }

            if (isHidden)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-hidden-note"), TomeDetailFont, ElementBounds.Fixed(552, 528, 520, 18), $"{pageName}-detail-hidden");
            }

            if (isLore)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-loreweave-note"), TomeDetailFont, ElementBounds.Fixed(552, 550, 520, 18), $"{pageName}-detail-lore");
            }

            if (!isLearned && allowUnlockHint)
            {
                var hint = string.IsNullOrWhiteSpace(spell.UnlockHint) ? Lang.Get("game:rustweave-discovery-required") : spell.UnlockHint;
                composer.AddStaticTextAutoBoxSize($"{Lang.Get("game:rustweave-unlock-hint")}: {hint}", TomeDetailFont, EnumTextOrientation.Left, ElementBounds.Fixed(552, 572, 520, 44), $"{pageName}-detail-hint");
            }
        }

        private void AddPreparedTab(GuiComposer composer)
        {
            var panelBounds = ElementBounds.Fixed(28, 142, 1084, 584);
            composer.AddInset(panelBounds, 3, 0.9f);
            composer.AddStaticText(Lang.Get("game:rustweave-prepared-spells"), TomeTitleFont, ElementBounds.Fixed(36, 146, 280, 20), "prepared-header");

            var listBounds = ElementBounds.Fixed(36, 172, 1048, 506);
            composer.BeginChildElements(listBounds);
            for (var slotIndex = 0; slotIndex < RustweaveConstants.PreparedSlotCount; slotIndex++)
            {
                AddPreparedSlotRow(composer, slotIndex);
            }
            composer.EndChildElements();
        }

        private void AddPreparedSlotRow(GuiComposer composer, int slotIndex)
        {
            var rowTop = slotIndex * 52;
            var isSelected = state.SelectedPreparedSpellIndex == slotIndex;
            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            var hasSpell = !string.IsNullOrWhiteSpace(spellCode);
            var spellExists = hasSpell && RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell != null;
            var spellName = !hasSpell
                ? Lang.Get("game:rustweave-slot-empty")
                : spellExists
                    ? RustweaveStateService.GetSpellDisplayName(spellCode)
                    : $"{spellCode} ({Lang.Get("game:rustweave-spell-invalid")})";
            composer.AddStaticText($"#{slotIndex + 1}", TomeListFont.WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, rowTop + 3, 30, 16), $"prepared-slot-number-{slotIndex}");

            composer.AddButton("Select", () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(40, rowTop, 48, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-select-{slotIndex}");

            composer.AddButton("Clear", () =>
            {
                RustweaveRuntime.Client?.RequestUnprepareSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(100, rowTop, 48, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-clear-{slotIndex}");

            composer.AddStaticText(isSelected ? Lang.Get("game:rustweave-active-slot") : string.Empty, TomeListFont.WithColor(isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DisabledTextColor), ElementBounds.Fixed(156, rowTop + 3, 58, 16), $"prepared-active-{slotIndex}");

            composer.AddButton(isSelected ? $"* {spellName}" : spellName, () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(228, rowTop, 820, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-slot-label-{slotIndex}");
        }

        private void NormalizeSelectedSpellForActiveTab()
        {
            if (activeTab == TomeTab.Prepared)
            {
                return;
            }

            var spells = GetSpellsForTab(activeTab);
            NormalizeSelectedSpellForActiveTab(spells);
        }

        private void NormalizeSelectedSpellForActiveTab(IReadOnlyList<SpellDefinition> spells)
        {
            if (spells.Count == 0)
            {
                if (activeTab != TomeTab.Prepared)
                {
                    selectedSpellCode = string.Empty;
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(selectedSpellCode) || spells.All(spell => !string.Equals(spell.Code, selectedSpellCode, StringComparison.OrdinalIgnoreCase)))
            {
                selectedSpellCode = spells[0].Code ?? string.Empty;
            }
        }

        private SpellDefinition? GetDisplayedSpell(IReadOnlyList<SpellDefinition> spells)
        {
            if (spells.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(selectedSpellCode))
            {
                var selectedSpell = spells.FirstOrDefault(spell => string.Equals(spell.Code, selectedSpellCode, StringComparison.OrdinalIgnoreCase));
                if (selectedSpell != null)
                {
                    return selectedSpell;
                }
            }

            return spells[0];
        }

        private List<SpellDefinition> GetSpellsForTab(TomeTab tab)
        {
            return tab switch
            {
                TomeTab.All => GetAllSpells(),
                TomeTab.Learned => GetLearnedSpells(),
                TomeTab.Locked => GetLockedSpells(),
                TomeTab.Loreweave => GetLoreweaveSpells(),
                TomeTab.Prepared => Array.Empty<SpellDefinition>().ToList(),
                _ => Array.Empty<SpellDefinition>().ToList()
            };
        }

        private List<SpellDefinition> GetAllSpells()
        {
            return RustweaveStateService.GetAllVisibleSpells(capi.World?.Player).ToList();
        }

        private List<SpellDefinition> GetLearnedSpells()
        {
            return RustweaveStateService.GetLearnedSpells(capi.World?.Player).ToList();
        }

        private List<SpellDefinition> GetLockedSpells()
        {
            return RustweaveStateService.GetLockedSpells(capi.World?.Player).ToList();
        }

        private List<SpellDefinition> GetLoreweaveSpells()
        {
            return RustweaveStateService.GetLoreweaveSpells(capi.World?.Player).ToList();
        }

        private bool CanPrepareSpell(SpellDefinition spell)
        {
            return spell != null && !string.IsNullOrWhiteSpace(spell.Code) && RustweaveStateService.IsSpellLearned(spell.Code, state);
        }

        private string GetStatusHeaderText()
        {
            var learnedCount = RustweaveRuntime.SpellRegistry.GetEnabledSpells()
                .Count(spell => spell != null && !string.IsNullOrWhiteSpace(spell.Code) && !RustweaveStateService.IsLoreweaveSpell(spell) && RustweaveStateService.IsSpellLearned(spell.Code, state));

            var rankName = RustweaveStateService.GetKnowledgeRankName(state);
            var testingMode = RustweaveConstants.AllSpellsLearnedByDefaultForTesting
                ? $" | {Lang.Get("game:rustweave-testing-mode-active")}"
                : string.Empty;

            return $"{Lang.Get("game:rustweave-knowledge-rank")}: {rankName}\n{Lang.Get("game:rustweave-learned-count")}: {learnedCount}{testingMode}";
        }

        private static string GetEffectsSummary(SpellDefinition spell)
        {
            var effects = spell.Effects?
                .Select(effect => effect?.Type ?? string.Empty)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .ToList() ?? new List<string>();

            return effects.Count == 0 ? Lang.Get("game:rustweave-slot-empty") : string.Join(", ", effects);
        }

        private static string GetTierDisplayLabel(int tier)
        {
            return tier switch
            {
                1 => "Initiate Spell",
                2 => "Apprentice Spell",
                3 => "Adept Spell",
                4 => "Expert Spell",
                5 => "Master Spell",
                6 => "Ascendant Spell",
                _ => "Initiate Spell"
            };
        }

        private string GetSpellStateLabel(SpellDefinition spell)
        {
            if (RustweaveStateService.IsLoreweaveSpell(spell))
            {
                return Lang.Get("game:rustweave-spell-state-lore");
            }

            return RustweaveStateService.IsSpellLearned(spell.Code, state)
                ? Lang.Get("game:rustweave-spell-state-learned")
                : Lang.Get("game:rustweave-spell-state-locked");
        }
    }
}

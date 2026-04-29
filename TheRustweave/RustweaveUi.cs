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
        public const float TargetLockHudWidth = 360f;
        public const float TargetLockHudHeight = 56f;
        public const float CastHudSpacing = 4f;

        public const float CorruptionOffsetX = 525f;
        public const float CorruptionOffsetY = -80f;

        public static Vec2d CorruptionOffset => new(CorruptionOffsetX, CorruptionOffsetY);

        public static Vec2d CastOffset => new(CorruptionOffsetX, CorruptionOffsetY - CorruptionHudHeight - CastHudSpacing);

        public static Vec2d TargetLockOffset => new(CorruptionOffsetX, CorruptionOffsetY - CorruptionHudHeight - CastHudSpacing - CastHudHeight - CastHudSpacing - TargetLockHudHeight);
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

    internal sealed class RustweaveTargetLockHud : HudElement
    {
        private RustweaveTargetLockPacket targetLock = new();
        private RustweaveCastStateData castState = new();
        private RustweavePlayerStateData playerState = RustweaveStateService.CreateDefaultState();
        private GuiElementDynamicText? targetLabel;

        public RustweaveTargetLockHud(ICoreClientAPI capi) : base(capi)
        {
        }

        public override bool Focusable => false;

        public override bool PrefersUngrabbedMouse => true;

        public override bool UnregisterOnClose => false;

        public override string ToggleKeyCombinationCode => string.Empty;

        public void SetState(RustweaveTargetLockPacket? newTargetLock, RustweaveCastStateData? newCastState, RustweavePlayerStateData? newPlayerState)
        {
            targetLock = newTargetLock?.Clone() ?? new RustweaveTargetLockPacket();
            castState = newCastState?.Clone() ?? new RustweaveCastStateData();
            playerState = RustweaveStateService.NormalizeState(newPlayerState?.Clone() ?? RustweaveStateService.CreateDefaultState());
            targetLabel?.SetNewText(GetTargetText(), false, true);
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

            var rootBounds = ElementBounds.FixedSize(RustweaveHudLayout.TargetLockHudWidth, RustweaveHudLayout.TargetLockHudHeight)
                .WithAlignment(EnumDialogArea.LeftBottom)
                .WithFixedOffset(GuiStyle.DialogToScreenPadding + RustweaveHudLayout.TargetLockOffset.X, RustweaveHudLayout.TargetLockOffset.Y);

            var textBounds = ElementBounds.Fixed(12, 10, RustweaveHudLayout.TargetLockHudWidth - 24, RustweaveHudLayout.TargetLockHudHeight - 20);
            var composer = capi.Gui.CreateCompo("therustweave-target-lock-hud", rootBounds);
            composer.AddDialogBG(ElementBounds.Fixed(0, 0, RustweaveHudLayout.TargetLockHudWidth, RustweaveHudLayout.TargetLockHudHeight), true, 0.95f);
            composer.AddDynamicText(GetTargetText(), CairoFont.WhiteSmallText().WithFontSize(9.5f), textBounds, "targetlocklabel");
            composer.Compose();
            SingleComposer = composer;
            targetLabel = composer.GetDynamicText("targetlocklabel");
            targetLabel?.SetNewText(GetTargetText(), false, true);
        }

        private string GetTargetText()
        {
            if (targetLock.IsActive && castState.IsCasting)
            {
                return GetLockedTargetText();
            }

            if (!TryGetPreviewSpell(out var previewSpell))
            {
                return string.Empty;
            }

            if (previewSpell == null)
            {
                return string.Empty;
            }

            return GetPreviewTargetText(previewSpell);
        }

        private string GetLockedTargetText()
        {
            var spellName = string.IsNullOrWhiteSpace(targetLock.SpellName) ? targetLock.SpellCode : targetLock.SpellName;
            var range = GetRangeToTarget();
            var targetLabel = SpellTargetTypes.GetDisplayName(targetLock.TargetType);

            return targetLock.TargetType switch
            {
                SpellTargetTypes.Self => $"Locked Target: {targetLabel}\nSpell: {spellName}",
                SpellTargetTypes.HeldItem => $"Locked Target: {targetLabel}\nSpell: {spellName}",
                SpellTargetTypes.Inventory => $"Locked Target: {targetLabel}\nSpell: {spellName}",
                SpellTargetTypes.LookEntity => $"Locked Target: {targetLock.TargetName}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                SpellTargetTypes.LookPlayer => $"Locked Target: {targetLock.TargetName}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                SpellTargetTypes.LookNonPlayerEntity => $"Locked Target: {targetLock.TargetName}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                SpellTargetTypes.LookDroppedItem => $"Locked Target: {targetLock.TargetName}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                SpellTargetTypes.LookBlock => $"Locked Target: {targetLabel}\nSpell: {spellName}\nPosition: {targetLock.TargetX:0.0}, {targetLock.TargetY:0.0}, {targetLock.TargetZ:0.0}",
                SpellTargetTypes.LookBlockEntity => $"Locked Target: {targetLabel}\nSpell: {spellName}\nPosition: {targetLock.TargetX:0.0}, {targetLock.TargetY:0.0}, {targetLock.TargetZ:0.0}",
                SpellTargetTypes.LookContainer => $"Locked Target: {targetLabel}\nSpell: {spellName}\nPosition: {targetLock.TargetX:0.0}, {targetLock.TargetY:0.0}, {targetLock.TargetZ:0.0}",
                SpellTargetTypes.SelfArea => $"Locked Target: {targetLabel}\nSpell: {spellName}",
                SpellTargetTypes.LookArea => $"Locked Target: {targetLabel}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                SpellTargetTypes.LookPosition => $"Locked Target: {targetLabel}\nSpell: {spellName}\nRange: {range:0.0} blocks",
                _ => string.Empty
            };
        }

        private string GetPreviewTargetText(SpellDefinition previewSpell)
        {
            var spellName = string.IsNullOrWhiteSpace(previewSpell.Name) ? previewSpell.Code : previewSpell.Name;
            var targetTypeLabel = SpellTargetTypes.GetDisplayName(previewSpell.TargetType);

            var range = SpellRegistry.GetEffectiveLookTargetRange(previewSpell);
            if (range > 0 && (SpellTargetTypes.RequiresLookRange(previewSpell.TargetType) || SpellTargetTypes.IsAreaTarget(previewSpell.TargetType)))
            {
                return $"Aiming: {targetTypeLabel}\nSpell: {spellName}\nRange: {range:0.0} blocks";
            }

            return $"Aiming: {targetTypeLabel}\nSpell: {spellName}";
        }

        private bool TryGetPreviewSpell(out SpellDefinition? spell)
        {
            spell = null;
            var player = capi.World?.Player;
            if (player == null || !RustweaveStateService.IsRustweaver(player) || !RustweaveStateService.IsHoldingTome(player))
            {
                return false;
            }

            var spellCode = RustweaveStateService.GetSelectedPreparedSpellCode(playerState);
            if (string.IsNullOrWhiteSpace(spellCode))
            {
                return false;
            }

            if (!RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out spell) || spell == null)
            {
                return false;
            }

            return true;
        }

        private double GetRangeToTarget()
        {
            var player = capi.World?.Player?.Entity;
            if (player == null)
            {
                return 0d;
            }

            var dx = player.Pos.XYZ.X - targetLock.TargetX;
            var dy = player.Pos.XYZ.Y - targetLock.TargetY;
            var dz = player.Pos.XYZ.Z - targetLock.TargetZ;
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
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
        private const int TomeSpellRowHeight = 44;
        private const int TomeSpellRowGap = 6;
        private const int TomeSpellRowStride = TomeSpellRowHeight + TomeSpellRowGap;
        private const int TomeSpellListTopPadding = 8;
        private const int TomeSpellListBottomPadding = 8;
        private const int TomeBrowseListLeft = 36;
        private const int TomeBrowseListTop = 188;
        private const int TomeBrowseListWidth = 500;
        private const int TomeBrowseListHeight = 470;
        private const int TomeBrowseDetailsLeft = 552;
        private const int TomeBrowseDetailsWidth = 520;
        private const int TomeSearchInputHeight = 22;
        private const int TomeSearchHeaderGap = 8;
        private const int TomeScrollbarWidth = 14;
        private const int TomeSearchRefreshDelayMs = 700;
        private const string MainComposerKey = "rustweave-tome-main";

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
        private string allSearchText = string.Empty;
        private string learnedSearchText = string.Empty;
        private string lockedSearchText = string.Empty;
        private string loreSearchText = string.Empty;
        private float allScrollY;
        private float learnedScrollY;
        private float lockedScrollY;
        private float loreScrollY;
        private bool rebuildQueued;
        private bool rebuilding;
        private bool suppressSearchCallbacks;
        private TomeTab? pendingSearchRefreshTab;
        private long lastSearchChangeAtMs;
        private readonly long searchRefreshTickListenerId;

        public RustweaveSpellPrepDialog(ICoreClientAPI capi) : base(capi)
        {
            capi.Gui.RegisterDialog(this);
            searchRefreshTickListenerId = capi.Event.RegisterGameTickListener(OnSearchRefreshTick, 100, 100);
        }

        public override string ToggleKeyCombinationCode => string.Empty;

        public override bool Focusable => true;

        public override bool PrefersUngrabbedMouse => true;

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
            if (rebuildQueued || rebuilding)
            {
                return;
            }

            rebuildQueued = true;
            capi.Event.EnqueueMainThreadTask(() =>
            {
                rebuildQueued = false;
                rebuilding = true;
                try
                {
                    RebuildDialogNow();
                }
                finally
                {
                    rebuilding = false;
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

        public override void OnGuiClosed()
        {
            DisposeMainComposer();
            rebuildQueued = false;
            rebuilding = false;
            base.OnGuiClosed();
        }

        private GuiComposer? GetMainComposerSafe()
        {
            if (Composers == null || !Composers.ContainsKey(MainComposerKey))
            {
                return null;
            }

            return Composers[MainComposerKey];
        }

        private void DisposeMainComposer()
        {
            try
            {
                var composer = GetMainComposerSafe();
                if (composer != null)
                {
                    composer.Dispose();
                }

                if (Composers != null && Composers.ContainsKey(MainComposerKey))
                {
                    Composers.Remove(MainComposerKey);
                }
            }
            catch (Exception ex)
            {
                capi?.Logger?.Warning("[TheRustweave] Failed while disposing Tome composer: {0}", ex);
            }
        }

        private void RebuildDialogNow()
        {
            try
            {
                var newComposer = BuildMainComposer();
                if (newComposer == null)
                {
                    capi.Logger.Error("[TheRustweave] Tome rebuild failed: BuildMainComposer returned null.");
                    return;
                }

                newComposer.Compose();

                if (Composers == null)
                {
                    capi.Logger.Error("[TheRustweave] Tome rebuild failed: Composers dictionary is null.");
                    newComposer.Dispose();
                    return;
                }

                DisposeMainComposer();
                Composers[MainComposerKey] = newComposer;
                capi.Logger.Debug("[TheRustweave] Tome UI compose complete.");
            }
            catch (Exception ex)
            {
                capi.Logger.Error("[TheRustweave] Tome rebuild crashed: {0}", ex);
                capi.ShowChatMessage("The Rustweaver's Tome failed to rebuild. Check the client log.");
            }
        }

        private void EnsureComposer()
        {
            if (capi?.Gui == null || GetMainComposerSafe() != null)
            {
                return;
            }

            NormalizeSelectedSpellForActiveTab();
            var composer = BuildMainComposer();
            if (composer == null)
            {
                capi.Logger.Error("[TheRustweave] Tome build failed: BuildMainComposer returned null.");
                return;
            }

            composer.Compose();

            if (Composers == null)
            {
                capi.Logger.Error("[TheRustweave] Tome build failed: Composers dictionary is null.");
                composer.Dispose();
                return;
            }

            Composers[MainComposerKey] = composer;
        }

        private GuiComposer? BuildMainComposer()
        {
            if (capi?.Gui == null)
            {
                return null;
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

            var composer = capi.Gui.CreateCompo(MainComposerKey, rootBounds);
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
                    AddSpellBrowseTab(composer, TomeTab.All, Lang.Get("game:rustweave-all-spells"), GetAllSpells(), allowPrepare: true, allowUnlockHint: true, showLoreNote: false, showStateLabel: true, showTierLabel: false, pageName: "all");
                    break;
                case TomeTab.Learned:
                    AddSpellBrowseTab(composer, TomeTab.Learned, Lang.Get("game:rustweave-learned-spells"), GetLearnedSpells(), allowPrepare: true, allowUnlockHint: false, showLoreNote: false, showStateLabel: false, showTierLabel: true, pageName: "learned");
                    break;
                case TomeTab.Locked:
                    AddSpellBrowseTab(composer, TomeTab.Locked, Lang.Get("game:rustweave-locked-spells"), GetLockedSpells(), allowPrepare: false, allowUnlockHint: true, showLoreNote: false, showStateLabel: true, showTierLabel: false, pageName: "locked");
                    break;
                case TomeTab.Loreweave:
                    AddSpellBrowseTab(composer, TomeTab.Loreweave, Lang.Get("game:rustweave-loreweave-spells"), GetLoreweaveSpells(), allowPrepare: true, allowUnlockHint: false, showLoreNote: true, showStateLabel: true, showTierLabel: false, pageName: "lore");
                    break;
                case TomeTab.Prepared:
                    AddPreparedTab(composer);
                    break;
            }

            return composer;
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

        private string GetSearchTextForTab(TomeTab tab)
        {
            return tab switch
            {
                TomeTab.All => allSearchText,
                TomeTab.Learned => learnedSearchText,
                TomeTab.Locked => lockedSearchText,
                TomeTab.Loreweave => loreSearchText,
                _ => string.Empty
            };
        }

        private void SetSearchTextForTab(TomeTab tab, string value)
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (string.Equals(GetSearchTextForTab(tab), normalized, StringComparison.Ordinal))
            {
                return;
            }

            switch (tab)
            {
                case TomeTab.All:
                    allSearchText = normalized;
                    break;
                case TomeTab.Learned:
                    learnedSearchText = normalized;
                    break;
                case TomeTab.Locked:
                    lockedSearchText = normalized;
                    break;
                case TomeTab.Loreweave:
                    loreSearchText = normalized;
                    break;
            }

            pendingSearchRefreshTab = tab;
            lastSearchChangeAtMs = capi.World?.ElapsedMilliseconds ?? 0;
        }

        private float GetScrollYForTab(TomeTab tab)
        {
            return tab switch
            {
                TomeTab.All => allScrollY,
                TomeTab.Learned => learnedScrollY,
                TomeTab.Locked => lockedScrollY,
                TomeTab.Loreweave => loreScrollY,
                _ => 0f
            };
        }

        private void SetScrollYForTab(TomeTab tab, float value)
        {
            var normalized = Math.Max(0f, value);
            switch (tab)
            {
                case TomeTab.All:
                    allScrollY = normalized;
                    break;
                case TomeTab.Learned:
                    learnedScrollY = normalized;
                    break;
                case TomeTab.Locked:
                    lockedScrollY = normalized;
                    break;
                case TomeTab.Loreweave:
                    loreScrollY = normalized;
                    break;
            }
        }

        private void ResetScrollForTab(TomeTab tab)
        {
            SetScrollYForTab(tab, 0f);
        }

        private void OnSearchRefreshTick(float dt)
        {
            if (!IsOpened() || pendingSearchRefreshTab == null)
            {
                return;
            }

            var now = capi.World?.ElapsedMilliseconds ?? 0;
            if (now - lastSearchChangeAtMs < TomeSearchRefreshDelayMs)
            {
                return;
            }

            pendingSearchRefreshTab = null;
            RequestRebuild();
        }

        private List<SpellDefinition> FilterSpellsForSearch(IEnumerable<SpellDefinition> spells, string searchText)
        {
            var list = spells.Where(spell => spell != null).ToList();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return list;
            }

            var normalized = searchText.Trim();
            return list.Where(spell => SpellMatchesSearch(spell, normalized)).ToList();
        }

        private static bool SpellMatchesSearch(SpellDefinition spell, string searchText)
        {
            static bool Contains(string? value, string search) => !string.IsNullOrWhiteSpace(value) && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            if (spell == null)
            {
                return false;
            }

            if (Contains(spell.Name, searchText) || Contains(spell.Code, searchText) || Contains(spell.School, searchText) || Contains(spell.Category, searchText) || Contains(spell.Description, searchText) || Contains(spell.TargetType, searchText))
            {
                return true;
            }

            return spell.Effects?.Any(effect => effect != null && Contains(effect.Type, searchText)) == true;
        }

        private void AddSpellBrowseTab(GuiComposer composer, TomeTab tab, string pageTitle, List<SpellDefinition> spells, bool allowPrepare, bool allowUnlockHint, bool showLoreNote, bool showStateLabel, bool showTierLabel, string pageName)
        {
            var searchText = GetSearchTextForTab(tab);
            var filteredSpells = FilterSpellsForSearch(spells, searchText);
            NormalizeSelectedSpellForActiveTab(filteredSpells);
            var selectedSpell = GetDisplayedSpell(filteredSpells);

            var listPanelBounds = ElementBounds.Fixed(28, 142, 520, 580);
            var detailsPanelBounds = ElementBounds.Fixed(TomeBrowseDetailsLeft, 142, TomeBrowseDetailsWidth, 580);
            composer.AddInset(listPanelBounds, 3, 0.9f);
            composer.AddInset(detailsPanelBounds, 3, 0.9f);
            composer.AddStaticText(pageTitle, TomeTitleFont, ElementBounds.Fixed(36, 146, 480, 20), $"{pageName}-header");

            composer.AddStaticText(Lang.Get("game:rustweave-search") + ":", TomeListFont, ElementBounds.Fixed(36, 170, 58, 18), $"{pageName}-search-label");
            composer.AddTextInput(ElementBounds.Fixed(94, 168, 382, TomeSearchInputHeight), value =>
            {
                if (suppressSearchCallbacks)
                {
                    return;
                }

                SetSearchTextForTab(tab, value);
                ResetScrollForTab(tab);
            }, TomeListFont, $"{pageName}-search");

            var searchInput = composer.GetTextInput($"{pageName}-search");
            if (searchInput != null)
            {
                suppressSearchCallbacks = true;
                try
                {
                    searchInput.SetPlaceHolderText(Lang.Get("game:rustweave-search-placeholder"));
                    searchInput.SetValue(searchText ?? string.Empty, false);
                }
                finally
                {
                    suppressSearchCallbacks = false;
                }
            }

            if (showLoreNote)
            {
                composer.AddStaticTextAutoBoxSize(Lang.Get("game:rustweave-loreweave-note"), TomeListFont, EnumTextOrientation.Left, ElementBounds.Fixed(36, 196, 470, 28), $"{pageName}-note");
            }

            var listTop = showLoreNote ? 228 : 204;
            var visibleListHeight = 468;
            var listBounds = ElementBounds.Fixed(TomeBrowseListLeft, listTop, TomeBrowseListWidth, visibleListHeight);
            var clipBounds = ElementBounds.Fixed(0, 0, TomeBrowseListWidth - TomeScrollbarWidth - 14, visibleListHeight);
            var scrollKey = $"{pageName}-scroll";
            var totalListHeight = Math.Max(visibleListHeight, TomeSpellListTopPadding + (filteredSpells.Count * TomeSpellRowStride) + TomeSpellListBottomPadding);
            var scrollY = Math.Min(GetScrollYForTab(tab), Math.Max(0, totalListHeight - visibleListHeight));
            SetScrollYForTab(tab, scrollY);

            composer.AddVerticalScrollbar(value =>
            {
                if (suppressSearchCallbacks)
                {
                    return;
                }

                SetScrollYForTab(tab, value);
                RequestRebuild();
            }, ElementBounds.Fixed(TomeBrowseListLeft + TomeBrowseListWidth - TomeScrollbarWidth, listTop, TomeScrollbarWidth, visibleListHeight), scrollKey);

            composer.BeginChildElements(listBounds);
            composer.BeginClip(clipBounds);
            var scrollbar = composer.GetScrollbar(scrollKey);
            scrollbar?.SetHeights(visibleListHeight, totalListHeight);
            if (scrollbar != null)
            {
                scrollbar.CurrentYPosition = scrollY;
                scrollbar.Enabled = totalListHeight > visibleListHeight;
            }

            if (filteredSpells.Count == 0)
            {
                composer.AddStaticText(Lang.Get("game:rustweave-no-matches"), TomeListFont.WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, 12, 340, 18), $"{pageName}-empty");
            }
            else
            {
                var firstVisible = Math.Max(0, (int)Math.Floor(scrollY / TomeSpellRowStride) - 1);
                var visibleCount = (int)Math.Ceiling(visibleListHeight / (double)TomeSpellRowStride) + 2;
                var lastVisible = Math.Min(filteredSpells.Count, firstVisible + visibleCount);
                for (var index = firstVisible; index < lastVisible; index++)
                {
                    AddSpellRow(composer, filteredSpells[index], index, allowPrepare, showStateLabel, showTierLabel, pageName, scrollY, visibleListHeight);
                }
            }
            composer.EndClip();
            composer.EndChildElements();

            AddSpellDetailsPanel(composer, selectedSpell, allowUnlockHint, pageName);
        }

        private void AddSpellRow(GuiComposer composer, SpellDefinition spell, int index, bool allowPrepare, bool showStateLabel, bool showTierLabel, string pageName, float scrollY, int visibleListHeight)
        {
            var code = spell.Code ?? string.Empty;
            var rowTop = TomeSpellListTopPadding + (index * TomeSpellRowStride) - scrollY;
            if (rowTop < -TomeSpellRowStride || rowTop > visibleListHeight + TomeSpellRowStride)
            {
                return;
            }

            var isSelected = string.Equals(code, selectedSpellCode, StringComparison.OrdinalIgnoreCase);
            var name = string.IsNullOrWhiteSpace(spell.Name) ? code : spell.Name;
            var stateLabel = GetSpellStateLabel(spell);
            var tierLabel = GetTierDisplayLabel(spell.Tier);
            var displayName = isSelected ? $"> {name} <" : name;
            var canPrepare = allowPrepare && CanPrepareSpell(spell);
            var nameWidth = showStateLabel || showTierLabel ? 260 : 296;

            composer.AddButton(displayName, () =>
            {
                selectedSpellCode = code;
                capi.Logger.Debug("[TheRustweave] Spell selected: {0} ({1}).", code, stateLabel);
                RequestRebuild();
                return true;
            }, ElementBounds.Fixed(0, rowTop, nameWidth, 21), TomeButtonFont, EnumButtonStyle.Normal, $"{pageName}-spell-{index}");

            if (showStateLabel)
            {
                composer.AddStaticText(stateLabel, TomeListFont, ElementBounds.Fixed(272, rowTop + 2, 118, 18), $"{pageName}-state-{index}");
            }
            else if (showTierLabel)
            {
                composer.AddStaticText(tierLabel, TomeListFont, ElementBounds.Fixed(272, rowTop + 2, 118, 18), $"{pageName}-tier-{index}");
            }

            if (canPrepare)
            {
                composer.AddButton("Prep", () =>
                {
                    RustweaveRuntime.Client?.RequestPrepareSpell(code, null);
                    return true;
                }, ElementBounds.Fixed(402, rowTop, 48, 21), TomeButtonFont, EnumButtonStyle.Normal, $"{pageName}-prep-{index}");
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
            for (var slotId = 1; slotId <= RustweaveConstants.PreparedSlotCount; slotId++)
            {
                AddPreparedSlotRow(composer, slotId);
            }
            composer.EndChildElements();
        }

        private void AddPreparedSlotRow(GuiComposer composer, int slotId)
        {
            var rowTop = (slotId - 1) * 52;
            var isSelected = state.ActivePreparedSlotId == slotId;
            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, (int?)slotId);
            var hasSpell = !string.IsNullOrWhiteSpace(spellCode);
            var spellExists = hasSpell && RustweaveRuntime.SpellRegistry.TryGetSpell(spellCode, out var spell) && spell != null;
            var spellName = !hasSpell
                ? Lang.Get("game:rustweave-slot-empty")
                : spellExists
                    ? RustweaveStateService.GetSpellDisplayName(spellCode)
                    : $"{spellCode} ({Lang.Get("game:rustweave-spell-invalid")})";
            composer.AddStaticText($"{slotId}:", TomeListFont.WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(0, rowTop + 3, 30, 16), $"prepared-slot-number-{slotId}");

            composer.AddButton("Select", () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotId);
                return true;
            }, ElementBounds.Fixed(40, rowTop, 48, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-select-{slotId}");

            composer.AddButton("Clear", () =>
            {
                RustweaveRuntime.Client?.RequestUnprepareSpell(slotId);
                return true;
            }, ElementBounds.Fixed(100, rowTop, 48, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-clear-{slotId}");

            composer.AddStaticText(isSelected ? Lang.Get("game:rustweave-active-slot") : string.Empty, TomeListFont.WithColor(isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DisabledTextColor), ElementBounds.Fixed(156, rowTop + 3, 58, 16), $"prepared-active-{slotId}");

            composer.AddButton(isSelected ? $"* {spellName}" : spellName, () =>
            {
                RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotId);
                return true;
            }, ElementBounds.Fixed(228, rowTop, 820, 22), TomeButtonFont, EnumButtonStyle.Normal, $"prepared-slot-label-{slotId}");
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

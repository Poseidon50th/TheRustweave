using System;
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
        private RustweavePlayerStateData state = RustweaveStateService.CreateDefaultState();

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
            if (IsOpened())
            {
                SingleComposer?.ReCompose();
            }
        }

        public void OpenDialog()
        {
            EnsureComposer();
            TryOpen();
        }

        public override bool TryOpen()
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

            var rootBounds = ElementBounds.FixedSize(560, 372).WithAlignment(EnumDialogArea.CenterMiddle);
            var contentLeft = 16;
            var contentTop = (int)GuiStyle.TitleBarHeight + 12;
            var width = 528;
            var contentBounds = ElementBounds.Fixed(12, contentTop - 2, 536, 310);

            var composer = capi.Gui.CreateCompo("therustweave-spell-prep", rootBounds);
            composer.AddDialogBG(rootBounds, false, 1f);
            composer.AddDialogTitleBar(Lang.Get("game:rustweave-prep-title"), () => TryClose(), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(0, 0, 560, GuiStyle.TitleBarHeight));
            composer.AddInset(contentBounds, 4, 0.65f);

            composer.AddStaticText(Lang.Get("game:rustweave-learned-spells"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(contentLeft, contentTop, width, 20), "learnedheader");
            composer.AddStaticTextAutoBoxSize(GetLearnedSpellSummary(), CairoFont.WhiteSmallText(), EnumTextOrientation.Left, ElementBounds.Fixed(contentLeft, contentTop + 18, 372, 42), "learnedsummary");
            composer.AddButton(Lang.Get("game:rustweave-prepare"), OnPrepareDummySpell, ElementBounds.Fixed(contentLeft + 384, contentTop + 14, 66, 20), EnumButtonStyle.Normal, "preparebutton");

            var preparedHeaderY = contentTop + 72;
            composer.AddStaticText(Lang.Get("game:rustweave-prepared-spells"), CairoFont.WhiteSmallishText(), ElementBounds.Fixed(contentLeft, preparedHeaderY, width, 20), "preparedheader");

            for (var slotIndex = 0; slotIndex < RustweaveConstants.PreparedSlotCount; slotIndex++)
            {
                AddPreparedSlotRow(composer, contentLeft, preparedHeaderY + 20 + (slotIndex * 23), slotIndex);
            }

            composer.Compose();
            SingleComposer = composer;
        }

        private bool OnPrepareDummySpell()
        {
            RustweaveRuntime.Client?.RequestPrepareSpell(RustweaveRuntime.SpellRegistry.StarterSpellCode, -1);
            return true;
        }

        private void AddPreparedSlotRow(GuiComposer composer, int left, int top, int slotIndex)
        {
            var isSelected = state.SelectedPreparedSpellIndex == slotIndex;
            var spellCode = RustweaveStateService.GetPreparedSpellCode(state, slotIndex);
            var hasSpell = !string.IsNullOrWhiteSpace(spellCode);

            composer.AddStaticText(Lang.Get("game:rustweave-prepared-slot", slotIndex + 1), CairoFont.WhiteSmallText(), ElementBounds.Fixed(left, top, 66, 18), $"slotlabel-{slotIndex}");

            if (hasSpell)
            {
                var spellName = RustweaveStateService.GetSpellDisplayName(spellCode);
                var color = isSelected ? GuiStyle.ActiveButtonTextColor : GuiStyle.DialogDefaultTextColor;
                composer.AddStaticText(spellName, CairoFont.WhiteSmallText().WithColor(color), ElementBounds.Fixed(left + 70, top, 208, 18), $"slotspell-{slotIndex}");
                composer.AddButton(isSelected ? Lang.Get("game:rustweave-selected") : Lang.Get("game:rustweave-select"), () =>
                {
                    RustweaveRuntime.Client?.RequestSelectPreparedSpell(slotIndex);
                    return true;
                }, ElementBounds.Fixed(left + 292, top - 1, 54, 18), EnumButtonStyle.Normal, $"select-{slotIndex}");
                composer.AddButton(Lang.Get("game:rustweave-unprepare"), () =>
                {
                    RustweaveRuntime.Client?.RequestUnprepareSpell(slotIndex);
                    return true;
                }, ElementBounds.Fixed(left + 350, top - 1, 54, 18), EnumButtonStyle.Normal, $"clear-{slotIndex}");
            }
            else
            {
                composer.AddStaticText(Lang.Get("game:rustweave-slot-empty"), CairoFont.WhiteSmallText().WithColor(GuiStyle.DisabledTextColor), ElementBounds.Fixed(left + 70, top, 208, 18), $"slotempty-{slotIndex}");
            }
        }

        private string GetLearnedSpellSummary()
        {
            var spellCode = RustweaveRuntime.SpellRegistry.StarterSpellCode;
            var spellName = RustweaveStateService.GetSpellDisplayName(spellCode);
            var spellDesc = RustweaveStateService.GetSpellDescription(spellCode);
            var isPrepared = RustweaveStateService.FindPreparedSpellSlot(state, spellCode) >= 0;
            var status = isPrepared ? Lang.Get("game:rustweave-prepared-status") : Lang.Get("game:rustweave-not-prepared-status");
            return $"{spellName}\n{spellDesc}\n{status}";
        }
    }
}

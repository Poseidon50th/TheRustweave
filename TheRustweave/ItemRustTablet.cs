using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace TheRustweave
{
    public sealed class ItemRustTablet : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (!firstEvent || byEntity is not EntityPlayer entityPlayer)
            {
                return;
            }

            handling = EnumHandHandling.PreventDefault;

            if (byEntity.World.Api.Side == EnumAppSide.Client)
            {
                if (byEntity.World.Api is not ICoreClientAPI clientApi || clientApi.World.Player == null)
                {
                    return;
                }

                if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
                {
                    clientApi.ShowChatMessage(Lang.Get("game:rustweave-rusttablet-reject"));
                    return;
                }

                var tabletStored = RustweaveStateService.GetTabletDisplayedCorruption(clientApi.World, slot?.Itemstack);
                if (tabletStored >= RustweaveConstants.TabletCapacity)
                {
                    clientApi.ShowChatMessage(Lang.Get("game:rustweave-rusttablet-full"));
                    return;
                }

                if (RustweaveStateService.TryGetClientState(clientApi.World.Player, out var playerState) &&
                    playerState.CurrentTemporalCorruption > playerState.EffectiveTemporalCorruptionThreshold)
                {
                    clientApi.ShowChatMessage(Lang.Get("game:rustweave-rusttablet-overloaded"));
                    return;
                }

                return;
            }

            if (byEntity.World.Api.Side != EnumAppSide.Server)
            {
                return;
            }

            RustweaveRuntime.Server?.TryStartTabletVenting(entityPlayer);
        }

        public override void OnGroundIdle(EntityItem entityItem)
        {
            if (entityItem?.World?.Api?.Side != EnumAppSide.Server)
            {
                return;
            }

            if (entityItem.Itemstack == null)
            {
                return;
            }

            if (RustweaveStateService.ApplyTabletPassiveDecay(entityItem.World, entityItem.Itemstack))
            {
                entityItem.Itemstack = entityItem.Itemstack;
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World.Api.Side == EnumAppSide.Client)
            {
                if (byEntity.World.Api is not ICoreClientAPI clientApi || clientApi.World.Player == null)
                {
                    return false;
                }

                if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
                {
                    return false;
                }

                if (RustweaveStateService.GetTabletDisplayedCorruption(clientApi.World, slot?.Itemstack) >= RustweaveConstants.TabletCapacity)
                {
                    return false;
                }

                if (RustweaveStateService.TryGetClientState(clientApi.World.Player, out var playerState) &&
                    playerState.CurrentTemporalCorruption > playerState.EffectiveTemporalCorruptionThreshold)
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World.Api.Side != EnumAppSide.Server || byEntity is not EntityPlayer entityPlayer)
            {
                return;
            }

            RustweaveRuntime.Server?.StopTabletVenting(entityPlayer);
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            if (byEntity.World.Api.Side != EnumAppSide.Server || byEntity is not EntityPlayer entityPlayer)
            {
                return true;
            }

            RustweaveRuntime.Server?.StopTabletVenting(entityPlayer);
            return true;
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            var storedCorruption = RustweaveStateService.GetTabletDisplayedCorruption(world, inSlot?.Itemstack);
            if (dsc.Length > 0)
            {
                dsc.AppendLine();
            }

            dsc.Append(Lang.Get("game:rusttablet-tooltip", storedCorruption, RustweaveConstants.TabletCapacity));
            dsc.AppendLine();
            dsc.Append(Lang.Get("game:rusttablet-tooltip-stage", RustweaveStateService.GetTabletStageLabel(storedCorruption)));
        }
    }
}

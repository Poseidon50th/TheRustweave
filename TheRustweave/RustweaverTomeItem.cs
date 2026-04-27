using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace TheRustweave
{
    public sealed class ItemRustweaverTome : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (!firstEvent || byEntity is not EntityPlayer entityPlayer)
            {
                return;
            }

            handling = EnumHandHandling.PreventDefault;

            if (byEntity.World.Api.Side != EnumAppSide.Client)
            {
                return;
            }

            if (byEntity.World.Api is not ICoreClientAPI clientApi)
            {
                return;
            }

            if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
            {
                clientApi.ShowChatMessage(Lang.Get("game:rustweave-tome-reject"));
                return;
            }

            clientApi.ShowChatMessage(Lang.Get("game:rustweave-tome-success"));

            if (entityPlayer.Controls?.ShiftKey == true)
            {
                RustweaveRuntime.Client?.OpenPreparationGui();
                return;
            }

            RustweaveRuntime.Client?.RequestPreviewUpdate(byEntity, blockSel, entitySel);
            RustweaveRuntime.Client?.RequestStartCast();
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World.Api.Side != EnumAppSide.Client)
            {
                return true;
            }

            if (byEntity.World.Api is not ICoreClientAPI clientApi)
            {
                return true;
            }

            if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
            {
                return true;
            }

            if (!RustweaveStateService.IsHoldingTome(clientApi.World.Player))
            {
                return false;
            }

            RustweaveRuntime.Client?.RequestPreviewUpdate(byEntity, blockSel, entitySel);
            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World.Api.Side != EnumAppSide.Client)
            {
                return;
            }

            if (byEntity.World.Api is not ICoreClientAPI clientApi)
            {
                return;
            }

            if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
            {
                return;
            }

            if (RustweaveRuntime.Client?.IsCasting == true)
            {
                RustweaveRuntime.Client?.RequestPreviewStop();
            }
            else
            {
                RustweaveRuntime.Client?.RequestPreviewStop(true);
            }
            // Successful completion is server-driven; do not cancel here.
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            if (byEntity.World.Api.Side != EnumAppSide.Client)
            {
                return true;
            }

            if (byEntity.World.Api is not ICoreClientAPI clientApi)
            {
                return true;
            }

            if (!RustweaveStateService.IsRustweaver(clientApi.World.Player))
            {
                return true;
            }

            RustweaveRuntime.Client?.RequestPreviewStop();
            RustweaveRuntime.Client?.RequestCancelCast();
            return true;
        }
    }
}

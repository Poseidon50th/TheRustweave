using System;
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

            var message = IsRustweaver(entityPlayer)
                ? "The Rust answers your call."
                : "You do not understand the tome.";

            if (byEntity.World.Api is ICoreClientAPI capi)
            {
                capi.ShowChatMessage(message);
            }
        }

        private static bool IsRustweaver(EntityPlayer entityPlayer)
        {
            var classCode =
                entityPlayer.WatchedAttributes?.GetString("characterClass", null) ??
                entityPlayer.WatchedAttributes?.GetString("characterclass", null) ??
                entityPlayer.WatchedAttributes?.GetString("class", null) ??
                string.Empty;

            return string.Equals(classCode, "rustweaver", StringComparison.OrdinalIgnoreCase);
        }
    }
}

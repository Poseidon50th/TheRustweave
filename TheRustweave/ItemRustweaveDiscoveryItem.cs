using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace TheRustweave
{
    public sealed class ItemRustweaveDiscoveryItem : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (!firstEvent || byEntity is not EntityPlayer)
            {
                return;
            }

            handling = EnumHandHandling.PreventDefault;

            if (byEntity.World.Api.Side != EnumAppSide.Server)
            {
                return;
            }

            if (byEntity is not EntityPlayer entityPlayer)
            {
                return;
            }

            if (slot?.Itemstack?.Collectible?.Code?.Path == null)
            {
                return;
            }

            RustweaveRuntime.Server?.TryUseDiscoveryItem(entityPlayer, slot);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            var collectible = inSlot?.Itemstack?.Collectible;
            var itemCode = collectible?.Code?.Path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return;
            }

            if (dsc.Length > 0)
            {
                dsc.AppendLine();
            }

            dsc.Append(Lang.Get($"itemdesc-{itemCode}"));
        }
    }
}

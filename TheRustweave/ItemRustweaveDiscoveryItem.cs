using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

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

            if (byEntity.World.Api.Side == EnumAppSide.Client && byEntity.World.Api is ICoreClientAPI clientApi)
            {
                TrySpawnArcaneNotesParticles(clientApi, slot);
            }

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

        private static void TrySpawnArcaneNotesParticles(ICoreClientAPI capi, ItemSlot slot)
        {
            try
            {
                var itemCode = slot?.Itemstack?.Collectible?.Code?.Path;
                if (!string.Equals(itemCode, "arcane-notes", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var center = capi.World.Player?.Entity?.Pos?.XYZ ?? new Vec3d(0, 0, 0);
                center = new Vec3d(center.X, center.Y + 0.9, center.Z);
                var color = unchecked((int)0xFF9C8BFF);
                var minPos = new Vec3d(center.X - 0.08, center.Y - 0.08, center.Z - 0.08);
                var maxPos = new Vec3d(center.X + 0.08, center.Y + 0.08, center.Z + 0.08);
                var minVelocity = new Vec3f(-0.02f, 0.02f, -0.02f);
                var maxVelocity = new Vec3f(0.02f, 0.06f, 0.02f);
                capi.World.SpawnParticles(4.0f, color, minPos, maxPos, minVelocity, maxVelocity, 0.45f, -0.01f, 0.06f, EnumParticleModel.Quad, capi.World.Player);
            }
            catch (Exception)
            {
                // Visual-only effect; never let particles interfere with item use.
            }
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

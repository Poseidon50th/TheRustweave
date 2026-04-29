using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace TheRustweave
{
    public class TheRustweaveModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemRustweaverTome", typeof(ItemRustweaverTome));
            api.RegisterItemClass("ItemRustTablet", typeof(ItemRustTablet));
            api.RegisterItemClass("ItemRustweaveDiscoveryItem", typeof(ItemRustweaveDiscoveryItem));
            Mod.Logger.Notification("[TheRustweave] Core mod systems initialized.");
            var origin = GetModOrigin();
            if (!string.Equals(origin, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                Mod.Logger.Notification("[TheRustweave] Loaded mod origin: {0}", origin);
            }
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            RustweaveRuntime.LoadSpellRegistry(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            RustweaveRuntime.InitializeServer(api);
            Mod.Logger.Notification("[TheRustweave] Server systems initialized.");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            RustweaveRuntime.InitializeClient(api);
            Mod.Logger.Notification("[TheRustweave] Client systems initialized.");
        }

        public override void Dispose()
        {
            RustweaveRuntime.ShutdownClient();
            RustweaveRuntime.ShutdownServer();
            base.Dispose();
        }

        private string GetModOrigin()
        {
            var info = Mod?.Info;
            if (info == null)
            {
                return "unknown";
            }

            foreach (var propertyName in new[] { "Origin", "SourcePath", "Path", "ModFolderPath" })
            {
                var property = info.GetType().GetProperty(propertyName);
                if (property?.PropertyType != typeof(string))
                {
                    continue;
                }

                var value = property.GetValue(info) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return info.GetType().GetProperty("Name")?.GetValue(info)?.ToString() ?? "unknown";
        }
    }
}

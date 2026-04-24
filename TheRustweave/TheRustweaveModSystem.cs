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
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
            Mod.Logger.Notification("[TheRustweave] Loaded mod origin: {0}", GetModOrigin());
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            RustweaveRuntime.LoadSpellRegistry(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            RustweaveRuntime.InitializeServer(api);
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("therustweave:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            RustweaveRuntime.InitializeClient(api);
            Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("therustweave:hello"));
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

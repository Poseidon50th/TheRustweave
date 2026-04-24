using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace TheRustweave
{
    public class TheRustweaveModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemRustweaverTome", typeof(ItemRustweaverTome));
            api.RegisterItemClass("ItemRustTablet", typeof(ItemRustTablet));
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
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

    }
}

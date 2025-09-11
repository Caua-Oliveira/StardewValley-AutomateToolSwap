using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using Microsoft.Xna.Framework;

namespace OnlyWhenItCounts
{
    public class ModEntry : Mod
    {
        private bool pendingRestore;
        private bool wasUsingTool;
        private float savedStamina;
        private int savedWaterLeft;
        private ModConfig Config;
        internal static IItemExtensionsApi? ItemExtensionsAPI;

        public override void Entry(IModHelper helper)
        {
            DidWork.SetMonitor(this.Monitor);
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ItemExtensionsAPI = Helper.ModRegistry.GetApi<IItemExtensionsApi>("mistyspring.ItemExtensions");
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Mod",
                tooltip: () => "Enable or disable the mod.",
                getValue: () => this.Config.Enabled,
                setValue: value => this.Config.Enabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Return Water To Watering Can",
                tooltip: () => "Returns the water wasted when the watering can doesn't work.",
                getValue: () => this.Config.ReturnWaterToWateringCan,
                setValue: value => this.Config.ReturnWaterToWateringCan = value
            );
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.Enabled)
                return;

            var player = Game1.player;
            bool isSwinging = player.UsingTool;

            if (player.CurrentTool == null)
                return;

            // 1) Tool swing started this tick
            if (!wasUsingTool && isSwinging)
            {
                bool didWork = false;

                Vector2 targetTile = new Vector2(
                    (int)(player.GetToolLocation().X / Game1.tileSize),
                    (int)(player.GetToolLocation().Y / Game1.tileSize)
                );

                Monitor.Log(targetTile.ToString(), LogLevel.Trace);

                switch (player.CurrentTool)
                {
                    case Pickaxe _:
                        didWork = DidWork.Pickaxe(targetTile);
                        break;
                    case Axe _:
                        didWork = DidWork.Axe(targetTile);
                        break;
                    case Hoe _:
                        didWork = DidWork.Hoe(targetTile);
                        break;
                    case FishingRod _:
                        didWork = DidWork.FishingRod(targetTile);
                        break;
                    case WateringCan wateringCan:
                        didWork = DidWork.WateringCan(targetTile);
                        if (!didWork)
                            savedWaterLeft = wateringCan.WaterLeft;
                        break;
                }

                if (didWork)
                {
                    wasUsingTool = true;
                    return;
                }

                savedStamina = player.stamina;
                pendingRestore = true;
            }

            // 2) Tool swing ended this tick
            if (pendingRestore && wasUsingTool && !isSwinging)
            {
                player.stamina = savedStamina;

                if (player.CurrentTool is WateringCan wateringCan && Config.ReturnWaterToWateringCan)
                {
                    wateringCan.WaterLeft = savedWaterLeft;
                }

                pendingRestore = false;
            }

            // 3) Save tool usage state for next tick
            wasUsingTool = isSwinging;
        }
    }
}
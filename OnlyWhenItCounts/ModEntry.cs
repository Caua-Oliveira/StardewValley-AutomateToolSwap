using HarmonyLib; // <-- Add this
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace OnlyWhenItCounts
{
    public class ModEntry : Mod
    {
        private readonly PerScreen<bool> pendingRestore = new();
        private readonly PerScreen<bool> wasUsingTool = new();
        private readonly PerScreen<float> savedStamina = new();
        private readonly PerScreen<int> savedWaterLeft = new();

        public static ModConfig Config { get; private set; }
        internal static IItemExtensionsApi? ItemExtensionsAPI;

        public override void Entry(IModHelper helper)
        {
            DidWork.SetMonitor(this.Monitor);
            Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Initialize Harmony
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ItemExtensionsAPI = Helper.ModRegistry.GetApi<IItemExtensionsApi>("mistyspring.ItemExtensions");
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Mod",
                tooltip: () => "Enable or disable the mod.",
                getValue: () => Config.Enabled,
                setValue: value => Config.Enabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Return Water To Watering Can",
                tooltip: () => "Returns the water wasted when the watering can doesn't work.",
                getValue: () => Config.ReturnWaterToWateringCan,
                setValue: value => Config.ReturnWaterToWateringCan = value
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
            if (!this.wasUsingTool.Value && isSwinging)
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
                        // Force didWork to true so the UpdateTicked loop ignores it.
                        // Harmony patch will handle the actual stamina refunding!
                        didWork = true;
                        break;
                    case WateringCan wateringCan:
                        didWork = DidWork.WateringCan(targetTile);
                        if (!didWork)
                            this.savedWaterLeft.Value = wateringCan.WaterLeft;
                        break;
                }

                if (didWork)
                {
                    this.wasUsingTool.Value = true;
                    return;
                }

                this.savedStamina.Value = player.stamina;
                this.pendingRestore.Value = true;
            }

            // 2) Tool swing ended this tick
            if (this.pendingRestore.Value && this.wasUsingTool.Value && !isSwinging)
            {
                player.stamina = this.savedStamina.Value;

                if (player.CurrentTool is WateringCan wateringCan && Config.ReturnWaterToWateringCan)
                {
                    wateringCan.WaterLeft = this.savedWaterLeft.Value;
                }

                this.pendingRestore.Value = false;
            }

            // 3) Save tool usage state for next tick
            this.wasUsingTool.Value = isSwinging;
        }
    }
}
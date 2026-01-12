
using System.Text.Json;
using Configuration;
using Core;
using Integrations;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.Tiles;


namespace AutomateToolSwap;

public class ModEntry : Mod
{
    internal static Integrations.IItemExtensionsApi? ItemExtensionsAPI;
    internal static ModEntry Instance { get; private set; } = null!;
    internal static ModConfig Config { get; private set; } = null!;
    internal static IventoryIndexMemory inventoryIndexMemory = new();
    internal static ITranslationHelper? i18n;
    internal static bool isTractorModInstalled;
    internal static string? modsPath;
    internal static Integrations.IAnimalsNeedWaterAPI? AnimalsNeedWaterAPI;

    public override void Entry(IModHelper helper)
    {

        Instance = this;
        i18n = Helper.Translation;
        Config = Helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;


    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        Configuration.ConfigSetup.SetupConfig(Helper, Instance);
        isTractorModInstalled = Helper.ModRegistry.IsLoaded("Pathoschild.TractorMod");
        modsPath = Path.Combine(AppContext.BaseDirectory, "Mods");
        ItemExtensionsAPI = Helper.ModRegistry.GetApi<Integrations.IItemExtensionsApi>("mistyspring.ItemExtensions");
        AnimalsNeedWaterAPI = null; //Helper.ModRegistry.GetApi<Integrations.IAnimalsNeedWaterAPI>("GZhynko.AnimalsNeedWater");
        

    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
            return;

        Farmer player = Game1.player;
        //Toggles the mod on and off
        if (Config.ToggleKey.JustPressed())
        {
            Config.Enabled = !Config.Enabled;
            if (Config.Enabled)
                Game1.addHUDMessage(new HUDMessage("AutomateToolSwap " + i18n.Get("mod.Enabled"), 2));
            else
                Game1.addHUDMessage(new HUDMessage("AutomateToolSwap " + i18n.Get("mod.Disabled"), 2));
        }


        // Logic to Add/Remove items from the blacklist
        if (Config.BlacklistKey.JustPressed())
        {
            Item? currentItem = player.CurrentItem;

            if (currentItem != null)
            {
                // Use QualifiedItemId to be precise
                string itemId = currentItem.QualifiedItemId;

                if (Config.ItemBlacklist.Contains(itemId))
                {
                    Config.ItemBlacklist.Remove(itemId);
                    Game1.addHUDMessage(new HUDMessage($"Removed {currentItem.DisplayName} from blacklist", 2));
                }
                else
                {
                    Config.ItemBlacklist.Add(itemId);
                    Game1.addHUDMessage(new HUDMessage($"Added {currentItem.DisplayName} to blacklist", 2));
                }

                Helper.WriteConfig(Config);
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("No item held to add/remove", 3));
            }

            return; 
        }
       

        // Swaps to the last used item
        if (Config.LastToolKey.JustPressed() && player.canMove && Config.Enabled)
            inventoryIndexMemory.GoToLastIndex(player);

        if (!SwapButtonPressed(e) || !Config.Enabled || !(player.canMove))
            return;

        if (IsHoldingBlacklistItem(Game1.player))
            return;

        if (Config.RequireClick)
            StartMod(player);
    }

    public void StartMod(Farmer player)
    {
        GameLocation currentLocation = player.currentLocation;
        ICursorPosition cursorPos = Helper.Input.GetCursorPosition();
        Vector2 frontOfPlayerTile = new(
            (int)(player.GetToolLocation().X / Game1.tileSize),
            (int)(player.GetToolLocation().Y / Game1.tileSize)
        );

        #region RangedTools Compatibility
        string folderPath;
        string configFilePath;
        string jsonString;
        int toolRange = 1;

        IModInfo? rangedToolsMod = Helper.ModRegistry.Get("vgperson.RangedTools");
        if (rangedToolsMod != null)
        {
            folderPath = Path.Combine(modsPath, "RangedTools");
            configFilePath = Path.Combine(folderPath, "config.json");
            jsonString = File.ReadAllText(configFilePath);
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("AxeRange", out JsonElement toolRangeElement))
            {
                toolRange = toolRangeElement.GetInt32();
            }
        }
        #endregion


        switch (Config.DetectionMethod)
        {
            case "Cursor": // Checked tile will be under the cursor if the tile is in range of tool, otherwise it will be the tile in fornt of the player
                if (rangedToolsMod != null)
                {
                    // Calculate Chebyshev distance
                    double distance = Math.Max(Math.Abs(player.Tile.X - cursorPos.Tile.X), Math.Abs(player.Tile.Y - cursorPos.Tile.Y));
                    if (toolRange == -1 || distance <= toolRange)
                    {
                        ItemSwapCoordinator.TrySwapAll(currentLocation, cursorPos.Tile, player);
                    }
                    else
                    {
                        ItemSwapCoordinator.TrySwapAll(currentLocation, cursorPos.GrabTile, player);
                    }
                }
                else
                {
                    ItemSwapCoordinator.TrySwapAll(currentLocation, cursorPos.GrabTile, player);
                }
                break;

            case "Player": // Checked tile will be the tile in front of the player
                ItemSwapCoordinator.TrySwapAll(currentLocation, frontOfPlayerTile, player);
                break;

            case "Cursor ONLY": // Checked tile will be under the cursor no matter what
                ItemSwapCoordinator.TrySwapAll(currentLocation, cursorPos.Tile, player);
                break;
        }

    }

    //Called when the game updates the tick (60 times per second)
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.activeClickableMenu != null || !Config.Enabled)
            return;

        //Alternative option for the entire mod, which does not require click
        if (!Config.RequireClick && Game1.player.canMove)
            StartMod(Game1.player);

        //Alternative option for "Weapon for Monsters that does not require click"
        if (Config.AlternativeWeaponOnMonsters && Config.WeaponOnMonsters)
        {
            InteractionRules.MonstersInteractionRules.AlternativeWeaponForMonsters();
        }

        //Tractor mod compatibility
        if (!isTractorModInstalled || Config.DisableTractorSwap || (!Config.Enabled && !Config.DisableTractorSwap))
            return;

        if (Game1.player.isRidingHorse() && Game1.player.mount.Name.ToLower().Contains("tractor"))
        {
            Farmer player = Game1.player;
            GameLocation currentLocation = Game1.currentLocation;
            ICursorPosition cursorPos = this.Helper.Input.GetCursorPosition();
            Vector2 cursorTile = cursorPos.GrabTile;
            Vector2 toolLocation = new((int)Game1.player.GetToolLocation().X / Game1.tileSize, (int)Game1.player.GetToolLocation().Y / Game1.tileSize);

            if (Config.DetectionMethod == "Cursor")
                ItemSwapCoordinator.TrySwapAll(currentLocation, cursorTile, player);

            else if (Config.DetectionMethod == "Player")
                ItemSwapCoordinator.TrySwapAll(currentLocation, toolLocation, player);

        }
    }

    /// Checks if the player is currently holding an item defined in the ItemBlacklist.
    private bool IsHoldingBlacklistItem(Farmer player)
    {
        if (player.CurrentItem == null)
            return false;

        // Check if the current item's Qualified ID exists in the config list
        return Config.ItemBlacklist.Contains(player.CurrentItem.QualifiedItemId);
    }

    //Checks if the swap button is pressed
    public static bool SwapButtonPressed(ButtonPressedEventArgs e)
    {
        if (Config.UseDifferentSwapKey)
        {
            return Config.SwapKey.JustPressed();

        }
        else
        {
            foreach (var button in Game1.options.useToolButton)
            {
                if (e.Button == button.ToSButton() || e.Button == SButton.ControllerX)
                    return true;

            }
            return false;
        }
    }

}




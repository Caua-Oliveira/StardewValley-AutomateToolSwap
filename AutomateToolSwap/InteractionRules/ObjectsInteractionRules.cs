using AutomateToolSwap;
using Core;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace InteractionRules;

/// <summary>
/// Implements the swap rules for monsters.
/// </summary>
public class ObjectsInteractionRules
{
    /// <summary>
    /// Checks object-related conditions and tries to swap to the correct tool/item appropriately.
    /// </summary>
    /// <param name="location">The current game location.</param>
    /// <param name="tile">The tile being checked.</param>
    /// <param name="player">The current player.</param>
    /// <returns>True if a swap was performed; otherwise, false.</returns>
    public static bool TrySwap(GameLocation location, Vector2 tile, Farmer player)
    {

        // Retrieve the object at the specified tile.
        var obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);
        if (obj == null)
            return false;

        bool currentItemIsNull = player.CurrentItem == null;
        string currentItemName = player.CurrentItem?.Name ?? "";
        int currentItemCategory = player.CurrentItem?.category.Value ?? 0;


        // Check if the player's current item is not a breaking tool. (So they can break objects without swapping to something else)
        bool currentItemCantBreak = player.CurrentItem is not (Pickaxe or Axe);
        // Check if the player's current item is not a bomb or staircase. (So they can use it in the mines without problem)
        bool currentItemIsNotForMine = currentItemIsNull ||
                                  !currentItemName.Contains("Bomb") && !currentItemName.Contains("Staircase");
        // If the ItemExtensions API is available, try to get the breaking tool for modded objects.
        if (ModEntry.ItemExtensionsAPI != null)
        {
            bool isClump = ModEntry.ItemExtensionsAPI.IsClump(obj.ItemId);
            bool foundTool = ModEntry.ItemExtensionsAPI.GetBreakingTool(obj.ItemId, isClump, out string tool);
            if (foundTool)
            {
                if (tool == "Pickaxe")
                {
                    if (ModEntry.Config.PickaxeForStoneAndOres && currentItemIsNotForMine)
                    {
                        InventoryHandler.SetTool(player, typeof(Pickaxe));
                    }
                }
                else if (tool == "Axe")
                {
                    if (ModEntry.Config.AxeForTwigs)
                    {
                        InventoryHandler.SetTool(player, typeof(Axe));
                    }
                }
                return true; // Indicate that a tool was found and potentially set.
            }
        }

        // Check object characteristics using its methods.
        if (obj.IsWeeds())
        {
            if (ModEntry.Config.AnyToolForWeeds && location is not MineShaft)
            {
                InventoryHandler.SetTool(player, typeof(Pickaxe), anyTool: true);
            }
            else if (ModEntry.Config.ScytheForWeeds)
            {
                InventoryHandler.SetTool(player, typeof(MeleeWeapon), "Scythe");
            }
            return true;
        }
        if (obj.IsBreakableStone())
        {
            if (ModEntry.Config.PickaxeForStoneAndOres && currentItemIsNotForMine)
            {
                InventoryHandler.SetTool(player, typeof(Pickaxe));
            }
            return true;
        }
        if (obj.IsTwig())
        {
            if (ModEntry.Config.AxeForTwigs)
            {
                InventoryHandler.SetTool(player, typeof(Axe));
            }
            return true;
        }
        if (obj.isForage())
        {
            if (ModEntry.Config.ScytheForForage)
            {
                InventoryHandler.SetTool(player, typeof(MeleeWeapon), "ScytheOnly");
            }
            return true;
        }
        if (obj is CrabPot crabPot && crabPot.bait.Value is null)
        {
            if (ModEntry.Config.BaitForCrabPot)
            {
                InventoryHandler.SetItem(player, "Bait", "Bait", aux: -21);
            }
            return true;
        }

        // Check object names to swap to specific items or tools.
        switch (obj.Name)
        {
            case "Furnace":
            case "Heavy Furnace":
                if (ModEntry.Config.OresForFurnace && currentItemCantBreak &&
                   (currentItemIsNull || !currentItemName.Contains("Ore")))
                {
                    InventoryHandler.SetItem(player, "Minerals", "Ore");
                }
                return true;
            case "Cheese Press":
                if (ModEntry.Config.MilkForCheesePress && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Animal Product", "Milk", aux: -6);
                }
                return true;
            case "Mayonnaise Machine":
                if (ModEntry.Config.EggsForMayoMachine && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Animal Product", "Egg", aux: -5);
                }
                return true;
            case "Seed Spot":
            case "Artifact Spot":
                if (ModEntry.Config.HoeForArtifactSpots)
                {
                    InventoryHandler.SetTool(player, typeof(Hoe));
                }
                return true;
            case "Garden Pot":
                if (ModEntry.Config.WateringCanForGardenPot &&
                    (currentItemIsNull || currentItemCantBreak && currentItemCategory != -74))
                {
                    InventoryHandler.SetTool(player, typeof(WateringCan));
                }
                return true;
            case "Barrel":
                if (ModEntry.Config.WeaponForMineBarrels)
                {
                    InventoryHandler.SetTool(player, typeof(MeleeWeapon), "Weapon");
                }
                return true;
            case "Supply Crate":
                if (ModEntry.Config.AnyToolForSupplyCrates)
                {
                    InventoryHandler.SetTool(player, typeof(Hoe), anyTool: true);
                }
                return true;
            case "Recycling Machine":
                if (ModEntry.Config.TrashForRecycling && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Trash", "Joja", aux: -20);
                }
                return true;
            case "Bone Mill":
                if (ModEntry.Config.BoneForBoneMill && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Minerals", "Bone Fragment");
                }
                return true;
            case "Loom":
                if (ModEntry.Config.WoolForLoom && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Animal Product", "Wool", aux: -18);
                }
                return true;
            case "Fish Smoker":
                if (ModEntry.Config.FishForSmoker && currentItemCategory != -4 && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Fish", aux: -4);
                }
                return true;
            case "Bait Maker":
                if (ModEntry.Config.FishForBaitMaker && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Fish", aux: -4);
                }
                return true;
            case "Crystalarium":
                if (ModEntry.Config.MineralsForCrystalarium &&
                    (currentItemIsNull || currentItemCantBreak && currentItemCategory != -2))
                {
                    InventoryHandler.SetItem(player, "Mineral", aux: -2);
                }
                return true;
            case "Seed Maker":
                if (ModEntry.Config.SwapForSeedMaker && currentItemCantBreak)
                {
                    InventoryHandler.SetItem(player, "Crops");
                }
                return true;
            case "Keg":
                if (currentItemCantBreak && ModEntry.Config.SwapForKegs != "None")
                {
                    InventoryHandler.SetItem(player, "Crops", crops: ModEntry.Config.SwapForKegs);
                }
                return true;
            case "Preserves Jar":
                if (currentItemCantBreak && ModEntry.Config.SwapForPreservesJar != "None")
                {
                    InventoryHandler.SetItem(player, "Crops", crops: ModEntry.Config.SwapForPreservesJar);
                }
                return true;
            case "Dehydrator":
                if (currentItemCantBreak && ModEntry.Config.SwapForDehydrator != "None")
                {
                    InventoryHandler.SetItem(player, "Dehydratable", crops: ModEntry.Config.SwapForDehydrator);
                }
                return true;
            case "Oil Maker":
                if (currentItemCantBreak && ModEntry.Config.SwapForOilMaker)
                {
                    InventoryHandler.SetItem(player, "Oil Maker");
                }
                return true;
            case "Geode Crusher":
                if (currentItemCantBreak && ModEntry.Config.GeodeForCrusher &&
                    (currentItemIsNull || !currentItemName.Contains("Geode")))
                {
                    InventoryHandler.SetItem(player, "", "Geode", aux: 0);
                }
                return true;
            case "Charcoal Kiln":
                if (currentItemCantBreak && ModEntry.Config.WoodForCharcoalKiln &&
                    (currentItemIsNull || !currentItemName.Contains("Wood")))
                {
                    InventoryHandler.SetItem(player, "Wood", "Wood", aux: -16);
                }
                return true;
            case "Cask":
                if (currentItemCantBreak && ModEntry.Config.GoodsForCask &&
                    (currentItemIsNull || !currentItemName.Contains("Wine") &&
                    !currentItemName.Contains("Cheese")))
                {
                    InventoryHandler.SetItem(player, "Cask");
                }
                return true;
            case "Wood Chipper":
                if (currentItemCantBreak && ModEntry.Config.HardwoodForChipper &&
                    (currentItemIsNull || !currentItemName.Contains("Hardwood")))
                {
                    InventoryHandler.SetItem(player, "Wood", "Hardwood", aux: -16);
                }
                return true;
            //AnimalNeedsWater MOD
            case "Water Bowl":
                if (currentItemCantBreak && ModEntry.Config.WateringCanForPetBowl)
                {
                    InventoryHandler.SetTool(player, typeof(WateringCan));
                }
                return true;
            default:
                return true;
        }
    }
}

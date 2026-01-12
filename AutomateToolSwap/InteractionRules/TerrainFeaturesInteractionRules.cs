using AutomateToolSwap;
using Core;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace InteractionRules;

/// <summary>
/// Implements the switch rule for terrain features (trees, bushes, grass, and tilled dirt).
/// </summary>
public class TerrainFeaturesInteractionRules
{
    /// <summary>
    /// Checks the terrain features at the given tile and swaps tools if necessary.
    /// </summary>
    /// <param name="location">The current game location.</param>
    /// <param name="tile">The tile being checked.</param>
    /// <param name="player">The current player.</param>
    /// <returns>True if a switch was performed; otherwise, false.</returns>
    public static bool TrySwap(GameLocation location, Vector2 tile, Farmer player)
    {
        bool currentItemIsNull = player.CurrentItem == null;
        string currentItemName = player.CurrentItem?.Name ?? "";
        int currentItemCategory = player.CurrentItem?.category.Value ?? 0;
        // Check for large terrain features (e.g. bushes that occupies multiple tiles)
        foreach (var terrainFeature in location.largeTerrainFeatures)
        {
            if (terrainFeature is Bush bush && ModEntry.Config.ScytheForBushes)
            {
                // Calculate bounding box of the bush and check if the clicked tile falls within it.
                Rectangle bushBox = bush.getBoundingBox();
                Vector2 tilePixel = new(tile.X * Game1.tileSize, tile.Y * Game1.tileSize);
                if (bushBox.Contains((int)tilePixel.X, (int)tilePixel.Y) && bush.inBloom())
                {
                    InventoryHandler.SetTool(player, typeof(MeleeWeapon), "Scythe");
                    return true;
                }
            }
        }

        // Check normal terrain features.
        if (!location.terrainFeatures.ContainsKey(tile))
            return false;

        var feature = location.terrainFeatures[tile];

        if (feature is Tree tree)
        {
            // If the tree has moss and the config enables scythe for moss, switch tool.
            if (tree.hasMoss.Value && tree.growthStage.Value >= Tree.stageForMossGrowth && ModEntry.Config.ScytheForMossOnTrees)
            {
                InventoryHandler.SetTool(player, typeof(MeleeWeapon), "Scythe");
                return true;
            }
            // If the player is holding a tapper or fertilizer tool, do not change.
            if (!ModEntry.Config.AxeForTrees || !currentItemIsNull &&
               (currentItemName == "Tapper" || currentItemName == "Heavy Tapper" ||currentItemName == "Tree Fertilizer"))
                return false;
            // If the tree is grown (or config does not ignore growing trees), switch to axe.
            if (!(tree.growthStage.Value < Tree.treeStage && ModEntry.Config.IgnoreGrowingTrees))
            {
                InventoryHandler.SetTool(player, typeof(Axe));
                return true;
            }
            return false;
        }

        //Ignore swap to Scythe if player is holding Animal Tool so they dont accidentally cut the grass
        if (feature is Grass && !(player.CurrentTool is MilkPail || player.CurrentTool is Shears) && ModEntry.Config.ScytheForGrass)
        {
            InventoryHandler.SetTool(player, typeof(MeleeWeapon), "ScytheOnly");
            return true;
        }

        if (feature is HoeDirt dirt)
        {
            bool dirtHasCrop = dirt.crop != null;
            if (!dirtHasCrop && ModEntry.Config.SeedForTilledDirt)
            {
                bool has_seeds = false;

                if (!(ModEntry.Config.PickaxeOverWateringCan && player.CurrentTool is Pickaxe))
                {
                    if (currentItemIsNull || currentItemCategory != -74 ||
                        player.CurrentItem.HasContextTag("tree_seed_item"))
                    {
                        foreach (var x in player.Items)
                        {
                            if (x != null && x.HasContextTag("category_seeds") && !x.HasContextTag("tree_seed_item"))
                            {
                                has_seeds = true;
                            }
                        }
                        InventoryHandler.SetItem(player, "Seed");
                        if (has_seeds)
                        {
                            return true;
                        }
                        else
                        {
                            if(ModEntry.Config.PickaxeForTilledSoil && !(player.CurrentTool is Pickaxe) && !(player.isRidingHorse()))
                            {
                                InventoryHandler.SetTool(player, typeof(Pickaxe));
                                return true;
                            }

                        }

                    }

                }           
            }
            if (!dirtHasCrop && ModEntry.Config.PickaxeForTilledSoil && (currentItemIsNull || !player.CurrentItem.HasContextTag("category_seeds")) && !(player.isRidingHorse()))
            {
                InventoryHandler.SetTool(player, typeof(Pickaxe));
                return true;
            }
            if (dirtHasCrop && (dirt.readyForHarvest() || dirt.crop.dead.Value) && ModEntry.Config.ScytheForCrops)
            {
                InventoryHandler.SetTool(player, typeof(MeleeWeapon), "ScytheOnly");
                return true;
            }
            if (dirtHasCrop && dirt.crop.whichForageCrop.Value == "2" && ModEntry.Config.HoeForGingerCrop)
            {
                InventoryHandler.SetTool(player, typeof(Hoe));
                return true;
            }
            if (dirtHasCrop && !dirt.isWatered() && !dirt.readyForHarvest() && ModEntry.Config.WateringCanForUnwateredCrop &&
                !(player.isRidingHorse() && player.mount.Name.ToLower().Contains("tractor") && player.CurrentTool is Hoe))
            {
                if (!(ModEntry.Config.PickaxeOverWateringCan && player.CurrentTool is Pickaxe))
                {
                    InventoryHandler.SetTool(player, typeof(WateringCan));
                }
                return true;
            }
            if (dirtHasCrop && !dirt.HasFertilizer() && dirt.CanApplyFertilizer("(O)369") && ModEntry.Config.FertilizerForCrops)
            {
                if (!ModEntry.Config.PickaxeOverWateringCan && player.CurrentTool is not Pickaxe && currentItemCategory != -19)
                {
                    InventoryHandler.SetItem(player, "Fertilizer", "Tree", aux: -19);
                }
                return true;
            }

            return true;
        }
        return false;
    }
}

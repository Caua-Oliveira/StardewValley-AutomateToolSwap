using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Dimensions;
using StardewModdingAPI;

namespace OnlyWhenItCounts;

public class DidWork
{
    private static IMonitor Monitor;

    public static void SetMonitor(IMonitor monitor)
    {
        Monitor = monitor;
    }

    public static bool Pickaxe(Vector2 tile)
    {
        if (Detects.Monster(tile))
        {
            Monitor.Log("Detected Monster with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.Boulders(tile))
        {
            Monitor.Log("Detected Boulders with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.BushFruits(tile))
        {
            Monitor.Log("Detected BushFruits with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.Grass(tile))
        {
            Monitor.Log("Detected Grass with Pickaxe", LogLevel.Trace);
            return false;
        }

        if (Detects.Crops(tile))
        {
            Monitor.Log("Detected Crops with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.MossOrGrowingTree(tile))
        {
            Monitor.Log("Detected Moss Or Growing Tree with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.Weeds(tile))
        {
            Monitor.Log("Detected Weed with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.Objects(tile, "Pickaxe"))
        {
            Monitor.Log("Detected Object with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.Trees(tile))
        {
            Monitor.Log("Detected Tree with Pickaxe", LogLevel.Trace);
            return false;
        }

        if (Detects.ModdedObjectRequiresTool(tile, "Pickaxe"))
        {
            Monitor.Log("Detected Modded Object with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.ModdedResourceClumpRequiresTool(tile, "Pickaxe"))
        {
            Monitor.Log("Detected Modded Resource Clump with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Detects.HoeDirt(tile))
        {
            Monitor.Log("Detected HoeDirt with Pickaxe", LogLevel.Trace);
            return true;
        }

        if (Game1.currentLocation.isPath(tile))
        {
            Monitor.Log("Detected Path with Pickaxe", LogLevel.Trace);
            return true;
        }

        return false;
    }

    public static bool Axe(Vector2 tile)
    {
        if (Detects.Monster(tile))
        {
            Monitor.Log("Detected Monster with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.StumpOrLog(tile))
        {
            Monitor.Log("Detected StumpOrLog with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.BushFruits(tile))
        {
            Monitor.Log("Detected BushFruits with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.GiantCrops(tile))
        {
            Monitor.Log("Detected GiantCrops with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.Grass(tile))
        {
            Monitor.Log("Detected Grass with Axe", LogLevel.Trace);
            return false;
        }

        if (Detects.Crops(tile))
        {
            Monitor.Log("Detected Crops with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.MossOrGrowingTree(tile))
        {
            Monitor.Log("Detected Moss Or Growing Tree with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.Trees(tile))
        {
            Monitor.Log("Detected Tree with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.Weeds(tile))
        {
            Monitor.Log("Detected Weed with Axe", LogLevel.Trace);
            return true;
        }
        if (Detects.Objects(tile, "Axe"))
        {
            Monitor.Log("Detected Object with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.ModdedObjectRequiresTool(tile, "Axe"))
        {
            Monitor.Log("Detected Modded Object with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.ModdedResourceClumpRequiresTool(tile, "Axe"))
        {
            Monitor.Log("Detected Modded Resource Clump with Axe", LogLevel.Trace);
            return true;
        }

        if (Detects.HoeDirt(tile))
        {
            Monitor.Log("Detected HoeDirt with Axe", LogLevel.Trace);
            return false;
        }

        if (Game1.currentLocation.isPath(tile))
        {
            Monitor.Log("Detected Path with Axe", LogLevel.Trace);
            return true;
        }

        return false;
    }

    public static bool Hoe(Vector2 tile)
    {
        bool isDiggable = Game1.currentLocation.doesTileHaveProperty(
            (int)tile.X,
            (int)tile.Y,
            "Diggable",
            "Back") != null;

        if (Detects.Monster(tile))
        {
            Monitor.Log("Detected Monster with Hoe", LogLevel.Trace);
            return true;
        }

        if (Detects.BushFruits(tile))
        {
            Monitor.Log("Detected BushFruits with Hoe", LogLevel.Trace);
            return true;
        }

        if (Detects.Grass(tile))
        {
            Monitor.Log("Detected Grass with Hoe", LogLevel.Trace);
            return false;
        }
        
        if (Detects.GiantCrops(tile))
        {
            Monitor.Log("Detected GiantCrop with Hoe", LogLevel.Trace);
            return false;
        }
        if (Detects.MossOrGrowingTree(tile))
        {
            Monitor.Log("Detected Moss Or Growing Tree with Hoe", LogLevel.Trace);
            return true;
        }

        if (Detects.Trees(tile))
        {
            Monitor.Log("Detected Tree with Hoe", LogLevel.Trace);
            return false;
        }

        if (Game1.currentLocation.isPath(tile))
        {
            Monitor.Log("Detected Path with Hoe", LogLevel.Trace);
            return false;
        }
        if (Detects.Weeds(tile))
        {
            Monitor.Log("Detected Weed with Hoe", LogLevel.Trace);
            return true;
        }
        if(Detects.HoeSpots(tile))
        {
            Monitor.Log("Detected HoeSpot with Hoe", LogLevel.Trace);
            return true;
        }
        if (Detects.BigCraftables(tile))
        {
            Monitor.Log("Detected BigCraftables with Hoe", LogLevel.Trace);
            return true;
        }

        if(Detects.Objects(tile, ""))
        {
            Monitor.Log("Detected objects with Hoe", LogLevel.Trace);
            return false;
        }

        if (isDiggable)
        {
            Monitor.Log("Detected Diggable Tile with Hoe", LogLevel.Trace);
            return true;
        }

        if (Detects.HoeDirt(tile))
        {
            Monitor.Log("Detected HoeDirt with Hoe", LogLevel.Trace);
            return false;
        }

        Monitor.Log("Detected nothing with Hoe", LogLevel.Trace);
        return false;
    }

    public static bool WateringCan(Vector2 tile)
    {
        if (Detects.WateredSoil(tile))
        {
            Monitor.Log("Detected Wated Soil with WateringCan", LogLevel.Trace);
            return false;
        }

        if (Detects.HoeDirt(tile))
        {
            Monitor.Log("Detected HoeDirt with WateringCan", LogLevel.Trace);
            return true;
        }

        if (Detects.WaterPlaces(tile))
        {
            Monitor.Log("Detected WaterPlace with WateringCan", LogLevel.Trace);
            return true;
        }

        return false;
    }
}

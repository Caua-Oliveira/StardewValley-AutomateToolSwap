using AutomateToolSwap;
using StardewValley;
using StardewValley.Tools;

namespace Core;

internal class InventoryHandler
{
    /// <summary>
    /// Finds the index of the item necessary for the action and tries to swap to it.
    /// </summary>
    /// <param name="player">The player object (Farmer).</param>
    /// <param name="category">The item's category, as each one has different rules for swapping.</param>
    /// <param name="item">The item that it needs to swap to OR it needs to ignore.</param>
    /// <param name="crops">What type of crops we can swap to for Kegs, Jars and Dehydrator.</param>
    /// <param name="aux">The item.category.value (as we can't get the category name untranslated).</param>
    public static void SetItem(Farmer player, string category, string item = "", string crops = "Both", int aux = 0)
    {
        ModEntry.inventoryIndexMemory.canSwitch = ModEntry.Config.AutoReturnToLastTool;
        var items = player.Items;

        // Each category has different rules for swapping, such as requiring a specific item or excluding one.
        switch (category)
        {
            case "Trash":
            case "Fertilizer":
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem && invItem.Category == aux && !invItem.Name.Contains(item))
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                return;

            case "Minerals":
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem &&
                        invItem.Category == -15 &&
                        invItem.Name.Contains(item) &&
                        invItem.Stack >= 5)
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                return;

            case "Cask":
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem &&
                        invItem.Category == -26 &&
                        (invItem.Name.Contains("Cheese") || invItem.Name.Contains("Wine")))
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                return;

            case "Seed":
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem &&
                        invItem.Category == -74 &&
                        !invItem.HasContextTag("tree_seed_item"))
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                return;

            case "Crops":
                {
                    bool canFruit = crops == "Both" || crops == "Fruit";
                    bool canVegetable = crops == "Both" || crops == "Vegetable";

                    bool isFruit(Item it) => it is { } item && item.Category == -79;
                    bool isVegetable(Item it) => it is { } item && item.Category == -75;

                    for (int i = 0; i < player.maxItems.Value; i++)
                    {
                        if (items[i] is { } invItem &&
                            (canFruit && isFruit(invItem) || canVegetable && isVegetable(invItem)))
                        {
                            // If current item is already a fruit or vegetable, do nothing.
                            if (isFruit(player.CurrentItem) || isVegetable(player.CurrentItem))
                                return;

                            if (player.CurrentToolIndex != i)
                                _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                            return;
                        }
                    }
                }
                return;

            case "Dehydratable":
                {
                    bool canFruit = crops == "Both" || crops == "Fruit";
                    bool canMushroom = crops == "Both" || crops == "Mushroom";

                    bool isFruit(Item it) => it is { } item && item.Category == -79;
                    bool isMushroom(Item it) => it is { } item && item.Category == -81 && item.Name != "Red Mushroom";

                    for (int i = 0; i < player.maxItems.Value; i++)
                    {
                        if (items[i] is { } invItem &&
                            (canFruit && isFruit(invItem) || canMushroom && isMushroom(invItem)))
                        {
                            if (isFruit(player.CurrentItem) || isMushroom(player.CurrentItem))
                                return;

                            if (player.CurrentToolIndex != i)
                                _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                            return;
                        }
                    }
                }
                return;

            case "Oil Maker":
                {
                    bool isOilMakerIngredient(Item it) => it is not null &&
                                                          ((it.Name == "Truffle" && it.Category == -17) ||
                                                           (it.Name == "Sunflower" && it.Category == -80) ||
                                                           (it.Name == "Sunflower Seeds" && it.Category == -74) ||
                                                           (it.Name == "Corn" && it.Category == -75));

                    // If the player is already holding a valid item, do nothing.
                    if (isOilMakerIngredient(player.CurrentItem))
                        return;

                    for (int i = 0; i < player.maxItems.Value; i++)
                    {
                        if (isOilMakerIngredient(items[i]))
                        {
                            if (player.CurrentToolIndex != i)
                                _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                            return;
                        }
                    }
                    return;
                }

            default:
                // Handles any other item
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem &&
                        invItem.Category == aux &&
                        invItem.Name.Contains(item))
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                return;
        }
    }

    /// <summary>
    /// Finds the index of the tool necessary for the action and tries to swap to it.
    /// </summary>
    /// <param name="player">The player object (Farmer).</param>
    /// <param name="toolType">The tool type (any from StardewValley.Tools).</param>
    /// <param name="aux">
    /// When swapping to a MeleeWeapon, specify if it needs to be a weapon, a scythe or weapon/scythe combination,
    /// or only a scythe ("Weapon", "Scythe", "ScytheOnly").
    /// </param>
    /// <param name="anyTool">If true, allow swapping to any tool (such as Axe, Pickaxe, or Hoe).</param>
    public static void SetTool(Farmer player, Type toolType, string aux = "", bool anyTool = false)
    {
        ModEntry.inventoryIndexMemory.canSwitch = ModEntry.Config.AutoReturnToLastTool;
        var items = player.Items;

        // If the current tool already matches and RequireClick is disabled, do nothing.
        if (items[ModEntry.inventoryIndexMemory.currentIndex] is { } currentItem &&
            currentItem.GetType() == toolType &&
            !ModEntry.Config.RequireClick)
            return;

        // MeleeWeapon is a special case, as it can be a damage weapon or a scythe, also the player might need any of them or only one.
        switch (toolType)
        {
            case Type t when t == typeof(MeleeWeapon):
                switch (aux)
                {
                    case "Scythe":
                    case "ScytheOnly":
                        for (int i = 0; i < player.maxItems.Value; i++)
                        {
                            if (items[i] is { } invItem &&
                                invItem.GetType() == toolType &&
                                invItem.Name.Contains("Scythe"))
                            {
                                if (player.CurrentToolIndex != i)
                                    _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                                return;
                            }
                        }
                        // If we only want a scythe and none is found, exit.
                        if (aux == "ScytheOnly")
                            return;
                        // Otherwise, fall through to check for non-scythe melee weapons.
                        goto default;

                    default:
                        for (int i = 0; i < player.maxItems.Value; i++)
                        {
                            if (items[i] is { } invItem &&
                                invItem.GetType() == toolType &&
                                !invItem.Name.Contains("Scythe"))
                            {
                                if (player.CurrentToolIndex != i)
                                    _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                                return;
                            }
                        }
                        return;
                }

            default:
                // Handles any other tool
                for (int i = 0; i < player.maxItems.Value; i++)
                {
                    if (items[i] is { } invItem &&
                        (invItem.GetType() == toolType || anyTool && invItem is Axe or Pickaxe or Hoe))
                    {
                        if (player.CurrentToolIndex != i)
                            _ = ModEntry.inventoryIndexMemory.SwitchIndex(i, player);
                        return;
                    }
                }
                break;
        }
    }
}

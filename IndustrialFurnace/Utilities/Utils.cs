using StardewModdingAPI;
using StardewValley;

namespace FurnaceSmokeStack.Utilities;

public static class Utils
{
    /// <summary>Displays a HUD message of defined type with a possible sound effect</summary>
    /// <param name="s">Displayed message</param>
    /// <param name="type">Message type</param>
    /// <param name="sound">Sound effect</param>
    public static void DisplayHudMessage(string s, int type, string? sound = null)
    {
        Game1.addHUDMessage(new HUDMessage(s, type));

        if (sound is not null)
        {
            Game1.playSound(sound);
        }
    }

    public static void RemoveItemFromPlayerInventory(string qualifiedItemId, int quantityToRemove)
    {
         for (int i = 0; i < Game1.player.Items.Count; i++)
        {
            Item item = Game1.player.Items[i];
            if (item != null && item.QualifiedItemId == qualifiedItemId)
            {
                if (item.Stack <= quantityToRemove)
                {
                    quantityToRemove -= item.Stack;
                    Game1.player.Items[i] = null; // Clear the slot
                }
                else
                {
                    item.Stack -= quantityToRemove;
                    quantityToRemove = 0;
                }
            }
            if (quantityToRemove <= 0) break;
        }
    }

    /// <summary>
    /// Adds a specific amount of an item to the player's inventory.
    /// </summary>
    /// <param name="qualifiedId">The Qualified Item ID, e.g., "(O)382" for Coal.</param>
    /// <param name="amount">The number of items to add.</param>
    public static void AddItemToPlayer(string qualifiedId, int amount)
    {
        // 1. Create the item instance using the 1.6 ItemRegistry
        // This automatically handles stacking and correct item types
        Item newItem = ItemRegistry.Create(qualifiedId, amount);

        if (newItem != null)
        {
            // 2. Add to inventory. This method handles:
            // - Merging into existing stacks
            // - Finding an empty slot
            // - Dropping the item on the ground if the inventory is full
            Game1.player.addItemToInventoryBool(newItem);
        }
    }

    public static int GetHeldCountOfItem(string qualifiedId)
    {
        if (!Context.IsWorldReady) return 0;

        return Game1.player.Items
            .Where(item => item?.QualifiedItemId == qualifiedId)
            .Sum(item => item.Stack);
    }
}


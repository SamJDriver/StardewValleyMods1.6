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

    /// <summary>
    /// Read data from a JSON file in the mod's folder.
    /// </summary>
    /// <typeparam name="T">The model type. This should be a plain class that has public properties for the data you want. The properties can be complex types.</typeparam>
    /// <param name="assetPath">The file path relative to the mod folder.</param>
    /// <param name="dataHelper">The api for reading the mod data.</param>
    /// <param name="monitor">The monitor to log into.</param>
    /// <returns>Returns the deserialized model, or the default instance of the object.</returns>
    public static T LoadAssetOrDefault<T>(string assetPath, IDataHelper dataHelper, IMonitor monitor) where T : class, new()
    {
        T? asset = LoadAsset<T>(assetPath, dataHelper, monitor);

        if (asset is null)
        {
            monitor.Log($"Loading {assetPath} failed. The mod may not work correctly.", LogLevel.Error);
            asset = new T();
        }

        return asset;
    }

    public static T? LoadAsset<T>(string assetPath, IDataHelper dataHelper, IMonitor monitor) where T : class, new()
    {
        T? asset = null;

        try
        {
            asset = dataHelper.ReadJsonFile<T>(assetPath);
        }
        catch (Exception ex)
        {
            monitor.Log(ex.ToString(), LogLevel.Error);
        }

        return asset;
    }
}


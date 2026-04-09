using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;

namespace IndustrialFurnace;

public class ModEntry : Mod
{
    private ModConfig config = null!;
    private ITranslationHelper i18n = null!;


    public override void Entry(IModHelper helper)
    {
        i18n = helper.Translation;
        config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        Farm farm = Game1.getFarm();
        string buildingId = "Vechio.MEGAFurnace_Furnace"; // The ID from Data/Buildings
        Vector2 tileLocation = new Vector2(63, 19); // X, Y coordinates on the farm

        // 1. Create the building instance
        Building newBuilding = Building.CreateInstanceFromId(buildingId, tileLocation);

        if (newBuilding != null)
        {
            // 2. Add it to the farm's building list
            farm.buildings.Add(newBuilding);

            newBuilding.FinishConstruction();

            // 3. Optional: Instantly finish construction if you don't want a "building site"
            this.Monitor.Log($"Successfully created {buildingId} at {tileLocation}.", LogLevel.Debug);
        }

        Item copperOre = ItemRegistry.Create("(O)378", 5); // Creates a stack of 5 Copper Ore
        Game1.player.addItemByMenuIfNecessary(copperOre);

        this.logAllbuildings();
    }

    private void logAllbuildings()
    {
        // 1. Load the dictionary of all building definitions
        Dictionary<string, BuildingData> buildingDefinitions = Game1.content.Load<Dictionary<string, BuildingData>>("Data\\Buildings");

        // 2. Iterate and log the details
        this.Monitor.Log($"Found {buildingDefinitions.Count} building definitions in Data/Buildings:", StardewModdingAPI.LogLevel.Info);

        foreach (var entry in buildingDefinitions)
        {
            string id = entry.Key;
            BuildingData data = entry.Value;

            // Log the ID, Display Name, and associated Texture path
            this.Monitor.Log($" - ID: {id} | Name: {data.Name} | Texture: {data.Texture}", StardewModdingAPI.LogLevel.Info);

            // Optional: Log if it has any production rules (ItemConversions)
            if (data.ItemConversions?.Count > 0)
            {
                this.Monitor.Log($"   --> Has {data.ItemConversions.Count} production rules.", StardewModdingAPI.LogLevel.Debug);
            }
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        // Only check occasionally for performance (e.g., every 60 ticks/1 second)
        if (!Context.IsWorldReady || !e.IsOneSecond) return;

        foreach (Building building in Game1.getFarm().buildings)
        {
            if (building.buildingType.Value.ToLower().Contains("furnace") && building.hasLoaded)
            {
                var buildingData = building.GetData();
                //var g = building.buildingChests.Select(b => b.addWorkingAnimation);


                if (building.buildingChests.Where(chest => chest.BaseName.ToLower().Contains("input")).Any(chest => chest.Items.Any()))
                {
                    building.texture = new Lazy<Texture2D>(delegate
                    {

                        string text = "Buildings\\Earth Obelisk";
                        Texture2D texture2D;
                        try
                        {
                            texture2D = Game1.content.Load<Texture2D>(text);
                        }
                        catch
                        {
                            return Game1.content.Load<Texture2D>("Buildings\\Error");
                        }
                        return texture2D;
                    });
                }
                else
                {
                    building.resetTexture();
                }
            }

        }
    }
}



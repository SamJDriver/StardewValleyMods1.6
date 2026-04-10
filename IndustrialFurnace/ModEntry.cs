using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady) return;

        // Check roughly every 20 ticks (1/3 second) for a natural smoke flow
        if (e.IsMultipleOf(20))
        {
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building.buildingType.Value.ToLower().Contains("furnace") && building.hasLoaded)
                {
                    // Logic to check if chest has items
                    bool isWorking = building.buildingChests
                        .Any(chest => chest.BaseName.ToLower().Contains("input") && chest.Items.Any());

                    if (isWorking)
                    {

                        TemporaryAnimatedSprite smoke = this.CreateSmokeSprite(building.tileX.Value, building.tileY.Value);

                        // 3. Add it to the map
                        Game1.getFarm().TemporarySprites.Add(smoke);
                    }
                }
            }
        }
    }

    private TemporaryAnimatedSprite CreateSmokeSprite(int x, int y)
    {
        TemporaryAnimatedSprite sprite;

        string textureName;
        Rectangle rectangle;

        textureName = Path.Combine("LooseSprites", "Cursors");
        rectangle = new Rectangle(372, 1956, 10, 10);


        sprite = new TemporaryAnimatedSprite(textureName,
            rectangle,
            new Vector2(x * 64 + 68, y * 64 + -64),
            false,
            1f / 500f,
            Color.Gray)
        {
            alpha = 0.75f,
            motion = new Vector2(0.0f, -0.5f),
            acceleration = new Vector2(1f / 500f, 0.0f),
            interval = 99999f,
            layerDepth = 1f,
            scale = 2,
            scaleChange = 0.02f,
            rotationChange = (float)(Game1.random.Next(-5, 6) * 3.14159274101257 / 256.0)
        };

        return sprite;
    }
}



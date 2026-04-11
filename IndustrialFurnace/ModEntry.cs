using Force.DeepCloner;
using FurnaceSmokeStack.Data;
using FurnaceSmokeStack.Utilities;
using IndustrialFurnace;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FurnaceSmokeStack;

public class ModEntry : Mod
{
    private ModConfig config = null!;
    private ITranslationHelper i18n = null!;


    private readonly PerScreen<List<Logic.IndustrialFurnace>> _onScreenFurnaces 
        = new PerScreen<List<Logic.IndustrialFurnace>>(() => new List<Logic.IndustrialFurnace>());

    private readonly PerScreen<int> _onScreenFurnacesBuilt = new PerScreen<int>(() => 0);      // Used to identify furnaces, placed in maxOccupants field.

    private SmeltingRules smeltingRules;

    public override void Entry(IModHelper helper)
    {
        i18n = helper.Translation;
        config = helper.ReadConfig<ModConfig>();

        this.smeltingRules = helper.Data.ReadJsonFile<SmeltingRules>("assets/SmeltingRules.json");


        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
        helper.Events.Player.InventoryChanged += this.OnPlayerInventoryChanged;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
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
                        .Any(chest => chest.BaseName.ToLower().Contains("input") && chest.Items.Any(i => i != null));

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

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is null)
        {

            if (e.OldMenu is ItemGrabMenu oldGrabMenu && oldGrabMenu.context is Building closedBuilding && closedBuilding.BuildingIsIndustrialFurnaceFlag())
            {
                Logic.IndustrialFurnace openFurnace = _onScreenFurnaces.Value.First(f => f.IsInputChestOpenFlag);
                openFurnace.IsInputChestOpenFlag = false;
            }

            return;
        }

        if (e.NewMenu is ItemGrabMenu newGrabMenu 
            && newGrabMenu.context is Building openedBuilding 
            && openedBuilding.BuildingIsIndustrialFurnaceFlag()) {

            var furnaceInRange = _onScreenFurnaces.Value.First(f => f.BuildingIsInRangeOfPlayer());
            furnaceInRange.IsInputChestOpenFlag = true;

        }
    }




    private void OnPlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        
        if ((!Game1.player.currentLocation.IsBuildableLocation())
            || _onScreenFurnaces.Value.Count() <= 0
            || _onScreenFurnacesBuilt.Value <= 0
            || (!_onScreenFurnaces.Value.Any(f => f.IsInputChestOpenFlag)))
        {
            return;
        }
        // Define the Ore IDs we care about
        //HashSet<string> oreIds = new HashSet<string> { "(O)378", "(O)380", "(O)384", "(O)386" };
        int totalOresPlaced = 0;
        string itemQualifiedId = "";

        // 1. Handle full stacks moved (Slot cleared)
        foreach (Item item in e.Removed)
        {
            if (smeltingRules.OreCoalCosts.Any(o => o.QualifiedItemId == item.QualifiedItemId))
            {
                totalOresPlaced += item.Stack;
                itemQualifiedId = item.QualifiedItemId;
            }
        }

        // 2. Handle partial stacks moved (Stack reduced)
        foreach (ItemStackSizeChange change in e.QuantityChanged)
        {
            if (smeltingRules.OreCoalCosts.Any(o => o.QualifiedItemId == change.Item.QualifiedItemId) && change.NewSize < change.OldSize)
            {
                totalOresPlaced += (change.OldSize - change.NewSize);
                itemQualifiedId = change.Item.QualifiedItemId;
            }
        }

        if (totalOresPlaced > 0)
        {
            this.Monitor.Log($"Detected {totalOresPlaced} ores moved to the chest.", LogLevel.Info);
            this.Monitor.Log($"Removed item {itemQualifiedId}.", LogLevel.Info);


            int amountOfCoalToRemove = (smeltingRules.OreCoalCosts.First(o => o.QualifiedItemId == itemQualifiedId).CoalRequiredToSmelt) * (totalOresPlaced);
            Utils.RemoveItemFromPlayerInventory("(O)382", amountOfCoalToRemove);
        }

    }


    private void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
    {
        // Add added furnaces to the controller list
        foreach (Building building in e.Added)
        {

            if (!building.BuildingIsIndustrialFurnaceFlag())
            {
                return;
            }
            // Add the controller that takes care of the functionality of the furnace
            Logic.IndustrialFurnace? furnace = new Logic.IndustrialFurnace(); 
            building.DeepCloneTo(furnace);

            if (furnace == null)
            {
                return;
            }
            furnace!.IndustrialFurnaceId = _onScreenFurnacesBuilt.Value;

            _onScreenFurnaces.Value.Add(furnace);
            _onScreenFurnacesBuilt.Value++;

        }

        // Remove destroyed furnaces from the controller list
        //foreach (Building building in e.Removed)
        //{
        //    if (building.BuildingIsIndustrialFurnaceFlag())
        //    {


        //            _onScreenFurnaces.Value.RemoveWhere(f => f.Id == )

        //    }
        //}
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



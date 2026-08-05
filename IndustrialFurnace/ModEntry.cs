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
    private SmeltingRules? _smeltingRules;


    private bool _insufficientCoalFlag = false;
    //private string _movedItemQualifiedId = "";
    //private int _quantityOfOreMoved = 0;
    //pivate int _

    public override void Entry(IModHelper helper)
    {
        i18n = helper.Translation;
        config = helper.ReadConfig<ModConfig>();

        this._smeltingRules = helper.Data.ReadJsonFile<SmeltingRules>("assets/SmeltingRules.json");


        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Input.ButtonPressed += this.OnMouseButtonPressed;


    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.populateOnScreenFurnaces();

        //Farm farm = Game1.getFarm();
        //string buildingId = "Vechio.MEGAFurnace_Furnace"; // The ID from Data/Buildings
        //Vector2 tileLocation = new Vector2(63, 19); // X, Y coordinates on the farm

        //// 1. Create the building instance
        //Building newBuilding = Building.CreateInstanceFromId(buildingId, tileLocation);

        //if (newBuilding != null)
        //{
        //    // 2. Add it to the farm's building list
        //    //farm.buildings.Add(newBuilding);

        //    //newBuilding.FinishConstruction();

        //    // 3. Optional: Instantly finish construction if you don't want a "building site"
        //    this.Monitor.Log($"Successfully created {buildingId} at {tileLocation}.", LogLevel.Debug);
        //}
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        // Check roughly every 20 ticks (1/3 second) for a natural smoke flow
        if (e.IsMultipleOf(20))
        {

            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building.BuildingIsIndustrialFurnaceFlag() && building.hasLoaded)
                {
                    // Logic to check if chest has items
                    bool isWorking = building.buildingChests
                        .Any(chest => chest.BaseName.ToLower().Contains("input") && chest.Items.Any(i => i != null));

                    if (isWorking)
                    {
                        TemporaryAnimatedSprite smoke = this.createSmokeSprite(building.tileX.Value, building.tileY.Value);
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


    /// <summary>
    /// Check to see if the player places an item in a furnace chest. If they did, remove the relevant amount of coal from 
    /// the player's inventory
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //private void OnPlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
    //{
    //    if ((!Game1.player.currentLocation.IsBuildableLocation())
    //        || _onScreenFurnaces.Value.Count() <= 0
    //        || (!_onScreenFurnaces.Value.Any(f => f.IsInputChestOpenFlag)))
    //    {
    //        return;
    //    }

    //    int totalOresPlaced = 0;
    //    string movedItemQualifiedId = "";

    //    // 1. Handle full stacks moved (Slot cleared)
    //    foreach (Item item in e.Removed)
    //    {
    //        if (_smeltingRules.OreCoalCosts.Any(o => o.QualifiedItemId == item.QualifiedItemId))
    //        {
    //            totalOresPlaced += item.Stack;
    //            movedItemQualifiedId = item.QualifiedItemId;
    //        }
    //    }

    //    // 2. Handle partial stacks moved (Stack reduced)
    //    foreach (ItemStackSizeChange change in e.QuantityChanged)
    //    {
    //        if (_smeltingRules.OreCoalCosts.Any(o => o.QualifiedItemId == change.Item.QualifiedItemId) && change.NewSize < change.OldSize)
    //        {
    //            totalOresPlaced += (change.OldSize - change.NewSize);
    //            movedItemQualifiedId = change.Item.QualifiedItemId;
    //        }
    //    }

    //    if (totalOresPlaced > 0)
    //    {
    //        this._movedItemQualifiedId = movedItemQualifiedId;
    //        this._quantityOfOreMoved = totalOresPlaced;

    //        int amountOfCoalToRemove = (_smeltingRules.OreCoalCosts.First(o => o.QualifiedItemId == movedItemQualifiedId).CoalRequiredToSmelt) * (totalOresPlaced);
    //        int playerHeldAmountOfCoal = Utils.GetHeldCountOfItem("(O)382");

    //        if (amountOfCoalToRemove <= playerHeldAmountOfCoal)
    //        {
    //            Utils.RemoveItemFromPlayerInventory("(O)382", amountOfCoalToRemove);
    //        }
    //        else
    //        {
    //            Game1.addHUDMessage(new HUDMessage("Insufficient amount of coal", HUDMessage.error_type));
    //            //Utils.AddItemToPlayer(movedItemQualifiedId, totalOresPlaced);
    //            this._insufficientCoalFlag = true;


    //            foreach(var item in e.Removed)
    //            {
    //                item.Stack = 0;
    //            }

    //            foreach (var item in e.Added)
    //            {

    //                item.Stack = 0;
    //            }

    //        }
    //    }
    //}


    /// <summary>
    /// Check if the player clicked on an item in their inventory or chest. Then run the coal calculations
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!_onScreenFurnaces.Value.Any(f => f.IsInputChestOpenFlag)
            || (e.Button != SButton.MouseLeft && e.Button != SButton.MouseRight)
            || !Context.IsWorldReady
            || Game1.activeClickableMenu == null
            || Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        // Get current mouse positionz  
        int x = (int)Game1.getMousePosition().X;
        int y = (int)Game1.getMousePosition().Y;

        var grabMenu = (Game1.activeClickableMenu as ItemGrabMenu)!;


        // Check player's bottom inventory grid
        ClickableComponent? playerSlot = grabMenu.inventory.inventory.FirstOrDefault(c => c.containsPoint(x, y));

        if (playerSlot == null)
        {
            return;
        }
        int index = int.Parse(playerSlot.name);
        Item? item = Game1.player.Items?.ElementAtOrDefault(index);

        if (item != null)
        {
            this.Monitor.Log($"Released over player item: {item.DisplayName}", LogLevel.Debug);

            int itemCount = e.Button == SButton.MouseRight ? 1 : item.Stack;
            int amountOfCoalToRemove = (_smeltingRules.OreCoalCosts.First(o => o.QualifiedItemId == item.QualifiedItemId).CoalRequiredToSmelt) * (itemCount);
            int playerHeldAmountOfCoal = Utils.GetHeldCountOfItem("(O)382");

            if (amountOfCoalToRemove <= playerHeldAmountOfCoal)
            {
                Utils.RemoveItemFromPlayerInventory("(O)382", amountOfCoalToRemove);
                this._insufficientCoalFlag = false;

            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("Insufficient amount of coal", HUDMessage.error_type));
                this._insufficientCoalFlag = true;
                this.Helper.Input.Suppress(e.Button);
            }


        }

    }


    /// <summary>
    /// When the list of buildings changes, make sure to add any furnaces to the list of furnaces
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
    {
        // If none of the buildings added were the furnace, just return
        if (!e.Added.Any(b => b.BuildingIsIndustrialFurnaceFlag()))
        {
            return;
        }

        this.populateOnScreenFurnaces();

        

        // Remove destroyed furnaces from the controller list
        //foreach (Building building in e.Removed)
        //{
        //    if (building.BuildingIsIndustrialFurnaceFlag())
        //    {


        //            _onScreenFurnaces.Value.RemoveWhere(f => f.Id == )

        //    }
        //}
    }
   
    private TemporaryAnimatedSprite createSmokeSprite(int x, int y)
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

    private void populateOnScreenFurnaces()
    {

        // Use the farm buildings because for some reason only these have the chests
        foreach (Building farmBuildings in Game1.getFarm().buildings.Where(b => b.BuildingIsIndustrialFurnaceFlag()))
        {
            Logic.IndustrialFurnace furnace = new Logic.IndustrialFurnace();
            farmBuildings.DeepCloneTo(furnace);
            furnace.IndustrialFurnaceId = _onScreenFurnaces.Value.Count();
            _onScreenFurnaces.Value.Add(furnace);
        }
    }
}



using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace FurnaceSmokeStack.Logic;

public class IndustrialFurnace : Building
{
    public int IndustrialFurnaceId { get; set; }
    public bool IsProcessingFlag { get; set; } = false;

    public bool IsInputChestOpenFlag { get; set;  } = false;


    /// <summary>Place items to the furnace</summary>
    /// <param name="furnace">The furnace controller</param>
    /// <returns>Whether the placement was successful or not</returns>
    public bool PlaceItemsInFurnace()
    {
        // Items can be placed only if the furnace is NOT on
        if (this.IsProcessingFlag)
        {
            //Utilities.Utils.DisplayHudMessage(i18n.Get("message.furnace-running"), HUDMessage.error_type, "cancel");
            return false;
        }

        // Get the current held object, null for tools etc.
        var heldItem = Game1.player.ActiveObject;
        if (heldItem is null) return false;

        int objectId = heldItem.ParentSheetIndex;
        return true;
        //Data.SmeltingRule? rule = newSmeltingRules.GetSmeltingRuleFromInputID(objectId);

        //// Check if the object is on the smeltables list
        //if (rule is not null)
        //{
        //    // Prevent the game from division by 0, even if the player edits the rules
        //    if (rule.InputItemAmount == 0)
        //    {
        //        Monitor.Log($"The smelting rule for object {objectId} had 0 for input amount.", LogLevel.Error);
        //        return false;
        //    }

        //    int amount = heldItem.Stack;

        //    // Check if the player has enough to smelt
        //    if (amount >= rule.InputItemAmount)
        //    {
        //        // Remove multiples of the required input amount
        //        int smeltAmount = amount / rule.InputItemAmount;
        //        Game1.player.removeItemsFromInventory(objectId, smeltAmount * rule.InputItemAmount);
        //        furnace.AddItemsToSmelt(objectId, smeltAmount * rule.InputItemAmount);

        //        Monitor.Log($"{Game1.player.Name} placed {smeltAmount * rule.InputItemAmount} {heldItem.Name} to the furnace {furnace.ID}.");
        //        return true;
        //    }
        //    else
        //    {
        //        DisplayHudMessage(i18n.Get("message.need-more-ore", new { oreAmount = rule.InputItemAmount }), HUDMessage.error_type, "cancel");
        //        return false;
        //    }
        //}
        //// Check if the player tries to put coal in the furnace and start the smelting
        //else if (objectId == SObject.coal && !furnace.CurrentlyOn)
        //{
        //    // The input has items to smelt
        //    if (furnace.input.items.Count > 0)
        //    {
        //        if (heldItem.Stack >= config.CoalAmount)
        //        {
        //            Game1.player.removeItemsFromInventory(objectId, config.CoalAmount);

        //            Monitor.Log($"{Game1.player.Name} started the furnace {furnace.ID} with {config.CoalAmount} {heldItem.Name}.");

        //            if (config.InstantSmelting)
        //            {
        //                Monitor.Log("And it finished immediately.");
        //                FinishSmelting(furnace);
        //            }
        //            else
        //            {
        //                furnace.ChangeCurrentlyOn(true);
        //                UpdateTexture(furnace.furnace, true);

        //                CreateLight(furnace);
        //            }

        //            Game1.playSound("furnace");
        //            return true;
        //        }
        //        else
        //        {
        //            DisplayHudMessage(i18n.Get("message.more-coal", new { coalAmount = config.CoalAmount }), HUDMessage.error_type, "cancel");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        DisplayHudMessage(i18n.Get("message.place-something-first"), HUDMessage.error_type, "cancel");
        //        return false;
        //    }
        //}
        //else
        //{
        //    DisplayHudMessage(i18n.Get("message.cant-smelt-this"), HUDMessage.error_type, "cancel");
        //    return false;
        //}
    }
}


using StardewValley.Buildings;
using StardewValley.Objects;

namespace FurnaceSmokeStack.Logic;

public class IndustrialFurnace : Building
{
    public int IndustrialFurnaceId { get; set; }
    public bool IsProcessingFlag { get; set; } = false;

    public bool IsInputChestOpenFlag { get; set;  } = false;

    public Chest GetInputChest()
    {
        return this.buildingChests.First(chest => chest.BaseName.ToLower().Contains("input"));
    }

}


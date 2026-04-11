using StardewValley;
using StardewValley.Buildings;

namespace FurnaceSmokeStack.Utilities;

public static class Extensions
{

    public static bool BuildingIsIndustrialFurnaceFlag(this Building building)
    {
        return building.buildingType.Value.ToLower().Contains("furnace");
    }

    public static bool BuildingIsInRangeOfPlayer(this Building building)
    {
        int playerx = (int)Game1.player.Tile.X;
        int playery = (int)Game1.player.Tile.Y;

        int buildingX = building.tileX.Value;
        int buildingY = building.tileY.Value;

        var xDelta = playerx - buildingX;
        var yDelta = playery - buildingY;

        var withinXFlag = xDelta >= 0 && xDelta <= building.tilesWide.Value;
        var withinYFlag = yDelta == building.tilesHigh.Value;

        return withinXFlag && withinYFlag;
    }
}


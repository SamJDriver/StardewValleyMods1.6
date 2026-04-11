namespace FurnaceSmokeStack.Data;

public class SmeltingRules
{
    public int InputItemID { get; set; }
    public int InputItemAmount { get; set; }
    public int OutputItemID { get; set; }
    public int OutputItemAmount { get; set; }
    public string[]? RequiredModID { get; set; }
}


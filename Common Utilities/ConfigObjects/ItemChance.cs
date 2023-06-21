namespace Common_Utilities.ConfigObjects;

public class ItemChance
{
    public string ItemName { get; set; } = ItemType.None.ToString();

    public double Chance { get; set; }

    public string Group { get; set; } = "none";

    public void Deconstruct(out string name, out double i, out string groupKey)
    {
        name = ItemName;
        i = Chance;
        groupKey = Group;
    }
}
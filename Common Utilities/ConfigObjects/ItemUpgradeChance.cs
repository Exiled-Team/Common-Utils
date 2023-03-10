namespace Common_Utilities.ConfigObjects
{
    public class ItemUpgradeChance
    {
        public ItemType Original { get; set; }
        public ItemType New { get; set; }
        public double Chance { get; set; }

        public void Deconstruct(out ItemType itemType, out ItemType itemType1, out double i)
        {
            itemType = Original;
            itemType1 = New;
            i = Chance;
        }
    }
}
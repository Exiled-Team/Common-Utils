namespace Common_Utilities.ConfigObjects
{
    public class ItemUpgradeChance : IChanceObject
    {
        public string OriginalItem { get; set; }

        public string NewItem { get; set; }

        public double Chance { get; set; }

        public int Count { get; set; } = 1;

        public void Deconstruct(out string originalItem, out string destinationItem, out double chance, out int count)
        {
            originalItem = OriginalItem;
            destinationItem = NewItem;
            chance = Chance;
            count = Count;
        }
    }
}
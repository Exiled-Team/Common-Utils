namespace Common_Utilities.ConfigObjects
{
    public class ItemUpgradeChance
    {
        public object OriginalItem { get; set; }

        public object NewItem { get; set; }

        public double Chance { get; set; }

        public int Count { get; set; } = 1;

        public void Deconstruct(out object original, out double i, out int count)
        {
            Deconstruct(out original, out _, out i, out count);
        }

        public void Deconstruct(out object original, out object newItem, out double i, out int count)
        {
            original = OriginalItem;
            newItem = NewItem;
            i = Chance;
            count = Count;
        }
    }
}
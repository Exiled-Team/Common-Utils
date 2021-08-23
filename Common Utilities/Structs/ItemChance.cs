namespace Common_Utilities.Structs
{
    public struct ItemChance
    {
        public string ItemName { get; set; }
        public int Chance { get; set; }
        public string GroupKey { get; set; }

        public void Deconstruct(out string name, out int i, out string groupKey)
        {
            name = ItemName;
            i = Chance;
            groupKey = GroupKey;
        }
    }
}
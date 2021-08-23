namespace Common_Utilities.Structs
{
    public struct ItemChance
    {
        public string ItemName { get; set; }
        public int Chance { get; set; }

        public void Deconstruct(out string name, out int i)
        {
            name = ItemName;
            i = Chance;
        }
    }
}
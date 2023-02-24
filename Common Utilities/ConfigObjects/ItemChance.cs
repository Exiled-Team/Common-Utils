namespace Common_Utilities.ConfigObjects
{
    public class ItemChance
    {
        public string ItemName { get; set; }
        public float Chance { get; set; }
        public string Group { get; set; }

        public void Deconstruct(out string name, out float i, out string groupKey)
        {
            name = ItemName;
            i = Chance;
            groupKey = Group;
        }
    }
}
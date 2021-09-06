namespace Common_Utilities.ConfigObjects
{
    public class StartingAmmo
    {
        public ItemType Type { get; set; }
        public ushort Amount { get; set; }
        public string Group { get; set; }

        public void Deconstruct(out ItemType type, out ushort limit, out string group)
        {
            type = Type;
            limit = Amount;
            group = Group;
        }
    }
}
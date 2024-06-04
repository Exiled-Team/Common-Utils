namespace Common_Utilities.ConfigObjects
{
    public class StartingAmmo
    {
        public ItemType AmmoType { get; set; }

        public ushort Amount { get; set; }

        public string Group { get; set; } = "none";

        public void Deconstruct(out ItemType ammoType, out ushort amount, out string group)
        {
            ammoType = AmmoType;
            amount = Amount;
            group = Group;
        }
    }
}

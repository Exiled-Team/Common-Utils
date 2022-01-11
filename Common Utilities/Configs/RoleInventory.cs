namespace Common_Utilities.Configs
{
    using System;
    using System.Collections.Generic;
    using Common_Utilities.ConfigObjects;
    using YamlDotNet.Serialization;

    public class RoleInventory
    {
        [YamlIgnore]
        public int UsedSlots
        {
            get
            {
                int i = 0;
                if (Slot1 != null && !Slot1.IsEmpty())
                    i++;
                if (Slot2 != null && !Slot2.IsEmpty())
                    i++;
                if (Slot3 != null && !Slot3.IsEmpty())
                    i++;
                if (Slot4 != null && !Slot4.IsEmpty())
                    i++;
                if (Slot5 != null && !Slot5.IsEmpty())
                    i++;
                if (Slot6 != null && !Slot6.IsEmpty())
                    i++;
                if (Slot7 != null && !Slot7.IsEmpty())
                    i++;
                if (Slot8 != null && !Slot8.IsEmpty())
                    i++;
                return i;
            }
        }

        public List<ItemChance> Slot1 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot2 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot3 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot4 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot5 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot6 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot7 { get; set; } = new List<ItemChance>();
        public List<ItemChance> Slot8 { get; set; } = new List<ItemChance>();

        public List<StartingAmmo> Ammo { get; set; } = new List<StartingAmmo>();

        public IEnumerable<ItemChance> this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return Slot1;
                    case 1:
                        return Slot2;
                    case 2:
                        return Slot3;
                    case 3:
                        return Slot4;
                    case 4:
                        return Slot5;
                    case 5:
                        return Slot6;
                    case 6:
                        return Slot7;
                    case 7:
                        return Slot8;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool CheckGroup(string group)
        {
            // I don't know what I was doing here?
            return true;
        }
    }
}
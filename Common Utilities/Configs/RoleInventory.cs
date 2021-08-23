namespace Common_Utilities.Configs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Common_Utilities.Structs;

    public class RoleInventory
    {
        public int UsedSlots
        {
            get
            {
                int i = 0;
                if (!Slot1.IsEmpty())
                    i++;
                if (!Slot2.IsEmpty())
                    i++;
                if (!Slot3.IsEmpty())
                    i++;
                if (!Slot4.IsEmpty())
                    i++;
                if (!Slot6.IsEmpty())
                    i++;
                if (!Slot7.IsEmpty())
                    i++;
                if (!Slot8.IsEmpty())
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
    }
}
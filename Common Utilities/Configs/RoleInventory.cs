namespace Common_Utilities.Configs
{ 
    using System;
    using System.Collections.Generic;

    using ConfigObjects;
    using YamlDotNet.Serialization;

    public class RoleInventory
    {
        [YamlIgnore]
        public int UsedSlots
        {
            get
            {
                int i = 0;
                if (Slot1 is not null && !Slot1.IsEmpty())
                    i++;
                if (Slot2 is not null && !Slot2.IsEmpty())
                    i++;
                if (Slot3 is not null && !Slot3.IsEmpty())
                    i++;
                if (Slot4 is not null && !Slot4.IsEmpty())
                    i++;
                if (Slot5 is not null && !Slot5.IsEmpty())
                    i++;
                if (Slot6 is not null && !Slot6.IsEmpty())
                    i++;
                if (Slot7 is not null && !Slot7.IsEmpty())
                    i++;
                if (Slot8 is not null && !Slot8.IsEmpty())
                    i++;
                return i;
            }
        }

        public List<ItemChance> Slot1 { get; set; } = new();

        public List<ItemChance> Slot2 { get; set; } = new();

        public List<ItemChance> Slot3 { get; set; } = new();

        public List<ItemChance> Slot4 { get; set; } = new();

        public List<ItemChance> Slot5 { get; set; } = new();

        public List<ItemChance> Slot6 { get; set; } = new();

        public List<ItemChance> Slot7 { get; set; } = new();

        public List<ItemChance> Slot8 { get; set; } = new();

        public List<StartingAmmo> Ammo { get; set; } = new();

        public IEnumerable<ItemChance> this[int i] => i switch
        {
            0 => Slot1,
            1 => Slot2,
            2 => Slot3,
            3 => Slot4,
            4 => Slot5,
            5 => Slot6,
            6 => Slot7,
            7 => Slot8,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
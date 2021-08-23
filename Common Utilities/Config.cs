namespace Common_Utilities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Common_Utilities.Configs;
    using Common_Utilities.Structs;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.CustomItems.API.Features;
    using Scp914;

    public class Config : IConfig
    {
        [Description("Wether or not debug messages should be shown.")]
        public bool Debug { get; set; } = false;

        [Description("The SCP Roles able to use V to talk to humans.")]
        public List<RoleType> ScpSpeech { get; set; } = new List<RoleType>
        {
            RoleType.Scp049
        };

        [Description("Whether or not MTF/CI can 'escape' while disarmed to switch teams.")]
        public bool DisarmSwitchTeams { get; set; } = true;

        [Description("Whether or not disarmed people will be prevented from interacting with doors/elevators.")]
        public bool RestrictiveDisarming { get; set; } = true;
        
        [Description("The text displayed at the timed interval specified below.")]
        public string TimedBroadcast { get; set; } = "<color=lime>This server is running </color><color=red>EXILED Common-Utilities</color><color=lime>, enjoy your stay!</color>";
        [Description("The time each timed broadcast will be displayed.")]
        public ushort TimedBroadcastDuration { get; set; } = 5;
        [Description("The delay between each timed broadcast. To disable timed broadcasts, set this to 0")]
        public float TimedBroadcastDelay { get; set; } = 300f;

        [Description("The message displayed to the player when they first join the server. Setting this to empty will disable these broadcasts.")]
        public string JoinMessage { get; set; } = "<color=lime>Welcome %player%! Please read our rules!</color>";
        [Description("The amount of time (in seconds) the join message is displayed.")]
        public ushort JoinMessageDuration { get; set; } = 5;

        [Description("The amount of time (in seconds) after the round starts, before the facilities auto-nuke will start.")]
        public float AutonukeTime { get; set; } = 600f;
        [Description("Wether or not the nuke should be unable to be disabled during the auto-nuke countdown.")]
        public bool AutonukeLock { get; set; } = true;

        [Description(
            "The list of items Class-D should have. Valid formatting should be ItemType:Chance where ItemType is the item to give them, and Chance is the percent chance of them spawning with it. You can specify the same item multiple times. This is true for all Inventory configs.")]
        public Dictionary<RoleType, RoleInventory> StartingInventories { get; set; } = new Dictionary<RoleType, RoleInventory>
        {
            {
                RoleType.ClassD, new RoleInventory
                {
                    Slot1 = new List<ItemChance>
                    {
                        new ItemChance
                        {
                            ItemName = ItemType.KeycardJanitor.ToString(),
                            Chance = 10,
                        },
                        new ItemChance
                        {
                            ItemName = ItemType.Coin.ToString(),
                            Chance = 100,
                        },
                    },
                    Slot2 = new List<ItemChance>
                    {
                        new ItemChance
                        {
                            ItemName = ItemType.Flashlight.ToString(),
                            Chance = 100,
                        }
                    },
                }
            }
        };
        
        [Description("The list of custom 914 recipies for the Rough setting. Valid formatting should be OriginalItemType:NewItemType:Chance where OriginalItem is the item being upgraded, NewItem is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item.")]
        public Dictionary<Scp914KnobSetting, List<ItemUpgradeChance>> Scp914ItemChanges { get; set; } = new Dictionary<Scp914KnobSetting, List<ItemUpgradeChance>>
        {
            {
                Scp914KnobSetting.Rough, new List<ItemUpgradeChance>
                {
                    {
                        new ItemUpgradeChance
                        {
                            Original = ItemType.KeycardO5,
                            New = ItemType.MicroHID,
                            Chance = 50,
                        }
                    }
                }
            },
        };
        
        [Description("The list of custom 914 recipies for 914. Valid formatting is OriginalRole:NewRole:Chance - IE: ClassD:Spectator:100 - for each knob setting defined.")]
        public Dictionary<Scp914KnobSetting, List<PlayerUpgradeChance>> Scp914ClassChanges { get; set; } = new Dictionary<Scp914KnobSetting, List<PlayerUpgradeChance>>
        {
            {
                Scp914KnobSetting.Rough, new List<PlayerUpgradeChance>
                {
                    {
                        new PlayerUpgradeChance
                        {
                            OldRole = RoleType.ClassD,
                            NewRole = RoleType.Spectator,
                            Chance = 100,
                        }
                    }
                }
            },
        };

        [Description("The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.")]
        public float RagdollCleanupDelay { get; set; } = 0f;

        [Description("If ragdoll cleanup should only happen in the Pocket Dimension or not.")]
        public bool RagdollCleanupOnlyPocket { get; set; } = false;
        
        [Description("The frequency (in seconds) between item cleanups. Set to 0 to disable.")]
        public float ItemCleanupDelay { get; set; } = 0f;

        [Description("If item cleanup should only happen in the Pocket Dimension or not.")]
        public bool ItemCleanupOnlyPocket { get; set; } = false;
        
        [Description("A list of all roles and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<RoleType, float> RoleDamageMultipliers { get; set; } = new Dictionary<RoleType, float>
        {
            {
                RoleType.Scp173, 1.0f
            },
        };
        
        [Description("A list of all Weapons and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<ItemType, float> WeaponDamageMultipliers { get; set; } = new Dictionary<ItemType, float>
        {
            {
                ItemType.GunE11SR, 1.0f
            }
        };

        [Description("A list of roles and how much health they should be given when they kill someone.")]
        public Dictionary<RoleType, float> HealthOnKill { get; set; } = new Dictionary<RoleType, float>
        {
            {
                RoleType.Scp173, 0
            },
            {
                RoleType.Scp93953, 10
            },
            {
                RoleType.Scp93989, 20
            }
        };
        
        [Description("A list of roles and what their default starting health should be.")]
        public Dictionary<RoleType, int> HealthValues { get; set; } = new Dictionary<RoleType, int>
        {
            {
                RoleType.Scp173, 3200
            },
            {
                RoleType.NtfCaptain, 150
            }
        };
        

        [Description("If the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;
    }
}

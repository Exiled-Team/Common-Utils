namespace Common_Utilities
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using ConfigObjects;
    using Configs;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using PlayerRoles;
    using Scp914;
    using UnityEngine;

    public class Config : IConfig
    {
        [Description("Whether or not debug messages should be shown.")]
        public bool Debug { get; set; } = false;

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
        public float AutonukeTime { get; set; } = 1500f;

        [Description("Wether or not the nuke should be unable to be disabled during the auto-nuke countdown.")]
        public bool AutonukeLock { get; set; } = true;

        [Description("The message given to all players when the auto-nuke is triggered. A duration of 2 or more will be a text message on-screen. A duration of 1 makes it a cassie announcement. A duration of 0 disables it.")]
        public Broadcast AutonukeBroadcast { get; set; } = new()
        {
            Content = "The auto nuke has been activated.",
            Duration = 10,
            Show = true,
            Type = global::Broadcast.BroadcastFlags.Normal,
        };

        [Description("Whether or not to show player's health under their name when you look at them.")]
        public bool PlayerHealthInfo { get; set; } = true;

        [Description("Whether or not friendly fire should automatically turn on when a round ends (it will turn itself back off before the next round starts).")]
        public bool FriendlyFireOnRoundEnd { get; set; } = false;

        [Description("The multiplier applied to radio battery usage. Set to 0 to disable radio battery drain.")]
        public float RadioBatteryDrainMultiplier { get; set; } = 1f;

        [Description("The color to use for lights while the warhead is active.")]
        public Color WarheadColor { get; set; } = new(1f, 0.2f, 0.2f);

        [Description("The maximum time, in seconds, that a player can be AFK before being kicked. Set to -1 to disable AFK system.")]
        public int AfkLimit { get; set; } = 120;
        
        [Description("The roles that are ignored by the AFK system.")]
        public List<RoleTypeId> AfkIgnoredRoles { get; set; } = new()
        {
            RoleTypeId.Scp079,
            RoleTypeId.Spectator,
            RoleTypeId.Tutorial,
        };

        [Description("Whether or not probabilities should be additive (50 + 50 = 100) or not (50 + 50 = 2 seperate 50% chances)")]
        public bool AdditiveProbabilities { get; set; } = false;

        [Description(
            "The list of starting items for roles. ItemName is the item to give them, and Chance is the percent chance of them spawning with it, and Group allows you to restrict the item to only players with certain RA groups (Leave this as 'none' to allow all players to get the item). You can specify the same item multiple times.")]
        public Dictionary<RoleTypeId, RoleInventory> StartingInventories { get; set; } = new()
        {
            {
                RoleTypeId.ClassD, new RoleInventory
                {
                    Slot1 = new List<ItemChance>
                    {
                        new()
                        {
                            ItemName = ItemType.KeycardJanitor.ToString(),
                            Chance = 10,
                            Group = "none",
                        },
                        new()
                        {
                            ItemName = ItemType.Coin.ToString(),
                            Chance = 100,
                            Group = "none",
                        },
                    },
                    Slot2 = new List<ItemChance>
                    {
                        new()
                        {
                            ItemName = ItemType.Flashlight.ToString(),
                            Chance = 100,
                            Group = "none",
                        },
                    },
                    Ammo = new List<StartingAmmo>
                    {
                        new()
                        {
                            Type = ItemType.Ammo556x45,
                            Amount = 200,
                            Group = "none",
                        },
                    },
                }
            },
        };

        [Description("The list of custom 914 recipies. Original is the item being upgraded, New is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item.")]
        public Dictionary<Scp914KnobSetting, List<ItemUpgradeChance>> Scp914ItemChanges { get; set; } = new()
        {
            {
                Scp914KnobSetting.Rough, new List<ItemUpgradeChance>
                {
                    {
                        new()
                        {
                            Original = ItemType.KeycardO5,
                            New = ItemType.MicroHID,
                            Chance = 50,
                        }
                    },
                }
            },
        };
        
        [Description("The list of custom 914 recipies for roles. Original is the role to be changed, New is the new role to assign, Chance is the % chance of the upgrade occuring.")]
        public Dictionary<Scp914KnobSetting, List<PlayerUpgradeChance>> Scp914ClassChanges { get; set; } = new()
        {
            {
                Scp914KnobSetting.Rough, new List<PlayerUpgradeChance>
                {
                    {
                        new()
                        {
                            Original = RoleTypeId.ClassD,
                            New = RoleTypeId.Spectator.ToString(),
                            Chance = 100,
                        }
                    },
                }
            },
        };

        [Description("The list of 914 teleport settings. Note that if you set \"zone\" to anything other than Unspecified, it will always select a random room from that zone, instead of the room type defined.")]
        public Dictionary<Scp914KnobSetting, List<Scp914TeleportChance>> Scp914TeleportChances { get; set; } = new()
        {
            {
                Scp914KnobSetting.Rough, new List<Scp914TeleportChance>
                {
                    new()
                    {
                        Room = RoomType.LczClassDSpawn,
                        Chance = 100,
                    },
                }
            },
        };

        [Description("A dictionary of random effects to apply to players when going through 914 on certain settings.")]
        public Dictionary<Scp914KnobSetting, List<Scp914EffectChance>> Scp914EffectChances { get; set; } = new()
        {
            {
                Scp914KnobSetting.Rough, new List<Scp914EffectChance>
                {
                    new()
                    {
                        Effect = EffectType.Bleeding,
                        Chance = 100,
                    },
                }
            },
        };

        [Description("Determines if 914 effects are exclusive, meaning only one can be applied each time a player is processed by 914.")]
        public bool Scp914EffectsExclusivity { get; set; } = false;

        [Description("Whether or not SCPs are immune to effects gained from 914.")]
        public bool ScpsImmuneTo914Effects { get; set; } = false;

        [Description("The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.")]
        public float RagdollCleanupDelay { get; set; } = 0f;

        [Description("If ragdoll cleanup should only happen in the Pocket Dimension or not.")]
        public bool RagdollCleanupOnlyPocket { get; set; } = false;
        
        [Description("The frequency (in seconds) between item cleanups. Set to 0 to disable.")]
        public float ItemCleanupDelay { get; set; } = 0f;

        [Description("If item cleanup should only happen in the Pocket Dimension or not.")]
        public bool ItemCleanupOnlyPocket { get; set; } = false;
        
        [Description("A list of all roles and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<RoleTypeId, float> RoleDamageMultipliers { get; set; } = new()
        {
            {
                RoleTypeId.Scp173, 1.0f
            },
        };
        
        [Description("A list of all Weapons and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<DamageType, float> DamageMultipliers { get; set; } = new()
        {
            {
                DamageType.E11Sr, 1.0f
            },
        };

        [Description("A list of roles and how much health they should be given when they kill someone.")]
        public Dictionary<RoleTypeId, float> HealthOnKill { get; set; } = new()
        {
            {
                RoleTypeId.Scp173, 0
            },
            {
                RoleTypeId.Scp939, 10
            },
        };
        
        [Description("A list of roles and what their default starting health should be.")]
        public Dictionary<RoleTypeId, int> HealthValues { get; set; } = new()
        {
            {
                RoleTypeId.Scp173, 3200
            },
            {
                RoleTypeId.NtfCaptain, 150
            },
        };

        [Description("If the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;
    }
}

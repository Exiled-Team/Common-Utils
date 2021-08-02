using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.CustomItems.API.Features;
using Scp914;

namespace Common_Utilities
{
    public class Config : IConfig
    {
        [Description("Wether or not debug messages should be shown.")]
        public bool Debug { get; set; } = false;

        [Description("Can 106 speak to humans?")]
        public bool CanScp106Speak { get; set; } = true;

        [Description("Can 049 speak to humans?")]
        public bool CanScp049Speak { get; set; } = true;

        [Description("Can 173 speak to humans?")]
        public bool CanScp173Speak { get; set; } = true;

        [Description("Can 096 speak to humans?")]
        public bool CanScp096Speak { get; set; } = true;

        [Description("Can 049-2 speak to humans?")]
        public bool CanScp0492Speak { get; set; } = true;

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
        public Dictionary<string, List<string>> ClassDInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> ChaosInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> ScientistInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> GuardInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> CadetInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> LieutenantInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> CommanderInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        public Dictionary<string, List<string>> NtfSciInventory { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "slot1", new List<string>()
            },
            {
                "slot2", new List<string>()
            },
            {
                "slot3", new List<string>()
            },
            {
                "slot4", new List<string>()
            },
            {
                "slot5", new List<string>()
            },
            {
                "slot6", new List<string>()
            },
            {
                "slot7", new List<string>()
            },
            {
                "slot8", new List<string>()
            },
        };
        
        [Description("The list of custom 914 recipies for the Rough setting. Valid formatting should be OriginalItemType:NewItemType:Chance where OriginalItem is the item being upgraded, NewItem is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item.")]
        public Dictionary<string, List<string>> Scp914ItemChanges { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "Rough", new List<string>()
            },
            {
                "Coarse", new List<string>()
            },
            {
                "OneToOne", new List<string>()
            },
            {
                "Fine", new List<string>()
            },
            {
                "VeryFine", new List<string>()
            }
        };
        
        [Description("The list of custom 914 recipies for 914. Valid formatting is OriginalRole:NewRole:Chance - IE: ClassD:Spectator:100 - for each knob setting defined.")]
        public Dictionary<string, List<string>> Scp914ClassChanges { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "Rough", new List<string>()
            },
            {
                "Coarse", new List<string>()
            },
            {
                "OneToOne", new List<string>()
            },
            {
                "Fine", new List<string>()
            },
            {
                "VeryFine", new List<string>()
            }
        };

        [Description("The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.")]
        public float RagdollCleanupDelay { get; set; } = 0f;

        [Description("If ragdoll cleanup should only happen in the Pocket Dimension or not.")]
        public bool RagdollCleanupOnlyPocket { get; set; } = false;
        
        [Description("The frequency (in seconds) between item cleanups. Set to 0 to disable.")]
        public float ItemCleanupDelay { get; set; } = 0f;

        [Description("If item cleanup should only happen in the Pocket Dimension or not.")]
        public bool ItemCleanupOnlyPocket { get; set; } = false;
        
        [Description("A list of all SCP roles and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<string, float> ScpDamageMultipliers { get; set; } = new Dictionary<string, float>
        {
            {
                "Scp173", 1.0f
            },
        };
        
        [Description("A list of all Weapons and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
        public Dictionary<string, float> WeaponDamageMultipliers { get; set; } = new Dictionary<string, float>
        {
            {
                "GunE11SR", 1.0f
                
            }
        };

        [Description("A list of roles and how much health they should be given when they kill someone.")]
        public Dictionary<string, float> HealthOnKill { get; set; } = new Dictionary<string, float>
        {
            {
                "Scp173", 0
            },
            {
                "Scp096", 0
            }
        };
        
        [Description("A list of roles and what their default starting health should be.")]
        public Dictionary<string, int> HealthValues { get; set; } = new Dictionary<string, int>
        {
            {
                "Scp173", 3200
            },
            {
                "NtfCommander", 150
            }
        };

        internal Dictionary<RoleType, Dictionary<string, List<Tuple<ItemType, int>>>> Inventories = new Dictionary<RoleType, Dictionary<string, List<Tuple<ItemType, int>>>>();
        internal Dictionary<RoleType, Dictionary<string, List<Tuple<CustomItem, int>>>> CustomInventories = new Dictionary<RoleType, Dictionary<string, List<Tuple<CustomItem, int>>>>();
        internal Dictionary<Scp914Knob, List<Tuple<ItemType, ItemType, int>>> Scp914Configs = new Dictionary<Scp914Knob, List<Tuple<ItemType, ItemType, int>>>();
        internal Dictionary<RoleType, int> Health = new Dictionary<RoleType, int>();
        internal Dictionary<RoleType, float> HealOnKill = new Dictionary<RoleType, float>();
        internal Dictionary<Scp914Knob, List<Tuple<RoleType, RoleType, int>>> Scp914RoleChanges = new Dictionary<Scp914Knob, List<Tuple<RoleType, RoleType, int>>>();
        internal Dictionary<RoleType, float> ScpDmgMult = new Dictionary<RoleType, float>();
        internal Dictionary<ItemType, float> WepDmgMult = new Dictionary<ItemType, float>();


        [Description("If the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;


        internal void ParseWeaponDamageMultipliers()
        {
            foreach (KeyValuePair<string, float> setting in WeaponDamageMultipliers)
            {
                try
                {
                    ItemType type = (ItemType) Enum.Parse(typeof(ItemType), setting.Key);

                    if (!type.IsWeapon())
                    {
                        Log.Warn($"{type} is not a valid weapon!");
                        continue;
                    }

                    WepDmgMult.Add(type, setting.Value);
                }
                catch (Exception)
                {
                    Log.Error($"Failed to parse Weapon Damage Multiplier: {setting.Key} is not a valid item type.");
                }
            }
        }
        internal void ParseScpDamageMultipliers()
        {
            foreach (KeyValuePair<string, float> setting in ScpDamageMultipliers)
            {
                try
                {
                    RoleType type = (RoleType) Enum.Parse(typeof(RoleType), setting.Key, true);

                    if (!IsScp(type))
                    {
                        Log.Warn($"{type} is not a valid SCP role.");
                        continue;
                    }
                    
                    ScpDmgMult.Add(type, setting.Value);
                }
                catch (Exception)
                {
                    Log.Error($"Failed to parse SCP Damage Multiplier: {setting.Key} is not a valid role type.");
                }
            }
        }

        bool IsScp(RoleType type) => type == RoleType.Scp049 || type == RoleType.Scp079 || type == RoleType.Scp096 || type == RoleType.Scp106 || type == RoleType.Scp173 || type == RoleType.Scp0492 || type.Is939();

        internal void Parse914ClassChanges()
        {
            foreach (KeyValuePair<string, List<string>> setting in Scp914ClassChanges)
            {
                try
                {
                    Scp914Knob knob = (Scp914Knob) Enum.Parse(typeof(Scp914Knob), setting.Key);

                    foreach (string chances in setting.Value)
                    {
                        if (string.IsNullOrEmpty(chances))
                            continue;
                        
                        string[] split = chances.Split(':');

                        if (split.Length < 3)
                        {
                            Log.Error($"Unable to parse SCP-914 class chance: {chances}. Invalid number of splits.");
                            continue;
                        }

                        RoleType originalRole;
                        RoleType newRole;
                        try
                        {
                            originalRole = (RoleType) Enum.Parse(typeof(RoleType), split[0]);
                        }
                        catch (Exception)
                        {
                            Log.Warn($"Unable to parse role: {split[0]} for {chances}.");
                            continue;
                        }

                        try
                        {
                            newRole = (RoleType) Enum.Parse(typeof(RoleType), split[1]);
                        }
                        catch (Exception)
                        {
                            Log.Warn($"Unable to parse role: {split[1]} for {chances}.");
                            continue;
                        }

                        if (!int.TryParse(split[2], out int chance))
                        {
                            Log.Warn($"Unable to parse chance {split[2]} for {chance}. Invalid integer.");
                            continue;
                        }
                        
                        if (!Scp914RoleChanges.ContainsKey(knob))
                            Scp914RoleChanges.Add(knob, new List<Tuple<RoleType,RoleType, int>>());
                        
                        Scp914RoleChanges[knob].Add(new Tuple<RoleType, RoleType, int>(originalRole, newRole, chance));
                    }
                }
                catch (Exception)
                {
                    Log.Warn($"Unable to parse {setting.Key} as a SCP-914 knob setting.");
                    continue;
                }
            }
        }
        
        internal void ParseHealthOnKill()
        {
            foreach (KeyValuePair<string, float> healOnKillSetting in HealthOnKill)
            {
                RoleType role;

                try
                {
                    role = (RoleType) Enum.Parse(typeof(RoleType), healOnKillSetting.Key, true);
                }
                catch (Exception)
                {
                    Log.Error($"Unable to parse Health on Kill Role: {healOnKillSetting.Key}.");
                    continue;
                }

                if (role == RoleType.None)
                {
                    Log.Error($"Role is none: {healOnKillSetting.Key}, {healOnKillSetting.Value} - This shouldn't happen.");
                    continue;
                }
                
                if (!HealOnKill.ContainsKey(role) && healOnKillSetting.Value > 0f)
                    HealOnKill.Add(role, healOnKillSetting.Value);
            }
        }

        internal void ParseHealthSettings()
        {
            foreach (KeyValuePair<string, int> healthSetting in HealthValues)
            {
                RoleType role;
                try
                {
                    role = (RoleType) Enum.Parse(typeof(RoleType), healthSetting.Key, true);
                }
                catch (Exception)
                {
                    Log.Error($"Unable to parse Role: {healthSetting.Key}");
                    continue;
                }

                if (role == RoleType.None)
                {
                    Log.Error($"Role type is null: {healthSetting.Key}, {healthSetting.Value}. This shouldn't happen.");
                    continue;
                }
                
                Log.Debug($"Added {role} to dictionary with {healthSetting.Value} value.", Debug);
                if (!Health.ContainsKey(role) && healthSetting.Value > 0)
                    Health.Add(role, healthSetting.Value);
            }
        }
        
        internal void Parse914Settings()
        {
            foreach (KeyValuePair<string, List<string>> setting in Scp914ItemChanges)
            {
                try
                {
                    Scp914Knob knob = (Scp914Knob) Enum.Parse(typeof(Scp914Knob), setting.Key);

                    foreach (string chances in setting.Value)
                    {
                        if (string.IsNullOrEmpty(chances))
                            continue;
                        
                        string[] split = chances.Split(':');

                        if (split.Length < 3)
                        {
                            Log.Error($"Unable to parse SCP-914 class chance: {chances}. Invalid number of splits.");
                            continue;
                        }

                        ItemType originalRole;
                        ItemType newRole;
                        try
                        {
                            originalRole = (ItemType) Enum.Parse(typeof(ItemType), split[0]);
                        }
                        catch (Exception)
                        {
                            Log.Warn($"Unable to parse role: {split[0]} for {chances}.");
                            continue;
                        }

                        try
                        {
                            newRole = (ItemType) Enum.Parse(typeof(ItemType), split[1]);
                        }
                        catch (Exception)
                        {
                            Log.Warn($"Unable to parse role: {split[1]} for {chances}.");
                            continue;
                        }

                        if (!int.TryParse(split[2], out int chance))
                        {
                            Log.Warn($"Unable to parse chance {split[2]} for {chance}. Invalid integer.");
                            continue;
                        }
                        
                        if (!Scp914Configs.ContainsKey(knob))
                            Scp914Configs.Add(knob, new List<Tuple<ItemType,ItemType, int>>());
                        
                        Scp914Configs[knob].Add(new Tuple<ItemType, ItemType, int>(originalRole, newRole, chance));
                    }
                }
                catch (Exception)
                {
                    Log.Warn($"Unable to parse {setting.Key} as a SCP-914 knob setting.");
                }
            }
        }

        internal void ParseInventorySettings()
        {
            foreach (PropertyInfo configSetting in GetType().GetProperties())
            {
                Log.Debug($"Name: {configSetting.Name}");
                if (!configSetting.Name.Contains("Inventory"))
                    continue;

                string configName = configSetting.Name;

                Dictionary<string, List<string>> dict = (Dictionary<string, List<string>>) configSetting.GetValue(this);
                RoleType role = RoleType.None;
                switch (configName)
                {
                    case nameof(ClassDInventory):
                        role = RoleType.ClassD;
                        break;
                    case nameof(ChaosInventory):
                        role = RoleType.ChaosInsurgency;
                        break;
                    case nameof(ScientistInventory):
                        role = RoleType.Scientist;
                        break;
                    case nameof(GuardInventory):
                        role = RoleType.FacilityGuard;
                        break;
                    case nameof(CadetInventory):
                        role = RoleType.NtfCadet;
                        break;
                    case nameof(LieutenantInventory):
                        role = RoleType.NtfLieutenant;
                        break;
                    case nameof(CommanderInventory):
                        role = RoleType.NtfCommander;
                        break;
                    case nameof(NtfSciInventory):
                        role = RoleType.NtfScientist;
                        break;
                }

                if (role == RoleType.None)
                {
                    Log.Error("Role is none - This should never happen.");
                    continue;
                }

                if (dict == null || dict.All(l => l.Value == null))
                {
                    Log.Warn($"{nameof(ParseInventorySettings)}: The dictionary for {configName} is empty, they will have default inventory.");
                    if (Inventories.ContainsKey(role))
                        Inventories.Remove(role);
                    
                    continue;
                }

                foreach (KeyValuePair<string, List<string>> unparsedDict in dict)
                {
                    string slotName = unparsedDict.Key;
                    List<string> list = unparsedDict.Value;
                    if (list == null)
                    {
                        Log.Debug($"{nameof(ParseInventorySettings)}: The list for {configName}:{slotName} is empty.");

                        continue;
                    }

                    foreach (string unparsedRaw in list)
                    {
                        ItemType item;
                        if (unparsedRaw == "empty")
                        {
                            Log.Debug($"{nameof(ParseInventorySettings)}: {role} inventory has been set to \"empty\", they will spawn with no items.", Debug);
                            if (!Inventories.ContainsKey(role))
                                Inventories.Add(role, new Dictionary<string, List<Tuple<ItemType, int>>>{{slotName, new List<Tuple<ItemType, int>>()}});
                            continue;
                        }

                        string[] rawChance = unparsedRaw.Split(':');
                        
                        if (!int.TryParse(rawChance[1], out int chance))
                        {
                            Log.Error(
                                $"{nameof(ParseInventorySettings)}: Unable to parse item chance {rawChance[0]} for {rawChance[0]} in {configName} inventory settings.");
                            continue;
                        }
                        
                        if (CustomItem.TryGet(rawChance[0], out CustomItem customItem))
                        {
                            Log.Debug($"{nameof(ParseInventorySettings)}: {rawChance[0]} is a custom item, adding to dictionary..", Debug);
                            if (!CustomInventories.ContainsKey(role))
                                CustomInventories.Add(role, new Dictionary<string, List<Tuple<CustomItem, int>>>
                                {
                                    {"slot1", new List<Tuple<CustomItem, int>>()},{"slot2", new List<Tuple<CustomItem, int>>()},{"slot3", new List<Tuple<CustomItem, int>>()},{"slot4", new List<Tuple<CustomItem, int>>()},{"slot5", new List<Tuple<CustomItem, int>>()},{"slot6", new List<Tuple<CustomItem, int>>()},{"slot7", new List<Tuple<CustomItem, int>>()},{"slot8", new List<Tuple<CustomItem, int>>()},
                                });
                            CustomInventories[role][slotName].Add(new Tuple<CustomItem, int>(customItem, chance));

                            continue;
                        }

                        try
                        {
                            item = (ItemType) Enum.Parse(typeof(ItemType), rawChance[0], true);
                        }
                        catch (Exception)
                        {
                            Log.Error($"{nameof(ParseInventorySettings)}: Unable to parse item: {rawChance[0]} in {configName} inventory settings.");
                            continue;
                        }

                        Log.Debug($"{nameof(ParseInventorySettings)}: {item} was added to {configName} inventory with {chance} chance.", Debug);
                        if (!Inventories.ContainsKey(role))
                            Inventories.Add(role, new Dictionary<string, List<Tuple<ItemType, int>>>
                            {
                                {"slot1", new List<Tuple<ItemType, int>>()}, {"slot2", new List<Tuple<ItemType, int>>()}, {"slot3", new List<Tuple<ItemType, int>>()}, {"slot4", new List<Tuple<ItemType, int>>()}, {"slot5", new List<Tuple<ItemType, int>>()}, {"slot6", new List<Tuple<ItemType, int>>()}, {"slot7", new List<Tuple<ItemType, int>>()}, {"slot8", new List<Tuple<ItemType, int>>()}
                            });
                        Inventories[role][slotName].Add(new Tuple<ItemType, int>(item, chance));
                    }
                }
            }
        }
    }
}

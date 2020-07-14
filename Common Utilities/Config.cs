using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Scp914;

namespace Common_Utilities
{
    public class Config : IConfig
    {
        [Description("Wether or not debug messages should be shown.")]
        public bool Debug { get; set; } = false;

        [Description("Wether or not SCP-049 should be able to talk to humans.")]
        public bool Scp049Speech { get; set; } = true;
        
        [Description("The text displayed at the timed interval specified below.")]
        public string TimedBroadcast { get; set; } = "<color=lime>This server is running <color=red>EXILED Common-Utilities</color>, enjoy your stay!";
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
        
        [Description("The list of items Class-D should have. Valid formatting should be ItemType:Chance where ItemType is the item to give them, and Chance is the percent chance of them spawning with it. You can specify the same item multiple times. This is true for all Inventory configs.")]
        public List<string> ClassDInventory { get; set; } = new List<string>();
        public List<string> ChaosInventory { get; set; } = new List<string>();
        public List<string> ScientistInventory { get; set; } = new List<string>();
        public List<string> GuardInventory { get; set; } = new List<string>();
        public List<string> CadetInventory { get; set; } = new List<string>();
        public List<string> LieutenantInventory { get; set; } = new List<string>();
        public List<string> CommanderInventory { get; set; } = new List<string>();
        public List<string> NtfSciInventory { get; set; } = new List<string>();
        
        [Description("The list of custom 914 recipies for the Rough setting. Valid formatting should be OriginalItemType:NewItemType:Chance where OriginalItem is the item being upgraded, NewItem is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item. This is true for all 914 configs.")]
        public List<string> Scp914RoughChances { get; set; } = new List<string>();
        public List<string> Scp914CoarseChances { get; set; } = new List<string>();
        public List<string> Scp914OnetoOneChances { get; set; } = new List<string>();
        public List<string> Scp914FineChances { get; set; } = new List<string>();
        public List<string> Scp914VeryFineChances { get; set; } = new List<string>();

        [Description("The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.")]
        public float RagdollCleanupDelay { get; set; } = 300f;
        [Description("The frequency (in seconds) between item cleanups. Set to 0 to disable.")]
        public float ItemCleanupDelay { get; set; } = 300f;

        public Dictionary<string, float> HealthOnKill { get; set; } = new Dictionary<string, float>();
        
        public Dictionary<string, int> HealthValues { get; set; } = new Dictionary<string, int>();

        internal Dictionary<RoleType, List<Tuple<ItemType, int>>> Inventories = new Dictionary<RoleType, List<Tuple<ItemType, int>>>();
        internal Dictionary<Scp914Knob, List<Tuple<ItemType, ItemType, int>>> Scp914Configs = new Dictionary<Scp914Knob, List<Tuple<ItemType, ItemType, int>>>();
        internal Dictionary<RoleType, int> Health = new Dictionary<RoleType, int>();
        internal Dictionary<RoleType, float> HealOnKill = new Dictionary<RoleType, float>();


        [Description("If the plugin is enabled or not.")]
        public bool IsEnabled { get; set; }
        
        internal void ParseHealthOnKill()
        {
            foreach (KeyValuePair<string, float> healOnKillSetting in HealthOnKill)
            {
                RoleType role;

                try
                {
                    role = (RoleType) Enum.Parse(typeof(RoleType), healOnKillSetting.Key, true);
                }
                catch (Exception e)
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
                catch (Exception e)
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
            foreach (PropertyInfo configSetting in GetType().GetProperties())
            {
                if (configSetting.Name.Contains("Scp914"))
                    continue;

                string configName = configSetting.Name;

                List<string> list = (List<string>) GetType().GetMethod($"get_{configName}", BindingFlags.Public)?.Invoke(this, null);
                Scp914Knob knobSetting = Scp914Knob.Rough;

                switch (configName)
                {
                    case nameof(Scp914RoughChances):
                        knobSetting = Scp914Knob.Rough;
                        break;
                    case nameof(Scp914CoarseChances):
                        knobSetting = Scp914Knob.Coarse;
                        break;
                    case nameof(Scp914OnetoOneChances):
                        knobSetting = Scp914Knob.OneToOne;
                        break;
                    case nameof(Scp914FineChances):
                        knobSetting = Scp914Knob.Fine;
                        break;
                    case nameof(Scp914VeryFineChances):
                        knobSetting = Scp914Knob.VeryFine;
                        break;
                }

                if (list == null)
                {
                    Log.Warn($"This list for {configName} is empty. Only base-game recipies will be used for this setting.");

                    if (Scp914Configs.ContainsKey(knobSetting))
                        Scp914Configs.Remove(knobSetting);

                    continue;
                }
                
                foreach (string unparsedRaw in list)
                {
                    ItemType sourceItem;
                    ItemType destinationItem;
                    string[] rawChances = unparsedRaw.Split(':');

                    try
                    {
                        sourceItem = (ItemType) Enum.Parse(typeof(ItemType), rawChances[0]);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Unable to parse source item: {rawChances[0]}.");
                        continue;
                    }

                    try
                    {
                        destinationItem = (ItemType) Enum.Parse(typeof(ItemType), rawChances[1]);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Unable to parse destination item: {rawChances[1]}");
                        continue;
                    }

                    if (!int.TryParse(rawChances[2], out int chance))
                    {
                        Log.Error($"Unable to parse conversion chance: {rawChances[2]} for {rawChances[0]} -> {rawChances[1]}.");
                        continue;
                    }
                    
                    Log.Debug($"Scp914 recipe added: {rawChances[0]} -> {rawChances[1]}, {rawChances[2]}%.", Debug);
                    if (!Scp914Configs.ContainsKey(knobSetting))
                        Scp914Configs.Add(knobSetting, new List<Tuple<ItemType, ItemType, int>>());
                    Scp914Configs[knobSetting].Add(new Tuple<ItemType, ItemType, int>(sourceItem, destinationItem, chance));
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
                
                List<string> list = (List<string>) GetType().GetMethod($"get_{configName}", BindingFlags.Public)?.Invoke(this, null);
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

                if (list == null)
                {
                    Log.Warn($"The list for {configName} is empty, they will have default inventory.");

                    if (Inventories.ContainsKey(role))
                        Inventories.Remove(role);
                    
                    continue;
                }

                foreach (string unparsedRaw in list)
                {
                    ItemType item;
                    if (unparsedRaw == "empty")
                    {
                        if (!Inventories.ContainsKey(role))
                            Inventories.Add(role, new List<Tuple<ItemType, int>>());
                        continue;
                    }
                    
                    string[] rawChance = unparsedRaw.Split(':');

                    try
                    {
                        item = (ItemType) Enum.Parse(typeof(ItemType), rawChance[0], true);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Unable to parse item: {rawChance[0]} in {configName} inventory settings.");
                        continue;
                    }

                    if (!int.TryParse(rawChance[1], out int chance))
                    {
                        Log.Error($"Unable to parse item chance {rawChance[0]} for {rawChance[0]} in {configName} inventory settings.");
                        continue;
                    }
                    
                    Log.Debug($"{item} was added to {configName} inventory with {chance} chance.", Debug);
                    if (!Inventories.ContainsKey(role))
                        Inventories.Add(role, new List<Tuple<ItemType, int>>());
                    Inventories[role].Add(new Tuple<ItemType, int>(item, chance));
                }
            }
        }
    }
}
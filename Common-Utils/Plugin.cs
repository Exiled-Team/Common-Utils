
using EXILED;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MEC;
using Scp914;
using Harmony;

/// <summary>
/// Thank you too everyone who contributed to this plugin, ily joker <3
/// </summary>

namespace Common_Utils
{
    public class Plugin : EXILED.Plugin
    {
        public static Plugin Instance { private set; get; }
        public override string getName => "Common-Utils";
        public bool EnableRandomInv;
        public Random Gen = new Random();
        public List<RoleType> TeslaIgnoredRoles = new List<RoleType>();
        public int PatchCounter;

        public bool Scp173Healing = true;
        public int Scp173HealAmount = 150;
        
        public bool Scp049Healing = true;
        public int Scp049HealAmount = 25;
        public float Scp049HealPow = 1.25f;

        public bool Scp0492Healing = true;
        public int Scp0492HealAmount = 25;
        
        public bool Scp106Healing = true;
        public int Scp106HealAmount = 75;
        
        public bool Scp096Healing = true;
        public int Scp096Heal = 150;
        
        public bool Scp939Healing = true;
        public int Scp939Heal = 125;

        // classes be like mega stupid amirite ladies?

        public partial class Scp914ItemUpgrade
        {
            public ItemType ToUpgrade { get; set; }
            public ItemType UpgradedTo { get; set; }

            public static Scp914ItemUpgrade ParseString(string config)
            {
                string[] splitted = config.Split('-');
                DebugBoi("Adding upgrade: " + splitted[0] + " --> " + splitted[1]);
                return new Scp914ItemUpgrade() { ToUpgrade = (ItemType)Enum.Parse(typeof(ItemType), splitted[0], true), UpgradedTo = (ItemType)Enum.Parse(typeof(ItemType), splitted[1], true) };
            }
        }

        public partial class Scp914PlayerUpgrade
        {
            public RoleType ToUpgrade { get; set; }
            public RoleType UpgradedTo { get; set; }

            public static Scp914PlayerUpgrade ParseString(string config)
            {
                string[] splitted = config.Split('-');
                DebugBoi("Adding upgrade: " + splitted[0] + " --> " + splitted[1]);
                return new Scp914PlayerUpgrade() { ToUpgrade = (RoleType)Enum.Parse(typeof(RoleType), splitted[0], true), UpgradedTo = (RoleType)Enum.Parse(typeof(RoleType), splitted[1], true) };
            }
        }

        // Iven tory. lol

        public partial class CustomInventory
        {
            public List<ItemType> NtfCadet = null;

            public List<ItemType> NtfLieutenant = null;

            public List<ItemType> NtfCommander = null;

            public List<ItemType> ClassD = null;

            public List<ItemType> Scientist = null;

            public List<ItemType> NtfScientist = null;

            public List<ItemType> Chaos = null;

            public List<ItemType> Guard = null;

            public Dictionary<ItemType, int> NtfCadetRan;
            public Dictionary<ItemType, int> NtfLtRan;
            public Dictionary<ItemType, int> NtfCmdRan;
            public Dictionary<ItemType, int> ClassDRan;
            public Dictionary<ItemType, int> ScientistRan;
            public Dictionary<ItemType, int> NtfSciRan;
            public Dictionary<ItemType, int> ChaosRan;
            public Dictionary<ItemType, int> GuardRan;

            public static List<ItemType> ConvertToItemList(List<string> list)
            {
                if (list == null)
                    return new List<ItemType>();
                List<ItemType> listd = new List<ItemType>();
                foreach (string s in list)
                {
                    DebugBoi("Adding item " + s);
                    listd.Add((ItemType)Enum.Parse(typeof(ItemType), s, true));
                }
                return listd;
            }

            public static Dictionary<ItemType, int> ConvertToRandomItemList(Dictionary<string, int> dict)
            {
                if (dict == null)
                    return null;
                if (dict.ContainsKey("null"))
                    return new Dictionary<ItemType, int>();
                Dictionary<ItemType, int> list = new Dictionary<ItemType, int>();
                foreach (string s in dict.Keys)
                {
                    Log.Debug($"Adding item: {s}");
                    list.Add((ItemType) Enum.Parse(typeof(ItemType), s, true), dict[s]);
                }

                return list;
            }

        }



        public CoroutineHandle cor;

        // Config settings.

        public CustomInventory Inventories = new CustomInventory();

        public Dictionary<RoleType, int> roleHealth = new Dictionary<RoleType, int>();

        public Dictionary<Scp914PlayerUpgrade, Scp914Knob> scp914Roles = new Dictionary<Scp914PlayerUpgrade, Scp914Knob>();

        public Dictionary<Scp914ItemUpgrade, Scp914Knob> scp914Items = new Dictionary<Scp914ItemUpgrade, Scp914Knob>();

        public EventHandlers EventHandler;

        public HarmonyInstance HarmonyInstance { private set; get; }
        
        public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= EventHandler.PlayerJoin;
            Events.Scp914UpgradeEvent -= EventHandler.SCP914Upgrade;
            Events.RoundStartEvent -= EventHandler.RoundStart;
            Events.PlayerSpawnEvent -= EventHandler.OnPlayerSpawn;
            Events.RoundEndEvent -= EventHandler.OnRoundEnd;
            Events.WaitingForPlayersEvent -= EventHandler.OnWaitingForPlayers;
            Events.TriggerTeslaEvent -= EventHandler.OnTriggerTesla;
            Events.PlayerDeathEvent -= EventHandler.OnPlayerDeath;
            Events.PocketDimDeathEvent -= EventHandler.OnPocketDeath;
            Events.Scp096EnrageEvent -= EventHandler.OnEnrage;
            Events.Scp096CalmEvent -= EventHandler.OnCalm;
            Events.PlayerHurtEvent -= EventHandler.OnPlayerHurt;

            Timing.KillCoroutines(cor);

            Inventories = null;
            roleHealth.Clear();
            scp914Items.Clear();
            scp914Roles.Clear();

            scp914Items = null;
            scp914Roles = null;
            EventHandler = null;
            Instance = null;
            HarmonyInstance?.UnpatchAll();
        }

        public static void DebugBoi(string line)
        {
            if (Config.GetBool("util_debug", false))
                Log.Info("CU-DEBUG | " + line);
        }

        public override void OnEnable()
        {
            if (!Config.GetBool("util_enable", true))
                return;

            Log.Info("Loading Common-Utils, created by the EXILED Team!");
            
            Instance = this;

            HarmonyInstance = HarmonyInstance.Create($"exiled.common.utils-{PatchCounter}");
            PatchCounter++;
            HarmonyInstance.PatchAll();

            bool enable914Configs = Config.GetBool("util_914_enable", true);

            try
            {
                List<string> teslaIgnoredStrings =
                    KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_tesla_ignores", "Tutorial"));

                foreach (string s in teslaIgnoredStrings)
                {
                    RoleType type = (RoleType) Enum.Parse(typeof(RoleType), s);
                    if (!TeslaIgnoredRoles.Contains(type))
                        TeslaIgnoredRoles.Add(type);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Tesla ignored roles error: {e}");
            }

            if (enable914Configs)
            {
                Dictionary<string, string> configHealth = KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_role_health", "NtfCommander:400,NtfScientist:350"));

                try
                {
                    foreach (KeyValuePair<string, string> kvp in configHealth)
                    {
                        roleHealth.Add((RoleType)Enum.Parse(typeof(RoleType), kvp.Key), int.Parse(kvp.Value));
                        DebugBoi(kvp.Key + "'s default health is now: " + kvp.Value);
                    }
                    Log.Info("Loaded " + configHealth.Keys.Count() + "('s) default health classes.");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to add custom health to roles. Check your 'util_role_health' config values for errors!\n" + e);
                }

                Dictionary<string, string> configRoles =
                    KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_914_roles", ""));
                try
                {
                    foreach (KeyValuePair<string, string> kvp in configRoles)
                        scp914Roles.Add(Scp914PlayerUpgrade.ParseString(kvp.Key),
                            (Scp914Knob)Enum.Parse(typeof(Scp914Knob), kvp.Value));

                    Log.Info("Loaded " + configRoles.Count + "('s) custom 914 upgrade classes.");
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to parse 914 role upgrade settings. {e}");
                }

                

                Dictionary<string, string> configItems = KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_914_items", "Painkillers-Medkit:Fine,Coin-Flashlight:OneToOne"));

                try
                {
                    foreach (KeyValuePair<string, string> kvp in configItems)
                        scp914Items.Add(Scp914ItemUpgrade.ParseString(kvp.Key), (Scp914Knob)Enum.Parse(typeof(Scp914Knob), kvp.Value));

                    Log.Info("Loaded " + configItems.Count + "('s) custom 914 recipes.");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to add items to 914. Check your 'util_914_items' config values for errors!\n" + e);
                }
            }

            bool enableCustomInv = Config.GetBool("util_enable_inventories", false);
            EnableRandomInv = Config.GetBool("util_enable_random_chances", false);

            if (enableCustomInv)
            {
                // Custom items
                try
                {
                    Inventories = new CustomInventory();

                    if (EnableRandomInv)
                    {
                        Dictionary<string, int> classD =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_classd_inventory", null));
                        Dictionary<string, int> chaos =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_chaos_inventory", null));
                        Dictionary<string, int> sci =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_scientist_inventory", null));
                        Dictionary<string, int> cadet =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_ntfcadet_inventory", null));
                        Dictionary<string, int> lt =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_ntflieutenant_inventory", null));
                        Dictionary<string, int> cmd =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_ntfcommander_inventory", null));
                        Dictionary<string, int> guard =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_guard_inventory", null));
                        Dictionary<string, int> ntfSci =
                            KConf.ExiledConfiguration.GetRandomListValue(
                                Config.GetString("util_ntfscientist_inventory", null));

                        Inventories.ClassDRan = CustomInventory.ConvertToRandomItemList(classD);
                        Inventories.ChaosRan = CustomInventory.ConvertToRandomItemList(chaos);
                        Inventories.ScientistRan = CustomInventory.ConvertToRandomItemList(sci);
                        Inventories.NtfCadetRan = CustomInventory.ConvertToRandomItemList(cadet);
                        Inventories.NtfLtRan = CustomInventory.ConvertToRandomItemList(lt);
                        Inventories.NtfCmdRan = CustomInventory.ConvertToRandomItemList(cmd);
                        Inventories.NtfSciRan = CustomInventory.ConvertToRandomItemList(ntfSci);
                        Inventories.GuardRan = CustomInventory.ConvertToRandomItemList(guard);
                    }
                    else
                    {
                        Inventories.ClassD = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(
                                Config.GetString("util_classd_inventory", null)));
                        Inventories.Chaos = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_chaos_inventory",
                                null)));
                        Inventories.NtfCadet = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_ntfcadet_inventory",
                                null)));
                        Inventories.NtfCommander = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_ntfcommander_inventory",
                                null)));
                        Inventories.NtfLieutenant = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(
                                Config.GetString("util_ntflieutenant_inventory", null)));
                        Inventories.NtfScientist = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_ntfscientist_inventory",
                                null)));
                        Inventories.Scientist = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_scientist_inventory",
                                null)));
                        Inventories.Guard = CustomInventory.ConvertToItemList(
                            KConf.ExiledConfiguration.GetListStringValue(Config.GetString("util_guard_inventory",
                                null)));
                    }

                    Log.Info("Loaded Inventories.");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to add items to custom inventories! Check your inventory config values for errors!\n[EXCEPTION] For Developers:\n" + e);
                    return;
                }
            }

            bool upgradeHeldItems = Config.GetBool("util_914_upgrade_hand", true);

            bool enableBroadcasting = Config.GetBool("util_broadcast_enable", false); //nobody wants this true on default -rin
            string broadcastMessage = Config.GetString("util_broadcast_message", "<color=lime>This server is running <b><color=red>EXILED-CommonUtils</color></b>, enjoy playing!</color>");

            int boradcastSeconds = Config.GetInt("util_broadcast_seconds", 300); // 300 is 5 minutes. :D
            int boradcastTime = Config.GetInt("util_broadcast_time", 4);

            bool enableJoinmessage = Config.GetBool("util_joinmessage_enable", true);
            string joinMessage = Config.GetString("util_joinMessage", "<color=lime>Welcome <b>%player%</b>! <i>Please read our rules!</i></color>");
            int joinMessageTime = Config.GetInt("util_joinMessage_time", 6); // 6 seconds duhhhhh

            bool enableAutoNuke = Config.GetBool("util_enable_autonuke", false);

            int autoNukeTime = Config.GetInt("util_autonuke_time", 600); // 600 seconds is 10 minutes.
            bool clearRagdolls = Config.GetBool("util_cleanup_ragdolls", true);
            float clearRagdollTimer = Config.GetFloat("util_cleanup_interval", 250f);
            bool clearOnlyPocket = Config.GetBool("util_cleanup_only_pocket", false);
            bool clearItems = Config.GetBool("util_cleanup_items", true);
            Scp049Healing = Config.GetBool("util_049_healing", true);
            Scp049HealAmount = Config.GetInt("util_049_heal_amount", 25);
            Scp049HealPow = Config.GetFloat("util_049_heal_power", 1.25f);
            Scp0492Healing = Config.GetBool("util_0492_healing", true);
            Scp0492HealAmount = Config.GetInt("util_0492_heal_amount", 25);
            Scp096Healing = Config.GetBool("util_096_healing", true);
            Scp096Heal = Config.GetInt("util_096_heal_amount", 150);
            Scp106Healing = Config.GetBool("util_106_healing", true);
            Scp106HealAmount = Config.GetInt("util_106_heal_amount", 75);
            Scp173Healing = Config.GetBool("util_173_healing", true);
            Scp173HealAmount = Config.GetInt("util_173_heal_amount", 150);
            Scp939Healing = Config.GetBool("util_939_healing", true);
            Scp939Heal = Config.GetInt("util_939_heal_amount", 125);
            

            EventHandler = new EventHandlers(upgradeHeldItems, scp914Roles, scp914Items, roleHealth, broadcastMessage, joinMessage, boradcastTime, boradcastSeconds, joinMessageTime, Inventories, autoNukeTime, enableAutoNuke, enable914Configs, enableJoinmessage, enableBroadcasting, enableCustomInv, clearRagdolls, clearRagdollTimer, clearOnlyPocket, TeslaIgnoredRoles, clearItems)
            { 
                LockAutoNuke = Config.GetBool("util_autonuke_lock", false)
            };
            Events.PlayerJoinEvent += EventHandler.PlayerJoin;
            Events.Scp914UpgradeEvent += EventHandler.SCP914Upgrade;
            Events.RoundStartEvent += EventHandler.RoundStart;
            Events.PlayerSpawnEvent += EventHandler.OnPlayerSpawn;
            Events.RoundEndEvent += EventHandler.OnRoundEnd;
            Events.WaitingForPlayersEvent += EventHandler.OnWaitingForPlayers;
            Events.TriggerTeslaEvent += EventHandler.OnTriggerTesla;
            Events.PlayerDeathEvent += EventHandler.OnPlayerDeath;
            Events.PocketDimDeathEvent += EventHandler.OnPocketDeath;
            Events.Scp096EnrageEvent += EventHandler.OnEnrage;
            Events.Scp096CalmEvent += EventHandler.OnCalm;
            Events.PlayerHurtEvent += EventHandler.OnPlayerHurt;

            Log.Info("Common-Utils Loaded! Created by the EXILED Team.");

            if (!enableBroadcasting)
                return;

            cor = Timing.RunCoroutine(EventHandler.CustomBroadcast());
        }

        public override void OnReload()
        {

        }
    }
}

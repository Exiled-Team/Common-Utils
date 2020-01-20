using EXILED;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEC;

namespace Common_Utils
{
    public class Plugin : EXILED.Plugin
    {
        public override string getName => "Common-Utils";

        // Item upgrade class :D

        public partial class Scp914ItemUpgrade
        {
            public ItemType ToUpgrade { get; set; }
            public ItemType UpgradedTo { get; set; }

            public static Scp914ItemUpgrade ParseString(string config)
            {
                string[] splitted = config.Split('-');
                return new Scp914ItemUpgrade() { ToUpgrade = (ItemType)Enum.Parse(typeof(ItemType), splitted[0]), UpgradedTo = (ItemType)Enum.Parse(typeof(ItemType), splitted[1]) };
            }
        }

        // Iven tory. lol

        public partial class CustomInventory
        {
            public List<ItemType> NtfCadet;

            public List<ItemType> NtfLieutenant;

            public List<ItemType> NtfCommander;

            public List<ItemType> ClassD;

            public List<ItemType> Scientist;

            public List<ItemType> NtfScientist;

            public List<ItemType> Chaos;

            public List<ItemType> Guard;

            public static List<ItemType> ConvertToItemList(List<string> list)
            {
                if (list.Count == 0)
                    return null;
                List<ItemType> listd = new List<ItemType>();
                foreach(string s in list)
                {
                    listd.Add((ItemType)Enum.Parse(typeof(ItemType), s));
                }
                return listd;
            }

        }




        public CoroutineHandle cor;

        // Config settings.

        public CustomInventory Inventories = new CustomInventory();

        public Dictionary<RoleType, int> roleHealth = new Dictionary<RoleType, int>();

        public Dictionary<RoleType, RoleType> scp914Roles = new Dictionary<RoleType, RoleType>();

        public Dictionary<Scp914ItemUpgrade, Scp914.Scp914Knob> scp914Items = new Dictionary<Scp914ItemUpgrade, Scp914.Scp914Knob>();

        public EventHandlers EventHandler;

        public override void OnDisable()
        {
            Events.Scp914UpgradeEvent -= EventHandler.SCP914Upgrade;
            Events.PlayerJoinEvent -= EventHandler.PlayerJoin;
            Events.SetClassEvent -= EventHandler.SetClass;

            Timing.KillCoroutines(cor);

            Inventories = null;
            roleHealth.Clear();
            scp914Items.Clear();
            scp914Roles.Clear();

            scp914Items = null;
            scp914Roles = null;
            EventHandler = null;
        }

        public override void OnEnable()
        {
            if (!Config.GetBool("util_enable", true))
                return;

            Dictionary<string, string> configHealth = KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_role_health", "NtfCommander:400,NtfScientist:350"));

            try
            {
                foreach (KeyValuePair<string, string> kvp in configHealth)
                {
                    roleHealth.Add((RoleType)Enum.Parse(typeof(RoleType), kvp.Key), int.Parse(kvp.Value));
                }
            }
            catch (Exception e)
            {
                Error("Failed to add custom health to roles. Check your 'util_role_health' config values for errors!\n" + e);
                return;
            }

            Dictionary<string, string> configRoles = KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_914_roles", "ClassD:Scientist,NtfCadet:NtfLieutenant,NtfLieutenant:NtfScientist,NtfScientist:NtfCommander"));

            try
            {
                foreach (KeyValuePair<string, string> kvp in configRoles)
                {
                    scp914Roles.Add((RoleType)Enum.Parse(typeof(RoleType), kvp.Key), (RoleType)Enum.Parse(typeof(RoleType), kvp.Value));
                }
            }
            catch (Exception e)
            {
                Error("Failed to add roles. Check your 'util_914_roles' config values for errors!\n" + e);
                return;
            }

            Dictionary<string, string> configItems = KConf.ExiledConfiguration.GetDictonaryValue(Config.GetString("util_914_items", "Painkillers-Medkit:Fine,Coin-Flashlight:OneToOne"));

            try
            {
                foreach (KeyValuePair<string, string> kvp in configItems)
                {
                    scp914Items.Add(Scp914ItemUpgrade.ParseString(kvp.Key), (Scp914.Scp914Knob) Enum.Parse(typeof(Scp914.Scp914Knob), kvp.Value));
                }
            }
            catch (Exception e)
            {
                Error("Failed to add items. Check your 'util_914_items' config values for errors!\n" + e);
                return;
            }

            // Custom items

            Inventories = new CustomInventory();
            Inventories.ClassD = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_classd_inventory"));
            Inventories.Chaos = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_choas_inventory"));
            Inventories.NtfCadet = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_ntfcadet_inventory"));
            Inventories.NtfCommander = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_ntfcommander_inventory"));
            Inventories.NtfLieutenant = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_ntflieutenant_inventory"));
            Inventories.NtfScientist = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_ntfscientist_inventory"));
            Inventories.Scientist = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_scientist_inventory"));
            Inventories.Guard = CustomInventory.ConvertToItemList(Plugin.Config.GetStringList("util_guard_inventory"));

            string broadcastMessage = Config.GetString("util_broadcast_message", "<color=lime>This server is running <color=red>EXILED-CommonUtils</color>, enjoy playing!</color>");

            int boradcastSeconds = Config.GetInt("util_broadcast_seconds", 300); // 300 is 5 minutes. :D
            int boradcastTime = Config.GetInt("util_broadcast_time", 4);

            string joinMessage = Config.GetString("util_joinMessage", "<color=lime>Welcome %player%! Please read our rules!</color>");
            int joinMessageTime = Config.GetInt("util_joinMessage_time", 6);    

            EventHandler = new EventHandlers(scp914Roles,scp914Items, roleHealth, broadcastMessage, joinMessage, boradcastTime, boradcastSeconds, joinMessageTime, Inventories);
            Events.PlayerJoinEvent += EventHandler.PlayerJoin;
            Events.Scp914UpgradeEvent += EventHandler.SCP914Upgrade;
            Events.SetClassEvent += EventHandler.SetClass;

            cor = Timing.RunCoroutine(EventHandler.CustomBroadcast());

            Info("Common-Utils Loaded! Created by KadeDev.");
        }

        public override void OnReload()
        {
            
        }
    }
}

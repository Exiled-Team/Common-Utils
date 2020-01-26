using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using MEC;
using Scp914;
using UnityEngine;
using static Common_Utils.Plugin;

namespace Common_Utils
{
    public class EventHandlers
    {

        // e
        public string BMessage;
        public string JMessage;

        public int BTime;
        public int BSeconds;
        public int JTime;
        public int ANTime;

        public Dictionary<RoleType, int> RolesHealth;
        public Dictionary<Scp914PlayerUpgrade, Scp914Knob> Roles;
        public Dictionary<Scp914ItemUpgrade, Scp914Knob> Items;

        public CustomInventory Inventories;

        public bool EnableBroadcasting;
        public bool EnableAutoNuke;
        public bool Enable914;
        public bool EnableInventories;
        public bool UpgradeHand;

        // T H I C K constructor
        public EventHandlers(bool uh, Dictionary<Scp914PlayerUpgrade, Scp914Knob> roles, Dictionary<Scp914ItemUpgrade, Scp914Knob> items, Dictionary<RoleType, int> health, string bm, string jm, int bt, int bs, int jt, CustomInventory inven, int nukeTime, bool autoNuke, bool enable914, bool enableBroadcasting, bool enableInventories)
        {
            Roles = roles;
            Items = items;
            BMessage = bm;
            JMessage = jm;
            BTime = bt;
            BSeconds = bs;
            JTime = jt;
            RolesHealth = health;
            Inventories = inven;
            UpgradeHand = uh;
            ANTime = nukeTime;
            EnableBroadcasting = enableBroadcasting;
            EnableAutoNuke = autoNuke;
            Enable914 = enable914;
            EnableInventories = enableInventories;

            // Auto nuke

        }

        IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(ANTime);

            AlphaWarheadController.Host.StartDetonation();
        }

        internal void SCP914Upgrade(ref SCP914UpgradeEvent ev)
        {
            if (!Enable914)
                return;

            Vector3 tpPos = ev.Machine.output.position - ev.Machine.intake.position;

            if (UpgradeHand) // Upgrade hand items like a boss
                foreach (ReferenceHub hub in ev.Players)
                    ev.Machine.UpgradeHeldItem(hub.inventory, hub.characterClassManager, ev.Machine.players); // use default game functions yeehaw

            foreach (KeyValuePair<Scp914ItemUpgrade, Scp914Knob> kvp in Items)
            {
                if (ev.KnobSetting != kvp.Value)
                    continue;
                foreach (Pickup item in ev.Items)
                {
                    if (kvp.Key.ToUpgrade == item.ItemId)
                    {
                        SpawnItem(kvp.Key.UpgradedTo, item.transform.position + tpPos, Vector3.zero);
                        item.Delete();
                    }
                    else
                        ev.Machine.UpgradeItem(item);
                }
            }

            if (ev.Items.Count > 1)
                return;

            foreach (KeyValuePair<Scp914PlayerUpgrade, Scp914Knob> kv in Roles)
            {
                if (ev.KnobSetting != kv.Value)
                    continue;
                foreach (ReferenceHub hub in ev.Players)
                    if (kv.Key.ToUpgrade == hub.characterClassManager.CurClass)
                    {
                        Vector3 oldPos = hub.transform.position;
                        hub.characterClassManager.SetPlayersClass(kv.Key.UpgradedTo, hub.gameObject);
                        Timing.RunCoroutine(TeleportToOutput(hub, oldPos, tpPos, hub.inventory));
                    }
            }

        }

        private IEnumerator<float> TeleportToOutput(ReferenceHub hub, Vector3 oldPos, Vector3 tpPos, Inventory inv)
        {
            yield return Timing.WaitForSeconds(1.1f);

            DebugBoi("Teleporting " + hub.nicknameSync.MyNick + " to the output of 914.");

            hub.plyMovementSync.OverridePosition(oldPos + tpPos, hub.gameObject.transform.rotation.y);
            hub.inventory.Clear();
            hub.inventory = inv;
        }

        public void SpawnItem(ItemType type, Vector3 pos, Vector3 rot)
        {
            PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(type, -4.656647E+11f, pos, Quaternion.Euler(rot), 0, 0, 0);
        }

        public IEnumerator<float> CustomBroadcast()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(BSeconds);
                foreach (ReferenceHub hub in Plugin.GetHubs())
                {
                    Extenstions.Broadcast(hub, (uint)BTime, BMessage);
                }
            }
        }

        internal void PlayerJoin(PlayerJoinEvent ev)
        {
            if (!EnableBroadcasting)
                return;

            Extenstions.Broadcast(ev.Player, (uint)JTime, JMessage.Replace("%player%", ev.Player.nicknameSync.MyNick));
        }

        internal void SetClass(SetClassEvent ev)
        {
            DebugBoi("Waiting to spawn in...");

            if (!EnableInventories)
                return;

            Timing.RunCoroutine(frick(ev.Player, ev.Role));
        }

        IEnumerator<float> frick(ReferenceHub p, RoleType role)
        {
            yield return Timing.WaitForSeconds(1f);
            // Bloat code :D

            if (RolesHealth.ContainsKey(role))
            {
                p.playerStats.health = RolesHealth[role];
                p.playerStats.maxHP = RolesHealth[role];
            }

            DebugBoi("Giving items for Custom Inventories.");
            switch (role)
            {
                case RoleType.ClassD:
                    if (Inventories.ClassD != null)
                    {
                        DebugBoi("Passed CD");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.ClassD)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.ChaosInsurgency:
                    if (Inventories.Chaos != null)
                    {
                        DebugBoi("Passed CS");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Chaos)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.Scientist:
                    if (Inventories.Scientist != null)
                    {
                        DebugBoi("Passed SC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Scientist)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfScientist:
                    if (Inventories.NtfScientist != null)
                    {
                        DebugBoi("Passed NS");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfScientist)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfLieutenant:
                    if (Inventories.NtfLieutenant != null)
                    {
                        DebugBoi("Passed NL");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfLieutenant)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfCommander:
                    if (Inventories.NtfCommander != null)
                    {
                        DebugBoi("Passed NCC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfCommander)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.FacilityGuard:
                    if (Inventories.Guard != null)
                    {
                        DebugBoi("Passed GD");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Guard)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfCadet:
                    if (Inventories.NtfCadet != null)
                    {
                        DebugBoi("Passed NC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfCadet)
                            p.inventory.AddNewItem(item);
                    }
                    break;
            }
        }
    }
}

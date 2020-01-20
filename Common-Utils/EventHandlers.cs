using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using MEC;
using UnityEngine;
using static Common_Utils.Plugin;

namespace Common_Utils
{
    public class EventHandlers
    {

        // e
        public string B_Message;
        public string J_Message;

        public int B_Time;
        public int B_Seconds;

        public int J_Time;

        public Dictionary<RoleType, int> RolesHealth = new Dictionary<RoleType, int>();

        public Dictionary<RoleType, RoleType> Roles = new Dictionary<RoleType, RoleType>();

        public CustomInventory Inventories;

        public Dictionary<Scp914ItemUpgrade, Scp914.Scp914Knob> Items = new Dictionary<Scp914ItemUpgrade, Scp914.Scp914Knob>();

        public EventHandlers(Dictionary<RoleType, RoleType> roles, Dictionary<Scp914ItemUpgrade, Scp914.Scp914Knob> items, Dictionary<RoleType, int> health, string bm, string jm, int bt, int bs, int jt, CustomInventory inven)
        {
            Roles = roles;
            Items = items;
            B_Message = bm;
            J_Message = jm;
            B_Time = bt;
            B_Seconds = bs;
            J_Time = jt;
            RolesHealth = health;
            Inventories = inven;
        }


        internal void SCP914Upgrade(ref SCP914UpgradeEvent ev)
        {
            Vector3 tpPos = ev.Machine.output.position - ev.Machine.intake.position;
            if (ev.KnobSetting == Scp914.Scp914Knob.Fine || ev.KnobSetting == Scp914.Scp914Knob.VeryFine)
            {
                foreach (ReferenceHub p in ev.Players)
                {
                    if (Roles.ContainsKey(p.characterClassManager.CurClass))
                    {
                        Vector3 pos = p.characterClassManager.transform.position;
                        p.characterClassManager.SetClassID(Roles[p.characterClassManager.CurClass]);
                        Timing.RunCoroutine(telPlayer(p,pos));
                    }
                }
            }

            foreach(KeyValuePair<Scp914ItemUpgrade, Scp914.Scp914Knob> kvp in Items)
            {
                if (ev.KnobSetting != kvp.Value)
                    continue;
                foreach(Pickup item in ev.Items)
                {
                    if (kvp.Key.ToUpgrade == item.ItemId)
                    {
                        SpawnItem(kvp.Key.UpgradedTo, item.transform.position + tpPos, item.transform.position);
                        item.Delete();
                    }
                }
            }
        }

        public void SpawnItem(ItemType type, Vector3 pos, Vector3 rot)
        {
            PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(type, -4.656647E+11f, pos, Quaternion.Euler(rot), 0, 0, 0);
        }

        public IEnumerator<float> CustomBroadcast()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(B_Seconds);
                foreach (ReferenceHub hub in Plugin.GetHubs())
                {
                    Extenstions.Broadcast(hub, (uint) B_Time, B_Message);
                }
            }   
        }

        internal void PlayerJoin(PlayerJoinEvent ev) => Extenstions.Broadcast(ev.Player, (uint) J_Time, J_Message.Replace("%player%", ev.Player.nicknameSync.MyNick));

        internal void SetClass(SetClassEvent ev)
        {
            if (RolesHealth.ContainsKey(ev.Role))
            {
                ev.Player.playerStats.health = RolesHealth[ev.Role];
                ev.Player.playerStats.maxHP = RolesHealth[ev.Role];
            }

            Plugin.DebugBoi("Waiting to spawn in...");

            Timing.RunCoroutine(frick(ev.Player, ev.Role));
        }


        IEnumerator<float> telPlayer(ReferenceHub p, Vector3 pos)
        {
            yield return Timing.WaitForSeconds(0.4f);
            p.GetComponent<PlyMovementSync>().OverridePosition(pos, 0f, false);
        }

        IEnumerator<float> frick(ReferenceHub p, RoleType role)
        {
            yield return Timing.WaitForSeconds(1f);
            // Bloat code :D

            Plugin.DebugBoi("Giving items for Custom Inventories.");
            switch (role)
            {
                case RoleType.ClassD:
                    if (Inventories.ClassD != null)
                    {
                        Plugin.DebugBoi("Passed CD");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.ClassD)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.ChaosInsurgency:
                    if (Inventories.Chaos != null)
                    {
                        Plugin.DebugBoi("Passed CS");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Chaos)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.Scientist:
                    if (Inventories.Scientist != null)
                    {
                        Plugin.DebugBoi("Passed SC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Scientist)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfScientist:
                    if (Inventories.NtfScientist != null)
                    {
                        Plugin.DebugBoi("Passed NS");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfScientist)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfLieutenant:
                    if (Inventories.NtfLieutenant != null)
                    {
                        Plugin.DebugBoi("Passed NL");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfLieutenant)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfCommander:
                    if (Inventories.NtfCommander != null)
                    {
                        Plugin.DebugBoi("Passed NCC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfCommander)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.FacilityGuard:
                    if (Inventories.Guard != null)
                    {
                        Plugin.DebugBoi("Passed GD");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.Guard)
                            p.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfCadet:
                    if (Inventories.NtfCadet != null)
                    {
                        Plugin.DebugBoi("Passed NC");
                        p.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfCadet)
                            p.inventory.AddNewItem(item);
                    }
                    break;
            }
        }
    }
}

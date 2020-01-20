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
                        Timing.RunCoroutine(frick());
                        p.GetComponent<PlyMovementSync>().OverridePosition(pos, 0f, false);
                    }
                }
            }

            List<Pickup> tpItems = new List<Pickup>();

            foreach(KeyValuePair<Scp914ItemUpgrade, Scp914.Scp914Knob> kvp in Items)
            {
                if (ev.KnobSetting != kvp.Value)
                    continue;
                foreach(Pickup item in ev.Items)
                {
                    if (kvp.Key.ToUpgrade == item.ItemId)
                    {
                        item.ItemId = kvp.Key.UpgradedTo;
                        tpItems.Add(item);
                    }
                }
            }

            foreach (Pickup p in tpItems)
            {
                ev.Items.Remove(p);
                p.transform.position = p.transform.position + tpPos;
            }

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

        internal void PlayerJoin(PlayerJoinEvent ev) => Extenstions.Broadcast(ev.Player, (uint) J_Time, J_Message.Replace("%player%", ev.Player.name));

        internal void SetClass(SetClassEvent ev)
        {
            if (RolesHealth.ContainsKey(ev.Role))
            {
                ev.Player.playerStats.health = RolesHealth[ev.Role];
                ev.Player.playerStats.maxHP = RolesHealth[ev.Role];
            }

            Timing.RunCoroutine(frick());

            // Bloat code :D

            switch(ev.Role)
            {
                case RoleType.ClassD:
                    if (Inventories.ClassD != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.ClassD)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.ChaosInsurgency:
                    if (Inventories.Chaos != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.Chaos)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.Scientist:
                    if (Inventories.Scientist != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.Scientist)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfScientist:
                    if (Inventories.NtfScientist != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfScientist)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfLieutenant:
                    if (Inventories.NtfLieutenant != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfLieutenant)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.NtfCommander:
                    if (Inventories.NtfCommander != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.NtfCommander)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
                case RoleType.FacilityGuard:
                    if (Inventories.Guard != null)
                    {
                        ev.Player.inventory.Clear();
                        foreach (ItemType item in Inventories.Guard)
                            ev.Player.inventory.AddNewItem(item);
                    }
                    break;
            }
        }


        IEnumerator<float> frick()
        {
            yield return Timing.WaitForOneFrame;
        }
    }
}

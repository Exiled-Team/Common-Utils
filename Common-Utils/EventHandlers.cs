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

        public bool LockAutoNuke;

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
            Timing.RunCoroutine(AutoNuke());
        }

        IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(ANTime);

            Patches.AutoWarheadLockPatches.AutoLocked = LockAutoNuke;
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

        internal void RoundStart()
        {
            Patches.AutoWarheadLockPatches.AutoLocked = false;
        }
    }
}

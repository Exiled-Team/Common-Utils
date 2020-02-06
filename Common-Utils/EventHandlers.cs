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
using Object = System.Object;

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

        public bool ClearRagdolls;
        public float ClearRagInterval;
        public bool ClearOnlyPocket;
        public bool ClearItems;

        // T H I C K constructor
        public EventHandlers(bool uh, Dictionary<Scp914PlayerUpgrade, Scp914Knob> roles, Dictionary<Scp914ItemUpgrade, Scp914Knob> items, Dictionary<RoleType, int> health, string bm, string jm, int bt, int bs, int jt, CustomInventory inven, int nukeTime, bool autoNuke, bool enable914, bool enableBroadcasting, bool enableInventories, bool clearRag, float clearInt, bool clearItems, bool clearOnlyPocket = false)
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
            ClearRagdolls = clearRag;
            ClearRagInterval = clearInt;
            ClearOnlyPocket = clearOnlyPocket;
            ClearItems = clearItems;
        }

        IEnumerator<float> AutoNuke()
        {
            yield return Timing.WaitForSeconds(ANTime);

            Patches.AutoWarheadLockPatches.AutoLocked = LockAutoNuke;
            AlphaWarheadController.Host.StartDetonation();
        }

        internal void SCP914Upgrade(ref SCP914UpgradeEvent ev)
        {
            try
            {
                if (!Enable914)
                    return;

                Vector3 tpPos = ev.Machine.output.position - ev.Machine.intake.position;

                Dictionary<ItemType, ItemType> upgradeItems = new Dictionary<ItemType, ItemType>();
                foreach (KeyValuePair<Scp914ItemUpgrade, Scp914Knob> ikvp in Items)
                {
                    if (ev.KnobSetting != ikvp.Value)
                        continue;
                    upgradeItems.Add(ikvp.Key.ToUpgrade, ikvp.Key.UpgradedTo);
                }
                
                if (UpgradeHand)
                    foreach (ReferenceHub hub in ev.Players)
                    {
                        if (upgradeItems.ContainsKey(hub.inventory.NetworkcurItem))
                        {
                            hub.inventory.NetworkcurItem = upgradeItems[hub.inventory.NetworkcurItem];
                        }
                        else
                            ev.Machine.UpgradeHeldItem(hub.inventory, hub.characterClassManager, ev.Machine.players);
                    }

                foreach (Pickup item in ev.Items)
                {
                    if (upgradeItems.ContainsKey(item.ItemId))
                    {
                        Vector3 pos = item.gameObject.transform.position + tpPos;
                        SpawnItem(upgradeItems[item.ItemId], pos, Vector3.zero);
                        item.Delete();
                    }
                    else
                        ev.Machine.UpgradeItem(item);
                }

                Dictionary<RoleType, RoleType> upgrades = new Dictionary<RoleType, RoleType>();
                foreach (KeyValuePair<Scp914PlayerUpgrade, Scp914Knob> kvp in Roles)
                {
                    if (ev.KnobSetting != kvp.Value)
                        continue;
                    upgrades.Add(kvp.Key.ToUpgrade, kvp.Key.UpgradedTo);
                }
                foreach (ReferenceHub player in ev.Players)
                    if (upgrades.ContainsKey(player.characterClassManager.CurClass))
                    {
                        Vector3 oldPos = player.gameObject.transform.position;
                        player.characterClassManager.NetworkCurClass = upgrades[player.characterClassManager.CurClass];
                        Timing.RunCoroutine(TeleportToOutput(player, oldPos, tpPos, player.inventory));
                    }
            }
            catch (Exception e)
            {
                Plugin.Error(e.ToString());
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
            Coroutines.Add(Timing.RunCoroutine(AutoNuke()));
            if (ClearRagdolls)
                Coroutines.Add(Timing.RunCoroutine(CleanupRagdolls()));
            if (ClearItems)
                Coroutines.Add(Timing.RunCoroutine(CleanupItems()));
        }

        private IEnumerator<float> CleanupItems()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(ClearRagInterval);
                foreach (Pickup item in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    if (ClearOnlyPocket)
                    {
                        if (item.model.transform.position.y < -1000f)
                            UnityEngine.Object.Destroy(item.model);
                    }
                    else
                        UnityEngine.Object.Destroy(item.model);
                }
            }
        }

        private IEnumerator<float> CleanupRagdolls()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(ClearRagInterval);
                foreach (Ragdoll ragdoll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
                {
                    if (ClearOnlyPocket)
                    {
                        if (ragdoll.gameObject.transform.position.y < -1000f)
                            UnityEngine.Object.Destroy(ragdoll.gameObject);
                    }
                    else
                        UnityEngine.Object.Destroy(ragdoll.gameObject);
                }
            }
        }

        public void OnRoundEnd()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
        }

        public void OnWaitingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
        }
    }
}

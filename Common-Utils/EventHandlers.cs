using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;
using MEC;
using Scp914;
using UnityEngine;
using static Common_Utils.Plugin;
using Object = UnityEngine.Object;

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
        public bool EnableJoinmessage;
        public bool EnableAutoNuke;
        public bool Enable914;
        public bool EnableInventories;
        public bool UpgradeHand;

        public bool LockAutoNuke;

        public bool ClearRagdolls;
        public float ClearRagInterval;
        public bool ClearOnlyPocket;
        public bool ClearItems;
        public List<RoleType> TeslaIgnoredRoles = new List<RoleType>();

        // T H I C K constructor
        public EventHandlers(bool uh, Dictionary<Scp914PlayerUpgrade, Scp914Knob> roles, Dictionary<Scp914ItemUpgrade, Scp914Knob> items, Dictionary<RoleType, int> health, string bm, string jm, int bt, int bs, int jt, CustomInventory inven, int nukeTime, bool autoNuke, bool enable914, bool enableJoinmessage, bool enableBroadcasting, bool enableInventories, bool clearRag, float clearInt, bool clearItems, List<RoleType> TeslaIgnoredRoles, bool clearOnlyPocket = false)
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
            EnableJoinmessage = enableJoinmessage;
            EnableBroadcasting = enableBroadcasting;
            EnableAutoNuke = autoNuke;
            Enable914 = enable914;
            EnableInventories = enableInventories;
            ClearRagdolls = clearRag;
            ClearRagInterval = clearInt;
            ClearOnlyPocket = clearOnlyPocket;
            ClearItems = clearItems;
            this.TeslaIgnoredRoles = TeslaIgnoredRoles;
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

                foreach (Pickup item in ev.Items.ToList())
                {
                    if (upgradeItems.ContainsKey(item.ItemId))
                    {
                        Vector3 pos = item.gameObject.transform.position + tpPos;
                        SpawnItem(upgradeItems[item.ItemId], pos, Vector3.zero);
                        item.Delete();
                        ev.Items.Remove(item);
                    }
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
                        Inventory inv = player.inventory;
                        Vector3 oldPos = player.gameObject.transform.position;
                        player.characterClassManager.NetworkCurClass = upgrades[player.characterClassManager.CurClass];
                        Timing.RunCoroutine(TeleportToOutput(player, oldPos, tpPos, inv));
                    }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
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
                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    hub.Broadcast((uint)BTime, BMessage);
                }
            }
        }

        internal void PlayerJoin(PlayerJoinEvent ev)
        {
            if (!EnableJoinmessage)
                return;

            ev.Player.Broadcast((uint)JTime, JMessage.Replace("%player%", ev.Player.nicknameSync.MyNick));
        }

        internal void RoundStart()
        {
            Patches.AutoWarheadLockPatches.AutoLocked = false;
            if (EnableAutoNuke)
                Coroutines.Add(Timing.RunCoroutine(AutoNuke()));
            if (ClearRagdolls)
                Coroutines.Add(Timing.RunCoroutine(CleanupRagdolls()));
            if (ClearItems)
                Coroutines.Add(Timing.RunCoroutine(CleanupItems()));
            if (Instance.Scp049Healing || Instance.Scp0492Healing)
                Coroutines.Add(Timing.RunCoroutine(DoScp049Heal()));
        }

        private IEnumerator<float> DoScp049Heal()
        {
            for (;;)
            {
                foreach (ReferenceHub hub in Player.GetHubs())
                    if (hub.GetRole() == RoleType.Scp049)
                    {
                        int counter = 0;
                        foreach (ReferenceHub rh in Player.GetHubs())
                        {
                            if (rh.GetRole() != RoleType.Scp0492)
                                continue;
                            if (Vector3.Distance(rh.GetPosition(), hub.GetPosition()) < 10f)
                                counter++;
                            if (Instance.Scp0492Healing && (rh.GetHealth() + Instance.Scp0492HealAmount) <= rh.playerStats.maxHP)
                                rh.SetHealth(rh.GetHealth() + Instance.Scp0492HealAmount);
                        }

                        float healing = (float)Math.Pow(counter, Instance.Scp049HealPow) * Instance.Scp0492HealAmount;
                        if (Instance.Scp049Healing && (hub.GetHealth() + healing) <= hub.playerStats.maxHP)
                            hub.SetHealth(hub.GetHealth() + healing);
                    }

                yield return Timing.WaitForSeconds(5f);
            }
        }

        private IEnumerator<float> CleanupItems()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(ClearRagInterval);
                foreach (Pickup item in Object.FindObjectsOfType<Pickup>())
                {
                    if (ClearOnlyPocket)
                    {
                        if (item.gameObject.transform.position.y < -1900f)
                            item.Delete();
                    }
                    else
                        item.Delete();
                }
            }
        }

        private IEnumerator<float> CleanupRagdolls()
        {
            for (;;)
            {
                yield return Timing.WaitForSeconds(ClearRagInterval);
                foreach (Ragdoll ragdoll in Object.FindObjectsOfType<Ragdoll>())
                {
                    if (ClearOnlyPocket)
                    {
                        if (ragdoll.gameObject.transform.position.y < -1000f)
                            Object.Destroy(ragdoll.gameObject);
                    }
                    else
                        Object.Destroy(ragdoll.gameObject);
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

        public void OnTriggerTesla(ref TriggerTeslaEvent ev)
        {
            if (TeslaIgnoredRoles.Contains(ev.Player.characterClassManager.CurClass))
                ev.Triggerable = false;
        }

        public void OnPlayerSpawn(PlayerSpawnEvent ev)
        {
            if (RolesHealth.ContainsKey(ev.Role))
                Timing.CallDelayed(0.5f, () =>
                {
                    ev.Player.playerStats.maxHP = RolesHealth[ev.Role];
                    ev.Player.playerStats.health = RolesHealth[ev.Role];
                });
        }

        public void OnPlayerDeath(ref PlayerDeathEvent ev)
        {
            if (ev.Killer.GetRole() == RoleType.Scp096)
                Scp096KillCount++;

            if (ev.Killer.GetRole().Is939() && Instance.Scp939Healing && Instance.Gen.Next(1, 100) > 50)
                Coroutines.Add(Timing.RunCoroutine(HealOverTime(ev.Killer, Instance.Scp939Heal), ev.Killer.GetUserId()));
            
            if (ev.Killer.GetRole() == RoleType.Scp173 && Instance.Scp173Healing)
                Coroutines.Add(Timing.RunCoroutine(HealOverTime(ev.Killer, Instance.Scp173HealAmount, 5f), ev.Killer.GetUserId()));
        }

        public void OnPocketDeath(PocketDimDeathEvent ev)
        {
            if (Instance.Scp106Healing)
            {
                ReferenceHub scp = Player.GetHubs().FirstOrDefault(r => r.GetRole() == RoleType.Scp106);
                Coroutines.Add(Timing.RunCoroutine(HealOverTime(scp, Instance.Scp106HealAmount), scp.GetUserId()));
            }
        }

        public int Scp096KillCount;
        public void OnEnrage(ref Scp096EnrageEvent ev)
        {
            Scp096KillCount = 0;
        }

        public void OnCalm(ref Scp096CalmEvent ev)
        {
            if (Instance.Scp096Healing)
                Coroutines.Add(Timing.RunCoroutine(HealOverTime(ev.Player, Instance.Scp096Heal * Scp096KillCount), ev.Player.GetUserId()));
        }

        public IEnumerator<float> HealOverTime(ReferenceHub hub, int amount, float duration = 10f)
        {
            float amountPerTick = amount / duration;

            for (int i = 0; i < duration; i++)
            {
                if ((hub.GetHealth() + amountPerTick) > hub.playerStats.maxHP)
                    break;
                hub.SetHealth(hub.GetHealth() + amountPerTick);
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public void OnPlayerHurt(ref PlayerHurtEvent ev)
        {
            Timing.KillCoroutines(ev.Player.GetUserId());
        }
    }
}

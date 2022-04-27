# Common-Utils
Common Utils is a plugin that serves many common utilites in a day to day server life.

# Main Features
## 914 Features
- Ability to change 914's class upgrading (ex, DClass goes in; scientist comes out.)
- Ability to add custom 914 recipies.
## Server Broadcast/Welcoming Features
- Ability to completly configure a welcome message.
- Ability to completly configure a broadcast message, this can appear every 'x' amount seconds.
## Custom Inventories
- Ability to add custom inventories to all the main classes.

(These are depricated, if you are using the Exiled 2.0 version of the plugin)
# Default config:
```yaml
CommonUtilities:
# Whether or not debug messages should be shown.
  debug: false
  # The SCP Roles able to use V to talk to humans.
  scp_speech:
  - Scp049
  # Whether or not MTF/CI can 'escape' while disarmed to switch teams.
  disarm_switch_teams: true
  # Whether or not disarmed people will be prevented from interacting with doors/elevators.
  restrictive_disarming: true
  # The text displayed at the timed interval specified below.
  timed_broadcast: <color=lime>This server is running </color><color=red>EXILED Common-Utilities</color><color=lime>, enjoy your stay!</color>
  # The time each timed broadcast will be displayed.
  timed_broadcast_duration: 5
  # The delay between each timed broadcast. To disable timed broadcasts, set this to 0
  timed_broadcast_delay: 300
  # The message displayed to the player when they first join the server. Setting this to empty will disable these broadcasts.
  join_message: <color=lime>Welcome %player%! Please read our rules!</color>
  # The amount of time (in seconds) the join message is displayed.
  join_message_duration: 5
  # The amount of time (in seconds) after the round starts, before the facilities auto-nuke will start.
  autonuke_time: 1500
  # Wether or not the nuke should be unable to be disabled during the auto-nuke countdown.
  autonuke_lock: true
  # Whether or not to show player's health under their name when you look at them.
  player_health_info: true
  # Whether or not friendly fire should automatically turn on when a round ends (it will turn itself back off before the next round starts).
  friendly_fire_on_round_end: false
  # The list of starting items for roles. ItemName is the item to give them, and Chance is the percent chance of them spawning with it, and Group allows you to restrict the item to only players with certain RA groups (Leave this as 'none' to allow all players to get the item). You can specify the same item multiple times.
  starting_inventories:
    ClassD:
      slot1:
      - item_name: KeycardJanitor
        chance: 10
        group: none
      - item_name: Coin
        chance: 100
        group: none
      slot2:
      - item_name: Flashlight
        chance: 100
        group: none
      slot3: []
      slot4: []
      slot5: []
      slot6: []
      slot7: []
      slot8: []
      ammo:
      - type: Ammo556x45
        amount: 200
        group: none
  # The list of custom 914 recipies. Original is the item being upgraded, New is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item.
  scp914_item_changes:
    Rough:
    - original: KeycardO5
      new: MicroHID
      chance: 50
  # The list of custom 914 recipies for roles. Original is the role to be changed, New is the new role to assign, Chance is the % chance of the upgrade occuring.
  scp914_class_changes:
    Rough:
    - original: ClassD
      new: Spectator
      chance: 100
      keep_inventory: true
  scp914_teleport_chances:
    Rough:
    - room: LczClassDSpawn
      offset:
        x: 0
        y: 0
        z: 0
      chance: 100
      damage: 0
  # A dictionary of random effects to apply to players when going through 914 on certain settings.
  scp914_effect_chances:
    Rough:
    - effect: Amnesia
      chance: 100
      duration: 0
  # Determines if 914 effects are exclusive, meaning only one can be applied each time a player is processed by 914.
  scp914_effects_exclusivity: false
  # Whether or not SCPs are immune to effects gained from 914.
  scps_immune_to914_effects: false
  # The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.
  ragdoll_cleanup_delay: 0
  # If ragdoll cleanup should only happen in the Pocket Dimension or not.
  ragdoll_cleanup_only_pocket: false
  # The frequency (in seconds) between item cleanups. Set to 0 to disable.
  item_cleanup_delay: 0
  # If item cleanup should only happen in the Pocket Dimension or not.
  item_cleanup_only_pocket: false
  # A list of all roles and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.
  role_damage_multipliers:
    Scp173: 1
  # A list of all Weapons and their damage modifiers. The number here is a multiplier, not a raw damage amount. Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.
  damage_multipliers:
    E11Sr: 1
  # A list of roles and how much health they should be given when they kill someone.
  health_on_kill:
    Scp173: 0
    Scp93953: 10
    Scp93989: 20
  # A list of roles and what their default starting health should be.
  health_values:
    Scp173: 3200
    NtfCaptain: 150
  # If the plugin is enabled or not.
  is_enabled: true

```

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
# Wether or not debug messages should be shown.
  debug: false
  # Wether or not SCP-049 should be able to talk to humans.
  scp049_speech: true
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
  autonuke_time: 1200
  # Wether or not the nuke should be unable to be disabled during the auto-nuke countdown.
  autonuke_lock: true
  # The list of items Class-D should have. Valid formatting should be ItemType:Chance where ItemType is the item to give them, and Chance is the percent chance of them spawning with it. You can speci$
  class_d_inventory:
    slot1:
    - Coin:100
    slot2:
    - Flashlight:100
    slot3:
    - KeycardJanitor:5
    slot4:
    - Medkit:1
    - Painkillers:10
    slot5:
    slot6: []
    slot7: []
    slot8: []
  chaos_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  scientist_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  guard_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  cadet_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  lieutenant_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  commander_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  ntf_sci_inventory:
    slot1: []
    slot2: []
    slot3: []
    slot4: []
    slot5: []
    slot6: []
    slot7: []
    slot8: []
  # The list of custom 914 recipies for the Rough setting. Valid formatting should be OriginalItemType:NewItemType:Chance where OriginalItem is the item being upgraded, NewItem is the item to upgrade$
  scp914_rough_chances:
  - KeycardO5:KeycardJanitor:50
  - KeycardO5:Coin:100
  scp914_coarse_chances: []
  scp914_oneto_one_chances: []
  scp914_fine_chances: []
  scp914_very_fine_chances: []
  # The list of custom 914 recipies for 914. Valid formatting is OriginalRole:NewRole:Chance - IE: ClassD:Spectator:100 - for each knob setting defined.
  scp914_class_changes:
    Rough:
    - ClassD:Scientist:10
    - ClassD:Spectator:50
    Coarse: []
    OneToOne: []
    Fine: []
    VeryFine: []
  # The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.
  ragdoll_cleanup_delay: 300
  # If ragdoll cleanup should only happen in the Pocket Dimension or not.
  ragdoll_cleanup_only_pocket: false
  # The frequency (in seconds) between item cleanups. Set to 0 to disable.
  item_cleanup_delay: 900
  # If item cleanup should only happen in the Pocket Dimension or not.
  item_cleanup_only_pocket: true
  # A list of roles and how much health they should be given when they kill someone.
  health_on_kill:
    Scp173: 125
    Scp096: 70
  # A list of roles and what their default starting health should be.
 health_values:
    Scp173: 3000
    NtfCommander: 200
  # If the plugin is enabled or not.
  is_enabled: true

```

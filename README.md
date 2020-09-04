# Improved version of [KadeDev/Common-Utils](https://github.com/KadeDev/Common-Utils) with these added options:
- options to make SCP-049-2, SCP-096, SCP-106, SCP-173 able to speak to humans
- an option to clean items/ragdolls everywhere except in Pocket Dimension
- an option to modify the amnesia duration of SCP-939
- options to modify the damage every SCP does except SCP-079
- an option to play a CASSIE announcement 30 seconds before the auto-nuke activation
- an optional command for SCP-079 to lock down all doors and turn off the lights during the alpha warhead countdown
- options to have CASSIE announce when all class-d/scientists/guards are dead/escaped
- an option to destroy specific doors automatically on round start

# Installation

Install the latest of version of [EXILED](https://github.com/galaxy119/EXILED) if you don't have it, then download [Common_Utilities.dll](https://github.com/Aevann1/Common-Utils/releases) and place it in in %appdata%\EXILED\Plugins

# Default configs:
```yaml
  # Whether or not debug messages should be shown.
  debug: false
  # Whether or not CASSIE makes an announcement when a Class-D Personnel escapes and when all Class-D Personnel are dead or have escaped.
  announce_classd_elimination: false
  # Whether or not CASSIE makes an announcement when a Scientist escapes and when all Scientists are dead or have escaped.
  announce_scientists_elimination: false
  # Whether or not CASSIE makes an announcement when all Facility Guards are dead.
  announce_guards_elimination: false
  # Whether or not SCP-079 can use .nukelockdown in the console to lock down all doors and turn off the lights during the nuke countdown.
  nuke_lockdown: true
  # Energy cost for the nuke lockdown command.
  nuke_lockdown_cost: 100
  # Duration of the nuke lockdown.
  nuke_lockdown_duration: 90
  # Whether or not SCP-049 should be able to talk to humans.
  scp049_speech: true
  # Whether or not SCP-049-2 should be able to talk to humans.
  scp0492_speech: true
  # Whether or not SCP-096 should be able to talk to humans.
  scp096_speech: true
  # Whether or not SCP-106 should be able to talk to humans.
  scp106_speech: true
  # Whether or not SCP-173 should be able to talk to humans.
  scp173_speech: true
  # Extra SCP-939 amnesia duration.
  extra_amnesia: 0
  # SCP-049 damage.
  scp049_damage: 4949
  # SCP-049-2 damage.
  scp0492_damage: 40
  # SCP-096 damage.
  scp096_damage: 9696
  # SCP-106 damage.
  scp106_damage: 40
  # SCP-173 damage.
  scp173_damage: 999990
  # SCP-939 damage.
  scp939_damage: 65
  # Whether to destroy specific doors at the beginning of the round.
  destroy_doors: false
  # Destroyed doors at the beginning of the round. You can see all door names inside the Remote Admin Panel in-game.
  destroyed_doors:
  - NUKE_SURFACE
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
  # The amount of time (in seconds) after the round starts, before the facilities auto-nuke will start. Put 0 to disable.
  autonuke_time: 600
  # Whether or not the nuke should be unable to be disabled during the auto-nuke countdown.
  autonuke_lock: true
  # Message said by Cassie after auto-nuke activation.
  autonuke_cassie_message: automatic alpha warhead will be activated in tminus 30 seconds . it can't be disabled. please evacuate the facility
  # The list of items Class-D should have. Valid formatting should be ItemType:Chance where ItemType is the item to give them, and Chance is the percent chance of them spawning with it. You can specify the same item multiple times. This is true for all Inventory configs.
  class_d_inventory: []
  chaos_inventory: []
  scientist_inventory: []
  guard_inventory: []
  cadet_inventory: []
  lieutenant_inventory: []
  commander_inventory: []
  ntf_sci_inventory: []
  # The list of custom 914 recipies for the Rough setting. Valid formatting should be OriginalItemType:NewItemType:Chance where OriginalItem is the item being upgraded, NewItem is the item to upgrade to, and Chance is the percent chance of the upgrade happening. You can specify multiple upgrade choices for the same item. This is true for all 914 configs.
  scp914_rough_chances: []
  scp914_coarse_chances: []
  scp914_oneto_one_chances: []
  scp914_fine_chances: []
  scp914_very_fine_chances: []
  # The list of custom 914 recipies for 914. Valid formatting is OriginalRole:NewRole:Chance - IE: ClassD:Spectator:100 - for each knob setting defined.
  scp914_class_changes:
    Rough: []
    Coarse: []
    OneToOne: []
    Fine: []
    VeryFine: []
  # The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.
  ragdoll_cleanup_delay: 300
  # If ragdoll cleanup should only happen in the Pocket Dimension or not.
  ragdoll_cleanup_only_pocket: false
  # If ragdoll cleanup should happen everywhere except the Pocket Dimension or not.
  ragdoll_cleanup_not_pocket: false
  # The frequency (in seconds) between item cleanups. Set to 0 to disable.
  item_cleanup_delay: 300
  # If item cleanup should only happen in the Pocket Dimension or not.
  item_cleanup_only_pocket: false
  # If item cleanup should happen everywhere except the Pocket Dimension or not.
  item_cleanup_not_pocket: false
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
# Default inventories:
These are the default inventories as of 04/09/2020:
```yaml
util_ntfscientist_inventory: KeycardNTFLieutenant, GunE11SR, WeaponManagerTablet, GrenadeFrag, Radio, Medkit
util_scientist_inventory: KeycardScientist, Medkit
util_chaos_inventory: KeycardChaosInsurgency, GunLogicer, Medkit, Painkillers
util_ntflieutenant_inventory: KeycardNTFLieutenant, GunE11SR, WeaponManagerTablet, GrenadeFrag, Radio, Disarmer, Medkit
util_ntfcommander_inventory: KeycardNTFCommander, GunE11SR, WeaponManagerTablet, GrenadeFrag, Radio, Disarmer, Adrenaline
util_ntfcadet_inventory: KeycardSeniorGuard, GunProject90, WeaponManagerTablet, Radio, Disarmer, Medkit
util_guard_inventory: KeycardGuard, GunMP7, Medkit, WeaponManagerTablet, Disarmer, GrenadeFlash, Radio

# Other inventories that are completely clean (as in they don't have any single item):
util_classd_inventory, util_tutorial_inventory, util_scp173_inventory,
util_spectator_inventory, util_scp106_inventory, util_scp049_inventory,
util_scp079_inventory, util_scp096_inventory, util_scp0492_inventory,
util_scp93953_inventory, util_scp93989_inventory
```

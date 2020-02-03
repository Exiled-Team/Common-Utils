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

# Default config:
```yaml
util_enable: true
util_debug: false
util_914_enable: true
util_role_health: NtfCommand:400,NtfScientist:350 # Simple dictonary
util_914_upgrade_hand: true # Do you want peoples held items to be upgraded?
util_914_roles: Scientist-ClassD:Coarse, ClassD-Spectator:Rough # A custom dictonary with a second value.
util_914_items: Painkillers-Medkit:Fine,Coin-Flashlight:OneToOne # A custom dictonary with a second value.
# If you do want to make Custom Inventorys. You must set only the ones you want!
# Example of a custom inventory: "util_classd_inventory: Coin" or "util_ntfcadet_inventory: Adrenaline,Ammo556,Flashlight,GrenadeFlash,KeycardGuard,GunMP7"
util_enable_inventories: false
util_broadcast_enable: true # Do you want broadcasting to run?
util_broadcast_message: <color=lime>This server is running <color=red>EXILED-CommonUtils</color>, enjoy playing!</color>
util_broadcast_seconds: 300
util_broadcast_time: 4
util_joinMessage: <color=lime>Welcome %player%! Please read our rules!</color>
util_joinMessage_time: 6
util_enable_autonuke: false
util_autonuke_time: 600
util_autonuke_lock: false
```
# Default inventories:
These are the default inventories as of 26/01/2020:
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

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
util_enable: true # Enable the plugin or not
util_role_health: NtfCommand:400,NtfScientist:350 # Simple dictonary
util_914_roles: Scientist-ClassD:Coarse, ClassD-Spectator:Rough
util_914_items: Painkillers-Medkit:Fine,Coin-Flashlight:OneToOne # A custom dictonary with a second value.
# If you do want to make Custom Inventorys. You must set only the ones you want!
# Example of a custom inventory: "util_classd_inventory: Coin" or "util_ntfcadet_inventory: Adrenaline,Ammo556,Flashlight,GrenadeFlash,KeycardGuard,GunMP7"
util_broadcast_message: <color=lime>This server is running <color=red>EXILED-CommonUtils</color>, enjoy playing!</color> # String
util_broadcast_seconds: 300 # Int (intervals for when the broadcast should go off)
util_broadcast_time: 4 # How long it should last
util_joinMessage: <color=lime>Welcome %player%! Please read our rules!</color> # String
util_joinMessage_time: 6 # How long it should last.
```


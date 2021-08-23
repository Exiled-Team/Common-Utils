### This guide should help you understand the differences between the old and new configs.

## Inventory
#### All inventory configs have been rolled into a single config value `starting_inventories`. This means that you will not need to have 'empty configs' defined for roles you do not wish to change the inventories for. The new configs also allow you specify a usergroup, which will mean only that group can get that item.
starting_inventories is a dictionary, and should be formatted like this:
```yaml
starting_inventories:
  ClassD:
    slot1:
    - item_name: KeycardJanitor
      chance: 10
      group: none
```
If you wish to add additional slots, or roles, you can do so. 
With the new inventory config, if you define a role, it's inventory will be replaced with whatever is listed. If you list a role here, but don't give them any items, it will give them an empty inventory.
If you don't wish to change a particular roles inventory, simply don't include their roletype in this config.

You can define configs for slots 1 through 8. Any slots that you don't define will be auto-generated as empty, but not defining them will NOT cause the config to break.

## SCP-914 Item recipe configs:
#### These configs have been simplified, to allow you to not define keys you will not use, and should look like this:
```yaml
scp914_item_changes:
  Rough:
  - original: KeycardO5
    new: MicroHID
    chance: 50
  - original: GunCOM18
    new: GunCOM15
    chance: 100
```
If you wish to add additional knob settings, you can do so, but unused knob setting values will not be auto-generated anymore.

## SCP-914 Class recipe configs:
#### Everything from the above Item recipe config applies, except this uses RoleTypes instead of ItemTypes, IE:
```yaml
scp914_class_changes:
  Rough:
  - original: ClassD
    new: Spectator
    chance: 100
  OneToOne:
  - original: ClassD
    new: Scientist
    chance: 100
  - original: Scientist
    new: ClassD
    chance: 100
```


## SCP Damage Multipliers
#### These have been renamed to `role_damage_multipliers` and will affect the damage of anyone who has that role.

## Weapon Damage Multipliers
#### This has changed to use ItemTypes, and will affect any damage dealt by the specified item. Since only guns grenades & micro can deal damage, only those are affected.

### Both of the above damage multiplier configs are applied together, meaning if you have NtfCaptain doing double damage, and then have E11 doing double damage, an NtfCaptain will deal 4x damage with an E11

## HealthOnKill and HealthValues have both been updated to use RoleTypes directly, instead of a string that's parsed into a RoleType by the plugin.

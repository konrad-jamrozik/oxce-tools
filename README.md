# oxce-tools
Tools to aid playing mods based on OXCE (Open X-COM Extended)

## X-Com Files stats explorer

Generate a standalone HTML report from your installed X-Com Files ruleset. The
report works offline and has sortable tables, combining filters, a details view,
and CSV export of the current results.

### Run on Windows

Install Python 3.10 or newer, then run these commands from the repository root:

```powershell
python -m pip install -r requirements.txt
python oxce_stats.py "$env:USERPROFILE\OneDrive\Documents\OpenXcom\mods\XComFiles"
Start-Process .\oxce-stats.html
```

The mod path defaults to the location above. Use `--output` to choose the
destination. If the OXCE base rules are installed elsewhere, pass
`--base-ruleset "C:\path\to\OXCE\standard\xcom1"`. The script tries
`~/games/OXCE/standard/xcom1` by default. Use `--mod-only` to skip base rules.

### Find what you need

| Question | Filter and sort |
| --- | --- |
| Highest base weapon damage | **Weapons** → **Base damage** descending |
| Most expensive kinetic weapon | **Weapons** → **Kinetic** → **Buy price** descending |
| Heaviest shotgun | **Weapons** → **Shotguns** → **Weapon weight** descending |
| Highest-damage kinetic sniper rifle | **Weapons** → **Kinetic** + **Sniper Rifles** → **Base damage** descending |
| Armor with the best laser resistance | **Armor** → **Rank by resistance** → **Laser** |
| Enemy with the most health | **Enemies & units** → **Hostile roster** → **Health** descending |
| Monster with the strongest melee attack | **Enemies & units** → **Monster / supernatural** → **Melee power (est.)** descending |

### How to read the numbers

- Weapon damage follows each compatible ammo item, including separate ammo
  slots on combination weapons. Base damage is raw ruleset power per
  projectile, before accuracy, pellets, stat bonuses, and target resistance.
  The weapon's weight and prices stay separate from its ammo's.
- Fire-mode TU costs display in flat TU or as a percentage of the unit's TU.
  The accuracy percentages are weapon modifiers. Actual hit chance also depends
  on the shooter, range, and game conditions. Resistance columns show damage
  taken, so 75% means 25% resistance.
- Buy and sell prices come from item rules. A dash indicates a missing or
  zero price. Availability in a campaign also depends on research and other
  conditions.
- Unit health is the base unit stat. Melee estimates use the strongest
  declared built-in melee option with supported stat bonuses evaluated on the
  unit's base stats. They exclude mission loadouts, difficulty adjustments, and
  scripted combat effects. The roster and monster filters use XCF race lists
  and specimen categories, with zombies included as monsters.
- The report layers XCF over the selected base rules. It reads the files on
  disk and does not apply your enabled add-ons or load a campaign save.

Ruleset field reference: [UFOpaedia](https://www.ufopaedia.org/Ruleset_Reference_Nightly_(OpenXcom)).

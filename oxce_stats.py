"""Build an offline equipment and unit report from OpenXcom Extended rulesets."""

import argparse
from collections import defaultdict
import json
from pathlib import Path
import re
import sys

import yaml


DAMAGE_TYPES = (
    "Special", "Kinetic", "Incendiary", "Explosive / concussive", "Laser",
    "Plasma", "Stun", "Melee / cutting", "Chemical", "Smoke", "EMP",
    "Electric", "Psi", "Warp", "Anti-E115", "Bio",
)
BATTLE_TYPES = (
    "Other", "Firearm", "Ammo", "Melee", "Grenade", "Proximity grenade",
    "Medical", "Scanner", "Mind probe", "Psi equipment", "Flare", "Corpse",
)
WEAPON_TYPES = (
    "PISTOLS", "SMGS", "SHOTGUNS", "RIFLES", "SNIPER_RIFLES", "MACHINE_GUNS",
    "CANNONS", "LAUNCHERS", "BOWS", "CROSSBOWS", "MELEE", "THROWING",
    "GRENADES", "HEAVY_WEAPONS", "FIREARMS", "MASS_DRIVER", "SONIC",
)
SECTIONS = ("items", "armors", "units", "alienRaces", "craftWeapons")
SECTION_LINE = re.compile(r"^(?:" + "|".join(SECTIONS) + r")\s*:", re.MULTILINE)
UNIT_STATS = ("tu", "stamina", "health", "bravery", "reactions", "firing",
              "throwing", "strength", "psiStrength", "psiSkill", "melee")


class RulesLoader(yaml.SafeLoader):
    def compose_node(self, parent, index):
        # XCF reuses a few anchor names. Later aliases use the latest definition.
        event = self.peek_event()
        if event.anchor and not isinstance(event, yaml.AliasEvent):
            self.anchors.pop(event.anchor, None)
        return super().compose_node(parent, index)


def load_yaml(path):
    with path.open(encoding="utf-8-sig") as handle:
        return yaml.load(handle, Loader=RulesLoader)


def flatten_ids(value):
    if isinstance(value, str):
        yield value
    elif isinstance(value, (list, tuple)):
        for part in value:
            yield from flatten_ids(part)
    elif isinstance(value, dict):
        for part in value.values():
            yield from flatten_ids(part)


def merge_fields(old, new):
    result = dict(old)
    for key, value in new.items():
        if isinstance(value, dict) and isinstance(result.get(key), dict):
            result[key] = merge_fields(result[key], value)
        else:
            result[key] = value
    return result


def expand_ref(entry):
    parent = entry.get("refNode")
    fields = {key: value for key, value in entry.items() if key != "refNode"}
    return merge_fields(expand_ref(parent), fields) if isinstance(parent, dict) else fields


def read_data(layers):
    tables = {section: {} for section in SECTIONS}
    labels = {}
    for layer in layers:
        rules_dir = layer / "Ruleset" if (layer / "Ruleset").is_dir() else layer
        files = sorted(rules_dir.rglob("*.rul"), key=lambda path: path.as_posix().casefold())
        if not files:
            raise ValueError(f"No .rul files found in {rules_dir}")
        for path in files:
            text = path.read_text(encoding="utf-8-sig")
            if not SECTION_LINE.search(text):
                continue
            try:
                document = yaml.load(text, Loader=RulesLoader) or {}
            except yaml.YAMLError as error:
                raise ValueError(f"{path}: {error}") from error
            for section, table in tables.items():
                for entry in document.get(section, []) or []:
                    if "delete" in entry:
                        table.pop(entry["delete"], None)
                    elif key := entry.get("id" if section == "alienRaces" else "type"):
                        table[key] = merge_fields(table.get(key, {}), expand_ref(entry))
        for language_dir in dict.fromkeys((layer / "Language", rules_dir.parent / "Language")):
            language = language_dir / "en-US.yml"
            if language.is_file():
                labels.update((load_yaml(language) or {}).get("en-US", {}))
    return tables, labels


def number(value):
    return value if type(value) in (int, float) else None


def index_label(values, index):
    if type(index) is not int:
        return None
    return values[index] if 0 <= index < len(values) else f"Type {index}"


def damage_type(profile, damage_key="damageType", alter_key="damageAlter", default=None):
    kind = profile.get(damage_key, default)
    return (index_label(DAMAGE_TYPES, kind),
            index_label(DAMAGE_TYPES, profile.get(alter_key, {}).get("ResistType", kind)))


def weapon_modes(weapon, name):
    result = {}
    for mode in ("Snap", "Aimed", "Auto", "Melee", "Use"):
        cost = weapon.get("cost" + mode, {}).get("time", weapon.get("tu" + mode))
        if not cost:
            continue
        flat = weapon.get("flat" + mode, weapon.get("flatRate", False))
        if isinstance(flat, dict):
            flat = flat.get("time", False)
        conf = weapon.get("conf" + mode, {})
        result[mode] = {
            "tu": cost, "flat": bool(flat), "accuracy": weapon.get("accuracy" + mode),
            "slot": conf.get("ammoSlot", 0),
            "shots": conf.get("shots", weapon.get("autoShots", 3) if mode == "Auto" else 1),
            "name": name(conf["name"]) if conf.get("name") else mode,
        }
    return result


def weapon_ammo(weapon):
    slots = {0: weapon.get("compatibleAmmo", [])}
    for key, slot in weapon.get("ammo", {}).items():
        if "compatibleAmmo" in slot:
            slots[int(key)] = slot["compatibleAmmo"]
    return [(slot, ammo) for slot, ids in slots.items() for ammo in ids]


def is_weapon(item):
    return (item.get("battleType") in {1, 3, 4, 5, 9}
            or any(key in item for key in ("compatibleAmmo", "ammo", "meleePower", "confMelee")))


def price(item, key):
    value = number(item.get(key))
    return value if value and value > 0 else None


def item_values(key, item, name):
    return {
        "id": key, "name": name(item.get("name") or key),
        "kind": index_label(BATTLE_TYPES, item.get("battleType", 0)),
        "categories": [name(category) for category in item.get("categories", [])],
        "weight": item.get("weight"), "buy": price(item, "costBuy"),
        "sell": price(item, "costSell"), "size": item.get("size"),
        "fixed": "Built-in / fixed" if item.get("fixedWeapon") or
                 "STR_BUILT_INS" in item.get("categories", []) else "Regular item",
    }


def effect(profile, melee=False):
    if melee:
        power = profile.get("meleePower")
        kind, resist = damage_type(profile, "meleeType", "meleeAlter", 6)
        bonus = profile.get("meleeBonus", {})
    else:
        power = profile.get("power")
        kind, resist = damage_type(profile, default=7 if profile.get("battleType") == 3 else None)
        bonus = profile.get("damageBonus", {"strength": 1} if profile.get("strengthApplied") else {})
    if bonus and power is None:
        power = 0  # A stat bonus can supply all of a weapon's power.
    return {"damage": power, "damageType": kind, "resistType": resist, "bonus": bonus or None}


def weapon_profiles(key, weapon, items, name):
    base = item_values(key, weapon, name)
    classes = [name("STR_" + kind) for kind in WEAPON_TYPES
               if "STR_" + kind in weapon.get("categories", [])]
    base["classes"] = classes or [base["kind"]]
    modes = weapon_modes(weapon, name)
    ammo_rows = weapon_ammo(weapon)
    if not ammo_rows:
        ammo_rows = [(None, None)]
    for slot, ammo in ammo_rows:
        source = items.get(ammo, {}) if ammo else weapon
        row = dict(base, **effect(source))
        row.update(ammo=name(ammo) if ammo else None, ammoId=ammo, slot=slot,
                   ammoWeight=source.get("weight") if ammo else None,
                   ammoBuy=price(source, "costBuy") if ammo else None,
                   ammoSell=price(source, "costSell") if ammo else None,
                   clip=source.get("clipSize"), pellets=source.get("shotgunPellets"),
                   powerDropoff=source.get("powerRangeReduction", weapon.get("powerRangeReduction")),
                   powerThreshold=source.get("powerRangeThreshold", weapon.get("powerRangeThreshold")))
        for mode, fields in modes.items():
            if (ammo and fields["slot"] != slot) or (
                mode == "Melee" and (weapon.get("meleePower") or weapon.get("meleeBonus"))
            ):
                continue
            add_mode(row, mode, fields)
        if weapon.get("battleType") in {1, 9}:
            row.update(range=weapon.get("maxRange", 200), minRange=weapon.get("minRange", 0),
                       dropoff=weapon.get("dropoff", 1 if weapon.get("battleType") == 9 else 2))
            for mode, field, default in (("Aimed", "aimRange", 200), ("Snap", "snapRange", 15),
                                         ("Auto", "autoRange", 7)):
                if mode + "Tu" in row:
                    row[field] = weapon.get(field, default)
        row["source"] = "Ammo" if ammo else "Weapon"
        if ammo and ammo not in items:
            row["source"] = "Unresolved ammo"
        yield row

    if weapon.get("meleePower") or weapon.get("meleeBonus"):
        row = dict(base, **effect(weapon, melee=True), source="Secondary melee")
        if "Melee" in modes:
            add_mode(row, "Melee", modes["Melee"])
        yield row


def add_mode(row, mode, fields):
    for key in ("tu", "flat", "accuracy", "shots", "name"):
        row[mode + key.title()] = fields[key]


def armor_values(key, armor, items, name):
    item = items.get(armor.get("storeItem"), {})
    row = {
        "id": key, "name": name(key), "kind": "Wearable" if armor.get("units") else "Unit armor",
        "storeItem": name(armor["storeItem"]) if armor.get("storeItem") else None,
        "weight": armor.get("weight"), "buy": price(item, "costBuy"), "sell": price(item, "costSell"),
        "wearers": [name(unit) for unit in armor.get("units", [])],
        "size": armor.get("size", 1), "psiDefence": armor.get("psiDefence"),
        "meleeDodge": armor.get("meleeDodge"), "stats": armor.get("stats", {}),
    }
    for side in ("front", "side", "rear", "under"):
        row[side] = armor.get(side + "Armor")
    modifiers = armor.get("damageModifier", [])
    for index, _ in enumerate(DAMAGE_TYPES):
        row[f"resist{index}"] = modifiers[index] if index < len(modifiers) else 1.0
    tags = armor.get("tags", {})
    row["shield"] = tags.get("ARMOR_ENERGY_SHIELD_CAPACITY")
    row["shieldRegen"] = tags.get("ARMOR_ENERGY_SHIELD_PER_TURN")
    return row


def estimated_power(attack, stats):
    base = attack["damage"]
    if base is None:
        return None
    total = base
    for stat, coefficients in (attack["bonus"] or {}).items():
        value = 100 if stat == "flatHundred" else number(stats.get(stat))
        if value is None:
            return None
        if not isinstance(coefficients, list):
            coefficients = [coefficients]
        if any(number(coefficient) is None for coefficient in coefficients):
            return None
        total += sum(coefficient * value ** degree
                     for degree, coefficient in enumerate(coefficients, 1))
    return int(total)


def units_report(tables, name):
    items, armors, units = (tables[key] for key in ("items", "armors", "units"))
    groups = defaultdict(list)
    for key, race in tables["alienRaces"].items():
        for unit in set(flatten_ids([race.get("members"), race.get("membersRandom")])):
            groups[unit].append(name(key))
    species_categories = defaultdict(set)
    for key, unit in units.items():
        armor = armors.get(unit.get("armor"), {})
        ids = [key, unit.get("liveAlien"), unit.get("race"), armor.get("corpseGeo")]
        ids.extend(flatten_ids(armor.get("corpseBattle")))
        for item_id in ids:
            species_categories[unit.get("race")].update(items.get(item_id, {}).get("categories", []))

    for key, unit in units.items():
        armor_id = unit.get("armor")
        armor = armors.get(armor_id, {})
        raw = [unit.get("builtInWeaponSets"), unit.get("weightedBuiltInWeaponSets"),
               unit.get("builtInWeapons"), unit.get("meleeWeapon"),
               unit.get("specialWeapon"), armor.get("builtInWeapons"), armor.get("specialWeapon")]
        equipment = {item: items[item] for item in dict.fromkeys(flatten_ids(raw)) if item in items}
        attacks = []
        for item_id, item in equipment.items():
            if item.get("battleType") == 3 or item_id == unit.get("meleeWeapon"):
                attacks.append(dict(effect(item), name=name(item_id)))
            if item.get("meleePower") or item.get("meleeBonus"):
                attacks.append(dict(effect(item, melee=True), name=name(item_id)))
        attacks.sort(key=lambda attack: attack["damage"] if attack["damage"] is not None else -1,
                     reverse=True)
        stats = unit.get("stats", {})
        for attack in attacks:
            attack["estimatedPower"] = estimated_power(attack, stats)
        race = unit.get("race", "")
        categories = species_categories[race]
        kinds = []
        if categories.intersection({"STR_BEASTS", "STR_DEMONS", "STR_GHOSTS"}) or race == "STR_ZOMBIE":
            kinds.append("Monster / supernatural")
        if "STR_ROBOTS" in categories or race == "STR_ROBOT":
            kinds.append("Robot")
        if "STR_ALIEN_SPECIMENS" in categories:
            kinds.append("Alien specimen")
        if race == "STR_HUMAN":
            kinds.append("Human")
        if unit.get("livingWeapon"):
            kinds.append("Living weapon")
        row = {stat: stats.get(stat) for stat in UNIT_STATS}
        row.update(id=key, name=name(key), race=name(race), rank=name(unit.get("rank", "")),
                   armor=name(armor_id) if armor_id else None, armorId=armor_id,
                   kinds=kinds or ["Other"], groups=groups[key],
                   role="Hostile roster" if groups[key] else "Other unit",
                   front=armor.get("frontArmor"), side=armor.get("sideArmor"),
                   rear=armor.get("rearArmor"), under=armor.get("underArmor"),
                   meleeDamage=next((attack["damage"] for attack in attacks if attack["damage"] is not None), None),
                   meleePower=max((attack["estimatedPower"] for attack in attacks
                                   if attack["estimatedPower"] is not None), default=None),
                   attacks=attacks, equipment=[name(item) for item in equipment],
                   armorHealth=armor.get("stats", {}).get("health"),
                   aggression=unit.get("aggression"), intelligence=unit.get("intelligence"))
        yield row


def build_report(tables, translations, sources):
    def name(key):
        if not key:
            return ""
        translation = translations.get(key)
        return translation if isinstance(translation, str) else key.removeprefix("STR_").replace("_", " ").title()

    items = tables["items"]
    catalog = [dict(item_values(key, item, name), **effect(item)) for key, item in items.items()]
    weapons = [row for key, item in items.items() if is_weapon(item)
               for row in weapon_profiles(key, item, items, name)]
    armors = [armor_values(key, armor, items, name) for key, armor in tables["armors"].items()]
    craft = []
    for key, weapon in tables["craftWeapons"].items():
        launcher = weapon.get("launcher") or key
        row = item_values(launcher, items.get(launcher, {}), name)
        row.update(id=key, damage=weapon.get("damage"), range=weapon.get("range"),
                   accuracy=weapon.get("accuracy"), mount=weapon.get("weaponType"),
                   ammo=name(weapon.get("clip")), capacity=weapon.get("ammoMax"),
                   cautious=weapon.get("reloadCautious"), standard=weapon.get("reloadStandard"),
                   aggressive=weapon.get("reloadAggressive"))
        craft.append(row)
    return {"weapons": weapons, "armors": armors, "units": list(units_report(tables, name)),
            "items": catalog, "craft": craft, "damageTypes": DAMAGE_TYPES, "sources": sources}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mod", nargs="?", type=Path,
                        default=Path.home() / "OneDrive/Documents/OpenXcom/mods/XComFiles")
    parser.add_argument("--base-ruleset", type=Path,
                        help="OXCE standard/xcom1 directory. Auto-detected in ~/games/OXCE if present.")
    parser.add_argument("--mod-only", action="store_true", help="Skip the base ruleset.")
    parser.add_argument("--output", "-o", type=Path, default=Path("oxce-stats.html"))
    args = parser.parse_args()
    base = args.base_ruleset
    if base is None:
        candidate = Path.home() / "games/OXCE/standard/xcom1"
        base = candidate if candidate.is_dir() else None
    if args.mod_only:
        base = None
    layers = [base, args.mod] if base else [args.mod]
    try:
        tables, labels = read_data(layers)
        if not all(tables[section] for section in ("items", "armors", "units")):
            raise ValueError("Expected item, armor, and unit rules. Pass the X-Com Files mod directory.")
        data = build_report(tables, labels, [str(path.resolve()) for path in layers])
        payload = json.dumps(data, ensure_ascii=False, separators=(",", ":")).replace("<", "\\u003c")
        template = Path(__file__).with_name("stats_report.html").read_text(encoding="utf-8")
        args.output.write_text(template.replace("<!-- REPORT_DATA -->", payload, 1), encoding="utf-8")
    except (OSError, ValueError, yaml.YAMLError) as error:
        parser.exit(1, f"Error: {error}\n")

    print(f"Wrote {args.output.resolve()}")
    print(f"{len(data['weapons'])} weapon profiles, {len(data['armors'])} armor definitions, "
          f"{len(data['units'])} unit definitions, {len(data['items'])} items, "
          f"{len(data['craft'])} craft weapons.")
    if base is None:
        print("Base game rules were not loaded. For a fuller report, pass --base-ruleset.",
              file=sys.stderr)


if __name__ == "__main__":
    main()

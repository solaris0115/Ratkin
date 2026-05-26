# -*- coding: utf-8 -*-
"""Inject NewRatkin.CompProperties_WeaponGripType into 1.6 weapon ThingDefs (one-shot maintenance)."""
from __future__ import annotations

import xml.etree.ElementTree as ET
from pathlib import Path


def is_equipment_weapon(thing: ET.Element) -> bool:
    if thing.get("Abstract") == "True":
        return False
    if thing.find("defName") is None:
        return False
    tc = thing.find("thingClass")
    if tc is not None and tc.text and "Projectile" in tc.text:
        return False
    cat = thing.find("category")
    if cat is not None and cat.text == "Projectile":
        return False
    if thing.find("projectile") is not None and thing.find("verbs") is None and thing.find("weaponTags") is None:
        return False
    if thing.find("verbs") is not None:
        return True
    if thing.find("weaponTags") is not None:
        return True
    if thing.find("tools") is not None:
        return True
    return False


def collect_weapon_tags(thing: ET.Element) -> list[str]:
    wt = thing.find("weaponTags")
    if wt is None:
        return []
    return [li.text for li in wt.findall("li") if li.text]


def grip_types_from_tags(tags: list[str]) -> list[str]:
    has_one = "RK_WeaponTag_OneHand" in tags or "RK_Weapon_OneHand" in tags
    has_two = "RK_WeaponTag_TwoHand" in tags or "RK_Weapon_TwoHand" in tags
    has_sc = "RK_WeaponTag_ShieldCompatible" in tags
    if has_two and has_sc:
        return ["TwoHand", "Special"]
    if has_one:
        return ["OneHand"]
    if has_two:
        return ["TwoHand"]
    return ["TwoHand"]


def has_grip_comp(thing: ET.Element) -> bool:
    comps = thing.find("comps")
    if comps is None:
        return False
    for li in comps.findall("li"):
        if li.get("Class") == "NewRatkin.CompProperties_WeaponGripType":
            return True
    return False


def inject_comp(thing: ET.Element, grips: list[str]) -> None:
    comps = thing.find("comps")
    grip_el = ET.Element("li")
    grip_el.set("Class", "NewRatkin.CompProperties_WeaponGripType")
    gt = ET.SubElement(grip_el, "gripTypes")
    for g in grips:
        li = ET.SubElement(gt, "li")
        li.text = g
    if comps is None:
        comps = ET.SubElement(thing, "comps")
        comps.append(grip_el)
    else:
        comps.insert(0, grip_el)


def indent(elem: ET.Element, level: int = 0) -> None:
    pad = "\n" + level * "\t"
    children = list(elem)
    if len(children):
        if not elem.text or not elem.text.strip():
            elem.text = pad + "\t"
        for child in children:
            indent(child, level + 1)
        for child in children:
            if not child.tail or not child.tail.strip():
                child.tail = pad + "\t"
        if not children[-1].tail or not children[-1].tail.strip():
            children[-1].tail = pad
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = pad


def process_file(fp: Path) -> int:
    tree = ET.parse(fp)
    root = tree.getroot()
    n = 0
    for thing in root.findall("ThingDef"):
        if not is_equipment_weapon(thing):
            continue
        if has_grip_comp(thing):
            continue
        grips = grip_types_from_tags(collect_weapon_tags(thing))
        inject_comp(thing, grips)
        n += 1
    indent(root)
    ET.register_namespace("", "")
    tree.write(fp, encoding="utf-8", xml_declaration=True)
    return n


def main() -> None:
    base = Path(__file__).resolve().parents[1]
    files = [
        base / "Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml",
        base / "Project/1.6/Defs/ThingsDefs/Weapon_Range.xml",
        base / "Project/1.6/Defs/ThingsDefs/Weapon_Util.xml",
        base / "Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml",
        base / "Project/1.6/Defs/ThingsDefs/Weapon_DropOnly.xml",
    ]
    for fp in files:
        count = process_file(fp)
        print(fp.name, "injected", count)


if __name__ == "__main__":
    main()

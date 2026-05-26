#!/usr/bin/env python3
"""
Parse ranged weapon ThingDefs from RimworldData and Project/1.6/Defs.
Extract DPS-related stats and output cache + report.
"""
import xml.etree.ElementTree as ET
from pathlib import Path
from collections import defaultdict
from datetime import datetime

ROOT = Path(__file__).resolve().parent.parent
RIMWORLD_SOURCES = [
    ROOT / "RimworldData" / "Core" / "Defs",
    ROOT / "RimworldData" / "Royalty" / "Defs",
    ROOT / "RimworldData" / "Ideology" / "Defs",
    ROOT / "RimworldData" / "Biotech" / "Defs",
    ROOT / "RimworldData" / "Anomaly" / "Defs",
    ROOT / "RimworldData" / "Odyssey" / "Defs",
]
PROJECT_DEFS = ROOT / "Project" / "1.6" / "Defs"

# Verb classes to include (projectile-based)
# Exact match or class name (handles namespaced: NewRatkin.Verb_BFRShoot)
INCLUDED_VERBS = {
    "Verb_Shoot", "Verb_LaunchProjectile", "Verb_ShootOneUse",
    "Verb_BFRShoot", "Verb_RatHolicGun",  # 랫킨 커스텀 (Verb_Shoot 상속)
}

def _is_shoot_verb(verb_class: str) -> bool:
    """verbClass가 발사형인지. 네임스페이스(NewRatkin.Verb_BFRShoot) 지원."""
    if not verb_class:
        return False
    name = verb_class.split(".")[-1] if "." in verb_class else verb_class
    return name in INCLUDED_VERBS

# Default from VerbProperties.cs
DEFAULT_TICKS_BETWEEN_BURST = 15

# Accuracy stat names (Touch~3셀, Short~12셀, Medium~25셀, Long~40+셀)
ACCURACY_STATS = ("AccuracyTouch", "AccuracyShort", "AccuracyMedium", "AccuracyLong")
DEFAULT_ACCURACY = 0.5  # 미지정 시

# 터렛 건물(turretGunDef) 스킵 여부. True면 터렛 제외, False면 터렛 건 포함
SKIP_TURRETS = True


def get_text(elem, default=""):
    if elem is not None and elem.text:
        return elem.text.strip()
    return default


def get_float(val, default=0.0):
    if val is None:
        return default
    if hasattr(val, "text"):
        t = get_text(val)
    else:
        t = str(val).strip() if val else ""
    if not t:
        return default
    try:
        return float(t)
    except ValueError:
        return default


def get_int(val, default=0):
    if val is None:
        return default
    if hasattr(val, "text"):
        t = get_text(val)
    else:
        t = str(val).strip() if val else ""
    if not t:
        return default
    try:
        return int(t)
    except ValueError:
        return default


def find_child(parent, tag, ns=None):
    if ns:
        return parent.find(f".//{{{ns}}}{tag}")
    for c in parent.iter():
        if c.tag.endswith("}" + tag) or c.tag == tag:
            return c
    return parent.find(f".//{tag}")


def find_direct(parent, tag):
    for c in parent:
        if c.tag.endswith("}" + tag) or c.tag == tag:
            return c
    return None


def parse_defs_from_file(path):
    """Parse all ThingDef elements from an XML file."""
    try:
        tree = ET.parse(path)
        root = tree.getroot()
    except ET.ParseError:
        return []
    ns = root.tag.split("}")[0] + "}" if "}" in root.tag else ""
    defs = []
    for def_elem in root.iter():
        if def_elem.tag.endswith("}ThingDef") or def_elem.tag == "ThingDef":
            defs.append((path, def_elem, ns))
    return defs


def get_def_name(elem, ns):
    child = find_direct(elem, "defName")
    if child is not None:
        t = get_text(child)
        if t:
            return t
    name_attr = elem.get("Name")
    if name_attr:
        return name_attr
    return None


def get_parent_name(elem, ns):
    parent_attr = elem.get("ParentName")
    if parent_attr:
        return parent_attr
    return None


def extract_stat_bases(elem, ns):
    stat_bases = {}
    sb = find_direct(elem, "statBases")
    if sb is not None:
        for stat in sb:
            tag = stat.tag.split("}")[-1] if "}" in stat.tag else stat.tag
            stat_bases[tag] = get_text(stat)
    return stat_bases


def extract_verbs(elem, ns):
    verbs = []
    verbs_elem = find_direct(elem, "verbs")
    if verbs_elem is None:
        return verbs
    for li in verbs_elem:
        if li.tag.endswith("}li") or li.tag == "li":
            verb = {}
            for c in li:
                tag = c.tag.split("}")[-1] if "}" in c.tag else c.tag
                if tag in ("verbClass", "defaultProjectile", "warmupTime", "burstShotCount", "ticksBetweenBurstShots", "range"):
                    verb[tag] = get_text(c)
            verbs.append(verb)
    return verbs


def extract_building(elem, ns):
    building = {}
    b = find_direct(elem, "building")
    if b is not None:
        tg = find_direct(b, "turretGunDef")
        if tg is not None:
            building["turretGunDef"] = get_text(tg)
    return building


WEAPON_PARENTS = {"BaseWeaponTurret", "BaseGun", "BaseMakeableGrenade", "BaseArtilleryWeapon"}


def _inherits_from(def_name, parent_target, all_defs, cache=None):
    """def_name이 parent_target을 조상으로 갖는지."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = False
        return False
    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    if not parent_name:
        cache[def_name] = False
        return False
    actual = _resolve_parent_name(parent_name, all_defs)
    if actual == parent_target:
        cache[def_name] = True
        return True
    result = _inherits_from(actual, parent_target, all_defs, cache)
    cache[def_name] = result
    return result


def _extract_comp_class_names(comps_elem, ns):
    """comps 요소에서 compClass 또는 Class 속성으로 컴포넌트 클래스명 추출."""
    names = []
    for li in comps_elem:
        if li.tag.endswith("}li") or li.tag == "li":
            cls_attr = li.get("Class")
            if cls_attr:
                names.append(cls_attr)
            else:
                comp_class = find_direct(li, "compClass")
                if comp_class is not None:
                    t = get_text(comp_class)
                    if t:
                        names.append(t)
    return names


def _collect_comps_with_inheritance(def_name, all_defs, cache=None):
    """부모 체인을 따라 comps 수집. Inherit=False면 부모 comps 대체."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = []
        return []

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_comps = []
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_comps = _collect_comps_with_inheritance(actual_parent, all_defs, cache)

    comps_elem = find_direct(elem, "comps")
    if comps_elem is None:
        cache[def_name] = parent_comps
        return parent_comps

    inherit = comps_elem.get("Inherit", "True")
    my_comps = _extract_comp_class_names(comps_elem, ns)
    if inherit and inherit.lower() == "false":
        cache[def_name] = my_comps
        return my_comps
    cache[def_name] = parent_comps + my_comps
    return parent_comps + my_comps


# Equippable 상속 컴포넌트: CompEquippable, CompEquippableAbility, CompEquippableAbilityReloadable
# CompProperties_EquippableAbility, CompProperties_EquippableAbilityReloadable
# CompProperties_EquipableHediff는 ThingComp 상속(착용 기능 없음) → "Equippable"만 체크


def _has_equippable_component(def_name, all_defs, cache=None):
    """부모 체인 포함 comps 중 Equippable/Equipable 상속 컴포넌트 존재 여부."""
    comps = _collect_comps_with_inheritance(def_name, all_defs, cache)
    for comp_name in comps:
        if "Equippable" in comp_name:
            return True
        if "Equipable" in comp_name and "Hediff" not in comp_name:
            return True
    return False


def _collect_turret_gun_defs(all_defs):
    """building.turretGunDef으로 참조되는 무기 defName 집합."""
    turret_guns = set()
    for def_name in all_defs:
        if def_name == "_name_to_def":
            continue
        path, elem, ns, _, _ = all_defs[def_name]
        building = extract_building(elem, ns)
        tg = building.get("turretGunDef")
        if tg:
            turret_guns.add(tg)
    return turret_guns


def is_weapon_def(def_name, all_defs):
    """Check if ThingDef is a weapon (exclude consumables like drinks)."""
    if def_name not in all_defs:
        return False
    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    if parent_name in WEAPON_PARENTS:
        return True
    if parent_name and is_weapon_def(parent_name, all_defs):
        return True
    tc = find_direct(elem, "thingCategories")
    if tc is not None:
        for li in tc:
            if li.tag.endswith("}li") or li.tag == "li":
                t = get_text(li)
                if t and any(w in t for w in ("WeaponsRanged", "WeaponsMelee", "Grenades")):
                    return True
    wc = find_direct(elem, "weaponClasses")
    if wc is not None:
        for li in wc:
            if li.tag.endswith("}li") or li.tag == "li":
                if get_text(li):
                    return True
    wt = find_direct(elem, "weaponTags")
    if wt is not None:
        for li in wt:
            if li.tag.endswith("}li") or li.tag == "li":
                if get_text(li):
                    return True
    return False


# ShootTuning.cs 고정 거리 구간 (타일)
DIST_TOUCH = 3.0
DIST_SHORT = 12.0
DIST_MEDIUM = 25.0
DIST_LONG = 40.0


def _acc_at_dist(d: float, acc_t: float, acc_s: float, acc_m: float, acc_l: float) -> float:
    """VerbProperties.GetHitChanceFactor 재현: 고정 구간 선형 보간."""
    if d <= DIST_TOUCH:
        return acc_t
    if d <= DIST_SHORT:
        return acc_t + (acc_s - acc_t) * (d - DIST_TOUCH) / (DIST_SHORT - DIST_TOUCH)
    if d <= DIST_MEDIUM:
        return acc_s + (acc_m - acc_s) * (d - DIST_SHORT) / (DIST_MEDIUM - DIST_SHORT)
    if d <= DIST_LONG:
        return acc_m + (acc_l - acc_m) * (d - DIST_MEDIUM) / (DIST_LONG - DIST_MEDIUM)
    return acc_l


def acc_weighted_avg(acc_t: float, acc_s: float, acc_m: float, acc_l: float,
                     weapon_range: float) -> float:
    """사격 가능 거리 [0, R] 전체에서 acc(d)를 적분한 가중 평균 명중률.
    선형 보간이므로 각 구간은 사다리꼴 공식으로 계산.
    30_Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md 참조.
    """
    if weapon_range <= 0:
        return 0.0
    R = weapon_range
    breakpoints = sorted(set([0, min(DIST_TOUCH, R), min(DIST_SHORT, R),
                              min(DIST_MEDIUM, R), min(DIST_LONG, R), R]))
    total = 0.0
    for i in range(len(breakpoints) - 1):
        d0, d1 = breakpoints[i], breakpoints[i + 1]
        width = d1 - d0
        if width <= 0:
            continue
        total += width * (_acc_at_dist(d0, acc_t, acc_s, acc_m, acc_l)
                          + _acc_at_dist(d1, acc_t, acc_s, acc_m, acc_l)) / 2
    return total / R


def _burst_sec(ticks: int) -> float:
    """Convert ticksBetweenBurstShots to seconds (60 ticks = 1 sec)."""
    return round(ticks / 60.0, 3)


def _markdown_table(headers: list[str], rows: list[list[str]]) -> str:
    """Build markdown table string. No blank lines between rows."""
    def row_str(cells):
        return "| " + " | ".join(str(c) for c in cells) + " |"
    sep = "|" + "|".join(["---"] * len(headers)) + "|"
    return "\n".join([row_str(headers), sep] + [row_str(r) for r in rows])


def collect_all_thingdefs():
    """Collect all ThingDefs from all source paths."""
    all_defs = {}  # defName -> (source_path, elem, ns)
    all_files = []

    for base in RIMWORLD_SOURCES:
        if base.exists():
            for p in base.rglob("*.xml"):
                all_files.append((p, "Rimworld"))
    if PROJECT_DEFS.exists():
        for p in PROJECT_DEFS.rglob("*.xml"):
            all_files.append((p, "Project"))

    name_to_def = {}  # Name 속성 -> defName (ParentName="NeedleGunBase" 참조용)

    for path, source_type in all_files:
        for _, elem, ns in parse_defs_from_file(path):
            def_name = get_def_name(elem, ns)
            if not def_name:
                continue
            name_attr = elem.get("Name")  # Name="NeedleGunBase" 등
            rel = str(path.relative_to(ROOT))
            is_abstract = elem.get("Abstract") == "True"

            if is_abstract:
                if def_name not in all_defs:
                    all_defs[def_name] = (path, elem, ns, rel, source_type)
            else:
                if def_name not in all_defs or "Project" in rel:
                    all_defs[def_name] = (path, elem, ns, rel, source_type)

            if name_attr and name_attr != def_name:
                name_to_def[name_attr] = def_name

    all_defs["_name_to_def"] = name_to_def
    return all_defs


def _resolve_parent_name(parent_name, all_defs):
    """ParentName이 defName이거나 Name 속성일 수 있음. 실제 defName 반환."""
    if parent_name in all_defs and parent_name != "_name_to_def":
        return parent_name
    name_to_def = all_defs.get("_name_to_def", {})
    return name_to_def.get(parent_name, parent_name)


def resolve_def(def_name, all_defs, cache):
    """Resolve a def with parent inheritance. Returns merged dict of values."""
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = {}
        return {}

    path, elem, ns, rel, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    base = {}
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        base = resolve_def(actual_parent, all_defs, cache)

    result = dict(base)
    stat_bases = extract_stat_bases(elem, ns)
    for k, v in stat_bases.items():
        result[f"stat_{k}"] = v
    result["_source"] = rel
    cache[def_name] = result
    return result


def get_projectile_damage(proj_def_name, all_defs, proj_cache):
    """Resolve projectile ThingDef and get damageAmountBase, armorPenetrationBase, damageDef.
    Returns (damage, ap_raw, damage_def_name).
    ap_raw: None when not in XML (use RimWorld fallback), float when explicit.
    """
    if proj_def_name in proj_cache:
        return proj_cache[proj_def_name]
    if proj_def_name not in all_defs:
        proj_cache[proj_def_name] = (None, None, None)
        return (None, None, None)

    path, elem, ns, _, _ = all_defs[proj_def_name]
    parent_name = get_parent_name(elem, ns)
    damage = None
    ap_raw = None
    damage_def_name = None

    proj_elem = find_direct(elem, "projectile")
    if proj_elem is not None:
        for c in proj_elem:
            tag = c.tag.split("}")[-1] if "}" in c.tag else c.tag
            if tag == "damageAmountBase":
                damage = get_float(c)
            elif tag == "armorPenetrationBase":
                ap_raw = get_float(c)
            elif tag == "damageDef":
                damage_def_name = get_text(c)

    if parent_name:
        p_damage, p_ap, p_dd = get_projectile_damage(parent_name, all_defs, proj_cache)
        if damage is None:
            damage = p_damage
        if ap_raw is None:
            ap_raw = p_ap
        if damage_def_name is None:
            damage_def_name = p_dd

    proj_cache[proj_def_name] = (damage, ap_raw, damage_def_name)
    return (damage, ap_raw, damage_def_name)


def collect_damagedefs():
    """Collect DamageDefs for defaultArmorPenetration lookup."""
    damage_defs = {}
    for base in RIMWORLD_SOURCES + [PROJECT_DEFS]:
        if not base.exists():
            continue
        for p in base.rglob("*.xml"):
            try:
                tree = ET.parse(p)
                root = tree.getroot()
            except ET.ParseError:
                continue
            ns = root.tag.split("}")[0] + "}" if "}" in root.tag else ""
            for elem in root.iter():
                if elem.tag.endswith("}DamageDef") or elem.tag == "DamageDef":
                    def_name = None
                    for c in elem:
                        tag = c.tag.split("}")[-1] if "}" in c.tag else c.tag
                        if tag == "defName":
                            def_name = get_text(c)
                            break
                    if not def_name:
                        def_name = elem.get("Name")
                    if def_name:
                        damage_defs[def_name] = (p, elem, ns)
    return damage_defs


def resolve_damagedef(def_name, all_damagedefs, cache):
    """Resolve DamageDef with parent inheritance. Returns dict with defaultArmorPenetration."""
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_damagedefs:
        cache[def_name] = {}
        return {}
    path, elem, ns = all_damagedefs[def_name]
    parent_name = elem.get("ParentName")
    base = {}
    if parent_name:
        base = resolve_damagedef(parent_name, all_damagedefs, cache)
    result = dict(base)
    for c in elem:
        tag = c.tag.split("}")[-1] if "}" in c.tag else c.tag
        if tag == "defaultArmorPenetration":
            result["defaultArmorPenetration"] = get_float(c)
    cache[def_name] = result
    return result


def compute_armor_penetration(damage, ap_raw, damage_def_name, all_damagedefs, dd_cache):
    """RimWorld ProjectileProperties.GetArmorPenetration 로직.
    ap_raw가 None(미지정) 또는 < 0이면: damageDef.defaultArmorPenetration 시도 후,
    여전히 < 0이면 damage * 0.015 사용.
    """
    if ap_raw is not None and ap_raw >= 0:
        return ap_raw
    default_ap = -1.0
    if damage_def_name and all_damagedefs:
        resolved = resolve_damagedef(damage_def_name, all_damagedefs, dd_cache)
        if "defaultArmorPenetration" in resolved:
            default_ap = resolved["defaultArmorPenetration"]
    if default_ap >= 0:
        return default_ap
    damage_val = damage if damage is not None else 0.0
    return round(damage_val * 0.015, 4)


def extract_weapon_data(def_name, all_defs, resolved_cache, proj_cache,
                       all_damagedefs=None, dd_cache=None):
    """Extract weapon stats. For turrets, use turretGunDef."""
    if def_name not in all_defs:
        return None
    path, elem, ns, rel, source_type = all_defs[def_name]

    building = extract_building(elem, ns)
    if building.get("turretGunDef"):
        return extract_weapon_data(
            building["turretGunDef"], all_defs, resolved_cache, proj_cache,
            all_damagedefs, dd_cache,
        )

    if not is_weapon_def(def_name, all_defs):
        return None

    verbs = extract_verbs(elem, ns)
    shoot_verb = None
    for v in verbs:
        vc = v.get("verbClass", "")
        if _is_shoot_verb(vc):
            shoot_verb = v
            break
    if not shoot_verb:
        return None

    default_proj = shoot_verb.get("defaultProjectile")
    if not default_proj:
        return None

    damage, ap_raw, damage_def_name = get_projectile_damage(
        default_proj, all_defs, proj_cache
    )
    if damage is None:
        damage = 0.0
    ap = compute_armor_penetration(
        damage, ap_raw, damage_def_name,
        all_damagedefs or {},
        dd_cache or {},
    )

    resolved = resolve_def(def_name, all_defs, resolved_cache)
    cooldown = get_float(resolved.get("stat_RangedWeapon_Cooldown")) or 0.0
    warmup = get_float(shoot_verb.get("warmupTime")) or 0.0
    burst = get_int(shoot_verb.get("burstShotCount")) or 1
    ticks_burst = get_int(shoot_verb.get("ticksBetweenBurstShots")) or DEFAULT_TICKS_BETWEEN_BURST

    # 명중률 (statBases, 부모 상속). 미지정 시 DEFAULT_ACCURACY
    acc = {}
    for stat_name in ACCURACY_STATS:
        key = f"stat_{stat_name}"
        val = get_float(resolved.get(key)) if key in resolved else DEFAULT_ACCURACY
        acc[stat_name] = val

    weapon_range = get_float(shoot_verb.get("range")) or 0.0

    ticks_burst_sec = ticks_burst / 60.0
    total_cycle = warmup + cooldown + (burst - 1) * ticks_burst_sec
    if total_cycle <= 0:
        dps = 0.0
    else:
        dps = (damage * burst) / total_cycle

    at, ash, am, al = acc["AccuracyTouch"], acc["AccuracyShort"], acc["AccuracyMedium"], acc["AccuracyLong"]

    dps_touch = round(dps * at, 2)
    dps_short = round(dps * ash, 2)
    dps_medium = round(dps * am, 2)
    dps_long = round(dps * al, 2)

    if weapon_range > 0:
        avg_acc = acc_weighted_avg(at, ash, am, al, weapon_range)
    else:
        avg_acc = (at + ash + am + al) / 4.0
    dps_weighted = round(dps * avg_acc, 2)

    return {
        "defName": def_name,
        "source": rel,
        "source_type": source_type,
        "burstShotCount": burst,
        "ticksBetweenBurstShots": ticks_burst,
        "ticksBetweenBurstShots_sec": round(ticks_burst_sec, 3),
        "RangedWeapon_Cooldown": cooldown,
        "warmupTime": warmup,
        "range": weapon_range,
        "damageAmountBase": damage,
        "armorPenetrationBase": ap,
        "totalCycleSec": round(total_cycle, 3),
        "DPS": round(dps, 2),
        "defaultProjectile": default_proj,
        "accTouch": at,
        "accShort": ash,
        "accMedium": am,
        "accLong": al,
        "accAvg": round(avg_acc, 4),
        "DPS_touch": dps_touch,
        "DPS_short": dps_short,
        "DPS_medium": dps_medium,
        "DPS_long": dps_long,
        "DPS_weighted": dps_weighted,
    }


def main():
    all_defs = collect_all_thingdefs()
    all_damagedefs = collect_damagedefs()
    turret_gun_defs = _collect_turret_gun_defs(all_defs)
    resolved_cache = {}
    proj_cache = {}
    dd_cache = {}
    comp_cache = {}

    weapons_by_source = defaultdict(list)
    for def_name in all_defs:
        if def_name == "_name_to_def":
            continue
        path, elem, ns, _, _ = all_defs[def_name]
        building = extract_building(elem, ns)
        if SKIP_TURRETS:
            if building.get("turretGunDef"):
                continue
            if def_name in turret_gun_defs:
                continue
            if _inherits_from(def_name, "BaseWeaponTurret", all_defs):
                continue
        if not _has_equippable_component(def_name, all_defs, comp_cache):
            continue
        w = extract_weapon_data(
            def_name, all_defs, resolved_cache, proj_cache,
            all_damagedefs, dd_cache,
        )
        if w is not None and w["damageAmountBase"] > 0:
            weapons_by_source[w["source_type"]].append(w)

    rimworld_weapons = []
    for st in ["Rimworld"]:
        rimworld_weapons.extend(weapons_by_source.get(st, []))
    project_weapons = weapons_by_source.get("Project", [])

    def source_to_dlc(path):
        if "Core" in path:
            return "Core"
        if "Royalty" in path:
            return "Royalty"
        if "Ideology" in path:
            return "Ideology"
        if "Biotech" in path:
            return "Biotech"
        if "Anomaly" in path:
            return "Anomaly"
        if "Odyssey" in path:
            return "Odyssey"
        return "Other"

    rimworld_by_dlc = defaultdict(list)
    for w in rimworld_weapons:
        dlc = source_to_dlc(w["source"])
        rimworld_by_dlc[dlc].append(w)

    return {
        "rimworld": rimworld_weapons,
        "rimworld_by_dlc": dict(rimworld_by_dlc),
        "project": project_weapons,
    }


def _acc_dps_cell(acc: float, dps_eff: float) -> str:
    """명중률(유효DPS) 형식: 0.75 (5.2)"""
    return f"{acc:.2f} ({dps_eff})"


def _weapon_row(weapon: dict, include_source: bool = False, include_accuracy: bool = True) -> list:
    """Build table row for weapon. burstSec in sec. No blank lines between rows."""
    burst_sec = _burst_sec(weapon["ticksBetweenBurstShots"])
    base = [
        weapon["defName"],
        weapon["burstShotCount"],
        burst_sec,
        weapon["RangedWeapon_Cooldown"],
        weapon["range"],
        weapon["damageAmountBase"],
        weapon["armorPenetrationBase"],
        weapon["DPS"],
        weapon["DPS_weighted"],
    ]
    if include_accuracy:
        base.extend([
            _acc_dps_cell(weapon["accTouch"], weapon["DPS_touch"]),
            _acc_dps_cell(weapon["accShort"], weapon["DPS_short"]),
            _acc_dps_cell(weapon["accMedium"], weapon["DPS_medium"]),
            _acc_dps_cell(weapon["accLong"], weapon["DPS_long"]),
        ])
    if include_source:
        base.insert(1, weapon["source"].replace("\\", "/"))
    return [str(x) for x in base]


def write_cache_md(result):
    out_path = ROOT / ".cursor" / "def-cache" / "ranged-weapons.md"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    sources = []
    for weapons in result["rimworld_by_dlc"].values():
        for w in weapons:
            if w["source"] not in sources:
                sources.append(w["source"])
    for w in result["project"]:
        if w["source"] not in sources:
            sources.append(w["source"])

    today = datetime.now().strftime("%Y-%m-%d")
    yaml = f"""---
category: ranged-weapons
last_updated: {today}
sources:
"""
    for s in sorted(sources)[:40]:
        yaml += f"  - {s.replace(chr(92), '/')}\n"
    yaml += """scope: Verb_Shoot/Verb_LaunchProjectile 원거리 무기, Equippable/Equipable 컴포넌트 보유(착용 가능)
fields: defName, burstShotCount, burstSec, cooldown, range, damage, AP, DPS, DPS_AVG, accTouch(DPS), accShort(DPS), accMedium(DPS), accLong(DPS)
---
"""

    sections = [yaml.strip(), "", "# 원거리 무기 (Ranged Weapons)", "",
                "## DPS 공식", "",
                "`totalCycleSec = warmupTime + cooldown + (burstShotCount - 1) * (ticksBetweenBurstShots / 60)`",
                "`DPS = (damageAmountBase * burstShotCount) / totalCycleSec`",
                "`DPS_AVG = DPS * acc̄` (거리 가중 적분 평균 명중률)",
                "`acc̄ = (1/R) * ∫₀ᴿ acc(d) dd` — [0, range] 구간에서 선형 보간 명중률을 적분 (사다리꼴 공식)",
                "고정 구간: Touch=3, Short=12, Medium=25, Long=40 타일 (ShootTuning.cs)",
                "AP: armorPenetrationBase 미지정 시 `damage * 0.015` (ProjectileProperties.GetArmorPenetration)",
                "상세: 30_Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md", "",
                "## 림월드 (Core + DLC)", ""]

    dlc_order = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
    for dlc in dlc_order:
        weapons = result["rimworld_by_dlc"].get(dlc, [])
        if not weapons:
            continue
        headers = ["defName", "burst", "burstSec", "cooldown", "range", "damage", "AP", "DPS", "DPS_AVG", "Touch", "Short", "Med", "Long"]
        rows = [_weapon_row(w) for w in weapons]
        sections.append(f"### {dlc} ({len(weapons)}개)")
        sections.append("")
        sections.append(_markdown_table(headers, rows))
        sections.append("")
        sections.append("")

    sections.append("## 랫킨 (Ratkin)")
    sections.append("")
    headers_rk = ["defName", "burst", "burstSec", "cooldown", "range", "damage", "AP", "DPS", "DPS_AVG", "Touch", "Short", "Med", "Long"]
    rows_rk = [_weapon_row(w) for w in result["project"]]
    sections.append(_markdown_table(headers_rk, rows_rk))

    out_path.write_text("\n".join(sections), encoding="utf-8")
    print(f"Wrote {out_path}")


def write_report(result):
    out_path = ROOT / "Report" / "Ranged_Weapon_Balancing_Report.md"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    today = datetime.now().strftime("%Y-%m-%d")
    lines = [
        "<!-- Ranged Weapon DPS Balancing Report Rimworld Ratkin -->",
        "",
        "# Ranged Weapon Balancing Report",
        "",
        "## 개요",
        "",
        f"- **생성일**: {today}",
        "- **데이터 소스**: RimworldData (Core, Royalty, Ideology, Biotech, Anomaly, Odyssey), Project/1.6/Defs",
        "- **포함 조건**: Verb_Shoot 또는 Verb_LaunchProjectile, Equippable/Equipable 컴포넌트 보유(부모 체인 포함), weaponClasses/thingCategories/weaponTags 보유, damageAmountBase > 0",
        "- **제외**: Verb_ShootBeam, Verb_Spray, Verb_SpewFire, 소모품(음료 등), 터렛 건물(turretGunDef 참조), 착용 불가(Equippable 미보유)",
        "",
        "## DPS 계산 공식",
        "",
        "```",
        "totalCycleSec = warmupTime + cooldown + (burstShotCount - 1) * (ticksBetweenBurstShots / 60)",
        "DPS = (damageAmountBase * burstShotCount) / totalCycleSec",
        "DPS_AVG = DPS × acc̄",
        "acc̄ = (1/R) × ∫₀ᴿ acc(d) dd   (R = weapon range)",
        "```",
        "",
        "- **명중률 보간**: 고정 구간 Touch=3, Short=12, Medium=25, Long=40 타일에서 선형 보간 (VerbProperties.GetHitChanceFactor)",
        "- **acc̄**: 사격 가능 거리 [0, R] 전체에서 acc(d)를 적분한 가중 평균 (사다리꼴 공식)",
        "- **ticksBetweenBurstShots**: 미지정 시 15 (VerbProperties.cs 기본값)",
        "- **burstShotCount**: 미지정 시 1",
        "- **AP**: armorPenetrationBase 미지정 시 damage × 0.015 (ProjectileProperties.GetArmorPenetration)",
                "- 상세: 30_Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md",
        "",
        "## 림월드 원거리 무기 요약",
        "",
    ]

    all_rimworld = []
    for dlc, weapons in result["rimworld_by_dlc"].items():
        all_rimworld.extend(weapons)
    lines.append("### 림월드 원거리 무기 (읽기 순서)")
    lines.append("")
    lines.append("| 순위 | defName | DPS | DPS_AVG | range | accT | accS | accM | accL | damage | AP |")
    lines.append("|------|---------|-----|--------|-------|------|------|------|------|--------|-----|")
    for i, w in enumerate(all_rimworld[:20], 1):
        lines.append(f"| {i} | {w['defName']} | {w['DPS']} | {w['DPS_weighted']} | {w['range']} | {w['accTouch']:.2f} | {w['accShort']:.2f} | {w['accMedium']:.2f} | {w['accLong']:.2f} | {w['damageAmountBase']} | {w['armorPenetrationBase']} |")
    lines.append("")

    lines.append("### DLC별 무기 수")
    lines.append("")
    for dlc in ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]:
        count = len(result["rimworld_by_dlc"].get(dlc, []))
        lines.append(f"- **{dlc}**: {count}개")
    lines.append("")

    lines.append("## 랫킨 원거리 무기")
    lines.append("")
    lines.append("| defName | DPS | DPS_AVG | range | accT | accS | accM | accL | damage | AP |")
    lines.append("|---------|-----|--------|-------|------|------|------|------|--------|-----|")
    for w in result["project"]:
        lines.append(f"| {w['defName']} | {w['DPS']} | {w['DPS_weighted']} | {w['range']} | {w['accTouch']:.2f} | {w['accShort']:.2f} | {w['accMedium']:.2f} | {w['accLong']:.2f} | {w['damageAmountBase']} | {w['armorPenetrationBase']} |")
    lines.append("")

    lines.append("## 전체 상세 데이터")
    lines.append("")
    lines.append("### 림월드 전체")
    lines.append("")
    lines.append("| defName | burst | cooldown | range | damage | AP | DPS | DPS_AVG | accAvg | accT | accS | accM | accL | defaultProjectile |")
    lines.append("|---------|-------|----------|-------|--------|-----|-----|--------|--------|------|------|------|------|-------------------|")
    for w in all_rimworld:
        lines.append(f"| {w['defName']} | {w['burstShotCount']} | {w['RangedWeapon_Cooldown']} | {w['range']} | {w['damageAmountBase']} | {w['armorPenetrationBase']} | {w['DPS']} | {w['DPS_weighted']} | {w['accAvg']} | {w['accTouch']:.2f} | {w['accShort']:.2f} | {w['accMedium']:.2f} | {w['accLong']:.2f} | {w['defaultProjectile']} |")
    lines.append("")

    lines.append("### 랫킨 전체")
    lines.append("")
    lines.append("| defName | burst | cooldown | range | damage | AP | DPS | DPS_AVG | accAvg | accT | accS | accM | accL | defaultProjectile |")
    lines.append("|---------|-------|----------|-------|--------|-----|-----|--------|--------|------|------|------|------|-------------------|")
    for w in result["project"]:
        lines.append(f"| {w['defName']} | {w['burstShotCount']} | {w['RangedWeapon_Cooldown']} | {w['range']} | {w['damageAmountBase']} | {w['armorPenetrationBase']} | {w['DPS']} | {w['DPS_weighted']} | {w['accAvg']} | {w['accTouch']:.2f} | {w['accShort']:.2f} | {w['accMedium']:.2f} | {w['accLong']:.2f} | {w['defaultProjectile']} |")
    lines.append("")

    if result["project"]:
        top_rim = max(all_rimworld, key=lambda x: x["DPS_weighted"]) if all_rimworld else None
        top_rk = max(result["project"], key=lambda x: x["DPS_weighted"])
        lines.append("## 밸런싱 참고")
        lines.append("")
        lines.append("- **림월드 최고 DPS_AVG**: " + (f"{top_rim['defName']} ({top_rim['DPS_weighted']})" if top_rim else "N/A"))
        lines.append("- **랫킨 최고 DPS_AVG**: " + f"{top_rk['defName']} ({top_rk['DPS_weighted']})")
        lines.append("- **제외된 무기**: 폭발물(로켓/박격포 등 damageAmountBase 없음), 빔/스프레이/화염(Verb_ShootBeam 등), 수류탄(explosion 기반)")
        lines.append("")

    out_path.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {out_path}")


def write_excel(result):
    import openpyxl
    from openpyxl.styles import Font, Alignment, PatternFill, Border, Side
    from openpyxl.utils import get_column_letter

    out_path = ROOT / "ExelData" / "Ranged_Weapon_Balancing.xlsx"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    wb = openpyxl.Workbook()

    header_font = Font(bold=True, size=11)
    header_fill = PatternFill(start_color="4472C4", end_color="4472C4", fill_type="solid")
    header_font_white = Font(bold=True, size=11, color="FFFFFF")
    thin_border = Border(
        left=Side(style="thin"), right=Side(style="thin"),
        top=Side(style="thin"), bottom=Side(style="thin"),
    )
    num_fmt_2 = "0.00"
    num_fmt_4 = "0.0000"

    headers = [
        "defName", "burst", "burstSec", "cooldown", "warmup", "range",
        "damage", "AP", "DPS", "DPS_AVG", "accAvg",
        "accTouch", "accShort", "accMedium", "accLong",
        "DPS_touch", "DPS_short", "DPS_medium", "DPS_long",
        "defaultProjectile",
    ]

    float_cols = {
        "burstSec", "cooldown", "warmup", "range", "damage", "AP",
        "DPS", "DPS_AVG", "accAvg",
        "accTouch", "accShort", "accMedium", "accLong",
        "DPS_touch", "DPS_short", "DPS_medium", "DPS_long",
    }
    acc_cols = {"accAvg", "accTouch", "accShort", "accMedium", "accLong"}

    def _weapon_to_row(w):
        return [
            w["defName"], w["burstShotCount"], _burst_sec(w["ticksBetweenBurstShots"]),
            w["RangedWeapon_Cooldown"], w["warmupTime"], w["range"],
            w["damageAmountBase"], w["armorPenetrationBase"],
            w["DPS"], w["DPS_weighted"], w["accAvg"],
            w["accTouch"], w["accShort"], w["accMedium"], w["accLong"],
            w["DPS_touch"], w["DPS_short"], w["DPS_medium"], w["DPS_long"],
            w["defaultProjectile"],
        ]

    def _write_sheet(ws, title, weapons):
        ws.title = title
        for col_idx, h in enumerate(headers, 1):
            cell = ws.cell(row=1, column=col_idx, value=h)
            cell.font = header_font_white
            cell.fill = header_fill
            cell.alignment = Alignment(horizontal="center")
            cell.border = thin_border

        for row_idx, w in enumerate(weapons, 2):
            vals = _weapon_to_row(w)
            for col_idx, val in enumerate(vals, 1):
                cell = ws.cell(row=row_idx, column=col_idx, value=val)
                cell.border = thin_border
                h = headers[col_idx - 1]
                if h in float_cols and isinstance(val, (int, float)):
                    cell.number_format = num_fmt_4 if h in acc_cols else num_fmt_2
                    cell.alignment = Alignment(horizontal="right")

        for col_idx in range(1, len(headers) + 1):
            col_letter = get_column_letter(col_idx)
            max_len = len(headers[col_idx - 1])
            for row_idx in range(2, len(weapons) + 2):
                val = ws.cell(row=row_idx, column=col_idx).value
                if val is not None:
                    max_len = max(max_len, len(str(val)))
            ws.column_dimensions[col_letter].width = min(max_len + 3, 40)

        ws.auto_filter.ref = f"A1:{get_column_letter(len(headers))}{len(weapons) + 1}"

    all_rimworld = []
    for weapons in result["rimworld_by_dlc"].values():
        all_rimworld.extend(weapons)

    _write_sheet(wb.active, "Rimworld", all_rimworld)

    ws_rk = wb.create_sheet()
    _write_sheet(ws_rk, "Ratkin", result["project"])

    dlc_order = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
    for dlc in dlc_order:
        weapons = result["rimworld_by_dlc"].get(dlc, [])
        if weapons:
            ws_dlc = wb.create_sheet()
            _write_sheet(ws_dlc, dlc, weapons)

    wb.save(out_path)
    print(f"Wrote {out_path}")


if __name__ == "__main__":
    result = main()
    write_cache_md(result)
    write_report(result)
    write_excel(result)

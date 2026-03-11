#!/usr/bin/env python3
"""
Parse apparel ThingDefs from RimworldData and Project/1.6/Defs.
Extract armor stats (Sharp/Blunt/Heat) and output cache MD + Excel.
전설 품질 기준, 소재별(로우엔드/하이엔드) 최종 방어도 계산.
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

# 품질 계수 (armor-rating-formula.md)
QUALITY_NORMAL = 1.0    # 평범 - LowEnd
QUALITY_LEGENDARY = 1.80  # 전설 - HighEnd

# 소재별 StuffPower_Armor (Sharp, Blunt, Heat) - armor-rating-formula.md 참조
STUFF_POWER = {
    "Steel": (0.90, 0.45, 0.60),
    "Plasteel": (1.14, 0.55, 0.65),
    "Leather_Plain": (0.81, 0.24, 1.50),
    "Leather_Thrumbo": (2.08, 0.36, 1.50),
    "Wood": (0.54, 0.54, 0.40),
    "Bioferrite": (1.10, 0.50, 0.50),  # Anomaly DLC
}


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


def extract_list(parent, tag):
    """Extract list of text from <tag><li>...</li></tag>."""
    elem = find_direct(parent, tag)
    if elem is None:
        return []
    items = []
    for c in elem:
        if c.tag.endswith("}li") or c.tag == "li":
            t = get_text(c)
            if t:
                items.append(t)
    return items


def collect_all_thingdefs():
    """Collect all ThingDefs from all source paths."""
    all_defs = {}
    all_files = []

    for base in RIMWORLD_SOURCES:
        if base.exists():
            for p in base.rglob("*.xml"):
                all_files.append((p, "Rimworld"))
    if PROJECT_DEFS.exists():
        for p in PROJECT_DEFS.rglob("*.xml"):
            all_files.append((p, "Project"))

    name_to_def = {}

    for path, source_type in all_files:
        for _, elem, ns in parse_defs_from_file(path):
            def_name = get_def_name(elem, ns)
            if not def_name:
                continue
            name_attr = elem.get("Name")
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


def resolve_stuff_categories(def_name, all_defs, cache):
    """Resolve stuffCategories with parent inheritance. Returns list or None."""
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = None
        return None

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_cats = None
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_cats = resolve_stuff_categories(actual_parent, all_defs, cache)

    sc_elem = find_direct(elem, "stuffCategories")
    if sc_elem is not None:
        my_cats = extract_list(elem, "stuffCategories")
        cache[def_name] = my_cats
        return my_cats
    cache[def_name] = parent_cats
    return parent_cats


def resolve_cost_stuff_count(def_name, all_defs, cache):
    """Resolve costStuffCount with parent inheritance. Returns int or None if absent in chain."""
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = None
        return None

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_val = None
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_val = resolve_cost_stuff_count(actual_parent, all_defs, cache)

    cost_elem = find_direct(elem, "costStuffCount")
    if cost_elem is not None:
        val = get_text(cost_elem)
        try:
            cache[def_name] = int(val) if val else 0
            return cache[def_name]
        except ValueError:
            cache[def_name] = 0
            return 0
    cache[def_name] = parent_val
    return parent_val


def _has_apparel_block(def_name, all_defs, cache=None):
    """Check if ThingDef has apparel block (self or parent)."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = False
        return False

    path, elem, ns, _, _ = all_defs[def_name]
    if find_direct(elem, "apparel") is not None:
        cache[def_name] = True
        return True
    parent_name = get_parent_name(elem, ns)
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        result = _has_apparel_block(actual_parent, all_defs, cache)
        cache[def_name] = result
        return result
    cache[def_name] = False
    return False


def _extract_from_apparel(elem, apparel_tag, ns):
    """Extract list from apparel/<tag> (e.g. layers, bodyPartGroups)."""
    apparel = find_direct(elem, "apparel")
    if apparel is None:
        return None
    return extract_list(apparel, apparel_tag)


def resolve_apparel_layers(def_name, all_defs, cache=None):
    """Resolve apparel.layers with parent inheritance. Returns list or None."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = None
        return None

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_layers = None
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_layers = resolve_apparel_layers(actual_parent, all_defs, cache)

    my_layers = _extract_from_apparel(elem, "layers", ns)
    if my_layers is not None:
        cache[def_name] = my_layers
        return my_layers
    cache[def_name] = parent_layers
    return parent_layers


def resolve_body_part_groups(def_name, all_defs, cache=None):
    """Resolve apparel.bodyPartGroups with parent inheritance. Inherit=False overrides."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = None
        return None

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_groups = None
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_groups = resolve_body_part_groups(actual_parent, all_defs, cache)

    apparel = find_direct(elem, "apparel")
    if apparel is None:
        cache[def_name] = parent_groups
        return parent_groups

    bpg_elem = find_direct(apparel, "bodyPartGroups")
    if bpg_elem is not None:
        my_groups = extract_list(apparel, "bodyPartGroups")
        inherit = bpg_elem.get("Inherit", "True")
        if inherit and inherit.lower() == "false":
            cache[def_name] = my_groups
            return my_groups
        cache[def_name] = my_groups if my_groups else parent_groups
        return my_groups if my_groups else parent_groups
    cache[def_name] = parent_groups
    return parent_groups


def resolve_thing_categories(def_name, all_defs, cache=None):
    """Resolve thingCategories with parent inheritance. Returns list or None."""
    if cache is None:
        cache = {}
    if def_name in cache:
        return cache[def_name]
    if def_name not in all_defs or def_name == "_name_to_def":
        cache[def_name] = None
        return None

    path, elem, ns, _, _ = all_defs[def_name]
    parent_name = get_parent_name(elem, ns)
    parent_cats = None
    if parent_name:
        actual_parent = _resolve_parent_name(parent_name, all_defs)
        parent_cats = resolve_thing_categories(actual_parent, all_defs, cache)

    tc_elem = find_direct(elem, "thingCategories")
    if tc_elem is not None:
        my_cats = extract_list(elem, "thingCategories")
        cache[def_name] = my_cats
        return my_cats
    cache[def_name] = parent_cats
    return parent_cats


def _is_belt_or_utility(def_name, all_defs, layers_cache, body_cache, thing_cat_cache):
    """True if def or any parent is belt/utility (layers=Belt, bodyPartGroups=Waist, or ApparelUtility)."""
    layers = resolve_apparel_layers(def_name, all_defs, layers_cache)
    if layers and "Belt" in layers:
        return True
    body_groups = resolve_body_part_groups(def_name, all_defs, body_cache)
    if body_groups and "Waist" in body_groups:
        return True
    thing_cats = resolve_thing_categories(def_name, all_defs, thing_cat_cache)
    if thing_cats and "ApparelUtility" in thing_cats:
        return True
    return False


def _is_face_item(def_name, all_defs, body_cache):
    """True if bodyPartGroups contains FullHead or Eyes (얼굴/눈 착용)."""
    body_groups = resolve_body_part_groups(def_name, all_defs, body_cache)
    if not body_groups:
        return False
    return "FullHead" in body_groups or "Eyes" in body_groups


def get_category_key(stuff_categories):
    """Constants 시트 VLOOKUP용 키. stuffCategory -> Metallic/Leathery/Woody/Bioferrite/Fixed"""
    if not stuff_categories:
        return "Fixed"
    cats = set(c.strip() for c in stuff_categories)
    if "Metallic" in cats:
        return "Metallic"
    if "Leathery" in cats or "Fabric" in cats:
        return "Leathery"
    if "Woody" in cats:
        return "Woody"
    if "Bioferrite" in cats:
        return "Bioferrite"
    return "Fixed"


def get_stuff_materials(stuff_categories):
    """
    Determine low-end and high-end materials from stuffCategories.
    Returns (low_name, high_name) or (None, None) for fixed material.
    """
    if not stuff_categories:
        return None, None

    cats = set(c.strip() for c in stuff_categories)
    if "Metallic" in cats:
        return "Steel", "Plasteel"
    if "Leathery" in cats or "Fabric" in cats:
        return "Leather_Plain", "Leather_Thrumbo"
    if "Woody" in cats:
        return "Wood", "Wood"
    if "Bioferrite" in cats:
        return "Bioferrite", "Bioferrite"
    return "Leather_Plain", "Leather_Thrumbo"


def compute_armor(base_armor, stuff_mult, stuff_power, quality_factor):
    """(BaseArmor + StuffEffectMultiplierArmor * StuffPower) * QualityFactor"""
    if stuff_power is None:
        return round(base_armor * quality_factor, 2)
    contrib = stuff_mult * stuff_power
    return round((base_armor + contrib) * quality_factor, 2)


def extract_apparel_data(def_name, all_defs, resolved_cache, stuff_cat_cache, cost_cache,
                        layers_cache, body_cache, thing_cat_cache):
    """Extract apparel armor data. Returns dict or None if not apparel."""
    if def_name not in all_defs or def_name == "_name_to_def":
        return None

    path, elem, ns, rel, source_type = all_defs[def_name]
    if elem.get("Abstract") == "True":
        return None

    if not _has_apparel_block(def_name, all_defs):
        return None

    # belt/utility 계열 제외 (부모 노드가 belt 기반이면 제외)
    if _is_belt_or_utility(def_name, all_defs, layers_cache, body_cache, thing_cat_cache):
        return None

    resolved = resolve_def(def_name, all_defs, resolved_cache)
    stuff_cats = resolve_stuff_categories(def_name, all_defs, stuff_cat_cache)
    cost_stuff = resolve_cost_stuff_count(def_name, all_defs, cost_cache)

    # 고정 소재: stuffCategories와 costStuffCount 둘 다 없어야 함 (부모 체인 포함)
    is_fixed = (stuff_cats is None or len(stuff_cats) == 0) and cost_stuff is None

    base_sharp = get_float(resolved.get("stat_ArmorRating_Sharp"))
    base_blunt = get_float(resolved.get("stat_ArmorRating_Blunt"))
    base_heat = get_float(resolved.get("stat_ArmorRating_Heat"))
    stuff_mult = get_float(resolved.get("stat_StuffEffectMultiplierArmor"))

    # 부모 체인에서 ArmorRating/StuffEffectMultiplierArmor 상속 (resolve_def가 이미 처리)

    if is_fixed:
        stuff_mult = 0.0
        ls, lb, lh = 0, 0, 0
        hs, hb, hh = 0, 0, 0
        low_sharp = compute_armor(base_sharp, 0, None, QUALITY_NORMAL)
        high_sharp = compute_armor(base_sharp, 0, None, QUALITY_LEGENDARY)
        low_blunt = compute_armor(base_blunt, 0, None, QUALITY_NORMAL)
        high_blunt = compute_armor(base_blunt, 0, None, QUALITY_LEGENDARY)
        low_heat = compute_armor(base_heat, 0, None, QUALITY_NORMAL)
        high_heat = compute_armor(base_heat, 0, None, QUALITY_LEGENDARY)
        stuff_category_str = "Fixed"
        low_material = high_material = "-"
    else:
        low_mat, high_mat = get_stuff_materials(stuff_cats)
        stuff_category_str = ",".join(stuff_cats) if stuff_cats else "-"

        def _power(name):
            return STUFF_POWER.get(name, (0, 0, 0))

        ls, lb, lh = _power(low_mat)
        hs, hb, hh = _power(high_mat)

        low_sharp = compute_armor(base_sharp, stuff_mult, ls, QUALITY_NORMAL)
        low_blunt = compute_armor(base_blunt, stuff_mult, lb, QUALITY_NORMAL)
        low_heat = compute_armor(base_heat, stuff_mult, lh, QUALITY_NORMAL)
        high_sharp = compute_armor(base_sharp, stuff_mult, hs, QUALITY_LEGENDARY)
        high_blunt = compute_armor(base_blunt, stuff_mult, hb, QUALITY_LEGENDARY)
        high_heat = compute_armor(base_heat, stuff_mult, hh, QUALITY_LEGENDARY)
        low_material = low_mat or "-"
        high_material = high_mat or "-"

    return {
        "defName": def_name,
        "source": rel,
        "source_type": source_type,
        "stuffCategory": stuff_category_str,
        "categoryKey": get_category_key(stuff_cats) if not is_fixed else "Fixed",
        "lowMaterial": low_material,
        "highMaterial": high_material,
        "StuffEffectMultiplierArmor": round(stuff_mult, 2),
        "BaseArmor_Sharp": round(base_sharp, 2),
        "BaseArmor_Blunt": round(base_blunt, 2),
        "BaseArmor_Heat": round(base_heat, 2),
        "LowEnd_Sharp": low_sharp,
        "LowEnd_Blunt": low_blunt,
        "LowEnd_Heat": low_heat,
        "HighEnd_Sharp": high_sharp,
        "HighEnd_Blunt": high_blunt,
        "HighEnd_Heat": high_heat,
    }


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


def main():
    all_defs = collect_all_thingdefs()
    resolved_cache = {}
    stuff_cat_cache = {}
    cost_cache = {}
    layers_cache = {}
    body_cache = {}
    thing_cat_cache = {}

    apparel_by_source = defaultdict(list)
    for def_name in all_defs:
        if def_name == "_name_to_def":
            continue
        data = extract_apparel_data(
            def_name, all_defs, resolved_cache, stuff_cat_cache, cost_cache,
            layers_cache, body_cache, thing_cat_cache,
        )
        if data is not None:
            apparel_by_source[data["source_type"]].append(data)

    rimworld_apparel = apparel_by_source.get("Rimworld", [])
    project_apparel = apparel_by_source.get("Project", [])

    # 얼굴 착용(FullHead, Eyes) vs 몸통/기타 분리
    def _split_face(items):
        face_items = []
        body_items = []
        for a in items:
            if _is_face_item(a["defName"], all_defs, body_cache):
                face_items.append(a)
            else:
                body_items.append(a)
        return body_items, face_items

    rimworld_body, rimworld_face = _split_face(rimworld_apparel)
    project_body, project_face = _split_face(project_apparel)

    rimworld_by_dlc = defaultdict(list)
    for a in rimworld_body:
        dlc = source_to_dlc(a["source"])
        rimworld_by_dlc[dlc].append(a)

    rimworld_face_by_dlc = defaultdict(list)
    for a in rimworld_face:
        dlc = source_to_dlc(a["source"])
        rimworld_face_by_dlc[dlc].append(a)

    return {
        "rimworld": rimworld_body,
        "rimworld_by_dlc": dict(rimworld_by_dlc),
        "project": project_body,
        "rimworld_face": rimworld_face,
        "rimworld_face_by_dlc": dict(rimworld_face_by_dlc),
        "project_face": project_face,
    }


def _markdown_table(headers, rows):
    def row_str(cells):
        return "| " + " | ".join(str(c) for c in cells) + " |"

    sep = "|" + "|".join(["---"] * len(headers)) + "|"
    return "\n".join([row_str(headers), sep] + [row_str(r) for r in rows])


def _apparel_row(a):
    """MD 캐시용 (계산된 LowEnd/HighEnd 포함)."""
    return [
        a["defName"],
        a["stuffCategory"],
        a["StuffEffectMultiplierArmor"],
        a["BaseArmor_Sharp"],
        a["BaseArmor_Blunt"],
        a["BaseArmor_Heat"],
        a["LowEnd_Sharp"],
        a["LowEnd_Blunt"],
        a["LowEnd_Heat"],
        a["HighEnd_Sharp"],
        a["HighEnd_Blunt"],
        a["HighEnd_Heat"],
    ]


def _excel_row_values(a):
    """엑셀용: 수식 제외 값만 (StuffPower는 Constants에서 VLOOKUP)."""
    return [
        a["defName"],
        a["stuffCategory"],
        a["categoryKey"],
        a["StuffEffectMultiplierArmor"],
        a["BaseArmor_Sharp"],
        a["BaseArmor_Blunt"],
        a["BaseArmor_Heat"],
    ]


def write_cache_md(result):
    out_path = ROOT / ".cursor" / "def-cache" / "apparel-armor.md"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    today = datetime.now().strftime("%Y-%m-%d")
    yaml = f"""---
category: apparel-armor
last_updated: {today}
sources: RimworldData (Core, Royalty, Ideology, Biotech, Anomaly, Odyssey), Project/1.6/Defs
scope: 방어구(Apparel) 방어력 Sharp/Blunt/Heat, 전설 품질 기준. belt/utility 제외, 얼굴착용은 별도파일
fields: defName, stuffCategory, StuffEffectMultiplierArmor, BaseArmor, LowEnd, HighEnd (Sharp/Blunt/Heat)
formula: (BaseArmor + StuffEffectMultiplierArmor × StuffPower) × 1.80
keywords: Apparel Armor Sharp Blunt Heat Legendary Steel Plasteel Leather_Plain Leather_Thrumbo
---
"""

    headers = [
        "defName",
        "stuffCategory",
        "StuffEffectMult",
        "Base_S",
        "Base_B",
        "Base_H",
        "Low_S",
        "Low_B",
        "Low_H",
        "High_S",
        "High_B",
        "High_H",
    ]

    sections = [
        yaml.strip(),
        "",
        "# 방어구 (Apparel) 방어력",
        "",
        "## 공식",
        "",
        "`최종 방어력 = (BaseArmor + StuffEffectMultiplierArmor × StuffPower) × QualityFactor`",
        "",
        "- **로우엔드**: 평범 품질(1.0) | Metallic→Steel, Leathery/Fabric→Leather_Plain, Woody→Wood",
        "- **하이엔드**: 전설 품질(1.80) | Metallic→Plasteel, Leathery/Fabric→Leather_Thrumbo, Woody→Wood",
        "- **고정 소재**: stuffCategories·costStuffCount 없음",
        "",
        "## 림월드 (Core + DLC)",
        "",
    ]

    dlc_order = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
    for dlc in dlc_order:
        items = result["rimworld_by_dlc"].get(dlc, [])
        if not items:
            continue
        rows = [[str(x) for x in _apparel_row(a)] for a in items]
        sections.append(f"### {dlc} ({len(items)}개)")
        sections.append("")
        sections.append(_markdown_table(headers, rows))
        sections.append("")
        sections.append("")

    sections.append("## 랫킨 (Ratkin)")
    sections.append("")
    rows_rk = [[str(x) for x in _apparel_row(a)] for a in result["project"]]
    sections.append(_markdown_table(headers, rows_rk))

    out_path.write_text("\n".join(sections), encoding="utf-8")
    print(f"Wrote {out_path}")


def write_cache_face_md(result):
    """얼굴 착용(FullHead, Eyes) 방어구 별도 파일."""
    out_path = ROOT / ".cursor" / "def-cache" / "apparel-armor-face.md"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    today = datetime.now().strftime("%Y-%m-%d")
    yaml = f"""---
category: apparel-armor-face
last_updated: {today}
sources: RimworldData (Core, Royalty, Ideology, Biotech, Anomaly, Odyssey), Project/1.6/Defs
scope: 얼굴/눈 착용 방어구 (bodyPartGroups: FullHead, Eyes) - 헬멧, 마스크, 고글 등
fields: defName, stuffCategory, StuffEffectMultiplierArmor, BaseArmor, LowEnd, HighEnd (Sharp/Blunt/Heat)
formula: (BaseArmor + StuffEffectMultiplierArmor × StuffPower) × 1.80
keywords: Apparel Armor Face Helmet Mask Eyes FullHead
---
"""

    headers = [
        "defName",
        "stuffCategory",
        "StuffEffectMult",
        "Base_S",
        "Base_B",
        "Base_H",
        "Low_S",
        "Low_B",
        "Low_H",
        "High_S",
        "High_B",
        "High_H",
    ]

    sections = [
        yaml.strip(),
        "",
        "# 얼굴 착용 방어구 (Face/Head Apparel)",
        "",
        "bodyPartGroups가 FullHead 또는 Eyes인 아이템 (헬멧, 마스크, 고글 등)",
        "",
        "## 림월드 (Core + DLC)",
        "",
    ]

    dlc_order = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
    for dlc in dlc_order:
        items = result["rimworld_face_by_dlc"].get(dlc, [])
        if not items:
            continue
        rows = [[str(x) for x in _apparel_row(a)] for a in items]
        sections.append(f"### {dlc} ({len(items)}개)")
        sections.append("")
        sections.append(_markdown_table(headers, rows))
        sections.append("")
        sections.append("")

    sections.append("## 랫킨 (Ratkin)")
    sections.append("")
    rows_rk = [[str(x) for x in _apparel_row(a)] for a in result["project_face"]]
    sections.append(_markdown_table(headers, rows_rk))

    out_path.write_text("\n".join(sections), encoding="utf-8")
    print(f"Wrote {out_path}")


def write_excel(result):
    import openpyxl
    from openpyxl.styles import Font, Alignment, PatternFill, Border, Side
    from openpyxl.utils import get_column_letter

    out_path = ROOT / "ExelData" / "Apparel_Armor_Balancing.xlsx"
    out_path.parent.mkdir(parents=True, exist_ok=True)

    wb = openpyxl.Workbook()
    header_fill = PatternFill(start_color="4472C4", end_color="4472C4", fill_type="solid")
    header_font_white = Font(bold=True, size=11, color="FFFFFF")
    thin_border = Border(
        left=Side(style="thin"),
        right=Side(style="thin"),
        top=Side(style="thin"),
        bottom=Side(style="thin"),
    )
    num_fmt = "0.00"
    quality_ref_low = "Constants!$B$2"   # QualityNormal (평범) - LowEnd
    quality_ref_high = "Constants!$B$1"  # QualityLegendary (전설) - HighEnd
    # Constants! StuffPower 테이블: A6:G11 (CategoryKey, Low_S, Low_B, Low_H, High_S, High_B, High_H)
    stuff_lookup = "Constants!$A$6:$G$11"

    headers = [
        "defName",
        "stuffCategory",
        "categoryKey",
        "StuffEffectMult",
        "Base_S",
        "Base_B",
        "Base_H",
        "LowEnd_S",
        "LowEnd_B",
        "LowEnd_H",
        "HighEnd_S",
        "HighEnd_B",
        "HighEnd_H",
    ]
    value_float_cols = {"StuffEffectMult", "Base_S", "Base_B", "Base_H"}

    def _formula_with_vlookup(row, col_base, col_mult, col_key, vlookup_col, quality_ref):
        """(Base + Mult * VLOOKUP(categoryKey)) * Quality 수식. vlookup_col: 2=Low_S, 3=Low_B, 4=Low_H, 5=High_S, 6=High_B, 7=High_H"""
        b = get_column_letter(col_base)
        c = get_column_letter(col_mult)
        k = get_column_letter(col_key)
        return f"=ROUND(({b}{row}+{c}{row}*VLOOKUP({k}{row},{stuff_lookup},{vlookup_col},0))*{quality_ref},2)"

    def _write_sheet(ws, title, items):
        ws.title = title[:31]
        for col_idx, h in enumerate(headers, 1):
            cell = ws.cell(row=1, column=col_idx, value=h)
            cell.font = header_font_white
            cell.fill = header_fill
            cell.alignment = Alignment(horizontal="center")
            cell.border = thin_border

        for row_idx, a in enumerate(items, 2):
            vals = _excel_row_values(a)
            for col_idx, val in enumerate(vals, 1):
                cell = ws.cell(row=row_idx, column=col_idx, value=val)
                cell.border = thin_border
                h = headers[col_idx - 1]
                if h in value_float_cols and isinstance(val, (int, float)):
                    cell.number_format = num_fmt
                    cell.alignment = Alignment(horizontal="right")

            # LowEnd: 평범(1.0) / HighEnd: 전설(1.8) 수식
            ws.cell(row=row_idx, column=8, value=_formula_with_vlookup(row_idx, 5, 4, 3, 2, quality_ref_low))
            ws.cell(row=row_idx, column=9, value=_formula_with_vlookup(row_idx, 6, 4, 3, 3, quality_ref_low))
            ws.cell(row=row_idx, column=10, value=_formula_with_vlookup(row_idx, 7, 4, 3, 4, quality_ref_low))
            ws.cell(row=row_idx, column=11, value=_formula_with_vlookup(row_idx, 5, 4, 3, 5, quality_ref_high))
            ws.cell(row=row_idx, column=12, value=_formula_with_vlookup(row_idx, 6, 4, 3, 6, quality_ref_high))
            ws.cell(row=row_idx, column=13, value=_formula_with_vlookup(row_idx, 7, 4, 3, 7, quality_ref_high))
            for c in range(8, 14):
                ws.cell(row=row_idx, column=c).number_format = num_fmt
                ws.cell(row=row_idx, column=c).alignment = Alignment(horizontal="right")
                ws.cell(row=row_idx, column=c).border = thin_border

        for col_idx in range(1, len(headers) + 1):
            col_letter = get_column_letter(col_idx)
            max_len = len(headers[col_idx - 1])
            for row_idx in range(2, len(items) + 2):
                val = ws.cell(row=row_idx, column=col_idx).value
                if val is not None:
                    max_len = max(max_len, len(str(val)))
            ws.column_dimensions[col_letter].width = min(max_len + 3, 40)

        ws.auto_filter.ref = f"A1:{get_column_letter(len(headers))}{len(items) + 1}"

    all_rimworld = []
    for items in result["rimworld_by_dlc"].values():
        all_rimworld.extend(items)

    _write_sheet(wb.active, "Rimworld", all_rimworld)

    ws_rk = wb.create_sheet()
    _write_sheet(ws_rk, "Ratkin", result["project"])

    dlc_order = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
    for dlc in dlc_order:
        items = result["rimworld_by_dlc"].get(dlc, [])
        if items:
            ws_dlc = wb.create_sheet()
            _write_sheet(ws_dlc, dlc, items)

    # 얼굴 착용 시트
    all_face = []
    for items in result["rimworld_face_by_dlc"].values():
        all_face.extend(items)
    all_face.extend(result["project_face"])
    if all_face:
        ws_face = wb.create_sheet()
        _write_sheet(ws_face, "Face", all_face)

    # Constants 시트: 품질 계수 + StuffPower 참조 테이블
    ws_const = wb.create_sheet("Constants")
    ws_const.cell(row=1, column=1, value="QualityLegendary")
    ws_const.cell(row=1, column=2, value=1.8)
    ws_const.cell(row=2, column=1, value="QualityNormal")
    ws_const.cell(row=2, column=2, value=1.0)
    ws_const.cell(row=3, column=1, value="(전설/평범 품질 계수, HighEnd/LowEnd)")
    # StuffPower 테이블 (categoryKey -> Low_S, Low_B, Low_H, High_S, High_B, High_H)
    stuff_headers = ["CategoryKey", "Low_S", "Low_B", "Low_H", "High_S", "High_B", "High_H"]
    for c, h in enumerate(stuff_headers, 1):
        ws_const.cell(row=6, column=c, value=h)
    stuff_data = [
        ("Metallic", 0.9, 0.45, 0.6, 1.14, 0.55, 0.65),
        ("Leathery", 0.81, 0.24, 1.5, 2.08, 0.36, 1.5),
        ("Woody", 0.54, 0.54, 0.4, 0.54, 0.54, 0.4),
        ("Bioferrite", 1.1, 0.5, 0.5, 1.1, 0.5, 0.5),
        ("Fixed", 0, 0, 0, 0, 0, 0),
    ]
    for r, row in enumerate(stuff_data, 7):
        for c, val in enumerate(row, 1):
            ws_const.cell(row=r, column=c, value=val)

    wb.save(out_path)
    print(f"Wrote {out_path}")


if __name__ == "__main__":
    result = main()
    write_cache_md(result)
    write_cache_face_md(result)
    write_excel(result)

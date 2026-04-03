# -*- coding: utf-8 -*-
"""ThingsDefs 내 shield defName XML 스캔 → JSON·표 (매 실행 시 Defs에서 재수집)."""
from __future__ import annotations

import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path

# =============================================================================
# 도탄 표 — 근접 스킬 반영 (아래만 고치면 됨)
# =============================================================================
#
# MELEE_DEFLECT_MODE
#   "multiply" — 유효 방어도(임계)에 곱:  final = base * f(melee)
#              예) f(0)=0.7, f(cap)=1.3  →  0스킬 ×70%, cap스킬 ×130%
#   "add"      — 유효 방어도에 가산(비율):  final = base + g(melee)
#              예) g(0)=0, g(cap)=0.3  →  0%~+30%p (0.30 = +30퍼센트 포인트)
#
# 스킬은 0 ~ MELEE_DEFLECT_SKILL_CAP 구간에서 선형 진행(t=melee/cap) 후,
# 곱 모드만 지수 보간: f = MULT_MIN * (MULT_MAX/MULT_MIN)^t  (log 스케일 곡선)
# 덧셈 모드는 선형: g = ADD_MIN + (ADD_MAX - ADD_MIN) * t
#
MELEE_DEFLECT_MODE = "add"  # "multiply" | "add"

MELEE_DEFLECT_SKILL_CAP = 20.0

# 곱 모드용 (MODE=="multiply")
MELEE_DEFLECT_MULT_MIN = 0.7
MELEE_DEFLECT_MULT_MAX = 1.3

# 덧셈 모드용 (MODE=="add") — 값은 0~1 비율 (0.3 = +30%p)
MELEE_DEFLECT_ADD_MIN = 0.0
MELEE_DEFLECT_ADD_MAX = 0.3

ROOT = Path(__file__).resolve().parents[1]
DEFS_DIR = ROOT / "Project" / "1.6" / "Defs"
_DATA_DIR = Path(__file__).resolve().parent
OUT_JSON = _DATA_DIR / "ratkin_tower_shield_rows.json"
OUT_TABLE = _DATA_DIR / "shield_deflect_tables.md"

SHIELD_DEFNAME = re.compile(r"shield", re.IGNORECASE)
ARMOR_KEYS = ("ArmorRating_Sharp", "ArmorRating_Blunt", "ArmorRating_Heat")

# 도탄 표 행 순서: 여기 이름 순서대로 먼저 나오고, 그 외 방패는 defName 알파벳순.
SHIELD_TABLE_DEF_ORDER = (
    "RK_SmallShield_Second",
    "RK_MediumShield_Second",
    "RK_TowerShield_Second",
)

STUFF = {
    "Steel": {"ArmorRating_Sharp": 0.9, "ArmorRating_Blunt": 0.45, "ArmorRating_Heat": 0.60},
    "Plasteel": {"ArmorRating_Sharp": 1.14, "ArmorRating_Blunt": 0.55, "ArmorRating_Heat": 0.65},
    "Leather_Plain": {"ArmorRating_Sharp": 0.81, "ArmorRating_Blunt": 0.24, "ArmorRating_Heat": 1.5},
    "Leather_Thrumbo": {"ArmorRating_Sharp": 2.08, "ArmorRating_Blunt": 0.36, "ArmorRating_Heat": 1.5},
}

QUALITY_FACTOR = {"Normal": 1.0, "Legendary": 1.8}

# 근접 무기 품질 → MeleeWeapon_DamageMultiplier (Core Defs/Stats/Stats_Weapons_Melee.xml StatPart_Quality)
MELEE_DMG_MULT_BY_QUALITY = {
    "Normal": 1.0,
    "Legendary": 1.65,
}

# 재료 날 피해 배율 — 플라스틸 (Core Items_Resource_Stuff SharpDamageMultiplier)
PLASTEEL_SHARP_DAMAGE_MULT = 1.1

# 참고 관통 AP: VerbProperties.AdjustedArmorPenetration — 명시 AP×근접피해배율,
# 미명시 시 (베이스 tool power)×SharpDamageMultiplier(재료)×MeleeDamageMultiplier(품질)×0.015
#
def melee_ap_plasteel_implicit(power: float, quality: str) -> float:
    """tool `power` 기준, 플라스틸·해당 품질 가정 시 근접 관통(암시 공식)."""
    return power * MELEE_DMG_MULT_BY_QUALITY[quality] * PLASTEEL_SHARP_DAMAGE_MULT * 0.015


# 표시 순서 = 아래 리스트 순
REF_PENETRATION_ROWS: list[tuple[str, float]] = [
    (
        "단분자검 전설·찌르기·날 (MeleeWeapon_MonoSword Stab/Cut, armorPenetration 0.9)",
        0.9 * MELEE_DMG_MULT_BY_QUALITY["Legendary"],
    ),
    (
        "RK_HeavyLance 플라스틸 전설·찌르기 (Stab, power 32)",
        melee_ap_plasteel_implicit(32.0, "Legendary"),
    ),
    (
        "RK_LongSword 정원사검 플라스틸 평범·찌르기 (Stab, power 23)",
        melee_ap_plasteel_implicit(23.0, "Normal"),
    ),
    (
        "RK_TwoHanded 양손검 플라스틸 평범·베기 (Cut, power 27)",
        melee_ap_plasteel_implicit(27.0, "Normal"),
    ),
    (
        "RK_HeavyLance 플라스틸 평범·찌르기 (Stab, power 32)",
        melee_ap_plasteel_implicit(32.0, "Normal"),
    ),
    (
        "장검 MeleeWeapon_LongSword 플라스틸 평범·찌르기 (Stab, power 23)",
        melee_ap_plasteel_implicit(23.0, "Normal"),
    ),
]

REF_ARMORS = [
    {
        "label": "림월드 판금갑옷 (Apparel_PlateArmor)",
        "sharp": 0.0, "blunt": 0.0, "heat": 0.0,
        "stuffMult": 0.9,
        "scenarios": [
            ("강철 평범", "Steel", "Normal"),
            ("플라스틸 전설", "Plasteel", "Legendary"),
        ],
    },
    {
        "label": "판금갑옷 (RK_Plate)",
        "sharp": 0.25, "blunt": 0.30, "heat": 0.0,
        "stuffMult": 0.63,
        "scenarios": [
            ("강철 평범", "Steel", "Normal"),
            ("플라스틸 전설", "Plasteel", "Legendary"),
        ],
    },
    {
        "label": "불사조갑옷 (Phoenix)",
        "sharp": 1.15, "blunt": 0.45, "heat": 0.75,
        "stuffMult": 0.0,
        "scenarios": [
            ("평범", None, "Normal"),
            ("전설", None, "Legendary"),
        ],
    },
    {
        "label": "정원사옷 (RK_GaurdenUniform)",
        "sharp": 0.0, "blunt": 0.40, "heat": 0.0,
        "stuffMult": 0.25,
        "scenarios": [
            ("평범가죽 평범", "Leather_Plain", "Normal"),
            ("트럼보가죽 전설", "Leather_Thrumbo", "Legendary"),
        ],
    },
]


def _melee_skill_t(melee_level: float | int) -> float:
    cap = MELEE_DEFLECT_SKILL_CAP
    m = float(melee_level)
    if cap <= 0:
        return 1.0
    if m <= 0:
        return 0.0
    if m >= cap:
        return 1.0
    return m / cap


def melee_deflect_mult_curve(t: float) -> float:
    """곱 모드 곡선: t∈[0,1] → MULT_MIN ~ MULT_MAX (지수 보간)."""
    lo = MELEE_DEFLECT_MULT_MIN
    hi = MELEE_DEFLECT_MULT_MAX
    if lo <= 0 or hi <= 0:
        return lo + t * (hi - lo)
    return lo * ((hi / lo) ** t)


def melee_deflect_add_curve(t: float) -> float:
    """덧셈 모드: t∈[0,1] → ADD_MIN ~ ADD_MAX (선형)."""
    lo = MELEE_DEFLECT_ADD_MIN
    hi = MELEE_DEFLECT_ADD_MAX
    return lo + t * (hi - lo)


def apply_melee_to_deflect_rate(base_rate: float, melee_level: float | int) -> float:
    """근접 스킬 반영 후 도탄 Rand 임계값."""
    t = _melee_skill_t(melee_level)
    if MELEE_DEFLECT_MODE == "add":
        return base_rate + melee_deflect_add_curve(t)
    return base_rate * melee_deflect_mult_curve(t)


def melee_deflect_legend_suffix(melee_level: float | int) -> str:
    """표 소제목용: 곱이면 배율, 덧셈이면 가산 %p."""
    t = _melee_skill_t(melee_level)
    if MELEE_DEFLECT_MODE == "add":
        b = melee_deflect_add_curve(t)
        return f"(가산 +{b * 100:.2f}%p)"
    m = melee_deflect_mult_curve(t)
    return f"(배율 {m:.3f})"


def shield_table_header_line() -> str:
    """README 한 줄 — 현재 MODE에 맞게."""
    cap = int(MELEE_DEFLECT_SKILL_CAP) if MELEE_DEFLECT_SKILL_CAP == int(MELEE_DEFLECT_SKILL_CAP) else MELEE_DEFLECT_SKILL_CAP
    if MELEE_DEFLECT_MODE == "add":
        return (
            f"ThingsDefs·shield defName 도탄 확률 (병합 statBases, 날/둔/열 모두 있을 때만 표 행; AP0, "
            f"Rand≤방어도+근접가산, 가산:0→+{MELEE_DEFLECT_ADD_MIN * 100:.0f}%p·{cap}→+{MELEE_DEFLECT_ADD_MAX * 100:.0f}%p 선형)"
        )
    return (
        f"ThingsDefs·shield defName 도탄 확률 (병합 statBases, 날/둔/열 모두 있을 때만 표 행; AP0, "
        f"Rand≤방어도×근접배율, 배율:0→×{MELEE_DEFLECT_MULT_MIN * 100:.0f}%·{cap}→×{MELEE_DEFLECT_MULT_MAX * 100:.0f}% 로그형)"
    )


def ref_penetration_caption() -> str:
    if MELEE_DEFLECT_MODE == "add":
        return "참고·상대 근접 관통 (도탄식 `Rand ≤ 방어도 + 근접가산 − AP` 에서 차감되는 AP; 전설·평범·플라스틸 등 예시)"
    return "참고·상대 근접 관통 (도탄식 `Rand ≤ 방어도×근접배율 − AP` 에서 차감되는 AP; 전설·평범·플라스틸 등 예시)"


def effective_armor(stat_key: str, base: float, stuff_powers: dict, stuff_mult: float, qfac: float) -> float:
    return (base + stuff_mult * stuff_powers[stat_key]) * qfac


def pct(rate: float) -> str:
    """Rand 기준값 rate 그대로 % (클램프 없음 — 관통 차감 후 실효는 rate−AP)."""
    return f"{rate * 100:.2f}%"


def parse_stat_bases(elem: ET.Element) -> dict[str, float]:
    out: dict[str, float] = {}
    sb = elem.find("statBases")
    if sb is None:
        return out
    for ch in sb:
        if ch.text is None:
            continue
        t = ch.text.strip()
        if not t:
            continue
        try:
            out[ch.tag] = float(t)
        except ValueError:
            pass
    return out


def text_child(elem: ET.Element, tag: str) -> str | None:
    el = elem.find(tag)
    if el is None or el.text is None:
        return None
    s = el.text.strip()
    return s or None


def is_first_part_things_defs(path: Path) -> bool:
    try:
        rel = path.relative_to(DEFS_DIR)
    except ValueError:
        return False
    return bool(rel.parts) and rel.parts[0] == "ThingsDefs"


def load_thing_nodes() -> list[dict]:
    nodes: list[dict] = []
    for path in sorted(DEFS_DIR.rglob("*.xml")):
        try:
            tree = ET.parse(path)
        except ET.ParseError:
            continue
        from_things = is_first_part_things_defs(path)
        for td in tree.getroot().findall("ThingDef"):
            name_attr = td.get("Name")
            parent = td.get("ParentName")
            abstract = str(td.get("Abstract", "")).lower() == "true"
            defn = text_child(td, "defName")
            tc = text_child(td, "thingClass")
            stats = parse_stat_bases(td)
            lab = text_child(td, "label")
            nodes.append(
                {
                    "file": str(path.relative_to(ROOT)),
                    "fromThingsDefs": from_things,
                    "name_attr": name_attr,
                    "defName": defn,
                    "parent": parent,
                    "abstract": abstract,
                    "thingClass": tc,
                    "label": lab,
                    "stats": stats,
                }
            )
    return nodes


def build_lookup(nodes: list[dict]) -> dict[str, dict]:
    lu: dict[str, dict] = {}
    for n in nodes:
        if n["name_attr"]:
            lu[n["name_attr"]] = n
        if n["defName"]:
            lu[n["defName"]] = n
    return lu


def resolve_chain(n: dict, lu: dict[str, dict]) -> list[dict]:
    chain: list[dict] = []
    seen: set[str] = set()
    cur: dict | None = n
    while cur is not None:
        key = cur["defName"] or cur["name_attr"] or ""
        if key in seen:
            break
        seen.add(key)
        chain.append(cur)
        p = cur.get("parent")
        cur = lu.get(p) if p else None
    return list(reversed(chain))


def merged_stats_and_class(chain: list[dict]) -> tuple[dict[str, float], str | None]:
    stats: dict[str, float] = {}
    for c in chain:
        stats.update(c["stats"])
    tclass: str | None = None
    for c in chain:
        if c.get("thingClass"):
            tclass = c["thingClass"]
    return stats, tclass


def resolved_label(n: dict, chain: list[dict]) -> str:
    if n.get("label"):
        return n["label"]
    for c in reversed(chain):
        if c.get("label"):
            return c["label"]
    return n["defName"] or ""


def collect_shield_rows_from_things_defs(nodes: list[dict]) -> list[dict]:
    """ThingsDefs 폴더 def, defName에 shield 포함, 추상 아님 → 상속 병합 후 hasFullArmor 판정."""
    lu = build_lookup(nodes)
    out: list[dict] = []
    for n in nodes:
        if not n["fromThingsDefs"]:
            continue
        if n["abstract"]:
            continue
        dfn = n.get("defName")
        if not dfn or not SHIELD_DEFNAME.search(dfn):
            continue
        chain = resolve_chain(n, lu)
        stats, tclass = merged_stats_and_class(chain)
        has_full = all(k in stats for k in ARMOR_KEYS)
        armor_present = [k for k in ARMOR_KEYS if k in stats]
        row = {
            "defName": dfn,
            "sourceFile": n["file"],
            "label": resolved_label(n, chain),
            "thingClass": tclass,
            "hasFullArmor": has_full,
            "armorKeysPresent": armor_present,
            "stats_merged": dict(sorted(stats.items())),
            "stuffMult": stats.get("StuffEffectMultiplierArmor", 0.0),
        }
        out.append(row)
    out.sort(key=lambda r: r["defName"].lower())
    return out


def sort_rows_for_deflect_table(rows: list[dict]) -> list[dict]:
    pri = {d: i for i, d in enumerate(SHIELD_TABLE_DEF_ORDER)}
    return sorted(rows, key=lambda r: (pri.get(r["defName"], 10_000), r["defName"].lower()))


def main() -> None:
    nodes = load_thing_nodes()
    rows = collect_shield_rows_from_things_defs(nodes)
    calc_rows = sort_rows_for_deflect_table([r for r in rows if r["hasFullArmor"]])

    _DATA_DIR.mkdir(parents=True, exist_ok=True)
    OUT_JSON.write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")

    lines: list[str] = [shield_table_header_line(), ""]

    scenarios = [
        ("강철 평범", "Steel", "Normal"),
        ("플라스틸 전설", "Plasteel", "Legendary"),
    ]
    for title_stuff, stuff_key, qkey in scenarios:
        qfac = QUALITY_FACTOR[qkey]
        for melee in (0, 20):
            suf = melee_deflect_legend_suffix(melee)
            lines.append(f"{title_stuff} 근접{melee} {suf}")
            lines.append("")
            lines.append("|ThingDef|Sharp|Blunt|Heat|")
            lines.append("|--------|-----|-----|----|")
            for r in calc_rows:
                mult = r["stuffMult"]
                st = r["stats_merged"]
                sps = STUFF[stuff_key]
                ps = pct(apply_melee_to_deflect_rate(effective_armor("ArmorRating_Sharp", st["ArmorRating_Sharp"], sps, mult, qfac), melee))
                pb = pct(apply_melee_to_deflect_rate(effective_armor("ArmorRating_Blunt", st["ArmorRating_Blunt"], sps, mult, qfac), melee))
                ph = pct(apply_melee_to_deflect_rate(effective_armor("ArmorRating_Heat", st["ArmorRating_Heat"], sps, mult, qfac), melee))
                lines.append(f"|{r['defName']}|{ps}|{pb}|{ph}|")
            lines.append("")

    if not calc_rows:
        lines.append("_(날카로움·둔탁·열 방어가 모두 병합된 stat에 없어 표 행 없음 — JSON에서 hasFullArmor=false 항목 확인)_")
        lines.append("")

    lines.append("---")
    lines.append("")
    lines.append(ref_penetration_caption())
    lines.append("")
    lines.append("|기준|관통|")
    lines.append("|---|---|")
    for label, ap in REF_PENETRATION_ROWS:
        lines.append(f"|{label}|{pct(ap)}|")
    lines.append("")

    lines.append("---")
    lines.append("")
    lines.append("참고·갑옷 방어도 (`(base + stuffMult × stuffPower) × quality`)")
    lines.append("")
    for arm in REF_ARMORS:
        lines.append(f"**{arm['label']}**")
        lines.append("")
        lines.append("|조건|Sharp|Blunt|Heat|")
        lines.append("|---|-----|-----|----|")
        for title, stuff_key, qkey in arm["scenarios"]:
            qfac = QUALITY_FACTOR[qkey]
            sm = arm["stuffMult"]
            if stuff_key is not None:
                sp = STUFF[stuff_key]
                s = (arm["sharp"] + sm * sp["ArmorRating_Sharp"]) * qfac
                b = (arm["blunt"] + sm * sp["ArmorRating_Blunt"]) * qfac
                h = (arm["heat"] + sm * sp["ArmorRating_Heat"]) * qfac
            else:
                s = arm["sharp"] * qfac
                b = arm["blunt"] * qfac
                h = arm["heat"] * qfac
            lines.append(f"|{title}|{pct(s)}|{pct(b)}|{pct(h)}|")
        lines.append("")

    text = "\n".join(lines).rstrip() + "\n"
    OUT_TABLE.write_text(text, encoding="utf-8")
    print(str(OUT_TABLE))


if __name__ == "__main__":
    main()

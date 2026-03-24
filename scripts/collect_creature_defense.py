"""
림월드 바닐라(Core + DLC) 생명체 방어 데이터 수집 스크립트
- ArmorRating (Sharp/Blunt/Heat), Flammability
- MeleeDodgeChance
- baseBodySize, baseHealthScale
- MoveSpeed, PsychicSensitivity
- 상속 체인 완전 해석 (Name + defName 이중 키)
"""

import xml.etree.ElementTree as ET
from pathlib import Path
from dataclasses import dataclass, field
import json
import datetime

BASE_DIR = Path(__file__).resolve().parent.parent
RIMWORLD_DATA = BASE_DIR / "RimworldData"

DLC_LIST = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]

STAT_FIELDS = [
    "ArmorRating_Sharp",
    "ArmorRating_Blunt",
    "ArmorRating_Heat",
    "Flammability",
    "MeleeDodgeChance",
    "MoveSpeed",
    "PsychicSensitivity",
]

RACE_FIELDS = [
    "baseBodySize",
    "baseHealthScale",
]


@dataclass
class CreatureDef:
    defName: str = ""
    xml_name: str = ""  # Name attribute (부모 참조용)
    parent_name: str = ""
    is_abstract: bool = False
    source_dlc: str = ""
    source_file: str = ""
    stat_bases: dict = field(default_factory=dict)
    race_props: dict = field(default_factory=dict)


def find_race_files(dlc: str) -> list[Path]:
    defs_dir = RIMWORLD_DATA / dlc / "Defs"
    if not defs_dir.exists():
        return []
    results = []
    race_dir = defs_dir / "ThingDefs_Races"
    if race_dir.exists():
        for f in race_dir.glob("Races_*.xml"):
            results.append(f)
    return sorted(results)


def parse_thingdefs(filepath: Path, dlc: str) -> list[CreatureDef]:
    creatures = []
    try:
        tree = ET.parse(filepath)
    except ET.ParseError as e:
        print(f"  [WARN] XML parse error in {filepath}: {e}")
        return creatures

    root = tree.getroot()
    for td in root.findall("ThingDef"):
        c = CreatureDef()
        c.source_dlc = dlc
        c.source_file = filepath.name
        c.defName = td.findtext("defName", "")
        c.xml_name = td.get("Name", "")
        c.parent_name = td.get("ParentName", "")
        c.is_abstract = td.get("Abstract", "").lower() == "true"

        if not c.defName and not c.xml_name:
            continue

        sb = td.find("statBases")
        if sb is not None:
            for stat in STAT_FIELDS:
                el = sb.find(stat)
                if el is not None and el.text:
                    c.stat_bases[stat] = el.text.strip()

        race = td.find("race")
        if race is not None:
            for rf in RACE_FIELDS:
                el = race.find(rf)
                if el is not None and el.text:
                    c.race_props[rf] = el.text.strip()

        if race is None and not c.is_abstract:
            continue

        creatures.append(c)
    return creatures


def resolve_inheritance(all_by_name: dict[str, CreatureDef]):
    """상속 체인을 따라 stat_bases와 race_props 병합"""
    resolved = set()

    def resolve(key: str, visited: set):
        if key in resolved or key not in all_by_name:
            return
        if key in visited:
            return
        visited.add(key)

        c = all_by_name[key]
        if c.parent_name and c.parent_name in all_by_name:
            resolve(c.parent_name, visited)
            parent = all_by_name[c.parent_name]
            merged_stats = dict(parent.stat_bases)
            merged_stats.update(c.stat_bases)
            c.stat_bases = merged_stats

            merged_race = dict(parent.race_props)
            merged_race.update(c.race_props)
            c.race_props = merged_race

        resolved.add(key)

    for key in list(all_by_name.keys()):
        resolve(key, set())


def collect_all() -> dict[str, list[CreatureDef]]:
    dlc_creatures: dict[str, list[CreatureDef]] = {}
    all_by_name: dict[str, CreatureDef] = {}

    for dlc in DLC_LIST:
        files = find_race_files(dlc)
        creatures = []
        for f in files:
            parsed = parse_thingdefs(f, dlc)
            creatures.extend(parsed)
        dlc_creatures[dlc] = creatures

        for c in creatures:
            if c.xml_name:
                all_by_name[c.xml_name] = c
            if c.defName and c.defName != c.xml_name:
                all_by_name[c.defName] = c

    resolve_inheritance(all_by_name)

    return dlc_creatures


def fv(val: str | None, default: str = "-") -> str:
    return val if val else default


def generate_markdown(dlc_creatures: dict[str, list[CreatureDef]]) -> str:
    today = datetime.date.today().isoformat()

    sources = []
    for dlc in DLC_LIST:
        if dlc_creatures.get(dlc):
            sources.append(f"RimworldData/{dlc}/Defs/ThingDefs_Races/")

    lines = [
        "---",
        "category: creature-defense",
        f"last_updated: {today}",
        "sources:",
    ]
    for s in sources:
        lines.append(f"  - {s}")
    lines.extend([
        "scope: 림월드 바닐라(Core+DLC) 모든 생명체의 방어/피격 관련 데이터",
        "fields: defName, ArmorRating_Sharp, ArmorRating_Blunt, ArmorRating_Heat, Flammability, MeleeDodgeChance, baseBodySize, baseHealthScale, MoveSpeed, PsychicSensitivity",
        "---",
        "",
        "# 생명체 방어 데이터 (Creature Defense)",
        "",
        "## 수집 기준",
        "",
        "- **ArmorRating**: ThingDef statBases 값. 상속 체인 완전 해석. StatDef 기본값=0",
        "- **Flammability**: 발화성. StatDef 기본값=0, BasePawn=0.7, BaseMechanoid=0, BaseFleshbeast=1.25",
        "- **MeleeDodgeChance**: StatDef 기본값=0. 대부분 미지정 → 스킬 기반 계산 (Melee스킬x1 + Movingx18 + Sightx8 → postProcessCurve)",
        "- **baseBodySize**: 기본값=1. 피격 면적·근접 피해 보정에 영향",
        "- **baseHealthScale**: 기본값=1. 전체 HP 배율",
        "- **MoveSpeed**: 이동속도. StatDef 기본값=0",
        "- **PsychicSensitivity**: StatDef 기본값=1",
        "",
    ])

    header = "| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |"
    separator = "|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|"

    for dlc in DLC_LIST:
        creatures = dlc_creatures.get(dlc, [])
        concrete = [c for c in creatures if not c.is_abstract and c.defName]
        if not concrete:
            continue

        lines.append(f"## {dlc}")
        lines.append("")
        lines.append(header)
        lines.append(separator)

        concrete.sort(key=lambda c: c.defName)

        for c in concrete:
            row = "| {} | {} | {} | {} | {} | {} | {} | {} | {} | {} |".format(
                c.defName,
                fv(c.stat_bases.get("ArmorRating_Sharp"), "0"),
                fv(c.stat_bases.get("ArmorRating_Blunt"), "0"),
                fv(c.stat_bases.get("ArmorRating_Heat"), "0"),
                fv(c.stat_bases.get("Flammability"), "0"),
                fv(c.stat_bases.get("MeleeDodgeChance"), "0"),
                fv(c.race_props.get("baseBodySize"), "1"),
                fv(c.race_props.get("baseHealthScale"), "1"),
                fv(c.stat_bases.get("MoveSpeed"), "0"),
                fv(c.stat_bases.get("PsychicSensitivity"), "1"),
            )
            lines.append(row)

        lines.append("")

    lines.append("## StatDef 기본값 참고")
    lines.append("")
    lines.append("| StatDef | defaultBaseValue | 비고 |")
    lines.append("|---------|-----------------|------|")
    lines.append("| ArmorRating_Sharp | 0 | 장갑 미지정 생명체는 0 |")
    lines.append("| ArmorRating_Blunt | 0 | 장갑 미지정 생명체는 0 |")
    lines.append("| ArmorRating_Heat | 0 | BaseMechanoid에서 2.0 상속 |")
    lines.append("| Flammability | 0 (StatDef) / 0.7 (BasePawn) | BasePawn 상속. 메카노이드=0, Fleshbeast=1.25 |")
    lines.append("| MeleeDodgeChance | 0 | Melee스킬x1 + Moving x18 + Sight x8 → postProcessCurve |")
    lines.append("| PsychicSensitivity | 1.0 | StatDef 기본값 |")
    lines.append("")
    lines.append("## MeleeDodgeChance PostProcessCurve")
    lines.append("")
    lines.append("| 입력값 | 최종 회피율 |")
    lines.append("|--------|-----------|")
    lines.append("| 5 | 0% |")
    lines.append("| 20 | 30% |")
    lines.append("| 60 | 50% |")
    lines.append("")

    return "\n".join(lines)


def generate_json(dlc_creatures: dict[str, list[CreatureDef]]) -> str:
    output = {}
    for dlc in DLC_LIST:
        creatures = dlc_creatures.get(dlc, [])
        concrete = [c for c in creatures if not c.is_abstract and c.defName]
        if not concrete:
            continue
        output[dlc] = []
        for c in sorted(concrete, key=lambda x: x.defName):
            output[dlc].append({
                "defName": c.defName,
                "ArmorRating_Sharp": float(c.stat_bases.get("ArmorRating_Sharp", 0)),
                "ArmorRating_Blunt": float(c.stat_bases.get("ArmorRating_Blunt", 0)),
                "ArmorRating_Heat": float(c.stat_bases.get("ArmorRating_Heat", 0)),
                "Flammability": float(c.stat_bases["Flammability"]) if c.stat_bases.get("Flammability") else None,
                "MeleeDodgeChance": c.stat_bases.get("MeleeDodgeChance"),
                "baseBodySize": float(c.race_props["baseBodySize"]) if c.race_props.get("baseBodySize") else None,
                "baseHealthScale": float(c.race_props["baseHealthScale"]) if c.race_props.get("baseHealthScale") else None,
                "MoveSpeed": float(c.stat_bases["MoveSpeed"]) if c.stat_bases.get("MoveSpeed") else None,
                "PsychicSensitivity": float(c.stat_bases["PsychicSensitivity"]) if c.stat_bases.get("PsychicSensitivity") else None,
            })
    return json.dumps(output, indent=2, ensure_ascii=False)


def main():
    print("=" * 60)
    print("림월드 바닐라 생명체 방어 데이터 수집")
    print("=" * 60)

    dlc_creatures = collect_all()

    total = 0
    for dlc in DLC_LIST:
        creatures = dlc_creatures.get(dlc, [])
        concrete = [c for c in creatures if not c.is_abstract and c.defName]
        abstract = [c for c in creatures if c.is_abstract]
        print(f"\n[{dlc}] 구체: {len(concrete)}개, 추상: {len(abstract)}개")
        for c in sorted(concrete, key=lambda x: x.defName):
            sharp = c.stat_bases.get('ArmorRating_Sharp', '0')
            blunt = c.stat_bases.get('ArmorRating_Blunt', '0')
            flamm = c.stat_bases.get('Flammability', '-')
            bs = c.race_props.get('baseBodySize', '?')
            print(f"  {c.defName:30s} body={bs:5s} Sharp={sharp:5s} Blunt={blunt:5s} Flamm={flamm}")
        total += len(concrete)

    print(f"\n총 {total}개 생명체 수집 완료")

    cache_dir = BASE_DIR / ".cursor" / "def-cache"
    cache_dir.mkdir(parents=True, exist_ok=True)

    md_path = cache_dir / "creature-defense.md"
    md_path.write_text(generate_markdown(dlc_creatures), encoding="utf-8")
    print(f"\n캐시: {md_path}")

    json_path = cache_dir / "creature-defense.json"
    json_path.write_text(generate_json(dlc_creatures), encoding="utf-8")
    print(f"JSON: {json_path}")


if __name__ == "__main__":
    main()

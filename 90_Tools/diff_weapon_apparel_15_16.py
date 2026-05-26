"""Scan Ratkin Project/1.5 vs 1.6 Defs for ThingDef weapon/apparel defNames."""
import xml.etree.ElementTree as ET
from pathlib import Path


def text(el, tag):
    c = el.find(tag)
    return (c.text or "").strip() if c is not None and c.text else ""


def trade_tags(el):
    tt = el.find("tradeTags")
    if tt is None:
        return []
    return [li.text for li in tt.findall("li") if li.text]


def inherits_apparel(thing):
    """RimWorld merges parent defs; ElementTree only sees this node."""
    if thing.find("apparel") is not None:
        return True
    tc = text(thing, "thingClass").lower()
    if "apparel" in tc:
        return True
    parent = (thing.get("ParentName") or "").lower()
    tokens = (
        "apparel",
        "helmet",
        "hatmakeable",
        "armormachineable",
        "armorsmithable",
        "armorhelm",
        "shielddeflect",
        "cataphract",
        "backpackbase",
        "bannerbase",
    )
    return any(t in parent for t in tokens)


def classify_thing(thing):
    if thing.get("Abstract") == "True":
        return None
    dn_el = thing.find("defName")
    if dn_el is None or not (dn_el.text or "").strip():
        return None
    dn = dn_el.text.strip()
    cat = text(thing, "category")
    tc = text(thing, "thingClass")
    parent = thing.get("ParentName") or ""

    if cat == "Projectile":
        return None
    if tc == "Bullet":
        return None
    if cat in ("Attachment", "Ethereal", "Mote", "Filth", "Gas", "Plant", "Building", "Pawn"):
        return None
    if parent.endswith("AnimalThingBase") or parent.endswith("BasePawn"):
        return None
    if tc.startswith("Pawn") or "Corpse" in tc:
        return None

    if inherits_apparel(thing):
        return "apparel"

    if thing.find("ingestible") is not None:
        return None

    if "MakeableShellBase" in parent or parent.endswith("ShellBase"):
        return None
    tags = trade_tags(thing)
    if "MortarShell" in tags and thing.find("weaponTags") is None:
        return None
    if parent == "BaseBolt" or "BallistaBolt" in parent:
        return None
    if dn.startswith("Bullet_") or dn.startswith("Bolt_"):
        return None

    if thing.find("weaponTags") is not None:
        return "weapon"
    verbs = thing.find("verbs")
    if verbs is not None and len(verbs.findall("li")) > 0:
        return "weapon"
    if thing.find("tools") is not None and cat in ("", "Item"):
        return "weapon"

    return None


def scan_defs(root: Path):
    weapons, apparel = set(), set()
    for xml_path in root.rglob("*.xml"):
        try:
            tree = ET.parse(xml_path)
        except ET.ParseError as e:
            print("SKIP", xml_path, e)
            continue
        rt = tree.getroot()
        if rt.tag != "Defs":
            continue
        for thing in rt.findall("ThingDef"):
            kind = classify_thing(thing)
            if kind is None:
                continue
            dn = thing.find("defName").text.strip()
            if kind == "weapon":
                weapons.add(dn)
            else:
                apparel.add(dn)
    return weapons, apparel


def main():
    base = Path(__file__).resolve().parents[1] / "Project"
    w15, a15 = scan_defs(base / "1.5" / "Defs")
    w16, a16 = scan_defs(base / "1.6" / "Defs")

    new_w = sorted(w16 - w15)
    new_a = sorted(a16 - a15)
    removed_w = sorted(w15 - w16)
    removed_a = sorted(a15 - a16)

    out = Path(__file__).resolve().parent / "weapon_apparel_15_16_report.md"
    formal = (
        Path(__file__).resolve().parents[1]
        / "30_Report"
        / "141_Ratkin_1.5_vs_1.6_Weapon_Apparel_DefName_Report.md"
    )
    lines = [
        "# Ratkin `Project/1.5` vs `Project/1.6` — Weapon / Apparel `defName`",
        "",
        "**정식 보고서:** `30_Report/141_Ratkin_1.5_vs_1.6_Weapon_Apparel_DefName_Report.md`",
        "",
        "---",
        "",
        "XML `ThingDef` 기준: `<apparel>` 또는 부모 `ParentName`에 의류·방패·배낭 등 토큰이 있으면 의류, `<ingestible>`(음식·약물)은 제외, 투사체·탄·포탄·동물 등은 제외, `weaponTags` / `verbs`(사격) / `tools`(아이템 근접)로 무기로 분류.",
        "",
        "## 1.5 무기 (`defName`, 정렬)",
        "",
        *[f"- `{x}`" for x in sorted(w15)],
        "",
        "## 1.5 의류",
        "",
        *[f"- `{x}`" for x in sorted(a15)],
        "",
        "## 1.6에만 있는 `defName` (신규로 보이는 항목)",
        "",
        "### 무기",
        "",
        *[f"- `{x}`" for x in new_w],
        "",
        "### 의류",
        "",
        *[f"- `{x}`" for x in new_a],
        "",
        "## 1.5에만 있고 1.6 Defs에 없음 (제거·이름 변경 가능)",
        "",
        "### 무기",
        "",
        *[f"- `{x}`" for x in removed_w],
        "",
        "### 의류",
        "",
        *[f"- `{x}`" for x in removed_a],
        "",
    ]
    out.write_text("\n".join(lines), encoding="utf-8")

    print("=== 1.5 weapons count", len(w15))
    print("=== 1.5 apparel count", len(a15))
    print("=== 1.6 weapons count", len(w16))
    print("=== 1.6 apparel count", len(a16))
    print()
    print("=== NEW in 1.6 (weapons)", len(new_w))
    for x in new_w:
        print("  W", x)
    print()
    print("=== NEW in 1.6 (apparel)", len(new_a))
    for x in new_a:
        print("  A", x)
    print()
    print("=== In 1.5 but missing in 1.6 (weapons)", len(removed_w))
    for x in removed_w:
        print("  -W", x)
    print()
    print("=== In 1.5 but missing in 1.6 (apparel)", len(removed_a))
    for x in removed_a:
        print("  -A", x)
    print()
    print("Wrote", out)
    print("Formal report (update manually if lists change):", formal)


if __name__ == "__main__":
    main()

# -*- coding: utf-8 -*-
import os
import re
from collections import defaultdict

ROOT = os.path.join(os.path.dirname(__file__), "..", "Project", "1.5", "Defs")
research_to_items: dict[str, list[tuple[str, str, str]]] = defaultdict(list)


def walk_defs():
    for dirpath, _, filenames in os.walk(ROOT):
        for fn in filenames:
            if fn.endswith(".xml"):
                yield os.path.join(dirpath, fn)


def strip_xml_comments(s: str) -> str:
    out = []
    i = 0
    while i < len(s):
        if s.startswith("<!--", i):
            j = s.find("-->", i + 4)
            if j == -1:
                break
            i = j + 3
            continue
        out.append(s[i])
        i += 1
    return "".join(out)


for path in walk_defs():
    with open(path, encoding="utf-8") as f:
        text = strip_xml_comments(f.read())
    parts = re.split(r"(?=<ThingDef\b)", text)
    for chunk in parts:
        if not chunk.strip().startswith("<ThingDef"):
            continue
        head = chunk[:800]
        if re.search(r'Abstract\s*=\s*"True"', head) or "<Abstract>True</Abstract>" in head:
            continue
        dm = re.search(r"<defName>([^<]+)</defName>", chunk)
        lb = re.search(r"<label>([^<]+)</label>", chunk)
        if not dm or not lb:
            continue
        defname, label = dm.group(1).strip(), lb.group(1).strip()
        src = os.path.basename(path)
        for m in re.finditer(r"<researchPrerequisite>([^<]+)</researchPrerequisite>", chunk):
            research_to_items[m.group(1).strip()].append((label, defname, src))
        for m in re.finditer(
            r"<researchPrerequisites>\s*(.*?)</researchPrerequisites>", chunk, re.DOTALL
        ):
            inner = m.group(1)
            for li in re.finditer(r"<li>([^<]+)</li>", inner):
                research_to_items[li.group(1).strip()].append((label, defname, src))

for k in list(research_to_items.keys()):
    seen: set[tuple[str, str]] = set()
    uniq: list[tuple[str, str, str]] = []
    for t in research_to_items[k]:
        key = (t[1], k)
        if key in seen:
            continue
        seen.add(key)
        uniq.append(t)
    research_to_items[k] = sorted(uniq, key=lambda x: x[1])

RATKIN = [
    "PiercingWeapon",
    "MechanicalWeapon",
    "Ballista",
    "Advanced_BallistaBolt",
    "Bigmouse",
    "MicroOptical",
    "FlechetteBullet",
    "RatkinClothing",
    "HighClassClothing",
]

print("=== RATKIN ===")
for r in RATKIN:
    print(f"\n{r}\tcount={len(research_to_items.get(r, []))}")
    for label, dn, src in research_to_items.get(r, []):
        print(f"  {label}\t({dn})")

print("\n=== VANILLA_PREREQ_WITH_RK_CONTENT ===")
for r in sorted(set(research_to_items) - set(RATKIN)):
    print(f"\n{r}\tcount={len(research_to_items[r])}")
    for label, dn, src in research_to_items[r]:
        print(f"  {label}\t({dn})")

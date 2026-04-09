"""
Korean -> Korean_v2 언어 데이터 누락 키 병합 스크립트

사용법:
    python merge_korean_lang.py           # dry-run (미리보기)
    python merge_korean_lang.py --apply   # 실제 파일 수정
"""

import os
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict

# ─────────────────────────────────────────────
# 경로 설정
# ─────────────────────────────────────────────
SCRIPT_DIR  = os.path.dirname(os.path.abspath(__file__))
KOREAN      = os.path.join(SCRIPT_DIR, "Korean")
KOREAN_V2   = os.path.join(SCRIPT_DIR, "Korean_v2")

# ─────────────────────────────────────────────
# 폴더명 매핑: Korean → Korean_v2
# (좌측: Korean 폴더명, 우측: Korean_v2 폴더명)
# ─────────────────────────────────────────────
FOLDER_MAP = {
    "InteractionDefs":              "InteractionDef",
    "ThoughtDefs":                  "ThoughtDef",
    "AlienRace.AlienBackstoryDef":  "BackstoryDef",
    "AlienRace.ThingDef_AlienRace": "ThingDef",
}

# ─────────────────────────────────────────────
# XML 파싱 헬퍼
# ─────────────────────────────────────────────
def parse_language_data(filepath):
    """
    XML 파일에서 <LanguageData> 직계 자식 태그를 파싱.
    반환: { 태그명: (값, 원본파일경로) }
    """
    keys = {}
    try:
        tree = ET.parse(filepath)
        root = tree.getroot()
        for child in root:
            if child.tag is ET.Comment:
                continue
            tag = child.tag
            text = (child.text or "").strip()
            if tag not in keys:
                keys[tag] = (text, filepath)
    except ET.ParseError as e:
        print(f"  [WARN] XML 파싱 실패: {filepath}\n         {e}")
    return keys


def collect_category_keys(base_dir, sub="DefInjected"):
    """
    base_dir/sub/카테고리폴더/*.xml 을 모두 읽어
    { 카테고리명: {태그명: (값, 파일경로)} } 반환
    """
    category_map = defaultdict(dict)
    target = os.path.join(base_dir, sub)
    if not os.path.isdir(target):
        return category_map

    for cat in os.listdir(target):
        cat_dir = os.path.join(target, cat)
        if not os.path.isdir(cat_dir):
            continue
        for fname in os.listdir(cat_dir):
            if not fname.lower().endswith(".xml"):
                continue
            fpath = os.path.join(cat_dir, fname)
            for tag, val in parse_language_data(fpath).items():
                if tag not in category_map[cat]:
                    category_map[cat][tag] = val
    return category_map


def collect_keyed_keys(base_dir):
    """
    base_dir/Keyed/*.xml 모두 읽어 단일 딕셔너리로 반환.
    { 태그명: (값, 파일경로) }
    """
    keyed = {}
    target = os.path.join(base_dir, "Keyed")
    if not os.path.isdir(target):
        return keyed
    for fname in os.listdir(target):
        if not fname.lower().endswith(".xml"):
            continue
        fpath = os.path.join(target, fname)
        for tag, val in parse_language_data(fpath).items():
            if tag not in keyed:
                keyed[tag] = val
    return keyed


# ─────────────────────────────────────────────
# 파일 끝에 키 추가 (XML 구조 보존)
# ─────────────────────────────────────────────
def append_keys_to_file(filepath, missing_items, dry_run=True):
    """
    filepath 의 </LanguageData> 직전에 missing_items 를 삽입.
    missing_items: [(태그명, 값), ...]
    """
    if not missing_items:
        return

    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    lines_to_add = []
    for tag, val in missing_items:
        if val:
            lines_to_add.append(f"  <{tag}>{val}</{tag}>")
        else:
            lines_to_add.append(f"  <{tag}></{tag}>")

    insert_block = "\n".join(lines_to_add) + "\n"

    close_tag = "</LanguageData>"
    if close_tag not in content:
        print(f"  [WARN] </LanguageData> 없음, 건너뜀: {filepath}")
        return

    new_content = content.replace(close_tag, insert_block + close_tag, 1)

    if dry_run:
        return

    with open(filepath, "w", encoding="utf-8") as f:
        f.write(new_content)


# ─────────────────────────────────────────────
# 메인
# ─────────────────────────────────────────────
def main():
    dry_run = "--apply" not in sys.argv

    if dry_run:
        print("=" * 60)
        print("  [DRY-RUN] 실제 파일은 수정되지 않습니다.")
        print("  실제 적용: python merge_korean_lang.py --apply")
        print("=" * 60)
    else:
        print("=" * 60)
        print("  [APPLY] 파일을 실제로 수정합니다.")
        print("=" * 60)

    # ── 데이터 수집 ──────────────────────────
    ko_def   = collect_category_keys(KOREAN,    "DefInjected")
    v2_def   = collect_category_keys(KOREAN_V2, "DefInjected")
    ko_keyed = collect_keyed_keys(KOREAN)
    v2_keyed = collect_keyed_keys(KOREAN_V2)

    total_added  = 0
    total_skip   = 0
    warnings     = []

    # ── DefInjected 비교 ─────────────────────
    print("\n[DefInjected 비교]")

    for ko_cat, ko_keys in sorted(ko_def.items()):
        # 매핑 테이블 적용
        v2_cat = FOLDER_MAP.get(ko_cat, ko_cat)

        if v2_cat not in v2_def:
            msg = f"  [WARN] Korean_v2에 대응 폴더 없음: {ko_cat} -> {v2_cat} (무시됨)"
            warnings.append(msg)
            continue

        v2_keys = v2_def[v2_cat]
        missing = [(tag, val[0]) for tag, val in ko_keys.items() if tag not in v2_keys]

        if not missing:
            print(f"  [OK]   {ko_cat} ({len(ko_keys)}키) — 누락 없음")
            total_skip += len(ko_keys)
            continue

        print(f"  [MISS] {ko_cat} -> {v2_cat}: {len(missing)}개 누락")
        for tag, _ in missing:
            src_file = ko_keys[tag][1]
            print(f"         + {tag}  (출처: ...{os.sep}{os.path.basename(os.path.dirname(src_file))}{os.sep}{os.path.basename(src_file)})")

        # 대상 파일: Korean_v2/DefInjected/{v2_cat}/mnkexg.xml
        target_file = os.path.join(KOREAN_V2, "DefInjected", v2_cat, "mnkexg.xml")
        if not os.path.isfile(target_file):
            if dry_run:
                print(f"         -> [NEW] 파일 생성 예정: {target_file}")
                total_added += len(missing)
                continue
            else:
                os.makedirs(os.path.dirname(target_file), exist_ok=True)
                with open(target_file, "w", encoding="utf-8") as f:
                    f.write('<?xml version="1.0" encoding="utf-8"?>\n<LanguageData>\n</LanguageData>\n')
                print(f"         -> [NEW] 파일 생성: {target_file}")

        append_keys_to_file(target_file, missing, dry_run=dry_run)
        total_added += len(missing)

        if not dry_run:
            print(f"         -> 추가 완료: {target_file}")

    # ── Keyed 비교 ───────────────────────────
    print("\n[Keyed 비교]")

    keyed_missing = [(tag, val[0]) for tag, val in ko_keyed.items() if tag not in v2_keyed]

    if not keyed_missing:
        print(f"  [OK]   Keyed ({len(ko_keyed)}키) — 누락 없음")
        total_skip += len(ko_keyed)
    else:
        print(f"  [MISS] Keyed: {len(keyed_missing)}개 누락")
        for tag, _ in keyed_missing:
            src_file = ko_keyed[tag][1]
            print(f"         + {tag}  (출처: ...{os.sep}{os.path.basename(src_file)})")

        target_file = os.path.join(KOREAN_V2, "Keyed", "mnkexg.xml")
        if not os.path.isfile(target_file):
            if dry_run:
                print(f"         -> [NEW] 파일 생성 예정: {target_file}")
            else:
                os.makedirs(os.path.dirname(target_file), exist_ok=True)
                with open(target_file, "w", encoding="utf-8") as f:
                    f.write('<?xml version="1.0" encoding="utf-8"?>\n<LanguageData>\n</LanguageData>\n')
        append_keys_to_file(target_file, keyed_missing, dry_run=dry_run)
        total_added += len(keyed_missing)
        if not dry_run:
            print(f"         -> 추가 완료: {target_file}")

    # ── 요약 ─────────────────────────────────
    print("\n" + "=" * 60)
    print(f"  누락 키 추가: {total_added}개")
    print(f"  이미 존재  : {total_skip}개")

    if warnings:
        print("\n  [경고 목록]")
        for w in warnings:
            print(w)

    if dry_run and total_added > 0:
        print("\n  -> 실제 적용하려면:  python merge_korean_lang.py --apply")
    print("=" * 60)


if __name__ == "__main__":
    main()

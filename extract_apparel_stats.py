import os
import xml.etree.ElementTree as ET
from collections import defaultdict

# RimWorldData 경로
rimworld_data_path = "RimWorldData"

# statBases 수집
stat_names = set()
stat_examples = defaultdict(list)

# Apparel 파일들 찾기
apparel_files = []
for root, dirs, files in os.walk(rimworld_data_path):
    for file in files:
        if file.startswith("Apparel_") and file.endswith(".xml"):
            apparel_files.append(os.path.join(root, file))

print(f"총 {len(apparel_files)}개의 Apparel 파일 발견\n")

# 각 파일 파싱
for filepath in apparel_files:
    try:
        tree = ET.parse(filepath)
        root = tree.getroot()
        
        # ThingDef 찾기
        for thing_def in root.findall(".//ThingDef"):
            # statBases 찾기
            stat_bases = thing_def.find("statBases")
            if stat_bases is not None:
                for stat in stat_bases:
                    stat_name = stat.tag
                    stat_value = stat.text
                    stat_names.add(stat_name)
                    
                    # 예시 값 저장 (최대 3개까지)
                    if len(stat_examples[stat_name]) < 3:
                        def_name = thing_def.find("defName")
                        def_name_text = def_name.text if def_name is not None else "Unknown"
                        stat_examples[stat_name].append(f"{def_name_text}: {stat_value}")
    
    except Exception as e:
        print(f"오류 발생 ({filepath}): {e}")

# 결과 출력
print("=" * 80)
print("착용 가능한 장비(Apparel)에서 사용되는 statBases 항목들")
print("=" * 80)
print(f"\n총 {len(stat_names)}개의 고유 stat 발견\n")

# 알파벳 순으로 정렬하여 출력
for stat_name in sorted(stat_names):
    print(f"\n{stat_name}")
    print("-" * 40)
    for example in stat_examples[stat_name]:
        print(f"  예시: {example}")

# 간단한 목록만 출력
print("\n" + "=" * 80)
print("전체 목록 (복사용)")
print("=" * 80)
for stat_name in sorted(stat_names):
    print(stat_name)


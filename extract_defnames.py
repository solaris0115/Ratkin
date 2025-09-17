#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
XML 파싱을 통해 모든 defName 값들을 추출하고 타입별로 분류하는 스크립트
"""

import os
import xml.etree.ElementTree as ET
from pathlib import Path

def extract_defnames_from_xml(file_path):
    """XML 파일에서 Defs 루트의 직접 자식 요소들의 defName과 타입을 추출"""
    defnames_with_types = []
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()
        
        # Defs 루트의 직접 자식 요소들만 확인
        for child in root:
            if child.tag.endswith('Def'):  # ThingDef, RecipeDef 등
                # 각 Def 요소의 직접 자식 중 defName 찾기
                for elem in child:
                    if elem.tag == 'defName' and elem.text:
                        defname = elem.text.strip()
                        def_type = child.tag  # ThingDef, RecipeDef 등
                        defnames_with_types.append((defname, def_type))
                        break  # 첫 번째 defName만 가져오기
                
    except ET.ParseError as e:
        print(f"XML 파싱 오류 {file_path}: {e}")
    except Exception as e:
        print(f"파일 처리 오류 {file_path}: {e}")
    
    return defnames_with_types

def scan_defs_directory(defs_path):
    """Defs 디렉토리를 스캔하여 모든 XML 파일에서 defName과 타입 추출"""
    all_defnames_with_types = []
    
    if not os.path.exists(defs_path):
        print(f"디렉토리가 존재하지 않습니다: {defs_path}")
        return all_defnames_with_types
    
    # 모든 XML 파일 찾기
    xml_files = []
    for root, dirs, files in os.walk(defs_path):
        for file in files:
            if file.endswith('.xml'):
                xml_files.append(os.path.join(root, file))
    
    print(f"총 {len(xml_files)}개의 XML 파일을 찾았습니다.")
    print("찾은 XML 파일들:")
    for xml_file in xml_files:
        print(f"  {xml_file}")
    
    # 각 XML 파일에서 defName과 타입 추출
    for xml_file in xml_files:
        defnames_with_types = extract_defnames_from_xml(xml_file)
        if defnames_with_types:
            all_defnames_with_types.extend(defnames_with_types)
            print(f"{xml_file}: {len(defnames_with_types)}개 defName 발견")
    
    return all_defnames_with_types

def main():
    # Defs 디렉토리 경로
    defs_path = "Project/1.6/Defs"
    
    print("DefName 추출 및 타입 분류 시작...")
    all_defnames_with_types = scan_defs_directory(defs_path)
    
    # 중복 제거 및 정렬
    unique_defnames_with_types = sorted(list(set(all_defnames_with_types)))
    
    print(f"\n총 {len(all_defnames_with_types)}개의 defName을 찾았습니다.")
    print(f"중복 제거 후 {len(unique_defnames_with_types)}개의 고유한 defName이 있습니다.")
    
    # 타입별로 그룹화
    type_groups = {}
    for defname, def_type in unique_defnames_with_types:
        if def_type not in type_groups:
            type_groups[def_type] = []
        type_groups[def_type].append(defname)
    
    # 결과를 파일로 저장 (타입별로 분류)
    with open("defnames_typed.txt", "w", encoding="utf-8") as f:
        f.write("# DefName 타입별 분류 목록\n\n")
        for def_type in sorted(type_groups.keys()):
            f.write(f"## {def_type}\n")
            for defname in sorted(type_groups[def_type]):
                f.write(f"{def_type}:{defname}\n")
            f.write("\n")
    
    # 기존 형식으로도 저장
    with open("defnames_list.txt", "w", encoding="utf-8") as f:
        for defname, def_type in unique_defnames_with_types:
            f.write(f"{defname}\n")
    
    print("\n타입별 분류 결과:")
    for def_type in sorted(type_groups.keys()):
        print(f"{def_type}: {len(type_groups[def_type])}개")
        for defname in sorted(type_groups[def_type]):
            print(f"  {def_type}:{defname}")
    
    print(f"\n결과가 다음 파일들에 저장되었습니다:")
    print(f"- 'defnames_typed.txt' (타입별 분류)")
    print(f"- 'defnames_list.txt' (기존 형식)")

if __name__ == "__main__":
    main()

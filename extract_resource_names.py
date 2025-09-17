#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Textures와 Sounds 폴더에서 파일명들을 추출하고 타입별로 분류하는 스크립트
접두사(_south 등)와 확장자는 제거하고 기본 이름만 추출
"""

import os
import re
from pathlib import Path

def clean_filename(filename):
    """파일명에서 접두사와 확장자 제거"""
    # 확장자 제거
    name_without_ext = os.path.splitext(filename)[0]
    
    # 접두사 패턴들 제거 (_south, _north, _east, _west, _front, _back 등)
    patterns = [
        r'_south$', r'_north$', r'_east$', r'_west$',
        r'_front$', r'_back$', r'_left$', r'_right$',
        r'_up$', r'_down$', r'_top$', r'_bottom$',
        r'_side$', r'_main$', r'_alt$', r'_variant$',
        r'_1$', r'_2$', r'_3$', r'_4$', r'_5$',
        r'_A$', r'_B$', r'_C$', r'_D$', r'_E$'
    ]
    
    cleaned_name = name_without_ext
    for pattern in patterns:
        cleaned_name = re.sub(pattern, '', cleaned_name)
    
    return cleaned_name

def scan_directory(directory_path, resource_type):
    """디렉토리를 스캔하여 파일명들을 추출하고 타입 정보와 함께 반환"""
    filenames_with_types = []
    
    if not os.path.exists(directory_path):
        print(f"디렉토리가 존재하지 않습니다: {directory_path}")
        return filenames_with_types
    
    # 모든 파일 찾기 (하위 디렉토리 포함)
    for root, dirs, files in os.walk(directory_path):
        for file in files:
            # 이미지와 오디오 파일만 처리
            if file.lower().endswith(('.png', '.jpg', '.jpeg', '.gif', '.bmp', '.tga', '.ogg', '.wav', '.mp3')):
                cleaned_name = clean_filename(file)
                if cleaned_name:  # 빈 문자열이 아닌 경우만
                    filenames_with_types.append((cleaned_name, resource_type))
    
    return filenames_with_types

def main():
    print("리소스 파일명 추출 및 타입 분류 시작...")
    
    # Textures 폴더 스캔
    print("\n=== Textures 폴더 스캔 ===")
    textures_path = "Project/Textures"
    texture_names_with_types = scan_directory(textures_path, "texture")
    print(f"Textures에서 {len(texture_names_with_types)}개의 파일명을 찾았습니다.")
    
    # Sounds 폴더 스캔
    print("\n=== Sounds 폴더 스캔 ===")
    sounds_path = "Project/Sounds"
    sound_names_with_types = scan_directory(sounds_path, "sound")
    print(f"Sounds에서 {len(sound_names_with_types)}개의 파일명을 찾았습니다.")
    
    # 전체 리스트 합치기
    all_names_with_types = texture_names_with_types + sound_names_with_types
    
    # 중복 제거 및 정렬
    unique_names_with_types = sorted(list(set(all_names_with_types)))
    
    print(f"\n총 {len(all_names_with_types)}개의 파일명을 찾았습니다.")
    print(f"중복 제거 후 {len(unique_names_with_types)}개의 고유한 파일명이 있습니다.")
    
    # 타입별로 그룹화
    type_groups = {}
    for name, resource_type in unique_names_with_types:
        if resource_type not in type_groups:
            type_groups[resource_type] = []
        type_groups[resource_type].append(name)
    
    # 결과를 파일로 저장 (타입별로 분류)
    with open("resource_names_typed.txt", "w", encoding="utf-8") as f:
        f.write("# Resource Name 타입별 분류 목록\n\n")
        for resource_type in sorted(type_groups.keys()):
            f.write(f"## {resource_type.title()} ({resource_type}의 첫 글자를 대문자로)\n")
            for name in sorted(type_groups[resource_type]):
                f.write(f"{resource_type}:{name}\n")
            f.write("\n")
    
    # 기존 형식으로도 저장
    with open("resource_names.txt", "w", encoding="utf-8") as f:
        f.write("=== Textures ===\n")
        texture_names = [name for name, resource_type in unique_names_with_types if resource_type == "texture"]
        for name in sorted(set(texture_names)):
            f.write(f"{name}\n")
        
        f.write("\n=== Sounds ===\n")
        sound_names = [name for name, resource_type in unique_names_with_types if resource_type == "sound"]
        for name in sorted(set(sound_names)):
            f.write(f"{name}\n")
        
        f.write(f"\n=== All ({len(unique_names_with_types)}개) ===\n")
        for name, resource_type in unique_names_with_types:
            f.write(f"{name}\n")
    
    print("\n타입별 분류 결과:")
    for resource_type in sorted(type_groups.keys()):
        print(f"{resource_type}: {len(type_groups[resource_type])}개")
        for name in sorted(type_groups[resource_type]):
            print(f"  {resource_type}:{name}")
    
    print(f"\n결과가 다음 파일들에 저장되었습니다:")
    print(f"- 'resource_names_typed.txt' (타입별 분류)")
    print(f"- 'resource_names.txt' (기존 형식)")

if __name__ == "__main__":
    main()

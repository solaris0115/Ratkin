#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Apparel 텍스처 파일 리네이밍 스크립트
RK_ -> RK_TextureApparel_ 접두사 변경
"""

import os
import shutil
import sys
from pathlib import Path

def rename_apparel_textures(apparel_dir, test_mode=False, test_file=None):
    """
    Apparel 텍스처 파일들을 리네이밍합니다.
    
    Args:
        apparel_dir (str): Apparel 폴더 경로
        test_mode (bool): 테스트 모드 (True면 실제 리네이밍 안함)
        test_file (str): 테스트할 특정 파일명 (None이면 전체 처리)
    """
    
    apparel_path = Path(apparel_dir)
    if not apparel_path.exists():
        print(f"❌ 경로가 존재하지 않습니다: {apparel_dir}")
        return
    
    # PNG 파일들만 필터링
    png_files = list(apparel_path.glob("*.png"))
    
    if not png_files:
        print("❌ PNG 파일이 없습니다.")
        return
    
    print(f"📁 총 {len(png_files)}개의 PNG 파일 발견")
    
    # 이미 RK_TextureApparel_ 접두사가 있는 파일들 확인
    already_renamed = []
    target_files = []
    
    for file_path in png_files:
        filename = file_path.name
        if filename.startswith("RK_TextureApparel_"):
            already_renamed.append(filename)
        elif filename.startswith("RK_"):
            target_files.append(file_path)
    
    print(f"✅ 이미 RK_TextureApparel_ 접두사가 있는 파일: {len(already_renamed)}개")
    print(f"🔄 리네이밍 대상 파일: {len(target_files)}개")
    
    if already_renamed:
        print("   이미 처리된 파일들:")
        for f in already_renamed[:5]:  # 처음 5개만 표시
            print(f"   - {f}")
        if len(already_renamed) > 5:
            print(f"   ... 외 {len(already_renamed)-5}개")
    
    if not target_files:
        print("✅ 리네이밍할 파일이 없습니다.")
        return
    
    # 테스트 모드 처리
    if test_mode and test_file:
        target_files = [f for f in target_files if f.name == test_file]
        if not target_files:
            print(f"❌ 테스트 파일을 찾을 수 없습니다: {test_file}")
            return
        print(f"🧪 테스트 모드: {test_file} 파일만 처리")
    
    # 리네이밍 실행
    renamed_count = 0
    error_count = 0
    
    for file_path in target_files:
        old_name = file_path.name
        new_name = old_name.replace("RK_", "RK_TextureApparel_", 1)  # 첫 번째 RK_만 교체
        new_path = file_path.parent / new_name
        
        # 중복 파일명 체크
        if new_path.exists():
            print(f"⚠️  중복 파일명으로 스킵: {new_name}")
            continue
        
        try:
            if test_mode:
                print(f"🧪 [테스트] {old_name} -> {new_name}")
            else:
                file_path.rename(new_path)
                print(f"✅ {old_name} -> {new_name}")
            renamed_count += 1
        except Exception as e:
            print(f"❌ 오류 발생 ({old_name}): {e}")
            error_count += 1
    
    # 결과 요약
    print(f"\n📊 작업 완료:")
    print(f"   ✅ 성공: {renamed_count}개")
    print(f"   ❌ 실패: {error_count}개")
    print(f"   ⏭️  스킵: {len(already_renamed)}개")
    
    if test_mode:
        print("🧪 테스트 모드로 실행되었습니다. 실제 파일은 변경되지 않았습니다.")

def main():
    """메인 함수"""
    apparel_dir = "Project/Textures/Thing/Apparel"
    
    print("🎯 Apparel 텍스처 리네이밍 스크립트")
    print("=" * 50)
    
    # 명령행 인수 처리
    if len(sys.argv) > 1:
        if sys.argv[1] == "test":
            if len(sys.argv) > 2:
                test_file = sys.argv[2]
                print(f"🧪 테스트 모드: {test_file} 파일 테스트")
                rename_apparel_textures(apparel_dir, test_mode=True, test_file=test_file)
            else:
                print("❌ 테스트할 파일명을 입력하세요.")
                print("사용법: python rename_apparel_textures.py test <파일명>")
        elif sys.argv[1] == "dry-run":
            print("🧪 드라이런 모드: 실제 변경 없이 미리보기")
            rename_apparel_textures(apparel_dir, test_mode=True)
        else:
            print("❌ 잘못된 인수입니다.")
            print("사용법:")
            print("  python rename_apparel_textures.py          # 전체 실행")
            print("  python rename_apparel_textures.py dry-run  # 미리보기")
            print("  python rename_apparel_textures.py test <파일명>  # 단일 파일 테스트")
    else:
        # 전체 실행 확인 (자동 승인)
        print("🚨 전체 파일을 리네이밍합니다.")
        rename_apparel_textures(apparel_dir)

if __name__ == "__main__":
    main()

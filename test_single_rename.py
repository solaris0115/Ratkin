#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
단일 파일 리네이밍 테스트 스크립트
"""

import os
import shutil
from pathlib import Path

def test_single_rename():
    """RK_WorkerWear.png 파일 하나만 실제로 리네이밍 테스트"""
    
    apparel_dir = Path("Project/Textures/Thing/Apparel")
    old_file = apparel_dir / "RK_WorkerWear.png"
    new_file = apparel_dir / "RK_TextureApparel_WorkerWear.png"
    
    if not old_file.exists():
        print(f"❌ 파일이 존재하지 않습니다: {old_file}")
        return
    
    if new_file.exists():
        print(f"⚠️  이미 존재하는 파일: {new_file}")
        return
    
    try:
        # 실제 리네이밍 실행
        old_file.rename(new_file)
        print(f"✅ 성공: {old_file.name} -> {new_file.name}")
        
        # 결과 확인
        if new_file.exists():
            print(f"✅ 확인: 새 파일이 정상적으로 생성되었습니다")
            print(f"   크기: {new_file.stat().st_size} bytes")
        
        # 롤백 테스트
        rollback = input("🔄 롤백하시겠습니까? (y/N): ")
        if rollback.lower() in ['y', 'yes']:
            new_file.rename(old_file)
            print(f"✅ 롤백 완료: {new_file.name} -> {old_file.name}")
        else:
            print("ℹ️  파일이 새 이름으로 유지됩니다.")
            
    except Exception as e:
        print(f"❌ 오류 발생: {e}")

if __name__ == "__main__":
    test_single_rename()

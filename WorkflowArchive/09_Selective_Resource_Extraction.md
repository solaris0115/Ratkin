# 선택적 리소스 추출 - 2024-12-19

## 작업 개요
- 요청 내용: Defs에서는 defName만, Sounds와 Textures에서는 파일명만 추출 (텍스처 접미사 제외)
- 목표: 각 폴더별로 다른 방식으로 리소스 이름 추출

## 계획 (AI가 결정한 계획)
1. 워크플로우 파일 생성
2. Defs 폴더에서 defName 속성만 추출
3. Sounds 폴더에서 파일명만 추출 (확장자 제외)
4. Textures 폴더에서 파일명만 추출 (확장자와 접미사 제외)
5. 결과를 통합하여 최종 리스트 생성

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 워크플로우 파일 생성
2. Defs 폴더에서 defName 속성만 추출
3. Sounds 폴더에서 파일명만 추출 (확장자 제외)
4. Textures 폴더에서 파일명만 추출 (확장자와 접미사 제외)
5. 결과를 통합하여 최종 리스트 생성

## 작업 세부 진행
1. 워크플로우 파일 생성 [v]
2. Defs 폴더에서 defName 속성만 추출 [v]
3. Sounds 폴더에서 파일명만 추출 [v]
4. Textures 폴더에서 파일명만 추출 [v]
5. 결과를 통합하여 최종 리스트 생성 [v]

## 진행 상황
### 1. 워크플로우 파일 생성
- 내용: 작업 추적을 위한 워크플로우 파일 생성
- 결과: WorkFlow/09_Selective_Resource_Extraction.md 파일 생성 완료

### 2. Defs 폴더에서 defName 속성만 추출
- 내용: XML 파일에서 <defName>값</defName> 패턴으로 defName 추출
- 결과: 총 273개의 defName 추출 완료
- 파일: defnames_only.txt

### 3. Sounds 폴더에서 파일명만 추출
- 내용: 확장자를 제외한 파일명만 추출
- 결과: 총 15개의 Sound 파일명 추출 완료
- 파일: sounds_only.txt

### 4. Textures 폴더에서 파일명만 추출
- 내용: 확장자와 _south 같은 접미사를 제외한 파일명만 추출
- 결과: 총 336개의 Texture 파일명 추출 완료
- 파일: textures_only.txt

### 5. 결과를 통합하여 최종 리스트 생성
- 내용: 모든 리소스를 카테고리별로 정리하여 통합
- 결과: 총 624개의 리소스 통합 완료
- 파일: Selective_Resource_List.txt

## 최종 작업 결과/ 중단 사유
완료 - 요청된 방식으로 각 폴더별로 다른 추출 방식 적용하여 성공적으로 완료

## 관련 파일 목록
- WorkFlow/09_Selective_Resource_Extraction.md
- extract_defnames_only.py
- extract_sounds_textures.py
- combine_selective_resources.py
- defnames_only.txt
- sounds_only.txt
- textures_only.txt
- Selective_Resource_List.txt

## 참고사항
- Defs: defName 속성만 추출 (273개)
- Sounds: 파일명만 (확장자 제외, 15개)
- Textures: 파일명만 (확장자와 _south 같은 접미사 제외, 336개)
- 총 624개의 리소스 추출 완료

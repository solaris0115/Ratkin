# 리소스 파일 추출 스크립트 작성 - 2025-09-17

## 작업 개요
- 요청 내용: Textures와 Sounds 폴더의 리소스 파일들을 추적하는 스크립트 작성
- 목표: 텍스처 접미사 제거하여 기본 이름만 추출, 별도 파일로 정리

## 계획 (AI가 결정한 계획)
1. Textures와 Sounds 폴더 구조 확인
2. 리소스 파일 추출 스크립트 작성 (텍스처 접미사 처리 포함)
3. 별도 파일로 리소스 리스트 생성

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 리소스 추출 스크립트 작성
2. 텍스처 접미사(_south 등) 제거 로직 구현
3. Resource_List.txt 파일 생성

## 작업 세부 진행
1. 리소스 추출 스크립트 작성 [v]
2. 텍스처 접미사 처리 로직 구현 [v]
3. 스크립트 실행 및 리소스 리스트 생성 [v]

## 진행 상황
### 1. 리소스 추출 스크립트 작성
- 내용: Textures와 Sounds 폴더의 리소스 파일들을 추적하는 파이썬 스크립트 작성 완료
- 결과: extract_resources.py 파일 생성

### 2. 텍스처 접미사 처리 로직 구현
- 내용: 텍스처 파일의 접미사(_south, _north, _east, _west 등) 제거 로직 구현
- 결과: 기본 텍스처 이름만 추출하여 중복 제거

### 3. 스크립트 실행 및 리소스 리스트 생성
- 내용: 파이썬 스크립트 실행하여 리소스 리스트 생성
- 결과: 총 946개의 리소스 추출 완료

## 최종 작업 결과
- extract_resources.py 스크립트 생성 완료
- Project/Textures와 Project/Sounds 폴더에서 총 946개의 리소스 추출 완료
- 형식: ResourceType:ResourceName - filePath 으로 출력 완료
- 텍스처 접미사 제거로 중복 방지 및 기본 이름만 추출
- 별도 파일 Resource_List.txt로 정리 완료

## 관련 파일 목록
- [extract_resources.py](../extract_resources.py)
- [Resource_List.txt](../Resource_List.txt)

## 참고사항
- 텍스처 접미사: _south, _north, _east, _west, _front, _back, _damaged 등 제거
- 사운드 파일: 접미사 제거 없이 원본 이름 유지
- 중복 제거로 동일한 리소스명은 하나만 기록

# String 데이터 추출 작업 - 2024-12-19

## 작업 개요
- 요청 내용: Defs 폴더에서 숫자가 아닌 모든 string 형 데이터 수집, 각 항목이 어느 파일에서 참조되는지 추적
- 목표: DefName_List.txt와 유사한 형태로 모든 string 데이터를 리스트화

## 계획 (AI가 결정한 계획)
1. Defs 폴더의 모든 XML 파일 스캔
2. 각 XML 파일에서 string 형 데이터 추출 (숫자 제외)
3. 각 string이 어느 파일에서 참조되는지 매핑
4. 중복 제거 및 정리된 리스트 생성
5. HTML 리스트 형태로 결과 저장

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. Defs 폴더의 모든 XML 파일 스캔
2. 각 XML 파일에서 string 형 데이터 추출 (숫자 제외)
3. 각 string이 어느 파일에서 참조되는지 매핑
4. 중복 제거 및 정리된 리스트 생성
5. HTML 리스트 형태로 결과 저장

## 작업 세부 진행
1. Defs 폴더 구조 파악 및 XML 파일 목록 생성 [v]
2. string 추출 스크립트 작성 (extract_strings.py) [v]
3. 전체 string 데이터 추출 실행 [v]
4. 간단한 형태의 결과 생성 스크립트 작성 (extract_strings_simple.py) [v]
5. HTML 리스트 형태 결과 생성 [v]
6. 워크플로우 파일 생성 [v]

## 진행 상황
### 1. Defs 폴더 구조 파악
- 내용: Project/1.6/Defs 폴더에서 28개의 XML 파일 발견
- 결과: 총 28개 파일 처리 대상 확인

### 2. string 추출 스크립트 작성
- 내용: XML 파싱을 통한 string 데이터 추출 로직 구현
- 결과: extract_strings.py 스크립트 완성

### 3. 전체 string 데이터 추출
- 내용: 28개 XML 파일에서 모든 string 형 데이터 추출
- 결과: 총 1,593개의 고유한 string 발견

### 4. 간단한 형태 결과 생성
- 내용: HTML 리스트 형태로 결과를 정리하는 스크립트 작성
- 결과: extract_strings_simple.py 스크립트 완성

### 5. 최종 결과 생성
- 내용: HTML 리스트 형태의 결과 파일들 생성
- 결과: String_List_Simple.txt, String_Names_Only.txt 파일 생성

## 최종 작업 결과
- 총 1,593개의 고유한 string 데이터 추출 완료
- 각 string이 어느 파일에서 참조되는지 매핑 완료
- HTML 리스트 형태로 정리된 결과 파일 생성

## 관련 파일 목록
- extract_strings.py: 전체 string 추출 스크립트
- extract_strings_simple.py: 간단한 형태 결과 생성 스크립트
- String_List.txt: 상세한 텍스트 형태 결과 (11,436줄)
- String_Map.json: JSON 형태의 구조화된 결과
- String_List_Simple.txt: 간단한 형태의 상세 결과
- String_Names_Only.txt: HTML 리스트 형태의 이름만

## 참고사항
- 숫자형 데이터는 자동으로 제외됨
- 중복된 string은 하나로 통합하고 참조 파일 목록을 유지
- 결과는 문자열 길이 순으로 정렬 (긴 것부터)
- 총 28개의 XML 파일에서 추출됨

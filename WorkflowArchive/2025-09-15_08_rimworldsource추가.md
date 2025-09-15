# RimworldSource 소스코드 정보 추가 - 2025-09-15

## 작업 개요
- 요청 내용: def-structure-guide.mdc에 RimworldSource 폴더 정보를 추가하여 림월드 오리지널 소스코드 참고 시 활용
- 목표: Def 구조 가이드에 소스코드 참조 정보 포함

## 계획 (AI가 결정한 계획)
1. 현재 def-structure-guide.mdc 내용 확인
   - 분석 파일: [.cursor/rules/def-structure-guide.mdc](.cursor/rules/def-structure-guide.mdc)
2. RimworldSource 폴더 구조 분석
   - 참고 폴더: [RimworldSource/](RimworldSource/) - 림월드 오리지널 소스코드
   - 주요 구성: Assembly-CSharp.csproj, Verse/, RimWorld/, LudeonTK/ 등
3. 레퍼런스 정보 섹션에 소스코드 정보 추가
4. 소스코드 활용 방법 설명 추가

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. def-structure-guide.mdc의 레퍼런스 정보 섹션 확장
2. RimworldSource 폴더 구조와 활용 방법 설명 추가
3. 소스코드 참고가 필요한 상황 명시

## 작업 세부 진행
1. 기존 가이드 내용 분석 [v]
2. RimworldSource 폴더 구조 파악 [v]
3. 레퍼런스 섹션에 소스코드 정보 추가 [v]

## 진행 상황
### 1. 기존 가이드 분석
- 참고 파일: [.cursor/rules/def-structure-guide.mdc](.cursor/rules/def-structure-guide.mdc)
- 내용: 현재 레퍼런스 정보에 RimWorldData만 포함됨
- 결과: 소스코드 정보 추가 필요

### 2. RimworldSource 폴더 구조 파악
- 위치: [RimworldSource/](RimworldSource/)
- 주요 구성:
  - Assembly-CSharp.csproj (568KB, 9249 lines) - 메인 프로젝트 파일
  - Verse/ - 게임 엔진 코어 소스
  - RimWorld/ - 게임 로직 소스
  - LudeonTK/ - 개발 도구 소스
  - 기타 유틸리티 클래스들 (LayoutWorker, MechanitorUtility 등)
- 결과: 림월드 전체 소스코드 구조 파악 완료

### 3. def-structure-guide.mdc 수정 완료
- 내용: 레퍼런스 정보 섹션에 소스코드 정보 추가
- 추가된 내용:
  - RimworldSource 폴더 구조 설명
  - 소스코드 활용 방법 섹션 신규 추가
- 결과: 소스코드 참조 정보 통합 완료

## 최종 작업 결과
성공적으로 def-structure-guide.mdc에 RimworldSource 정보 추가 완료

### 추가된 내용:
1. **소스코드 레퍼런스 정보**:
   - RimworldSource 폴더 구조와 각 디렉토리 설명
   - Assembly-CSharp.csproj, Verse/, RimWorld/, LudeonTK/ 등 주요 구성 요소

2. **소스코드 활용 방법 섹션 신규 추가**:
   - Def 구현 방식 참고 방법
   - 게임 메커니즘 이해를 위한 활용법
   - API 및 메서드 확인 방법
   - 구조 패턴 학습 지침

## 관련 파일 목록
- [.cursor/rules/def-structure-guide.mdc](.cursor/rules/def-structure-guide.mdc) - 수정 대상
- [RimworldSource/](RimworldSource/) - 추가할 소스코드 폴더

## 참고사항
- 소스코드 참조로 더 정확하고 효율적인 Def 작업 가능
- 게임 내부 동작 방식 이해를 통한 고급 기능 구현 지원

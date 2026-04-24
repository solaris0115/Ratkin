# PawnGroupKindDef 소스코드 분석 - 2024-12-19

## 작업 개요
- 요청 내용: PawnGroupKindDef가 소스코드에서 해주는 역할 조사
- 목표: PawnGroupKindDef의 구조, 기능, 사용처를 상세히 분석

## 계획 (AI가 결정한 계획)
1. PawnGroupKindDef 기본 클래스 구조 분석
2. PawnGroupKindWorker 추상 클래스 및 구현체들 분석
3. PawnGroupMaker와의 관계 분석
4. 실제 사용 사례들 조사
5. 분석 결과 정리

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. PawnGroupKindDef 기본 클래스 구조 분석
2. PawnGroupKindWorker 추상 클래스 및 구현체들 분석
3. PawnGroupMaker와의 관계 분석
4. 실제 사용 사례들 조사
5. 분석 결과 정리

## 작업 세부 진행
1. PawnGroupKindDef 기본 클래스 구조 분석 [v]
2. PawnGroupKindWorker 추상 클래스 및 구현체들 분석 [v]
3. PawnGroupMaker와의 관계 분석 [v]
4. 실제 사용 사례들 조사 [v]
5. 분석 결과 정리 [v]

## 진행 상황
### 1. PawnGroupKindDef 기본 클래스 구조 분석
- 내용: PawnGroupKindDef.cs 파일 분석
- 결과: 
  - Def 클래스를 상속받는 기본 정의 클래스
  - workerClass 필드로 PawnGroupKindWorker 타입 지정
  - Worker 프로퍼티로 실제 작업을 수행하는 Worker 인스턴스 제공

### 2. PawnGroupKindWorker 추상 클래스 및 구현체들 분석
- 내용: PawnGroupKindWorker.cs 및 구현체들 분석
- 결과:
  - 추상 클래스로 다양한 그룹 생성 로직을 캡슐화
  - 주요 메서드: MinPointsToGenerateAnything, GeneratePawns, CanGenerateFrom, GeneratePawnKindsExample
  - 구현체: PawnGroupKindWorker_Normal, PawnGroupKindWorker_Trader, PawnGroupKindWorker_Shamblers 등

### 3. PawnGroupMaker와의 관계 분석
- 내용: PawnGroupMaker.cs 파일 분석
- 결과:
  - PawnGroupKindDef를 참조하여 그룹 생성 로직 결정
  - commonality, maxTotalPoints, options, traders, carriers, guards 등 설정 포함
  - Worker를 통해 실제 그룹 생성 수행

### 4. 실제 사용 사례들 조사
- 내용: 소스코드에서 PawnGroupKindDef 사용처 검색
- 결과:
  - 레이드 생성: PawnGroupKindDefOf.Combat 사용
  - 상인 캐러밴: PawnGroupKindDefOf.Trader 사용
  - 평화로운 그룹: PawnGroupKindDefOf.Peaceful 사용
  - 정착지 주민: PawnGroupKindDefOf.Settlement 사용
  - 특수 그룹들: Shamblers, Fleshbeasts, Sightstealers 등

### 5. 분석 결과 정리
- 내용: 전체 분석 결과를 종합하여 정리
- 결과: 완료

## 최종 작업 결과/ 중단 사유
완료 - PawnGroupKindDef의 역할과 구조를 상세히 분석 완료

## 관련 파일 목록
- RimworldSource/RimWorld/PawnGroupKindDef.cs
- RimworldSource/RimWorld/PawnGroupKindWorker.cs
- RimworldSource/RimWorld/PawnGroupKindWorker_Normal.cs
- RimworldSource/RimWorld/PawnGroupKindWorker_Trader.cs
- RimworldSource/RimWorld/PawnGroupMaker.cs
- RimworldSource/RimWorld/PawnGroupKindDefOf.cs
- RimworldSource/RimWorld/PawnGroupMakerParms.cs

## 참고사항
- PawnGroupKindDef는 게임 내 다양한 상황에서 적절한 폰 그룹을 생성하는 핵심 시스템
- 각 종류별로 특화된 Worker 클래스가 존재하여 다양한 생성 로직을 지원
- 레이드, 상인 캐러밴, 정착지 주민 등 게임의 핵심 요소들을 담당


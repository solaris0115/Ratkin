# Tool 선택 가중치 로그 출력 하모니 패치 구현 - 2025-01-XX

## 작업 개요
- **요청 내용**: 하모니 패치를 만들어서 tools의 가중치와 선택된 tool을 로그로 출력
- **목표**: RimWorld 소스코드에서 tool 선택 시 각 tool의 가중치와 최종 선택된 tool을 로그로 출력하는 하모니 패치 구현
- **범위**: 
  - VerbProperties.AdjustedMeleeSelectionWeight 메서드 패치
  - Pawn_MeleeVerbs.TryGetMeleeVerb 메서드 패치

## 계획 (AI가 결정한 계획)

### 1단계: Tool 선택 로직 위치 확인
- [x] RimWorld 소스코드에서 tool 선택 로직 위치 확인
- [x] VerbProperties.AdjustedMeleeSelectionWeight 메서드 확인
- [x] Pawn_MeleeVerbs.TryGetMeleeVerb 메서드 확인

### 2단계: 하모니 패치 파일 생성
- [x] ToolSelectionPatch.cs 파일 생성
- [x] ToolSelectionWeightPatch 클래스 구현 (가중치 로그 출력)
- [x] ToolSelectionResultPatch 클래스 구현 (선택된 tool 로그 출력)

### 3단계: 프로젝트 파일 업데이트
- [x] NewRatkin.csproj에 새 파일 추가

### 4단계: 워크플로우 파일 작성
- [x] 작업 계획서 작성

## 최종 계획 (컨펌 완료)
1. VerbProperties.AdjustedMeleeSelectionWeight 메서드에 Postfix 패치 적용
   - 각 tool의 가중치 계산 요소들을 로그로 출력
   - Power, ExpectedDamage, DamageSquared, Commonality, ChanceFactor, PawnNativeMultiplier, FinalWeight 출력

2. Pawn_MeleeVerbs.TryGetMeleeVerb 메서드에 Postfix 패치 적용
   - 선택된 tool 정보를 로그로 출력
   - Tool Label, Weapon Name, Capacity, Power, ChanceFactor, Pawn, Target 정보 출력

## 작업 세부 진행
1. [x] Tool 선택 로직 위치 확인
2. [x] 하모니 패치 파일 생성 및 구현
3. [x] 프로젝트 파일 업데이트
4. [x] 워크플로우 파일 작성

## 진행 상황

### 1. Tool 선택 로직 위치 확인
- [x] RimWorld 소스코드에서 tool 선택 로직 위치 확인
  - VerbProperties.AdjustedMeleeSelectionWeight: 각 tool의 가중치 계산
  - Pawn_MeleeVerbs.TryGetMeleeVerb: 실제 tool 선택 수행
- [x] 기존 분석 보고서 확인 (Report/24_Tool_Selection_Probability_Report.md)

### 2. 하모니 패치 파일 생성
- [x] Project/1.6/Source/Gunlance/ToolSelectionPatch.cs 파일 생성
- [x] ToolSelectionWeightPatch 클래스 구현
  - VerbProperties.AdjustedMeleeSelectionWeight 메서드 Postfix 패치
  - 각 tool의 가중치 계산 요소들을 상세히 로그 출력
  - Power, ExpectedDamage, DamageSquared, Commonality, ChanceFactor, PawnNativeMultiplier, FinalWeight 출력
- [x] ToolSelectionResultPatch 클래스 구현
  - Pawn_MeleeVerbs.TryGetMeleeVerb 메서드 Postfix 패치
  - 선택된 tool 정보를 로그로 출력
  - Tool Label, Weapon Name, Capacity, Power, ChanceFactor, Pawn, Target 정보 출력
- [x] 예외 처리 추가 (try-catch)

### 3. 프로젝트 파일 업데이트
- [x] NewRatkin.csproj에 ToolSelectionPatch.cs 추가

### 4. 워크플로우 파일 작성
- [x] WorkFlow/25_Tool_Selection_Log_Patch.md 작성 완료

## 최종 작업 결과
✅ 완료

### 결과 요약

**구현된 기능:**
1. **ToolSelectionWeightPatch**: 각 tool의 가중치 계산 시 상세 정보를 로그로 출력
   - Tool Label, Source (무기 또는 Pawn Native), Capacity
   - Power, ExpectedDamage, DamageSquared
   - Commonality, ChanceFactor, PawnNativeMultiplier
   - FinalWeight (최종 가중치)

2. **ToolSelectionResultPatch**: 실제로 선택된 tool 정보를 로그로 출력
   - Tool Label, Weapon Name, Capacity
   - Power, ChanceFactor
   - Pawn, Target 정보

**로그 출력 형식:**
- `[ToolSelection] Tool: {label} | Source: {source} | Capacity: {capacity} | Power: {power} | ExpectedDamage: {damage} | DamageSquared: {damageSquared} | Commonality: {commonality} | ChanceFactor: {chanceFactor} | PawnNativeMultiplier: {multiplier} | FinalWeight: {weight}`
- `[ToolSelection] SELECTED Tool: {label} | Weapon: {weapon} | Capacity: {capacity} | Power: {power} | ChanceFactor: {chanceFactor} | Pawn: {pawn} | Target: {target}`

## 관련 파일 목록
- Project/1.6/Source/Gunlance/ToolSelectionPatch.cs (새로 생성)
- Project/1.6/Source/NewRatkin.csproj (수정)
- Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml (참고)
- Report/24_Tool_Selection_Probability_Report.md (참고)

## 참고사항
- BasePatch.cs에서 PatchAll을 사용하므로 새 패치 파일이 자동으로 로드됨
- 예외 처리를 추가하여 안정성 확보
- Reflection을 사용하여 AdjustedExpectedDamageForVerbUsableInMelee 메서드 호출
- 로그 태그 `[ToolSelection]`을 사용하여 쉽게 필터링 가능


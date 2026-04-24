# Tool 선택 확률 분석 - 2025-01-XX

## 작업 개요
- **요청 내용**: Weapon_HighTech.xml의 Tools와 Ratkin RaceDef의 Tools를 포함하여 각각의 발동 확률(가중치) 조사
- **목표**: RimWorld 소스코드에서 tool 선택 로직 분석 및 각 tool의 확률 계산
- **범위**: 
  - Weapon_HighTech.xml의 tools (point, edge)
  - Ratkin RaceDef의 tools (teeth, left fist, right fist)

## 계획 (AI가 결정한 계획)

### 1단계: Tool 정의 확인
- [x] Weapon_HighTech.xml의 tools 확인
- [x] Ratkin RaceDef의 tools 확인
- [ ] 각 tool의 chanceFactor 확인

### 2단계: RimWorld 소스코드 조사
- [ ] Tool.cs 파일 분석 (chanceFactor 필드 확인)
- [ ] tool 선택 로직 위치 찾기
- [ ] 가중치 계산 방법 파악

### 3단계: 확률 계산
- [ ] 기본 가중치 (chanceFactor 또는 기본값)
- [ ] 총 가중치 계산
- [ ] 각 tool의 발동 확률(퍼센트) 계산

### 4단계: 결과 보고서 작성
- [ ] 분석 결과 정리
- [ ] 각 tool별 확률 표 작성
- [ ] Report 폴더에 보고서 생성

## 최종 계획 (컨펌 필요)
[사용자 승인 대기]

## 작업 세부 진행
1. [x] Tool 정의 확인
2. [x] RimWorld 소스코드 조사
3. [x] 확률 계산
4. [x] 결과 보고서 작성

## 진행 상황

### 1. Tool 정의 확인
- [x] Weapon_HighTech.xml의 tools 확인 (point, edge)
- [x] Ratkin RaceDef의 tools 확인 (teeth, left fist, right fist)
- [x] 각 tool의 chanceFactor 확인 (모두 기본값 1.0)

### 2. RimWorld 소스코드 조사
- [x] Tool.cs에서 chanceFactor 필드 확인 (기본값 1f)
- [x] VerbProperties.cs의 AdjustedMeleeSelectionWeight 메서드 확인
- [x] 가중치 계산 공식 파악:
  - 선택 가중치 = (예상 피해량²) × ManeuverDef.commonality × Tool.chanceFactor × (Pawn 기본 Tool인 경우 0.3)

### 3. 확률 계산
- [x] Weapon_HighTech.xml Tool 확률 계산
  - Point: 50%
  - Edge: 50%
- [x] Ratkin RaceDef Tool 확률 계산
  - Teeth: 66.67%
  - Left Fist: 16.67%
  - Right Fist: 16.67%

### 4. 결과 보고서 작성
- [x] Report/24_Tool_Selection_Probability_Report.md 작성 완료

## 최종 작업 결과
✅ 완료

### 결과 요약

**Weapon_HighTech.xml:**
- Point와 Edge는 각각 50% 확률로 발동

**Ratkin RaceDef:**
- Teeth: 66.67%
- Left Fist: 16.67%
- Right Fist: 16.67%

**핵심 발견:**
1. Tool 선택 가중치 공식: (피해량²) × commonality × chanceFactor × (Pawn 기본 Tool인 경우 0.3)
2. Ratkin의 Pawn 기본 Tool들은 모두 0.3 배율 적용
3. 피해량의 제곱이 가중치에 사용되어, power 차이가 확률에 크게 영향을 줌

## 관련 파일 목록
- Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
- Project/1.6/Defs/ThingDefs_Races/Races_Rakinlike.xml
- RimworldSource/Verse/Tool.cs

## 참고사항
- Tool.cs에 chanceFactor = 1f 기본값 확인
- Weapon_HighTech.xml에는 chanceFactor 미지정
- Ratkin RaceDef에는 chanceFactor 미지정

# 건랜스 사운드 개선 작업 - 2025-01-XX

## 작업 개요
- **요청 내용**: 코어 사운드들과 HellcatBurner 관련 사운드들을 참고하여 Weapon_HighTech.xml의 건랜스 사운드들 수정
- **목표**: HellcatBurner 스타일과 림월드 코어 사운드 패턴을 참고하여 사운드 정의 개선

## 계획 (AI가 결정한 계획)
1. HellcatBurner Ability 및 사운드 정의 분석
2. FireSpew_Warmup, FireSpew_Resolve 사운드 구조 확인
3. 코어 사운드들의 volumeRange, pitchRange 패턴 분석
4. 현재 건랜스 사운드 정의 확인
5. 개선안 제시 및 적용

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. HellcatBurner Ability 및 사운드 정의 분석 [v]
2. FireSpew_Warmup, FireSpew_Resolve 사운드 구조 확인 [v]
3. 코어 사운드들의 volumeRange, pitchRange 패턴 분석 [v]
4. 현재 건랜스 사운드 정의 확인 [v]
5. 개선안 제시 및 적용 [v]

## 작업 세부 진행
1. [HellcatBurner 사운드 분석] [v]
2. [코어 사운드 패턴 분석] [v]
3. [현재 사운드 정의 확인] [v]
4. [개선안 제시] [v]
5. [사운드 정의 수정] [v]

## 진행 상황

### 1. HellcatBurner 사운드 분석

#### AbilityDef 구조
- `warmupStartSound`: `FireSpew_Warmup`
- `soundCast`: `FireSpew_Resolve`
- `soundReload`: `Standard_Reload` (CompProperties_EquippableAbilityReloadable에서)

#### FireSpew_Warmup 특징
- `maxSimultaneous`: 5
- `priorityMode`: `PrioritizeNearest`
- `volumeRange`: 60
- `muteWhenPaused`: True
- `tempoAffectedByGameSpeed`: true
- `AudioGrain_Folder` 사용

#### FireSpew_Resolve 특징
- `maxSimultaneous`: 5
- `priorityMode`: `PrioritizeNearest`
- `volumeRange`: 60
- `muteWhenPaused`: True
- `tempoAffectedByGameSpeed`: true

### 2. 코어 사운드 패턴 분석

#### 일반 총기 사운드
- `volumeRange`: 35~94.71 (무기 종류에 따라 다양)
- `pitchRange`: 0.7~1.85 (무기 종류에 따라 다양)
- `maxSimultaneous`: 1~4 (사운드 종류에 따라)

#### Standard_Reload
- `maxSimultaneous`: 1
- `volumeRange`: 없음
- `AudioGrain_Clip` 사용

### 3. 현재 건랜스 사운드 문제점

1. **이름 불일치**: SoundDef 이름과 GunlanceDefOf 필드명 불일치
   - `RK_Sound_Warmup` vs `RK_Charge`
   - `RK_Sound_GunlanceNormalFire` vs `RK_Fire`
   - `RK_Sound_Empty` vs `RK_OverHeat`
   - `RK_Sound_Reload` vs `RK_Reload`

2. **설정 부족**:
   - `volumeRange` 없음
   - `maxSimultaneous` 모두 1로 제한적
   - `priorityMode` 없음
   - 워밍업 사운드에 `muteWhenPaused`, `tempoAffectedByGameSpeed` 없음

3. **피치 범위**: 모두 동일 (0.9152174~1.042391)

### 4. 개선안 적용

#### 적용된 변경사항

**RK_Charge (워밍업)**
- 이름: `RK_Sound_Warmup` → `RK_Charge`
- `maxSimultaneous`: 1 → 3
- `volumeRange`: 50~60 추가
- `priorityMode`: `PrioritizeNearest` 추가
- `muteWhenPaused`: True 추가
- `tempoAffectedByGameSpeed`: true 추가
- 피치 범위: 0.9~1.1로 조정

**RK_Fire (일반 발사)**
- 이름: `RK_Sound_GunlanceNormalFire` → `RK_Fire`
- `maxSimultaneous`: 1 → 3
- `volumeRange`: 55~65 추가
- `priorityMode`: `PrioritizeNearest` 추가
- 피치 범위: 0.9~1.1로 조정

**RK_OverHeat (과열)**
- 이름: `RK_Sound_Empty` → `RK_OverHeat`
- `maxSimultaneous`: 1 → 2
- `volumeRange`: 40~50 추가
- 피치 범위: 0.85~1.0으로 조정 (경고 느낌)

**RK_WyvernFire (용격포)**
- 이름: `RK_Sound_WyvernFire` → `RK_WyvernFire` (이미 일치)
- `maxSimultaneous`: 1 → 2
- `volumeRange`: 70~80 추가 (강력한 공격)
- `priorityMode`: `PrioritizeNearest` 추가
- 피치 범위: 0.8~1.0으로 조정 (무거운 느낌)

**RK_Reload (재장전)**
- 이름: `RK_Sound_Reload` → `RK_Reload`
- `maxSimultaneous`: 1 유지
- `volumeRange`: 35~45 추가
- 피치 범위: 0.9~1.1로 조정

## 최종 작업 결과
✅ 완료: 건랜스 사운드 정의 개선
- 이름을 GunlanceDefOf와 일치하도록 변경
- HellcatBurner 스타일 참고하여 설정 추가
- volumeRange 추가로 사운드 밸런스 개선
- maxSimultaneous 조정으로 게임플레이 경험 개선
- 피치 범위를 사운드 특성에 맞게 조정

## 관련 파일 목록
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` (수정됨)
- `Project/1.6/Source/Gunlance/GunlanceDefOf.cs` (참고)
- `RimworldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml` (HellcatBurner 참고)
- `RimworldData/Core/Defs/SoundDefs/Interact_Oneshots_Misc.xml` (FireSpew 사운드 참고)
- `Report/47_Gunlance_Sound_Improvement_Proposal.md` (상세 제안서)

## 참고사항
- HellcatBurner는 `AudioGrain_Folder`를 사용하지만, 현재 프로젝트는 `AudioGrain_Clip` 사용 유지
- 실제 오디오 파일 특성에 맞게 volumeRange와 pitchRange를 미세 조정 가능
- 여러 클립 파일이 있다면 향후 `AudioGrain_Folder`로 전환 고려







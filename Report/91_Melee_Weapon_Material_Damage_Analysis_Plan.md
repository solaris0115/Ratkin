# 소재별 근접 무기 피해량 및 관통력 분석 조사 계획

## 조사 목표
1. 소재에 따른 근접 무기 피해량에 영향을 주는 Stat 확인
2. 관통력(ArmorPenetration) 계산 방식 및 소재와의 관계 파악
3. 주요 소재별 피해량 배율 정리

## 조사 항목

### 1. 근접 무기 피해량 계산 로직
- **소스코드 위치**: `RimworldSource/Verse/Tool.cs`, `RimworldSource/Verse/VerbProperties.cs`
- **확인 사항**:
  - `AdjustedBaseMeleeDamageAmount` 메서드 분석
  - `AdjustedMeleeDamageAmount` 메서드 분석
  - 소재(Stuff)가 피해량에 영향을 주는 방식

### 2. DamageDef의 armorCategory.multStat 확인
- **소스코드 위치**: `RimWorldData/Core/Defs/Misc/DamageArmorCategoryDef/DamageArmorCategoryDefs.xml`
- **확인 사항**:
  - Sharp 카테고리 → `SharpDamageMultiplier` 사용
  - Blunt 카테고리 → `BluntDamageMultiplier` 사용
  - Heat 카테고리 → multStat 없음

### 3. 소재별 Stat 값 확인
- **소스코드 위치**: `RimWorldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml`
- **확인 사항**:
  - 주요 소재별 `SharpDamageMultiplier` 값
  - 주요 소재별 `BluntDamageMultiplier` 값
  - 소재별 특징 및 용도

### 4. 관통력 계산 로직
- **소스코드 위치**: `RimworldSource/Verse/VerbProperties.cs`
- **확인 사항**:
  - `AdjustedArmorPenetration` 메서드 분석
  - 관통력이 피해량과의 관계
  - 소재가 관통력에 영향을 주는지 여부

### 5. 관련 Stat 정리
- **확인 사항**:
  - `MeleeWeapon_DamageMultiplier` (품질 배율)
  - `SharpDamageMultiplier` (소재별 날카로운 공격 배율)
  - `BluntDamageMultiplier` (소재별 둔기 공격 배율)
  - `MeleeDamageFactor` (Pawn 스탯)

## 예상 결과물

### 1. 피해량 계산 공식 정리
```
최종 피해량 = Tool.power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier × GetDamageFactorFor
```

### 2. 소재별 피해량 배율 표
- 주요 소재별 SharpDamageMultiplier, BluntDamageMultiplier 값
- 소재별 특징 및 추천 용도

### 3. 관통력 계산 방식 정리
- 관통력이 피해량에 비례하는지 확인
- 소재가 관통력에 영향을 주는지 확인

### 4. Stat 영향도 정리
- 각 Stat이 피해량에 미치는 영향도
- Stat 간 우선순위 및 적용 순서

## 조사 완료 항목
- ✅ 근접 무기 피해량 계산 로직 확인
- ✅ DamageDef의 armorCategory.multStat 확인
- ✅ 소재별 Stat 값 확인 (일부)
- ✅ 관통력 계산 로직 확인

## 다음 단계
1. 소재별 피해량 배율 표 작성
2. 관통력과 피해량의 관계 정리
3. 실제 계산 예시 작성
4. 보고서 작성

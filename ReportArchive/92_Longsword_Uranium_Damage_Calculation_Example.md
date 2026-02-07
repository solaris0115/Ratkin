# 롱소드(우라늄 소재) 피해량 계산 예시

## 무기 정보

### 롱소드 (MeleeWeapon_LongSword)
**소재**: 우라늄 (Uranium)

### Tools 정의
| Tool | Capacity | Power | Cooldown | DamageDef | armorCategory |
|------|----------|-------|-----------|------------|---------------|
| handle | Blunt | 9 | 2.0 | Blunt | Blunt |
| point | Stab | 23 | 2.6 | Stab | Sharp |
| edge | Cut | 23 | 2.6 | Cut | Sharp |

### 우라늄 소재 Stat 값
- **SharpDamageMultiplier**: 1.1
- **BluntDamageMultiplier**: 1.5
- **MeleeWeapon_CooldownMultiplier**: 1.10 (공격 딜레이 증가)

## 피해량 계산 공식

```
최종 피해량 = Tool.power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier × GetDamageFactorFor
```

**관통력 계산** (armorPenetration이 -1인 경우):
```
관통력 = 최종 피해량 × 0.015
```

## 각 Tool별 피해량 계산 (Normal 품질 기준)

### 가정 조건
- **품질**: Normal (MeleeWeapon_DamageMultiplier = 1.0)
- **Pawn 스탯**: MeleeDamageFactor = 1.0 (기본값)
- **BodyPart 효율**: 100% (정상 상태)
- **LifeStage**: 성인 (meleeDamageFactor = 1.0)

---

### 1. handle Tool (Blunt 공격)

**DamageDef**: Blunt  
**armorCategory**: Blunt → **BluntDamageMultiplier** 사용

#### 피해량 계산
```
피해량 = Tool.power × MeleeWeapon_DamageMultiplier × BluntDamageMultiplier × GetDamageFactorFor
      = 9 × 1.0 × 1.5 × 1.0
      = 13.5
```

#### 관통력 계산
```
관통력 = 피해량 × 0.015 (armorPenetration이 -1이므로)
       = 13.5 × 0.015
       = 0.2025 (20.25%)
```

**결과**:
- **피해량**: 13.5
- **관통력**: 20.25%

---

### 2. point Tool (Stab 공격)

**DamageDef**: Stab  
**armorCategory**: Sharp → **SharpDamageMultiplier** 사용

#### 피해량 계산
```
피해량 = Tool.power × MeleeWeapon_DamageMultiplier × SharpDamageMultiplier × GetDamageFactorFor
      = 23 × 1.0 × 1.1 × 1.0
      = 25.3
```

#### 관통력 계산
```
관통력 = 피해량 × 0.015 (armorPenetration이 -1이므로)
       = 25.3 × 0.015
       = 0.3795 (37.95%)
```

**결과**:
- **피해량**: 25.3
- **관통력**: 37.95%

---

### 3. edge Tool (Cut 공격)

**DamageDef**: Cut  
**armorCategory**: Sharp → **SharpDamageMultiplier** 사용

#### 피해량 계산
```
피해량 = Tool.power × MeleeWeapon_DamageMultiplier × SharpDamageMultiplier × GetDamageFactorFor
      = 23 × 1.0 × 1.1 × 1.0
      = 25.3
```

#### 관통력 계산
```
관통력 = 피해량 × 0.015 (armorPenetration이 -1이므로)
       = 25.3 × 0.015
       = 0.3795 (37.95%)
```

**결과**:
- **피해량**: 25.3
- **관통력**: 37.95%

---

## 품질별 비교 (Legendary 품질)

### Legendary 품질 (MeleeWeapon_DamageMultiplier = 1.65)

| Tool | Power | 소재 배율 | Normal 품질 | Legendary 품질 | 증가율 |
|------|-------|-----------|-------------|----------------|--------|
| handle (Blunt) | 9 | 1.5 | **13.5** | **22.275** | +65% |
| point (Stab) | 23 | 1.1 | **25.3** | **41.745** | +65% |
| edge (Cut) | 23 | 1.1 | **25.3** | **41.745** | +65% |

---

## 핵심 포인트

### 1. 소재 배율 적용 방식
- **각 DamageDef의 armorCategory에 따라 다른 소재 배율 적용**
  - Stab (Sharp) → SharpDamageMultiplier (1.1)
  - Cut (Sharp) → SharpDamageMultiplier (1.1)
  - Blunt (Blunt) → BluntDamageMultiplier (1.5)

### 2. 우라늄 소재의 특징
- **Blunt 공격에 유리**: BluntDamageMultiplier 1.5로 둔기 공격이 50% 증가
- **Sharp 공격에 보통**: SharpDamageMultiplier 1.1로 날카로운 공격이 10% 증가
- **공격 속도 감소**: MeleeWeapon_CooldownMultiplier 1.10으로 공격 딜레이 10% 증가

### 3. 계산 순서
1. Tool.power (기본값)
2. MeleeWeapon_DamageMultiplier (품질 배율)
3. **StuffDamageMultiplier** (소재 배율 - DamageDef의 armorCategory에 따라 결정)
4. GetDamageFactorFor (Pawn 스탯, BodyPart 효율 등)

### 4. 관통력과의 관계
- 관통력은 **최종 피해량에 비례**하여 계산됨
- 피해량이 높을수록 관통력도 높아짐
- 소재 배율이 피해량에 영향을 주므로, 간접적으로 관통력에도 영향

---

## 비교: Steel 소재 vs 우라늄 소재

### Steel 소재 (SharpDamageMultiplier: 1.0, BluntDamageMultiplier: 1.0)

| Tool | Power | Steel 피해량 | 우라늄 피해량 | 차이 |
|------|-------|-------------|--------------|------|
| handle (Blunt) | 9 | 9.0 | **13.5** | +50% |
| point (Stab) | 23 | 23.0 | **25.3** | +10% |
| edge (Cut) | 23 | 23.0 | **25.3** | +10% |

**결론**: 우라늄 소재는 특히 **Blunt 공격에 강점**을 보이며, Sharp 공격에도 약간의 이점이 있습니다.

---

## 관련 파일
- `RimWorldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml` (롱소드 정의)
- `RimWorldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml` (우라늄 소재 정의)
- `RimWorldData/Core/Defs/DamageDefs/Damages_MeleeWeapon.xml` (DamageDef 정의)
- `RimworldSource/Verse/Tool.cs` (피해량 계산 로직)
- `RimworldSource/Verse/VerbProperties.cs` (관통력 계산 로직)

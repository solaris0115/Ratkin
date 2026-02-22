# 건랜스 공격력 및 방어관통 계산 분석 보고서

## 개요
RK_Gunlance_NormalType 무기를 기준으로 공격력과 방어관통이 어떻게 계산되는지 분석하고, 품질(Normal vs Legendary)에 따른 차이를 계산합니다.

## 1. 계산 로직 분석

### 1.1 공격력 계산 (AdjustedMeleeDamageAmount)

**소스코드 위치**: `RimworldSource/Verse/VerbProperties.cs`, `RimworldSource/Verse/Tool.cs`

#### 계산 공식
```
공격력 = Tool.power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier × GetDamageFactorFor
```

**단계별 계산:**

1. **Tool.power** (기본값)
   - point: 20
   - edge: 25

2. **MeleeWeapon_DamageMultiplier** (품질 배율)
   - 품질별 배율 (`RimWorldData/Core/Defs/Stats/Stats_Weapons_Melee.xml`):
     - Awful: 0.8
     - Poor: 0.9
     - **Normal: 1.0**
     - Good: 1.1
     - Excellent: 1.2
     - Masterwork: 1.45
     - **Legendary: 1.65**

3. **StuffDamageMultiplier** (재료 배율)
   - Steel의 경우: SharpDamageMultiplier = 1.0 (기본값)
   - 재료별로 다른 배율 적용 가능

4. **GetDamageFactorFor** (Pawn 관련 배율)
   - Pawn의 MeleeDamageFactor 스탯
   - BodyPart 효율 (부상 등)
   - LifeStage meleeDamageFactor

#### 코드 확인

```85:96:RimworldSource/Verse/Tool.cs
		public float AdjustedBaseMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
		{
			float num = this.power;
			if (ownerEquipment != null)
			{
				num *= ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, true, -1);
				if (ownerEquipment.Stuff != null && damageDef != null)
				{
					num *= ownerEquipment.Stuff.GetStatValueAbstract(damageDef.armorCategory.multStat, null);
				}
			}
			return num;
		}
```

```352:372:RimworldSource/Verse/VerbProperties.cs
		public float AdjustedMeleeDamageAmount(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			if (!this.IsMeleeAttack)
			{
				Log.ErrorOnce(string.Format("Attempting to get melee damage for a non-melee verb {0}", this), 26181238);
			}
			float num;
			if (tool != null)
			{
				num = tool.AdjustedBaseMeleeDamageAmount(equipment, this.meleeDamageDef);
			}
			else
			{
				num = (float)this.meleeDamageBaseAmount;
			}
			if (attacker != null)
			{
				num *= this.GetDamageFactorFor(tool, attacker, hediffCompSource);
			}
			return num;
		}
```

### 1.2 방어관통 계산 (AdjustedArmorPenetration)

**소스코드 위치**: `RimworldSource/Verse/VerbProperties.cs`

#### 계산 공식

**Case 1: Tool.armorPenetration이 -1인 경우 (기본값)**
```
방어관통 = 공격력 × 0.015
```

**Case 2: Tool.armorPenetration이 지정된 경우**
```
방어관통 = Tool.armorPenetration × MeleeWeapon_DamageMultiplier
```

#### 코드 확인

```406:427:RimworldSource/Verse/VerbProperties.cs
		public float AdjustedArmorPenetration(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			float num;
			if (tool != null)
			{
				num = tool.armorPenetration;
			}
			else
			{
				num = this.meleeArmorPenetrationBase;
			}
			if (num < 0f)
			{
				num = this.AdjustedMeleeDamageAmount(tool, attacker, equipment, hediffCompSource) * 0.015f;
			}
			else if (equipment != null)
			{
				float statValue = equipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, true, -1);
				num *= statValue;
			}
			return num;
		}
```

## 2. RK_Gunlance_NormalType 무기 분석

### 2.1 무기 정의 확인

```39:56:Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
		<tools>
			<li>
				<label>point</label>
				<capacities>
					<li>Stab</li>
				</capacities>
				<power>20</power>
				<cooldownTime>3</cooldownTime>
			</li>
			<li>
				<label>edge</label>
				<capacities>
					<li>GunlanceShell_Normal</li>
				</capacities>
				<power>25</power>
				<cooldownTime>3</cooldownTime>
			</li>
		</tools>
```

**특징:**
- **point**: power 20, armorPenetration 미지정 (기본값 -1)
- **edge**: power 25, armorPenetration 미지정 (기본값 -1)
- 무기 재료: Steel (stuffCategories: Metallic)

### 2.2 Verb_GunlanceFiring에서의 사용

```84:101:Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs
			// 근접 무기 데미지 및 방어구 관통력 계산 (Tool의 power 기반, 품질 영향 포함)
			float calculatedDamage = this.verbProps.AdjustedMeleeDamageAmount(this, this.CasterPawn);
			float calculatedArmorPen = this.verbProps.AdjustedArmorPenetration(this, this.CasterPawn);

			int finalDamage = GenMath.RoundRandom(calculatedDamage);

			Thing weapon = CasterPawn.equipment.Primary;
			GenExplosion.DoExplosion(
				center: CasterPawn.Position,
				map: CasterPawn.Map,
				radius: verbProperties.range,
				damType: verbProperties.damageDef ?? DamageDefOf.Bomb,
				instigator: CasterPawn,
				damAmount: finalDamage,
				armorPenetration: calculatedArmorPen,
				weapon: weapon?.def,
				screenShakeFactor: 0f,
				overrideCells: explosionCells);
```

**핵심:**
- `edge` tool이 사용될 때 `AdjustedMeleeDamageAmount`와 `AdjustedArmorPenetration` 호출
- Tool.power = 25를 기반으로 계산

## 3. 실제 계산 결과

### 3.1 가정 조건
- **무기 재료**: Steel (SharpDamageMultiplier = 1.0)
- **Pawn 스탯**: MeleeDamageFactor = 1.0 (기본값)
- **BodyPart 효율**: 100% (정상 상태)
- **LifeStage**: 성인 (meleeDamageFactor = 1.0)

### 3.2 Edge Tool (GunlanceShell_Normal) 계산

#### A. Normal 품질

**공격력:**
```
공격력 = Tool.power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier × GetDamageFactorFor
      = 25 × 1.0 × 1.0 × 1.0
      = 25.0
```

**방어관통:**
```
방어관통 = 공격력 × 0.015 (armorPenetration이 -1이므로)
         = 25.0 × 0.015
         = 0.375 (37.5%)
```

#### B. Legendary 품질

**공격력:**
```
공격력 = Tool.power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier × GetDamageFactorFor
      = 25 × 1.65 × 1.0 × 1.0
      = 41.25
```

**방어관통:**
```
방어관통 = 공격력 × 0.015 (armorPenetration이 -1이므로)
         = 41.25 × 0.015
         = 0.61875 (61.875%)
```

### 3.3 Point Tool (Stab) 계산 (참고)

#### A. Normal 품질

**공격력:**
```
공격력 = 20 × 1.0 × 1.0 × 1.0 = 20.0
```

**방어관통:**
```
방어관통 = 20.0 × 0.015 = 0.3 (30%)
```

#### B. Legendary 품질

**공격력:**
```
공격력 = 20 × 1.65 × 1.0 × 1.0 = 33.0
```

**방어관통:**
```
방어관통 = 33.0 × 0.015 = 0.495 (49.5%)
```

## 4. 품질별 차이 비교표

### 4.1 Edge Tool (GunlanceShell_Normal)

| 품질 | MeleeWeapon_DamageMultiplier | 공격력 | 방어관통 | 비고 |
|------|------------------------------|--------|----------|------|
| Normal | 1.0 | **25.0** | **37.5%** | 기준값 |
| Legendary | 1.65 | **41.25** | **61.875%** | +65% 데미지, +65% 관통 |

**차이:**
- 공격력: Legendary가 Normal보다 **+16.25 (65% 증가)**
- 방어관통: Legendary가 Normal보다 **+24.375%p (65% 증가)**

### 4.2 Point Tool (Stab)

| 품질 | MeleeWeapon_DamageMultiplier | 공격력 | 방어관통 | 비고 |
|------|------------------------------|--------|----------|------|
| Normal | 1.0 | **20.0** | **30%** | 기준값 |
| Legendary | 1.65 | **33.0** | **49.5%** | +65% 데미지, +65% 관통 |

**차이:**
- 공격력: Legendary가 Normal보다 **+13.0 (65% 증가)**
- 방어관통: Legendary가 Normal보다 **+19.5%p (65% 증가)**

## 5. 실제 게임에서의 적용

### 5.1 Verb_GunlanceFiring 실행 시

1. **edge tool 선택** (GunlanceShell_Normal capacity)
2. **공격력 계산**:
   - `AdjustedMeleeDamageAmount` 호출
   - Tool.power (25) × 품질 배율 × 기타 배율
3. **방어관통 계산**:
   - `AdjustedArmorPenetration` 호출
   - armorPenetration이 -1이므로 공격력 × 0.015
4. **폭발 데미지 적용**:
   - `GenExplosion.DoExplosion`에 계산된 데미지와 관통 전달
   - 최종 데미지는 `GenMath.RoundRandom`로 반올림됨

### 5.2 데미지 반올림

**Normal 품질:**
- 공격력: 25.0 → **25** (반올림)
- 방어관통: 0.375 → **0.375** (소수점 유지)

**Legendary 품질:**
- 공격력: 41.25 → **41** (반올림)
- 방어관통: 0.61875 → **0.61875** (소수점 유지)

## 6. 추가 고려사항

### 6.1 Pawn 스탯 영향

**MeleeDamageFactor**가 1.0이 아닌 경우:
- 예: MeleeDamageFactor = 1.2인 경우
  - Normal: 25 × 1.2 = 30.0
  - Legendary: 41.25 × 1.2 = 49.5

### 6.2 재료 영향

**Steel이 아닌 다른 재료**를 사용하는 경우:
- 예: Plasteel의 경우 SharpDamageMultiplier가 다를 수 있음
- 실제 재료의 `SharpDamageMultiplier` 스탯 값 확인 필요

### 6.3 ManeuverDef의 damageAmount

**주의**: ManeuverDef에 `damageAmount: 15`가 있지만, 실제 계산에는 사용되지 않습니다.

```90:99:Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
	<ManeuverDef>
		<defName>RK_GunlanceExplosion_Normal</defName>
		<requiredCapacity>GunlanceShell_Normal</requiredCapacity>
		<verb Class="NewRatkin.VerbProperties_Gunlance">
			<verbClass>NewRatkin.Verb_GunlanceFiring</verbClass>
			<damageDef>Bomb</damageDef>
			<damageAmount>15</damageAmount>
			<range>1</range>
			<angle>50</angle>	
		</verb>
```

**실제 사용**: `Verb_GunlanceFiring`에서는 `AdjustedMeleeDamageAmount`를 사용하므로 **Tool.power 기반**으로 계산됩니다.

## 7. 결론

### 7.1 Edge Tool (GunlanceShell_Normal)

**Normal 품질:**
- 공격력: **25.0**
- 방어관통: **37.5%**

**Legendary 품질:**
- 공격력: **41.25** (+65%)
- 방어관통: **61.875%** (+65%)

### 7.2 핵심 발견

1. **공격력 계산**: Tool.power (25) × 품질 배율 × 재료 배율 × Pawn 배율
2. **방어관통 계산**: armorPenetration이 -1이면 공격력 × 0.015로 자동 계산
3. **품질 영향**: Legendary는 Normal보다 **65% 더 높은 데미지와 관통력** 제공
4. **실제 데미지**: `GenMath.RoundRandom`로 반올림되어 정수로 적용

## 관련 파일 목록
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml`
- `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs`
- `RimworldSource/Verse/VerbProperties.cs`
- `RimworldSource/Verse/Tool.cs`
- `RimWorldData/Core/Defs/Stats/Stats_Weapons_Melee.xml`


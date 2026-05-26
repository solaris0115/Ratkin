# 1.5 랫킨 방패 도탄 규칙 (AI 참조용)

> **태그**: Ratkin Shield Deflection 1.5 WoodenShield HeavyShield RK_HeavyShield_Big CheckPreAbsorbDamage

## 1. 개요

1.5 방패는 `Apparel.CheckPreAbsorbDamage`를 오버라이드하여, Pawn이 피해를 받기 직전에 **확률적으로 데미지를 완전 차단**하는 도탄 시스템이다. 3종 방패 모두 동일한 `NewRatkin.Shield` 클래스를 사용하며, 소재에 따라 방어력만 달라진다.

## 2. 전제 조건

도탄 판정 자체가 실행되려면 다음을 만족해야 한다.

- 착용자가 살아있어야 함 (Dead가 아님)
- 착용자가 쓰러지지 않았어야 함 (Downed가 아님)

이 조건을 만족하지 않으면 도탄 로직을 건너뛰고 데미지가 그대로 적용된다.

## 3. 도탄 범위 (방향 조건)

방패는 **정면 공격만** 막을 수 있다. 착용자가 바라보는 방향을 기준으로 **좌우 70도씩, 총 140도** 범위 내에서 들어온 공격만 도탄 판정 대상이 된다.

- 공격 방향은 `dinfo.Angle`(데미지 진행 방향)을 180도 반전하여 "공격자가 있는 방향"을 구한 뒤, 착용자의 `Rotation.AsAngle`(0/90/180/270 중 하나)과 비교한다.
- 두 각도의 차이가 ±70도를 넘으면 측면·후면 공격으로 간주하여 도탄 판정 자체를 하지 않는다.

## 4. 도탄 확률

범위 안의 공격이라면, **스킬 보너스 + 소재 보너스**를 합산한 값이 도탄 확률이 된다. 0~1 랜덤값이 이 확률 이하면 도탄 성공(데미지 0), 초과하면 도탄 실패(데미지 그대로).

### 4-1. 스킬 보너스

Melee 스킬 레벨에 비례하여 선형 증가한다. 레벨당 2%로, 최대 20레벨이면 40%.

| Melee | 스킬 보너스 |
|-------|------------|
| 0 | 0% |
| 5 | 10% |
| 10 | 20% |
| 15 | 30% |
| 20 | 40% |

### 4-2. 소재 보너스

공격 타입(Sharp/Blunt/Heat)에 대응하는 방패의 ArmorRating을 읽어 변환한다. 고방어력 소재가 과도하게 유리해지는 것을 막기 위해 상한이 걸려 있다.

변환 공식: `Clamp01(ArmorRating ÷ 2) ÷ 2` — 즉, ArmorRating을 절반으로 줄이고, 0~1로 자르고, 다시 절반. 최대 50%.

| ArmorRating | 소재 보너스 | 설명 |
|-------------|------------|------|
| 0.2 | 5% | 낮은 방어력 소재 |
| 0.5 | 12.5% | 일반적인 금속 수준 |
| 1.0 | 25% | 고급 소재 |
| ≥2.0 | 50% | 상한 도달 |

### 4-3. 종합 예시

Steel 소재 RK_HeavyShield, Melee 10, Sharp 공격 기준:
- 스킬 보너스: 10 × 2% = 20%
- ArmorRating_Sharp ≈ 0.54 → 소재 보너스 ≈ 13.5%
- **도탄 확률 ≈ 33.5%**

## 5. 방어력(ArmorRating) 구성

방패의 ArmorRating은 림월드 표준 계산식을 따른다.

**ArmorRating = (기본값 + 소재 계수 × 소재 방어력) × 품질 배율**

### 5-1. 방패별 기본값/계수

| DefName | 기본 Sharp | 기본 Blunt | 소재 계수(StuffEffectMultiplierArmor) | 소재 카테고리 | stuffCount |
|---------|-----------|-----------|--------------------------------------|--------------|------------|
| RK_WoodenShield | 0.57 | 0.57 | — | — | — |
| RK_HeavyShield | 0.1 | 0.0 | 0.80 | Metallic | 120 |
| RK_HeavyShield_Big | 0.1 | 0.15 | 0.85 | Metallic | 165 |

- WoodenShield는 stuff를 사용하지 않는 고정 비용 제작(Steel 25 + WoodLog 120). ArmorRating이 고정이라 소재 보너스 변동 없음.
- HeavyShield/HeavyShield_Big은 Metallic stuff를 사용하므로 Steel, Plasteel 등 소재에 따라 방어력이 달라진다.

### 5-2. 주요 Metallic 소재 방어력

| 소재 | StuffPower Sharp | StuffPower Blunt |
|------|-----------------|-----------------|
| Steel | 0.55 | 0.25 |
| Plasteel | 1.14 | 0.55 |
| Uranium | 0.55 | 0.55 |

### 5-3. 품질 배율

품질에 따라 ArmorRating에 곱해지는 배율. 전설 품질이면 1.8배.

| 품질 | 배율 |
|------|------|
| 조악 | 0.50 |
| 열등 | 0.65 |
| 보통 | 0.80 |
| 양호 | 0.90 |
| 우수 | 1.00 |
| 걸작 | 1.25 |
| 전설 | 1.80 |

## 6. 착용 패널티

방패가 무거울수록 회피·명중·이속에 패널티가 붙는다. WoodenShield는 패널티 없음.

| DefName | 회피(MeleeDodgeChance) | 명중(MeleeHitChance) | 이속(MoveSpeed) |
|---------|----------------------|---------------------|----------------|
| RK_WoodenShield | — | — | — |
| RK_HeavyShield | -10% | — | -0.1 |
| RK_HeavyShield_Big | -20% | -20% | -0.8 |

이 오프셋은 equippedStatOffsets로, 해당 스탯의 **Raw값에 가산**된다(131 보고서 참조).

## 7. 기타 규칙

- **도탄 성공 시**: "ShieldBlock" 텍스트 모트 + Deflect_Metal 이펙트가 표시됨.
- **원거리 발사 제한**: 1.5에서는 `AllowVerbCast`가 `Verb_LaunchProjectile`을 차단하므로, 방패 착용 중에는 원거리 무기를 발사할 수 없다.
- **근접/원거리 구분 없음**: 도탄 판정은 공격 타입(근접/원거리)을 가리지 않는다. 정면 ±70도 이내의 모든 공격이 대상.

## 소스
- `Project/1.5/Source/ShieldOfRatkinia/WoodenShield.cs` — Shield 클래스, CheckPreAbsorbDamage 구현
- `Project/1.5/Defs/ThingsDefs/RK_Apparel.xml` — 3종 방패 Def

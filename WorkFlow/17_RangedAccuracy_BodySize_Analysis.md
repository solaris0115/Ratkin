# 원거리 명중률과 BodySize 관계 분석 - 2025-10-15

## 분석 개요
원거리 공격에서 타겟의 bodySize가 명중률에 영향을 주는지, 그리고 회피 메커니즘이 존재하는지 확인

## 핵심 발견 사항

### 🎯 원거리 공격에서 bodySize는 명중률에 영향을 줍니다!

근접 회피율과 달리, **원거리 명중률은 타겟의 bodySize에 직접적으로 영향을 받습니다**.

## 1. ShotReport - 명중률 계산 시스템

**파일**: `RimworldSource/Verse/ShotReport.cs`

### A. factorFromTargetSize 계산 (lines 167-183)

```csharp
if (target.HasThing)
{
    Pawn pawn = target.Thing as Pawn;
    if (pawn != null)
    {
        // ★ Pawn의 경우: BodySize를 직접 사용!
        shotReport.factorFromTargetSize = pawn.BodySize;
    }
    else
    {
        // 건물/오브젝트의 경우: 크기 계산
        shotReport.factorFromTargetSize = target.Thing.def.fillPercent 
            * (float)target.Thing.def.size.x 
            * (float)target.Thing.def.size.z 
            * 2.5f;
    }
    
    // ★ 0.5 ~ 2.0으로 클램프 (최소 50%, 최대 200%)
    shotReport.factorFromTargetSize = Mathf.Clamp(
        shotReport.factorFromTargetSize, 
        0.5f, 
        2f
    );
}
else
{
    shotReport.factorFromTargetSize = 1f;
}
```

### B. 최종 명중률 계산 (lines 67-95)

```csharp
// 1. 기본 명중률 (표준 타겟 기준)
public float AimOnTargetChance_StandardTarget
{
    get
    {
        float num = this.factorFromShooterAndDist  // 사수 능력 + 거리
                  * this.factorFromEquipment        // 무기 정확도
                  * this.factorFromWeather          // 날씨
                  * this.factorFromCoveringGas      // 블라인드 스모크
                  * this.FactorFromExecution;       // 처형 보너스
        
        num += this.offsetFromDarkness;             // 어둠/밝기 보정
        
        if (num < 0.0201f)
            num = 0.0201f;  // 최소 명중률
        
        return num;
    }
}

// 2. 타겟 크기 적용
public float AimOnTargetChance_IgnoringPosture
{
    get
    {
        // ★ bodySize가 여기서 곱해짐!
        return this.AimOnTargetChance_StandardTarget * this.factorFromTargetSize;
    }
}

// 3. 자세 보정 적용
public float AimOnTargetChance
{
    get
    {
        // 엎드린 타겟은 50% 명중률
        return this.AimOnTargetChance_IgnoringPosture * this.FactorFromPosture;
    }
}

// 4. 최종 명중률 (엄폐물 포함)
public float TotalEstimatedHitChance
{
    get
    {
        return Mathf.Clamp01(this.AimOnTargetChance * this.PassCoverChance);
    }
}
```

### C. BodySize 범위 제한 (ShootTuning.cs)

```csharp
public const float TargetSizeFactorFromFillPercentFactor = 2.5f;
public const float TargetSizeFactorMin = 0.5f;   // 최소 50%
public const float TargetSizeFactorMax = 2f;     // 최대 200%
```

## 2. Human vs Ratkin 비교

### A. BodySize 값
| 종족 | baseBodySize | factorFromTargetSize | 명중률 영향 |
|------|-------------|---------------------|------------|
| Human | 1.0 | 1.0 | 기준 (100%) |
| Ratkin | 0.8 | 0.8 | **-20%** |

### B. 실제 명중률 계산 예시

**조건**: 
- 거리: 20칸
- 무기: Assault Rifle (정확도 90%)
- 사수: Shooting 10 스킬
- 날씨: 맑음
- 엄폐물: 없음

**Human 타겟**:
```
기본 명중률: 45%
× factorFromTargetSize (1.0): 45%
× 엄폐물 (1.0): 45%
최종 명중률: 45%
```

**Ratkin 타겟**:
```
기본 명중률: 45%
× factorFromTargetSize (0.8): 36%  ← ★ -9% 감소!
× 엄폐물 (1.0): 36%
최종 명중률: 36%
```

**결과**: Ratkin은 Human보다 **명중당할 확률이 20% 낮습니다** (45% → 36%)

## 3. 원거리 회피 메커니즘

### 투사체는 회피할 수 없음

**중요**: 원거리 공격에는 **회피 메커니즘이 존재하지 않습니다**.

```csharp
// Projectile.cs - 투사체는 발사되면 목표로 직진
protected virtual void Impact(Thing hitThing, bool blockedByShield = false)
{
    // 회피 체크 없음
    // 실드로 막히거나, 명중하거나 둘 중 하나
}
```

**명중률 vs 회피**:
- **근접 공격**: 명중 판정 → 회피 판정 (2단계)
- **원거리 공격**: 명중 판정만 (1단계)
  - 명중하면 타격 확정
  - 빗나가면 완전히 빗나감

## 4. 전체 명중률 공식

```
최종 명중률 = 기본 명중률 × bodySize × 자세 보정 × 엄폐물 통과율

여기서:
- 기본 명중률 = 사수능력 × 무기정확도 × 날씨 × 가스 × 처형보너스 + 밝기보정
- bodySize = 0.5 ~ 2.0 (Pawn의 BodySize)
- 자세 보정 = 1.0 (서있음) / 0.5 (엎드림) / 7.5 (처형 거리)
- 엄폐물 통과율 = 1.0 - 엄폐물 차단률
```

## 5. Ratkin의 전투 이점

### A. 원거리 전투
bodySize 0.8 덕분에:
- **적의 명중률 -20%** (받는 피해 감소)
- 작은 체구 = 맞기 어려운 타겟
- 엄폐물과 조합 시 생존력 극대화

### B. 근접 전투
MeleeDodgeChance 1.15 덕분에:
- **회피율 +1~2%** (근접 공격 회피)
- bodySize와 무관한 순수 회피 보너스

### C. 운반 능력
bodySize 0.8 때문에:
- **CarryingCapacity 감소** (45 × 0.8 = 36)
- Human의 48% 수준

## 6. 명중률에 영향을 주는 모든 요소

### 사수 측 요소
1. **ShootingAccuracyPawn** 스탯
2. **거리별 정확도 보정**:
   - Touch (0-3칸)
   - Short (3-12칸)
   - Medium (12-25칸)
   - Long (25-40칸)
3. **무기 정확도**
4. **날씨** (비, 눈)
5. **블라인드 스모크** (70% 페널티)
6. **조명 환경** (Ideology DLC)

### 타겟 측 요소
1. **bodySize** (0.5~2.0 배율)
2. **자세** (서있음/엎드림/처형)
3. **엄폐물** (벽, 바리케이드 등)
4. **이동 상태** (이동 중 명중률 동일)

## 7. 게임 디자인 분석

### 왜 원거리에는 회피가 없을까?

1. **현실성**: 총알은 회피할 수 없음
2. **엄폐물 시스템**: 회피 대신 엄폐물로 방어
3. **균형**: 원거리 무기가 너무 약하면 게임이 지루함
4. **타겟 크기**: bodySize로 "맞기 어려움"을 표현

### 근접 vs 원거리 생존 메커니즘

| 공격 유형 | 방어 메커니즘 | bodySize 영향 |
|----------|--------------|---------------|
| 근접 | 회피율 (MeleeDodgeChance) | ❌ 없음 |
| 원거리 | 명중률 감소 (bodySize) | ✅ -20% |
| 원거리 | 엄폐물 차단 | 동일 |

## 8. Ratkin의 최적 전투 전술

### 권장 전술
1. **원거리 전투 선호**
   - bodySize 0.8로 생존력 우수
   - 엄폐물 활용 시 명중률 대폭 감소

2. **기동전**
   - 작은 체구 활용
   - 빠른 이동으로 엄폐 전환

3. **근접 회피**
   - MeleeDodgeChance 보너스 활용
   - 단, 운반력 부족 고려

### 주의사항
- CarryingCapacity 36으로 무거운 장비 제한
- 원거리 무기 위주 편성 필요

## 9. 데미지 계산

### 원거리 데미지는 고정값

투사체가 명중하면:
```csharp
public virtual int DamageAmount
{
    get
    {
        return this.def.projectile.GetDamageAmount(this.equipment, null);
    }
}
```

- **bodySize는 데미지에 영향 없음**
- **회피 메커니즘 없음**
- 명중 = 풀 데미지
- 빗나감 = 0 데미지

### 근접 데미지는 가변값

```csharp
// 80~120% 랜덤 데미지
num = Rand.Range(num * 0.8f, num * 1.2f);
```

## 결론

### 핵심 요약

1. **원거리 명중률에 bodySize 영향 있음!**
   - Ratkin (0.8): Human 대비 **-20% 명중률**
   - 적이 Ratkin을 맞추기 더 어려움

2. **원거리 회피는 없음**
   - 명중 판정만 존재
   - 회피 판정 없음
   - 투사체는 발사되면 회피 불가

3. **근접 vs 원거리 차이**
   - 근접: bodySize 무관, 회피율로 방어
   - 원거리: bodySize로 명중률 감소, 회피 없음

4. **Ratkin의 전투 특성**
   - 원거리 전투에서 생존력 우수 (받는 명중률 -20%)
   - 근접 전투에서 회피력 우수 (회피율 +1~2%)
   - 운반력 부족 (36)

### 전술적 함의

Ratkin은 **원거리 전투에 유리한 종족**입니다:
- 작은 체구로 총알 피하기 쉬움
- 엄폐물 활용 시 생존력 극대화
- 기동성 높은 사격수에 최적화

## 참고 파일

### 소스코드
- `RimworldSource/Verse/ShotReport.cs` - 명중률 계산
- `RimworldSource/Verse/ShootTuning.cs` - bodySize 제한값
- `RimworldSource/Verse/Projectile.cs` - 투사체 메커니즘
- `RimworldSource/RimWorld/Verb_MeleeAttack.cs` - 근접 공격 비교

### Ratkin 설정
- `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml`
  - baseBodySize: 0.8
  - MeleeDodgeChance: 1.15

## 추가 확인 사항

### ✅ 확인 완료
- [x] 원거리 명중률에 bodySize 영향 여부
- [x] 원거리 회피 메커니즘 존재 여부
- [x] 데미지 계산에 bodySize 영향 여부
- [x] Human vs Ratkin 명중률 차이

### 결과
- ✅ bodySize는 원거리 명중률에 직접 영향 (×0.8)
- ❌ 원거리 회피 메커니즘은 없음
- ❌ bodySize는 데미지에 영향 없음 (명중 시 고정 데미지)


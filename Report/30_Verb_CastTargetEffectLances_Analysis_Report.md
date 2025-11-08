# Verb_CastTargetEffectLances 상세 분석 보고서

## 개요

`Verb_CastTargetEffectLances`는 RimWorld에서 사이킥 랜스(Lance) 장비에 사용되는 특수 Verb 클래스입니다. 타겟에게 사이킥 효과를 적용하는 장비(주로 사이킥 쇼크 랜스, 사이킥 인새니티 랜스 등)에 사용됩니다.

---

## 1. 클래스 정의 및 계층 구조

### 1.1 클래스 정보

- **네임스페이스**: `Verse.AI`
- **상속**: `Verb_CastTargetEffect`
- **위치**: `RimworldSource/Verse/AI/Verb_CastTargetEffectLances.cs`
- **특징**: 사이킥 랜스 전용 타겟 검증 로직 포함

### 1.2 계층 구조

```
Verb (abstract)
└── Verb_CastBase (abstract)
    └── Verb_CastTargetEffect
        └── Verb_CastTargetEffectLances
```

### 1.3 부모 클래스 역할

#### Verb_CastBase (기본 시전 클래스)
- **기능**: 장비의 시전 기본 동작
- **특징**: 
  - `MultiSelect = true` (다중 선택 가능)
  - 장비 재장전 가능 여부 확인
  - 타겟 하이라이트 및 범위 표시

#### Verb_CastTargetEffect (타겟 효과 시전)
- **핵심 메서드**: `TryCastShot()`
- **기능**: 
  - 장비의 모든 `CompTargetEffect` 컴포넌트를 순회
  - 각 컴포넌트의 `DoEffectOn()` 메서드 호출
  - 장비 재장전 가능한 경우 사용 횟수 차감

```csharp
protected override bool TryCastShot()
{
    Pawn casterPawn = this.CasterPawn;
    Thing thing = this.currentTarget.Thing;
    if (casterPawn == null || thing == null)
    {
        return false;
    }
    foreach (CompTargetEffect compTargetEffect in base.EquipmentSource.GetComps<CompTargetEffect>())
    {
        compTargetEffect.DoEffectOn(casterPawn, thing);
    }
    CompApparelReloadable reloadableCompSource = base.ReloadableCompSource;
    if (reloadableCompSource != null)
    {
        reloadableCompSource.UsedOnce();
    }
    return true;
}
```

---

## 2. Verb_CastTargetEffectLances의 특수 기능

### 2.1 주요 기능

`Verb_CastTargetEffectLances`는 부모 클래스 `Verb_CastTargetEffect`의 기능을 상속받으면서, **사이킥 랜스 전용 타겟 검증 로직**을 추가로 구현합니다.

### 2.2 오버라이드 메서드

#### A. ValidateTarget() - 타겟 검증

타겟이 유효한지 검증하는 메서드입니다. 다음과 같은 조건을 확인합니다:

1. **보스 타겟 불가**
   ```csharp
   if (target.Pawn.kindDef.isBoss)
   {
       return false;
   }
   ```

2. **사이킥 감도 확인**
   ```csharp
   if (pawn.GetStatValue(StatDefOf.PsychicSensitivity, true, -1) <= 0f)
   {
       return false;
   }
   ```
   - 타겟의 사이킥 감도가 0 이하면 사용 불가
   - 사이킥적으로 귀먹은(psychically deaf) 타겟은 효과가 없음

3. **CompTargetEffect의 CanApplyOn() 확인**
   ```csharp
   foreach (CompTargetEffect compTargetEffect in base.EquipmentSource.GetComps<CompTargetEffect>())
   {
       if (!compTargetEffect.CanApplyOn(target.Pawn))
       {
           return false;
       }
   }
   ```
   - 장비의 모든 `CompTargetEffect` 컴포넌트가 적용 가능한지 확인
   - 하나라도 적용 불가능하면 사용 불가

#### B. OnGUI() - UI 피드백

타겟 선택 시 UI 피드백을 제공합니다:

1. **유효한 타겟인 경우**
   - 일반적인 타겟 마커 표시

2. **보스 타겟인 경우**
   ```csharp
   GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
   Widgets.MouseAttachedLabel(this.verbProps.invalidTargetPawn.CapitalizeFirst(), 0f, -20f, null);
   ```
   - 사용 불가 아이콘 표시
   - `invalidTargetPawn` 메시지 표시 (예: "psychic shock immune")

3. **사이킥 감도가 없는 경우**
   ```csharp
   GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
   Widgets.MouseAttachedLabel("CannotShootPawnIsPsychicallyDeaf".Translate(pawn), 0f, -20f, null);
   ```
   - "사이킥적으로 귀먹은" 메시지 표시

---

## 3. RimWorld에서의 사용 예시

### 3.1 사용되는 장비

#### A. Psychic Shock Lance (사이킥 쇼크 랜스)
**위치**: `RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Utility.xml`

```xml
<ThingDef>
    <defName>Apparel_PsychicShockLance</defName>
    <verbs>
        <li>
            <verbClass>Verb_CastTargetEffectLances</verbClass>
            <label>psychic shock lance</label>
            <range>41.9</range>
            <warmupTime>2.2</warmupTime>
            <invalidTargetPawn>psychic shock immune</invalidTargetPawn>
            <targetParams>
                <canTargetBuildings>false</canTargetBuildings>
                <neverTargetIncapacitated>true</neverTargetIncapacitated>
            </targetParams>
        </li>
    </verbs>
    <comps>
        <li Class="CompProperties_ApparelReloadable">
            <maxCharges>2</maxCharges>
            <destroyOnEmpty>true</destroyOnEmpty>
        </li>
        <li>
            <compClass>CompTargetEffect_PsychicShock</compClass>
        </li>
        <li Class="CompProperties_TargetEffect_BrainDamageChance">
            <brainDamageChance>0.3</brainDamageChance>
        </li>
        <li Class="CompProperties_TargetEffect_GoodwillImpact">
            <goodwillImpact>-200</goodwillImpact>
        </li>
        <li Class="CompProperties_TargetEffect_FleckOnTarget">
            <fleckDef>PsycastPsychicEffect</fleckDef>
        </li>
        <li Class="CompProperties_TargetEffect_FleckConnecting">
            <fleckDef>PsycastPsychicLine</fleckDef>
        </li>
    </comps>
</ThingDef>
```

**효과**:
- `CompTargetEffect_PsychicShock`: 사이킥 쇼크 헤딥 적용
- `CompProperties_TargetEffect_BrainDamageChance`: 30% 확률로 뇌 손상
- `CompProperties_TargetEffect_GoodwillImpact`: 관계도 -200 (적대 관계)
- 시각 효과: 사이킥 이펙트 및 연결선 표시

#### B. Psychic Insanity Lance (사이킥 인새니티 랜스)
**위치**: `RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Utility.xml`

```xml
<ThingDef>
    <defName>Apparel_PsychicInsanityLance</defName>
    <verbs>
        <li>
            <verbClass>Verb_CastTargetEffectLances</verbClass>
            <label>psychic insanity lance</label>
            <range>41.9</range>
            <warmupTime>2.2</warmupTime>
            <invalidTargetPawn>psychic insanity immune</invalidTargetPawn>
        </li>
    </verbs>
    <comps>
        <li>
            <compClass>CompTargetEffect_Berserk</compClass>
        </li>
        <!-- 기타 컴포넌트 동일 -->
    </comps>
</ThingDef>
```

**효과**:
- `CompTargetEffect_Berserk`: 광기 상태 적용
- 나머지 효과는 쇼크 랜스와 동일

#### C. Shard Shock Lance (샤드 쇼크 랜스) - Anomaly DLC
**위치**: `RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Utility.xml`

- 범위: 16.9 (더 짧음)
- 워밍업 시간: 2.4초
- 효과는 기본 쇼크 랜스와 동일

---

## 4. CompTargetEffect 시스템

### 4.1 CompTargetEffect 기본 클래스

**위치**: `RimworldSource/RimWorld/CompTargetEffect.cs`

```csharp
public abstract class CompTargetEffect : ThingComp
{
    public abstract void DoEffectOn(Pawn user, Thing target);
    
    public virtual bool CanApplyOn(Thing target)
    {
        return true;
    }
}
```

### 4.2 주요 CompTargetEffect 구현체

#### A. CompTargetEffect_PsychicShock
- **효과**: 사이킥 쇼크 헤딥 적용
- **특징**: 
  - 의식 부위에 헤딥 적용
  - 뮤턴트 중 사이킥 쇼크 면역인 경우 적용 불가

#### B. CompTargetEffect_Berserk
- **효과**: 광기(MentalStateDefOf.Berserk) 상태 적용
- **용도**: 인새니티 랜스에 사용

#### C. CompTargetEffect_BrainDamageChance (CompProperties)
- **효과**: 확률적으로 뇌 손상 발생
- **기본 확률**: 30% (0.3)

#### D. CompTargetEffect_GoodwillImpact (CompProperties)
- **효과**: 관계도 영향
- **기본값**: -200 (적대 관계)

#### E. CompTargetEffect_FleckOnTarget (CompProperties)
- **효과**: 타겟에 시각 효과(플렉) 표시
- **기본 플렉**: `PsycastPsychicEffect`

#### F. CompTargetEffect_FleckConnecting (CompProperties)
- **효과**: 시전자와 타겟 사이 연결선 표시
- **기본 플렉**: `PsycastPsychicLine`

---

## 5. 동작 흐름

### 5.1 사용 시나리오

1. **플레이어가 사이킥 랜스로 타겟 선택**
   - `OnGUI()` 메서드 호출
   - 타겟 유효성 검증 (보스, 사이킥 감도, CanApplyOn)

2. **타겟 유효한 경우**
   - 워밍업 시작 (2.2초)
   - 타겟 하이라이트 표시

3. **워밍업 완료 후**
   - `TryCastShot()` 호출
   - `Verb_CastTargetEffect.TryCastShot()` 실행

4. **효과 적용**
   - 모든 `CompTargetEffect` 컴포넌트 순회
   - 각 컴포넌트의 `DoEffectOn()` 호출
   - 시각 효과 표시 (플렉, 연결선)

5. **재장전 가능 장비인 경우**
   - `CompApparelReloadable.UsedOnce()` 호출
   - 사용 횟수 차감 (최대 2회, 소진 시 파괴)

### 5.2 타겟 검증 순서

```
1. 타겟이 Pawn인가?
   └─ 아니면 → 일반 타겟 검증

2. 보스 타겟인가?
   └─ 예 → 사용 불가 (false 반환)

3. 사이킥 감도가 있는가?
   └─ 없음 → 사용 불가 (false 반환)

4. 모든 CompTargetEffect가 적용 가능한가?
   └─ 하나라도 불가능 → 사용 불가 (false 반환)

5. 모든 검증 통과 → 사용 가능
```

---

## 6. 특수 사항

### 6.1 사이킥 감도 요구사항

- 타겟은 반드시 사이킥 감도가 있어야 함
- 사이킥적으로 귀먹은(psychically deaf) 타겟은 효과 없음
- 메크, 로봇 등은 일반적으로 사이킥 감도 없음

### 6.2 보스 타겟 불가

- `pawn.kindDef.isBoss == true`인 타겟은 사용 불가
- 보스는 사이킥 효과에 면역

### 6.3 재장전 시스템

- `CompApparelReloadable` 컴포넌트 사용
- 최대 2회 사용 가능
- 소진 시 장비 파괴 (`destroyOnEmpty: true`)

### 6.4 관계도 영향

- 사용 시 관계도 -200 (적대 관계)
- 적대 팩션에게 사용하면 전쟁 선포와 유사한 효과

---

## 7. 관련 시스템

### 7.1 CompApparelReloadable
- 장비 재장전 시스템
- 사용 횟수 관리
- 소진 시 파괴 옵션

### 7.2 Hediff 시스템
- 사이킥 쇼크 헤딥 적용
- 뇌 손상 가능성
- 전투 로그 기록

### 7.3 MentalState 시스템
- 광기 상태 적용 (인새니티 랜스)
- 정신 상태 변경

### 7.4 Fleck 시스템
- 시각 효과 표시
- 타겟 이펙트
- 연결선 효과

---

## 8. Verb_CastTargetEffect와의 차이점

| 기능 | Verb_CastTargetEffect | Verb_CastTargetEffectLances |
|------|----------------------|----------------------------|
| 기본 타겟 검증 | ✅ | ✅ (상속) |
| 보스 타겟 검증 | ❌ | ✅ |
| 사이킥 감도 검증 | ❌ | ✅ |
| UI 피드백 | 기본 | 보강 (사이킥 감도 메시지) |
| CompTargetEffect 확인 | ❌ | ✅ (CanApplyOn) |

---

## 9. 사용 시 주의사항

### 9.1 타겟 제한
- 건물 타겟 불가 (`canTargetBuildings: false`)
- 의식 불명 타겟 불가 (`neverTargetIncapacitated: true`)
- 보스 타겟 불가
- 사이킥 감도 없는 타겟 불가

### 9.2 관계도 영향
- 사용 시 관계도 -200
- 적대 팩션에게 사용 시 전쟁 가능성

### 9.3 사용 횟수
- 최대 2회 사용 가능
- 소진 시 장비 파괴
- 재장전 불가 (일회용)

### 9.4 뇌 손상 위험
- 30% 확률로 뇌 손상 발생
- 타겟이 영구적으로 손상될 수 있음

---

## 10. 코드 예시

### 10.1 Verb_CastTargetEffectLances 전체 코드

```csharp
using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
    public class Verb_CastTargetEffectLances : Verb_CastTargetEffect
    {
        public override void OnGUI(LocalTargetInfo target)
        {
            if (this.CanHitTarget(target) && 
                this.verbProps.targetParams.CanTarget(target.ToTargetInfo(this.caster.Map), null))
            {
                Pawn pawn = target.Pawn;
                if (pawn == null)
                {
                    base.OnGUI(target);
                    return;
                }
                
                bool flag = target.Pawn.kindDef.isBoss;
                
                // CompTargetEffect의 CanApplyOn 확인
                foreach (CompTargetEffect compTargetEffect in 
                    base.EquipmentSource.GetComps<CompTargetEffect>())
                {
                    if (!compTargetEffect.CanApplyOn(pawn))
                    {
                        flag = true;
                        break;
                    }
                }
                
                // 보스 또는 적용 불가능한 경우
                if (flag)
                {
                    GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
                    if (!string.IsNullOrEmpty(this.verbProps.invalidTargetPawn))
                    {
                        Widgets.MouseAttachedLabel(
                            this.verbProps.invalidTargetPawn.CapitalizeFirst(), 
                            0f, -20f, null);
                        return;
                    }
                }
                // 사이킥 감도 없는 경우
                else if (pawn.GetStatValue(StatDefOf.PsychicSensitivity, true, -1) <= 0f)
                {
                    GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
                    Widgets.MouseAttachedLabel(
                        "CannotShootPawnIsPsychicallyDeaf".Translate(pawn), 
                        0f, -20f, null);
                    return;
                }
            }
            else
            {
                GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                // 보스 타겟 불가
                if (target.Pawn.kindDef.isBoss)
                {
                    return false;
                }
                
                // 사이킥 감도 확인
                if (pawn.GetStatValue(StatDefOf.PsychicSensitivity, true, -1) <= 0f)
                {
                    return false;
                }
                
                // CompTargetEffect의 CanApplyOn 확인
                foreach (CompTargetEffect compTargetEffect in 
                    base.EquipmentSource.GetComps<CompTargetEffect>())
                {
                    if (!compTargetEffect.CanApplyOn(target.Pawn))
                    {
                        return false;
                    }
                }
            }
            return base.ValidateTarget(target, showMessages);
        }
    }
}
```

---

## 11. 요약

### 11.1 핵심 기능
- 사이킥 랜스 장비 전용 Verb 클래스
- 타겟 검증 로직 강화 (보스, 사이킥 감도, CompTargetEffect 확인)
- UI 피드백 개선 (사이킥 감도 메시지)

### 11.2 사용 위치
- Psychic Shock Lance (사이킥 쇼크 랜스)
- Psychic Insanity Lance (사이킥 인새니티 랜스)
- Shard Shock Lance (샤드 쇼크 랜스) - Anomaly DLC

### 11.3 관련 시스템
- `CompTargetEffect`: 효과 적용 시스템
- `CompApparelReloadable`: 재장전 시스템
- `Hediff`: 헤딥 시스템
- `MentalState`: 정신 상태 시스템
- `Fleck`: 시각 효과 시스템

---

## 업데이트 날짜
2025-01-XX

---

## 관련 문서
- [VerbClass_Complete_Reference.md](29_VerbClass_Complete_Reference.md)
- [Verb_DuplicateLoadID_Analysis_Report.md](19_Verb_DuplicateLoadID_Analysis_Report.md)


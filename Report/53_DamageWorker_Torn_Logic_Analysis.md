# DamageWorker_Torn 데미지 로직 상세 분석 보고서

## 개요
`DamageWorker_Torn`의 데미지 처리 로직을 상세히 분석하여 수정 작업을 위한 기반을 마련합니다.

## 클래스 구조

### 상속 관계
```
DamageWorker (기본 클래스)
  └─ DamageWorker_AddInjury (부상 추가 워커)
      └─ DamageWorker_Torn (커스텀 구현)
```

## 데미지 처리 흐름

### 1. 전체 처리 흐름 (DamageWorker_AddInjury)

```
Apply(DamageInfo, Thing)
  └─ ApplyToPawn(DamageInfo, Pawn)
      └─ ApplyDamageToPart(DamageInfo, Pawn, DamageResult)
          ├─ GetExactPartFromDamageInfo() → ChooseHitPart() [오버라이드됨]
          ├─ ArmorUtility.GetPostArmorDamage() [방어구 계산]
          ├─ ApplySpecialEffectsToPart() [오버라이드됨] ⭐ 핵심 로직
          └─ FinalizeAndAddInjury() [Hediff 생성 및 적용]
```

### 2. ChooseHitPart 메서드 (9-12줄)

**기능**: 타겟 부위 선택

```9:12:Project/1.6/Source/DamageWorker_Torn.cs
protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
{
	return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
}
```

**동작**:
- `BodyPartDepth.Outside`만 선택 (표면 부위만)
- 누락되지 않은 부위 중 랜덤 선택
- `dinfo.Def`를 사용하여 부위 필터링

**차이점**: 기본 `DamageWorker_AddInjury.ChooseHitPart`는 `dinfo.Depth`를 사용하지만, 여기서는 항상 `Outside`로 고정

### 3. ApplySpecialEffectsToPart 메서드 (14-69줄)

이 메서드가 `DamageWorker_Torn`의 핵심 로직입니다.

#### 3-1. Inside 처리 (16-35줄)

**조건**: `dinfo.HitPart.depth == BodyPartDepth.Inside`

**로직**:
1. HitPart부터 부모로 올라가며 `Outside`까지의 모든 부위 수집
2. 데미지를 부위 수에 따라 분산
   - 분산 계수: `(list.Count - 1) + 0.5f`
   - 첫 번째 부위(Inside): `totalDamage / num * 0.5f`
   - 나머지 부위: `totalDamage / num * 1f`
3. 각 부위에 `FinalizeAndAddInjury` 호출

**예시**:
- Inside → Middle → Outside 구조인 경우
- 분산 계수: `(3 - 1) + 0.5 = 2.5`
- Inside: `totalDamage / 2.5 * 0.5 = totalDamage * 0.2`
- Middle: `totalDamage / 2.5 * 1 = totalDamage * 0.4`
- Outside: `totalDamage / 2.5 * 1 = totalDamage * 0.4`

#### 3-2. Outside 처리 (36-68줄)

**조건**: `dinfo.HitPart.depth != BodyPartDepth.Inside` (즉, Outside)

**단계별 처리**:

##### 단계 1: 추가 타겟 부위 수 결정 (36줄)

```36:36:Project/1.6/Source/DamageWorker_Torn.cs
int num2 = (this.def.cutExtraTargetsCurve != null) ? GenMath.RoundRandom(this.def.cutExtraTargetsCurve.Evaluate(Rand.Value)) : 0;
```

- `cutExtraTargetsCurve`: 랜덤 값(0~1)을 입력받아 추가 타겟 수 반환
- 현재 XML 설정: `0 → 0`, `1 → 4`
- `Rand.Value`가 0.5면 약 2개 추가 타겟

##### 단계 2: 추가 타겟 부위 수집 (38-51줄)

**수집 범위**:
1. HitPart의 직접 자식 부위들 (`GetDirectChildParts()`)
2. HitPart의 부모 부위 (`parent`)
3. 부모의 직접 자식 부위들 (`parent.GetDirectChildParts()`)

**필터링 조건**:
- `conceptual`이 아님
- `coverageAbs > 0f` (실제 커버리지가 있음)
- HitPart 제외
- 랜덤 순서로 정렬 후 `num2`개만 선택

**예시**:
- HitPart: 왼팔
- 수집 대상: 왼손, 왼손가락들, 어깨, 어깨의 다른 자식들
- `num2 = 2`면 랜덤으로 2개 선택

##### 단계 3: 타겟 리스트 구성 (57줄)

```57:57:Project/1.6/Source/DamageWorker_Torn.cs
list2.Add(dinfo.HitPart);
```

- 추가 타겟들 + 원래 HitPart

##### 단계 4: 데미지 분산 계산 (58줄)

```58:58:Project/1.6/Source/DamageWorker_Torn.cs
float num3 = totalDamage * (1f + this.def.cutCleaveBonus) / ((float)list2.Count + this.def.cutCleaveBonus);
```

**공식 분석**:
- 분자: `totalDamage * (1 + cutCleaveBonus)`
- 분모: `부위수 + cutCleaveBonus`
- 현재 XML: `cutCleaveBonus = 0.3`

**예시 계산**:
- `totalDamage = 100`, `cutCleaveBonus = 0.3`, `부위수 = 3`
- `num3 = 100 * 1.3 / (3 + 0.3) = 130 / 3.3 ≈ 39.39`

**의미**:
- `cutCleaveBonus`가 클수록 데미지가 더 많이 분산됨
- 부위가 많을수록 각 부위의 데미지는 감소하지만, 총 데미지는 증가

##### 단계 5: 데미지 보존 처리 (59-62줄)

```59:62:Project/1.6/Source/DamageWorker_Torn.cs
if (num2 == 0)
{
	num3 = base.ReduceDamageToPreserveOutsideParts(num3, dinfo, pawn);
}
```

**조건**: 추가 타겟이 없을 때만 (`num2 == 0`)

**목적**: 부위 파괴를 방지하기 위해 데미지 감소

**ReduceDamageToPreserveOutsideParts 로직** (429-447줄):
1. `ShouldReduceDamageToPreservePart` 체크
   - `depth == Outside`이고
   - CorePart가 아니어야 함
2. 부위 체력보다 데미지가 크면
3. `overkillPctToDestroyPart`에 따라 확률적으로 데미지 감소
   - 현재 XML: `0~0.1` (10% 확률로 파괴)
4. 감소 시: `partHealth - 1f`로 제한

**중요**: 추가 타겟이 있으면 이 보존 로직이 적용되지 않음

##### 단계 6: 각 부위에 데미지 적용 (63-68줄)

```63:68:Project/1.6/Source/DamageWorker_Torn.cs
for (int j = 0; j < list2.Count; j++)
{
	DamageInfo dinfo3 = dinfo;
	dinfo3.SetHitPart(list2[j]);
	base.FinalizeAndAddInjury(pawn, num3, dinfo3, result);
}
```

- 모든 타겟 부위에 동일한 데미지(`num3`) 적용
- 각 부위마다 새로운 `DamageInfo` 생성하여 부위 설정

## 주요 파라미터 활용

### XML에서 정의된 파라미터

1. **cutExtraTargetsCurve** (570-579줄)
   - 타입: `SimpleCurve`
   - 용도: 추가 타겟 부위 수 결정
   - 현재 값: `0 → 0`, `1 → 4`

2. **cutCleaveBonus** (580줄)
   - 타입: `float`
   - 용도: 데미지 분산 보너스
   - 현재 값: `0.3`

3. **overkillPctToDestroyPart** (569줄)
   - 타입: `FloatRange`
   - 용도: 부위 파괴 확률
   - 현재 값: `0~0.1`
   - **주의**: `num2 == 0`일 때만 적용됨

## 부모 클래스 메서드

### FinalizeAndAddInjury (229-281줄)

**기능**: Hediff 생성 및 적용

**처리 과정**:
1. HediffDef 결정 (`HealthUtility.GetHediffDefFromDamage`)
2. Hediff_Injury 생성
3. 소스 정보 설정 (무기, 부위 그룹 등)
4. Severity 설정 (`totalDamage`)
5. 영구 부상 처리 (`InstantPermanentInjury`)
6. 실제 Hediff 추가 및 결과 반영

### ReduceDamageToPreserveOutsideParts (429-447줄)

**기능**: 부위 파괴 방지를 위한 데미지 감소

**조건**:
- `depth == Outside`
- CorePart가 아님
- 데미지가 부위 체력보다 큼

**로직**:
- `overkillPctToDestroyPart`에 따라 확률적으로 파괴 허용
- 그렇지 않으면 `partHealth - 1f`로 제한

## 로직의 특징 및 주의사항

### 1. Inside vs Outside 처리 차이
- **Inside**: 부모 체인을 따라 데미지 분산 (깊이 우선)
- **Outside**: 형제/부모 부위에 데미지 분산 (폭 우선)

### 2. 데미지 보존 로직의 조건부 적용
- `num2 == 0`일 때만 `ReduceDamageToPreserveOutsideParts` 호출
- 추가 타겟이 있으면 보존 로직 미적용 → 더 많은 부위에 데미지 분산 가능

### 3. cutCleaveBonus의 영향
- 분자와 분모 모두에 영향을 줌
- 부위 수가 많을수록 총 데미지 증가 효과
- 각 부위의 데미지는 감소하지만, 전체적으로는 더 많은 데미지

### 4. 데미지 분산 방식
- **Inside**: 첫 부위에 0.5배, 나머지에 1배 (비대칭)
- **Outside**: 모든 부위에 동일한 데미지 (대칭)

## 수정 가능한 지점

1. **ChooseHitPart** (9-12줄)
   - 부위 선택 로직 변경 가능

2. **Inside 처리** (16-35줄)
   - 데미지 분산 비율 변경
   - 부위 수집 방식 변경

3. **추가 타겟 수집** (38-51줄)
   - 수집 범위 확장/축소
   - 필터링 조건 변경

4. **데미지 분산 계산** (58줄)
   - 공식 변경
   - cutCleaveBonus 활용 방식 변경

5. **데미지 보존 로직** (59-62줄)
   - 조건 변경 (`num2 == 0` → 항상 적용 등)
   - 보존 로직 커스터마이징

6. **데미지 적용** (63-68줄)
   - 부위별 데미지 차등 적용
   - 추가 효과 적용

## 관련 파일

- [DamageWorker_Torn.cs](Project/1.6/Source/DamageWorker_Torn.cs)
- [DamageWorker_AddInjury.cs](RimworldSource/Verse/DamageWorker_AddInjury.cs)
- [Weapon_HighTech.xml](Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml) (557-581줄)


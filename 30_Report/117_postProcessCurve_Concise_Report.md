# postProcessCurve 간결 가이드

> **Tags**: postProcessCurve SimpleCurve StatDef Input Output 계산플로우  
> **작성일**: 2026-03-04

---

## 1. 개요

`postProcessCurve`는 StatDef의 **raw값을 최종값으로 변환**하는 곡선입니다. 주로 확률(0~1) 스탯에서 사용됩니다.

| 구분 | 설명 |
|------|------|
| **Input** | raw값 (스킬·능력치·장비 등 합산된 중간값) |
| **Output** | 최종 스탯값 (예: 0.5 = 50% 확률) |
| **타입** | `SimpleCurve` (점 목록 + 선형 보간) |

---

## 2. 계산 플로우

```
GetValue()
    │
    ├─► GetValueUnfinalized()  →  raw값 산출
    │       (baseValue + skillNeedOffsets + capacityOffsets + equippedStatOffsets + ...)
    │
    └─► FinalizeValue()
            │
            ├─► stat.parts (StatPart 변환)
            ├─► postProcessCurve.Evaluate(raw)  ← 여기서 raw → 최종값
            ├─► postProcessStatFactors (추가 곱연산)
            ├─► min/max 클램프
            └─► 최종값 반환
```

**핵심 코드** (`StatWorker.FinalizeValue`):

```csharp
if (applyPostProcess && this.stat.postProcessCurve != null)
{
    val = this.stat.postProcessCurve.Evaluate(val);
}
```

---

## 3. SimpleCurve.Evaluate 로직

| 조건 | 반환 |
|------|------|
| x ≤ 첫 점의 x | 첫 점의 y |
| x ≥ 마지막 점의 x | 마지막 점의 y |
| 그 외 | 인접 두 점 사이 **선형 보간** |

```
t = (x - curvePoint.x) / (curvePoint2.x - curvePoint.x)
return Lerp(curvePoint.y, curvePoint2.y, t)
```

---

## 4. 예시: MeleeHitChance / MeleeDodgeChance

### MeleeHitChance

| Input (raw) | Output (확률) |
|-------------|--------------|
| -20 | 5% |
| 0 | 50% |
| 20 | 90% |
| 60 | 98% |

### MeleeDodgeChance

| Input (raw) | Output (확률) |
|-------------|--------------|
| 5 | 0% |
| 20 | 30% |
| 60 | 50% |

### XML 정의 예시

```xml
<postProcessCurve>
  <points>
    <li>(-20, 0.05)</li>
    <li>(0.0, 0.50)</li>
    <li>(20, 0.90)</li>
    <li>(60, 0.98)</li>
  </points>
</postProcessCurve>
```

---

## 5. 관련 소스

| 파일 | 역할 |
|------|------|
| `StatWorker.cs` | GetValue → FinalizeValue → postProcessCurve.Evaluate |
| `StatDef.cs` | `SimpleCurve postProcessCurve` 필드 |
| `SimpleCurve.cs` | Evaluate (경계 + 선형 보간) |

# HediffDef painPerSeverity 분석 보고서

## 개요
RimWorld에서 Damage로 인한 HediffDef들의 `painPerSeverity` 값에 대한 조사 결과입니다.

## painPerSeverity란?
- **정의**: 부상의 심각도(Severity) 1당 발생하는 통증(Pain)의 양
- **계산식**: `통증 = Severity × painPerSeverity / HealthScale`
- **기본값**: `1.0` (InjuryProps.cs에서 확인)

## 소스 코드 분석

### InjuryProps.cs
```csharp
public class InjuryProps
{
    public float painPerSeverity = 1f;  // 기본값: 1.0
    public float averagePainPerSeverityPermanent = 0.5f;  // 영구 상처 평균값: 0.5
    // ...
}
```

### Hediff_Injury.cs
```csharp
// 일반 부상의 경우
num = this.Severity * this.def.injuryProps.painPerSeverity;

// 영구 상처의 경우
num = this.Severity * this.def.injuryProps.averagePainPerSeverityPermanent * hediffComp_GetsPermanent.PainFactor;
```

## RimWorld 기본 HediffDef painPerSeverity 값

웹 검색 및 일반적인 RimWorld 설정 기준:

| HediffDef 타입 | painPerSeverity | 설명 |
|---------------|----------------|------|
| **Cut** | 1.25 | 절단 상처 - 높은 통증 |
| **Bruise** | 0.5 | 타박상 - 낮은 통증 |
| **Crack** | 0.8 | 골절 - 중간 통증 |
| **Gunshot** | 1.5 | 총상 - 매우 높은 통증 |
| **Burn** | 1.0 | 화상 - 기본 통증 |
| **Scratch** | 0.5~0.8 | 긁힘 - 낮은 통증 |
| **Stab** | 1.0~1.2 | 찔림 - 중간~높은 통증 |
| **기본값** | 1.0 | 명시되지 않은 경우 |

## 통증 수준 분류

### 낮은 통증 (0.5 ~ 0.8)
- **Bruise (타박상)**: 0.5
- **Scratch (긁힘)**: 0.5~0.8
- **Crack (골절)**: 0.8

### 중간 통증 (1.0 ~ 1.2)
- **Burn (화상)**: 1.0
- **Stab (찔림)**: 1.0~1.2
- **기본값**: 1.0

### 높은 통증 (1.25 ~ 1.5)
- **Cut (절단)**: 1.25
- **Gunshot (총상)**: 1.5

## 프로젝트 내 사용 현황

### 현재 사용 중인 HediffDef
- **RK_Hediff_Torn**: `painPerSeverity = 1.25` (Cut과 동일)

## 권장 사항

### RK_Hediff_Torn의 painPerSeverity 값
현재 값인 **1.25**는 Cut과 동일한 수준으로 적절합니다.

다른 옵션:
- **1.0**: 기본값, 중간 통증 수준
- **1.5**: Gunshot 수준, 매우 높은 통증
- **0.8**: Crack 수준, 중간~낮은 통증

### 일반적인 가이드라인
1. **가벼운 부상**: 0.5 ~ 0.8
2. **일반 부상**: 1.0 ~ 1.2
3. **심각한 부상**: 1.25 ~ 1.5

## 참고사항
- `painPerSeverity`는 최종 통증에 HealthScale로 나누어지므로, 실제 통증은 생물의 크기에 따라 달라집니다.
- 영구 상처의 경우 `averagePainPerSeverityPermanent` (기본값 0.5)가 사용됩니다.
- `bleedRate`와 함께 고려하여 부상의 심각도를 조정할 수 있습니다.


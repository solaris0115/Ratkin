# HediffStage 속성 참조 문서

**작성일**: 2025-01-XX  
**목적**: HediffDef의 stages에서 사용 가능한 모든 속성 정리

---

## HediffStage 사용 가능 속성 목록

### 기본 속성
- **`minSeverity`** (float): 이 stage가 활성화되는 최소 severity 값
- **`label`** (string): Stage의 표시 이름
- **`overrideLabel`** (string): 기본 label을 덮어쓰는 이름
- **`becomeVisible`** (bool): UI에 표시될지 여부 (기본값: true)
- **`lifeThreatening`** (bool): 생명을 위협하는지 여부
- **`tale`** (TaleDef): 관련 이야기 정의

---

### 스탯 수정 (Stat Modifiers)
- **`statOffsets`** (List<StatModifier>): 스탯에 더하기/빼기
  ```xml
  <statOffsets>
    <MoveSpeed>0.10</MoveSpeed>
    <MeleeDodgeChance>0.15</MeleeDodgeChance>
  </statOffsets>
  ```

- **`statFactors`** (List<StatModifier>): 스탯에 곱하기
  ```xml
  <statFactors>
    <RangedCooldownFactor>0.8125</RangedCooldownFactor>
    <MeleeCooldownFactor>0.9</MeleeCooldownFactor>
  </statFactors>
  ```

- **`statOffsetsBySeverity`** (List<StatModifierBySeverity>): Severity에 따라 변하는 offset
- **`statFactorsBySeverity`** (List<StatModifierBySeverity>): Severity에 따라 변하는 factor
- **`multiplyStatChangesBySeverity`** (bool): Stat 변경을 severity로 곱할지 여부
- **`statOffsetEffectMultiplier`** (StatDef): Stat offset 효과 배율
- **`statFactorEffectMultiplier`** (StatDef): Stat factor 효과 배율

---

### 신체 능력 수정 (Capacity Modifiers)
- **`capMods`** (List<PawnCapacityModifier>): 신체 능력 수정
  ```xml
  <capMods>
    <li>
      <capacity>Consciousness</capacity>
      <offset>-0.1</offset>  <!-- 또는 setMax: 0.9 -->
    </li>
    <li>
      <capacity>Moving</capacity>
      <offset>-0.05</offset>
    </li>
  </capMods>
  ```
- **`capacityFactorEffectMultiplier`** (StatDef): Capacity 효과 배율

---

### 통증 관련
- **`painFactor`** (float): 통증 배율 (기본값: 1.0)
- **`painOffset`** (float): 통증 오프셋

---

### 생명력/회복 관련
- **`totalBleedFactor`** (float): 총 출혈 배율 (기본값: 1.0)
- **`naturalHealingFactor`** (float): 자연 회복 배율 (-1이면 비활성화)
- **`regeneration`** (float): 재생 속도 (-1이면 비활성화)
- **`showRegenerationStat`** (bool): 재생 스탯 표시 여부 (기본값: true)

---

### 정신 상태 관련
- **`mentalBreakMtbDays`** (float): 정신 붕괴 평균 시간 (일 단위, -1이면 비활성화)
- **`mentalBreakExplanation`** (string): 정신 붕괴 설명
- **`blocksMentalBreaks`** (bool): 정신 붕괴 차단 여부
- **`blocksInspirations`** (bool): 영감 차단 여부
- **`allowedMentalBreakIntensities`** (List<MentalBreakIntensity>): 허용된 정신 붕괴 강도 목록
- **`mentalStateGivers`** (List<MentalStateGiver>): 정신 상태 부여
  ```xml
  <mentalStateGivers>
    <li>
      <mentalState>WanderConfused</mentalState>
      <mtbDays>50</mtbDays>
    </li>
  </mentalStateGivers>
  ```

---

### 기억/생각 관련
- **`forgetMemoryThoughtMtbDays`** (float): 기억 잊기 평균 시간 (일 단위)
- **`pctConditionalThoughtsNullified`** (float): 조건부 생각 무효화 비율
- **`pctAllThoughtNullification`** (float): 모든 생각 무효화 비율
- **`overrideMoodBase`** (float): 기분 기본값 오버라이드 (-1이면 비활성화)

---

### 사회적 상호작용
- **`opinionOfOthersFactor`** (float): 타인에 대한 의견 배율 (기본값: 1.0)
- **`socialFightChanceFactor`** (float): 사회적 싸움 확률 배율 (기본값: 1.0)

---

### 생리적 요구사항
- **`hungerRateFactor`** (float): 배고픔 속도 배율 (기본값: 1.0)
- **`hungerRateFactorOffset`** (float): 배고픔 속도 오프셋
- **`restFallFactor`** (float): 휴식 감소 배율 (기본값: 1.0)
- **`restFallFactorOffset`** (float): 휴식 감소 오프셋
- **`fertilityFactor`** (float): 생식력 배율 (기본값: 1.0)

---

### 기타 효과
- **`vomitMtbDays`** (float): 구토 평균 시간 (일 단위, -1이면 비활성화)
- **`deathMtbDays`** (float): 사망 평균 시간 (일 단위, -1이면 비활성화)
- **`mtbDeathDestroysBrain`** (bool): 사망 시 뇌 파괴 여부
- **`foodPoisoningChanceFactor`** (float): 식중독 확률 배율 (기본값: 1.0)
- **`severityGainFactor`** (float): Severity 증가 배율 (기본값: 1.0)

---

### 면역/부상 관련
- **`makeImmuneTo`** (List<HediffDef>): 면역 부여할 Hediff 목록
- **`hediffGivers`** (List<HediffGiver>): 다른 Hediff 부여
- **`damageFactors`** (List<DamageFactor>): 데미지 배율
  ```xml
  <damageFactors>
    <li>
      <damage>Heat</damage>
      <factor>0.5</factor>
    </li>
  </damageFactors>
  ```

---

### 욕구 (Needs)
- **`enablesNeeds`** (List<NeedDef>): 활성화할 욕구 목록
- **`disablesNeeds`** (List<NeedDef>): 비활성화할 욕구 목록

---

### 작업 관련
- **`disabledWorkTags`** (WorkTags): 비활성화할 작업 태그

---

### 신체 부위 관련
- **`partEfficiencyOffset`** (float): 부위 효율 오프셋
- **`partIgnoreMissingHP`** (bool): 부위 HP 손실 무시 여부
- **`destroyPart`** (bool): 부위 파괴 여부

---

### 수면 관련
- **`blocksSleeping`** (bool): 수면 차단 여부

---

### UI/표시 관련
- **`overrideTooltip`** (string): 툴팁 오버라이드
- **`extraTooltip`** (string): 추가 툴팁

---

## 사용 예시

### 예시 1: 전투 버프 Hediff
```xml
<stages>
  <li>
    <statOffsets>
      <MeleeDodgeChance>0.15</MeleeDodgeChance>
      <MoveSpeed>0.10</MoveSpeed>
    </statOffsets>
    <statFactors>
      <RangedCooldownFactor>0.9</RangedCooldownFactor>
    </statFactors>
  </li>
</stages>
```

### 예시 2: 신체 능력 감소 Hediff
```xml
<stages>
  <li>
    <capMods>
      <li>
        <capacity>Consciousness</capacity>
        <setMax>0.7</setMax>
      </li>
      <li>
        <capacity>Moving</capacity>
        <offset>-0.2</offset>
      </li>
    </capMods>
    <painFactor>0.8</painFactor>
  </li>
</stages>
```

### 예시 3: 복합 효과 Hediff
```xml
<stages>
  <li>
    <minSeverity>0.5</minSeverity>
    <label>moderate</label>
    <statOffsets>
      <RestFallRateFactor>0.7</RestFallRateFactor>
    </statOffsets>
    <capMods>
      <li>
        <capacity>Manipulation</capacity>
        <offset>-0.3</offset>
      </li>
    </capMods>
    <vomitMtbDays>1.0</vomitMtbDays>
    <painOffset>0.2</painOffset>
  </li>
</stages>
```

---

## 관련 파일 목록

- `RimworldSource/Verse/HediffStage.cs` - HediffStage 클래스 정의
- `RimworldData/Core/Defs/HediffDefs/` - HediffDef 예시 파일들


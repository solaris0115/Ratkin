# WeaponTraitDef 추가 속성 정리

## 개요
WeaponTraitDef에서 statOffset/statFactor 외에 사용 가능한 성능 관련 속성들 정리

---

## 1. 장착 시 스탯 변경 (equippedStatOffsets)

**설명**: 무기를 장착했을 때만 적용되는 스탯 오프셋 (무기 자체 스탯이 아닌, 착용자에게 적용)

**사용 예시** (Royalty DLC):
```xml
<equippedStatOffsets>
  <PsychicSensitivity>0.4</PsychicSensitivity>  <!-- 사이킥 감도 +40% -->
  <MeditationFocusGain>0.1</MeditationFocusGain>  <!-- 명상 포커스 획득 +10% -->
</equippedStatOffsets>
```

**차이점**:
- `statOffsets`: 무기 자체의 스탯 변경 (예: 무기 데미지, 명중률 등)
- `equippedStatOffsets`: 무기를 장착한 캐릭터의 스탯 변경 (예: 캐릭터의 사이킥 감도, 이동 속도 등)

---

## 2. 장착 시 헤딥 부여 (equippedHediffs)

**설명**: 무기를 장착했을 때 착용자에게 부여되는 헤딥 (상태 효과)

**사용 예시** (Royalty DLC):
```xml
<equippedHediffs>
  <li>NoPain</li>  <!-- 통증 무시 -->
  <li>SpeedBoost</li>  <!-- 이동 속도 15% 증가 -->
  <li>NeuralHeatRecoveryGain</li>  <!-- 신경열 회복 속도 증가 -->
</equippedHediffs>
```

**특징**: 
- 무기를 벗으면 헤딥이 제거됨
- 헤딥을 통해 복잡한 상태 효과 구현 가능 (통증 무시, 능력치 변경 등)

---

## 3. 결속 시 헤딥 부여 (bondedHediffs)

**설명**: 무기와 결속(bond)되었을 때 부여되는 헤딥 (무기를 장착하지 않아도 유지)

**사용 예시** (Royalty DLC):
```xml
<bondedHediffs>
  <li>HungerMaker</li>  <!-- 배고픔 속도 50% 증가 -->
</bondedHediffs>
```

**차이점**:
- `equippedHediffs`: 무기를 장착해야만 효과 발동
- `bondedHediffs`: 결속만 되면 무기를 벗어도 효과 유지

---

## 4. 결속 시 생각 부여 (bondedThought)

**설명**: 무기와 결속되었을 때 생기는 생각(Thought) - 기분에 영향

**사용 예시** (Royalty DLC):
```xml
<bondedThought>BondedThoughtKind</bondedThought>  <!-- 친절한 생각 (+6 기분) -->
<bondedThought>BondedThoughtCalm</bondedThought>  <!-- 차분한 생각 (+3 기분) -->
<bondedThought>BondedThoughtMuttering</bondedThought>  <!-- 미친 중얼거림 (-3 기분) -->
<bondedThought>BondedThoughtWailing</bondedThought>  <!-- 미친 울부짖음 (-6 기분) -->
```

---

## 5. 킬 시 생각 부여 (killThought)

**설명**: 이 무기로 적을 죽였을 때 생기는 생각(Thought)

**사용 예시** (Royalty DLC):
```xml
<killThought>OnKill_GoodThought</killThought>  <!-- 킬 시 기쁨 (+6 기분, 3일 지속) -->
<killThought>OnKill_BadThought</killThought>  <!-- 킬 시 슬픔 (-3 기분, 3일 지속) -->
```

**특징**: 
- 스택 가능 (최대 5회까지)
- 일정 기간 지속 (예: 3일)

---

## 6. 커스텀 워커 클래스 (workerClass)

**설명**: 커스텀 로직을 구현한 Worker 클래스 지정

**사용 예시** (Royalty DLC):
```xml
<workerClass>WeaponTraitWorker_PsyfocusOnKill</workerClass>  <!-- 킬 시 사이포커스 획득 -->
<workerClass>WeaponTraitWorker_Jealous</workerClass>  <!-- 다른 무기 사용 시 질투 -->
```

**특징**:
- 기본 `WeaponTraitWorker`를 상속받아 커스텀 로직 구현 가능
- `Notify_KilledPawn`, `Notify_OtherWeaponWielded` 등의 이벤트 처리 가능

---

## 7. 결속 불가 (neverBond)

**설명**: 무기가 결속되지 않도록 설정

**사용 예시** (Royalty DLC):
```xml
<neverBond>true</neverBond>
```

---

## 8. 발사 관련 속성

### burstShotSpeedMultiplier
**설명**: 버스트 발사 시 각 발사 사이의 속도 배율
```xml
<burstShotSpeedMultiplier>2.0</burstShotSpeedMultiplier>  <!-- 발사 속도 2배 -->
```

### burstShotCountMultiplier
**설명**: 버스트 발사 시 발사 수량 배율
```xml
<burstShotCountMultiplier>1.5</burstShotCountMultiplier>  <!-- 발사 수 50% 증가 -->
```

---

## 9. 데미지 관련 속성

### damageDefOverride
**설명**: 데미지 타입 변경
```xml
<damageDefOverride>Bullet_TraitTox</damageDefOverride>  <!-- 독 데미지로 변경 -->
<damageDefOverride>Bullet_TraitIncendiary</damageDefOverride>  <!-- 화염 데미지로 변경 -->
<damageDefOverride>Nerve</damageDefOverride>  <!-- 신경 데미지로 변경 -->
```

### extraDamages
**설명**: 추가 데미지 타입 및 양
```xml
<extraDamages>
  <li>
    <def>EMP</def>
    <amount>4</amount>
  </li>
</extraDamages>
```

### additionalStoppingPower
**설명**: 추가 스톱핑 파워
```xml
<additionalStoppingPower>2</additionalStoppingPower>  <!-- 스톱핑 파워 +2 -->
```

---

## 10. 명중률 관련 속성

### ignoresAccuracyMaluses
**설명**: 명중률 페널티 무시 (날씨, 연기 등)
```xml
<ignoresAccuracyMaluses>true</ignoresAccuracyMaluses>
```

---

## 11. 능력 관련 속성

### abilityProps
**설명**: 무기에 특수 능력 추가
```xml
<abilityProps>
  <abilityDef>EMPPulse</abilityDef>  <!-- EMP 펄스 능력 -->
  <maxCharges>1</maxCharges>
  <ammoDef>Steel</ammoDef>
  <ammoCountPerCharge>25</ammoCountPerCharge>
</abilityProps>
```

---

## 속성 비교표

| 속성 | 적용 시점 | 적용 대상 | 지속성 |
|------|----------|----------|--------|
| `statOffsets` | 항상 | 무기 자체 | 무기 소유 중 |
| `statFactors` | 항상 | 무기 자체 | 무기 소유 중 |
| `equippedStatOffsets` | 장착 시 | 착용자 | 장착 중 |
| `equippedHediffs` | 장착 시 | 착용자 | 장착 중 |
| `bondedHediffs` | 결속 시 | 결속자 | 결속 중 (장착 안 해도) |
| `bondedThought` | 결속 시 | 결속자 | 결속 중 |
| `killThought` | 킬 시 | 사용자 | 일정 기간 |

---

## 요약

**statOffset/statFactor 외에 사용 가능한 성능 관련 속성들**:

1. ✅ **equippedStatOffsets** - 장착 시 착용자 스탯 변경
2. ✅ **equippedHediffs** - 장착 시 상태 효과 부여
3. ✅ **bondedHediffs** - 결속 시 상태 효과 부여
4. ✅ **bondedThought** - 결속 시 기분 효과
5. ✅ **killThought** - 킬 시 기분 효과
6. ✅ **workerClass** - 커스텀 로직 구현
7. ✅ **burstShotSpeedMultiplier** - 버스트 발사 속도
8. ✅ **burstShotCountMultiplier** - 버스트 발사 수량
9. ✅ **damageDefOverride** - 데미지 타입 변경
10. ✅ **extraDamages** - 추가 데미지
11. ✅ **additionalStoppingPower** - 스톱핑 파워 증가
12. ✅ **ignoresAccuracyMaluses** - 명중률 페널티 무시
13. ✅ **abilityProps** - 특수 능력 추가

**참고**: Royalty DLC의 BladeLink 무기 특성들에서 많은 예시를 확인할 수 있습니다.

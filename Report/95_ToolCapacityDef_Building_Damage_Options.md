# ToolCapacityDef 및 Tool 속성 분석 (코드 기반)

## 개요
RimWorld 소스코드(`Verse/ToolCapacityDef.cs`, `Verse/Tool.cs`)와 실제 사용 사례를 기반으로 ToolCapacityDef와 Tool의 속성을 분석합니다.

---

## 1. ToolCapacityDef 코드 분석

### 1.1 소스코드 구조 (`Verse/ToolCapacityDef.cs`)

```csharp
public class ToolCapacityDef : Def
{
    // 추가 속성 없음 - Def의 기본 속성만 상속
    public IEnumerable<ManeuverDef> Maneuvers { get; }
    public IEnumerable<VerbProperties> VerbsProperties { get; }
}
```

**결론**: ToolCapacityDef는 **Def 클래스를 상속**하며, **자체 속성은 없습니다**. Def의 기본 속성만 사용 가능합니다.

### 1.2 ToolCapacityDef 사용 가능 속성 (Def 기본)

| 속성 | 타입 | 필수 | 설명 | 소스 위치 |
|------|------|------|------|----------|
| **defName** | string | ✅ | 고유 식별자 | `Def.cs:12` |
| **label** | string | ✅ | 게임 내 표시 이름 | `Def.cs:17` |
| **description** | string | ❌ | 설명 텍스트 | `Def.cs:22` |
| **descriptionHyperlinks** | List\<DefHyperlink\> | ❌ | 설명 내 하이퍼링크 | `Def.cs:25` |
| **modExtensions** | List\<DefModExtension\> | ❌ | 모드 확장 데이터 | `Def.cs:36` |
| **ignoreConfigErrors** | bool | ❌ | 설정 오류 검사 무시 | `Def.cs:30` |

**ToolCapacityDef 자체에는 건축물 피해 관련 속성이 없습니다.**

---

## 2. Tool 코드 분석 (`Verse/Tool.cs`)

### 2.1 Tool 클래스 속성 (전체)

```csharp
public class Tool
{
    public string id;                                    // 내부 ID
    public string label;                                 // 표시 이름
    public string labelNoLocation;                       // 위치 없는 표시 이름
    public bool labelUsedInLogging = true;               // 로그 사용 여부
    
    public List<ToolCapacityDef> capacities;             // ToolCapacity 목록
    public float power;                                   // 기본 공격력
    public float armorPenetration = -1f;                 // 방어관통 (-1이면 자동 계산)
    public float cooldownTime;                           // 쿨다운 시간
    
    public SurpriseAttackProps surpriseAttack;           // 기습 공격 속성
    public HediffDef hediff;                             // 부여할 헤딥
    public float chanceFactor = 1f;                      // 사용 확률 배율
    
    public bool alwaysTreatAsWeapon;                     // 항상 무기로 취급
    public List<ExtraDamage> extraMeleeDamages;          // 추가 근접 데미지
    
    public SoundDef soundMeleeHit;                       // 근접 명중 사운드
    public SoundDef soundMeleeMiss;                      // 근접 빗나감 사운드
    
    public BodyPartGroupDef linkedBodyPartsGroup;        // 연결된 신체 부위 그룹
    public bool ensureLinkedBodyPartsGroupAlwaysUsable;  // 부위 항상 사용 가능 보장
}
```

### 2.2 Tool 속성 상세 설명

#### capacities (List\<ToolCapacityDef\>)
- **설명**: 이 Tool이 사용할 수 있는 ToolCapacity 목록
- **예시**:
  ```xml
  <capacities>
    <li>Demolish</li>
    <li>Blunt</li>
  </capacities>
  ```

#### power (float)
- **설명**: 기본 공격력 (데미지 계산의 기본값)
- **계산식**: `power × MeleeWeapon_DamageMultiplier × StuffDamageMultiplier`
- **예시**: `<power>12</power>`

#### armorPenetration (float, 기본값: -1)
- **설명**: 방어관통 값 (-1이면 `power × 0.015`로 자동 계산)
- **예시**: `<armorPenetration>0.5</armorPenetration>` (50% 관통)

#### cooldownTime (float)
- **설명**: 공격 쿨다운 시간 (초 단위)
- **계산식**: `cooldownTime × MeleeWeapon_CooldownMultiplier`
- **예시**: `<cooldownTime>2</cooldownTime>`

#### chanceFactor (float, 기본값: 1.0)
- **설명**: 이 Tool이 선택될 확률 배율
- **예시**: `<chanceFactor>0.2</chanceFactor>` (20% 확률)

#### alwaysTreatAsWeapon (bool)
- **설명**: 항상 무기로 취급 (무기 슬롯에 표시)
- **예시**: `<alwaysTreatAsWeapon>true</alwaysTreatAsWeapon>`

#### linkedBodyPartsGroup (BodyPartGroupDef)
- **설명**: 이 Tool이 연결된 신체 부위 그룹
- **예시**: `<linkedBodyPartsGroup>LeftHand</linkedBodyPartsGroup>`

#### ensureLinkedBodyPartsGroupAlwaysUsable (bool)
- **설명**: 연결된 부위가 부상당해도 항상 사용 가능하도록 보장
- **예시**: `<ensureLinkedBodyPartsGroupAlwaysUsable>true</ensureLinkedBodyPartsGroupAlwaysUsable>`

#### extraMeleeDamages (List\<ExtraDamage\>)
- **설명**: 추가 근접 데미지 타입 및 양
- **ExtraDamage 구조**:
  ```csharp
  public class ExtraDamage
  {
      public DamageDef def;              // 데미지 타입
      public float amount;                // 데미지 양
      public float armorPenetration = -1f; // 방어관통
      public float chance = 1f;           // 발동 확률
  }
  ```
- **예시**:
  ```xml
  <extraMeleeDamages>
    <li>
      <def>NerveStun</def>
      <amount>3</amount>
      <armorPenetration>0.5</armorPenetration>
      <chance>0.5</chance>
    </li>
  </extraMeleeDamages>
  ```

#### surpriseAttack (SurpriseAttackProps)
- **설명**: 기습 공격 시 추가 데미지
- **구조**:
  ```csharp
  public class SurpriseAttackProps
  {
      public List<ExtraDamage> extraMeleeDamages;
  }
  ```

#### hediff (HediffDef)
- **설명**: 공격 시 부여할 헤딥
- **예시**: `<hediff>Hediff_Stun</hediff>`

#### soundMeleeHit (SoundDef)
- **설명**: 근접 공격 명중 시 사운드
- **예시**: `<soundMeleeHit>Pawn_Melee_MechanoidBash_HitPawn</soundMeleeHit>`

#### soundMeleeMiss (SoundDef)
- **설명**: 근접 공격 빗나감 시 사운드
- **예시**: `<soundMeleeMiss>Pawn_Melee_MechanoidBash_Miss</soundMeleeMiss>`

---

## 3. 실제 사용 사례

### 3.1 Mech_Centurion (바이오텍) - Demolish Tool

```xml
<tools>
  <li>
    <label>head</label>
    <capacities>
      <li>Demolish</li>
    </capacities>
    <power>12</power>
    <cooldownTime>2</cooldownTime>
    <linkedBodyPartsGroup>HeadAttackTool</linkedBodyPartsGroup>
    <ensureLinkedBodyPartsGroupAlwaysUsable>true</ensureLinkedBodyPartsGroupAlwaysUsable>
    <chanceFactor>0.2</chanceFactor>
  </li>
</tools>
```

**분석**:
- `Demolish` ToolCapacity 사용
- `power: 12` - 기본 공격력
- `chanceFactor: 0.2` - 20% 확률로 선택
- `ensureLinkedBodyPartsGroupAlwaysUsable: true` - 부위 부상 시에도 사용 가능

### 3.2 Mech_Lancer (Core) - 다중 Tool

```xml
<tools>
  <li>
    <label>left fist</label>
    <labelNoLocation>fist</labelNoLocation>
    <capacities>
      <li>Blunt</li>
    </capacities>
    <power>12.0</power>
    <cooldownTime>2</cooldownTime>
    <linkedBodyPartsGroup>LeftHand</linkedBodyPartsGroup>
    <alwaysTreatAsWeapon>true</alwaysTreatAsWeapon>
  </li>
  <li>
    <label>right fist</label>
    <labelNoLocation>fist</labelNoLocation>
    <capacities>
      <li>Blunt</li>
    </capacities>
    <power>12.0</power>
    <cooldownTime>2</cooldownTime>
    <linkedBodyPartsGroup>RightHand</linkedBodyPartsGroup>
    <alwaysTreatAsWeapon>true</alwaysTreatAsWeapon>
  </li>
  <li>
    <label>head</label>
    <capacities>
      <li>Blunt</li>
    </capacities>
    <power>8.5</power>
    <cooldownTime>2</cooldownTime>
    <linkedBodyPartsGroup>HeadAttackTool</linkedBodyPartsGroup>
    <ensureLinkedBodyPartsGroupAlwaysUsable>true</ensureLinkedBodyPartsGroupAlwaysUsable>
    <chanceFactor>0.2</chanceFactor>
  </li>
</tools>
```

**분석**:
- 3개의 Tool 정의 (왼손, 오른손, 머리)
- `alwaysTreatAsWeapon: true` - 손은 항상 무기로 취급
- `chanceFactor: 0.2` - 머리는 20% 확률

### 3.3 Odyssey Drone - extraMeleeDamages 사용

```xml
<tools>
  <li>
    <capacities>
      <li>Stab</li>
    </capacities>
    <power>6</power>
    <cooldownTime>2.4</cooldownTime>
    <linkedBodyPartsGroup>SpikedShellAttackTool</linkedBodyPartsGroup>
    <ensureLinkedBodyPartsGroupAlwaysUsable>true</ensureLinkedBodyPartsGroupAlwaysUsable>
    <extraMeleeDamages>
      <li>
        <def>DamageDef 모든 유형 찾아서</def>
        <amount>3</amount>
      </li>
    </extraMeleeDamages>
  </li>
</tools>
```

**분석**:
- `extraMeleeDamages`로 추가 `NerveStun` 데미지 3 부여
- 기본 Stab 데미지 외에 추가 효과

---

## 4. 건축물 피해 조정 방법

### 4.1 Tool 자체에는 건축물 피해 속성이 없음

**중요**: Tool 클래스에는 건축물 피해 관련 속성이 없습니다. 건축물 피해는 **DamageDef**에서 조정합니다.

### 4.2 DamageDef 속성 (핵심)

#### buildingDamageFactor
- **위치**: `DamageDef`
- **설명**: 건물 피해 배율
- **예시**: `<buildingDamageFactor>10</buildingDamageFactor>`

#### buildingDamageFactorImpassable
- **위치**: `DamageDef`
- **설명**: 통과 불가 건물(벽) 피해 배율
- **예시**: `<buildingDamageFactorImpassable>0.75</buildingDamageFactorImpassable>`

#### buildingDamageFactorPassable
- **위치**: `DamageDef`
- **설명**: 통과 가능 건물(문) 피해 배율
- **예시**: `<buildingDamageFactorPassable>5</buildingDamageFactorPassable>`

### 4.3 동작 흐름

1. **Tool** (`capacities: Demolish`) 
   ↓
2. **ManeuverDef** (`requiredCapacity: Demolish`)
   ↓
3. **DamageDef** (`defName: Demolish`, `buildingDamageFactor: 10`)
   ↓
4. **건축물 피해 적용**

---

## 5. 요약

### ToolCapacityDef 속성
- **Def 기본 속성만 사용** (defName, label, description 등)
- **건축물 피해 관련 속성 없음**

### Tool 속성 (주요)
| 속성 | 타입 | 기본값 | 설명 |
|------|------|-------|------|
| **capacities** | List\<ToolCapacityDef\> | - | ToolCapacity 목록 |
| **power** | float | - | 기본 공격력 |
| **armorPenetration** | float | -1 | 방어관통 (-1이면 자동) |
| **cooldownTime** | float | - | 쿨다운 시간 |
| **chanceFactor** | float | 1.0 | 선택 확률 배율 |
| **alwaysTreatAsWeapon** | bool | false | 항상 무기로 취급 |
| **extraMeleeDamages** | List\<ExtraDamage\> | null | 추가 데미지 |
| **linkedBodyPartsGroup** | BodyPartGroupDef | null | 연결된 신체 부위 |
| **ensureLinkedBodyPartsGroupAlwaysUsable** | bool | false | 부위 항상 사용 가능 |
| **soundMeleeHit** | SoundDef | null | 명중 사운드 |
| **soundMeleeMiss** | SoundDef | null | 빗나감 사운드 |

### 건축물 피해 조정
- **Tool/ToolCapacityDef에는 건축물 피해 속성이 없음**
- **DamageDef의 buildingDamageFactor 계열 속성으로 조정**
- **ManeuverDef를 통해 ToolCapacity → DamageDef 연결**

---

## 참고 소스코드 위치

- `RimworldSource/Verse/ToolCapacityDef.cs` - ToolCapacityDef 클래스
- `RimworldSource/Verse/Tool.cs` - Tool 클래스
- `RimworldSource/Verse/ExtraDamage.cs` - ExtraDamage 클래스
- `RimworldSource/Verse/SurpriseAttackProps.cs` - SurpriseAttackProps 클래스
- `RimworldSource/Verse/Def.cs` - Def 기본 클래스

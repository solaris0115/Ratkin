# 건랜스 포격 확률 분석 보고서

**작성일**: 2024-12-29
**분석 목적**: GunlanceShell_Normal이 너무 자주 발동하는 문제 분석

## 문제 정의

건랜스에 `point` (Stab)와 `edge` (GunlanceShell_Normal) 두 개의 tools가 있는데, edge가 너무 자주 발동하는 것 같다.

```xml
<tools>
    <li>
        <label>point</label>
        <capacities>
            <li>Stab</li>
        </capacities>
        <power>16</power>
        <cooldownTime>3</cooldownTime>
    </li>
    <li>
        <label>edge</label>
        <capacities>
            <li>GunlanceShell_Normal</li>
        </capacities>
        <power>27</power>
        <cooldownTime>3.4</cooldownTime>
    </li>
</tools>
```

## 근접 무기 Tool 선택 메커니즘 분석

### 1. Tool → Verb 생성 과정

림월드에서 Tool은 직접 사용되지 않고, 각 Tool의 Capacity에 연결된 ManeuverDef를 통해 Verb가 생성됩니다.

**VerbTracker.cs** (225-247):
```csharp
List<Tool> tools = this.directOwner.Tools;
if (tools != null)
{
    for (int j = 0; j < tools.Count; j++)
    {
        Tool tool = tools[j];
        foreach (ManeuverDef maneuverDef in tool.Maneuvers)
        {
            VerbProperties verb = maneuverDef.verb;
            string text2 = Verb.CalculateUniqueLoadID(this.directOwner, tool, maneuverDef);
            this.InitVerb(creator(verb.verbClass, text2), verb, tool, maneuverDef, text2);
        }
    }
}
```

**Tool.cs** (66-74):
```csharp
public IEnumerable<ManeuverDef> Maneuvers
{
    get
    {
        return from x in DefDatabase<ManeuverDef>.AllDefsListForReading
        where this.capacities.Contains(x.requiredCapacity)
        select x;
    }
}
```

### 2. Verb 선택 로직

**Pawn_MeleeVerbs.cs** - `ChooseMeleeVerb` (61-98):
- 가용한 모든 Verb 목록 생성
- 각 Verb에 대해 `GetSelectionWeight` 호출
- `RandomElementByWeight`로 선택

```csharp
List<VerbEntry> updatedAvailableVerbsList = this.GetUpdatedAvailableVerbsList(flag);
if (updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out verbEntry))
{
    flag2 = true;
}
```

### 3. Selection Weight 계산

#### 3.1 Initial Verb Weight

**VerbUtility.cs** - `InitialVerbWeight` (231-234):
```csharp
public static float InitialVerbWeight(Verb v, Pawn p)
{
    return VerbUtility.DPS(v, p) * VerbUtility.AdditionalSelectionFactor(v);
}
```

**DPS 계산** (236-239):
```csharp
public static float DPS(Verb v, Pawn p)
{
    return v.verbProps.AdjustedMeleeDamageAmount(v, p) 
        * (1f + v.verbProps.AdjustedArmorPenetration(v, p)) 
        * v.verbProps.accuracyTouch 
        / v.verbProps.AdjustedFullCycleTime(v, p);
}
```

**AdditionalSelectionFactor** (241-252):
```csharp
private static float AdditionalSelectionFactor(Verb v)
{
    float num = (v.tool != null) ? v.tool.chanceFactor : 1f;
    if (v.verbProps.meleeDamageDef != null && !v.verbProps.meleeDamageDef.additionalHediffs.NullOrEmpty())
    {
        foreach (DamageDefAdditionalHediff damageDefAdditionalHediff in v.verbProps.meleeDamageDef.additionalHediffs)
        {
            num += 0.1f;
        }
    }
    return num;
}
```

**핵심**: Tool의 `chanceFactor`가 가중치에 곱해집니다. 기본값은 1.0입니다.

#### 3.2 Final Selection Weight (카테고리화)

**VerbUtility.cs** - `FinalSelectionWeight` (254-273):
```csharp
public static float FinalSelectionWeight(Verb verb, Pawn p, List<Verb> allMeleeVerbs, float highestWeight)
{
    VerbSelectionCategory selectionCategory = verb.GetSelectionCategory(p, highestWeight);
    if (selectionCategory == VerbSelectionCategory.Worst)
    {
        return 0f;
    }
    int num = 0;
    foreach (Verb current in allMeleeVerbs)
    {
        if (current.GetSelectionCategory(p, highestWeight) == selectionCategory)
        {
            num++;
        }
    }
    return 1f / (float)num * ((selectionCategory == VerbSelectionCategory.Mid) ? 0.25f : 0.75f);
}
```

**VerbSelectionCategory 결정** (217-229):
- **Best**: InitialWeight >= highestWeight * 0.95
- **Mid**: 0.25 <= InitialWeight < highestWeight * 0.95
- **Worst**: InitialWeight < highestWeight * 0.25

**핵심**: 
- 같은 카테고리 내에서는 동일한 가중치를 가짐
- Best 카테고리는 0.75 가중치, Mid는 0.25 가중치

## GunlanceShell 발동 확률 계산

### 현재 설정

| Tool | Power | Cooldown | ChanceFactor |
|------|-------|----------|--------------|
| point (Stab) | 16 | 3.0 | 1.0 (기본) |
| edge (Shell) | 27 | 3.4 | 1.0 (기본) |

### DPS 계산

**point (Stab)**:
- DPS = 16 × (1 + AP) × 1.0 / 3.0 = **5.33 × (1 + AP)**

**edge (GunlanceShell)**:
- DPS = 27 × (1 + AP) × 1.0 / 3.4 = **7.94 × (1 + AP)**

### 예상 결과

1. **edge가 약 1.5배 더 높은 DPS**를 가지므로 `InitialWeight`가 더 큼
2. 두 Verb가 모두 Best 카테고리에 속할 가능성이 높음
3. **Best 카테고리 내에서는 동일한 가중치**를 가지므로:
   - **발동 확률: 각각 50%**

### 실제 관찰과의 차이

사용자가 "너무 자주 발동한다"고 느끼는 이유:
1. edge의 DPS가 더 높아서 이미 더 자주 선택되는 경향
2. 카테고리가 잘릴 때 edge만 Best에 남아 100% 확률이 될 수 있음
3. 시각적/청각적 효과가 크게 느껴짐

## 해결 방법

### 방법 1: chanceFactor 조정 (권장)

point의 chanceFactor를 높여서 선택 확률 증가:

```xml
<tools>
    <li>
        <label>point</label>
        <capacities>
            <li>Stab</li>
        </capacities>
        <power>16</power>
        <cooldownTime>3</cooldownTime>
        <chanceFactor>2.0</chanceFactor>
    </li>
    <li>
        <label>edge</label>
        <capacities>
            <li>GunlanceShell_Normal</li>
        </capacities>
        <power>27</power>
        <cooldownTime>3.4</cooldownTime>
    </li>
</tools>
```

**효과**: point의 InitialWeight가 2배가 되어 edge보다 높아질 수 있음

### 방법 2: Power/Cooldown 조정

point의 Power를 높이거나 Cooldown을 줄여 DPS를 높임:

```xml
<tools>
    <li>
        <label>point</label>
        <capacities>
            <li>Stab</li>
        </capacities>
        <power>25</power>
        <cooldownTime>3</cooldownTime>
    </li>
</tools>
```

### 방법 3: edge의 chanceFactor 감소

edge의 chanceFactor를 낮춤:

```xml
<tools>
    <li>
        <label>edge</label>
        <capacities>
            <li>GunlanceShell_Normal</li>
        </capacities>
        <power>27</power>
        <cooldownTime>3.4</cooldownTime>
        <chanceFactor>0.5</chanceFactor>
    </li>
</tools>
```

## 참고: 림월드 원본 예시

### 도끼 (Axe) 예시

```xml
<tools>
    <li>
        <label>blade</label>
        <capacities>
            <li>Cut</li>
        </capacities>
        <power>14</power>
        <cooldownTime>2.8</cooldownTime>
        <chanceFactor>3</chanceFactor>
    </li>
    <li>
        <label>handle</label>
        <capacities>
            <li>Blunt</li>
        </capacities>
        <power>6</power>
        <cooldownTime>1.5</cooldownTime>
    </li>
</tools>
```

blade에 chanceFactor 3.0을 주어서 더 자주 선택되도록 설계됨.

## 결론

GunlanceShell이 자주 발동하는 이유:
1. 기본적으로 **edge의 DPS가 더 높음** (27/3.4 vs 16/3.0)
2. 두 tool 모두 chanceFactor가 없어서 **같은 카테고리에서는 50% 확률**
3. edge가 더 높은 성능을 가지므로 Best 카테고리에 있을 가능성이 높음

**권장 조치**: point에 chanceFactor를 추가하거나, edge의 chanceFactor를 낮춰 포격 빈도를 조절.


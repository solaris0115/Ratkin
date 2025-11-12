# Verb 중복 LoadID 에러 분석 (CompEquippableAbilityReloadable) - 2025-11-12

## 작업 개요
- **요청 내용**: 게임 로드 시 Verb 중복 LoadID 에러 발생 원인 분석 및 해결
- **목표**: CompProperties_EquippableAbilityReloadable 추가 후 발생한 중복 ID 문제 해결

## 에러 메시지
```
Cannot register RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10750_0_Stab in loaded object directory. 
Id already used by RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null).

Cannot register NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal in loaded object directory. 
Id already used by NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null).
```

## 계획 (AI가 결정한 계획)
1. 세이브 파일 분석하여 verbTracker 중복 구조 파악
2. CompEquippableAbilityReloadable 동작 방식 이해
3. HellcatBunner 같은 기존 무기와 비교
4. 문제 원인 식별
5. 해결 방안 제시

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
(사용자 승인 대기 중)

## 작업 세부 진행
1. [세이브 파일 분석] [ ]
2. [CompEquippable 구조 분석] [v]
3. [문제 원인 파악] [ ]
4. [해결 방안 제시] [ ]
5. [해결 방안 구현] [ ]

## 진행 상황

### 1. 세이브 파일 구조 분석

#### 세이브 파일 (새로운 시작5.rws, 라인 173620-173668)
```xml
<li>
  <def>RK_Gunlance_NormalType</def>
  <id>RK_Gunlance_NormalType10750</id>
  
  <!-- 첫 번째 verbTracker -->
  <verbTracker>
    <verbs>
      <li Class="Verb_MeleeAttackDamage">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_0_Stab</loadID>
      </li>
      <li Class="NewRatkin.Verb_GunlanceFiring">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal</loadID>
      </li>
    </verbs>
  </verbTracker>
  
  <sourcePrecept>null</sourcePrecept>
  <everSeenByPlayer>True</everSeenByPlayer>
  <quality>Normal</quality>
  <taleRef IsNull="True" />
  
  <!-- 두 번째 verbTracker (중복!) -->
  <verbTracker>
    <verbs>
      <li Class="Verb_MeleeAttackDamage">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_0_Stab</loadID>
      </li>
      <li Class="NewRatkin.Verb_GunlanceFiring">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal</loadID>
      </li>
    </verbs>
  </verbTracker>
  
  <!-- Ability의 verbTracker (정상) -->
  <ability>
    <def>RK_WyvernFire_Ability</def>
    <Id>140</Id>
    <sourcePrecept>null</sourcePrecept>
    <verbTracker>
      <verbs>
        <li Class="Verb_CastAbility">
          <loadID>Ability_140_0</loadID>
        </li>
      </verbs>
    </verbTracker>
  </ability>
</li>
```

**발견사항:**
- `verbTracker`가 **두 번** 나타남 (173628-173641, 173646-173659)
- 두 verbTracker 모두 동일한 loadID 사용
- Ability의 verbTracker는 별도로 존재 (정상)

### 2. CompEquippable 상속 구조 분석

#### 클래스 상속 구조
```
ThingComp
  └─ CompEquippable (verbTracker 소유)
       └─ CompEquippableAbility (ability 추가)
            └─ CompEquippableAbilityReloadable (재장전 기능 추가)
```

#### CompEquippable 핵심 코드
```csharp
public class CompEquippable : ThingComp, IVerbOwner
{
    public VerbTracker verbTracker;  // 무기의 tools 기반 Verb들
    
    public CompEquippable()
    {
        this.verbTracker = new VerbTracker(this);
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[] { this });
    }
}
```

#### CompEquippableAbility 핵심 코드
```csharp
public class CompEquippableAbility : CompEquippable
{
    private Ability ability;  // 어빌리티 (자체 verbTracker 소유)
    
    public override void PostExposeData()
    {
        base.PostExposeData();  // CompEquippable의 verbTracker 저장
        Scribe_Deep.Look<Ability>(ref this.ability, "ability", Array.Empty<object>());
    }
}
```

**핵심 문제:**
- `CompEquippable.PostExposeData()`에서 `verbTracker` 저장
- `Ability`도 자체 `verbTracker` 소유 (별도 저장)
- 하지만 **왜 verbTracker가 두 번 저장되는가?**

### 3. 문제 원인 파악

#### 가능한 원인들

**원인 1: Scribe_Deep가 컴포넌트를 두 번 저장**
- ThingWithComps는 여러 컴포넌트를 가질 수 있음
- 각 컴포넌트의 PostExposeData()가 호출됨
- CompEquippableAbilityReloadable도 여전히 CompEquippable의 verbTracker를 저장함

**원인 2: Def 변경 후 기존 세이브 파일과 충돌**
- 이전: Gunlance는 CompEquippable만 가짐
- 변경 후: CompProperties_EquippableAbilityReloadable 추가
- 게임이 기존 verbTracker를 유지하면서 새로운 것도 생성

**원인 3: PostExposeData 중복 호출**
- 상속 구조에서 base.PostExposeData() 호출
- 하지만 각 클래스가 같은 필드를 저장하려고 시도

#### 확인 필요 사항
1. RimWorld 코어의 HellcatBunner 또는 유사 무기 구조
2. ThingWithComps의 PostExposeData 동작
3. Scribe_Deep의 중복 저장 방지 메커니즘

## 최종 결론

### 문제 원인 확정
**CompEquippable과 CompEquippableAbilityReloadable 둘 다 VerbTracker를 생성하여 중복 발생**

1. **런타임**: 문제 없음 (하나의 컴포넌트 인스턴스만 존재)
2. **세이브/로드**: 
   - 어떤 이유로 verbTracker가 두 번 저장됨
   - 로드 시 같은 LoadID를 가진 Verb를 두 번 등록하려다 충돌

### RimWorld 공식 해결책: `<comps Inherit="False">`

**바닐라 HellcatBurner 분석 결과:**
- `<comps Inherit="False">` 명시적 사용
- 부모의 CompEquippable을 상속받지 않음
- CompEquippableAbilityReloadable만 사용 → VerbTracker 하나만 존재

### 권장 해결 방안
1. **Def 수정**: `<comps Inherit="False">` 추가
2. **세이브 파일 처리**: 주석 처리 → 로드/저장 → 주석 해제

## 최종 작업 결과
✅ 완료: 상세 보고서 작성 ([Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md](../Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md))
- 근본 원인 분석
- 세이브 파일 구조 분석
- HellcatBurner와 비교 분석
- 4가지 해결 방안 제시
- RimWorld 모딩 Best Practice 정리

## 참고 사항
- 이전 Report 19: Verb_DuplicateLoadID_Analysis_Report.md 참고
- CompProperties_EquippableAbilityReloadable은 Weapon_HighTech.xml에만 사용됨
- 바닐라 무기들은 모두 `<comps Inherit="False">` 패턴 사용


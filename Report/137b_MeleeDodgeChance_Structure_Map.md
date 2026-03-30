# MeleeDodgeChance 구조 맵 (간결 버전)

> **Tags**: MeleeDodgeChance StatDef 구조맵 데이터흐름 연관관계 새스탯참조

---

## 1. 데이터 정의 구조

```
StatDef (XML)
├─ defName: "MeleeDodgeChance"
├─ category: StatCategoryDef → "PawnCombat"
├─ skillNeedOffsets
│   └─ SkillNeed_BaseBonus
│       ├─ skill → SkillDef "Melee"
│       ├─ baseValue
│       └─ bonusPerLevel
├─ capacityOffsets
│   ├─ PawnCapacityOffset → PawnCapacityDef "Moving" + scale
│   └─ PawnCapacityOffset → PawnCapacityDef "Sight" + scale + max
├─ postProcessCurve → SimpleCurve (points)
├─ toStringStyle / toStringStyleUnfinalized
├─ displayPriorityInCategory
├─ showDevelopmentalStageFilter
├─ minValue / maxValue
└─ defaultBaseValue
```

---

## 2. 스탯 계산 파이프라인 (StatWorker)

```
StatWorker.GetValue(pawn)
│
├─ GetValueUnfinalized()
│   │
│   ├─ ① base ← StatRequest.GetBaseValueFor()
│   │       └─ ThingDef.statBases[해당stat] or StatDef.defaultBaseValue
│   │
│   ├─ ② +Σ skillNeedOffsets[i].ValueFor(pawn)
│   │       └─ SkillNeed_BaseBonus.ValueFor()
│   │           └─ pawn.skills.GetSkill(skill).Level → baseValue + bonusPerLevel × level
│   │
│   ├─ ③ +Σ capacityOffsets[i].GetOffset(pawn.health.capacities.GetLevel(capacity))
│   │       └─ PawnCapacityOffset.GetOffset(efficiency)
│   │           └─ (min(efficiency, max) - 1.0) × scale
│   │
│   ├─ ④ +Σ 외부 가산 소스들
│   │       ├─ pawn.story.traits[].OffsetOfStat()     ← TraitDef.statOffsets
│   │       ├─ pawn.health.hediffs[].CurStage         ← HediffDef.stages[].statOffsets
│   │       ├─ pawn.Ideo.precepts[].def.statOffsets    ← PreceptDef.statOffsets
│   │       ├─ pawn.genes[].def.statOffsets            ← GeneDef.statOffsets
│   │       ├─ equipment.equippedStatOffsets            ← ThingDef.equippedStatOffsets
│   │       └─ apparel[].equippedStatOffsets            ← ThingDef.equippedStatOffsets
│   │
│   ├─ ⑤ ×Π 외부 곱연산 소스들
│   │       ├─ trait.StatFactor()                       ← TraitDef.statFactors
│   │       ├─ hediff.CurStage.statFactors
│   │       ├─ precept.def.statFactors
│   │       ├─ gene.def.statFactors
│   │       └─ equipment/apparel.equippedStatFactors
│   │
│   └─ return Raw값
│
└─ FinalizeValue(raw)
    ├─ ⑥ StatDef.parts[].TransformValue()    ← StatPart 서브클래스 (미사용)
    ├─ ⑦ StatDef.postProcessCurve.Evaluate(raw)  ← SimpleCurve 선형보간
    ├─ ⑧ ×Π StatDef.postProcessStatFactors       (미사용)
    └─ ⑨ Clamp(minValue, maxValue)
```

---

## 3. 전투 판정 흐름 (Verb_MeleeAttack)

```
Verb_MeleeAttack.TryCastShot()
│
├─ Rand.Chance( GetNonMissChance(target) )  ← 공격자 MeleeHitChance
│   ├─ FAIL → Miss (빗맞춤)
│   │   ├─ SoundMiss()
│   │   └─ combatLogRulesMiss
│   │
│   └─ PASS → Rand.Chance( GetDodgeChance(target) )  ← 방어자 MeleeDodgeChance
│       │
│       ├─ GetDodgeChance(target)
│       │   ├─ surpriseAttack? → 0
│       │   ├─ IsTargetImmobile? → 0
│       │   ├─ target is not Pawn? → 0
│       │   ├─ 원거리 Verb Stance_Busy? → 0
│       │   ├─ pawn.GetStatValue(StatDefOf.MeleeDodgeChance)  ← StatWorker 경유
│       │   └─ (Ideology) += pawn.GetStatValue(조명별 Offset StatDef)
│       │       ├─ MeleeDodgeChanceOutdoorsLitOffset
│       │       ├─ MeleeDodgeChanceOutdoorsDarkOffset
│       │       ├─ MeleeDodgeChanceIndoorsDarkOffset
│       │       └─ MeleeDodgeChanceIndoorsLitOffset
│       │
│       ├─ DODGE → 회피 성공
│       │   ├─ SoundDodge(target)          ← RaceProperties.soundMeleeDodge
│       │   ├─ MoteMaker.ThrowText("Dodge") ← TextMote
│       │   └─ CreateCombatLog(combatLogRulesDodge)
│       │       └─ ManeuverDef.combatLogRulesDodge → RulePackDef
│       │
│       └─ HIT → 피격
│           ├─ ApplyMeleeDamageToTarget()
│           ├─ SoundHitPawn/Building()
│           └─ CreateCombatLog(combatLogRulesHit)
```

---

## 4. UI 표시 구조

```
Pawn 정보 창 (스탯 탭)
│
├─ StatsReportUtility
│   └─ StatsToDraw(pawn)
│       └─ StatDef 목록 순회 → showOnPawns / showDevelopmentalStageFilter 필터
│           └─ StatDrawEntry 생성
│               ├─ category ← StatDef.category (PawnCombat)
│               ├─ 정렬순서 ← StatDef.displayPriorityInCategory
│               ├─ 표시값 ← StatWorker.GetValue() → toStringStyle 포맷
│               └─ 설명 펼침 ← StatWorker.GetExplanationUnfinalized()
│                   └─ 자동으로 각 보정 소스를 줄별 나열
│                       ├─ "Base value: ..."
│                       ├─ "Melee skill: +..."
│                       ├─ "Moving: ..."
│                       ├─ "장비 이름: ..."
│                       └─ "Final: ...%"
│
├─ Pawn.SpecialDisplayStats()  ← 추가 행 (이데올로기)
│   └─ DarknessCombatUtility.GetStatEntriesForPawn(pawn)
│       └─ precepts에서 4종 MeleeDodgeChance*Offset 수집
│           └─ StatDrawEntry (min~max 범위 문자열)
│               └─ displayPriority = StatDisplayOrder.Pawn_DarknessMeleeDodgeChance
```

---

## 5. 외부 Def → StatDef 참조 관계

```
ThingDef (장비/갑옷)
├─ statBases: { MeleeDodgeChance: 값 }           ← 종족 base 오버라이드
├─ equippedStatOffsets: { MeleeDodgeChance: 값 }  ← 장비 착용 시 Raw 가산
└─ equippedStatFactors: { MeleeDodgeChance: 값 }  ← 장비 착용 시 곱연산

TraitDef
├─ statOffsets: { MeleeDodgeChance: 값 }
└─ statFactors: { MeleeDodgeChance: 값 }

HediffDef.stages[]
├─ statOffsets: { MeleeDodgeChance: 값 }
└─ statFactors: { MeleeDodgeChance: 값 }

PreceptDef (이데올로기)
├─ statOffsets: { MeleeDodgeChance*Offset: 값 }
└─ conditionalStatAffecters[].statOffsets

GeneDef
├─ statOffsets: { MeleeDodgeChance: 값 }
└─ statFactors: { MeleeDodgeChance: 값 }

AbilityDef (능력)
└─ hediff → HediffDef → stages[].statOffsets

ManeuverDef
└─ combatLogRulesDodge → RulePackDef

RaceProperties
└─ soundMeleeDodge → SoundDef
```

---

## 6. C# 참조 지점

```
StatDefOf.cs
└─ public static StatDef MeleeDodgeChance          ← 코드 참조 진입점

Verb_MeleeAttack.cs
└─ GetDodgeChance() → pawn.GetStatValue(StatDefOf.MeleeDodgeChance)

Verb_MeleeExplosion.cs  (Ratkin)
└─ GetDodgeChance() → pawn.GetStatValue(StatDefOf.MeleeDodgeChance)

Verb_GunlanceFiring.cs  (Ratkin)
└─ GetDodgeChance() → pawn.GetStatValue(StatDefOf.MeleeDodgeChance)

DarknessCombatUtility.cs
└─ GetStatEntriesForPawn() → 4종 Offset StatDef 수집 → UI 표시

StatWorker.cs
├─ GetValueUnfinalized() → 계산 엔진
└─ FinalizeValue() → postProcessCurve 적용
```

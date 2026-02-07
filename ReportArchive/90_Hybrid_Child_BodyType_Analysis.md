# Hybrid Child BodyType 결정 로직 분석 보고서

**태그**: BodyType Hybrid Child Ratkin Human AlienRace Reproduction

## 요약

인간 + 랫킨 혼혈 아이의 BodyType 결정은 **아이의 종족(Race)**에 따라 달라집니다. 혼혈 아이가 랫킨 종족으로 생성되면 랫킨의 bodyTypes(Thin, Baby, Child) 중에서 선택되고, 인간 종족으로 생성되면 인간의 기본 BodyType 로직이 적용됩니다.

## 1. 혼혈 아이 종족 결정

### 1.1 BirthOutcomeHelper 로직

**위치**: `AlienRace/HarmonyPatches.cs:991-1006`

```csharp
public static PawnKindDef BirthOutcomeHelper(Pawn mother, Pawn partner)
{
    if (mother?.def is not ThingDef_AlienRace alienProps)
        return mother?.kindDef;

    PawnKindDef kindDef = alienProps.alienRace.generalSettings.reproduction.childKindDef;

    if (partner != null)
    {
        List<HybridSpecificSettings> hybrids = alienProps.alienRace.generalSettings.reproduction.hybridSpecific
            .Where(hss => hss.partnerRace == partner.def).ToList();
        if (hybrids.Any() && hybrids.TryRandomElementByWeight(hss => hss.probability, out HybridSpecificSettings res))
            kindDef = res.childKindDef;
    }

    return kindDef ?? mother.kindDef;
}
```

**결정 순서**:
1. 어머니가 AlienRace인지 확인
2. 어머니 종족의 기본 `childKindDef` 사용
3. 파트너가 있고 `hybridSpecific` 설정이 있으면 그 중에서 가중치 기반 선택
4. 없으면 어머니의 `kindDef` 사용

**현재 랫킨 설정**:
- `hybridSpecific` 설정 없음
- 따라서 **어머니의 종족으로 생성됨**
  - 랫킨 어머니 → 랫킨 아이
  - 인간 어머니 → 인간 아이 (AlienRace가 아니므로 기본 로직)

## 2. BodyType 결정 로직

### 2.1 림월드 원본 로직

**위치**: `RimworldSource/Verse/PawnGenerator.cs:2002-2045`

```csharp
public static BodyTypeDef GetBodyTypeFor(Pawn pawn)
{
    // 유아/아이 단계
    if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
    {
        if (pawn.DevelopmentalStage == DevelopmentalStage.Baby)
            return BodyTypeDefOf.Baby;
        return BodyTypeDefOf.Child;
    }
    
    // 성인 단계
    // 1. 유전자에서 bodyType 확인
    if (ModsConfig.BiotechActive && pawn.genes != null)
    {
        // 유전자의 bodyType 중 랜덤 선택
        if (PawnGenerator.tmpBodyTypes.TryRandomElement(out result))
            return result;
    }
    
    // 2. 백스토리에서 bodyType 확인
    if (pawn.story.Adulthood != null)
        return pawn.story.Adulthood.BodyTypeFor(pawn.gender);
    
    // 3. 기본값
    if (Rand.Value < 0.5f)
        return BodyTypeDefOf.Thin;
    if (pawn.gender != Gender.Female)
        return BodyTypeDefOf.Male;
    return BodyTypeDefOf.Female;
}
```

**결정 순서**:
1. 유아/아이 단계 → Baby/Child 고정
2. 성인 단계:
   - 유전자의 bodyType (있으면)
   - 백스토리의 bodyType (있으면)
   - 기본값: 50% Thin, 아니면 성별에 따라 Male/Female

### 2.2 AlienRace 모드 패치

**위치**: `AlienRace/HarmonyPatches.cs:3840-3890`

**Postfix 적용**:
- `GetBodyTypeForPostfix`: `PawnGenerator.GetBodyTypeFor` 결과를 수정
- `GenerateBodyTypePostfix`: `PawnGenerator.GenerateBodyType` 결과를 수정

**CheckBodyType 로직**:

```csharp
public static BodyTypeDef CheckBodyType(Pawn pawn, BodyTypeDef bodyType)
{
    // 1. 특수 백스토리 체크
    if (AlienBackstoryDef.checkBodyType.Contains(pawn.story.GetBackstory(BackstorySlot.Adulthood)))
        bodyType = DefDatabase<BodyTypeDef>.GetRandom();
    
    // 2. AlienRace인 경우
    if (pawn.def is ThingDef_AlienRace alienProps && 
        alienProps.alienRace.generalSettings.alienPartGenerator is { } parts &&
        !parts.bodyTypes.NullOrEmpty())
    {
        List<BodyTypeDef> bodyTypeDefs = parts.bodyTypes.ListFullCopy();
        
        // 유아/아이 단계
        if ((Baby || Newborn) && bodyTypeDefs.Contains(BodyTypeDefOf.Baby))
            bodyType = BodyTypeDefOf.Baby;
        else if (Juvenile && bodyTypeDefs.Contains(BodyTypeDefOf.Child))
            bodyType = BodyTypeDefOf.Child;
        else // 성인 단계
        {
            bodyTypeDefs.Remove(BodyTypeDefOf.Baby);
            bodyTypeDefs.Remove(BodyTypeDefOf.Child);
            
            // 성별에 따라 반대 성별 기본 BodyType 제거 (여러 개 있을 때만)
            if (pawn.gender == Gender.Male)
            {
                BodyTypeDef femaleBodyType = parts.defaultFemaleBodyType;
                if (bodyTypeDefs.Contains(femaleBodyType) && bodyTypeDefs.Count > 1)
                    bodyTypeDefs.Remove(femaleBodyType);
            }
            if (pawn.gender == Gender.Female)
            {
                BodyTypeDef maleBodyType = parts.defaultMaleBodyType;
                if (bodyTypeDefs.Contains(maleBodyType) && bodyTypeDefs.Count > 1)
                    bodyTypeDefs.Remove(maleBodyType);
            }
            
            // 현재 bodyType이 리스트에 없으면 랜덤 선택
            if (!bodyTypeDefs.Contains(bodyType))
                bodyType = bodyTypeDefs.RandomElement();
        }
    }
    
    return bodyType;
}
```

**핵심 로직**:
1. AlienRace인 경우 해당 종족의 `bodyTypes` 리스트 사용
2. 유아/아이 단계는 Baby/Child 고정
3. 성인 단계:
   - Baby/Child 제거
   - 성별에 따라 반대 성별 기본 BodyType 제거 (여러 개 있을 때만)
   - 현재 bodyType이 리스트에 없으면 리스트에서 랜덤 선택

## 3. 랫킨 BodyType 설정

**위치**: `Project/1.6/Defs/ThingDefs_Races/Races_Rakinlike.xml:27-31`

```xml
<bodyTypes>
    <li>Thin</li>
    <li MayRequire="Ludeon.RimWorld.Biotech">Baby</li>
    <li MayRequire="Ludeon.RimWorld.Biotech">Child</li>
</bodyTypes>
```

**설정 내용**:
- 성인: **Thin만** 사용 가능
- 유아/아이: Baby, Child 사용 가능
- `defaultMaleBodyType`, `defaultFemaleBodyType` 미설정

## 4. 혼혈 아이 BodyType 결정 시나리오

### 시나리오 1: 랫킨 어머니 + 인간 아버지

**아이 종족**: 랫킨 (어머니 종족 상속)

**BodyType 결정**:
1. **유아/아이 단계**: Baby 또는 Child (고정)
2. **성인 단계**:
   - 림월드 원본: 유전자 → 백스토리 → 기본값(Thin/Male/Female)
   - AlienRace 패치: 랫킨의 bodyTypes(Thin) 확인
   - **결과**: Thin (랫킨의 bodyTypes에 Thin만 있으므로)

### 시나리오 2: 인간 어머니 + 랫킨 아버지

**아이 종족**: 인간 (어머니가 AlienRace가 아니므로 기본 로직)

**BodyType 결정**:
1. **유아/아이 단계**: Baby 또는 Child (고정)
2. **성인 단계**:
   - 림월드 원본: 유전자 → 백스토리 → 기본값(Thin/Male/Female)
   - AlienRace 패치: 적용 안 됨 (인간은 AlienRace가 아님)
   - **결과**: 유전자/백스토리에 따라 Thin, Male, Female 중 선택

### 시나리오 3: 랫킨 어머니 + 랫킨 아버지

**아이 종족**: 랫킨

**BodyType 결정**:
- 시나리오 1과 동일
- **성인 단계**: Thin

## 5. 생명 단계 전환 시 BodyType 변경

### 5.1 아이 → 성인 전환

**위치**: `RimworldSource/RimWorld/LifeStageWorker_HumanlikeAdult.cs:55-64`

```csharp
if (pawn.story.bodyType == BodyTypeDefOf.Child || pawn.story.bodyType == BodyTypeDefOf.Baby)
{
    // 아이 전용 의류 제거
    apparel2.DropAllOrMoveAllToInventory(...);
    
    // BodyType 재생성
    BodyTypeDef bodyTypeFor = PawnGenerator.GetBodyTypeFor(pawn);
    pawn.story.bodyType = bodyTypeFor;
    pawn.Drawer.renderer.SetAllGraphicsDirty();
}
```

**동작**:
- 성인 단계로 전환 시 `PawnGenerator.GetBodyTypeFor` 호출
- AlienRace 패치가 적용되어 랫킨의 bodyTypes 확인
- 랫킨 아이 → 성인: **Thin으로 변경**

### 5.2 유아 → 아이 전환

**위치**: `RimworldSource/RimWorld/LifeStageWorker_HumanlikeChild.cs:43-52`

```csharp
if (pawn.story.bodyType != BodyTypeDefOf.Child)
{
    // 유아 전용 의류 제거
    apparel2.DropAllOrMoveAllToInventory(...);
    
    // BodyType 재생성
    BodyTypeDef bodyTypeFor = PawnGenerator.GetBodyTypeFor(pawn);
    pawn.story.bodyType = bodyTypeFor;
}
```

**동작**:
- 아이 단계로 전환 시 `PawnGenerator.GetBodyTypeFor` 호출
- 유아/아이 단계이므로 Child로 설정

## 6. 결론

### 랫킨 혼혈 아이의 BodyType 결정

1. **아이의 종족이 랫킨인 경우**:
   - 유아/아이: Baby, Child
   - 성인: **Thin** (랫킨의 bodyTypes에 Thin만 있음)

2. **아이의 종족이 인간인 경우**:
   - 유아/아이: Baby, Child
   - 성인: 유전자/백스토리에 따라 Thin, Male, Female 중 선택

3. **현재 설정의 문제점**:
   - 랫킨은 성인 BodyType이 Thin만 있음
   - 혼혈 아이가 랫킨 종족으로 생성되면 성인 시 **무조건 Thin**
   - 인간 종족으로 생성되면 Thin, Male, Female 중 선택 가능

### 권장 사항

혼혈 아이가 인간 종족으로 생성될 때도 Thin을 선호하도록 하려면:

1. **유전자 활용**: `RK_Gene_SmallBody`에 `bodyType>Thin</bodyType>` 설정 (이미 설정됨)
2. **백스토리 활용**: 랫킨 백스토리에 `bodyTypeMale>Thin</bodyTypeMale>`, `bodyTypeFemale>Thin</bodyTypeFemale>` 설정 (이미 설정됨)
3. **hybridSpecific 설정**: 인간과의 혼혈 시 랫킨 종족으로 생성되도록 설정

**현재 상태**: 랫킨 어머니의 아이는 랫킨 종족으로 생성되므로 성인 시 Thin이 보장됨. 인간 어머니의 아이는 유전자/백스토리에 따라 결정됨.

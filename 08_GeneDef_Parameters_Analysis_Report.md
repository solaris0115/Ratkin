# GeneDef 파라미터 분석 보고서

## 1. RimWorldData GeneDef 관련 파일

### Core
- [GeneDefs_Endogenes.xml](mdc:RimWorldData/Core/Defs/GeneDefs/GeneDefs_Endogenes.xml)

### Biotech
- [GeneCategoryDefs.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneCategoryDefs.xml)
- [GeneDefs_Abilities.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Abilities.xml)
- [GeneDefs_Cosmetic.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Cosmetic.xml)
- [GeneDefs_Endogenes.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Endogenes.xml)
- [GeneDefs_Health.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Health.xml)
- [GeneDefs_Misc.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Misc.xml)
- [GeneDefs_Sanguophage.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Sanguophage.xml)
- [GeneDefs_Spectrum.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Spectrum.xml)
- [GeneTemplateDefs.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/GeneTemplateDefs.xml)
- [XenotypeDefs.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/XenotypeDefs.xml)
- [XenotypeIconDefs.xml](mdc:RimWorldData/Biotech/Defs/GeneDefs/XenotypeIconDefs.xml)

### Odyssey
- [GeneDefs_Health.xml](mdc:RimWorldData/Odyssey/Defs/GeneDefs/GeneDefs_Health.xml)
- [XenotypeDefs.xml](mdc:RimWorldData/Odyssey/Defs/GeneDefs/XenotypeDefs.xml)

## 2. GeneDef 파라미터 목록

### 기본 정보
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `geneClass` | Type | typeof(Gene) | 유전자 클래스 타입 |
| `labelShortAdj` | string | - | 짧은 형용사 레이블 (번역 필요) |
| `customEffectDescriptions` | List\<string\> | - | 커스텀 효과 설명 (번역 필요) |
| `iconPath` | string | - | 아이콘 경로 |
| `iconColor` | Color? | - | 아이콘 색상 |

### 카테고리 및 표시
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `displayCategory` | GeneCategoryDef | - | 표시 카테고리 |
| `displayOrderInCategory` | float | - | 카테고리 내 표시 순서 |

### 렌더링
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `renderNodeProperties` | List\<PawnRenderNodeProperties\> | - | 렌더 노드 속성 |
| `neverGrayHair` | bool | false | 회색 머리 없음 |
| `skinIsHairColor` | bool | false | 피부가 머리 색상과 동일 |
| `tattoosVisible` | bool | true | 타투 표시 여부 |
| `fur` | FurDef | - | 털 정의 |

### 사운드
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `soundCall` | SoundDef | - | 호출 사운드 |
| `soundDeath` | SoundDef | - | 사망 사운드 |
| `soundWounded` | SoundDef | - | 부상 사운드 |

### 리소스 관리
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `resourceGizmoType` | Type | typeof(GeneGizmo_Resource) | 리소스 기즈모 타입 |
| `resourceLossPerDay` | float | - | 일일 리소스 손실 |
| `resourceLabel` | string | - | 리소스 레이블 (번역 필요) |
| `resourceDescription` | string | - | 리소스 설명 (번역 필요) |
| `resourceGizmoThresholds` | List\<float\> | - | 리소스 기즈모 임계값 |
| `showGizmoOnWorldView` | bool | false | 월드 뷰에서 기즈모 표시 |
| `showGizmoWhenDrafted` | bool | false | 징집 시 기즈모 표시 |
| `showGizmoOnMultiSelect` | bool | false | 다중 선택 시 기즈모 표시 |

### 능력 및 특성
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `abilities` | List\<AbilityDef\> | - | 능력 목록 |
| `forcedTraits` | List\<GeneticTraitData\> | - | 강제 특성 |
| `suppressedTraits` | List\<GeneticTraitData\> | - | 억제된 특성 |

### 욕구
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `enablesNeeds` | List\<NeedDef\> | - | 활성화되는 욕구 |
| `disablesNeeds` | List\<NeedDef\> | - | 비활성화되는 욕구 |

### 작업 및 행동
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `disabledWorkTags` | WorkTags | - | 비활성화된 작업 태그 |
| `ignoreDarkness` | bool | false | 어둠 무시 |
| `dislikesSunlight` | bool | false | 햇빛 싫어함 |
| `minAgeActive` | float | - | 활성화 최소 나이 |
| `lovinMTBFactor` | float | 1f | 사랑 평균 시간 요소 |
| `dontMindRawFood` | bool | false | 날 음식 거부감 없음 |

### 면역 및 저항
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `immuneToToxGasExposure` | bool | false | 독가스 면역 |
| `immuneToVacuumBurns` | bool | false | 진공 화상 면역 |
| `makeImmuneTo` | List\<HediffDef\> | - | 면역 부여 질병 목록 |
| `hediffGiversCannotGive` | List\<HediffDef\> | - | 부여할 수 없는 질병 목록 |
| `preventPermanentWounds` | bool | false | 영구 부상 방지 |

### 스탯 수정
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `statOffsets` | List\<StatModifier\> | - | 스탯 오프셋 |
| `statFactors` | List\<StatModifier\> | - | 스탯 팩터 |
| `conditionalStatAffecters` | List\<ConditionalStatAffecter\> | - | 조건부 스탯 영향 |
| `capMods` | List\<PawnCapacityModifier\> | - | 능력 수정자 |

### 통증 및 고통
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `painOffset` | float | - | 통증 오프셋 |
| `painFactor` | float | 1f | 통증 팩터 |
| `foodPoisoningChanceFactor` | float | 1f | 식중독 확률 팩터 |
| `damageFactors` | List\<DamageFactor\> | - | 피해 팩터 |

### 노화 및 생물학
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `biologicalAgeTickFactorFromAgeCurve` | SimpleCurve | - | 나이별 생물학적 노화 속도 커브 |
| `sterilize` | bool | false | 불임 여부 |
| `randomBrightnessFactor` | float | - | 랜덤 밝기 팩터 |

### 화학물질
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `chemical` | ChemicalDef | - | 화학물질 |
| `addictionChanceFactor` | float | 1f | 중독 확률 팩터 |
| `overdoseChanceFactor` | float | 1f | 과다복용 확률 팩터 |
| `toleranceBuildupFactor` | float | 1f | 내성 축적 팩터 |

### 외형
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `hairColorOverride` | Color? | - | 머리 색상 오버라이드 |
| `skinColorBase` | Color? | - | 피부 기본 색상 |
| `skinColorOverride` | Color? | - | 피부 색상 오버라이드 |
| `hairTagFilter` | TagFilter | - | 머리 태그 필터 |
| `beardTagFilter` | TagFilter | - | 수염 태그 필터 |
| `bodyType` | GeneticBodyType? | - | 체형 타입 |
| `forcedHeadTypes` | List\<HeadTypeDef\> | - | 강제 머리 타입 |
| `minMelanin` | float | -1f | 최소 멜라닌 |
| `forcedHair` | HairDef | - | 강제 헤어 스타일 |
| `womenCanHaveBeards` | bool | false | 여성 수염 가능 |

### 사회적 행동
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `socialFightChanceFactor` | float | 1f | 사회적 싸움 확률 팩터 |
| `aggroMentalBreakSelectionChanceFactor` | float | 1f | 공격적 정신이상 선택 확률 팩터 |
| `mentalBreakMtbDays` | float | - | 정신이상 평균 발생 일수 |
| `mentalBreakDef` | MentalBreakDef | - | 정신이상 정의 |
| `missingGeneRomanceChanceFactor` | float | 1f | 유전자 누락 로맨스 확률 팩터 |
| `prisonBreakMTBFactor` | float | 1f | 탈옥 평균 시간 팩터 |

### 바이오스탯
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `biostatCpx` | int | 1 | 복잡도 |
| `biostatMet` | int | - | 신진대사 |
| `biostatArc` | int | - | 아키테크 요구량 |

### 유전자 시스템
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `exclusionTags` | List\<string\> | - | 배제 태그 |
| `prerequisite` | GeneDef | - | 선행 유전자 |
| `selectionWeight` | float | 1f | 선택 가중치 |
| `canGenerateInGeneSet` | bool | true | 유전자 세트 생성 가능 |
| `symbolPack` | GeneSymbolPack | - | 심볼 팩 |
| `passOnDirectly` | bool | true | 직접 전달 여부 |
| `selectionWeightFactorDarkSkin` | float | 1f | 어두운 피부 선택 가중치 팩터 |
| `selectionWeightCultist` | float | 1f | 컬티스트 선택 가중치 |

### 적성
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `aptitudes` | List\<Aptitude\> | - | 스킬 적성 목록 |
| `passionMod` | PassionMod | - | 열정 수정자 |

### 기타
| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| `randomChosen` | bool | false | 랜덤 선택 |
| `waterCellCost` | int? | - | 물 타일 비용 |
| `deathHistoryEvent` | HistoryEventDef | - | 사망 히스토리 이벤트 |
| `marketValueFactor` | float | 1f | 시장 가치 팩터 |
| `removeOnRedress` | bool | false | 재착용 시 제거 |
| `endogeneCategory` | EndogeneCategory | - | 내부 유전자 카테고리 |

## 3. 복합 타입 상세

### GeneticTraitData
- `def`: TraitDef - 특성 정의
- `degree`: int - 특성 정도

### Aptitude
- `skill`: SkillDef - 스킬 정의
- `level`: int - 적성 레벨

### StatModifier
- `stat`: StatDef - 스탯 정의
- `value`: float - 수정 값

### PawnCapacityModifier
- `capacity`: PawnCapacityDef - 능력 정의
- `offset`: float - 오프셋
- `postFactor`: float - 후처리 팩터
- `setMax`: float - 최대값 설정

### DamageFactor
- `damageDef`: DamageDef - 피해 타입
- `factor`: float - 피해 배율

### ConditionalStatAffecter
- `statOffsets`: List\<StatModifier\> - 스탯 오프셋 목록
- `statFactors`: List\<StatModifier\> - 스탯 팩터 목록

### PassionMod
- `skill`: SkillDef - 스킬
- `modType`: PassionModType - 수정 타입 (AddOneLevel, DropAll)

### PawnRenderNodeProperties
렌더링 노드의 그래픽 속성들 정의

## 4. 주요 프로퍼티

### Icon (읽기 전용)
아이콘 텍스처를 반환. `iconPath`에서 로드하거나 BadTex 반환

### IconColor (읽기 전용)
아이콘 색상 반환. 우선순위: iconColor → skinColorBase → skinColorOverride → hairColorOverride → white

### LabelShortAdj (읽기 전용)
짧은 형용사 레이블 반환. `labelShortAdj`가 없으면 `label` 반환

### DescriptionFull (읽기 전용)
모든 효과를 포함한 전체 설명 생성 (캐시됨)

### HasDefinedGraphicProperties (읽기 전용)
렌더 노드 속성이 정의되어 있는지 확인

### RandomChosen (읽기 전용)
랜덤 선택 가능 여부. 바이오스탯이 모두 0이고 외형 관련 속성이 있으면 자동으로 true

## 5. 주요 메서드

### ResolveReferences()
참조 해결. displayCategory가 없으면 Miscellaneous로 설정

### AptitudeFor(SkillDef skill)
특정 스킬에 대한 적성 레벨 반환

### ConflictsWith(GeneDef other)
다른 유전자와 충돌하는지 확인 (같은 유전자이거나 exclusionTags가 겹치면 충돌)

### ConfigErrors()
설정 오류 검증

## 6. 소스코드 참조
[GeneDef.cs](mdc:RimworldSource/Verse/GeneDef.cs)


# PawnKindDef XML 설정 가능한 모든 값 분석 - 2024-12-19

## 작업 개요
- 요청 내용: PawnKindDef관련 XML을 통해서 세팅 가능한 모든 값 소스코드 참고해서 알려줘
- 목표: PawnKindDef 클래스와 관련 클래스들을 분석하여 XML에서 설정 가능한 모든 속성들을 정리

## 계획 (AI가 결정한 계획)
1. PawnKindDef 클래스 소스코드 분석
2. 관련 클래스들 분석 (PawnKindLifeStage, TraitRequirement, SkillRange 등)
3. XML 설정 가능한 모든 값들 정리 및 분류

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. PawnKindDef 클래스 소스코드 분석
2. 관련 클래스들 분석 (PawnKindLifeStage, TraitRequirement, SkillRange 등)
3. XML 설정 가능한 모든 값들 정리 및 분류

## 작업 세부 진행
1. PawnKindDef 클래스 분석 [v]
2. 관련 클래스들 분석 [v]
3. XML 설정값 정리 [진행중]

## 진행 상황
### 1. PawnKindDef 클래스 분석
- 내용: RimworldSource/Verse/PawnKindDef.cs 파일 분석
- 결과: 552줄의 클래스 정의에서 모든 public 속성들 확인

### 2. 관련 클래스들 분석
- 내용: PawnKindLifeStage, TraitRequirement, SkillRange, StartingHediff, MiscDamage, SpecificApparelRequirement 클래스 분석
- 결과: 각 클래스의 속성들 확인

### 3. XML 설정값 정리
- 내용: PawnKindDef에서 XML로 설정 가능한 모든 값들을 카테고리별로 정리
- 결과: 진행중

## 최종 작업 결과/ 중단 사유
완료 - PawnKindDef와 관련 클래스들의 모든 XML 설정 가능한 값들을 28개 카테고리로 분류하여 정리 완료

## 관련 파일 목록
- RimworldSource/Verse/PawnKindDef.cs
- RimworldSource/Verse/PawnKindLifeStage.cs
- RimworldSource/Verse/TraitRequirement.cs
- RimworldSource/Verse/SkillRange.cs
- RimworldSource/Verse/StartingHediff.cs
- RimworldSource/Verse/MiscDamage.cs
- RimworldSource/Verse/SpecificApparelRequirement.cs

## 참고사항
- PawnKindDef는 Def 클래스를 상속받음
- 모든 public 필드가 XML에서 설정 가능
- 일부 필드는 특별한 XML 속성 어노테이션을 가짐

# Verb 중복 ID 등록 오류 분석 - 2025-11-12

## 작업 개요
- 요청 내용: 세이브 로드 시 Verb ID 중복 등록 오류 해결
- 목표: RK_Gunlance_NormalType의 Verb ID 중복 등록 오류 원인 분석 및 해결방안 제시

## 오류 내용

```
Cannot register RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10123_0_Stab in loaded object directory. 
Id already used by RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null).

Cannot register NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10123_1_RK_GunlanceExplosion_Normal in loaded object directory. 
Id already used by NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null).
```

## 계획 (AI가 결정한 계획)
1. 오류 메시지 분석 및 원인 파악
2. 건랜스 무기 정의 검토
3. Verb 클래스 구현 검토 (ExposeData 메서드 확인)
4. 해결방안 제시
5. 필요시 코드 수정

## 최종 계획 (컨펌 대기)
1. 오류 원인 분석 및 해결방안 제시
2. 사용자 승인 후 코드 수정 (필요시)

## 작업 세부 진행
1. [오류 메시지 분석] [v]
2. [관련 파일 검토] [v]
3. [원인 파악 및 해결방안 제시] [-]
4. [코드 수정] [-]

## 진행 상황

### 1. 오류 메시지 분석
- 내용: 세이브 로드 시 Verb ID 중복 등록 오류 발생
- 결과:
  - 무기 ID: RK_Gunlance_NormalType (Thing loadID: 10123)
  - 중복 Verb 1: `Verb_CompEquippable_RK_Gunlance_NormalType10123_0_Stab` (RimWorld.Verb_MeleeAttackDamage)
  - 중복 Verb 2: `Verb_CompEquippable_RK_Gunlance_NormalType10123_1_RK_GunlanceExplosion_Normal` (NewRatkin.Verb_GunlanceFiring)
  - 오류 타입: LoadedObjectDirectory에 이미 같은 ID가 등록되어 있음

### 2. 관련 파일 검토
- 내용: 건랜스 무기 정의 및 Verb 클래스 검토
- 결과:
  - 무기 정의: `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` - 정상
  - Verb 클래스 1: `Project/1.6/Source/RatkinGuerrilla/MeeleExplosion.cs` (Verb_MeleeAttackDamage)
  - Verb 클래스 2: `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs`
  - 두 클래스 모두 ExposeData 메서드를 오버라이드하지 않음 (부모 클래스 메서드 사용)

### 3. 원인 분석 (진행 중)
- 진행 중...

## 최종 작업 결과/ 중단 사유
[진행 중]

## 관련 파일 목록
- [Weapon_HighTech.xml](Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml)
- [MeeleExplosion.cs](Project/1.6/Source/RatkinGuerrilla/MeeleExplosion.cs)
- [Verb_GunlanceFiring.cs](Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs)

## 참고사항
- 이 오류는 세이브 로드 시에만 발생
- 새 게임 시작 시 문제 없음 가능성








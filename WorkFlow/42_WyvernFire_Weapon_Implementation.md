# Wyvern Fire 무기 구현 - 2025-11-10

## 작업 개요
- 요청 내용: 드래곤 브레스 같은 무기 제작 (HellcatBurner 참고)
- 목표: RK_Ammo_WyvernFire 탄약을 사용하는 원뿔형 공격 무기 구현

## 요구사항
1. **탄약 및 충전**
   - 탄약: RK_Ammo_WyvernFire (1발당 1회 충전)
   - 최대 충전: 2발
   - 재사용 대기시간: 2시간 (게임 내 시간 = 120000 ticks)

2. **공격 방식**
   - 불 대신 폭발만 일으킴
   - DamageDef 지정 가능 (Flame 또는 Bomb)
   - 원뿔 길이와 각도를 조절 가능

3. **기타**
   - 이펙트 및 사운드는 HellcatBurner와 동일하게 유지
   - 기존 건랜스와는 별도의 독립 무기

## 계획 (AI가 결정한 계획)

### 1단계: HellcatBurner 구조 분석 완료
- AbilityDef 기반 구조
- CompProperties_EquippableAbilityReloadable 사용
- CompProperties_AbilityFireSpew (공격 효과)
- CompProperties_AbilityBurner (시각 효과)

### 2단계: RK_WyvernFire_Gun ThingDef 생성
- BaseHumanMakeableGun 또는 적절한 Parent 선택
- CompProperties_EquippableAbilityReloadable 설정
  - maxCharges: 2
  - ammoDef: RK_Ammo_WyvernFire
  - ammoCountPerCharge: 1
  - cooldownTicks: 120000 (2시간)

### 3단계: RK_WyvernFire_Ability AbilityDef 생성
- verbProperties 설정 (range, warmupTime 등)
- CompProperties_AbilityFireSpew 설정
  - damAmount: 조절 가능
  - range: 조절 가능
- CompProperties_AbilityBurner 설정
  - coneSizeDegrees: 각도 조절 가능
  - range: 길이 조절 가능

### 4단계: DamageDef 파라미터화
- Bomb 또는 Flame 선택 가능하도록 설정
- CompProperties_AbilityFireSpew의 damAmount 및 기타 설정

### 5단계: 테스트 및 조정
- 게임 내 로드 확인
- 탄약 소비 및 재장전 확인
- 원뿔 공격 범위 및 각도 확인
- DamageDef 적용 확인

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. HellcatBurner 기준으로 구조 파악
2. ThingDef 생성 (무기) - 원뿔 각도 7.2도, 길이 9.9
3. AbilityDef 생성 - 데미지 타입 Bomb, XML에서 변경 가능
4. 무기 이름: "Wyvern Fire" 또는 "Dragon's Breath" (임시)
5. 테스트 및 검증

## 작업 세부 진행
1. HellcatBurner 전체 구조 분석 [v]
2. 커스텀 Comp 작성 (CompProperties_AbilityWyvernFire, CompAbilityEffect_WyvernFire) [v]
3. C# 프로젝트 빌드 [v]
4. ThingDef (무기) 생성 [v]
5. AbilityDef 생성 [v]
6. XML 파일에 추가 [v]
7. 로드 테스트 [ ]

## 진행 상황
### 1. HellcatBurner 구조 분석
- CompAbilityEffect_FireSpew는 Flame 데미지만 가능 (하드코딩)
- CompAbilityEffect_Explosion은 damageDef 설정 가능하지만 단일 지점 폭발
- 결론: 원뿔형 범위 + 데미지 타입 선택을 위해 커스텀 Comp 필요

### 2. 커스텀 Comp 작성
- `CompProperties_AbilityWyvernFire.cs` 생성
  - FireSpew의 range, lineWidthEnd 포함
  - damageDef, damAmount, armorPenetration 파라미터 추가
- `CompAbilityEffect_WyvernFire.cs` 생성
  - FireSpew의 원뿔 계산 로직 복사
  - 하드코딩된 Flame 대신 설정 가능한 damageDef 사용
- 위치: `Project/1.6/Source/WyvernFire/`

### 3. C# 프로젝트 빌드
- 빌드 성공 (0 Warning, 0 Error)
- DLL 생성: `Project/1.6/Assemblies/NewRatkin.dll`

### 4. 기존 건랜스에 어빌리티 추가
- RK_Gunlance_NormalType에 CompProperties_EquippableAbilityReloadable 추가
- 탄약: RK_Ammo_WyvernFire (1개당 1충전)
- 최대 충전: 2발
- 설명 업데이트: 용격포(Wyvern Fire) 기능 설명 추가

### 5. AbilityDef 생성 (RK_WyvernFire_Ability)
- cooldownTicksRange: 120000 (2시간)
- range: 9.9 (원뿔 길이)
- coneSizeDegrees: 7.2 (원뿔 각도)
- damageDef: Bomb (폭발 데미지)
- damAmount: 20
- 시각 효과: CompProperties_AbilityBurner 사용 (HellcatBurner와 동일)

### 6. XML 파일 추가
- 파일: `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml`
- RK_Gunlance_NormalType에 CompProperties_EquippableAbilityReloadable 추가
- RK_WyvernFire_Ability AbilityDef 추가

## 최종 작업 결과
### 완료 사항
- ✅ 커스텀 Comp 작성 완료 (CompProperties_AbilityWyvernFire, CompAbilityEffect_WyvernFire)
- ✅ C# 프로젝트 빌드 성공
- ✅ ThingDef 및 AbilityDef 작성 완료
- ✅ XML 파일에 추가 완료

### 구현 내용
1. **무기 (RK_Gunlance_NormalType에 추가)**
   - 기존 건랜스에 Wyvern Fire 어빌리티 추가
   - 드래곤 브레스 스타일의 원뿔형 폭발 공격
   - 2발 충전 가능 (RK_Ammo_WyvernFire 1개당 1충전)
   - 각 발 사용 후 2시간 쿨다운 (120000 ticks)
   
2. **Ability (RK_WyvernFire_Ability)**
   - 원뿔 길이: 9.9 (range)
   - 원뿔 각도: 7.2도 (coneSizeDegrees)
   - 데미지 타입: Bomb (XML에서 Flame으로 변경 가능)
   - 데미지: 20 (damAmount)
   - 관통력: 0.5 (armorPenetration)
   
3. **커스텀 Comp**
   - FireSpew의 원뿔 계산 로직 사용
   - damageDef를 파라미터로 받아 Bomb 또는 Flame 선택 가능
   - 이펙트는 HellcatBurner와 동일 (CompProperties_AbilityBurner)

### 변경 사항 (사용자 요청)
- **초기 계획**: 새로운 원거리 무기 생성
- **최종 구현**: 기존 건랜스(RK_Gunlance_NormalType)에 Wyvern Fire 어빌리티 추가
- 근접 공격 + 포격 + 용격포(Wyvern Fire) 3가지 기능 통합

### 빌드 오류 해결
- **.csproj 파일에 WyvernFire 폴더 추가**: 새 파일들이 빌드에 포함되지 않았음
- **using RimWorld 추가**: CompProperties_AbilityEffect는 RimWorld 네임스페이스에 있음
- **최종 빌드 성공**: NewRatkin.dll에 커스텀 Comp 포함 완료

### 다음 단계
- ✅ DLL 빌드 완료
- 게임 내 로드 테스트 필요
- 필요시 데미지, 범위, 각도 조정 가능

## 관련 파일 목록
### C# 소스
- [CompProperties_AbilityWyvernFire.cs](Project/1.6/Source/WyvernFire/CompProperties_AbilityWyvernFire.cs)
- [CompAbilityEffect_WyvernFire.cs](Project/1.6/Source/WyvernFire/CompAbilityEffect_WyvernFire.cs)

### XML 정의
- [Weapon_HighTech.xml](Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml) - RK_Gunlance_NormalType에 어빌리티 추가, RK_WyvernFire_Ability 정의

### 참고 파일
- [HellcatBurner](RimWorldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml)
- [CompAbilityEffect_FireSpew.cs](RimworldSource/RimWorld/CompAbilityEffect_FireSpew.cs)

## 참고사항
- HellcatBurner는 maxCharges: 2, ammoDef: Bioferrite (10개당 1충전)
- Wyvern Fire는 maxCharges: 2, ammoDef: RK_Ammo_WyvernFire (1개당 1충전)
- 재사용 대기시간: 2시간 = 120000 ticks (1시간 = 60000 ticks)
- CompProperties_AbilityFireSpew의 damAmount로 데미지 조절
- coneSizeDegrees로 각도 조절 (HellcatBurner는 7.2도)


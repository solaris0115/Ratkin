# Bolter Verb Stats Update - 2024-12-19

## 작업 개요
- 요청 내용: Weapon_Range.xml (825-838) 라인을 아예 권총 기준으로 변경
- 목표: RK_Weapon_Bolter의 전체 설정을 림월드 Revolver 권총 기준으로 변경

## 계획 (AI가 결정한 계획)
1. 림월드 기본 권총(Revolver) 데이터 확인
2. 현재 Bolter의 전체 설정 확인
3. 권총 기준으로 전체 설정 변경:
   - statBases: Accuracy, Cooldown, Mass 변경
   - weaponTags: TwoHanded → OneHanded 변경
   - weaponClasses: RangedLight 추가
   - verbs: soundCastTail 변경 (GunTail_Heavy → GunTail_Light)
   - soundInteract: Interact_Rifle → Interact_Revolver
   - tools: stock → grip 변경, power 조정
4. 변경 사항 적용

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. [승인 대기 중]

## 작업 세부 진행
1. 림월드 권총 데이터 확인 [v]
2. 현재 설정 확인 [v]
3. 전체 설정 권총 기준으로 변경 [v]

## 진행 상황
### 1. 림월드 권총 데이터 확인
- 내용: RimWorldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml 확인
- 결과: Revolver 권총 기준값 확인
  - statBases: Mass 1.4, AccuracyTouch 0.80, AccuracyShort 0.75, AccuracyMedium 0.55, AccuracyLong 0.40, Cooldown 1.6
  - warmupTime: 0.3
  - range: 25.9
  - weaponClasses: RangedLight
  - weaponTags: SimpleGun, Revolver
  - soundInteract: Interact_Revolver
  - soundCastTail: GunTail_Light
  - tools: grip (Blunt, power 9), barrel (Blunt, Poke, power 9)

### 2. 현재 설정 확인
- 내용: Weapon_Range.xml 802-877 라인 확인
- 결과: 현재 설정값 확인
  - statBases: Mass 3.5, Accuracy 높음, Cooldown 1.5
  - weaponTags: TwoHanded 관련 태그
  - soundInteract: Interact_Rifle
  - soundCastTail: GunTail_Heavy
  - tools: stock (power 7), barrel (power 7)

### 3. 전체 설정 권총 기준으로 변경
- 내용: Revolver 권총 기준으로 전체 무기 설정 변경
- 변경 사항:
  - **statBases**:
    - Mass: 3.5 → 1.4
    - AccuracyTouch: 0.75 → 0.80
    - AccuracyShort: 0.85 → 0.75
    - AccuracyMedium: 0.65 → 0.55
    - AccuracyLong: 0.45 → 0.40
    - RangedWeapon_Cooldown: 1.5 → 1.6
  - **soundInteract**: Interact_Rifle → Interact_Revolver
  - **verbs**:
    - soundCastTail: GunTail_Heavy → GunTail_Light
  - **weaponClasses**: RangedLight 추가
  - **weaponTags**: 
    - TwoHanded 관련 태그 제거
    - OneHanded 관련 태그 추가 (RK_WeaponTag_OneHanded, RK_WeaponTag_OneHandedRange, RK_WeaponTag_OneHandedRangeAttack)
  - **tools**:
    - stock → grip으로 변경
    - power: 7 → 9 (권총 기준)
- 결과: 수정 완료, 린터 오류 없음

## 최종 작업 결과/ 중단 사유
완료: RK_Weapon_Bolter의 전체 설정을 림월드 Revolver 권총 기준으로 변경 완료. 이제 권총 스타일의 한손 무기로 동작합니다.

## 관련 파일 목록
- `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`

## 참고사항
- 림월드 Revolver 권총 기준: warmupTime 0.3, range 25.9, 단발 사격


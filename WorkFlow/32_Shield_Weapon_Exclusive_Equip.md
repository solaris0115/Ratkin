# 방패와 특정 무기 동시 착용 방지 구현 - 2025-01-XX

## 작업 개요
- 요청 내용: 방패와 특정 Weapon을 동시 착용을 막으려고 함. 가능하면 Harmony patch 없이 구현. 아이템 우클릭 시 착용 가능/불가능 상태 표시. 아예 처음부터 동시 착용을 막기.
- 목표: Harmony patch 없이 방패(Apparel)와 특정 무기(Weapon)의 동시 착용을 방지하고, 우클릭 메뉴에서 착용 옵션을 비활성화

## 계획 (AI가 결정한 계획)
1. RimWorld 착용 검사 로직 조사 및 분석
2. 방패 클래스에 PawnCanWear 오버라이드 구현
3. 무기 착용 검사 로직 구현 (방패 착용 시 특정 무기 착용 불가)
4. 테스트 및 검증

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
[사용자 승인 대기]

## 작업 세부 진행
1. RimWorld 착용 검사 로직 조사 [v]
2. 작업 계획서 작성 및 사용자 승인 대기 [진행중]
3. 방패 클래스에 PawnCanWear 오버라이드 구현 [ ]
4. 무기 착용 검사 로직 구현 [ ]
5. 테스트 및 검증 [ ]

## 진행 상황

### 1. RimWorld 착용 검사 로직 조사
- 내용: RimWorld의 Apparel과 Weapon 착용 검사 메커니즘 분석
- 결과: 
  - **Apparel 착용 검사**:
    - `ApparelUtility.CanWearTogether()`: 두 Apparel이 동시 착용 가능한지 확인 (레이어와 bodyPartGroups 기반)
    - `Apparel.PawnCanWear()`: Pawn이 Apparel을 착용할 수 있는지 확인 (virtual 메서드, 오버라이드 가능)
    - `FloatMenuOptionProvider_Wear`: 우클릭 메뉴 생성, `EquipmentUtility.CanEquip()` 호출
  
  - **Weapon 착용 검사**:
    - `EquipmentUtility.CanEquip()`: 무기 착용 가능 여부 확인 (생체 인증, 역할 제한 등)
    - `FloatMenuOptionProvider_Equip`: 우클릭 메뉴 생성, `EquipmentUtility.CanEquip()` 호출
  
  - **핵심 발견**:
    - `Apparel.PawnCanWear()`는 virtual 메서드이므로 오버라이드 가능
    - `EquipmentUtility.CanEquip()`는 static 메서드이므로 직접 오버라이드 불가
    - `FloatMenuOptionProvider_Wear`와 `FloatMenuOptionProvider_Equip`는 각각 Apparel과 Weapon의 우클릭 메뉴를 생성

### 2. 해결 방법 분석

#### 방법 1: Apparel.PawnCanWear() 오버라이드 (권장)
- **장점**: 
  - Harmony patch 불필요
  - 방패 착용 시점에 무기 확인 가능
  - 우클릭 메뉴에서 자동으로 비활성화됨
- **단점**: 
  - 무기 착용 시점에는 방패 확인 불가 (반대 방향 검사 필요)
- **구현**: `Shield` 클래스에서 `PawnCanWear()` 오버라이드하여 현재 착용 중인 무기 확인

#### 방법 2: EquipmentUtility.CanEquip() 확장
- **장점**: 무기 착용 시점에 방패 확인 가능
- **단점**: 
  - static 메서드라 직접 오버라이드 불가
  - Harmony patch 필요 (사용자 요구사항과 충돌)

#### 방법 3: FloatMenuOptionProvider 커스텀
- **장점**: 우클릭 메뉴에서 완전한 제어 가능
- **단점**: 
  - 복잡한 구현 필요
  - 기존 Provider와 충돌 가능성

#### 최종 선택: 방법 1 + 방법 2 (하이브리드)
- **방패 착용 시**: `Shield.PawnCanWear()` 오버라이드로 무기 확인
- **무기 착용 시**: `EquipmentUtility.CanEquip()`를 확장하는 Comp 추가 (Harmony 없이)

### 3. 구현 계획

#### 3.1 방패 클래스 수정 (`ApparelShield.cs`)
- `PawnCanWear()` 메서드 오버라이드
- 현재 Pawn이 착용 중인 무기 확인
- 특정 무기(defName 또는 weaponTag)와 충돌하는지 확인
- 충돌 시 false 반환

#### 3.2 무기 착용 검사 (방패 착용 시 무기 착용 불가)
- **옵션 A**: `EquipmentUtility.CanEquip()`를 확장하는 Comp 추가
  - `CompProperties_Equippable` 확장
  - `CompEquippable`에서 착용 전 검사
- **옵션 B**: 무기 ThingDef에 Comp 추가하여 방패 확인
  - `CompProperties_ShieldIncompatible` 같은 Comp 생성
  - `CompEquippable`의 `PostEquip` 또는 `PreEquip`에서 검사

#### 3.3 특정 무기 지정 방법
- **방법 1**: XML에서 weaponTag 사용
- **방법 2**: XML에서 defName 리스트 사용
- **방법 3**: CompProperties로 설정 가능하게

## 최종 작업 결과/ 중단 사유
[진행 중]

## 관련 파일 목록
- `RimworldSource/RimWorld/ApparelUtility.cs` - CanWearTogether 메서드
- `RimworldSource/RimWorld/Apparel.cs` - PawnCanWear 메서드
- `RimworldSource/RimWorld/EquipmentUtility.cs` - CanEquip 메서드
- `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs` - 의류 착용 메뉴
- `RimworldSource/RimWorld/FloatMenuOptionProvider_Equip.cs` - 무기 착용 메뉴
- `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs` - Shield 클래스
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml` - 방패 정의

## 참고사항
- RimWorld의 의상 동시 착용 로직은 레이어와 bodyPartGroups를 모두 확인
- `Apparel.PawnCanWear()`는 virtual 메서드이므로 오버라이드 가능
- `EquipmentUtility.CanEquip()`는 static 메서드이므로 직접 오버라이드 불가
- Harmony patch 없이 구현하려면 Apparel 클래스의 virtual 메서드를 활용해야 함


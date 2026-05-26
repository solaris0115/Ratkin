# RK_TowerShield_Second (v2 방패) 구현 현황 보고서

> **태그**: RK_TowerShield_Second v2 방패 구현현황 Status Shield Deflect FaceDirection Stuff

## 개요

| 항목 | 값 |
|------|-----|
| DefName | `RK_TowerShield_Second` |
| thingClass | `NewRatkin.ApparelShieldTowerSecond` |
| 부모 Def | `RK_ApparelAttr_ShieldBase` |
| 정의 파일 | `Project/1.6/Defs/ThingsDefs/Apparel_Shield.xml` |

---

## 구현 완료 기능

### 1. 이진 도탄 (Deflect)

- 소집 상태에서만 작동
- **근접 + 원거리 공격 모두** 차단 대상
- 근접/원거리 별도 각도 판정:
  - **근접**: 바라보는 방향 기준 **±45°** (전방 90°)
  - **원거리**: 바라보는 방향 기준 **±70°** (전방 140°)
- 조건 충족 시 **100% 완전 차단** (이진 판정: 차단 or 통과)
- 차단 불가 조건: 기절, 화염, 정신붕괴, 사망, 쓰러짐
- `ignoreShields` 또는 EMP 공격은 관통
- 차단 시 "ShieldBlock" 텍스트 + 금속 튕김 이펙트 출력

### 2. 방향 고정 커맨드

- 소집 시 전용 기즈모 버튼 표시
- 지점/폰/건물 타겟팅 → 해당 방향으로 회전 고정
- 쿨다운 300틱 (5초)
- 다중 폰 선택 시 일괄 명령 지원

### 3. 방향 고정 잡 (Job)

- 대상 방향을 지속적으로 응시
- 근접 피해 수신 시 → 즉시 Job 해제 (반격 가능)
- 근접 위협(meleeThreat) 감지 시 → 즉시 해제
- 소집 해제 시 → 자동 종료

### 4. 무기 호환성 제한

- 허용 태그: `RK_WeaponTag_OneHand` (한손 무기만)
- 비호환 무기 장착 시 자동 드롭
- 장비 화면에서 불가 사유 메시지 표시

### 5. 조건부 그래픽

- **소집 시**: 팔/어깨 위치에 방패 드로잉 (`RK_TowerShield` 텍스처)
- **비소집 시**: 등에 수납 형태 드로잉 (`RK_TowerShieldUnarm` 텍스처)
- 방향별(동서남북) 개별 오프셋·각도 설정

### 6. 소재(Stuff) 기반 제작

- 소재 카테고리: `Metallic` (금속류 선택 가능)
- 주 재료: 선택한 금속 200개 (`costStuffCount`)
- 보조 재료: Plasteel 30 + Uranium 20 (고정)
- Gold, Silver는 필터에서 제외
- `StuffEffectMultiplierArmor: 0.4` → 소재에 따라 방어력 변동

---

## Def 수치 요약

### 기본 스탯

| 스탯 | 값 |
|------|-----|
| MaxHitPoints | 300 |
| WorkToMake | 40000 |
| Mass | 10 |
| ArmorRating_Sharp (Base) | 0.10 |
| ArmorRating_Blunt (Base) | 0.15 |
| StuffEffectMultiplierArmor | 0.40 |
| Flammability | 0.2 |

### 장비 패널티

| 스탯 | 값 |
|------|-----|
| MeleeDodgeChance | -20% |
| MeleeHitChance | -20% |
| MoveSpeed | -0.8 |

### 소재별 방어력 (평범 품질)

| 소재 | Sharp | Blunt |
|------|-------|-------|
| Steel | 0.32 | 0.29 |
| Plasteel | 0.556 | 0.37 |
| Uranium | 0.372 | 0.37 |

### 도탄 파라미터

| 항목 | 값 |
|------|-----|
| meleeDeflectAngleHalf | 45° (±45° = 전방 90°) |
| rangedDeflectAngleHalf | 70° (±70° = 전방 140°) |
| deflectChance | 1.0 (100%) |
| cooldownTicks | 300 (5초) |

---

## 미구현 (계획안, Report 130 참조)

| 기능 | 설명 |
|------|------|
| 관통력 반영 | `armorPenetration` 값에 따른 차단 성공률 변동 |
| 3단계 도탄 | 완전무효 / 50%감소+Sharp→Blunt / 관통 |
| 근접 스킬 보정 | 근접 스킬에 따른 원거리방호력 계수 |
| 로그형 확률 커브 | 30%~75% 범위의 로그 기반 도탄 확률 |

---

## 관련 파일 목록

| 구분 | 경로 |
|------|------|
| Def | `Project/1.6/Defs/ThingsDefs/Apparel_Shield.xml` |
| Job Def | `Project/1.6/Defs/JobDefs/Jobs_Shield.xml` |
| C# 도탄 | `Project/1.6/Source/ShieldOfRatkinia/ApparelShieldTowerSecond.cs` |
| C# 방향 Comp | `Project/1.6/Source/ShieldFaceDirection/CompShieldFaceDirection.cs` |
| C# 방향 Props | `Project/1.6/Source/ShieldFaceDirection/CompProperties_ShieldFaceDirection.cs` |
| C# 방향 Job | `Project/1.6/Source/ShieldFaceDirection/JobDriver_ShieldFaceDirection.cs` |
| C# 무기제한 | `Project/1.6/Source/StaminaShield/CompShieldWeaponIncompatible.cs` |
| 이전 보고서 | `./130_RK_TowerShield_v2_Armor_Deflection_Formula_Report.md` |

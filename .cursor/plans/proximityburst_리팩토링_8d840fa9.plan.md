---
name: ProximityBurst 리팩토링
overview: Properties 클래스 분리, 미사용 필드 정리, 하드코딩 제거, 불필요 인스턴스 필드 로컬화 등 Projectile_ProximityBurst 전반 리팩토링
todos:
  - id: extract-properties
    content: ProjectileProperties_ProximityBurst를 별도 파일로 분리 + 미사용 필드 교체 + 하드코딩 필드 확장
    status: completed
  - id: refactor-projectile
    content: "Projectile_ProximityBurst 정리: wasBlockedByShield 제거(파라미터 직접 사용), ShieldZone.parent 제거, 새 Properties 필드 활용"
    status: completed
  - id: build-verify
    content: 빌드 검증
    status: completed
isProject: false
---

# ProjectileProperties_ProximityBurst 리팩토링 계획

## 현황 분석

대상 파일: [Projectile_ProximityBurst.cs](Project/1.6/Source/BFR/Projectile_ProximityBurst.cs) (414줄, 클래스 2개 포함)

Def 사용처 (2곳):

- `RK_Bullet_SawedOff` (Weapon_Range.xml:783) -- sectorAngle=140, sectorRadius=2.7, preDet=0
- `Bullet_BFR_HE` (Weapon_Range.xml:1175) -- sectorAngle=120, sectorRadius=3.7, preDet=2

## 발견된 문제점

1. **미사용 선언 (타입 불일치)**: `sectorCellEffecterDef` (EffecterDef)가 Properties에 선언되어 있으나, 실제 `SpawnSectorCellEffects`에서는 `FleckDefOf.ShotHit_Dirt` (FleckDef)를 하드코딩. Def XML에서도 사용 이력 없음 (Grep 0건)
2. **미사용 필드**: `ShieldZone.parent`를 `GetHostileShieldZones`에서 설정(line 264)하지만 어디서도 읽지 않음
3. **불필요 인스턴스 필드**: `wasBlockedByShield`는 `Impact(blockedByShield)` 파라미터를 그대로 복사한 것. 파라미터를 직접 사용하면 됨
4. **하드코딩된 이펙트 설정**:
  - FleckDef 이름 `"RK_WyvernFireExplosion"` (line 310)
  - 색상 `(0.75f, 0.55f, 0.55f, 0.7f)` (line 318)
  - 스케일 `(3f, 1f, 2f)` (line 316)
  - 셀당 이펙트 수 `2`, 노이즈 범위 `0.3f` (lines 325-326)
5. **2개 클래스 한 파일**: Properties 데이터 클래스와 Projectile 로직 클래스가 혼재

## 리팩토링 계획

### 1. Properties 클래스 분리 및 확장

`ProjectileProperties_ProximityBurst`를 별도 파일 `BFR/ProjectileProperties_ProximityBurst.cs`로 이동하고, 미사용 필드를 교체하며 하드코딩 값을 Def 설정 가능하게 확장:

```csharp
public class ProjectileProperties_ProximityBurst : ProjectileProperties
{
    // 기존 필드 (유지)
    public float preDetonationDistance = 0f;
    public float sectorAngle = 90f;
    public float sectorRadius = 1.7f;
    public float wallBreachRadius = 1.7f;
    public int damageAmountDirect = 40;
    public int damageAmountExplosion = 25;
    public DamageDef damageDefDirect;
    public DamageDef damageDefExplosion;
    public float armorPenetrationDirect = -1f;
    public float armorPenetrationExplosion = -1f;

    // 변경: sectorCellEffecterDef (EffecterDef, 미사용) 제거 -> 실제 타입에 맞게 교체
    public FleckDef sectorCellFleckDef;          // null이면 FleckDefOf.ShotHit_Dirt 폴백
    public int sectorEffectsPerCell = 2;
    public float sectorEffectNoiseRange = 0.3f;

    // 추가: 중심 폭발 이펙트 설정
    public FleckDef centerExplosionFleckDef;     // null이면 "RK_WyvernFireExplosion" 폴백
    public float centerExplosionVisualRadius = 1f;
}
```

핵심 변경:

- `sectorCellEffecterDef` (EffecterDef) -> `sectorCellFleckDef` (FleckDef)로 교체 -- 타입 불일치 해소
- XML Def에서 `sectorCellEffecterDef`를 사용한 적이 없으므로 하위 호환 문제 없음

### 2. Projectile 클래스 정리

- `wasBlockedByShield` 인스턴스 필드 삭제 -> `Impact()` 내 `blockedByShield` 파라미터 직접 사용
- `ShieldZone.parent` 필드 제거 + `GetHostileShieldZones`에서 해당 할당 제거
- `SpawnSectorCellEffects`: props에서 FleckDef와 이펙트 설정 참조
- `SpawnCenterExplosionVisual`: props에서 FleckDef 참조 (null이면 기존 `GetNamedSilentFail` 폴백)

### 3. XML Def 변경 없음

- 새 필드들은 모두 기본값이 기존 하드코딩 값과 동일하므로 XML 수정 불필요
- 제거되는 `sectorCellEffecterDef`는 어떤 Def에서도 사용된 적 없음

## 파일 변경 요약

- **신규**: `BFR/ProjectileProperties_ProximityBurst.cs` (Properties 클래스 분리)
- **수정**: `BFR/Projectile_ProximityBurst.cs` (Properties 클래스 제거 + 로직 정리)


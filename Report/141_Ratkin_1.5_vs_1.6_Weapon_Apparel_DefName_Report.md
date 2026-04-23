# Ratkin 1.5 vs 1.6 무기·의류 ThingDef `defName` 비교 보고서

**작성 기준일:** 2026-04-23  
**대상:** 모드 내 `Project/1.5/Defs` ↔ `Project/1.6/Defs` XML만 (바닐라 림월드 Def 미포함)

---

## 1. 요약

| 구분 | 1.5 | 1.6 |
|------|-----|-----|
| 무기 `defName` 수 | 28 | 30 |
| 의류 `defName` 수 | 47 | 52 |
| 1.6에만 존재 (집합 차이) — 무기 | — | 11 |
| 1.6에만 존재 (집합 차이) — 의류 | — | 8 |
| 1.5에만 존재 (1.6 Defs에 없음) — 무기 | 9 | — |
| 1.5에만 존재 — 의류 | 3 | — |

---

## 2. 분석 범위·방법

- **스캔 경로:** `Project/1.5/Defs/**/*.xml`, `Project/1.6/Defs/**/*.xml` 루트가 `<Defs>`인 파일의 `<ThingDef>`만 대상.
- **의류:** 노드에 `<apparel>`이 있거나, `ParentName`에 의류·방패·배낭·깃발 베이스 등이 포함된 경우(림월드 부모 병합을 XML 단독으로 재구성한 근사).
- **무기:** 위 의류·제외 규칙에 걸리지 않고, `weaponTags` 또는 비어 있지 않은 `<verbs>`, 또는 아이템 근접용 `<tools>`가 있는 `ThingDef`.
- **제외:** `Abstract="True"`, `defName` 없음, `category`가 `Projectile`·`Building`·`Pawn` 등, `thingClass`가 `Bullet`, `<ingestible>`(음식·약물), 포탄/볼트 베이스, `AnimalThingBase` 계열 동물, 모르타 포탄류(`MortarShell` 등).

재현 스크립트: `TempData/diff_weapon_apparel_15_16.py` (실행 시 `TempData/weapon_apparel_15_16_report.md`도 갱신됨)

---

## 3. 1.5 기준 전체 목록

### 3.1 무기 (28)

| # | defName |
|---|---------|
| 1 | `RK_AutoCrossBow` |
| 2 | `RK_Axe` |
| 3 | `RK_BFR` |
| 4 | `RK_Cleaver` |
| 5 | `RK_Crossbow` |
| 6 | `RK_Dagger` |
| 7 | `RK_EnhanceCrossBow` |
| 8 | `RK_FlechetteRifle` |
| 9 | `RK_FlechetteSniperRifle` |
| 10 | `RK_Fork` |
| 11 | `RK_Gunlance_NormalType` |
| 12 | `RK_Gunlance_SpreadType` |
| 13 | `RK_Halberd` |
| 14 | `RK_HeavyLance` |
| 15 | `RK_Hockey` |
| 16 | `RK_LightLance` |
| 17 | `RK_LongSword` |
| 18 | `RK_Mace` |
| 19 | `RK_MagicWand` |
| 20 | `RK_OneHanded` |
| 21 | `RK_PrototypePulseRifle` |
| 22 | `RK_Rifle` |
| 23 | `RK_Rifle_line` |
| 24 | `RK_SniperRifle` |
| 25 | `RK_Spear` |
| 26 | `RK_Turret_Ballista_Strait` |
| 27 | `RK_Turret_Cannon_Strait` |
| 28 | `RK_TwoHanded` |

### 3.2 의류 (47)

| # | defName |
|---|---------|
| 1 | `RK_ApronSkirt` |
| 2 | `RK_Backpack` |
| 3 | `RK_BattleSuit` |
| 4 | `RK_BulletProofHelmet` |
| 5 | `RK_Cardigan` |
| 6 | `RK_ChefHat` |
| 7 | `RK_ChefSuit` |
| 8 | `RK_Coif` |
| 9 | `RK_CrossBack` |
| 10 | `RK_EarCostume` |
| 11 | `RK_ExplorerHat` |
| 12 | `RK_ExplorerWear` |
| 13 | `RK_FlatColorCoat` |
| 14 | `RK_FrillOnepiece` |
| 15 | `RK_GaurdenUniform` |
| 16 | `RK_HairCorsage` |
| 17 | `RK_HeadBand` |
| 18 | `RK_HeavyShield` |
| 19 | `RK_HeavyShield_Big` |
| 20 | `RK_Mask` |
| 21 | `RK_MaskB` |
| 22 | `RK_Muffler` |
| 23 | `RK_OrderUniform` |
| 24 | `RK_OutdoorBackpack` |
| 25 | `RK_Plate` |
| 26 | `RK_PlateHelmA` |
| 27 | `RK_PlateHelmB` |
| 28 | `RK_PlateHelmC` |
| 29 | `RK_ResearchGlasses` |
| 30 | `RK_ResearchGown` |
| 31 | `RK_RibbonHairBand` |
| 32 | `RK_RottiHat` |
| 33 | `RK_RoyalCrown` |
| 34 | `RK_RoyalRobe` |
| 35 | `RK_Sack` |
| 36 | `RK_SantaHat` |
| 37 | `RK_SantaRobe` |
| 38 | `RK_SantaSack` |
| 39 | `RK_SistersDerss` |
| 40 | `RK_SistersVeil` |
| 41 | `RK_StrawHat` |
| 42 | `RK_SummerDress` |
| 43 | `RK_WhiteCoat` |
| 44 | `RK_WinterRobe` |
| 45 | `RK_WoodenShield` |
| 46 | `RK_WoolenHat` |
| 47 | `RK_WorkerWear` |

---

## 4. 1.6 기준 전체 목록 (같은 분류 규칙)

### 4.1 무기 (30)

`RK_AutoCrossBow`, `RK_Axe`, `RK_Building_CannonTurret`, `RK_Cleaver`, `RK_Crossbow`, `RK_Dagger`, `RK_Fork`, `RK_Halberd`, `RK_HeavyLance`, `RK_Hockey`, `RK_LightLance`, `RK_LongSword`, `RK_Mace`, `RK_MagicWand`, `RK_OneHanded`, `RK_Pickaxe`, `RK_PrototypePulseRifle`, `RK_Rifle`, `RK_SniperRifle`, `RK_Spear`, `RK_TwoHanded`, `RK_Weapon_Arbalest`, `RK_Weapon_BFR`, `RK_Weapon_Ballista`, `RK_Weapon_Bolter`, `RK_Weapon_Gunlance`, `RK_Weapon_ProtoChainSword`, `RK_Weapon_ProtoFlameChainSword`, `RK_Weapon_RatHolicGun`, `RK_Weapon_SawedOff`

### 4.2 의류 (52)

`RK_Apparel_Banner`, `RK_Apparel_SpaceArmor`, `RK_Apparel_SpaceArmorHelmet`, `RK_Apparel_Vacsuit`, `RK_Apparel_VacsuitChildren`, `RK_Apparel_VacsuitHelmet`, `RK_ApronSkirt`, `RK_ApronSkirtChildren`, `RK_Backpack`, `RK_BattleSuit`, `RK_BulletProofHelmet`, `RK_Cardigan`, `RK_ChefHat`, `RK_ChefSuit`, `RK_Coif`, `RK_CrossBack`, `RK_ExplorerHat`, `RK_ExplorerWear`, `RK_FlatColorCoat`, `RK_FrillOnepiece`, `RK_GaurdenUniform`, `RK_HairCorsage`, `RK_HeadBand`, `RK_HeavyShield`, `RK_Mask`, `RK_MaskB`, `RK_Muffler`, `RK_OrderUniform`, `RK_OutdoorBackpack`, `RK_Plate`, `RK_PlateHelmA`, `RK_PlateHelmB`, `RK_PlateHelmC`, `RK_ResearchGlasses`, `RK_ResearchGown`, `RK_RibbonHairBand`, `RK_RoyalCrown`, `RK_RoyalRobe`, `RK_Sack`, `RK_SantaHat`, `RK_SantaRobe`, `RK_SantaSack`, `RK_SistersDerss`, `RK_SistersVeil`, `RK_StrawHat`, `RK_SummerDress`, `RK_TowerShield`, `RK_WhiteCoat`, `RK_WinterRobe`, `RK_WoodenShield`, `RK_WoolenHat`, `RK_WorkerWear`

---

## 5. 1.6에만 있는 `defName` (신규로 보이는 항목)

### 5.1 무기 (11)

- `RK_Building_CannonTurret` — 포 건물에 붙는 총 `ThingDef`(포탑용).
- `RK_Pickaxe`
- `RK_Weapon_Arbalest`
- `RK_Weapon_BFR`
- `RK_Weapon_Ballista`
- `RK_Weapon_Bolter`
- `RK_Weapon_Gunlance`
- `RK_Weapon_ProtoChainSword`
- `RK_Weapon_ProtoFlameChainSword`
- `RK_Weapon_RatHolicGun`
- `RK_Weapon_SawedOff`

### 5.2 의류 (8)

- `RK_Apparel_Banner`
- `RK_Apparel_SpaceArmor`
- `RK_Apparel_SpaceArmorHelmet`
- `RK_Apparel_Vacsuit`
- `RK_Apparel_VacsuitChildren`
- `RK_Apparel_VacsuitHelmet`
- `RK_ApronSkirtChildren`
- `RK_TowerShield`

---

## 6. 1.5에만 있고 1.6 `Defs`에 없음

번역·패치·다른 경로에 남아 있을 수 있으므로, **세이브·외부 모드 참조 시 호환 점검**용으로 보면 됨.

### 6.1 무기 (9)

- `RK_BFR`
- `RK_EnhanceCrossBow`
- `RK_FlechetteRifle`
- `RK_FlechetteSniperRifle`
- `RK_Gunlance_NormalType`
- `RK_Gunlance_SpreadType`
- `RK_Rifle_line`
- `RK_Turret_Ballista_Strait`
- `RK_Turret_Cannon_Strait`

### 6.2 의류 (3)

- `RK_EarCostume`
- `RK_HeavyShield_Big`
- `RK_RottiHat`

---

## 7. 추정 리네임·통합 (수동 대조)

| 1.5 `defName` | 비고 |
|---------------|------|
| `RK_BFR` | → `RK_Weapon_BFR` 가능성 높음 |
| `RK_Gunlance_NormalType`, `RK_Gunlance_SpreadType` | → `RK_Weapon_Gunlance` 등으로 통합 가능성 |
| `RK_Turret_Ballista_Strait` | → `RK_Weapon_Ballista` |
| `RK_Turret_Cannon_Strait` | → `RK_Building_CannonTurret` (포 `turretGunDef`) |
| `RK_HeavyShield_Big` | → `RK_TowerShield` |

`RK_EnhanceCrossBow`, `RK_FlechetteRifle`, `RK_FlechetteSniperRifle`, `RK_Rifle_line`은 1.6 Defs 기준 동일 이름이 없음 — **삭제**되었거나 **다른 `defName`으로만 존재**할 수 있음(코드·다른 XML 검색 필요).

---

## 8. 부록: 스크립트 갱신

```powershell
Set-Location d:\GitProject\Ratkin
python TempData\diff_weapon_apparel_15_16.py
```

갱신 산출물: `TempData/weapon_apparel_15_16_report.md` (본 보고서와 동일 데이터의 간단 복제)

---

*본 문서는 XML 정적 분석이며, 게임 런타임에서의 `Def` 병합·모드 로드 순서는 포함하지 않음.*

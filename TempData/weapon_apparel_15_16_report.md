# Ratkin `Project/1.5` vs `Project/1.6` — Weapon / Apparel `defName`

**정식 보고서:** `Report/141_Ratkin_1.5_vs_1.6_Weapon_Apparel_DefName_Report.md`

---

XML `ThingDef` 기준: `<apparel>` 또는 부모 `ParentName`에 의류·방패·배낭 등 토큰이 있으면 의류, `<ingestible>`(음식·약물)은 제외, 투사체·탄·포탄·동물 등은 제외, `weaponTags` / `verbs`(사격) / `tools`(아이템 근접)로 무기로 분류.

## 1.5 무기 (`defName`, 정렬)

- `RK_AutoCrossBow`
- `RK_Axe`
- `RK_BFR`
- `RK_Cleaver`
- `RK_Crossbow`
- `RK_Dagger`
- `RK_EnhanceCrossBow`
- `RK_FlechetteRifle`
- `RK_FlechetteSniperRifle`
- `RK_Fork`
- `RK_Gunlance_NormalType`
- `RK_Gunlance_SpreadType`
- `RK_Halberd`
- `RK_HeavyLance`
- `RK_Hockey`
- `RK_LightLance`
- `RK_LongSword`
- `RK_Mace`
- `RK_MagicWand`
- `RK_OneHanded`
- `RK_PrototypePulseRifle`
- `RK_Rifle`
- `RK_Rifle_line`
- `RK_SniperRifle`
- `RK_Spear`
- `RK_Turret_Ballista_Strait`
- `RK_Turret_Cannon_Strait`
- `RK_TwoHanded`

## 1.5 의류

- `RK_ApronSkirt`
- `RK_Backpack`
- `RK_BattleSuit`
- `RK_BulletProofHelmet`
- `RK_Cardigan`
- `RK_ChefHat`
- `RK_ChefSuit`
- `RK_Coif`
- `RK_CrossBack`
- `RK_EarCostume`
- `RK_ExplorerHat`
- `RK_ExplorerWear`
- `RK_FlatColorCoat`
- `RK_FrillOnepiece`
- `RK_GaurdenUniform`
- `RK_HairCorsage`
- `RK_HeadBand`
- `RK_HeavyShield`
- `RK_HeavyShield_Big`
- `RK_Mask`
- `RK_MaskB`
- `RK_Muffler`
- `RK_OrderUniform`
- `RK_OutdoorBackpack`
- `RK_Plate`
- `RK_PlateHelmA`
- `RK_PlateHelmB`
- `RK_PlateHelmC`
- `RK_ResearchGlasses`
- `RK_ResearchGown`
- `RK_RibbonHairBand`
- `RK_RottiHat`
- `RK_RoyalCrown`
- `RK_RoyalRobe`
- `RK_Sack`
- `RK_SantaHat`
- `RK_SantaRobe`
- `RK_SantaSack`
- `RK_SistersDerss`
- `RK_SistersVeil`
- `RK_StrawHat`
- `RK_SummerDress`
- `RK_WhiteCoat`
- `RK_WinterRobe`
- `RK_WoodenShield`
- `RK_WoolenHat`
- `RK_WorkerWear`

## 1.6에만 있는 `defName` (신규로 보이는 항목)

### 무기

- `RK_Building_CannonTurret`
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

### 의류

- `RK_Apparel_Banner`
- `RK_Apparel_SpaceArmor`
- `RK_Apparel_SpaceArmorHelmet`
- `RK_Apparel_Vacsuit`
- `RK_Apparel_VacsuitChildren`
- `RK_Apparel_VacsuitHelmet`
- `RK_ApronSkirtChildren`
- `RK_TowerShield`

## 1.5에만 있고 1.6 Defs에 없음 (제거·이름 변경 가능)

### 무기

- `RK_BFR`
- `RK_EnhanceCrossBow`
- `RK_FlechetteRifle`
- `RK_FlechetteSniperRifle`
- `RK_Gunlance_NormalType`
- `RK_Gunlance_SpreadType`
- `RK_Rifle_line`
- `RK_Turret_Ballista_Strait`
- `RK_Turret_Cannon_Strait`

### 의류

- `RK_EarCostume`
- `RK_HeavyShield_Big`
- `RK_RottiHat`

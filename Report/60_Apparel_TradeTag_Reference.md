# Apparel TradeTag 참조 문서

## 개요
RimWorld Core 및 모든 DLC에서 `Apparel_`로 시작하는 모든 의류 아이템의 tradeTag를 부모 상속 관계를 포함하여 정리한 문서입니다.

## 부모 클래스 tradeTag 상속 관계

### Abstract 부모 클래스
- **ApparelMakeableBase**: `Clothing` (기본 상속)
- **ArmorSmithableBase**: tradeTag 없음
- **ArmorMachineableBase**: tradeTag 없음
- **ApparelArmorPowerBase**: `HiTechArmor`, `Armor`
- **ApparelArmorReconBase**: `HiTechArmor`, `Armor`
- **PsychicApparelBase**: `PsychicApparel`, `Clothing`

---

## TradeTag별 분류

### BasicClothing
- Apparel_TribalA
- Apparel_Parka
- Apparel_Pants
- Apparel_BasicShirt
- Apparel_CollarShirt
- Apparel_Duster
- Apparel_Jacket
- Apparel_Robe
- Apparel_CowboyHat
- Apparel_BowlerHat
- Apparel_TribalHeaddress
- Apparel_Tuque
- Apparel_WarMask
- Apparel_WarVeil
- Apparel_HatHood
- Apparel_LabCoat

### Clothing
- Apparel_PlateArmor
- Apparel_FlakPants
- Apparel_FlakJacket
- Apparel_SimpleHelmet
- Apparel_PsychicFoilHelmet
- Apparel_Cape
- Apparel_Coronet
- Apparel_Crown
- Apparel_CrownStellic
- Apparel_TortureCrown
- Apparel_PsyfocusHelmet
- Apparel_EltexSkullcap
- Apparel_PsyfocusShirt
- Apparel_PsyfocusVest
- Apparel_PsyfocusRobe
- Apparel_SmokepopBelt
- Apparel_FirefoampopPack
- Apparel_PackJump

### Armor
- Apparel_FlakVest
- Apparel_AdvancedHelmet
- Apparel_PowerArmor
- Apparel_ArmorRecon
- Apparel_PowerArmorHelmet
- Apparel_ArmorHelmetRecon
- Apparel_ShieldBelt
- Apparel_MechlordSuit
- Apparel_Bandolier
- Apparel_Sash
- Apparel_Vacsuit
- Apparel_VacsuitChildren
- Apparel_PackBroadshield
- Apparel_WarMask
- Apparel_PlateArmor
- Apparel_FlakPants
- Apparel_FlakJacket
- Apparel_SimpleHelmet
- Apparel_SmokepopBelt

### HiTechArmor
- Apparel_PowerArmor
- Apparel_ArmorRecon
- Apparel_PowerArmorHelmet
- Apparel_ArmorHelmetRecon
- Apparel_MechlordSuit
- Apparel_Vacsuit
- Apparel_VacsuitChildren

### PsychicApparel
- Apparel_PsyfocusHelmet
- Apparel_EltexSkullcap
- Apparel_PsyfocusShirt
- Apparel_PsyfocusVest
- Apparel_PsyfocusRobe

### HoraxArmor (Anomaly DLC)
- Apparel_CultistMask
- Apparel_CeremonialCultistMask

### ExoticMisc
- Apparel_PackJump
- Apparel_PackBroadshield

### Artifact
- Apparel_PsychicShockLance
- Apparel_PsychicInsanityLance

---

## 참고사항

1. **상속 우선순위**: 자식 클래스에서 직접 지정한 tradeTag가 부모의 tradeTag를 덮어씁니다.
2. **다중 태그**: 일부 아이템은 여러 tradeTag를 가질 수 있습니다 (예: Clothing + Armor).
3. **ApparelMakeableBase 상속**: 이 부모를 상속받는 대부분의 아이템은 기본적으로 `Clothing` 태그를 가지지만, 많은 경우 `BasicClothing`으로 오버라이드됩니다.


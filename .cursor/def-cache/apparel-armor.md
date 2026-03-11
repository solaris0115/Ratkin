---
category: apparel-armor
last_updated: 2026-03-12
sources: RimworldData (Core, Royalty, Ideology, Biotech, Anomaly, Odyssey), Project/1.6/Defs
scope: 방어구(Apparel) 방어력 Sharp/Blunt/Heat, 전설 품질 기준. belt/utility 제외, 얼굴착용은 별도파일
fields: defName, stuffCategory, StuffEffectMultiplierArmor, BaseArmor, LowEnd, HighEnd (Sharp/Blunt/Heat)
formula: (BaseArmor + StuffEffectMultiplierArmor × StuffPower) × 1.80
keywords: Apparel Armor Sharp Blunt Heat Legendary Steel Plasteel Leather_Plain Leather_Thrumbo
---

# 방어구 (Apparel) 방어력

## 공식

`최종 방어력 = (BaseArmor + StuffEffectMultiplierArmor × StuffPower) × QualityFactor`

- **로우엔드**: 평범 품질(1.0) | Metallic→Steel, Leathery/Fabric→Leather_Plain, Woody→Wood
- **하이엔드**: 전설 품질(1.80) | Metallic→Plasteel, Leathery/Fabric→Leather_Thrumbo, Woody→Wood
- **고정 소재**: stuffCategories·costStuffCount 없음

## 림월드 (Core + DLC)

### Core (24개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_CowboyHat | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_BowlerHat | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_TribalHeaddress | Fabric | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Tuque | Fabric | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_SimpleHelmet | Metallic | 0.5 | 0.0 | 0.0 | 0.0 | 0.45 | 0.23 | 0.3 | 1.03 | 0.5 | 0.59 |
| Apparel_AdvancedHelmet | Metallic | 0.7 | 0.0 | 0.0 | 0.0 | 0.63 | 0.32 | 0.42 | 1.44 | 0.69 | 0.82 |
| Apparel_PsychicFoilHelmet | Fixed | 0.0 | 0.09 | 0.09 | 0.27 | 0.09 | 0.09 | 0.27 | 0.16 | 0.16 | 0.49 |
| Apparel_HatHood | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_ClothMask | Fabric | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_TribalA | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Parka | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Pants | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_BasicShirt | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_CollarShirt | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Duster | Fabric,Leathery | 0.3 | 0.0 | 0.0 | 0.0 | 0.24 | 0.07 | 0.45 | 1.12 | 0.19 | 0.81 |
| Apparel_Jacket | Fabric,Leathery | 0.3 | 0.0 | 0.0 | 0.0 | 0.24 | 0.07 | 0.45 | 1.12 | 0.19 | 0.81 |
| Apparel_PlateArmor | Metallic,Woody | 0.9 | 0.0 | 0.0 | 0.0 | 0.81 | 0.41 | 0.54 | 1.85 | 0.89 | 1.05 |
| Apparel_FlakVest | Fixed | 0.0 | 1.0 | 0.36 | 0.27 | 1.0 | 0.36 | 0.27 | 1.8 | 0.65 | 0.49 |
| Apparel_FlakPants | Fixed | 0.0 | 0.55 | 0.08 | 0.1 | 0.55 | 0.08 | 0.1 | 0.99 | 0.14 | 0.18 |
| Apparel_FlakJacket | Fixed | 0.0 | 0.55 | 0.08 | 0.1 | 0.55 | 0.08 | 0.1 | 0.99 | 0.14 | 0.18 |
| Apparel_PowerArmor | Fixed | 0.0 | 1.06 | 0.45 | 0.54 | 1.06 | 0.45 | 0.54 | 1.91 | 0.81 | 0.97 |
| Apparel_ArmorRecon | Fixed | 0.0 | 0.92 | 0.4 | 0.46 | 0.92 | 0.4 | 0.46 | 1.66 | 0.72 | 0.83 |
| Apparel_Cape | Fabric,Leathery | 0.3 | 0.0 | 0.0 | 0.0 | 0.24 | 0.07 | 0.45 | 1.12 | 0.19 | 0.81 |
| Apparel_Robe | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |


### Royalty (23개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_PsyfocusHelmet | Fixed | 0.0 | 0.09 | 0.09 | 0.27 | 0.09 | 0.09 | 0.27 | 0.16 | 0.16 | 0.49 |
| Apparel_EltexSkullcap | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_PsyfocusShirt | Fixed | 0.0 | 0.07 | 0.0 | 0.04 | 0.07 | 0.0 | 0.04 | 0.13 | 0.0 | 0.07 |
| Apparel_PsyfocusVest | Fixed | 0.0 | 0.07 | 0.0 | 0.04 | 0.07 | 0.0 | 0.04 | 0.13 | 0.0 | 0.07 |
| Apparel_PsyfocusRobe | Fixed | 0.0 | 0.07 | 0.0 | 0.04 | 0.07 | 0.0 | 0.04 | 0.13 | 0.0 | 0.07 |
| Apparel_ShirtRuffle | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Corset | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_VestRoyal | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_RobeRoyal | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_HatLadies | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_HatTop | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_Coronet | Metallic | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_Crown | Metallic | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_CrownStellic | Metallic | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_Beret | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_ArmorReconPrestige | Fixed | 0.0 | 0.92 | 0.4 | 0.46 | 0.92 | 0.4 | 0.46 | 1.66 | 0.72 | 0.83 |
| Apparel_ArmorMarinePrestige | Fixed | 0.0 | 1.06 | 0.45 | 0.54 | 1.06 | 0.45 | 0.54 | 1.91 | 0.81 | 0.97 |
| Apparel_ArmorCataphract | Fixed | 0.0 | 1.2 | 0.5 | 0.6 | 1.2 | 0.5 | 0.6 | 2.16 | 0.9 | 1.08 |
| Apparel_ArmorCataphractPrestige | Fixed | 0.0 | 1.2 | 0.5 | 0.6 | 1.2 | 0.5 | 0.6 | 2.16 | 0.9 | 1.08 |
| Apparel_ArmorLocust | Fixed | 0.0 | 0.87 | 0.35 | 0.41 | 0.87 | 0.35 | 0.41 | 1.57 | 0.63 | 0.74 |
| Apparel_ArmorMarineGrenadier | Fixed | 0.0 | 1.01 | 0.4 | 0.49 | 1.01 | 0.4 | 0.49 | 1.82 | 0.72 | 0.88 |
| Apparel_ArmorCataphractPhoenix | Fixed | 0.0 | 1.15 | 0.45 | 0.75 | 1.15 | 0.45 | 0.75 | 2.07 | 0.81 | 1.35 |
| Apparel_Gunlink | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |


### Ideology (9개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_Headwrap | Fabric | 0.25 | 0.0 | 0.0 | 0.0 | 0.2 | 0.06 | 0.38 | 0.94 | 0.16 | 0.68 |
| Apparel_Slicecap | Woody,Metallic | 0.1 | 0.0 | 0.0 | 0.0 | 0.09 | 0.05 | 0.06 | 0.21 | 0.1 | 0.12 |
| Apparel_Collar | Metallic,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_AuthorityCap | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Tailcap | Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Shadecone | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_Flophat | Fabric | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_BodyStrap | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| Apparel_TortureCrown | Metallic,Woody | 0.15 | 0.0 | 0.0 | 0.0 | 0.14 | 0.07 | 0.09 | 0.31 | 0.15 | 0.18 |


### Biotech (13개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_KidRomper | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_KidShirt | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_KidPants | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_KidParka | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_KidTribal | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| Apparel_KidHelmet | Metallic | 0.5 | 0.0 | 0.0 | 0.0 | 0.45 | 0.23 | 0.3 | 1.03 | 0.5 | 0.59 |
| Apparel_AirwireHeadset | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_ArrayHeadset | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_IntegratorHeadset | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_HeavyShield | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| Apparel_MechlordSuit | Fixed | 0.0 | 0.92 | 0.4 | 0.46 | 0.92 | 0.4 | 0.46 | 1.66 | 0.72 | 0.83 |
| Apparel_Bandolier | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| Apparel_Sash | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |


### Anomaly (1개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_LabCoat | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |


### Odyssey (2개)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Apparel_Vacsuit | Fixed | 0.0 | 0.52 | 0.25 | 0.66 | 0.52 | 0.25 | 0.66 | 0.94 | 0.45 | 1.19 |
| Apparel_VacsuitChildren | Fixed | 0.0 | 0.52 | 0.2 | 0.66 | 0.52 | 0.2 | 0.66 | 0.94 | 0.36 | 1.19 |


## 랫킨 (Ratkin)

| defName | stuffCategory | StuffEffectMult | Base_S | Base_B | Base_H | Low_S | Low_B | Low_H | High_S | High_B | High_H |
|---|---|---|---|---|---|---|---|---|---|---|---|
| RK_Plate | Metallic | 0.63 | 0.25 | 0.3 | 0.0 | 0.82 | 0.58 | 0.38 | 1.74 | 1.16 | 0.74 |
| RK_SantaRobe | Fabric,Leathery | 0.32 | 0.0 | 0.0 | 0.0 | 0.26 | 0.08 | 0.48 | 1.2 | 0.21 | 0.86 |
| RK_SantaHat | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_SantaSack | Leathery,Fabric | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_WhiteCoat | Fixed | 0.0 | 0.75 | 0.35 | 0.6 | 0.75 | 0.35 | 0.6 | 1.35 | 0.63 | 1.08 |
| RK_Sack | Leathery,Fabric | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_RoyalRobe | Fabric,Leathery | 0.36 | 0.0 | 0.0 | 0.0 | 0.29 | 0.09 | 0.54 | 1.35 | 0.23 | 0.97 |
| RK_RoyalCrown | Metallic | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_Apparel_Vacsuit | Fixed | 0.0 | 0.52 | 0.25 | 0.66 | 0.52 | 0.25 | 0.66 | 0.94 | 0.45 | 1.19 |
| RK_Apparel_VacsuitChildren | Fixed | 0.0 | 0.52 | 0.2 | 0.66 | 0.52 | 0.2 | 0.66 | 0.94 | 0.36 | 1.19 |
| RK_Apparel_SpaceArmor | Fixed | 0.0 | 1.0 | 0.5 | 1.0 | 1.0 | 0.5 | 1.0 | 1.8 | 0.9 | 1.8 |
| RK_CrossBack | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_Backpack | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_OutdoorBackpack | Fabric,Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_ApronSkirt | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_ApronSkirtChildren | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_SummerDress | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| RK_StrawHat | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_Muffler | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| RK_Cardigan | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_WoolenHat | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| RK_WorkerWear | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| RK_Coif | Fabric,Leathery | 0.1 | 0.0 | 0.0 | 0.0 | 0.08 | 0.02 | 0.15 | 0.37 | 0.06 | 0.27 |
| RK_ResearchGown | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_ResearchGlasses | Fixed | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_ExplorerWear | Fabric,Leathery | 0.2 | 0.0 | 0.0 | 0.0 | 0.16 | 0.05 | 0.3 | 0.75 | 0.13 | 0.54 |
| RK_ExplorerHat | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_ChefSuit | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_ChefHat | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_GaurdenUniform | Fabric,Leathery | 0.25 | 0.0 | 0.4 | 0.0 | 0.2 | 0.46 | 0.38 | 0.94 | 0.88 | 0.68 |
| RK_WinterRobe | Fabric,Leathery | 0.32 | 0.0 | 0.0 | 0.0 | 0.26 | 0.08 | 0.48 | 1.2 | 0.21 | 0.86 |
| RK_OrderUniform | Fabric,Leathery | 0.15 | 0.8 | 0.15 | 0.0 | 0.92 | 0.19 | 0.22 | 2.0 | 0.37 | 0.4 |
| RK_FlatColorCoat | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_FrillOnepiece | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_HairCorsage | Fabric | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_RibbonHairBand | Leathery | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_SistersDerss | Fabric,Leathery | 0.15 | 0.0 | 0.0 | 0.0 | 0.12 | 0.04 | 0.22 | 0.56 | 0.1 | 0.4 |
| RK_SistersVeil | Leathery,Fabric | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 | 0.0 |
| RK_BattleSuit | Leathery | 0.35 | 0.45 | 0.25 | 0.6 | 0.73 | 0.33 | 1.12 | 2.12 | 0.68 | 2.02 |
| RK_HeadBand | Fixed | 0.0 | 0.0 | 0.0 | 0.8 | 0.0 | 0.0 | 0.8 | 0.0 | 0.0 | 1.44 |
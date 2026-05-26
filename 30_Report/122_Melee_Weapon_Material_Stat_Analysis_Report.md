---

## Melee Weapon Material Stuff Plasteel Uranium DPS Legendary 근접무기 소재 스탯 분석
category: report
last_updated: 2026-03-09
sources:
  - .cursor/def-cache/melee-weapons.md
  - .cursor/def-cache/combat-coefficients.md
  - .cursor/def-cache/damage-type-mapping.md
scope: 전체 근접 무기. stuff=전설+소재, 비-stuff=전설만

# 근접 무기 소재별 스탯 분석 보고서

## 1. 분석 조건


| 항목     | 적용 값                                                                                  |
| ------ | ------------------------------------------------------------------------------------- |
| **등급** | 전설(Legendary) - MeleeWeapon_DamageMultiplier 1.65                                     |
| **소재** | sharp 타입 → 플라스틸 (Sharp 1.1, Cooldown 0.8) / blunt 타입 → 우라늄 (Blunt 1.5, Cooldown 1.10) |
| **대상** | **전체** (stuff: 전설+소재 / 비-stuff: 전설만)                                                  |


### DPS 계산식

```
power_adj = power × 1.65 × (SharpMult | BluntMult)
cooldown_adj = cooldown × (CooldownMult)
dps = power_adj / cooldown_adj
```


| 소재   | SharpMult | BluntMult | CooldownMult |
| ---- | --------- | --------- | ------------ |
| 플라스틸 | 1.1       | 0.9       | 0.8          |
| 우라늄  | 1.1       | 1.5       | 1.10         |


※ sharp 타입 capacity: Cut, Stab, Scratch, Bite, RK_HalberdCleave, RK_ToolCapacity_PickaxeStab, RK_ToolCapacity_ChainSword, Bomb  
※ blunt 타입 capacity: Blunt, Poke, Demolish, Crush

**소재 적용 불가 (생산 불가 / Fixed Cost)**: power_adj = power × 1.65, cooldown_adj = cooldown (소재 배율 없음)

---

## 2. 림월드 바닐라

Core → Royalty → Odyssey 순. stuff=전설+소재, 생산 불가/Fixed Cost=전설만(–).

| defName | capacity(damageType) | power | cooldown | 소재 | power_adj | cooldown_adj | dps |
|---------|----------------------|-------|----------|------|-----------|--------------|-----|
| MeleeWeapon_Club | Poke(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Club | Blunt(blunt) | 14 | 2 | 우라늄 | 34.65 | 2.20 | 15.75 |
| MeleeWeapon_Knife | Blunt(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Knife | Cut(sharp) | 12 | 1.5 | 플라스틸 | 21.78 | 1.20 | 18.15 |
| MeleeWeapon_Knife | Stab(sharp) | 13 | 2 | 플라스틸 | 23.60 | 1.60 | 14.75 |
| MeleeWeapon_Ikwa | Blunt(blunt); Poke(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Ikwa | Stab(sharp) | 15 | 2 | 플라스틸 | 27.23 | 1.60 | 17.02 |
| MeleeWeapon_Ikwa | Cut(sharp) | 15 | 2 | 플라스틸 | 27.23 | 1.60 | 17.02 |
| MeleeWeapon_Spear | Blunt(blunt); Poke(blunt) | 13 | 2.6 | 우라늄 | 32.14 | 2.86 | 11.24 |
| MeleeWeapon_Spear | Stab(sharp) | 23 | 2.6 | 플라스틸 | 41.75 | 2.08 | 20.07 |
| MeleeWeapon_Mace | Poke(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Mace | Blunt(blunt) | 15.7 | 2 | 우라늄 | 38.86 | 2.20 | 17.66 |
| MeleeWeapon_Gladius | Blunt(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Gladius | Stab(sharp) | 16 | 2 | 플라스틸 | 29.04 | 1.60 | 18.15 |
| MeleeWeapon_Gladius | Cut(sharp) | 16 | 2 | 플라스틸 | 29.04 | 1.60 | 18.15 |
| MeleeWeapon_LongSword | Blunt(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_LongSword | Stab(sharp) | 23 | 2.6 | 플라스틸 | 41.75 | 2.08 | 20.07 |
| MeleeWeapon_LongSword | Cut(sharp) | 23 | 2.6 | 플라스틸 | 41.75 | 2.08 | 20.07 |
| MeleeWeapon_BreachAxe | Blunt(blunt); Poke(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_BreachAxe | Demolish(blunt) | 7.5 | 1 | 우라늄 | 18.56 | 1.10 | 16.87 |
| MeleeWeapon_Axe | Poke(blunt) | 9 | 2 | 우라늄 | 22.28 | 2.20 | 10.13 |
| MeleeWeapon_Axe | Cut(sharp) | 15 | 2 | 플라스틸 | 27.23 | 1.60 | 17.02 |
| MeleeWeapon_Warhammer | Poke(blunt) | 11 | 2.6 | 우라늄 | 27.23 | 2.86 | 9.52 |
| MeleeWeapon_Warhammer | Blunt(blunt) | 20 | 2.6 | 우라늄 | 49.50 | 2.86 | 17.31 |
| MeleeWeapon_MonoSwordBladelink | Blunt(blunt) | 12 | 1.6 | – | 19.80 | 1.60 | 12.38 |
| MeleeWeapon_MonoSwordBladelink | Stab(sharp) | 27 | 1.6 | – | 44.55 | 1.60 | 27.84 |
| MeleeWeapon_MonoSwordBladelink | Cut(sharp) | 27 | 1.6 | – | 44.55 | 1.60 | 27.84 |
| MeleeWeapon_ZeusHammerBladelink | Poke(blunt) | 15 | 1.6 | – | 24.75 | 1.60 | 15.47 |
| MeleeWeapon_ZeusHammerBladelink | Blunt(blunt)+EMP | 31 | 2.2 | – | 51.15 | 2.20 | 23.25 |
| MeleeWeapon_PlasmaSwordBladelink | Blunt(blunt) | 12 | 1.6 | – | 19.80 | 1.60 | 12.38 |
| MeleeWeapon_PlasmaSwordBladelink | Stab(sharp)+Flame(heat) | 23 | 2 | – | 37.95 | 2.00 | 18.98 |
| MeleeWeapon_PlasmaSwordBladelink | Cut(sharp)+Flame(heat) | 23 | 2 | – | 37.95 | 2.00 | 18.98 |
| MeleeWeapon_MonoSword | Blunt(blunt) | 12 | 1.6 | – | 19.80 | 1.60 | 12.38 |
| MeleeWeapon_MonoSword | Stab(sharp) | 25 | 2 | – | 41.25 | 2.00 | 20.63 |
| MeleeWeapon_MonoSword | Cut(sharp) | 25 | 2 | – | 41.25 | 2.00 | 20.63 |
| MeleeWeapon_Zeushammer | Poke(blunt) | 15 | 2 | – | 24.75 | 2.00 | 12.38 |
| MeleeWeapon_Zeushammer | Blunt(blunt)+EMP | 31 | 3 | – | 51.15 | 3.00 | 17.05 |
| MeleeWeapon_PlasmaSword | Blunt(blunt) | 12 | 2 | – | 19.80 | 2.00 | 9.90 |
| MeleeWeapon_PlasmaSword | Stab(sharp)+Flame(heat) | 21 | 2.6 | – | 34.65 | 2.60 | 13.33 |
| MeleeWeapon_PlasmaSword | Cut(sharp)+Flame(heat) | 21 | 2.6 | – | 34.65 | 2.60 | 13.33 |
| MeleeWeapon_PsyfocusStaff | Blunt(blunt) | 12 | 2.6 | – | 19.80 | 2.60 | 7.62 |

---

## 3. 랫킨

stuff=전설+소재, Fixed Cost/생산 불가=전설만(–).

| defName | capacity(damageType) | power | cooldown | 소재 | power_adj | cooldown_adj | dps |
|---------|----------------------|-------|----------|------|-----------|--------------|-----|
| RK_Dagger | Stab(sharp) | 15 | 2 | 플라스틸 | 27.23 | 1.60 | 17.02 |
| RK_Dagger | Cut(sharp) | 12 | 1.5 | 플라스틸 | 21.78 | 1.20 | 18.15 |
| RK_OneHanded | Stab(sharp) | 21 | 2.4 | 플라스틸 | 38.12 | 1.92 | 19.85 |
| RK_OneHanded | Cut(sharp) | 21 | 2.4 | 플라스틸 | 38.12 | 1.92 | 19.85 |
| RK_OneHanded | Blunt(blunt) | 10 | 1.8 | 우라늄 | 24.75 | 1.98 | 12.50 |
| RK_Mace | Blunt(blunt) | 19 | 2.4 | 우라늄 | 47.03 | 2.64 | 17.81 |
| RK_Mace | Blunt(blunt) | 10 | 2.2 | 우라늄 | 24.75 | 2.42 | 10.23 |
| RK_LightLance | Stab(sharp) | 27 | 2.5 | – | 44.55 | 2.50 | 17.82 |
| RK_LightLance | Cut(sharp) | 21 | 2.8 | – | 34.65 | 2.80 | 12.38 |
| RK_LightLance | Blunt(blunt) | 13 | 2.8 | – | 21.45 | 2.80 | 7.66 |
| RK_TwoHanded | Blunt(blunt) | 12 | 2 | 우라늄 | 29.70 | 2.20 | 13.50 |
| RK_TwoHanded | Stab(sharp) | 27 | 2.9 | 플라스틸 | 49.01 | 2.32 | 21.12 |
| RK_TwoHanded | Cut(sharp) | 27 | 2.9 | 플라스틸 | 49.01 | 2.32 | 21.12 |
| RK_HeavyLance | Stab(sharp) | 32 | 3 | 플라스틸 | 58.08 | 2.40 | 24.20 |
| RK_HeavyLance | Cut(sharp) | 27 | 3.4 | 플라스틸 | 49.01 | 2.72 | 18.02 |
| RK_HeavyLance | Blunt(blunt) | 16 | 3.4 | 우라늄 | 39.60 | 3.74 | 10.59 |
| RK_LongSword | Stab(sharp) | 23 | 2.1 | 플라스틸 | 41.75 | 1.68 | 24.85 |
| RK_LongSword | Cut(sharp) | 23 | 2.1 | 플라스틸 | 41.75 | 1.68 | 24.85 |
| RK_LongSword | Blunt(blunt) | 12 | 1.8 | 우라늄 | 29.70 | 1.98 | 15.00 |
| RK_Spear | Blunt(blunt); Poke(blunt) | 6.5 | 1.3 | 우라늄 | 16.09 | 1.43 | 11.25 |
| RK_Spear | Stab(sharp) | 11.5 | 1.3 | 플라스틸 | 20.87 | 1.04 | 20.07 |
| RK_Halberd | Stab(sharp) | 15 | 1.6 | 플라스틸 | 27.23 | 1.28 | 21.27 |
| RK_Halberd | RK_HalberdCleave(sharp) | 23 | 3.2 | 플라스틸 | 41.75 | 2.56 | 16.31 |
| RK_Axe | Blunt(blunt) | 8 | 2 | 우라늄 | 19.80 | 2.20 | 9.00 |
| RK_Axe | Cut(sharp) | 20 | 2.5 | 플라스틸 | 36.30 | 2.00 | 18.15 |
| RK_Cleaver | Blunt(blunt) | 5 | 2 | 우라늄 | 12.38 | 2.20 | 5.63 |
| RK_Cleaver | Cut(sharp) | 12 | 1.8 | 플라스틸 | 21.78 | 1.44 | 15.13 |
| RK_Hockey | Stab(sharp) | 14 | 2 | 플라스틸 | 25.41 | 1.60 | 15.88 |
| RK_Hockey | Cut(sharp) | 14 | 2 | 플라스틸 | 25.41 | 1.60 | 15.88 |
| RK_Fork | Stab(sharp) | 14 | 2 | 플라스틸 | 25.41 | 1.60 | 15.88 |
| RK_Fork | Cut(sharp) | 14 | 2 | 플라스틸 | 25.41 | 1.60 | 15.88 |
| RK_Pickaxe | Blunt(blunt) | 15 | 2.5 | 우라늄 | 37.13 | 2.75 | 13.50 |
| RK_Pickaxe | PickaxeStab(sharp)+Crush(blunt) | 10 | 3 | 플라스틸* | 18.15 | 2.40 | 7.56 |
| RK_Weapon_Gunlance | Stab(sharp) | 20 | 3 | 플라스틸 | 36.30 | 2.40 | 15.13 |
| RK_Weapon_Gunlance | GunlanceShell_Normal(Bomb→sharp) | 25 | 3 | 플라스틸 | 45.38 | 2.40 | 18.91 |
| RK_Weapon_ProtoChainSword | RK_ToolCapacity_ChainSword(sharp) | 20 | 3 | – | 33.00 | 3.00 | 11.00 |
| RK_Weapon_ProtoFlameChainSword | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) | 20 | 3 | – | 33.00 | 3.00 | 11.00 |
| RK_MagicWand | RK_ToolCapacity_MeleeExplosion(sharp) | 5 | 1.5 | – | 8.25 | 1.50 | 5.50 |

※ RK_Pickaxe PickaxeStab: 복합 타입(sharp+blunt). 주 데미지 Stab(sharp) 기준 플라스틸 적용

---

## 4. DPS 순위

### 4.1 바닐라 기준 순위 15

| 순위 | defName | capacity | power_adj | cooldown_adj | dps |
|------|---------|----------|-----------|--------------|-----|
| 1 | MeleeWeapon_MonoSwordBladelink | Stab, Cut | 44.55 | 1.60 | 27.84 |
| 2 | MeleeWeapon_ZeusHammerBladelink | Blunt+EMP | 51.15 | 2.20 | 23.25 |
| 3 | MeleeWeapon_MonoSword | Stab, Cut | 41.25 | 2.00 | 20.63 |
| 4 | MeleeWeapon_LongSword | Stab, Cut | 41.75 | 2.08 | 20.07 |
| 4 | MeleeWeapon_Spear | Stab | 41.75 | 2.08 | 20.07 |
| 6 | MeleeWeapon_PlasmaSwordBladelink | Stab, Cut | 37.95 | 2.00 | 18.98 |
| 7 | MeleeWeapon_Knife | Cut | 21.78 | 1.20 | 18.15 |
| 7 | MeleeWeapon_Gladius | Stab, Cut | 29.04 | 1.60 | 18.15 |
| 9 | MeleeWeapon_Mace | Blunt | 38.86 | 2.20 | 17.66 |
| 10 | MeleeWeapon_Warhammer | Blunt | 49.50 | 2.86 | 17.31 |
| 11 | MeleeWeapon_Ikwa | Stab, Cut | 27.23 | 1.60 | 17.02 |
| 11 | MeleeWeapon_Axe | Cut | 27.23 | 1.60 | 17.02 |
| 13 | MeleeWeapon_Zeushammer | Blunt+EMP | 51.15 | 3.00 | 17.05 |
| 14 | MeleeWeapon_BreachAxe | Demolish | 18.56 | 1.10 | 16.87 |
| 15 | MeleeWeapon_Club | Blunt | 34.65 | 2.20 | 15.75 |

### 4.2 랫킨 기준 순위 15

| 순위 | defName | capacity | power_adj | cooldown_adj | dps |
|------|---------|----------|-----------|--------------|-----|
| 1 | RK_LongSword | Stab, Cut | 41.75 | 1.68 | 24.85 |
| 2 | RK_HeavyLance | Stab | 58.08 | 2.40 | 24.20 |
| 3 | RK_Halberd | Stab | 27.23 | 1.28 | 21.27 |
| 4 | RK_TwoHanded | Stab, Cut | 49.01 | 2.32 | 21.12 |
| 5 | RK_Spear | Stab | 20.87 | 1.04 | 20.07 |
| 6 | RK_OneHanded | Stab, Cut | 38.12 | 1.92 | 19.85 |
| 7 | RK_Weapon_Gunlance | GunlanceShell | 45.38 | 2.40 | 18.91 |
| 8 | RK_Dagger | Cut | 21.78 | 1.20 | 18.15 |
| 8 | RK_Axe | Cut | 36.30 | 2.00 | 18.15 |
| 10 | RK_Mace | Blunt | 47.03 | 2.64 | 17.81 |
| 11 | RK_LightLance | Stab | 44.55 | 2.50 | 17.82 |
| 12 | RK_Hockey | Stab, Cut | 25.41 | 1.60 | 15.88 |
| 12 | RK_Fork | Stab, Cut | 25.41 | 1.60 | 15.88 |
| 14 | RK_Cleaver | Cut | 21.78 | 1.44 | 15.13 |
| 15 | RK_Pickaxe | Blunt | 37.13 | 2.75 | 13.50 |

### 4.3 전체 순위 (20개)

| 순위 | defName | capacity | power_adj | cooldown_adj | dps |
|------|---------|----------|-----------|--------------|-----|
| 1 | MeleeWeapon_MonoSwordBladelink | Stab, Cut | 44.55 | 1.60 | 27.84 |
| 2 | RK_LongSword | Stab, Cut | 41.75 | 1.68 | 24.85 |
| 3 | RK_HeavyLance | Stab | 58.08 | 2.40 | 24.20 |
| 4 | MeleeWeapon_ZeusHammerBladelink | Blunt+EMP | 51.15 | 2.20 | 23.25 |
| 5 | RK_Halberd | Stab | 27.23 | 1.28 | 21.27 |
| 6 | RK_TwoHanded | Stab, Cut | 49.01 | 2.32 | 21.12 |
| 7 | MeleeWeapon_MonoSword | Stab, Cut | 41.25 | 2.00 | 20.63 |
| 8 | MeleeWeapon_LongSword | Stab, Cut | 41.75 | 2.08 | 20.07 |
| 8 | MeleeWeapon_Spear | Stab | 41.75 | 2.08 | 20.07 |
| 8 | RK_Spear | Stab | 20.87 | 1.04 | 20.07 |
| 11 | RK_OneHanded | Stab, Cut | 38.12 | 1.92 | 19.85 |
| 12 | MeleeWeapon_PlasmaSwordBladelink | Stab, Cut | 37.95 | 2.00 | 18.98 |
| 13 | RK_Weapon_Gunlance | GunlanceShell | 45.38 | 2.40 | 18.91 |
| 14 | MeleeWeapon_Knife | Cut | 21.78 | 1.20 | 18.15 |
| 14 | MeleeWeapon_Gladius | Stab, Cut | 29.04 | 1.60 | 18.15 |
| 14 | RK_Dagger | Cut | 21.78 | 1.20 | 18.15 |
| 14 | RK_Axe | Cut | 36.30 | 2.00 | 18.15 |
| 18 | RK_LightLance | Stab | 44.55 | 2.50 | 17.82 |
| 19 | RK_Mace | Blunt | 47.03 | 2.64 | 17.81 |
| 20 | MeleeWeapon_Mace | Blunt | 38.86 | 2.20 | 17.66 |

### 4.4 sharp 기준 순위 15

| 순위 | defName | capacity | power_adj | cooldown_adj | dps |
|------|---------|----------|-----------|--------------|-----|
| 1 | MeleeWeapon_MonoSwordBladelink | Stab, Cut | 44.55 | 1.60 | 27.84 |
| 2 | RK_LongSword | Stab, Cut | 41.75 | 1.68 | 24.85 |
| 3 | RK_HeavyLance | Stab | 58.08 | 2.40 | 24.20 |
| 4 | RK_Halberd | Stab | 27.23 | 1.28 | 21.27 |
| 5 | RK_TwoHanded | Stab, Cut | 49.01 | 2.32 | 21.12 |
| 6 | MeleeWeapon_MonoSword | Stab, Cut | 41.25 | 2.00 | 20.63 |
| 7 | MeleeWeapon_LongSword | Stab, Cut | 41.75 | 2.08 | 20.07 |
| 7 | MeleeWeapon_Spear | Stab | 41.75 | 2.08 | 20.07 |
| 7 | RK_Spear | Stab | 20.87 | 1.04 | 20.07 |
| 10 | RK_OneHanded | Stab, Cut | 38.12 | 1.92 | 19.85 |
| 11 | MeleeWeapon_PlasmaSwordBladelink | Stab, Cut | 37.95 | 2.00 | 18.98 |
| 12 | RK_Weapon_Gunlance | GunlanceShell | 45.38 | 2.40 | 18.91 |
| 13 | MeleeWeapon_Knife | Cut | 21.78 | 1.20 | 18.15 |
| 13 | MeleeWeapon_Gladius | Stab, Cut | 29.04 | 1.60 | 18.15 |
| 13 | RK_Dagger | Cut | 21.78 | 1.20 | 18.15 |
| 13 | RK_Axe | Cut | 36.30 | 2.00 | 18.15 |

### 4.5 blunt 기준 순위 15

| 순위 | defName | capacity | power_adj | cooldown_adj | dps |
|------|---------|----------|-----------|--------------|-----|
| 1 | MeleeWeapon_ZeusHammerBladelink | Blunt+EMP | 51.15 | 2.20 | 23.25 |
| 2 | RK_Mace | Blunt | 47.03 | 2.64 | 17.81 |
| 3 | MeleeWeapon_Mace | Blunt | 38.86 | 2.20 | 17.66 |
| 4 | MeleeWeapon_Warhammer | Blunt | 49.50 | 2.86 | 17.31 |
| 5 | MeleeWeapon_Zeushammer | Blunt+EMP | 51.15 | 3.00 | 17.05 |
| 6 | MeleeWeapon_BreachAxe | Demolish | 18.56 | 1.10 | 16.87 |
| 7 | MeleeWeapon_Club | Blunt | 34.65 | 2.20 | 15.75 |
| 8 | MeleeWeapon_ZeusHammerBladelink | Poke | 24.75 | 1.60 | 15.47 |
| 9 | RK_LongSword | Blunt | 29.70 | 1.98 | 15.00 |
| 10 | RK_TwoHanded | Blunt | 29.70 | 2.20 | 13.50 |
| 10 | RK_Pickaxe | Blunt | 37.13 | 2.75 | 13.50 |
| 12 | RK_OneHanded | Blunt | 24.75 | 1.98 | 12.50 |
| 13 | MeleeWeapon_MonoSwordBladelink | Blunt | 19.80 | 1.60 | 12.38 |
| 13 | MeleeWeapon_PlasmaSwordBladelink | Blunt | 19.80 | 1.60 | 12.38 |
| 13 | MeleeWeapon_MonoSword | Blunt | 19.80 | 1.60 | 12.38 |


---

## 5. 요약

- **stuff**: 전설 + 소재(sharp→플라스틸, blunt→우라늄) 적용
- **소재 적용 불가** (생산 불가 / Fixed Cost): **전설만** 적용 (power × 1.65, cooldown 그대로)
- **sharp → 플라스틸**: Cut, Stab, Scratch 등. Cooldown 0.8로 공격 속도 20% 상승
- **blunt → 우라늄**: Blunt, Poke, Demolish 등. BluntDamage 1.5배
- **전체 최고 DPS**: MeleeWeapon_MonoSwordBladelink 27.84 (전설만, 생산 불가)
- **stuff 최고 DPS**: RK_LongSword 24.85 (sharp), RK_Mace 17.81 (blunt)


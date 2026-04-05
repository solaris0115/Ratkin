<!-- Tags: RimWorld ArmorPenetration Mechanoid Centipede Industrial Spacer TripleRocket DoomsdayRocket 방어관통 삼단로켓 심판의날 BioTech 품질 상급 Melee Longsword 장검 Steel Plasteel 강철 플라스틸 전설 Legendary -->

# 바닐라 원거리 — 발사체 기준 관통 (참고)

**소스**: `RimworldData/Core`, `RimworldData/Biotech` · 무기 배율 **1.0** 가정.

**품질 가정 (밸런스용)**

- **상급**: 산업 **일반 총기**만 (아래 §산업 총기 표).
- **평범**: 그 외 (차지·로켓·발사기·수류탄·포탑 탄·Biotech 독탄 등).
- **없음**: 메카노이드 무기.

---

## 메카노이드 — 품질 없음

| defName | AP |
|---------|-----|
| Gun_ChargeBlasterHeavy | 0.225 |
| Gun_InfernoCannon | 0 |
| Gun_Needle | 0.35 |
| Gun_NeedleLauncher | 0.35 |
| Gun_ToxicNeedle | 0.35 |
| Gun_MiniShotgun | 0.12 |
| Gun_Slugthrower | 0.18 |
| Gun_Spiner | 0.18 |
| Gun_MiniFlameblaster | 0 |
| Gun_BeamGraser | 0.5 |
| Gun_HellsphereCannon | 1.0 |

*센터피드: 위 중 차지블래스터·인페르노·니들 조합이 일반적.*

---

## 산업 총기 — 품질 **상급**

| defName | AP |
|---------|-----|
| Gun_Revolver | 0.18 |
| Gun_Autopistol | 0.15 |
| Gun_MachinePistol | 0.09 |
| Gun_BoltActionRifle | 0.27 |
| Gun_PumpShotgun | 0.14 |
| Gun_ChainShotgun | 0.14 |
| Gun_HeavySMG | 0.18 |
| Gun_LMG | 0.18 |
| Gun_AssaultRifle | 0.165 |
| Gun_SniperRifle | 0.375 |
| Gun_Minigun | 0.15 |

---

## 평범

**차지**

| defName | AP |
|---------|-----|
| Gun_ChargeRifle | 0.35 |
| Gun_ChargeLance | 0.45 |

**발사기·수류탄**

| defName | AP |
|---------|-----|
| Gun_IncendiaryLauncher | 0 |
| Gun_SmokeLauncher | 0 |
| Gun_EmpLauncher | 0 |
| Weapon_GrenadeFrag | 0.10 |
| Weapon_GrenadeMolotov | 0 |
| Weapon_GrenadeEMP | 0 |

**일회용 로켓**

| defName | AP |
|---------|-----|
| Gun_TripleRocket | 0.10 |
| Gun_DoomsdayRocket | 0.10 |

**포탑 (발사체)**

| defName | AP |
|---------|-----|
| Bullet_MiniTurret | 0.18 |
| Bullet_AutocannonTurret | 0.405 |
| Bullet_TurretSniper | 0.825 |
| Proj_Rocket | 0.36 |

**Biotech**

| defName | AP |
|---------|-----|
| Gun_ToxbombLauncher | 0 |

---

## 근접 무기 — 장검 (MeleeWeapon_LongSword)

**공식**: `AP = power × MeleeWeapon_DamageMultiplier(품질) × StuffDamageMult(소재) × 0.015`

**소재 계수**: Steel Sharp=1.0 Blunt=1.0 · Plasteel Sharp=1.1 Blunt=0.9
**품질 계수**: 상급=1.1 · 전설=1.65

**장검 툴**: handle (Blunt, 9) / point (Stab, 23) / edge (Cut, 23)

### 강철 상급

| 툴 | 종류 | AP |
|-----|------|-----|
| handle | Blunt | 0.149 |
| point | Stab | 0.380 |
| edge | Cut | 0.380 |

### 플라스틸 상급

| 툴 | 종류 | AP |
|-----|------|-----|
| handle | Blunt | 0.134 |
| point | Stab | 0.417 |
| edge | Cut | 0.417 |

### 강철 전설

| 툴 | 종류 | AP |
|-----|------|-----|
| handle | Blunt | 0.223 |
| point | Stab | 0.569 |
| edge | Cut | 0.569 |

### 플라스틸 전설

| 툴 | 종류 | AP |
|-----|------|-----|
| handle | Blunt | 0.200 |
| point | Stab | 0.626 |
| edge | Cut | 0.626 |

---

*방패/디플렉트 참조용. 버전에 따라 Def 변동 가능.*

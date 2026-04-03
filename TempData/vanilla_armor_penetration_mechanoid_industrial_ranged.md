<!-- Tags: RimWorld ArmorPenetration Mechanoid Centipede Industrial Spacer TripleRocket DoomsdayRocket 방어관통 삼단로켓 심판의날 BioTech -->

# 림월드 바닐라 — 메카노이드·센터피드·산업 이상 원거리 무기 관통력 정리

**데이터 경로**: `RimworldData/Core`, `RimworldData/Biotech` Defs + `RimworldSource/Verse/ProjectileProperties.cs` (`GetArmorPenetration`)

**표에 쓴 값**: 발사체(Projectile) 기준 **기본 관통**. 실제 인게임은 무기의 `RangedWeapon_ArmorPenetrationMultiplier` 등으로 곱해질 수 있음(여기서는 무기 배율 **1.0 가정**).

**규칙 요약**

1. `armorPenetrationBase >= 0` 이면 그 값이 баз스.
2. 그렇지 않고 `damageAmountBase`가 있으면(명시 또는 DamageDef 기본 피해로 채워진 경우) 먼저 `armorPenetrationBase`가 **-1**이면 일단 **-1**이 되고, 최종적으로 **0 미만이면 `GetDamageAmount × 0.015`**.
3. `damageAmountBase`도 없고(Def상 -1) `armorPenetrationBase`도 -1이면 → **`damageDef.defaultArmorPenetration`**.
4. `damageDef.armorCategory == null` 이면 관통 **0**(EMP·연막·독가스 등).

---

## 1. 메카노이드 원거리 (Core + Biotech)

| 무기 (defName) | 발사체 / 비고 | 관통(Sharp 등 유효 시) | 산출 |
|----------------|---------------|-------------------------|------|
| Gun_ChargeBlasterHeavy (헤비 차지 블래스터) | Bullet_ChargeBlasterHeavy | **0.225** | 15×0.015 |
| Gun_InfernoCannon (인페르노 캐논) | Bullet_InfernoCannon, Flame 폭발 | **0** | Flame `defaultArmorPenetration` |
| Gun_Needle (니들 건) | Bullet_NeedleGun | **0.35** | XML 명시 |
| Gun_NeedleLauncher (니들 런처, Biotech) | Bullet_NeedleGun | **0.35** | 동일 탄 |
| Gun_ToxicNeedle (독 니들 건) | Bullet_ToxicNeedleGun | **0.35** | XML 명시 |
| Gun_MiniShotgun | Bullet_MiniShotgun | **0.12** | XML 명시 |
| Gun_Slugthrower | Bullet_Slugthrower | **0.18** | 12×0.015 |
| Gun_Spiner | Bullet_Spiner | **0.18** | 12×0.015 |
| Gun_MiniFlameblaster | Verb_SpewFire (화염 방사) | **0** | 직접 폭발/화염 계열 (별도 발사체 관통 테이블 밖) |
| Gun_BeamGraser | 빔 — DamageDef `Beam` | **0.5** | Beam `defaultArmorPenetration` (빔은 실제 적용이 틱/셀 단위라 참고용) |
| Gun_HellsphereCannon | Bullet_HellsphereCannonGun, Vaporize | **1.0** | Vaporize `defaultArmorPenetration` |

---

## 2. 센터피드(Centipede) 탑승 무장

센터피드는 위 **Core 메카노이드 중형/대형 셋**(차지 블래스터 / 인페르노 / 니들)을 탑재하는 경우가 일반적이다. 위 표에서 동일 행을 참고하면 된다.

---

## 3. 산업(Industrial) 이상 — 사람/제작 탄환 원거리 (Core `RangedIndustrial.xml` + `RangedSpacer.xml`)

### 3.1 산업 (techLevel Industrial, 총기류)

| 무기 (defName) | 발사체 | 관통 | 산출 |
|----------------|--------|------|------|
| Gun_Revolver | Bullet_Revolver | 0.18 | 12×0.015 |
| Gun_Autopistol | Bullet_Autopistol | 0.15 | 10×0.015 |
| Gun_MachinePistol | Bullet_MachinePistol | 0.09 | 6×0.015 |
| Gun_BoltActionRifle | Bullet_BoltActionRifle | 0.27 | 18×0.015 |
| Gun_PumpShotgun | Bullet_Shotgun | **0.14** | XML 명시 |
| Gun_ChainShotgun | Bullet_Shotgun | **0.14** | 동일 |
| Gun_HeavySMG | Bullet_HeavySMG | 0.18 | 12×0.015 |
| Gun_LMG | Bullet_LMG | 0.18 | 12×0.015 |
| Gun_AssaultRifle | Bullet_AssaultRifle | 0.165 | 11×0.015 |
| Gun_SniperRifle | Bullet_SniperRifle | 0.375 | 25×0.015 |
| Gun_Minigun | Bullet_Minigun | 0.15 | 10×0.015 |

### 3.2 산업 — 발사기·수류탄 투척 (폭발/특수)

| 무기 / 발사체 | damageDef | 관통 | 비고 |
|---------------|-----------|------|------|
| Gun_IncendiaryLauncher → Bullet_IncendiaryLauncher | Flame | **0** | |
| Gun_SmokeLauncher → Bullet_SmokeLauncher | Smoke | **0** | armorCategory 없음 |
| Gun_EmpLauncher → Bullet_EMPLauncher | EMP | **0** | armorCategory 없음 |
| Weapon_GrenadeFrag → Proj_GrenadeFrag | Bomb | **0.10** | 피해·관통 미지정 → Bomb 기본 |
| Weapon_GrenadeMolotov → Proj_GrenadeMolotov | Flame | **0** | |
| Weapon_GrenadeEMP → Proj_GrenadeEMP | EMP | **0** | |

### 3.3 우주 (Spacer) — 차지 무기

| 무기 (defName) | 발사체 | 관통 | 산출 |
|----------------|--------|------|------|
| Gun_ChargeRifle | Bullet_ChargeRifle | **0.35** | XML 명시 |
| Gun_ChargeLance | Bullet_ChargeLance | **0.45** | 30×0.015 |

---

## 4. 일회용 로켓 (강조) — `RangedIndustrialConsumable.xml`

| 무기 (defName) | 발사체 | 주 폭발 damageDef | 관통 | 산출 |
|----------------|--------|-------------------|------|------|
| **Gun_TripleRocket** (삼단 로켓) | Bullet_Rocket | Bomb | **0.10** | 탄에 피해/관통 미지정 → **Bomb `defaultArmorPenetration`** |
| **Gun_DoomsdayRocket** (심판의 날 / 최후의 심판 로켓) | Bullet_DoomsdayRocket | Bomb(주) + Flame(부가) | **Bomb 0.10** | 동일. `Projectile_DoomsdayRocket`가 Bomb 후 추가 Flame 폭발 시에도 동일 관통 인자를 넘김(Heat 방어에는 별도 계산). |

※ 삼단/심판 모두 **내부적으로는 Bomb 폭발의 기본 관통 0.10**이다. 피해량 50 등은 Bomb Def 기본값이 폭발 피해 계산에 쓰이나, **이 둘은 XML에 `damageAmountBase`가 없어 `50×0.015` 규칙이 적용되지 않고** `defaultArmorPenetration`만 쓰인다.

---

## 5. 포탑 등 (산업~, 참고)

| 구성 | 발사체 | 관통 | 산출 |
|------|--------|------|------|
| 미니 포탑 | Bullet_MiniTurret | 0.18 | 12×0.015 |
| 오토캐논 포탑 | Bullet_AutocannonTurret | 0.405 | 27×0.015 |
| 우라늄 슬러그 포탑 | Bullet_TurretSniper | 0.825 | 55×0.015 |
| 로켓탄막 포탑 | Proj_Rocket | **0.36** | 24×0.015 (명시 피해 24) |

---

## 6. Biotech 제작 산업 무기 (참고)

| 무기 | 발사체 | 관통 |
|------|--------|------|
| Gun_ToxbombLauncher | Bullet_ToxbombLauncher | **0** (ToxGas — 방어 카테고리 없음) |

---

*생성 목적: 방패/방어구 디플렉트·밸런스 참조용. 게임 버전에 따라 Def가 바뀔 수 있음.*

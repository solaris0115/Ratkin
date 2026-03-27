# Vanilla Weapon Armor Penetration Legendary Top10 Report

태그: `RimWorld` `ArmorPenetration` `ArmorPenetrationMultiplier` `MeleeWeapon_DamageMultiplier` `Legendary` `Quality` `ChargeLance` `Hellsphere` `MonoSword` `BeamRepeater` `Odyssey` `바닐라 무기` `방어 관통` `전설 품질` `원거리` `근접` `ProjectileProperties` `VerbProperties`

---

## 1. 목적

림월드 **공식 데이터(코어 + DLC)** 기준으로, **전설(Legendary) 품질**을 가정했을 때 **방어 관통력이 높은 원거리·근접 무기**를 각각 정리한다. 플라스틸·우라늄·제작 불가 무기 등 **재료/획득 조건은 제한하지 않음**.

---

## 2. 계산 근거 (소스)

### 2.1 원거리

- `Verse.ProjectileProperties.GetArmorPenetration` (`RimworldSource/Verse/ProjectileProperties.cs`)
  - `armorPenetrationBase >= 0` 이거나 `damageAmountBase`가 지정된 경우 기본값은 `armorPenetrationBase` 사용.
  - 기본값이 음수면 **`GetDamageAmount(null, null)`** (무기 없음 → 피해 배율 1) × **0.015**.
  - 이후 장착 무기가 있으면 **`StatDefOf.RangedWeapon_ArmorPenetrationMultiplier`** 를 곱함.
- 전설 품질에서 해당 배율: **`Core/Defs/Stats/Stats_Weapons_Ranged.xml`** 의 `StatPart_Quality` → **Legendary 1.5**.

### 2.2 근접

- `Verse.VerbProperties.AdjustedArmorPenetration` (`RimworldSource/Verse/VerbProperties.cs`)
  - 도구에 **`armorPenetration >= 0`** 이면 그 값을 사용 후, 장비가 있으면 **`MeleeWeapon_DamageMultiplier`** 곱.
  - 도구 관통이 **-1(미지정)** 이면 **조정 근접 피해 × 0.015** (재료 날/둔기 배율 등 포함).
- 전설 품질 `MeleeWeapon_DamageMultiplier`: **Legendary 1.65** (`Core/Defs/Stats/Stats_Weapons_Melee.xml`).

### 2.3 데이터 출처

- 무기·탄환 Def: `RimworldData/**/ThingDefs_Misc/Weapons/*.xml`
- 피해 종류 기본 관통: `RimworldData/Core/Defs/DamageDefs/*.xml` (예: `Vaporize.defaultArmorPenetration`)

---

## 3. 원거리 무기 — 관통 상위 10 (전설 기준)

표의 **%**는 게임 내 방어 관통 표기와 동일한 **0~1 스케일을 퍼센트로 표시**한 것 (예: 1.5 = 150%).

| 순위 | 무기 (라벨 기준) | DefName | 대략 관통 (전설) | 비고 |
|:----:|------------------|---------|------------------|------|
| 1 | 헬스피어 캐논 | `Gun_HellsphereCannon` | **150%** | `Vaporize` 기본 관통 100% × 1.5. 바이오텍/메카노이드. |
| 2 | 빔 리피터 | `Gun_BeamRepeater` | **75%** | **Odyssey DLC**. 탄 `armorPenetrationBase` 0.5 × 1.5. |
| 3 | 차지 랜스 | `Gun_ChargeLance` | **67.5%** | 피해 30만 지정 → 30×0.015=45% 기본, ×1.5 = 67.5%. |
| 4 | 스나이퍼 라이플 | `Gun_SniperRifle` | **56.25%** | 피해 25 → 37.5% × 1.5. |
| 5 | 차지 라이플 | `Gun_ChargeRifle` | **52.5%** | `Bullet_ChargeRifle` 관통 0.35 × 1.5. |
| 6 | 메카노이드 니들건 | `Gun_Needle` | **52.5%** | 탄 관통 0.35 × 1.5 (⑤와 동일 수치대). |
| 7 | 독침 발사기 | `Gun_ToxicNeedle` | **52.5%** | ⑥과 동일 계열. |
| 8 | 니들 런처 | `Gun_NeedleLauncher` | **52.5%** | 제작용; ⑥과 같은 `Bullet_NeedleGun`. |
| 9 | 볼트 액션 라이플 | `Gun_BoltActionRifle` | **40.5%** | 피해 18 → 27% × 1.5. |
| 10 | 헤비 차지 블래스터 | `Gun_ChargeBlasterHeavy` | **33.75%** | 피해 15 → 22.5% × 1.5. |

**참고:** 5~8위는 **표시 수치가 같아** 순위를 인위로 나눈 것이다. 동순위로 보고 묶어도 된다.

---

## 4. 근접 무기 — 관통 상위 10 (전설, 「한 타」 최대 기준)

근접은 **공격(도구)마다** 관통이 다르다. 아래는 **그 무기에서 나오는 최대 관통 타** 기준이며, 재료 미지정 무기는 **날 공격 = 플라스틸/우라늄(Sharp 1.1), 둔기 = 우라늄/제이드(Blunt 1.5)** 등 **배율이 높은 쪽**으로 가정했다.

| 순위 | 무기 (라벨 기준) | DefName | 대략 최대 관통 (전설) | 비고 |
|:----:|------------------|---------|------------------------|------|
| 1 | 모노소드 | `MeleeWeapon_MonoSword` | **148.5%** | 찌르기/베기 `armorPenetration` 0.9 × 1.65. |
| 2 | 페르소나 모노소드 | `MeleeWeapon_MonoSwordBladelink` | **148.5%** | ①과 동일 공식(위력·쿨다운은 다름). |
| 3 | 제우스해머 | `MeleeWeapon_Zeushammer` | **~115%** | 머리 **둔기**만 명시 관통 없음 → 피해×0.015×1.65×둔기재 배율(1.5 가정). |
| 4 | 페르소나 제우스해머 | `MeleeWeapon_ZeusHammerBladelink` | **~115%** | ③과 구조 동일. |
| 5 | 창 | `MeleeWeapon_Spear` | **82.5%** | 끝 찌르기 고정 0.5 × 1.65. |
| 6 | 워해머 | `MeleeWeapon_Warhammer` | **~74%** | 머리 둔기 power 20, 둔기 재료 1.5 가정. |
| 7 | 롱소드 | `MeleeWeapon_LongSword` | **~62.7%** | 찌르기/베기 power 23 × 날 배율 1.1 × 0.015 × 1.65. |
| 8 | 페르소나 플라즈마소드 | `MeleeWeapon_PlasmaSwordBladelink` | **~62.7%** | 날/끝 power 23 — ⑦과 비슷한 급. |
| 9 | 메이스 | `MeleeWeapon_Mace` | **~58.2%** | 머리 둔기 15.7 × … |
| 10 | 플라즈마소드 | `MeleeWeapon_PlasmaSword` | **~57.2%** | 날/끝 power 21. |

---

## 5. 요약

- **원거리 단일 탄 관통 최상단**은 **헬스피어 캐논**, 그 다음은 **Odyssey 빔 리피터**, **차지 랜스**, **스나이퍼 라이플** 순이다.
- **근접은 Def에 고정 관통이 박힌 모노소드 계(0.9)** 가 압도적이며, 그 다음은 **제우스해머 머리(고둔기 피해→0.015 스케일)** , **창(0.5 고정)** 순이다.
- 원거리는 **관통 품질 배율(1.5)** 과 **피해-연동 0.015 규칙**이 분리되어 있어, “피해 높은 총 = 관통도 자동으로 품질만큼 오른다”가 **아니다** (피해 쪽 `GetDamageAmount(null)` 호출).

---

## 6. 한계

- 실전 DPS·명중·사거리·연사는 포함하지 않음 (**관통 수치만** 비교).
- 근접 **“평균 관통”** (`MeleeWeapon_AverageArmorPenetration`)은 공격 가중 평균이라, 표의 “최대 타” 순위와 다를 수 있음.
- DLC 미소지 시 **빔 리피터** 등 일부는 해당 없음.

---

*작성: Ratkin 프로젝트 분석용 보고서. Git 작업 미포함.*

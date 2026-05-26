# RatHolic Gun Legacy StatPart / Custom Stat — Disabled Memo

**Tags:** RatHolic Gun RangedCooldownFactor VEF StatPart RK_Stat_RangeCoolDown Legacy Disabled

**배경:** 가열 쿨다운을 `RK_Stat_*` + `RangedWeapon_Cooldown` StatPart 대신, 헤딥 `RangedCooldownFactor`(바닐라)로 전환함 (`Weapon_Range.xml`). VEF 호환.

**현재 유효 경로:** `RK_Hediff_RatHolicGunSpooling` → `statFactors` / `RangedCooldownFactor` → `VerbProperties.AdjustedCooldown`의 폰 배수 곱.

---

## 주석 처리함 (미사용)

| 항목 | 경로 | 비고 |
|------|------|------|
| StatPart (초 감소) | `Project/1.6/Source/RatHolicGun/StatPart_RatHolicGunSpooling_Cooldown.cs` | `#if false`, 패치에도 원래 미등록 |
| StatPart (배수) | `Project/1.6/Source/RatHolicGun/StatPart_RatHolicGunSpooling_Cooldown_Multiplier.cs` | `#if false` |
| XML 패치 | `Project/1.6/Patches/RatHolicGun_Stats_Patch.xml` | 전체 `<!-- -->` |
| 커스텀 StatDef | `Project/1.6/Defs/Stats/Stats_RatHolicGun.xml` | 전체 `<!-- -->` |
| DefOf | `Project/1.6/Source/DefOf.cs` | `RK_Stat_RangeCoolDown*` 필드 주석 |
| 번역 (4파일) | `Contents/Languages/*/DefInjected/StatDef/` 내 `RK_Stat_RangeCoolDown*` 노드 | `<!-- -->` |

---

## 계속 사용 중 (참고)

- `Verb_RatHolicGun.cs`, `VerbProperties_RatHolicGun.cs`
- `Comp_RatHolicGun.cs`, `HediffCompProperties_RatHolicGun`
- `Weapon_Range.xml` — `RK_Hediff_RatHolicGunSpooling` + `RangedCooldownFactor`
- `RatkinHediffDefOf.RK_Hediff_RatHolicGunSpooling` (Verb/Comp에서 참조)

---

## 복구 시

1. `Weapon_Range.xml` 헤딥을 `RK_Stat_RangeCoolDownMultiplier` 등으로 되돌릴지 결정
2. 아래 주석 해제 후 StatPart 패치·Def·DefOf·번역 복원
3. VEF와 StatPart 충돌 여부 재검증

**삭제는 하지 않음** — 주석만 해제하면 롤백 가능.

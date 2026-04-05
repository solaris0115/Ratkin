ThingsDefs·shield defName 도탄 확률 (병합 statBases, 날/둔/열 모두 있을 때만 표 행; AP0, Rand≤방어도, 근접 가산/배율 없음 — 순수 방패·재료·품질만)

강철 평범 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|20.00%|20.00%|5.00%|
|RK_MediumShield_Second|46.30%|31.65%|37.20%|
|RK_TowerShield_Second|64.50%|44.75%|53.00%|

강철 완벽 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|29.00%|29.00%|7.25%|
|RK_MediumShield_Second|67.14%|45.89%|53.94%|
|RK_TowerShield_Second|93.53%|64.89%|76.85%|

강철 전설 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|36.00%|36.00%|9.00%|
|RK_MediumShield_Second|83.34%|56.97%|66.96%|
|RK_TowerShield_Second|116.10%|80.55%|95.40%|

플라스틸 평범 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|20.00%|20.00%|5.00%|
|RK_MediumShield_Second|55.18%|35.35%|39.05%|
|RK_TowerShield_Second|77.70%|50.25%|55.75%|

플라스틸 완벽 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|29.00%|29.00%|7.25%|
|RK_MediumShield_Second|80.01%|51.26%|56.62%|
|RK_TowerShield_Second|112.66%|72.86%|80.84%|

플라스틸 전설 (근접 반영 없음 · 순수 방패)

|ThingDef|Sharp|Blunt|Heat|
|--------|-----|-----|----|
|RK_SmallShield_Second|36.00%|36.00%|9.00%|
|RK_MediumShield_Second|99.32%|63.63%|70.29%|
|RK_TowerShield_Second|139.86%|90.45%|100.35%|

---

참고·상대 근접 관통 (도탄식 `Rand ≤ 방어도 − AP` 에서 차감되는 AP; 전설·평범·플라스틸 등 예시)

|기준|관통|
|---|---|
|단분자검 전설·찌르기·날 (MeleeWeapon_MonoSword Stab/Cut, armorPenetration 0.9)|148.50%|
|RK_HeavyLance 플라스틸 전설·찌르기 (Stab, power 32)|87.12%|
|RK_LongSword 정원사검 플라스틸 평범·찌르기 (Stab, power 23)|37.95%|
|RK_TwoHanded 양손검 플라스틸 평범·베기 (Cut, power 27)|44.55%|
|RK_HeavyLance 플라스틸 평범·찌르기 (Stab, power 32)|52.80%|
|장검 MeleeWeapon_LongSword 플라스틸 평범·찌르기 (Stab, power 23)|37.95%|

---

참고·갑옷 방어도 (`(base + stuffMult × stuffPower) × quality`)

**림월드 판금갑옷 (Apparel_PlateArmor)**

|조건|Sharp|Blunt|Heat|
|---|-----|-----|----|
|강철 평범|81.00%|40.50%|54.00%|
|플라스틸 전설|184.68%|89.10%|105.30%|

**판금갑옷 (RK_Plate)**

|조건|Sharp|Blunt|Heat|
|---|-----|-----|----|
|강철 평범|81.70%|58.35%|37.80%|
|플라스틸 전설|174.28%|116.37%|73.71%|

**불사조갑옷 (Phoenix)**

|조건|Sharp|Blunt|Heat|
|---|-----|-----|----|
|평범|115.00%|45.00%|75.00%|
|전설|207.00%|81.00%|135.00%|

**정원사옷 (RK_GaurdenUniform)**

|조건|Sharp|Blunt|Heat|
|---|-----|-----|----|
|평범가죽 평범|20.25%|46.00%|37.50%|
|트럼보가죽 전설|93.60%|88.20%|67.50%|

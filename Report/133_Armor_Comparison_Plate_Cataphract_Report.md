# 갑옷 4종 방어력/단열 비교 (AI 참조용)

> **태그**: Armor Plate Cataphract RK_Plate RK_SpaceArmor ArmorRating Insulation Steel Plasteel 품질 비교

## 1. 대상 갑옷

| 약칭 | defName | 소재 방식 | 테크 | 비고 |
|------|---------|-----------|------|------|
| 바닐라 판금 | Apparel_PlateArmor | stuff (Metallic/Woody) | Medieval | 기본 AR 없음, 소재 전적 의존 |
| 랫킨 판금 | RK_Plate | stuff (Metallic) + 고정비용 Steel 60 | Medieval | 기본 AR 있음 + 소재 보정 |
| 랫킨 우주 | RK_Apparel_SpaceArmor | 고정 소재 | Spacer | Plasteel 150 + Uranium 50 등 |
| 바닐라 카타 | Apparel_ArmorCataphract | 고정 소재 | Spacer | Plasteel 150 + Uranium 50 |

---

## 2. Def 원본 수치

### 2-1. 기본 ArmorRating / Insulation (품질·소재 적용 전)

| 갑옷 | Sharp | Blunt | Heat | Ins_Cold | Ins_Heat | StuffEffMul_Armor | StuffEffMul_Ins_Cold | StuffEffMul_Ins_Heat |
|------|-------|-------|------|----------|----------|-------------------|---------------------|---------------------|
| 바닐라 판금 | 0 | 0 | 0 | 0 | 0 | 0.90 | 1.0 | 0 |
| 랫킨 판금 | 0.25 | 0.30 | 0 | 0 | 0 | 0.63 | 0 | 0 |
| 랫킨 우주 | 1.10 | 0.80 | 1.00 | 70 | 70 | — | — | — |
| 바닐라 카타 | 1.20 | 0.50 | 0.60 | 70 | 12 | — | — | — |

- 바닐라 판금은 기본 AR이 0이라 방어력 전부가 소재에서 나온다. StuffEffMul 0.9로 소재 방어력의 90%를 반영.
- 랫킨 판금은 기본 AR(Sharp 0.25, Blunt 0.30)이 있어 Steel로 만들어도 바닐라보다 기본기가 있다. 대신 StuffEffMul 0.63으로 소재 반영률이 낮다.
- 우주급 갑옷 2종은 고정 소재라 stuff 관련 값이 없다. 품질만 영향.

### 2-2. 소재 StuffPower

| 소재 | Armor_Sharp | Armor_Blunt | Armor_Heat | Ins_Cold | Ins_Heat |
|------|-------------|-------------|------------|----------|----------|
| Steel | 0.90 | 0.45 | 0.60 | 3 | 0 |
| Plasteel | 1.14 | 0.55 | 0.65 | 3 | 0 |

금속 소재는 단열(Insulation) 기여가 매우 낮다. Cold 3, Heat 0. 판금 갑옷으로는 단열을 기대하기 어렵다.

### 2-3. 품질 배율

방어력과 단열은 서로 다른 품질 배율을 사용한다.

| 품질 | 방어력(Armor) 배율 | 단열(Insulation) 배율 |
|------|-------------------|---------------------|
| 조악 | 0.50 | 0.80 |
| 열등 | 0.65 | 0.90 |
| 보통 | 0.80 | 1.00 |
| 양호 | 0.90 | 1.10 |
| 우수 | 1.00 | 1.20 |
| 걸작 | 1.25 | 1.50 |
| 전설 | 1.80 | 1.80 |

방어력은 보통 품질이 0.80(=우수의 80%)이지만, 단열은 보통이 1.00으로 기준값이다. 방어력은 품질에 더 민감하다.

---

## 3. 계산식

### stuff 사용 갑옷 (바닐라 판금, 랫킨 판금)

```
ArmorRating = (기본값 + StuffEffMul_Armor × StuffPower_Armor) × 방어력 품질배율
Insulation  = (기본값 + StuffEffMul_Ins  × StuffPower_Ins)  × 단열 품질배율
```

### 고정 소재 갑옷 (랫킨 우주, 바닐라 카타)

```
ArmorRating = 기본값 × 방어력 품질배율
Insulation  = 기본값 × 단열 품질배율
```

---

## 4. 방어력 계산 결과

### 4-1. 바닐라 판금 (Apparel_PlateArmor) — stuff 사용

| 소재 | 품질 | Sharp | Blunt | Heat |
|------|------|-------|-------|------|
| Steel | 보통 | 0.65 | 0.32 | 0.43 |
| Steel | 전설 | 1.46 | 0.73 | 0.97 |
| Plasteel | 보통 | 0.82 | 0.40 | 0.47 |
| Plasteel | 전설 | 1.85 | 0.89 | 1.05 |

### 4-2. 랫킨 판금 (RK_Plate) — stuff 사용

| 소재 | 품질 | Sharp | Blunt | Heat |
|------|------|-------|-------|------|
| Steel | 보통 | 0.65 | 0.47 | 0.30 |
| Steel | 전설 | 1.47 | 1.05 | 0.68 |
| Plasteel | 보통 | 0.77 | 0.52 | 0.33 |
| Plasteel | 전설 | 1.74 | 1.16 | 0.74 |

### 4-3. 랫킨 우주 (RK_Apparel_SpaceArmor) — 고정 소재

| 품질 | Sharp | Blunt | Heat |
|------|-------|-------|------|
| 보통 | 0.88 | 0.64 | 0.80 |
| 전설 | 1.98 | 1.44 | 1.80 |

### 4-4. 바닐라 카타프락토이 (Apparel_ArmorCataphract) — 고정 소재

| 품질 | Sharp | Blunt | Heat |
|------|-------|-------|------|
| 보통 | 0.96 | 0.40 | 0.48 |
| 전설 | 2.16 | 0.90 | 1.08 |

---

## 5. 단열 계산 결과

### 5-1. 바닐라 판금 — stuff 사용

StuffEffMul_Ins_Cold 1.0, Heat 0. 금속 소재의 Ins_Cold는 3, Ins_Heat는 0.

| 소재 | 품질 | Ins_Cold | Ins_Heat |
|------|------|----------|----------|
| Steel | 보통 | 3.0 | 0 |
| Steel | 전설 | 5.4 | 0 |
| Plasteel | 보통 | 3.0 | 0 |
| Plasteel | 전설 | 5.4 | 0 |

단열이 사실상 없다. 금속 소재는 Ins_Cold 기여가 3뿐이고 Heat는 0.

### 5-2. 랫킨 판금 — stuff 사용

StuffEffMul_Ins_Cold/Heat 모두 0. 소재 단열을 전혀 반영하지 않는다.

| 소재 | 품질 | Ins_Cold | Ins_Heat |
|------|------|----------|----------|
| 전체 | 전체 | **0** | **0** |

랫킨 판금은 어떤 소재·품질이든 단열 0. 순수 방어 장비.

### 5-3. 랫킨 우주 — 고정 소재

| 품질 | Ins_Cold | Ins_Heat |
|------|----------|----------|
| 보통 | 70 | 70 |
| 전설 | 126 | 126 |

Cold와 Heat 단열이 동일하게 높다. 바닐라 카타와의 가장 큰 차별점.

### 5-4. 바닐라 카타 — 고정 소재

| 품질 | Ins_Cold | Ins_Heat |
|------|----------|----------|
| 보통 | 70 | 12 |
| 전설 | 126 | 21.6 |

Cold 단열은 랫킨 우주와 동일하지만, Heat 단열이 12로 매우 낮다. 사막/고온 환경에서 불리.

---

## 6. 종합 비교표 (보통 품질)

### 방어력 (보통, 소재별 최선 = Plasteel)

| 갑옷 | 소재 | Sharp | Blunt | Heat | Ins_Cold | Ins_Heat |
|------|------|-------|-------|------|----------|----------|
| 바닐라 판금 | Plasteel | 0.82 | 0.40 | 0.47 | 3.0 | 0 |
| 랫킨 판금 | Plasteel | 0.77 | 0.52 | 0.33 | 0 | 0 |
| 랫킨 우주 | 고정 | 0.88 | 0.64 | 0.80 | 70 | 70 |
| 바닐라 카타 | 고정 | 0.96 | 0.40 | 0.48 | 70 | 12 |

### 방어력 (전설, 소재별 최선 = Plasteel)

| 갑옷 | 소재 | Sharp | Blunt | Heat | Ins_Cold | Ins_Heat |
|------|------|-------|-------|------|----------|----------|
| 바닐라 판금 | Plasteel | 1.85 | 0.89 | 1.05 | 5.4 | 0 |
| 랫킨 판금 | Plasteel | 1.74 | 1.16 | 0.74 | 0 | 0 |
| 랫킨 우주 | 고정 | 1.98 | 1.44 | 1.80 | 126 | 126 |
| 바닐라 카타 | 고정 | 2.16 | 0.90 | 1.08 | 126 | 21.6 |

---

## 7. 착용 패널티 비교

| 갑옷 | 이속 | 회피 | 명중 | 기타 |
|------|------|------|------|------|
| 바닐라 판금 | -0.8 | — | — | — |
| 랫킨 판금 | -0.65 | -10% | — | — |
| 랫킨 우주 | -0.2 | — | — | 인화성 -0.68 |
| 바닐라 카타 | -0.5 | — | — | — |

---

## 8. 분석

### Sharp 방어력
바닐라 카타가 가장 높고(기본 1.2), 랫킨 우주가 근소하게 뒤따른다(1.1). 판금 계열은 Plasteel 전설이어야 우주급 보통에 겨우 근접한다.

### Blunt 방어력
랫킨 계열이 바닐라보다 우수하다. 랫킨 판금은 기본 Blunt 0.30으로 바닐라 판금(0)보다 높고, 랫킨 우주(0.8)는 바닐라 카타(0.5)보다 크게 높다. 랫킨 갑옷이 둔기 방어에 특화된 설계.

### Heat 방어력
랫킨 우주(1.0)가 바닐라 카타(0.6)보다 훨씬 높다. 판금 계열에서는 바닐라 판금이 랫킨 판금보다 Heat에 강하다(StuffEffMul 차이).

### 단열
- 판금 계열: 사실상 단열 없음. 바닐라 판금이 미미한 Cold 단열(3~5), 랫킨 판금은 0.
- 우주급: 랫킨 우주가 Cold/Heat 모두 70으로 압도적. 바닐라 카타는 Cold 70이지만 Heat 12로 고온에 취약.

### 패널티
랫킨 우주(-0.2 이속)가 바닐라 카타(-0.5)보다 가볍다. 판금 계열은 바닐라(-0.8)가 가장 무겁고, 랫킨(-0.65 + 회피 -10%)은 이속은 나은 대신 회피 패널티가 있다.

## 소스
- `RimworldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml` — Apparel_PlateArmor
- `RimworldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml` — Apparel_ArmorCataphract, ApparelArmorCataphractBase
- `Project/1.6/Defs/ThingsDefs/Apparel_Armor.xml` — RK_Plate
- `Project/1.6/Defs/ThingsDefs/Apparel_Spacer.xml` — RK_Apparel_SpaceArmor
- `RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml` — Steel, Plasteel StuffPower

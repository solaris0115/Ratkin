# Ratkin 의상별 가죽/머플로 착용 온도 범위

> **Tags**: Ratkin Apparel Leather Bluefur Muffalo ComfyTemperature StuffPower 착용온도 가죽 머플로  
> **작성일**: 2026-03-04  
> **관련 보고서**: [118_Ratkin_Apparel_ComfyTemperature_Report.md](118_Ratkin_Apparel_ComfyTemperature_Report.md)

---

## 1. 개요

**랫킨 기본 쾌적 온도**: 21°C ~ 26°C

재료 의존 의상은 제작 시 사용한 재료의 StuffPower에 따라 착용 시 온도 범위가 달라집니다.  
본 보고서는 **일반 가죽(Leather)**과 **머플로 가죽(Bluefur)** 기준으로 비교합니다.

### 재료별 StuffPower


| 재료               | StuffPower_Cold | StuffPower_Heat | 비고                            |
| ---------------- | --------------- | --------------- | ----------------------------- |
| Leather (일반 가죽)  | 16              | 16              | Leather_Plain                 |
| Bluefur (머플로 가죽) | 20              | 16              | Leather_Bluefur, Muffalo에서 채취 |


**계산 공식**:

- 최소 적정 온도 = 21 - Insulation_Cold
- 최대 적정 온도 = 26 + Insulation_Heat
- Insulation = StuffPower × StuffEffectMultiplier (+ 고정값)

---

## 2. 재료 의존 단열 (StuffEffectMultiplier) 의상

형식: **소재 온도 (착용 온도)** — 단열값 (착용 시 쾌적 범위)

| defName (Cold/Heat 계수)              | 가죽 | 머플로 |
| ----------------------------------- | ---- | ------ |
| RK_ApronSkirt (0.20 / 0.10)         | 3.2 / 1.6 (18 ~ 28°C) | 4 / 1.6 (17 ~ 28°C) |
| RK_ApronSkirtChildren (0.20 / 0.10) | 3.2 / 1.6 (18 ~ 28°C) | 4 / 1.6 (17 ~ 28°C) |
| RK_SummerDress (0.10 / 0.70)        | 1.6 / 11.2 (19 ~ 37°C) | 2 / 11.2 (19 ~ 37°C) |
| RK_Muffler (0.65 / 0.25)            | 10.4 / 4 (11 ~ 30°C) | 13 / 4 (8 ~ 30°C) |
| RK_WoolenHat (0.50 / 0)             | 8 (13 ~ 26°C) | 10 (11 ~ 26°C) |
| RK_WorkerWear (0.30 / 0.20)         | 4.8 / 3.2 (16 ~ 29°C) | 6 / 3.2 (15 ~ 29°C) |
| RK_Coif (0.15 / 0.10)               | 2.4 / 1.6 (19 ~ 28°C) | 3 / 1.6 (19 ~ 28°C) |
| RK_ResearchGown (0.30 / 0.15)       | 4.8 / 2.4 (16 ~ 28°C) | 6 / 2.4 (15 ~ 28°C) |
| RK_ExplorerWear (0.20 / 0.40)       | 3.2 / 6.4 (18 ~ 32°C) | 4 / 6.4 (17 ~ 32°C) |
| RK_ExplorerHat (0.15 / 0.25)        | 2.4 / 4 (19 ~ 30°C) | 3 / 4 (19 ~ 30°C) |
| RK_ChefSuit (0.25 / 0.25)           | 4 / 4 (17 ~ 30°C) | 5 / 4 (16 ~ 30°C) |
| RK_ChefHat (0.15 / 0.15)            | 2.4 / 2.4 (19 ~ 28°C) | 3 / 2.4 (19 ~ 28°C) |
| RK_GaurdenUniform (0.40 / 0.25)      | 6.4 / 4 (15 ~ 30°C) | 8 / 4 (13 ~ 30°C) |
| RK_OrderUniform (0.40 / 0.40)       | 6.4 / 6.4 (15 ~ 32°C) | 8 / 6.4 (13 ~ 32°C) |
| RK_BulletProofHelmet (0.25 / 0.15)  | 4 / 2.4 (17 ~ 28°C) | 5 / 2.4 (16 ~ 28°C) |
| RK_FlatColorCoat (0.40 / 0.35)      | 6.4 / 5.6 (15 ~ 32°C) | 8 / 5.6 (13 ~ 32°C) |
| RK_FrillOnepiece (0.35 / 0.40)      | 5.6 / 6.4 (15 ~ 32°C) | 7 / 6.4 (14 ~ 32°C) |
| RK_SistersDerss (0.35 / 0.45)       | 5.6 / 7.2 (15 ~ 33°C) | 7 / 7.2 (14 ~ 33°C) |
| RK_SistersVeil (0.25 / 0.15)        | 4 / 2.4 (17 ~ 28°C) | 5 / 2.4 (16 ~ 28°C) |
| RK_BattleSuit (0.55 / 0.45)         | 8.8 / 7.2 (12 ~ 33°C) | 11 / 7.2 (10 ~ 33°C) |
| RK_Apparel_GasMask (0.55 / 0.45)    | 8.8 / 7.2 (12 ~ 33°C) | 11 / 7.2 (10 ~ 33°C) |
| RK_HeadBand (0.15 / 0.15)           | 2.4 / 2.4 (19 ~ 28°C) | 3 / 2.4 (19 ~ 28°C) |
| RK_SantaHat (0.65 / 0)              | 10.4 (11 ~ 26°C) | 13 (8 ~ 26°C) |


---

## 3. 고정 + 재료 의존 혼합 의상

형식: **소재 온도 (착용 온도)** — 단열값 (착용 시 쾌적 범위)

| defName (고정 Cold/Heat, Cold/Heat 배수) | 가죽 | 머플로 |
| ----------------------------------- | ---- | ------ |
| RK_Cardigan (8 / 0, 0.95 / 0.30)     | 23.2 / 4.8 (-2 ~ 31°C) | 27 / 4.8 (-6 ~ 31°C) |
| RK_WinterRobe (8 / 0, 1.20 / 0)      | 27.2 (-6 ~ 26°C) | 32 (-11 ~ 26°C) |
| RK_RoyalRobe (12 / 0, 1.40 / 0)      | 34.4 (-13 ~ 26°C) | 40 (-19 ~ 26°C) |
| RK_SantaRobe (13 / 0, 1.30 / 0)      | 33.8 (-13 ~ 26°C) | 39 (-18 ~ 26°C) |


---

## 4. 요약

- **머플로 가죽(Bluefur)**은 냉기 StuffPower가 20으로 일반 가죽(16)보다 높아, **냉기 단열이 있는 의상**에서 착용 시 온도 범위가 더 넓어짐.
- **열기 단열**은 두 재료 모두 16으로 동일하여, Heat 계수만 있는 의상은 재료 차이가 없음.
- **냉기 계수가 큰 의상**일수록 머플로 가죽 사용 시 최소 적정 온도가 더 낮아짐 (예: RK_SantaHat, RK_WinterRobe).


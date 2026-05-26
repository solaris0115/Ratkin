# KingHamster Muffalo Animal Comparison Review Report
> Tags: KingHamster, Hamstrox, Muffalo, Animal, Def Review, Balance, Pack Animal, Wool, Leather

## 개요

`RK_KingHamster` (햄스트록스)를 바닐라 `Muffalo` (머팔로)와 비교 분석한 리뷰 보고서입니다.
두 동물 모두 **무리 동물 + 짐꾼 + 양털 생산** 역할을 공유하며, 햄스트록스는 랫킨 전용 가축으로 설계되었습니다.

---

## 1. 수정 이력 (2026-03-18)

### 적용된 변경사항

| # | 항목 | 변경 전 | 변경 후 | 사유 |
|---|------|---------|---------|------|
| 1 | MeatAmount | 50 (명시 고정) | 제거 (bodySize 기반 자동 계산) | bodySize 기반 통일 |
| 2 | ComfyTemperatureMin | -45 | -60 | 극한 환경 적응 강화 |
| 3 | ComfyTemperatureMax | 50 | 60 | 극한 환경 적응 강화 |
| 4 | FilthRate | 미지정 | 16 | 머팔로와 동일 |
| 5 | baseHungerRate | 0.8 | 0.65 | 과도한 식사량 하향 |
| 6 | gestationPeriodDays | 16 | 8 | 절반으로 축소 (머팔로 6.66 대비 약간 긴 수준) |
| 7 | roamMtbDays | 미지정 | 3 | 머팔로(2)보다 낮은 배회 빈도 |
| 8 | soundEating | 미지정 | Herbivore_Eat | 먹는 소리 추가 |
| 9 | 앞발 label | left/right hand | left/right paw | 쥐 계열 발톱 스타일로 변경 |
| 10 | 앞발 labelNoLocation | 미지정 | paw | 추가 |
| 11 | 울 Insulation_Cold | 16 | 30 | 보온 특화로 변경 |
| 12 | 울 Insulation_Heat | 26 | 10 | 보온 특화 (방열 하향) |
| 13 | 울 description | 통풍성과 보온성 | 보온성 특화 | 수치와 일치하도록 |
| 14 | 가죽 Insulation_Cold | 12 | 10 | 방열 특화 (보온 하향) |
| 15 | 가죽 Insulation_Heat | 10 | 30 | 방열 특화로 변경 |
| 16 | 가죽 Beauty | 미지정 | 1.3 | 머팔로 블루퍼와 동일 |
| 17 | 가죽 description | 체온 조절에 좋습니다 | 통풍성 덕분에 더운 지방에서 체온 조절에 좋습니다 | 방열 특화 반영 |

---

## 2. 최종 스탯 비교표

### ThingDef 기본 스탯

| 항목 | 햄스트록스 | 머팔로 | 비고 |
|------|-----------|--------|------|
| MoveSpeed | 4.5 | 4.5 | 동일 |
| MarketValue | 450 | 300 | 프리미엄 모드 동물 |
| ComfyTemperatureMin | **-60** | -55 | 더 추위 견딤 |
| ComfyTemperatureMax | **60** | 45 | 훨씬 더위 견딤 |
| FilthRate | **16** | 16 | 동일 |
| Wildness | 0.0 | 0.6 | 완전 가축화 |
| MeatAmount | 자동 계산 (~126) | 자동 계산 (~168) | bodySize 기반 |

### Race 속성

| 항목 | 햄스트록스 | 머팔로 | 비고 |
|------|-----------|--------|------|
| baseBodySize | 1.8 | 2.4 | 더 작은 체구 |
| baseHealthScale | 1.6 | 1.75 | 약간 약함 |
| baseHungerRate | **0.65** | 0.535 | 약간 높음 (체구 대비 적정) |
| gestationPeriodDays | **8** | 6.66 | 약간 긴 임신 |
| trainability | Intermediate | None | 훈련 가능 |
| roamMtbDays | **3** | 2 | 덜 배회 |
| soundEating | **Herbivore_Eat** | Herbivore_Eat | 동일 |
| manhunterOnDamage | 0 | 0.1 | 절대 광폭화 안 함 |

### 전투 도구

| 공격 | 햄스트록스 | 머팔로 | 비고 |
|------|-----------|--------|------|
| Head (Blunt) | 11 / cd 2.0 | 13 / cd 2.6 | 비슷한 DPS |
| Left/Right **paw** (Scratch) | 8 / cd 1.7 | 10 / cd 2.0 (Blunt+Poke) | 쥐 스타일 할퀴기 |
| Bite | 8 / cd 1.7 / 0.5x | 10 / cd 2.0 / 0.5x | 비슷한 DPS |

### 울 (Wool) — 보온 특화

| 항목 | RK_Wool_KingHamster | WoolMuffalo | 비고 |
|------|---------------------|-------------|------|
| Insulation_Cold | **30** | 28 | 머팔로보다 약간 높은 보온 |
| Insulation_Heat | **10** | 12 | 방열은 낮음 |
| MarketValue | 2.7 (기본값) | 2.7 (기본값) | 동일 |

### 가죽 (Leather) — 방열 특화

| 항목 | RK_Leather_KingHamster | Leather_Bluefur | 비고 |
|------|------------------------|-----------------|------|
| Insulation_Cold | **10** | 20 | 보온 낮음 |
| Insulation_Heat | **30** | 16 (기본값) | 바닐라 최고 수준 방열 |
| MarketValue | 2.3 | 2.3 | 동일 |
| Beauty | **1.3** | 1.3 | 동일 |
| Armor_Sharp | 0.81 (기본값) | 0.81 (기본값) | 동일 (머팔로급) |

---

## 3. 디자인 컨셉 정리

### 울과 가죽의 역할 분담
```
울 (Wool)  → 보온 특화: Cold 30 / Heat 10 → 극지방 의복 재료
가죽 (Fur) → 방열 특화: Cold 10 / Heat 30 → 사막 의복 재료
```

### 머팔로 대비 포지셔닝
- **체구**: 더 작음 (1.8 vs 2.4) → 고기/가죽 수량 적음
- **온도 적응**: 극한 환경 양쪽 모두 우수 (-60~60 vs -55~45)
- **가축성**: 완전 가축화 (Wildness 0, 광폭화 0, 훈련 가능)
- **생산물**: 울(보온) + 가죽(방열)로 양면 활용 가능
- **번식**: 약간 느림 (8일 vs 6.66일)
- **전투**: 약간 약함 (combatPower 65 vs 100)

---

*보고서 작성일: 2026-03-18*
*비교 대상: RK_Animal.xml vs Races_Animal_CowGroup.xml (Core)*

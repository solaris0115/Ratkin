# 랫킨 제노타입·Gene 수정 및 대사효율 보고서

<!-- Ratkin Xenotype Gene Metabolism BiostatMet 수정 보고 -->

## 1. 수정 요약

### 1.1 제노타입 수정 (RK_XenoType_Ratkin)

| 항목 | 변경 내용 |
|------|----------|
| **부분 항독성 폐** (ToxicEnvironmentResistance_Partial) | 제거 |
| **강한 면역력** (Immunity_Strong) | 제거 |

### 1.2 Gene 수정

| Gene | 변경 내용 |
|------|----------|
| **RK_Gene_ThinTail** | 이동속도 +0.05 제거 → 외형만 (biostatMet -1 → 0) |
| **RK_Gene_Nimble** | 근접 회피율 1.25 → **1.3** 상향 |

---

## 2. 대사효율 (biostatMet) 계산

### 2.1 수정 후 유전자 목록 및 biostatMet

| 유전자 | biostatMet | 비고 |
|--------|------------|------|
| RK_Gene_SmallBody | +1 | 작은 체형 |
| RK_Gene_Nimble | -2 | 재빠름 (MeleeDodge ×1.3, MoveSpeed +0.1) |
| RK_Gene_PoorEyesight | +3 | 근시 |
| CaveDweller | -1 | 실내 거주자 |
| RK_Gene_FragilePain | +1 | 약한 통증 역치 |
| AptitudePoor_Construction | +1 | 나쁜 건설 |
| AptitudeStrong_Plants | -1 | 좋은 원예 |
| MinTemp_SmallIncrease | +1 | 냉기에 약함 |
| Skin_SheerWhite | 0 | 순백 피부 |
| Beard_NoBeardOnly | 0 | 수염 없음 |
| RK_Gene_LargeEars | 0 | 큰 귀 |
| RK_Gene_ThinTail | 0 | 얇은 꼬리 (외형만) |
| RK_Gene_CuteFace | 0 | 귀여운 얼굴 |

### 2.2 대사효율 합계

**음수 (대사 소비):**
- RK_Gene_Nimble: -2
- CaveDweller: -1
- AptitudeStrong_Plants: -1  
- **소계: -4**

**양수 (대사 절약):**
- RK_Gene_SmallBody: +1
- RK_Gene_PoorEyesight: +3
- RK_Gene_FragilePain: +1
- AptitudePoor_Construction: +1
- MinTemp_SmallIncrease: +1  
- **소계: +7**

**최종 대사효율: +7 + (-4) = +3**

---

## 3. 수정 전·후 비교

| 항목 | 수정 전 | 수정 후 |
|------|---------|---------|
| Immunity_Strong | 포함 (Met:-1) | **제거** |
| ToxicEnvironmentResistance_Partial | 포함 (Met:-1) | **제거** |
| RK_Gene_ThinTail MoveSpeed | +0.05 | **제거** |
| RK_Gene_ThinTail biostatMet | -1 | **0** |
| RK_Gene_Nimble MeleeDodgeChance | ×1.25 | **×1.3** |
| **대사효율 합계** | **0** | **+3** |

---

## 4. 결론

- **최종 대사효율: +3**
- 수정으로 인해 랫킨은 **기준(0)보다 3 포인트 적은 음식**을 소비합니다.
- 제거된 유전자(부분 항독성 폐, 강한 면역력)로 인한 대사 절약(+2)과 ThinTail 이동속도 제거로 인한 biostatMet 조정(+1)이 반영되었습니다.
- RK_Gene_Nimble 근접 회피율 상향(1.25→1.3)은 biostatMet 변경 없이 적용되었습니다.

---

## 5. 수정된 파일

- `Project/Biotech/Defs/GeneDefs/XenotypeDefs.xml`
- `Project/Biotech/Defs/GeneDefs/CustomGeneDefs.xml`

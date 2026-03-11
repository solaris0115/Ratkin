---
name: Parse Apparel Armor Script
overview: 기존 parse_ranged_weapons.py를 참고하여 방어구(Apparel) 데이터를 수집하는 Python 스크립트를 생성. 림월드 Core+DLC 및 랫킨 방어구의 방어력(Sharp/Blunt/Heat)을 전설 품질 기준으로 소재별(로우엔드/하이엔드) 최종 방어도를 계산하여 캐시 MD와 엑셀로 출력.
todos:
  - id: create-script
    content: scripts/parse_apparel_armor.py 스크립트 생성 - XML 파싱, 부모 상속, 방어력 계산, 소재 구분 로직 구현
    status: completed
  - id: output-cache-md
    content: 캐시 MD 출력 (.cursor/def-cache/apparel-armor.md) - DLC별 + 랫킨 방어구 테이블
    status: completed
  - id: output-excel
    content: 엑셀 출력 (ExelData/Apparel_Armor_Balancing.xlsx) - DLC별/랫킨 시트 구분
    status: completed
  - id: test-run
    content: 스크립트 실행 테스트 및 출력 확인
    status: completed
isProject: false
---

# 방어구 파싱 스크립트 생성 계획

## 핵심 로직

### 방어력 계산 공식 ([armor-rating-formula.md](.cursor/def-cache/armor-rating-formula.md) 기반)

```
최종 방어력 = (BaseArmor + StuffEffectMultiplierArmor x StuffPower_Armor) x QualityFactor(전설=1.80)
```

### 소재 구분 규칙


| stuffCategories                                            | 로우엔드                   | 하이엔드                     |
| ---------------------------------------------------------- | ---------------------- | ------------------------ |
| Metallic                                                   | Steel (강철)             | Plasteel (플라스틸)          |
| Leathery                                                   | Leather_Plain (평범한 가죽) | Leather_Thrumbo (트럼보 모피) |
| Fabric                                                     | Leather_Plain (평범한 가죽) | Leather_Thrumbo (트럼보 모피) |
| Fabric + Leathery 둘 다                                      | Leather_Plain (평범한 가죽) | Leather_Thrumbo (트럼보 모피) |
| 고정 소재 (stuffCategories 없음 AND costStuffCount 없음, 부모 체인 포함) | 계수 0, 고정방어력 x 1.80     | 계수 0, 고정방어력 x 1.80       |


### 소재별 StuffPower_Armor 값 (하드코딩)


| 소재              | Sharp | Blunt | Heat |
| --------------- | ----- | ----- | ---- |
| Steel           | 0.90  | 0.45  | 0.60 |
| Plasteel        | 1.14  | 0.55  | 0.65 |
| Leather_Plain   | 0.81  | 0.24  | 1.50 |
| Leather_Thrumbo | 2.08  | 0.36  | 1.50 |


## 출력 테이블 컬럼

```
defName | stuffCategory | StuffEffectMultiplierArmor | BaseArmor_Sharp | BaseArmor_Blunt | BaseArmor_Heat | LowEnd_Sharp | LowEnd_Blunt | LowEnd_Heat | HighEnd_Sharp | HighEnd_Blunt | HighEnd_Heat
```

- LowEnd/HighEnd = `(BaseArmor + StuffEffectMultiplierArmor x StuffPower) x 1.80` (전설 품질)
- 고정 소재: StuffEffectMultiplierArmor=0으로 표기, LowEnd = HighEnd = BaseArmor x 1.80

## 스크립트 구조 (참고: [scripts/parse_ranged_weapons.py](scripts/parse_ranged_weapons.py))

기존 스크립트에서 재활용할 부분:

- `collect_all_thingdefs()`: XML 파싱 및 부모 상속 시스템
- `resolve_def()`: 부모 체인 따라 stat 상속 해결
- `extract_stat_bases()`, `get_text()`, `get_float()` 등 유틸리티
- DLC별 분류 로직

새로 구현할 부분:

- **Apparel 식별**: `thingClass`가 `Apparel`이거나 부모 체인에서 `Apparel`을 상속하는 ThingDef (Abstract 제외)
- **stuffCategories / costStuffCount 상속 해결**: 부모 체인을 따라가며 확인
- **소재 구분**: stuffCategories 값에 따라 로우엔드/하이엔드 소재 결정
- **방어력 계산**: 공식 적용하여 최종 방어도 산출

## 출력 파일

1. **캐시 MD**: `.cursor/def-cache/apparel-armor.md` - YAML 헤더 + 마크다운 테이블
2. **엑셀**: `ExelData/Apparel_Armor_Balancing.xlsx` - 시트 구분 (Rimworld DLC별, Ratkin)


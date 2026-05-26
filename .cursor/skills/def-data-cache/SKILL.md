---
name: def-data-cache
description: Def 데이터 캐시 관리 스킬. 림월드/랫킨 Def 데이터를 카테고리별로 캐싱하고 조회한다. 밸런싱, 분석, 비교, 보고서 작성 시 자동으로 캐시를 우선 참조하여 토큰을 절약한다. 사용자가 무기, 방어구, 건물, 연구 등 Def 데이터 관련 작업을 요청할 때 사용.
---

# Def 데이터 캐시 시스템

## 핵심 워크플로우

모든 Def 데이터 관련 요청은 아래 3단계로 처리한다.

### 1단계: 캐시 확인

`.cursor/def-cache/` 디렉터리에서 해당 카테고리 캐시 파일을 찾는다.

```
.cursor/def-cache/{category}.md
```

### 2단계: 캐시 미스 시 수집/캐싱

캐시가 없거나 필요한 필드가 부족하면:
1. 원본 Def 탐색 (RimworldData/ + Project/1.6/Defs/)
2. 데이터 수집 후 캐시 파일 생성/갱신
3. 반드시 [reference.md](reference.md)의 캐시 포맷을 따른다

### 3단계: 작업 수행

캐시 데이터를 기반으로 요청된 작업(보고서, 밸런싱, 비교 등)을 수행한다.

## 카테고리 판별 규칙

사용자 요청에서 카테고리를 자동 판별한다:

| 키워드 | 카테고리 파일 |
|--------|--------------|
| 근접 무기, melee | `melee-weapons.md` |
| 원거리 무기, ranged, 총기, 총 | `ranged-weapons.md` |
| 방어구, apparel, 갑옷, 의류 | `apparel.md` |
| 건물, building, 설비 | `buildings.md` |
| 연구, research | `research.md` |
| 능력, ability, 스킬 | `abilities.md` |
| 종족, race | `races.md` |
| 전투 계수, 등급별 피해, 등급별 방어, quality coefficient | `combat-coefficients.md` |
| 방어력 계산, armor rating, 품질 소재 고정방어력, StuffEffectMultiplierArmor | `armor-rating-formula.md` |

판별 불가 시 사용자에게 카테고리명을 확인한다.

## 캐시 데이터 규칙

### 불변/가변

- **림월드 데이터** (RimworldData/): 불변. 절대 수정하지 않는다.
- **랫킨 데이터** (Project/1.6/Defs/): 가변. Def 변경 시 갱신 가능.
- **수정 요청 전까지 모든 캐시는 읽기 전용**으로 취급한다.

### 갱신 트리거

- 사용자가 "캐시 갱신" 명시적 요청
- 랫킨 Def 수정 작업 완료 후 해당 카테고리만 갱신
- "전체 갱신" 요청 시 모든 캐시 파일 재생성

### 필드 확장

캐시에 필요한 필드가 없으면:
1. 기존 캐시 데이터는 유지
2. 원본 Def에서 새 필드만 추가 수집
3. 캐시 메타데이터의 `fields` 업데이트

## 보고서 연계

캐시 기반 보고서 작성 시:
- `30_Report/` 폴더에 보고서 생성
- 보고서 상단에 캐시 출처와 갱신일 명시
- 기존 `30_Report/` 폴더의 관련 보고서도 참고

## 데이터 소스 경로

| 구분 | 경로 |
|------|------|
| 림월드 Core | `RimworldData/Core/Defs/` |
| 림월드 Royalty | `RimworldData/Royalty/Defs/` |
| 림월드 Ideology | `RimworldData/Ideology/Defs/` |
| 림월드 Biotech | `RimworldData/Biotech/Defs/` |
| 림월드 Anomaly | `RimworldData/Anomaly/Defs/` |
| 림월드 Odyssey | `RimworldData/Odyssey/Defs/` |
| 랫킨 Defs | `Project/1.6/Defs/` |

## 상세 참고

캐시 파일 포맷, 카테고리별 수집 가이드는 [reference.md](reference.md) 참조.

---
name: def-data-cache
description: RimWorld와 Ratkin Def 데이터 캐시를 우선 참조해 무기, 방어구, 건물, 연구, 능력, 종족, 전투 계수, 방어력 계산식을 분석, 비교, 밸런싱, 보고서화한다. 사용자가 Def 데이터, 밸런스, 수치 비교, 캐시 갱신, 무기/방어구/연구/종족 분석을 요청할 때 사용.
---

# Def Data Cache

## 핵심 워크플로우

1. `.codex/def-cache/`에서 요청 카테고리의 캐시 파일을 찾는다.
2. 없거나 필요한 필드가 부족하면 `RimworldData/`와 `Project/1.6/Defs/`에서 원본 Def를 수집한다.
3. 수집 또는 갱신이 필요하면 [references/reference.md](references/reference.md)의 포맷을 따른다.
4. 캐시와 원본 근거를 바탕으로 분석, 비교, 밸런싱, 보고서를 작성한다.

## 카테고리 판별

| 요청 키워드 | 캐시 파일 |
| --- | --- |
| 근접 무기, melee | `melee-weapons.md` |
| 원거리 무기, ranged, 총기, 총 | `ranged-weapons.md` |
| 방어구, apparel, 갑옷, 의류 | `apparel.md`, `apparel-armor.md`, `apparel-armor-face.md` |
| 건물, building, 설비 | `buildings.md` |
| 연구, research | `research.md` |
| 능력, ability, 스킬 | `abilities.md` |
| 종족, race | `races.md` |
| 전투 계수, quality coefficient | `combat-coefficients.md` |
| 방어력 계산, armor rating | `armor-rating-formula.md` |
| 피해 타입 매핑 | `damage-type-mapping.md` |
| 생물 방어력 | `creature-defense.md`, `creature-defense.json` |

판별이 애매하면 파일 검색으로 후보를 좁힌 뒤 필요한 경우 사용자에게 카테고리를 확인한다.

## 데이터 원칙

- `RimworldData/`는 원본 참고 데이터로 취급하고 수정하지 않는다.
- `Project/1.6/Defs/`는 Ratkin 작업 대상이다.
- 캐시는 사용자가 갱신을 요청하거나 Ratkin Def 수정 후 관련 카테고리만 갱신한다.
- 기존 캐시에 필드가 부족하면 기존 내용을 보존하고 새 필드만 추가한다.
- `.cursor/def-cache/`는 원본 Cursor 자료로 보존한다. Codex 작업에서는 `.codex/def-cache/`를 우선 사용한다.

## 원본 경로

| 구분 | 경로 |
| --- | --- |
| RimWorld Core | `RimworldData/Core/Defs/` |
| Royalty | `RimworldData/Royalty/Defs/` |
| Ideology | `RimworldData/Ideology/Defs/` |
| Biotech | `RimworldData/Biotech/Defs/` |
| Anomaly | `RimworldData/Anomaly/Defs/` |
| Odyssey | `RimworldData/Odyssey/Defs/` |
| Ratkin Defs | `Project/1.6/Defs/` |
| RimWorld source | `RimworldSource/` |
| AlienRace source | `AlienRace/` |

## 보고서

- 보고서 작성 시 `30_Report/`의 기존 관련 보고서를 먼저 검색한다.
- 새 보고서는 `30_Report/`에 작성하고 상단에 검색용 태그와 캐시 출처, 갱신일을 명시한다.
- 사용자 관점의 결론을 먼저 쓰고, 근거 표나 수식은 뒤에 둔다.

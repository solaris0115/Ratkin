# RK_Cardigan AlienRace 머리칼 가림 개선 보고서

<!-- RK_Cardigan AlienRace Hair North shellRenderedBehindHead Apparel Graphics -->

## 1. 문제

랫킨이 **RK_Cardigan**(가디건)을 착용한 상태에서 **북쪽(North)**을 바라볼 때 머리칼이 가려지는 현상이 발생합니다.

## 2. 원인 분석: 왜 목도리(RK_Muffler)는 가리지 않는가?

| 항목 | RK_Muffler (목도리) | RK_Cardigan (가디건) |
|------|---------------------|----------------------|
| bodyPartGroups | Neck만 | Torso, Neck, Shoulders, Arms |
| layers | **OuterClothing** (drawOrder 275) | **Shell** (drawOrder 200) |
| shellRenderedBehindHead | (해당 없음) | **false** (기본값) |

**핵심**: `Shell` 레이어 의류는 `DynamicPawnRenderNodeSetup_Apparel.cs`에서 **North 방향일 때 layer 88**로 고정됩니다. 이로 인해 머리/머리칼보다 **앞에** 그려져 가려집니다.

반면 **목도리**는 `OuterClothing` 레이어라 이 특수 처리 대상이 아니며, `bodyPartGroups`가 Neck만이라 텍스처도 목 주변에만 그려져 머리칼을 덮지 않습니다.

**Cape**(망토)처럼 `shellRenderedBehindHead=true`를 쓰면 이 North 전용 layer 88 처리가 적용되지 않고, 머리 뒤에 그려져 머리칼이 보입니다.

## 3. 해결: shellRenderedBehindHead 적용 (적용 완료)

`RK_Cardigan`에 `<shellRenderedBehindHead>true</shellRenderedBehindHead>`를 추가했습니다.

- **효과**: North에서 카디건이 머리/머리칼 **뒤**에 그려져 머리칼이 보임
- **참고**: Vanilla Cape도 동일 설정 사용

## 4. 대안 (텍스처 수정이 필요한 경우)

Def 수정만으로 해결되지 않을 때:

- **North 마스크**: `RK_Cardigan_northm.png` 추가 (머리칼 영역 검은색)
- **North 텍스처**: `RK_Cardigan_north.png`에서 머리 영역을 투명 처리

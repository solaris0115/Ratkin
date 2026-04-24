# 백팩 공통 속성 추상화 - 2025-11-09

## 작업 개요
- 요청 내용: 가방 1, 2, 3레벨 공통 사항을 추출해 `Name="RK_ApparelAttr_BackpackBase"` 추상 부모로 분리
- 목표: 공통 속성 재사용을 위한 추상 `ThingDef` 생성 및 기존 백팩 정의 정리

## 계획 (AI가 결정한 계획)
1. 기존 백팩 `ThingDef` 3종의 공통 속성 식별
2. 공통 항목을 포함한 추상 `ThingDef` 작성
3. 기존 정의를 새 추상 부모로 변경하고 중복 항목 제거

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 기존 백팩 `ThingDef` 3종의 공통 속성 식별
2. 공통 항목을 포함한 추상 `ThingDef` 작성
3. 기존 정의를 새 추상 부모로 변경하고 중복 항목 제거

## 작업 세부 진행
1. 공통 속성 추출 [v]
2. 추상 부모 정의 추가 [v]
3. 기존 정의 업데이트 [v]

## 진행 상황
### 1. 공통 속성 추출
- 내용: 각 백팩 정의의 `apparel` 및 기본 속성을 비교해 중복 요소 파악
- 결과: `apparel/bodyPartGroups`의 `Shoulders`, `apparel/layers`의 `BackPack`가 3종 모두 공통으로 확인됨

### 2. 추상 부모 정의 추가
- 내용: 공통 속성을 모아 추상 `ThingDef` 생성
- 결과: `RK_ApparelAttr_BackpackBase`에 `apparel/bodyPartGroups`와 `apparel/layers` 공통값을 정의

### 3. 기존 정의 업데이트
- 내용: 각 백팩 정의의 `ParentName`을 새 추상 부모로 변경하고 중복 속성을 제거
- 결과: `RK_CrossBack`, `RK_Backpack`, `RK_OutdoorBackpack`이 새 부모를 상속하며 중복 노드 제거 완료

## 최종 작업 결과/ 중단 사유
완료

## 관련 파일 목록
- [Apparel_Util.xml](../Project/1.6/Defs/ThingsDefs/Apparel_Util.xml)

## 참고사항
- 없음


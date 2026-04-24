# 체인소드 Tool 선택 가중치 증가 작업 - 2024-12-19

## 작업 개요
- 요청 내용: Weapon_HighTech.xml의 체인소드 무기(point, edge) Tool 선택 가중치를 올려서 무기 위주로 공격하도록 수정
- 목표: 무기 Tools의 chanceFactor를 증가시켜 Ratkin 기본 Tools(teeth, fist)보다 우선적으로 선택되도록 함

## 계획 (AI가 결정한 계획)
1. 현재 상황 분석
   - 무기 Tools: point, edge (power 20, cooldown 2.4, chanceFactor 없음 → 기본값 1.0)
   - Ratkin 기본 Tools: teeth (power 10), left/right fist (power 5), chanceFactor 없음 → 기본값 1.0, Pawn 기본 Tool이므로 0.3 배율 적용
   - 현재 가중치: 무기 Tools가 이미 높지만, chanceFactor를 증가시켜 더 우선적으로 선택되도록 함

2. chanceFactor 값 설정
   - RimWorld에서 chanceFactor는 Tool 선택 가중치에 직접 곱해지는 값
   - VerbUtility.AdditionalSelectionFactor에서 `tool.chanceFactor` 사용
   - 기본값 1.0에서 증가시키면 선택 확률이 비례적으로 증가
   - 권장 값: 2.0 ~ 3.0 (무기 Tools를 Ratkin 기본 Tools보다 2~3배 우선적으로 선택)

3. XML 수정
   - Weapon_HighTech.xml의 point와 edge Tool에 `<chanceFactor>` 태그 추가
   - 두 Tool 모두 동일한 값으로 설정하여 균형 유지

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. Weapon_HighTech.xml의 point와 edge Tool에 chanceFactor 3.0 추가
2. 무기 Tools의 선택 가중치를 3배 증가시켜 Ratkin 기본 Tools보다 우선적으로 선택되도록 함

## 작업 세부 진행
1. Weapon_HighTech.xml 파일 수정 [v]
2. chanceFactor 값 적용 확인 [v]
3. 워크플로우 파일 업데이트 [v]

## 진행 상황
### 1. Weapon_HighTech.xml 파일 수정
- 내용: point와 edge Tool에 `<chanceFactor>3.0</chanceFactor>` 추가
- 결과: 무기 Tools의 선택 가중치가 3배 증가하여 Ratkin 기본 Tools(teeth, fist)보다 우선적으로 선택됨

### 2. 린터 검사
- 내용: XML 파일 문법 오류 확인
- 결과: 오류 없음

## 최종 작업 결과/ 중단 사유
완료: Weapon_HighTech.xml의 체인소드 무기(point, edge) Tool에 chanceFactor 3.0을 추가하여 무기 위주로 공격하도록 수정 완료

## 관련 파일 목록
- Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
- RimworldSource/Verse/VerbUtility.cs (참고)

## 참고사항
- RimWorld Tool 선택 로직: InitialVerbWeight = DPS × AdditionalSelectionFactor
- AdditionalSelectionFactor = tool.chanceFactor (기본값 1.0)
- Pawn 기본 Tool은 0.3 배율이 자동 적용됨
- chanceFactor는 0 이상의 실수 값 가능


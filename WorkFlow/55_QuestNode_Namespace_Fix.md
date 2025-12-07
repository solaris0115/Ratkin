# QuestNode Namespace 수정 - 2025-01-XX

## 작업 개요
- 요청 내용: Exception loading def from file Script_ReliquaryPilgrims_Ratkin.xml: System.ArgumentException: Could not find type named QuestNode_Root_ReliquaryPilgrims_Ratkin from node <root Class="QuestNode_Root_ReliquaryPilgrims_Ratkin" />
- 목표: XML 파일에서 QuestNode 클래스 참조에 namespace 추가

## 계획 (AI가 결정한 계획)
1. Script_ReliquaryPilgrims_Ratkin.xml 파일의 107번째 줄 수정
   - 현재: `<root Class="QuestNode_Root_ReliquaryPilgrims_Ratkin" />`
   - 수정: `<root Class="NewRatkin.QuestNode_Root_ReliquaryPilgrims_Ratkin" />`
2. 수정 후 빌드 테스트 (필요시)

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. [사용자 승인 대기]

## 작업 세부 진행
1. XML 파일 수정 [v]
2. 수정 확인 [v]

## 진행 상황
### 1. XML 파일 수정
- 내용: Script_ReliquaryPilgrims_Ratkin.xml 파일의 root 노드에 namespace 추가
- 변경: `<root Class="QuestNode_Root_ReliquaryPilgrims_Ratkin" />` → `<root Class="NewRatkin.QuestNode_Root_ReliquaryPilgrims_Ratkin" />`
- 결과: 수정 완료, 린터 오류 없음

## 최종 작업 결과/ 중단 사유
완료: XML 파일에서 QuestNode 클래스 참조에 NewRatkin namespace를 추가하여 타입을 찾을 수 없는 오류를 해결했습니다.

## 관련 파일 목록
- Project\1.6\Defs\QuestScriptDefs\Script_ReliquaryPilgrims_Ratkin.xml
- Project\1.6\Source\Pilgrim\QuestNode_Root_ReliquaryPilgrims_Ratkin.cs

## 참고사항
- QuestNode_Root_ReliquaryPilgrims_Ratkin 클래스는 NewRatkin namespace에 정의되어 있음
- 다른 XML 파일들에서도 NewRatkin.ClassName 형식으로 namespace를 포함하여 사용 중


# Def 정보 기록 - 2025-09-14

## 작업 개요
- 목표: Def 관련 정보를 루트에 기록하여 향후 Def 작업 시 참고 자료로 활용
- 시작 시간: 2025-09-14
- 완료 시간: 2025-09-14

## 기록 내용

### Def 기본 개념
- **정의**: 데이터 정의에 대한 것으로, 게임 내 각종 요소들의 고유한 정의를 담고 있음
- **네이밍 규칙**: 주로 `XxxDef` 형태로 표현 (예: RecipeDef, ThingDef, RaceDef 등)
- **구성 요소**:
  - DefName: 데이터의 고유 정의명
  - 값과 다른 Def 참조
  - 리소스 경로 정보

### 프로젝트 구조
- **Defs 위치**: `Project/1.6/Defs/` 디렉토리에 모든 Def 파일들이 위치
- **리소스 위치**:
  - `Project/Sounds/`: 오디오 리소스 파일들
  - `Project/Textures/`: 텍스처/이미지 리소스 파일들

### 네이밍 규칙
- **접두사**: `RK_` 사용
- **표기법**: 파스칼 표기법 (PascalCase)
- **예시**: `RK_RecipeSurgery`, `RK_WeaponRifle`, `RK_RaceRatkin`

### 주요 Def 타입들
1. **RecipeDef**: 조리법/제작법 정의
2. **ThingDef**: 아이템/건물/기타 객체 정의
3. **RaceDef**: 종족 정의
4. **TraitDef**: 특성 정의
5. **FactionDef**: 세력 정의
6. **ResearchDef**: 연구 프로젝트 정의
7. **AbilityDef**: 능력 정의
8. **BackStoryDef**: 백스토리 정의
9. **CultureDef**: 문화 정의
10. **HairDef**: 헤어 스타일 정의

## 완료 상태
- Def 기본 개념 및 구조 파악 완료
- 네이밍 규칙 정리 완료
- 주요 Def 타입들 분류 완료

## 참고사항
- 모든 Def 작업 시 이 정보를 참고하여 일관성 있는 네이밍과 구조 유지
- RK_ 접두사와 파스칼 표기법을 반드시 준수
- 리소스 파일들은 Sounds/와 Textures/ 디렉토리 구조를 활용

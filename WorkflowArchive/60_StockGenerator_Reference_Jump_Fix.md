# StockGenerator 참조 점프 문제 해결 - 2024-12-19

## 작업 개요
- 요청 내용: RimworldSource의 StockGenerator_BuyExpensiveSimple.cs에서 StockGenerator 클래스로 F12 참조 점프가 안 되는 문제 해결
- 목표: 디컴파일 소스는 빌드하지 않으면서, C# 언어 서버가 참조를 인식하도록 설정

## 문제 분석

### 현재 상황
1. **솔루션 구조**
   - `Project/1.6/Source/NewRatkin.sln`: 모드 프로젝트만 포함
   - `RimworldSource/Assembly-CSharp.csproj`: 디컴파일 소스 (솔루션에 미포함)

2. **프로젝트 타입 차이**
   - `NewRatkin.csproj`: 레거시 스타일 (MSBuild ToolsVersion 15.0)
   - `Assembly-CSharp.csproj`: SDK 스타일 (.NET SDK)

3. **문제 원인**
   - C# 언어 서버가 `Assembly-CSharp.csproj`를 프로젝트로 인식하지 못함
   - `RimworldSource` 폴더의 .cs 파일들이 고립된 파일로 취급됨
   - F12로 정의 찾기 시 프로젝트 컨텍스트가 없어 실패

## 계획 (AI가 결정한 계획)

### 방법 1: 솔루션에 Assembly-CSharp 프로젝트 추가 + Build 제외 (권장)
**장점:**
- 기존 빌드 구조 유지 (모드만 빌드)
- C# 언어 서버가 두 프로젝트 모두 인식
- F12 참조 점프 정상 작동

**단점:**
- SDK 스타일과 레거시 스타일 프로젝트 혼재 (하지만 솔루션에서 지원 가능)
- 솔루션 파일 수정 필요

**구현 단계:**
1. `Assembly-CSharp.csproj`의 GUID 생성
2. `NewRatkin.sln`에 `Assembly-CSharp` 프로젝트 추가
3. `ProjectConfigurationPlatforms`에서 Build 제외 설정 (`.Build.0` 제거)
4. 상대 경로 계산하여 프로젝트 경로 설정

### 방법 2: 솔루션 파일을 루트로 이동 (비권장)
**단점:**
- 프로젝트 구조 변경이 큼
- 기존 빌드 스크립트 수정 필요
- 불필요한 복잡도 증가

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)

**방법 1 채택**: 솔루션에 Assembly-CSharp 프로젝트 추가 + Build 제외 설정

1. `Assembly-CSharp.csproj`에 고유 GUID 추가
2. `NewRatkin.sln`에 `Assembly-CSharp` 프로젝트 추가
3. Build 설정에서 제외 (`.Build.0` 제거)
4. 테스트: F12 참조 점프 확인

## 작업 세부 진행
1. [Assembly-CSharp.csproj GUID 추가] [v] (SDK 스타일 프로젝트는 솔루션에서만 GUID 관리)
2. [NewRatkin.sln에 프로젝트 추가] [v]
3. [Build 제외 설정] [v] (.Build.0 라인 제거)
4. [테스트 및 검증] [ ]

## 진행 상황
### 1. 프로젝트 구조 분석
- 내용: 현재 솔루션 및 프로젝트 파일 구조 확인
- 결과: 솔루션에 모드 프로젝트만 포함되어 있음 확인

### 2. 해결 방법 검토
- 내용: 여러 방법 검토 및 장단점 분석
- 결과: 방법 1 (솔루션 추가 + Build 제외) 채택

## 최종 작업 결과/ 중단 사유

### 구현 완료
1. **솔루션 파일 수정 완료**
   - `Assembly-CSharp` 프로젝트를 솔루션에 추가
   - 프로젝트 타입 GUID: `{9A19103F-16F7-4668-BE54-9A1E7A4F7556}` (SDK 스타일)
   - 프로젝트 GUID: `{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}`
   - 상대 경로: `..\..\..\RimworldSource\Assembly-CSharp.csproj`

2. **Build 제외 설정 완료**
   - Debug/Release 모두에서 `.Build.0` 라인 제거
   - `ActiveCfg`만 유지하여 프로젝트는 인식하지만 빌드는 제외됨

### 변경 사항
- `Project/1.6/Source/NewRatkin.sln`: Assembly-CSharp 프로젝트 추가 및 Build 제외 설정

### 테스트 필요
- Cursor 재시작 후 F12 참조 점프 테스트
- `StockGenerator_BuyExpensiveSimple.cs`에서 `StockGenerator` 클래스로 점프 확인
- 모드 빌드 시 Assembly-CSharp가 빌드되지 않는지 확인

## 관련 파일 목록
- [NewRatkin.sln](Project/1.6/Source/NewRatkin.sln)
- [Assembly-CSharp.csproj](RimworldSource/Assembly-CSharp.csproj)
- [NewRatkin.csproj](Project/1.6/Source/NewRatkin.csproj)

## 참고사항
- SDK 스타일 프로젝트와 레거시 스타일 프로젝트는 솔루션에서 함께 사용 가능
- Build 제외는 `.Build.0` 라인을 제거하면 됨
- Cursor/VSCode는 솔루션 파일을 읽어 프로젝트를 인식함


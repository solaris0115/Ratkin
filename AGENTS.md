# Ratkin Codex Agent Rules

## 기본 응답

- 답변은 한국어로 작성한다.
- 답변 최상단에 현재 에이전트를 `Agent: Codex 입니다.` 형식으로 표기한다.
- 미사여구를 줄이고 원인, 결과, 답을 중심으로 간결하게 작성한다.
- 제안은 사용자가 요청했거나 작업상 필요한 경우에만 한다.

## 작업 원칙

- 기존 Cursor 문서와 `.cursor/` 파일은 수정하지 않는다. Codex 전용 지침은 `AGENTS.md` 또는 `.codex/` 아래에 추가한다.
- 사용자 변경분을 되돌리지 않는다. 변경 전 `git status --short`로 작업 트리를 확인하고, 이 세션에서 만든 파일만 작업 범위로 취급한다.
- 파일 삭제는 사용자 명시 승인 없이는 하지 않는다.
- 불필요한 리팩터링, 주변 코드 포맷 변경, 요청과 무관한 정리는 하지 않는다.
- 분석/보고서 파일을 만들 때는 검색용 키워드 태그를 문서 상단에 둔다.
- 분석 요청 시 `30_Report/`의 관련 문서를 먼저 검색한다.
- 시각화 요청 시 Mermaid를 우선 고려한다.

## 명령 별 처리

- `!워크플로`: 작업 계획 문서를 `10_Docs/Design/`에 만들고 진행한다. 기본 형식은 작업 개요, 계획, 최종 계획, 진행 상황, 결과, 관련 파일 목록을 포함한다.
- `!분석`: 관련 Def 구조와 `30_Report/` 기존 보고서를 확인한 뒤 분석 보고서를 `30_Report/`에 작성한다.
- `!빌드`, `/build-dev`, `/build-release`, `/release`, `/prerelease`, `!테스트빌드`: `.codex/skills/ratkin-build/SKILL.md`를 따른다.
- Def 데이터, 무기, 방어구, 연구, 종족, 밸런싱, 비교 분석 요청: `.codex/skills/def-data-cache/SKILL.md`를 따른다.
- `!커밋`: 이 Codex 세션에서 변경한 파일만 스테이징하고, 커밋 제목은 `[AI] `로 시작한다. 커밋 성공 후 `python 90_Tools/release_notes.py append-daily`를 실행한다. push는 별도 요청이 있을 때만 한다.

## 프로젝트 구조

- 작업 Def: `Project/1.6/Defs/`
- 텍스처: `Project/Textures/`
- 사운드: `Project/Sounds/`
- C# 소스: `Project/1.6/Source/`
- DLL 출력: `Project/1.6/Assemblies/NewRatkin.dll`
- RimWorld 원본 Def: `RimworldData/`
- RimWorld 원본 소스: `RimworldSource/`
- AlienRace 참고 소스: `AlienRace/`
- 분석 보고서: `30_Report/`
- 릴리즈 노트: `80_ReleaseNote/`

## C# 빌드와 파일 관리

- 프로젝트 파일: `Project/1.6/Source/NewRatkin.csproj`
- Debug 빌드:

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Debug /restore:false; cd ..\..\..
```

- Release 빌드:

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

- Windows PowerShell 5.x에서는 `&&`를 쓰지 말고 `;`를 사용한다.
- 새 `.cs` 파일을 추가하면 `NewRatkin.csproj`에 `<Compile Include="Folder\File.cs" />`를 추가한다.
- `.cs` 파일을 삭제할 때는 csproj의 해당 `<Compile>` 항목도 제거한다.

## Git 안전 규칙

- 사용자가 명시하지 않으면 stage, commit, push를 하지 않는다.
- 금지: `git reset --hard`, `git clean -fd`, 강제 삭제성 branch/rebase 작업.
- 파괴적 작업이 필요하면 먼저 사용자 승인을 받는다.
- PowerShell 외부 실행 파일은 `& "path"` 형식으로 호출한다.

## Def 규칙

- Def 이름은 기본적으로 `RK_` 접두사를 사용한다.
- PascalCase를 사용하고, 구조는 `RK_` + `DefType` + `_` + `SpecificName`을 따른다.
- 예: `RK_Recipe_Xxx`, `RK_Research_Xxx`, `RK_Thing_Xxx`, `RK_PawnKind_Xxx`, `RK_Faction_Xxx`, `RK_Ability_Xxx`.

## RimWorld 로그

- 로그 분석, 오류 확인, 디버깅 요청 시 다음 파일을 읽는다:

```text
C:\Users\HunJaeJeung\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log
```

## 버그 리포트

- 사용자가 에러 로그나 상황 설명을 공유하면 에러 메시지, 발생 상황, 관련 변경, 재현 조건을 기록 대상으로 본다.
- 사용자가 버그 리포트를 요청하면 `30_Report/NNN_요약제목_BugReport.md` 형식으로 작성한다.
- 문서 상단에는 검색용 태그를 추가하고, 에러, 원인, 해결, 참고 링크를 포함한다.

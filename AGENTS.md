# Ratkin Codex Instructions

## 응답

- 모든 답변은 한국어로 작성한다.
- 답변 첫 줄은 `Agent: Codex 입니다.`로 시작한다.
- 원인, 결과, 변경점, 검증 여부를 중심으로 간결하게 답한다.
- 제안은 사용자가 요청했거나 작업 완료에 필요한 경우에만 한다.

## 작업 기본값

- 변경 작업 전 `git status --short`로 기존 변경분을 확인한다.
- 기존 변경분은 사용자 변경으로 간주하고 되돌리지 않는다.
- 요청 범위와 직접 관련된 파일만 수정한다.
- 불필요한 리팩터링, 주변 포맷 변경, 정리는 하지 않는다.
- 파일 삭제와 파괴적 Git 작업은 사용자 명시 승인 전에는 하지 않는다.
- `.cursor/`와 기존 Cursor 문서는 수정하지 않는다.
- 수동 파일 편집은 `apply_patch`를 사용한다.
- PowerShell에서는 명령 연결에 `;`를 사용하고, 외부 실행 파일은 `& "path"` 형식으로 호출한다.

## Git

- 사용자가 명시하지 않으면 stage, commit, push를 하지 않는다.
- 금지: `git reset --hard`, `git clean -fd`, 강제 삭제성 branch/rebase 작업.
- `!커밋` 요청 시 이 Codex 세션에서 변경한 파일만 스테이징한다.
- 커밋 제목은 `[AI] `로 시작한다.
- 커밋 성공 후 `python 90_Tools/release_notes.py append-daily`를 실행한다.
- push는 별도 요청이 있을 때만 한다.

## 프로젝트 경로

- 작업 Def: `Project/1.6/Defs/`
- 텍스처: `Project/Textures/`
- 사운드: `Project/Sounds/`
- C# 소스: `Project/1.6/Source/`
- C# 프로젝트: `Project/1.6/Source/NewRatkin.csproj`
- DLL 출력: `Project/1.6/Assemblies/NewRatkin.dll`
- RimWorld 원본 Def: `RimworldData/`
- RimWorld 원본 소스: `RimworldSource/`
- AlienRace 참고 소스: `AlienRace/`
- 분석 보고서: `30_Report/`
- 릴리즈 노트: `80_ReleaseNote/`

## 요청 처리

- `!워크플로`: `10_Docs/Design/`에 작업 계획 문서를 만들고 진행한다. 문서에는 작업 개요, 계획, 최종 계획, 진행 상황, 결과, 관련 파일 목록을 둔다.
- `!분석`: 관련 Def와 `30_Report/` 기존 보고서를 먼저 확인한 뒤 `30_Report/`에 분석 보고서를 작성한다.
- Def 데이터, 무기, 방어구, 연구, 종족, 밸런싱, 비교 분석 요청: 가능한 경우 `.codex/skills/def-data-cache/SKILL.md`를 따른다.
- `!빌드`, `/build-dev`, `/build-release`, `/release`, `/prerelease`, `!테스트빌드`: 가능한 경우 `.codex/skills/ratkin-build/SKILL.md`를 따른다.
- 버그 리포트 요청: `30_Report/NNN_요약제목_BugReport.md` 형식으로 작성한다.
- 분석/보고서 파일 상단에는 검색용 키워드 태그를 둔다.
- 시각화 요청은 Mermaid를 우선 고려한다.

## C# 빌드와 파일 관리

- Debug 빌드:

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Debug /restore:false; cd ..\..\..
```

- Release 빌드:

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

- 새 `.cs` 파일을 추가하면 `NewRatkin.csproj`에 `<Compile Include="Folder\File.cs" />`를 추가한다.
- `.cs` 파일 삭제는 사용자 승인 후 진행하고, 삭제 시 csproj의 해당 `<Compile>` 항목도 함께 제거한다.

## Def 규칙

- DefName은 `RK_`로 시작한다.
- PascalCase를 사용한다.
- 기존 DefName은 안정성 목적상 수정하지 않는다. 사용자가 요청한 경우에만 수정한다.
- 신규 DefName 작성 시에만 해당 DefType을 고려해 `RK_` + `DefType` + `_` + `SpecificName` 구조를 사용한다.
- 예: `RK_Recipe_Xxx`, `RK_Research_Xxx`, `RK_Thing_Xxx`, `RK_PawnKind_Xxx`, `RK_Faction_Xxx`, `RK_Ability_Xxx`.

## RimWorld 로그

- 로그 분석, 오류 확인, 디버깅 요청 시 다음 파일을 확인한다:

```text
C:\Users\HunJaeJeung\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log
```

## 버그 리포트

- 사용자가 에러 로그나 상황 설명을 공유하면 에러 메시지, 발생 상황, 관련 변경, 재현 조건을 기록 대상으로 본다.
- 버그 리포트에는 검색용 태그, 에러, 원인, 해결, 참고 링크를 포함한다.

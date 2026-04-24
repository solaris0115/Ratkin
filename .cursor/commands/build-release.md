---
description: Release 모드로 NewRatkin만 로컬 컴파일 (ZIP·GitHub 배포 없음)
---

# /build-release

**로컬로 MSBuild `Build` 타깃만** 돌린다. `/build-dev`와 **차이는 속성**(구성·`RatkinDevFeatures`·그에 따른 전처리/컴파일 조건만)뿐이다.

- **속성**: `Configuration=Release`, `RatkinDevFeatures=false` (`RATKIN_DEV_FEATURES` 없음) → `Project/1.6/Assemblies/NewRatkin.dll`
- **하지 않음**: ZIP, `gh`, GitHub

**GitHub까지 배포**는 **`/release`**(정식) 또는 **`/prerelease`**(프리릴리스) — 둘 다 빌드 후 ZIP·`gh` 업로드까지 포함한다.

## 에이전트가 할 일

워크스페이스 **저장소 루트**에서 아래 **한 줄**만 실행한다 (PowerShell에서 `&&` 금지 — `;` 사용).

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Build /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

## 참조

- [NewRatkin.csproj](../../Project/1.6/Source/NewRatkin.csproj): `OutputPath`는 항상 `..\Assemblies\` — `bin\Release\` DLL을 Assemblies로 복사하지 말 것
- MSBuild 경로가 다르면 `Community`를 `Professional` / `Enterprise`로 바꾸거나, `/release`와 동일하게 `MSBUILD` 환경 변수에 `MSBuild.exe` 전체 경로를 두고 그 경로를 사용한다

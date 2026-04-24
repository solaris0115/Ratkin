---
description: Debug 모드로 NewRatkin만 로컬 컴파일 (ZIP·GitHub 배포 없음)
---

# /build-dev

**로컬로 MSBuild `Build` 타깃만** 돌린다. `/build-release`와 **차이는 속성**(구성·`RatkinDevFeatures`·그에 따른 전처리/컴파일 조건만)뿐이다.

- **속성**: `Configuration=Debug` — csproj 기본에 따라 **`RatkinDevFeatures=true`**(AutoTests·InfoCard 관련 소스, `RATKIN_DEV_FEATURES` 등)
- **하지 않음**: ZIP, `gh`, GitHub

배포용 DLL·ZIP·GitHub는 **`/release`** 또는 **`/prerelease`**.

## 에이전트가 할 일

워크스페이스 **저장소 루트**에서 아래 **한 줄**만 실행한다 (PowerShell에서 `&&` 금지 — `;` 사용).

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Build /p:Configuration=Debug /restore:false; cd ..\..\..
```

## 참조

- [NewRatkin.csproj](../../Project/1.6/Source/NewRatkin.csproj): Debug / `RatkinDevFeatures` 조건
- MSBuild 경로: [build-release.md](build-release.md)와 동일

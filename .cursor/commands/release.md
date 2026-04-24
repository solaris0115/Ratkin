---
description: 1.6 정식 배포 — Release 빌드·ZIP 패키징·GitHub Release 에셋 교체 (solaris0115/NewRatkin 1.6)
---

# /release — 정식 배포 (빌드 + ZIP + GitHub)

사용자가 `/release`를 실행했다. **로컬 빌드만이 아니라 배포까지** 한다.

1. **빌드**: 프리릴리스와 동일 — MSBuild **`/t:Rebuild`**, `Configuration=Release`, `RatkinDevFeatures=false` → `Assemblies/NewRatkin.dll` (`tools/release.py`와 동일 인자)
2. **패키징**: `Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip` 생성 (`Project/1.5` 제외)
3. **배포**: 정식 릴리스 [1.6](https://github.com/solaris0115/NewRatkin/releases/tag/1.6)에 ZIP을 **`gh release upload`로 교체 업로드**한다.

**로컬 컴파일만**(ZIP·GitHub 없음)은 **`/build-release`** / **`/build-dev`** — 둘 다 **`Rebuild`**(강제 전체 빌드), 속성만 다름.

**GitHub 배포까지** 하는 경로는 **`/release`**(정식 태그 `1.6` 에셋 교체)와 **`/prerelease`**(Dev1.6 프리릴리스 재생성)뿐이다.

## 에이전트가 할 일

1. 워크스페이스 **저장소 루트**에서 아래 **한 줄**만 실행한다 (PowerShell에서 `&&` 금지 — `;` 사용). 이 스크립트가 빌드·ZIP·**GitHub 배포**를 모두 수행한다.

```powershell
python tools/release.py
```

2. 실패 시 stderr·exit code를 보고 원인만 짧게 전달한다. 성공 시 업로드된 ZIP 경로를 한 줄로 알린다.

## 전제

- `gh` 로그인됨 (`gh auth login`), `7z` PATH, Visual Studio MSBuild(또는 `MSBUILD` 환경 변수로 `MSBuild.exe` 경로).
- 태그 `1.6` 릴리스가 GitHub에 이미 있어야 한다.

## 동작 요약 (스크립트)

- `Project/1.6/Source` — MSBuild **`/t:Rebuild`**, Release, `RatkinDevFeatures=false` → `Assemblies/NewRatkin.dll` 반영
- `Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip` 생성 (`Project/1.5` 제외)
- `gh release view`로 **기존 커스텀 .zip** 파일명이 있으면 그 이름으로 복사한 뒤 `gh release upload 1.6 … --clobber` — 없으면 새 이름으로 업로드

## 옵션 (환경 변수)

| 변수 | 기본 | 설명 |
|------|------|------|
| `RATKIN_RELEASE_REPO` | `solaris0115/NewRatkin` | `-R` |
| `RATKIN_RELEASE_TAG` | `1.6` | 업로드 태그 |
| `RATKIN_RELEASE_VERSION` | (자동 패치+1) | ZIP 버전 문자열 |
| `MSBUILD` | (vswhere/2022 Community) | MSBuild.exe 경로 |

---
description: 1.6 정식 릴리스 빌드 후 GitHub 에셋 교체 (solaris0115/NewRatkin 1.6)
---

# /release

사용자가 `/release`를 실행했다. **프리릴리스(!prerelease)와 동일한 Release 빌드·ZIP**을 만든 뒤, 정식 릴리스 [1.6](https://github.com/solaris0115/NewRatkin/releases/tag/1.6)에 ZIP을 **교체 업로드**한다.

## 에이전트가 할 일

1. 워크스페이스 **저장소 루트**에서 아래 **한 줄**만 실행한다 (PowerShell에서 `&&` 금지 — `;` 사용).

```powershell
python tools/release.py
```

2. 실패 시 stderr·exit code를 보고 원인만 짧게 전달한다. 성공 시 업로드된 ZIP 경로를 한 줄로 알린다.

## 전제

- `gh` 로그인됨 (`gh auth login`), `7z` PATH, Visual Studio MSBuild(또는 `MSBUILD` 환경 변수로 `MSBuild.exe` 경로).
- 태그 `1.6` 릴리스가 GitHub에 이미 있어야 한다.

## 동작 요약 (스크립트)

- `Project/1.6/Source` Release + `RatkinDevFeatures=false` → `Assemblies/NewRatkin.dll` 반영
- `Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip` 생성 (`Project/1.5` 제외)
- `gh release view`로 **기존 커스텀 .zip** 파일명이 있으면 그 이름으로 복사한 뒤 `gh release upload 1.6 … --clobber` — 없으면 새 이름으로 업로드

## 옵션 (환경 변수)

| 변수 | 기본 | 설명 |
|------|------|------|
| `RATKIN_RELEASE_REPO` | `solaris0115/NewRatkin` | `-R` |
| `RATKIN_RELEASE_TAG` | `1.6` | 업로드 태그 |
| `RATKIN_RELEASE_VERSION` | (자동 패치+1) | ZIP 버전 문자열 |
| `MSBUILD` | (vswhere/2022 Community) | MSBuild.exe 경로 |

# Ratkin Build Reference

## 버전 관리

- 패키징 전 `Build/TestBuild/`의 기존 ZIP을 확인한다.
- 최신 버전에서 패치 번호를 +1한다.
- 폴더가 비어 있으면 `0.0.1`을 임의 선택하지 말고 사용자에게 시작 버전을 확인한다.
- 마이너/메이저 증가는 사용자가 명시할 때만 한다.

```powershell
Get-ChildItem "Build/TestBuild" -Filter "*.zip" | Sort-Object Name -Descending | Select-Object -First 5
```

## ZIP 규칙

- 파일명: `Ratkin_TestBuild_YYMMDD_버전.zip`
- 출력 경로: `Build/TestBuild/`
- 포함: `Project/` 전체
- 제외: `Project/1.5/`
- 대괄호가 없는 파일명을 사용해 `gh` glob 문제를 피한다.

```powershell
7z a -tzip "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" "./Project/*" -xr!"Project/1.5"
```

## 배포 흐름

1. Release 빌드: `Configuration=Release`, `RatkinDevFeatures=false`
2. ZIP 생성: `Project/1.5` 제외
3. GitHub Release 업로드

## 저장소 구분

- 기본 배포: `solaris0115/NewRatkin`
- 별도 Ratkin 저장소: `solaris0115/Ratkin`

## GitHub Pre-release

- 저장소: `solaris0115/NewRatkin`
- 태그: `Dev1.6`
- 제목: `[1.6]TestBuild`
- 최신 ZIP 하나만 업로드한다.

```powershell
gh -R solaris0115/NewRatkin release delete Dev1.6 --yes
gh -R solaris0115/NewRatkin release create Dev1.6 "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" --title "[1.6]TestBuild" --prerelease -n " "
```

## 정식 Release

- `/release`는 `python 90_Tools/release.py`를 실행한다.
- 환경 변수:
  - `RATKIN_RELEASE_REPO`: 기본 `solaris0115/NewRatkin`
  - `RATKIN_RELEASE_TAG`: 기본 `1.6`
  - `RATKIN_RELEASE_VERSION`: ZIP 버전 강제 지정
  - `MSBUILD`: MSBuild.exe 전체 경로

---
name: ratkin-build
description: Ratkin 프로젝트 빌드, 테스트 빌드 ZIP 패키징, GitHub release/prerelease 배포를 수행한다. 사용자가 !빌드, !테스트빌드, /build-dev, /build-release, /release, /prerelease, build, testbuild, prerelease, release를 요청할 때 사용.
---

# Ratkin Build

## 핵심 규칙

- PowerShell 5.x 기준으로 `&&`를 쓰지 말고 `;`를 사용한다.
- C# 빌드는 항상 MSBuild `Rebuild` 타깃을 사용해 증분 건너뜀을 피한다.
- `Project/1.6/Source/NewRatkin.csproj`의 `OutputPath`는 `Project/1.6/Assemblies/`이다. Release DLL을 `bin/Release`에서 복사하지 않는다.
- 배포 대상 기본 저장소는 `solaris0115/NewRatkin`이다.
- 상세 버전 결정, 7-Zip 옵션, 배포 흐름은 [references/reference.md](references/reference.md)를 필요할 때 읽는다.

## 로컬 Debug 빌드

사용자가 `!빌드`, `/build-dev`, 로컬 디버그 빌드를 요청하면 저장소 루트에서 실행한다.

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Debug /restore:false; cd ..\..\..
```

Debug는 기본적으로 `RatkinDevFeatures=true`이며 `RATKIN_DEV_FEATURES` 관련 코드가 포함된다.

## 로컬 Release 빌드

사용자가 `/build-release` 또는 배포 기능 없는 Release 컴파일을 요청하면 실행한다.

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

Release는 `RATKIN_DEV_FEATURES` 없이 `Project/1.6/Assemblies/NewRatkin.dll`에 직접 반영된다.

## 테스트 빌드 ZIP

사용자가 `!테스트빌드` 또는 `testbuild`를 요청하면:

1. `Build/TestBuild/`의 기존 ZIP 목록을 확인한다.
2. 최신 버전의 패치 번호를 +1한다. 폴더가 비어 있으면 시작 버전을 사용자에게 확인한다.
3. `Project/` 전체를 ZIP으로 묶고 `Project/1.5/`는 제외한다.
4. 파일명은 `Ratkin_TestBuild_YYMMDD_버전.zip`으로 한다.

```powershell
Get-ChildItem "Build/TestBuild" -Filter "*.zip" | Sort-Object Name -Descending | Select-Object -First 5
if (-not (Test-Path "Build/TestBuild")) { New-Item -ItemType Directory -Path "Build/TestBuild" -Force }
7z a -tzip "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" "./Project/*" -xr!"Project/1.5"
```

## 프리릴리스 배포

`/prerelease` 또는 `!프리릴리스`는 빌드, ZIP, GitHub Pre-release 재생성을 모두 수행한다. 기본은 Release + `RatkinDevFeatures=false`이다.

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
7z a -tzip $output "./Project/*" -xr!"Project/1.5"
gh -R solaris0115/NewRatkin release delete Dev1.6 --yes
gh -R solaris0115/NewRatkin release create Dev1.6 $output --title "[1.6]TestBuild" --prerelease -n " "
```

## 정식 배포

`/release`는 `90_Tools/release.py`가 빌드, ZIP, GitHub 릴리스 에셋 교체를 모두 수행한다.

```powershell
python 90_Tools/release.py
```

전제 조건은 `gh` 로그인, `7z` PATH, Visual Studio MSBuild 또는 `MSBUILD` 환경 변수이다.

## C# 파일 관리

- 새 `.cs` 파일을 만들면 `NewRatkin.csproj`에 `<Compile Include="Folder\File.cs" />`를 추가한다.
- `.cs` 파일을 삭제하면 해당 `<Compile>` 항목도 제거한다.

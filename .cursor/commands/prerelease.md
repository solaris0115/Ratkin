---
description: 프리릴리스 배포 — Build·ZIP·GitHub Pre-release(Dev1.6)까지 (로컬 빌드만은 /build-release·/build-dev)
---

# /prerelease — 프리릴리스 배포 (빌드 + ZIP + GitHub)

`!prerelease` / `!프리릴리스` 또는 **`/prerelease`** 입력 시 이 커맨드를 실행합니다.

**`/release`와 같이 GitHub까지 배포**한다. 차이는 **대상 태그**뿐이다.

| 커맨드 | 빌드·ZIP | GitHub |
|--------|----------|--------|
| **`/release`** | 동일 계열 TestBuild ZIP | 정식 [1.6](https://github.com/solaris0115/NewRatkin/releases/tag/1.6) 에셋 교체 (`python tools/release.py`) |
| **`/prerelease`** | 아래 PowerShell | **Pre-release** `Dev1.6` 재생성 (`gh` 삭제 후 create) |

**로컬 컴파일만**은 **`/build-release`** / **`/build-dev`** — MSBuild **`/t:Build`** 만, 속성만 다름.

배포용 프리릴리스 ZIP에는 **`RATKIN_DEV_FEATURES` 없음**(디버그 액션·InfoCard 갓모드 패치 등 제외). 내부 테스트용으로 dev DLL을 넣으려면 아래 **모드 B**를 쓴다.

## 참조 규칙

- [07-testbuild-packaging.mdc](../rules/07-testbuild-packaging.mdc)
- [NewRatkin.csproj](../../Project/1.6/Source/NewRatkin.csproj): `RatkinDevFeatures`, `RATKIN_DEV_FEATURES`

## 실행 순서

1. **버전 확인**: `Build/TestBuild/` 폴더에서 기존 ZIP 목록 조회
2. **버전 결정**: 최신 패치 버전 +1 (예: 0.0.6 → 0.0.7). 폴더가 비어 있으면 사용자에게 시작 버전 확인
3. **C# 빌드 → Assemblies 반영** (아래 **모드 A** 또는 **모드 B** 중 하나)
4. **ZIP 패키징**: `Project/` 전체 압축, `Project/1.5/` 제외 → **파일 1개만** 생성
5. **GitHub Release**: 기존 Dev1.6 삭제 후 최신 파일로 Pre-release 재생성

### 모드 A — 배포용 프리릴리스 (기본, dev 기능 제외)

`Configuration=Release`이면 `RatkinDevFeatures`는 기본 `false`이며 `RATKIN_DEV_FEATURES`가 정의되지 않는다. `NewRatkin.csproj`의 `OutputPath`가 `..\Assemblies\`이므로 **`/t:Build`** 만으로 `Project/1.6/Assemblies/NewRatkin.dll`에 반영된다. **`bin\Release\NewRatkin.dll`은 더 이상 갱신되지 않으므로 Assemblies로 복사하면 안 된다**(오래된 DLL로 덮여 타입 누락 오류가 난다).

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Build /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

### 모드 B — 프리릴리스이지만 내부용(dev DLL 유지)

ZIP만 프리릴리스로 올리고 DLL은 디버그 액션 등 포함(로컬·내부 테스트). `Project/1.6/Assemblies/`에 Debug(+기본 `RatkinDevFeatures=true`) 산출물이 들어가게 한다.

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Build /p:Configuration=Debug /restore:false; cd ..\..\..
```

> **파일명**: `Ratkin_TestBuild_YYMMDD_버전.zip` (대괄호 없음 → gh glob 오류 방지, 복사 단계 불필요)

## 실행 명령어 (PowerShell)

워크스페이스 루트에서 실행:

```powershell
# 1. 변수 설정 (버전은 위 단계에서 결정된 값 사용)
$date = Get-Date -Format "yyMMdd"
$version = "0.0.7"  # ← 결정된 버전으로 교체
$filename = "Ratkin_TestBuild_${date}_${version}.zip"
$output = "Build/TestBuild/$filename"

# 2. 디렉토리 확인
if (-not (Test-Path "Build/TestBuild")) { 
    New-Item -ItemType Directory -Path "Build/TestBuild" -Force 
}

# 3. C# — 모드 A(배포용): 아래 한 줄 실행 후 4번으로 진행
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Build /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..

# 4. 압축 (파일 1개만 생성)
7z a -tzip $output "./Project/*" -xr!"Project/1.5"

# 5. GitHub Pre-release 업로드 (NewRatkin)
gh -R solaris0115/NewRatkin release delete Dev1.6 --yes
gh -R solaris0115/NewRatkin release create Dev1.6 $output --title "[1.6]TestBuild" --prerelease -n " "
```

## 버전 확인 (사전 실행)

```powershell
Get-ChildItem "Build/TestBuild" -Filter "*.zip" | Sort-Object Name -Descending | Select-Object -First 5
```

## 주의사항

- **배포 대상**: solaris0115/NewRatkin (Ratkin 아님)
- **태그**: Dev1.6 (고정)
- **에셋**: 최신 빌드 1개만 (기존 Release 삭제 후 재생성)

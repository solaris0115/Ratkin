---
name: ratkin-build
description: Ratkin 프로젝트 빌드, 패키징, 배포 규칙. MSBuild로 C# 컴파일, 테스트 빌드 ZIP 패키징, GitHub Release 업로드 시 사용. !빌드, !테스트빌드, build, testbuild 요청 시 적용.
---

# Ratkin 빌드 스킬

## 핵심 규칙

### PowerShell 문법 (필수)
- **`&&` 사용 금지**: Windows PowerShell 5.x에서 오류 발생
- **명령 연결 시 `;` 사용**: `cd ... ; & "MSBuild..."` 형태

## 1. C# 빌드 (!빌드)

**프로젝트**: `Project/1.6/Source/NewRatkin.csproj`

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Debug
```

- **`/t:Rebuild`**: Clean 후 빌드 — 증분 판단으로 컴파일을 건너뛰지 않는다.
- **출력 DLL**: `Project/1.6/Assemblies/NewRatkin.dll` (자동 복사)
- **성공 메시지**: `Build succeeded.`

### 프리릴리스·릴리스용 (dev 기능 제외)

`RATKIN_DEV_FEATURES` 없이 빌드: **Release** + 명시적으로 `RatkinDevFeatures=false`. 산출 DLL은 **`Project/1.6/Assemblies/NewRatkin.dll`**로 바로 나간다(`OutputPath`). `bin\Release\NewRatkin.dll`은 예전 설정 잔재일 수 있으니 Assemblies로 복사하지 않는다.

```powershell
cd Project/1.6/Source; & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NewRatkin.csproj /t:Rebuild /p:Configuration=Release /p:RatkinDevFeatures=false /restore:false; cd ..\..\..
```

`!prerelease` / `!프리릴리스` 커맨드는 위 순서를 패키징 전에 실행한다.

### C# 파일 관리
- **새 파일 생성 시**: `NewRatkin.csproj`의 `<ItemGroup>`에 `<Compile Include="폴더\파일명.cs" />` 추가
- **파일 삭제 시**: 해당 `<Compile Include="..."/>` 항목 제거

## 2. 테스트 빌드 패키징 (!테스트빌드)

**파일명**: `Ratkin_TestBuild_YYMMDD_버전.zip`  
**출력 경로**: `Build/TestBuild/`

**패키징 대상**: `Project/` 전체, **제외**: `Project/1.5/`

```powershell
# 디렉터리 생성
if (-not (Test-Path "Build/TestBuild")) { New-Item -ItemType Directory -Path "Build/TestBuild" -Force }

# 압축 (7-Zip)
7z a -tzip "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" "./Project/*" -xr!"Project/1.5"
```

**버전 확인**: `Get-ChildItem "Build/TestBuild" -Filter "*.zip" | Sort-Object Name -Descending | Select-Object -First 5`

## 3. GitHub Release 배포

### NewRatkin (기본)
- **저장소**: solaris0115/NewRatkin
- **태그**: Dev1.6
- **제목**: [1.6]TestBuild
- **Pre-release**: 사용

```powershell
gh -R solaris0115/NewRatkin release delete Dev1.6 --yes
gh -R solaris0115/NewRatkin release create Dev1.6 "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" --title "[1.6]TestBuild" --prerelease -n " "
```

### Ratkin (별도)
- **저장소**: solaris0115/Ratkin
- **언어**: 제목·설명 영어

```powershell
gh -R solaris0115/Ratkin release create v버전 "Build/TestBuild/Ratkin_TestBuild_YYMMDD_버전.zip" --title "Test Build 버전" --notes "Test build (YYYY-MM-DD)"
```

## 상세 참조

- [reference.md](reference.md): 버전 관리, 옵션 설명, 워크플로우

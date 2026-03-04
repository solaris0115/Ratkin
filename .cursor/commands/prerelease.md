# !prerelease / !프리릴리스

`!prerelease` 또는 `!프리릴리스` 입력 시 이 커맨드를 실행합니다.

테스트 빌드 패키징 후 **solaris0115/NewRatkin** GitHub Pre-release로 배포합니다.

## 참조 규칙

- [07-testbuild-packaging.mdc](../rules/07-testbuild-packaging.mdc)

## 실행 순서

1. **버전 확인**: `Build/TestBuild/` 폴더에서 기존 ZIP 목록 조회
2. **버전 결정**: 최신 패치 버전 +1 (예: 0.0.6 → 0.0.7). 폴더가 비어 있으면 사용자에게 시작 버전 확인
3. **ZIP 패키징**: `Project/` 전체 압축, `Project/1.5/` 제외 → **파일 1개만** 생성
4. **GitHub Release**: 기존 Dev1.6 삭제 후 최신 파일로 Pre-release 재생성

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

# 3. 압축 (파일 1개만 생성)
7z a -tzip $output "./Project/*" -xr!"Project/1.5"

# 4. GitHub Pre-release 업로드 (NewRatkin)
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

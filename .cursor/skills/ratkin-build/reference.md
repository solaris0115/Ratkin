# Ratkin 빌드 상세 참조

## 버전 관리 규칙

- **필수**: 패키징 전 `Build/TestBuild/` 폴더 확인하여 기존 빌드 목록 조회
- **버전 결정**: 최신 버전에서 패치 +1 (예: 0.0.6 → 0.0.7)
- **폴더 비어있음**: 0.0.1부터 시작하지 않고 사용자에게 시작 버전 확인
- **마이너/메이저**: 사용자 명시적 요청 시만
  - 마이너: 0.0.6 → 0.1.0
  - 메이저: 0.1.0 → 1.0.0

## 7-Zip 옵션 설명

| 옵션 | 설명 |
|------|------|
| `a` | 압축 파일 생성 |
| `-tzip` | ZIP 형식 지정 |
| `-xr!"Project/1.5"` | Project/1.5 폴더 재귀적 제외 |

## 배포 워크플로우

1. **(프리릴리스)** Release C# 빌드 + `RatkinDevFeatures=false` → `Project/1.6/Assemblies/NewRatkin.dll`에 직접 출력(`OutputPath`; `bin/Release` 복사 금지)
2. **압축**: Project/ 폴더를 ZIP으로 패키징
3. **1.5 제외**: Project/1.5/ 압축에서 제외
4. **웹 배포**: GitHub Release 업로드 (기본: solaris0115/NewRatkin)

## 저장소 구분

- **NewRatkin**: solaris0115/NewRatkin — 기본 배포 대상
- **Ratkin**: solaris0115/Ratkin — 별도 리포지토리

## 프로젝트 경로 요약

| 항목 | 경로 |
|------|------|
| 소스 | Project/1.6/Source |
| 출력 DLL (로컬 Debug) | Project/1.6/Assemblies/NewRatkin.dll |
| Release 산출 DLL | Project/1.6/Assemblies/NewRatkin.dll (`Source/bin/Release`는 미사용·잔재 가능) |
| 중간 출력 | Project/1.6/Source/obj/Debug/ 또는 obj/Release/ |
| csproj | Project/1.6/Source/NewRatkin.csproj |

**프리릴리스·배포 ZIP**: Release + `/p:RatkinDevFeatures=false` Rebuild 한 번이면 Assemblies에 반영됨. (`RATKIN_DEV_FEATURES` 없음)

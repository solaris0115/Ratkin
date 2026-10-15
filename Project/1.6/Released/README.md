# RimWorld 1.6 고정 배포본

- 기준 배포일: 2026-05-20
- 기준 Git 커밋: `8669dcc70fd0c4a25e82d71c2c9d25007d74a03e`
- XML, DLC 패치, 번역, 텍스처, 사운드, C# 소스는 위 커밋의 원본을 보존한다.
- `1.6/Assemblies/NewRatkin.dll`은 당시 DLL이 Git에 없으므로 이 폴더의 당시 소스를 Release / RatkinDevFeatures=false로 재생성한다.
- 재생성 시 설치된 RimWorld 1.6 Managed DLL과 Harmony 1.6을 참조한다.
- 루트 About 및 LoadFolders는 버전 선택을 위한 공용 메타데이터다.
- 1.6은 이 보관본만 로드한다. 이후 개발 내용은 기존 개발 폴더를 사용하는 1.7에만 적용한다.
- 보관본의 콘텐츠를 현재 개발 소스로 덮어쓰거나 현재 1.6 개발 프로젝트의 DLL을 복사하지 않는다.
- 외부 의존 DLL `SYS.dll`은 기존 1.6 파일을 보존했다. 로컬본과 설치된 Workshop본의 SHA-256이 동일하다: `8ee21292e2c8000e5d34077a87b07ed62d2b85b9d39790ee5fe6a9156e38f8ef`.

## 보관본 DLL 재생성 기록

- MSBuild 2022 / Release / RatkinDevFeatures=false
- 프로젝트: `1.6/Source/NewRatkin.csproj`
- Harmony 경로가 과거 Current 폴더를 가리키므로 빌드 인자로 `ReferencePath=C:/Program Files (x86)/Steam/steamapps/workshop/content/294100/2009463077/1.6/Assemblies`를 전달했다.
- 게임 실행 테스트는 수행하지 않았다.

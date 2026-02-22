# DefaultOutfit 태그 리스트

## 개요
`defaultOutfitTags`는 게임 시작 시 기본 복장 정책을 생성하는 데 사용되는 태그입니다.

## 생성되는 기본 복장 정책

### 1. Anything
- **설명**: 모든 의상 허용
- **조건**: 전체 Apparel 카테고리
- **태그**: 없음

### 2. Worker
- **설명**: 작업자용 복장 정책
- **조건**: Worker 태그 + ApparelUtility 카테고리
- **태그**: `<li>Worker</li>`

### 3. Soldier
- **설명**: 전투원용 복장 정책
- **조건**: Soldier 태그 + ApparelUtility 카테고리
- **태그**: `<li>Soldier</li>`

### 4. Nudist
- **설명**: 누드주의자용 복장 정책
- **조건**: Nudist 태그 OR 다리/몸통 미착용 의상
- **태그**: `<li>Nudist</li>`

### 5. Slave
- **DLC**: Ideology
- **설명**: 노예용 복장 정책
- **조건**: Slave 태그
- **태그**: `<li>Slave</li>`

### 6. Spacefarer
- **DLC**: Odyssey
- **설명**: 우주여행자용 복장 정책
- **조건**: Spacefarer 태그 + ApparelUtility 카테고리
- **태그**: `<li>Spacefarer</li>`

## 사용 예시

```xml
<!-- 범용 의상 (작업/전투/우주 모두) -->
<apparel>
  <defaultOutfitTags>
    <li>Worker</li>
    <li>Soldier</li>
    <li>Spacefarer</li>
  </defaultOutfitTags>
</apparel>

<!-- 전투 전용 의상 -->
<apparel>
  <defaultOutfitTags>
    <li>Soldier</li>
  </defaultOutfitTags>
</apparel>

<!-- 누드주의자도 착용 가능한 의상 -->
<apparel>
  <defaultOutfitTags>
    <li>Worker</li>
    <li>Nudist</li>
  </defaultOutfitTags>
</apparel>
```

## 중요 사항

- **게임 시작 시에만 적용**: 런타임에는 생성된 ApparelPolicy.filter가 사용됨
- **세이브 파일에 저장**: 게임 중간에 태그 변경해도 기존 세이브에는 영향 없음
- **ApparelUtility 자동 포함**: Worker/Soldier/Spacefarer 정책은 유틸리티 의상도 자동 포함

## 참고 파일

- 소스코드: `RimworldSource/RimWorld/OutfitDatabase.cs`
- 정의: `RimworldSource/RimWorld/ApparelProperties.cs`


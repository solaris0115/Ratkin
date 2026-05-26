# 유니크폰 네이밍 규칙

## DefName 패턴

| Def 종류 | 패턴 | 예시 |
|----------|------|------|
| 유년 Backstory | `RK_Unique_{Name}_Childhood` | `RK_Unique_Kira_Childhood` |
| 성년 Backstory | `RK_Unique_{Name}_Adulthood` | `RK_Unique_Kira_Adulthood` |
| PawnKind | `RK_Unique_{Name}` | `RK_Unique_Kira` |
| Trait (커스텀) | `RK_Trait_{TraitName}` | `RK_Trait_AntipyrManiac` |

## 파일 배치

실제 구현 파일은 아래 경로에 생성:

```
Project/1.6/Defs/UniquePawn/{Name}.xml      ← Backstory + PawnKind 합본
Project/1.6/Defs/UniquePawn/Trait_{Name}.xml ← 커스텀 트레잇 (필요 시)
```

번역:
```
Project/Contents/Languages/Korean (한국어)/DefInjected/
  AlienRace.AlienBackstoryDef/Backstory_UniquePawn_{Name}.xml
  PawnKindDef/PawnKinds_UniquePawn_{Name}.xml
  TraitDef/Traits_UniquePawn_{Name}.xml (필요 시)
```

## 작업 흐름

1. `Template.md` 기획 내용 채움
2. `Backstory.xml` → `{Name}` 치환, 주석 해제/삭제
3. `PawnKind.xml` → `{Name}` 치환, 필요 옵션만 주석 해제
4. `Trait.xml` → 커스텀 필요 시만 사용
5. 완성된 XML을 `Project/1.6/Defs/UniquePawn/`에 복사
6. 번역 파일 생성
7. 빌드 & 인게임 테스트

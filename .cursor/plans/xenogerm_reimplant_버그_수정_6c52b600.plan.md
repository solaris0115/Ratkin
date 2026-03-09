---
name: Xenogerm Reimplant 버그 수정
overview: 생귀오파지가 랫킨에게 유전자 이식(ReimplantXenogerm) 시 xenotype이 생귀오파지가 아닌 랫킨으로 표시되는 문제를 Harmony 패치로 수정합니다.
todos:
  - id: analyze-approach
    content: 방법 A vs C 중 최종 구현 방식 결정 (사용자 확인 필요)
    status: pending
  - id: create-harmony-patch
    content: HarmonyPatch_ReimplantXenogerm.cs 파일 생성 - ReimplantXenogerm Prefix 패치 구현
    status: pending
  - id: update-csproj
    content: NewRatkin.csproj에 새 파일 Compile Include 추가
    status: pending
  - id: build-test
    content: 빌드 확인
    status: pending
isProject: false
---

# Xenogerm Reimplant Xenotype 버그 수정 계획

## 문제 원인

`GeneUtility.ReimplantXenogerm()` 실행 시:

```
185행: recipient.genes.SetXenotype(caster.genes.Xenotype)  // AlienRace가 차단
186행: recipient.genes.xenotypeName = caster.genes.xenotypeName  // 실행됨
189행: recipient.genes.ClearXenogenes()  // 실행됨
190행: foreach ... AddGene(gene.def, true)  // 실행됨
```

AlienRace의 `SetXenotypePrefix` 패치가 `SetXenotype` 호출만 차단하고, 이후 유전자 복사 로직은 그대로 진행됨. 결과적으로 xenotype 레이블은 "랫킨"이지만 실제 유전자는 생귀오파지의 것으로 교체되는 불일치 발생.

## 해결 방향

두 가지 접근법이 있습니다:

### 방법 A: `ReimplantXenogerm` Harmony 패치 (권장)

`GeneUtility.ReimplantXenogerm`을 Harmony **Prefix**로 패치하여, 랫킨이 recipient일 때 전체 메서드 동작을 커스터마이징.

- **장점**: 가장 정확하게 문제를 제어 가능, xenotype 설정도 올바르게 처리
- **구현 위치**: `Project/1.6/Source/` 에 새 Harmony 패치 파일 생성

**로직**:

1. recipient가 랫킨인지 확인
2. 랫킨이면 `SetXenotype` 대신 `SetXenotypeDirect`로 xenotype 직접 설정 (AlienRace 패치를 우회)하거나, whiteXenotypeList에 생귀오파지를 추가하는 방식 대신 직접 xenotype 필드를 Reflection으로 설정
3. 나머지 유전자 복사 로직은 원본 그대로 실행

### 방법 B: whiteXenotypeList 확장 (간단하지만 부작용 있음)

`Races_Rakinlike.xml`의 `whiteXenotypeList`에 `Sanguophage` 추가.

- **장점**: XML 한 줄 수정으로 해결
- **단점**: 랫킨이 폰 생성 시 Sanguophage xenotype으로 스폰될 수 있는 부작용, 다른 xenotype 이식도 같은 문제가 발생할 수 있음 (모든 xenotype을 화이트리스트에 추가 불가)

### 방법 C: `SetXenotype` 패치에서 ReimplantXenogerm 컨텍스트 감지

기존 AlienRace의 `SetXenotypePrefix`가 차단하지 않도록, `ReimplantXenogerm` 호출 컨텍스트에서는 제한을 풀어주는 래퍼 패치.

- 기존 `SetXenotypePrefix`를 건드리지 않고, `ReimplantXenogerm`에 Prefix/Postfix를 걸어 플래그를 세팅 후 `SetXenotype`에 추가 Postfix로 처리

## 권장안: 방법 A

`GeneUtility.ReimplantXenogerm`에 Harmony Prefix를 걸어 recipient가 랫킨일 때:

1. caster의 xenotype을 recipient에 강제 설정 (Reflection으로 `xenotype` private 필드 직접 설정하거나, `SetXenotypeDirect` 사용)
2. xenotypeName, iconDef 복사
3. xenogenes 클리어 후 caster의 xenogenes 복사
4. 나머지 로직 (사운드, hediff, extract, replication) 실행
5. 원본 메서드 실행 방지 (`return false`)

혹은 더 간단하게 방법 C 변형: `ReimplantXenogerm`에 Prefix로 **정적 플래그**를 세우고, Postfix에서 해제. 별도로 `SetXenotype`에도 Prefix를 걸어 플래그가 활성 상태면 AlienRace 제한을 우회하여 xenotype을 직접 설정.

## 구현 파일

- **새 파일**: `Project/1.6/Source/Harmony/HarmonyPatch_ReimplantXenogerm.cs`
- **수정 파일**: [Project/1.6/Source/NewRatkin.csproj](Project/1.6/Source/NewRatkin.csproj) (Compile Include 추가)


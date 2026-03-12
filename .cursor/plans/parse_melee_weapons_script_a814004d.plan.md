---
name: Parse Melee Weapons Script
overview: 기존 parse_ranged_weapons.py 구조를 참고하여 근접무기 XML을 파싱하고, tool별 DPS(전설등급, sharp=플라스틸/blunt=우라늄)를 계산하여 캐시 MD + 엑셀로 출력하는 스크립트를 생성합니다.
todos:
  - id: create-script
    content: scripts/parse_melee_weapons.py 생성 - XML 파싱, 부모 상속, tools 추출, DPS 계산, 소재/등급 계수 적용
    status: completed
  - id: output-cache-md
    content: 캐시 MD 출력 (.cursor/def-cache/melee-weapons.md) - YAML 헤더 + DLC별/랫킨 테이블
    status: completed
  - id: output-excel
    content: 엑셀 출력 (ExelData/Melee_Weapon_Balancing.xlsx) - DLC별/랫킨 시트
    status: completed
  - id: test-run
    content: 스크립트 실행 테스트 및 출력 확인
    status: completed
isProject: false
---

# 근접무기 파싱 스크립트 생성

## 생성 파일

- **스크립트**: `scripts/parse_melee_weapons.py`
- **캐시 출력**: `.cursor/def-cache/melee-weapons.md` (기존 수동 캐시 덮어쓰기)
- **엑셀 출력**: `ExelData/Melee_Weapon_Balancing.xlsx`

## 핵심 로직

### 1. 근접무기 식별 기준

기존 `parse_ranged_weapons.py`의 XML 파싱/부모 상속 시스템(`collect_all_thingdefs`, `resolve_def`, `parse_defs_from_file`)을 재활용하되, 필터 조건을 변경:

- `tools` 요소가 존재하고
- `verbs`에 `Verb_Shoot`/`Verb_LaunchProjectile`가 **없는** ThingDef
- `Abstract="True"` 제외
- 예외: Gunlance처럼 근접+원거리 혼합은 tools가 있으면 포함 (melee-weapons.md 기존 범위 유지)

### 2. 출력 테이블 구조 (tool별 1행)

```
| defName | 생산 | damageType | power | cooldown | DPS |
```

- **defName**: 무기 정의명 (같은 무기의 tool이 여러 개면 defName 반복)
- **생산**: `stuff` (stuffCategories 존재) / `Fixed Cost` (costList만) / `생산 불가`
- **damageType**: `capacity(armorCategory)` 형식 (예: `Stab(sharp)`, `Blunt(blunt)`)
  - 복수 capacity: `Blunt(blunt); Poke(blunt)`
  - 추가 데미지: `Stab(sharp)+Flame(heat)` 형식
- **power**: tool의 power 값
- **cooldown**: tool의 cooldown 값
- **DPS**: 전설등급 기준 tool별 DPS

### 3. DPS 계산 공식

```
DPS = (power x DamageMultiplier x MaterialDamageMultiplier) / (cooldown x MaterialCooldownMultiplier)
```

**등급 계수** (전설):

- `MeleeWeapon_DamageMultiplier` = **1.65** ([combat-coefficients.md](d:\GitProject\Ratkin.cursor\def-cache\combat-coefficients.md))

**소재 계수** (stuff 무기만 적용, Fixed Cost/생산 불가는 소재 계수 1.0):

- **sharp 계열** (Cut, Stab, Scratch 등): **플라스틸** → SharpDamageMultiplier=1.1, CooldownMultiplier=0.8
- **blunt 계열** (Blunt, Poke, Demolish 등): **우라늄** → BluntDamageMultiplier=1.5, CooldownMultiplier=1.1
- **heat 계열** (Flame, Burn): 소재 계수 없음 → DamageMultiplier=1.0, CooldownMultiplier=소재에 따름

**복합 damageType** (예: `Stab(sharp)+Flame(heat)`):

- 주 데미지(Stab)에 해당하는 소재 계수 적용, 추가 데미지(Flame)는 별도 행으로 분리하지 않고 표기만

**계산 예시** (stuff 무기, sharp):

```
DPS = (power x 1.65 x 1.1) / (cooldown x 0.8)
```

**계산 예시** (Fixed Cost 무기):

```
DPS = (power x 1.65) / cooldown
```

### 4. 소스 분류

- **Core**: `RimworldData/Core/` 하위
- **Royalty/Ideology/Biotech/Anomaly/Odyssey**: 각 DLC 경로
- **랫킨**: `Project/1.6/Defs/` 하위 (Weapon_Melee.xml, Weapon_Util.xml, Weapon_DropOnly.xml, Weapon_HighTech.xml)

### 5. 캐시 MD 출력 형식

기존 [melee-weapons.md](d:\GitProject\Ratkin.cursor\def-cache\melee-weapons.md) 형식 유지:

```markdown
---
category: melee-weapons
last_updated: YYYY-MM-DD
sources:
  - RimworldData/Core/...
  - Project/1.6/...
scope: 순수 근접 무기만 (원거리 무기의 근접 tools 제외)
fields: defName, 생산, damageType, power, cooldown, DPS
note: DPS = 전설등급 기준. stuff→sharp:플라스틸/blunt:우라늄, Fixed Cost/생산불가→소재계수 미적용
---

# 근접 무기 (Melee Weapons)

## 림월드 (Core + DLC)

### Core (N개)
| defName | 생산 | damageType | power | cooldown | DPS |
|---------|------|------------|-------|----------|-----|
...

### Royalty (N개)
...

## 랫킨 (Ratkin)

### Weapon_Melee.xml (N개)
...
### Weapon_Util.xml (N개)
...
```

### 6. 엑셀 출력

- `ExelData/Melee_Weapon_Balancing.xlsx`
- 시트 구성: Rimworld(전체), Ratkin, Core, Royalty, ... (DLC별)
- 헤더 스타일: 파란 배경, 흰 글씨, 테두리, 자동 필터, 열 너비 자동 조정
- 기존 `parse_ranged_weapons.py`의 `_write_sheet` 패턴 재활용

## 코드 구조

`parse_ranged_weapons.py`에서 재활용하는 함수:

- `parse_defs_from_file()`, `collect_all_thingdefs()`, `resolve_def()`
- `get_text()`, `get_float()`, `find_child()`, `find_direct()`, `extract_stat_bases()`
- `_markdown_table()`, `source_to_dlc()`, 엑셀 출력 패턴

새로 구현하는 부분:

- `extract_tools()`: ThingDef에서 tools 요소 파싱 (capacity, power, cooldown, armorPenetration, extraMeleeDamages)
- `is_melee_weapon()`: 근접무기 판별 (tools 존재 + Verb_Shoot 없음)
- `classify_production()`: stuff / Fixed Cost / 생산 불가 판별
- `calc_melee_dps()`: tool별 DPS 계산 (등급 + 소재 계수 적용)
- `capacity_to_armor_category()`: capacity → armorCategory 매핑 (damage-type-mapping.md 기반 하드코딩)


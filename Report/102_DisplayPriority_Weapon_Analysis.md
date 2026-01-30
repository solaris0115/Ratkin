# DisplayPriority 무기 분석 보고서

**태그**: DisplayPriority Weapon Order 정렬 순서 무기 레시피

**작성일**: 2026-01-29

**분석 대상**: 림월드 코어 및 Ratkin 프로젝트 무기의 `displayPriority` 속성

---

## 1. 개요

`displayPriority`는 림월드에서 레시피의 제작 메뉴 표시 순서를 제어하는 속성입니다. 이 보고서는 무기에 한정하여 `displayPriority`의 사용 현황과 패턴을 분석하고, 랫킨 무기에 대한 권장 값을 제안합니다.

---

## 2. displayPriority 기본 개념

### 2.1 위치 및 구조

`displayPriority`는 `ThingDef`의 `recipeMaker` 섹션 내에 위치합니다:

```xml
<ThingDef ParentName="BaseWeapon">
    <defName>RK_Weapon_RatHolicGun</defName>
    <!-- ... -->
    <recipeMaker>
        <recipeUsers>
            <li>RK_FueledSmithy</li>
            <li>RK_ElectricSmithy</li>
        </recipeUsers>
        <displayPriority>450</displayPriority>
    </recipeMaker>
</ThingDef>
```

### 2.2 값의 의미

- **숫자 값**: 정수로 표현되며, **낮을수록 우선순위가 높습니다**
- **정렬 순서**: 제작 메뉴에서 낮은 값이 먼저 표시됨
- **기본값**: `displayPriority`가 없으면 기본값(보통 0 또는 매우 높은 값)이 적용됨

---

## 3. 림월드 코어 무기 displayPriority 분석

### 3.1 일반적인 패턴

림월드 코어 무기들은 일반적으로 다음과 같은 패턴을 따릅니다:

- **기본 무기 (Neolithic/Medieval)**: displayPriority 없음 또는 낮은 값 (100-300)
- **산업 시대 무기 (Industrial)**: 중간 값 (300-500)
- **우주 시대 무기 (Spacer/Ultratech)**: 높은 값 (500-700)

### 3.2 무기 카테고리별 일반적인 범위

| 카테고리 | 일반 범위 | 설명 |
|---------|----------|------|
| 근접 무기 (Melee) | 100-400 | 단검, 검, 철퇴 등 |
| 원거리 무기 (Ranged) | 200-500 | 활, 석궁, 총기류 |
| 고급 무기 (Advanced) | 400-600 | 첨단 무기, 특수 무기 |
| 유틸리티 도구 | 50-200 | 도끼, 곡괭이 등 작업 도구 |

---

## 4. Ratkin 프로젝트 displayPriority 현황

### 4.1 현재 사용 현황

#### 4.1.1 displayPriority가 있는 무기

1. **Weapon_Range.xml**
   - `RK_Weapon_RatHolicGun`: 450

#### 4.1.2 displayPriority가 없는 무기

다음 파일들에는 `displayPriority`가 **없습니다**:

- `Weapon_Range.xml` (대부분의 원거리 무기)
- `Weapon_Melee.xml` (모든 근접 무기)
- `Weapon_HighTech.xml` (고급 무기)
- `Weapon_Util.xml` (유틸리티 도구)
- `Weapon_DropOnly.xml` (드롭 전용 무기)

### 4.2 무기 목록 및 분류

#### 4.2.1 원거리 무기 (Weapon_Range.xml)

**1티어 (Neolithic)**:
- `RK_Crossbow`: 없음
- `RK_AutoCrossBow`: 없음
- `RK_Weapon_Arbalest`: 없음

**2티어 (Industrial)**:
- `RK_Rifle`: 없음
- `RK_SniperRifle`: 없음
- `RK_Rifle_line` (샷건): 없음

**3티어 (Advanced Industrial)**:
- `RK_FlechetteRifle`: 없음
- `RK_FlechetteSniperRifle`: 없음

**4티어 (Spacer/Ultratech)**:
- `RK_Weapon_Bolter`: 없음
- `RK_PrototypePulseRifle`: 없음
- `RK_Weapon_BFR`: 없음
- `RK_Weapon_RatHolicGun`: **450** (이미 설정됨)

#### 4.2.2 근접 무기 (Weapon_Melee.xml)

**기본 근접 무기**:
- `RK_Dagger`: 없음
- `RK_OneHanded`: 없음
- `RK_Mace`: 없음
- `RK_LightLance`: 없음
- `RK_TwoHanded`: 없음
- `RK_HeavyLance`: 없음
- `RK_LongSword`: 없음
- `RK_Spear`: 없음
- `RK_Halberd`: 없음

#### 4.2.3 고급 무기 (Weapon_HighTech.xml)

- `RK_Weapon_Gunlance`: 없음
- `RK_Weapon_ProtoChainSword`: 없음
- `RK_Weapon_ProtoFlameChainSword`: 없음

#### 4.2.4 유틸리티 도구 (Weapon_Util.xml)

- `RK_Axe`: 없음
- `RK_Cleaver`: 없음
- `RK_Hockey` (쟁기): 없음
- `RK_Fork`: 없음
- `RK_Pickaxe`: 없음

#### 4.2.5 드롭 전용 무기 (Weapon_DropOnly.xml)

- `RK_MagicWand`: 없음 (레시피 없음)

---

## 5. displayPriority 권장 값 제안

### 5.1 전체 범위 전략

**78000번대부터 시작하여 랫킨 무기 전용 범위 할당**:

- **78000-78999**: 랫킨 무기 전용 범위
- 티어별로 100 단위씩 구분
- 카테고리별로 10 단위씩 구분

### 5.2 티어별 권장 범위

| 티어 | 권장 범위 | 설명 |
|------|----------|------|
| 유틸리티 도구 | 78000-78050 | 작업 도구 (도끼, 곡괭이 등) |
| 1티어 (Neolithic) | 78100-78199 | 기본 원거리 무기 (석궁 등) |
| 1티어 (Medieval) | 78200-78299 | 기본 근접 무기 (단검, 검 등) |
| 2티어 (Industrial) | 78300-78399 | 산업 시대 무기 (라이플, 샷건 등) |
| 3티어 (Advanced) | 78400-78499 | 고급 무기 (플레셰트 라이플 등) |
| 4티어 (Spacer/Ultratech) | 78500-78599 | 첨단 무기 (볼터, 건랜스 등) |

### 5.3 카테고리별 세부 제안

#### 5.3.1 유틸리티 도구 (78000-78050)

```
RK_Axe: 78000
RK_Cleaver: 78005
RK_Hockey: 78010
RK_Fork: 78015
RK_Pickaxe: 78020
```

#### 5.3.2 원거리 무기 - 1티어 (78100-78199)

```
RK_Crossbow: 78100
RK_AutoCrossBow: 78110
RK_Weapon_Arbalest: 78120
```

#### 5.3.3 근접 무기 - 1티어 (78200-78299)

```
RK_Dagger: 78200
RK_OneHanded: 78210
RK_Mace: 78220
RK_Spear: 78230
RK_LightLance: 78240
RK_TwoHanded: 78250
RK_HeavyLance: 78260
RK_LongSword: 78270
RK_Halberd: 78280
```

#### 5.3.4 원거리 무기 - 2티어 (78300-78399)

```
RK_Rifle: 78300
RK_SniperRifle: 78310
RK_Rifle_line (샷건): 78320
```

#### 5.3.5 원거리 무기 - 3티어 (78400-78499)

```
RK_FlechetteRifle: 78400
RK_FlechetteSniperRifle: 78410
```

#### 5.3.6 고급 무기 - 4티어 (78500-78599)

```
RK_Weapon_Bolter: 78500
RK_PrototypePulseRifle: 78510
RK_Weapon_BFR: 78520
RK_Weapon_RatHolicGun: 78530 (현재 450 → 변경 필요)
RK_Weapon_Gunlance: 78540
RK_Weapon_ProtoChainSword: 78550
RK_Weapon_ProtoFlameChainSword: 78560
```

---

## 6. 구현 권장사항

### 6.1 우선순위

1. **1단계**: 고급 무기 (4티어)부터 적용
   - 가장 중요한 무기들
   - 현재 `RK_Weapon_RatHolicGun`의 값도 업데이트 필요

2. **2단계**: 기본 무기 (1-2티어) 적용
   - 가장 많이 사용되는 무기들

3. **3단계**: 유틸리티 도구 적용
   - 작업 도구들

### 6.2 값 조정 가이드

- **값 사이 여유**: 향후 추가/조정을 위해 5-10 단위씩 여유를 둠
- **논리적 그룹화**: 같은 티어/카테고리는 연속된 값 사용
- **확장성**: 새로운 무기 추가 시 기존 범위 내에서 값 할당 가능하도록 설계

### 6.3 기존 값 업데이트

- `RK_Weapon_RatHolicGun`: 현재 450 → **78530**으로 변경 권장

---

## 7. 결론 및 권장사항

### 7.1 현재 상태

- **림월드 코어**: 대부분의 무기에 `displayPriority`가 없거나 낮은 값 사용
- **Ratkin 프로젝트**: 대부분의 무기에 `displayPriority`가 **누락**되어 있음
- **영향**: 제작 메뉴에서 무기 순서가 일관되지 않을 수 있음

### 7.2 권장 조치사항

1. **1단계**: 모든 무기에 `displayPriority` 추가
   - 78000번대 범위 사용
   - 티어별로 적절한 범위 할당
   - 카테고리별로 논리적인 순서 유지

2. **2단계**: 테스트 및 조정
   - 게임 내 제작 메뉴 확인
   - 사용자 경험에 맞게 값 조정

3. **3단계**: 문서화
   - `displayPriority` 값 범위 가이드라인 문서화
   - 향후 무기 추가 시 참고용

### 7.3 참고사항

- `displayPriority`는 **낮을수록 우선순위가 높음**
- 값 사이에 여유를 두어 향후 추가/조정이 용이하도록 함
- 림월드 코어 패턴을 참고하되, 프로젝트 특성에 맞게 조정
- **78000번대부터 시작하여 랫킨 무기 전용 범위 확보**

---

## 8. 참고 자료

- **프로젝트 무기**: `Project/1.6/Defs/ThingsDefs/Weapon_*.xml`
- **의류 displayPriority 분석**: `Report/101_DisplayPriority_Apparel_Recipe_Analysis.md`
- **유니크 무기 목록**: `유니크 무기 목록.md`

---

**작성자**: AI Assistant  
**검토 필요**: displayPriority 값 범위 최종 확인 필요

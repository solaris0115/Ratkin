# 건랜스 사운드 개선 제안서

## 개요
HellcatBurner와 림월드 코어 사운드 정의를 참고하여 Weapon_HighTech.xml의 건랜스 사운드들을 개선하는 제안서입니다.

## 현재 상황 분석

### 현재 사운드 정의 (Weapon_HighTech.xml 250-345 라인)

| SoundDef 이름 | 클립 경로 | 사용 용도 | GunlanceDefOf 매핑 |
|--------------|----------|----------|-------------------|
| `RK_Sound_Warmup` | `Gunlance/Charge` | 워밍업 | `RK_Charge` |
| `RK_Sound_GunlanceNormalFire` | `Gunlance/Fire` | 일반 발사 | `RK_Fire` |
| `RK_Sound_Empty` | `Gunlance/OverHeat` | 과열/빈 탄창 | `RK_OverHeat` |
| `RK_Sound_WyvernFire` | `Gunlance/WyvernFire` | 용격포 발사 | `RK_WyvernFire` |
| `RK_Sound_Reload` | `Gunlance/Reload` | 재장전 | `RK_Reload` |

### 현재 사운드 설정
- `maxSimultaneous`: 1 (모두 동일)
- `pitchRange`: 0.9152174 ~ 1.042391 (모두 동일)
- `volumeRange`: 없음
- `grains`: `AudioGrain_Clip` 사용
- `context`: `MapOnly`

### 문제점
1. **이름 불일치**: SoundDef 이름과 GunlanceDefOf 필드명이 일치하지 않음
2. **HellcatBurner와 차이**: HellcatBurner는 더 풍부한 설정 사용
3. **volumeRange 부재**: 볼륨 조절 불가
4. **maxSimultaneous 제한**: 동시 재생 제한이 너무 낮을 수 있음

## HellcatBurner 참고 사항

### FireSpew_Warmup (warmupStartSound)
```xml
<SoundDef>
    <defName>FireSpew_Warmup</defName>
    <context>MapOnly</context>
    <maxSimultaneous>5</maxSimultaneous>
    <priorityMode>PrioritizeNearest</priorityMode>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Folder">
                    <clipFolderPath>Pawn/Abilities/FireSpew/Warmup</clipFolderPath>
                </li>
            </grains>
            <volumeRange>60</volumeRange>
            <muteWhenPaused>True</muteWhenPaused>
            <tempoAffectedByGameSpeed>true</tempoAffectedByGameSpeed>
        </li>
    </subSounds>
</SoundDef>
```

### FireSpew_Resolve (soundCast)
```xml
<SoundDef>
    <defName>FireSpew_Resolve</defName>
    <context>MapOnly</context>
    <maxSimultaneous>5</maxSimultaneous>
    <priorityMode>PrioritizeNearest</priorityMode>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Folder">
                    <clipFolderPath>Pawn/Abilities/FireSpew/Resolve</clipFolderPath>
                </li>
            </grains>
            <volumeRange>60</volumeRange>
            <muteWhenPaused>True</muteWhenPaused>
            <tempoAffectedByGameSpeed>true</tempoAffectedByGameSpeed>
        </li>
    </subSounds>
</SoundDef>
```

### Standard_Reload (soundReload)
```xml
<SoundDef>
    <defName>Standard_Reload</defName>
    <context>MapOnly</context>
    <maxSimultaneous>1</maxSimultaneous>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>UI/WeaponHandling/HandleWeaponB</clipPath>
                </li>
            </grains>
        </li>
    </subSounds>
</SoundDef>
```

## 코어 사운드 참고 사항

### 일반 총기 사운드 패턴
- **발사 사운드**: `volumeRange` 35~94.71 (무기 종류에 따라 다름)
- **피치 범위**: 다양함 (0.7~1.85)
- **AudioGrain_Folder**: 여러 클립을 가진 폴더 사용 (변화성 증가)
- **AudioGrain_Clip**: 단일 클립 사용

## 개선 제안

### 제안 1: 이름 통일 및 기본 개선

#### RK_Charge (워밍업/충전)
```xml
<SoundDef>
    <defName>RK_Charge</defName>
    <eventNames />
    <context>MapOnly</context>
    <maxSimultaneous>3</maxSimultaneous>
    <priorityMode>PrioritizeNearest</priorityMode>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Gunlance/Charge</clipPath>
                </li>
            </grains>
            <volumeRange>50~60</volumeRange>
            <pitchRange>
                <min>0.9</min>
                <max>1.1</max>
            </pitchRange>
            <muteWhenPaused>True</muteWhenPaused>
            <tempoAffectedByGameSpeed>true</tempoAffectedByGameSpeed>
        </li>
    </subSounds>
</SoundDef>
```

**변경 사항:**
- 이름: `RK_Sound_Warmup` → `RK_Charge` (GunlanceDefOf와 일치)
- `maxSimultaneous`: 1 → 3 (여러 건랜스 동시 사용 고려)
- `volumeRange` 추가: 50~60
- `priorityMode` 추가: `PrioritizeNearest`
- `muteWhenPaused`, `tempoAffectedByGameSpeed` 추가 (HellcatBurner 스타일)

#### RK_Fire (일반 발사)
```xml
<SoundDef>
    <defName>RK_Fire</defName>
    <eventNames />
    <context>MapOnly</context>
    <maxSimultaneous>3</maxSimultaneous>
    <priorityMode>PrioritizeNearest</priorityMode>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Gunlance/Fire</clipPath>
                </li>
            </grains>
            <volumeRange>55~65</volumeRange>
            <pitchRange>
                <min>0.9</min>
                <max>1.1</max>
            </pitchRange>
        </li>
    </subSounds>
</SoundDef>
```

**변경 사항:**
- 이름: `RK_Sound_GunlanceNormalFire` → `RK_Fire`
- `maxSimultaneous`: 1 → 3
- `volumeRange` 추가: 55~65 (발사 사운드는 약간 더 크게)
- `priorityMode` 추가

#### RK_OverHeat (과열/빈 탄창)
```xml
<SoundDef>
    <defName>RK_OverHeat</defName>
    <eventNames />
    <context>MapOnly</context>
    <maxSimultaneous>2</maxSimultaneous>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Gunlance/OverHeat</clipPath>
                </li>
            </grains>
            <volumeRange>40~50</volumeRange>
            <pitchRange>
                <min>0.85</min>
                <max>1.0</max>
            </pitchRange>
        </li>
    </subSounds>
</SoundDef>
```

**변경 사항:**
- 이름: `RK_Sound_Empty` → `RK_OverHeat`
- `maxSimultaneous`: 1 → 2
- `volumeRange` 추가: 40~50 (경고음이므로 약간 낮게)
- 피치 범위 조정: 더 낮은 톤 (경고 느낌)

#### RK_WyvernFire (용격포 발사)
```xml
<SoundDef>
    <defName>RK_WyvernFire</defName>
    <eventNames />
    <context>MapOnly</context>
    <maxSimultaneous>2</maxSimultaneous>
    <priorityMode>PrioritizeNearest</priorityMode>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Gunlance/WyvernFire</clipPath>
                </li>
            </grains>
            <volumeRange>70~80</volumeRange>
            <pitchRange>
                <min>0.8</min>
                <max>1.0</max>
            </pitchRange>
        </li>
    </subSounds>
</SoundDef>
```

**변경 사항:**
- 이름: `RK_Sound_WyvernFire` → `RK_WyvernFire` (이미 일치)
- `maxSimultaneous`: 1 → 2
- `volumeRange` 추가: 70~80 (강력한 공격이므로 더 크게)
- 피치 범위 조정: 더 낮고 무거운 느낌
- `priorityMode` 추가

#### RK_Reload (재장전)
```xml
<SoundDef>
    <defName>RK_Reload</defName>
    <eventNames />
    <context>MapOnly</context>
    <maxSimultaneous>1</maxSimultaneous>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Gunlance/Reload</clipPath>
                </li>
            </grains>
            <volumeRange>35~45</volumeRange>
            <pitchRange>
                <min>0.9</min>
                <max>1.1</max>
            </pitchRange>
        </li>
    </subSounds>
</SoundDef>
```

**변경 사항:**
- 이름: `RK_Sound_Reload` → `RK_Reload`
- `maxSimultaneous`: 1 유지 (재장전은 동시에 하나만)
- `volumeRange` 추가: 35~45 (조작음이므로 적당히)
- 피치 범위 유지

### 제안 2: AudioGrain_Folder 사용 (선택사항)

여러 클립 파일이 있다면 `AudioGrain_Folder` 사용을 고려:

```xml
<grains>
    <li Class="AudioGrain_Folder">
        <clipFolderPath>Gunlance/Fire</clipFolderPath>
    </li>
</grains>
```

**장점:**
- 여러 클립 중 랜덤 선택으로 변화성 증가
- 더 자연스러운 사운드

**단점:**
- 폴더 구조 필요
- 단일 클립만 있다면 불필요

## 권장 사항

### 즉시 적용 권장
1. ✅ **이름 통일**: GunlanceDefOf와 일치하도록 변경
2. ✅ **volumeRange 추가**: 각 사운드 특성에 맞는 볼륨 설정
3. ✅ **maxSimultaneous 조정**: 사운드 종류에 따라 적절히 설정
4. ✅ **priorityMode 추가**: 중요한 사운드에 `PrioritizeNearest` 적용

### 선택적 개선
1. ⚠️ **AudioGrain_Folder**: 여러 클립이 있을 때만 적용
2. ⚠️ **muteWhenPaused, tempoAffectedByGameSpeed**: 워밍업 사운드에만 적용 고려
3. ⚠️ **피치 범위 조정**: 실제 오디오 파일 특성에 맞게 미세 조정

## 적용 우선순위

1. **높음**: 이름 통일 (코드 일관성)
2. **높음**: volumeRange 추가 (사운드 밸런스)
3. **중간**: maxSimultaneous 조정 (게임플레이 경험)
4. **낮음**: 추가 옵션들 (세밀한 조정)

## 참고 자료

- `RimworldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml` (HellcatBurner)
- `RimworldData/Core/Defs/SoundDefs/Interact_Oneshots_Misc.xml` (FireSpew 사운드)
- `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Weapons.xml` (총기 사운드)
- `Project/1.6/Source/Gunlance/GunlanceDefOf.cs` (DefOf 정의)








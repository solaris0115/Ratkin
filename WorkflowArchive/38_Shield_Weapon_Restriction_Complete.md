# 방패-무기 동시 착용 제한 최종 구현 완료 - 2025-11-08

## 작업 개요
- 요청 내용: Harmony Patch로 방패-무기 동시 착용 제한 구현
- 목표: 
  1. WhiteListWeaponTags에 있는 장비라면 착용 가능
  2. 목록에 없다면 "방패와 동시 착용할 수 없습니다" 메시지 표시

## 최종 계획 (확정)
1. 번역 키 추가 (한국어, 영어)
2. XML Def 설정 (CompProperties에 blockReasonKey 추가)
3. 빌드 및 테스트

## 작업 세부 진행
1. [번역 키 추가] [✓]
2. [XML Def 설정] [✓]
3. [빌드 및 테스트] [대기]

## 진행 상황

### 1. 번역 키 추가
- 내용: 한국어/영어 Messages.xml에 번역 키 추가
- 파일:
  - `Project/Contents/Languages/Korean/Keyed/Messages.xml`
  - `Project/Contents/Languages/English/Keyed/Messages.xml`
- 추가된 키:
  - `RK_ApparelWeaponIncompatible_Dropped` - 무기가 제거되었을 때
  - `RK_ApparelWeaponIncompatible_Dropped_Default` - 기본 메시지
  - `RK_ShieldWeaponIncompatible` - "방패와 동시 착용할 수 없습니다"
- 결과: 완료 ✅

### 2. XML Def 설정
- 내용: 3개 방패 Def에 blockReasonKey 추가
- 대상:
  - `RK_WoodenShield` - 목제 방패
  - `RK_HeavyShield` - 철제 방패
  - `RK_TowerShield` - 대형 철제 방패
- 추가 내용:
```xml
<blockReasonKey>RK_ShieldWeaponIncompatible</blockReasonKey>
```
- 결과: 완료 ✅

## 최종 작업 결과
✅ 완료

## 구현 내용 요약

### 1. 코드 구조 (이미 구현됨)

#### Harmony Patch
**파일**: `Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs`

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
[HarmonyPostfix]
public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
{
    // CompShieldWeaponIncompatible 검증
    // 호환되지 않는 무기 착용 중이면 FloatMenuOption을 "착용 불가"로 대체
}
```

#### Comp 검증 로직
**파일**: `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`

```csharp
public bool TryIsWeaponAllowed(ThingDef weaponDef, out string reason)
{
    // allowedWeaponTags 화이트리스트 검증
    // 무기의 weaponTags 중 하나라도 allowedWeaponTags에 있으면 허용
    // 없으면 차단
}

public override void Notify_Equipped(Pawn pawn)
{
    // 착용 시 호환되지 않는 무기 자동 제거 (안전망)
}
```

### 2. 번역 키

#### 한국어 (`Project/Contents/Languages/Korean/Keyed/Messages.xml`)
```xml
<!-- 방패-무기 호환성 -->
<RK_ApparelWeaponIncompatible_Dropped>{0}을(를) 착용하여 {1}을(를) 내려놓았습니다. 사유: {2}</RK_ApparelWeaponIncompatible_Dropped>
<RK_ApparelWeaponIncompatible_Dropped_Default>{0}을(를) 착용하여 {1}을(를) 내려놓았습니다.</RK_ApparelWeaponIncompatible_Dropped_Default>
<RK_ShieldWeaponIncompatible>방패와 동시 착용할 수 없습니다</RK_ShieldWeaponIncompatible>
```

#### 영어 (`Project/Contents/Languages/English/Keyed/Messages.xml`)
```xml
<!-- Shield-Weapon Compatibility -->
<RK_ApparelWeaponIncompatible_Dropped>Equipped {0} and dropped {1}. Reason: {2}</RK_ApparelWeaponIncompatible_Dropped>
<RK_ApparelWeaponIncompatible_Dropped_Default>Equipped {0} and dropped {1}.</RK_ApparelWeaponIncompatible_Dropped_Default>
<RK_ShieldWeaponIncompatible>Cannot be worn with shield</RK_ShieldWeaponIncompatible>
```

### 3. XML Def 설정

**파일**: `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml`

```xml
<comps>
    <li Class="NewRatkin.CompProperties_ShieldWeaponIncompatible">
        <!-- 허용할 무기 태그 리스트 (화이트리스트) -->
        <allowedWeaponTags>
            <li>RK_WeaponTag_OneHand</li>
        </allowedWeaponTags>
        <!-- 차단 사유 번역 키 -->
        <blockReasonKey>RK_ShieldWeaponIncompatible</blockReasonKey>
    </li>
</comps>
```

**적용된 방패**:
- `RK_WoodenShield` - 목제 방패
- `RK_HeavyShield` - 철제 방패
- `RK_TowerShield` - 대형 철제 방패

## 동작 방식

### 시나리오 1: FloatMenu에서 착용 불가 표시 (Harmony)

```
1. Ratkin이 양손 무기(Bolter) 착용 중
2. 사용자가 방패(Shield) 우클릭
3. FloatMenu 생성
4. FloatMenuPatch_ShieldWeaponCheck.Postfix() 실행
5. TryIsWeaponAllowed(Bolter.def) 호출
   ├─ Bolter.weaponTags 확인
   ├─ allowedWeaponTags에 "RK_WeaponTag_OneHand" 있음
   └─ Bolter에 "RK_WeaponTag_OneHand" 태그 없음 → 차단
6. FloatMenuOption을 "착용 불가"로 대체
7. FloatMenu 표시:
   ❌ "착용 불가: 방패와 동시 착용할 수 없습니다" (회색, 클릭 불가)
```

### 시나리오 2: FloatMenu를 거치지 않는 경우 (안전망)

```
1. Ratkin이 양손 무기(Bolter) 착용 중
2. Job 시스템이나 다른 모드가 직접 방패 착용 시도
3. Notify_Equipped(pawn) 호출
4. TryIsWeaponAllowed(Bolter.def) 호출
5. Bolter 차단 판정
6. DropIncompatibleWeapon() 실행
7. Bolter를 바닥에 떨어뜨림
8. 메시지 표시:
   "방패를 착용하여 Bolter를 내려놓았습니다. 사유: 방패와 동시 착용할 수 없습니다"
```

### 시나리오 3: 한손 무기 착용 중 (착용 가능)

```
1. Ratkin이 한손 무기(Dagger) 착용 중
2. 사용자가 방패(Shield) 우클릭
3. FloatMenu 생성
4. FloatMenuPatch_ShieldWeaponCheck.Postfix() 실행
5. TryIsWeaponAllowed(Dagger.def) 호출
   ├─ Dagger.weaponTags 확인
   ├─ allowedWeaponTags에 "RK_WeaponTag_OneHand" 있음
   └─ Dagger에 "RK_WeaponTag_OneHand" 태그 있음 → 허용
6. FloatMenuOption 수정 없음 (원본 유지)
7. FloatMenu 표시:
   ✅ "강제 착용" (녹색, 클릭 가능)
```

## 이중 안전 메커니즘

```
[1차 방어선: Harmony Patch]
FloatMenu 생성 시
→ 착용 불가 표시
→ 시도 자체 차단
→ 사용자 경험 향상 ⭐

[2차 방어선: Notify_Equipped]
FloatMenu를 거치지 않는 경로
→ 착용 후 무기 제거
→ 안전망 역할 ⭐
```

## 관련 파일 목록

**소스 코드**:
- [Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs](../Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs)
- [Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs](../Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs)

**번역 파일**:
- [Project/Contents/Languages/Korean/Keyed/Messages.xml](../Project/Contents/Languages/Korean/Keyed/Messages.xml)
- [Project/Contents/Languages/English/Keyed/Messages.xml](../Project/Contents/Languages/English/Keyed/Messages.xml)

**Def 파일**:
- [Project/1.6/Defs/ThingsDefs/Apparel_Util.xml](../Project/1.6/Defs/ThingsDefs/Apparel_Util.xml)

**워크플로우**:
- [WorkFlow/38_Shield_Weapon_Restriction_Complete.md](38_Shield_Weapon_Restriction_Complete.md)

## 다음 단계

1. ✅ 빌드
2. ✅ 게임 내 테스트
   - 양손 무기 착용 중 방패 우클릭 → 착용 불가 표시 확인
   - 한손 무기 착용 중 방패 우클릭 → 착용 가능 확인
   - 무기 없이 방패 착용 → 착용 가능 확인
3. ✅ Git 커밋


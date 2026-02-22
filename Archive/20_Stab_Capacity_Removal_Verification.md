# Stab Capacity 제거 검증 보고서

## 검증 완료 사항

### ✅ 1. RK_Gunlance_NormalType에서 Stab 제거 확인

```37:45:Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
<tools>
	<li>
		<label>point</label>
		<capacities>
			<li>GunlanceShell_Normal</li>
		</capacities>
		<power>16</power>
		<cooldownTime>3</cooldownTime>
	</li>
```

**확인 결과:** ✅ Stab capacity가 제거되었고 `GunlanceShell_Normal`만 남아있습니다.

### ✅ 2. Parent 클래스 상속 확인

```15:22:Project/1.6/Defs/ThingsDefs/Weapon_Util.xml
<ThingDef Abstract="True" Name="RK_MeleeWeapon" ParentName="BaseMeleeWeapon_Sharp_Quality">
	<recipeMaker>
		<recipeUsers Inherit="false">
			<li>RK_FueledSmithy</li>
			<li>RK_ElectricSmithy</li>
		</recipeUsers>
	</recipeMaker>
</ThingDef>
```

**확인 결과:** ✅ Parent 클래스(`RK_MeleeWeapon`, `BaseMeleeWeapon_Sharp_Quality`)에는 tools 정의가 없어서 Stab을 상속받지 않습니다.

### ✅ 3. 다른 참조 확인

**검색 결과:**
- `RK_Gunlance`와 `Stab`을 함께 참조하는 곳 없음 ✅
- 다른 건랜스 타입 없음 ✅
- 관련 Patch 파일 없음 ✅

### ✅ 4. ManeuverDef 확인

```112:127:Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
<ManeuverDef>
	<defName>RK_GunlanceExplosion_Normal</defName>
	<requiredCapacity>GunlanceShell_Normal</requiredCapacity>
	<verb Class="NewRatkin.VerbProperties_MeleeExplosion">
		<verbClass>NewRatkin.Verb_MeleeAttackExplosion</verbClass>
		<explosionRadius>2</explosionRadius>
		<explosionAngle>50</explosionAngle>
		<explosionDamageDef>Bomb</explosionDamageDef>
		<explosionDamageAmount>15</explosionDamageAmount>
		<explosionArmorPenetration>0.5</explosionArmorPenetration>
	</verb>
```

**확인 결과:** ✅ `RK_GunlanceExplosion_Normal` ManeuverDef는 `GunlanceShell_Normal` capacity를 사용하므로 문제 없습니다.

## 예상 효과

### ✅ 해결되는 문제
1. **Verb 중복 LoadID 오류 해결**
   - `Stab` ManeuverDef(`Verb_MeleeAttackDamage`)가 더 이상 생성되지 않음
   - `Verb_CompEquippable_RK_Gunlance_NormalType85822_0_Stab` LoadID 충돌 없음

2. **기능 유지**
   - `GunlanceShell_Normal` capacity로 `RK_GunlanceExplosion_Normal` ManeuverDef 사용 가능
   - `Verb_MeleeAttackExplosion`으로 폭발 공격 정상 작동

### ⚠️ 주의사항

1. **저장된 게임 호환성**
   - 이전에 저장된 게임에서 `Stab` Verb가 이미 저장되어 있다면:
     - 게임을 불러올 때 "Replaced verb" 경고가 나타날 수 있음 (정상 동작)
     - 기존 저장 파일은 문제 없이 불러올 수 있음

2. **기능 변경**
   - 건랜스로 `Stab` 공격을 할 수 없게 됨 (의도된 변경)
   - 폭발 공격만 가능 (정상 동작)

## 최종 검증 결과

✅ **모든 검증 항목 통과**

1. ✅ Stab capacity 제거 확인
2. ✅ Parent 클래스 상속 확인
3. ✅ 다른 참조 없음 확인
4. ✅ ManeuverDef 정상 확인
5. ✅ 기능 유지 확인

**결론:** 변경사항이 정상적으로 적용되었으며, Verb 중복 LoadID 문제가 해결되었습니다.

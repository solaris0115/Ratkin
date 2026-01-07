# 에너지 쉴드 장비 내구도 소모 분석 보고서

**작성일**: 2025-01-XX  
**목적**: 림월드 코어의 에너지 쉴드(ShieldBelt)가 장비로 장착되었을 때 피해를 받는 경우 내구도 소모 여부 확인

---

## 핵심 결론

**에너지 쉴드는 피해를 받아도 내구도가 소모되지 않습니다.**

에너지 쉴드는 피해를 흡수할 때 **에너지만 소모**하고, 내구도(HitPoints)는 소모하지 않습니다.

---

## 1. 에너지 쉴드의 피해 처리 메커니즘

### 1.1 피해 처리 흐름

```
Pawn이 피해를 받음
  ↓
Pawn_HealthTracker.PostApplyDamage()
  ↓
Apparel.CheckPreAbsorbDamage() 호출 (Apparel인 경우)
  ↓
CompShield.PostPreApplyDamage() 호출 ⭐
  ↓
피해 흡수 시 absorbed = true 설정
  ↓
Thing.TakeDamage()에서 absorbed가 true면 DamageWorker.Apply() 호출 안 함
  ↓
내구도 소모 없음 ✅
```

### 1.2 CompShield.PostPreApplyDamage() 메서드

```247:278:RimworldSource/RimWorld/CompShield.cs
public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
{
	absorbed = false;
	if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
	{
		return;
	}
	if (dinfo.Def == DamageDefOf.EMP)
	{
		this.energy = 0f;
		this.Break();
		return;
	}
	if (dinfo.Def.ignoreShields)
	{
		return;
	}
	if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
	{
		this.energy -= dinfo.Amount * this.Props.energyLossPerDamage;
		if (this.energy < 0f)
		{
			this.Break();
		}
		else
		{
			this.AbsorbedDamage(dinfo);
		}
		absorbed = true;  // ⭐ 핵심: 피해를 흡수하여 내구도 소모 방지
		return;
	}
}
```

**동작 방식:**
1. 쉴드가 활성 상태이고 원거리/폭발 피해인 경우
2. 에너지를 소모 (`this.energy -= dinfo.Amount * this.Props.energyLossPerDamage`)
3. **`absorbed = true`로 설정하여 피해를 완전히 흡수**
4. 이로 인해 Thing.TakeDamage()에서 DamageWorker.Apply()가 호출되지 않음

### 1.3 Thing.TakeDamage() 메서드

```1719:1787:RimworldSource/Verse/Thing.cs
public DamageWorker.DamageResult TakeDamage(DamageInfo dinfo)
{
	// ... 생략 ...
	bool flag;
	this.PreApplyDamage(ref dinfo, out flag);
	if (flag)  // ⭐ absorbed가 true면 여기서 반환
	{
		return new DamageWorker.DamageResult();
	}
	// ... 생략 ...
	DamageWorker.DamageResult damageResult = dinfo.Def.Worker.Apply(dinfo, this);
	// ⬆️ absorbed가 true면 이 부분이 실행되지 않음
	// ... 생략 ...
}
```

**핵심:**
- `PreApplyDamage()`에서 `absorbed = true`가 반환되면
- `DamageWorker.Apply()`가 호출되지 않아 내구도가 소모되지 않음

### 1.4 DamageWorker.Apply() - 일반적인 내구도 소모

```150:169:RimworldSource/Verse/DamageWorker.cs
// ... 생략 ...
damageResult.totalDamageDealt = (float)Mathf.Min(victim.HitPoints, GenMath.RoundRandom(num));
victim.HitPoints -= Mathf.RoundToInt(damageResult.totalDamageDealt);  // ⭐ 일반적으로 여기서 내구도 소모
if (victim.HitPoints <= 0)
{
	victim.HitPoints = 0;
	victim.Kill(new DamageInfo?(dinfo), null);
}
```

**일반적인 Thing의 경우:**
- `DamageWorker.Apply()`가 호출되면 내구도가 소모됨
- 하지만 에너지 쉴드는 `absorbed = true`로 설정하여 이 과정을 건너뜀

---

## 2. 에너지 쉴드의 에너지 시스템

### 2.1 에너지 소모

```266:266:RimworldSource/RimWorld/CompShield.cs
this.energy -= dinfo.Amount * this.Props.energyLossPerDamage;
```

**기본값:**
- `energyLossPerDamage = 0.033f` (피해량의 3.3%)
- 예: 10 피해 → 0.33 에너지 소모

### 2.2 에너지 회복

```237:244:RimworldSource/RimWorld/CompShield.cs
else if (this.ShieldState == ShieldState.Active)
{
	this.energy += this.EnergyGainPerTick;
	if (this.energy > this.EnergyMax)
	{
		this.energy = this.EnergyMax;
	}
}
```

**회복 메커니즘:**
- 쉴드가 활성 상태일 때 매 틱마다 에너지 회복
- `EnergyGainPerTick = EnergyShieldRechargeRate / 60f`

### 2.3 쉴드 브레이크

```301:315:RimworldSource/RimWorld/CompShield.cs
private void Break()
{
	// ... 이펙트 처리 ...
	this.energy = 0f;
	this.ticksToReset = this.Props.startingTicksToReset;  // 기본값: 3200틱 (약 53초)
}
```

**브레이크 조건:**
- 에너지가 0 이하로 떨어질 때
- EMP 피해를 받을 때

---

## 3. 일반 Equipment vs 에너지 쉴드 비교

### 3.1 일반 Equipment (무기 등)

- 피해를 받으면 `DamageWorker.Apply()` 호출
- 내구도(HitPoints)가 소모됨
- 내구도가 0이 되면 파괴됨

### 3.2 에너지 쉴드 (ShieldBelt)

- 피해를 받으면 `CompShield.PostPreApplyDamage()`에서 흡수
- **내구도는 소모되지 않음** ✅
- 에너지만 소모됨
- 에너지가 0이 되면 일시적으로 비활성화 (파괴되지 않음)
- 시간이 지나면 자동으로 재충전됨

---

## 4. 에너지 고갈 상태에서의 동작

### 4.1 에너지 고갈 시 ShieldState

```64:84:RimworldSource/RimWorld/CompShield.cs
public ShieldState ShieldState
{
	get
	{
		// ... 생략 ...
		if (this.ticksToReset <= 0)
		{
			return ShieldState.Active;
		}
		return ShieldState.Resetting;  // ⭐ 에너지 고갈 시 Resetting 상태
	}
}
```

**에너지 고갈 시:**
- `ticksToReset > 0`이면 `ShieldState.Resetting` 상태
- `ShieldState != ShieldState.Active`가 됨

### 4.2 에너지 고갈 상태에서의 피해 처리

```247:278:RimworldSource/RimWorld/CompShield.cs
public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
{
	absorbed = false;
	if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
	{
		return;  // ⭐ 에너지 고갈 시 absorbed = false로 반환
	}
	// ... 나머지 코드는 실행되지 않음 ...
}
```

**에너지 고갈 상태에서:**
1. `ShieldState != ShieldState.Active` 조건이 true
2. `absorbed = false`로 반환 (기본값)
3. 피해를 흡수하지 않음
4. **Pawn이 피해를 받게 됨**

### 4.3 Apparel의 피해 처리 흐름

```270:283:RimworldSource/RimWorld/Apparel.cs
public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
{
	List<ThingComp> allComps = base.AllComps;
	for (int i = 0; i < allComps.Count; i++)
	{
		bool flag;
		allComps[i].PostPreApplyDamage(ref dinfo, out flag);
		if (flag)
		{
			return true;  // ⭐ 피해 흡수 성공
		}
	}
	return false;  // ⭐ 피해 흡수 실패 → Pawn이 피해를 받음
}
```

**중요한 점:**
- `Apparel.CheckPreAbsorbDamage()`는 **Pawn이 피해를 받기 전**에 호출됨
- `absorbed = false`면 Pawn이 피해를 받게 됨
- 하지만 **Apparel 자체는 피해를 받지 않음**
- Apparel이 직접 피해를 받는 경우는 별도의 메커니즘임

### 4.4 결론: 에너지 고갈 상태에서도 내구도 소모 없음

**에너지 고갈 상태에서:**
1. 쉴드가 피해를 흡수하지 않음 (`absorbed = false`)
2. Pawn이 피해를 받게 됨
3. **하지만 Apparel(에너지 쉴드) 자체는 피해를 받지 않음**
4. 따라서 **내구도가 소모되지 않음** ✅

**이유:**
- `Apparel.CheckPreAbsorbDamage()`는 Pawn의 피해를 흡수하는 용도
- Apparel 자체가 직접 피해를 받는 경우는 별도 메커니즘
- 에너지 쉴드는 Pawn이 피해를 받을 때만 작동하며, Apparel 자체는 피해를 받지 않음

---

## 5. 결론

1. **에너지 쉴드는 피해를 받아도 내구도가 소모되지 않습니다.**
2. **에너지 고갈 상태에서도 내구도가 소모되지 않습니다.** ✅
3. 대신 **에너지 시스템**을 사용하여 피해를 처리합니다.
4. 에너지가 소모되면 쉴드가 일시적으로 비활성화되지만, 시간이 지나면 자동으로 재충전됩니다.
5. 이는 에너지 쉴드가 **무한정 사용 가능한 장비**임을 의미합니다 (EMP 피해 제외).

---

## 참고 사항

- **EMP 피해**: EMP 피해를 받으면 에너지가 0이 되고 쉴드가 브레이크됩니다.
- **근접 피해**: 에너지 쉴드는 원거리/폭발 피해만 흡수하며, 근접 피해는 흡수하지 않습니다.
- **에너지 고갈 시**: 쉴드는 피해를 흡수하지 않아 Pawn이 피해를 받지만, 쉴드 자체는 피해를 받지 않습니다.
- **ShieldBelt는 Apparel**: 에너지 쉴드는 Equipment가 아닌 Apparel로 구현되어 있습니다.


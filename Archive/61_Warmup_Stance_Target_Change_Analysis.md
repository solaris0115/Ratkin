# Warmup Stance 대상 변경 감지 로직 분석

**작성일**: 2025-01-XX  
**분석 목적**: Warmup stance에서 다른 대상으로 조준을 바꾸는 경우, 대상 변경 감지 메커니즘과 warmup stance 생성 방식 분석

## 핵심 질문

1. **대상이 변경되었음을 뭐로 탐지하는가?**
2. **Warmup stance가 새로 생성되는가, 아니면 기존 것을 재사용하는가?**

## 분석 결과

### 1. Warmup Stance는 매번 새로 생성됨

**Verb.TryStartCastOn** (`RimworldSource/Verse/Verb.cs:481-532`):

```481:532:RimworldSource/Verse/Verb.cs
public virtual bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
{
	if (this.caster == null)
	{
		Log.Error("Verb " + this.GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
		return false;
	}
	if (!this.caster.Spawned)
	{
		return false;
	}
	if (this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))
	{
		return false;
	}
	if (this.CausesTimeSlowdown(castTarg))
	{
		Find.TickManager.slower.SignalForceNormalSpeed();
	}
	this.surpriseAttack = surpriseAttack;
	this.canHitNonTargetPawnsNow = canHitNonTargetPawns;
	this.preventFriendlyFire = preventFriendlyFire;
	this.nonInterruptingSelfCast = nonInterruptingSelfCast;
	this.currentTarget = castTarg;
	this.currentDestination = destTarg;
	if (this.CasterIsPawn && this.WarmupTime > 0f)
	{
		ShootLine newShootLine;
		if (!this.TryFindShootLineFromTo(this.caster.Position, castTarg, out newShootLine, false))
		{
			return false;
		}
		this.CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, this.caster.Position);
		float statValue = this.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor, true, -1);
		int ticks = (this.WarmupTime * statValue).SecondsToTicks();
		this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
		if (this.verbProps.stunTargetOnCastStart && castTarg.Pawn != null)
		{
			castTarg.Pawn.stances.stunner.StunFor(ticks, null, false, true, false);
		}
	}
	else
	{
		Ability ability = this.verbTracker.directOwner as Ability;
		if (ability != null)
		{
			ability.lastCastTick = Find.TickManager.TicksGame;
		}
		this.WarmupComplete();
	}
	return true;
}
```

**핵심 포인트**:
- **492줄**: `if (this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))` - **Warmup 상태는 체크하지 않음**
- **516줄**: `new Stance_Warmup(ticks, castTarg, this)` - **매번 새로운 인스턴스 생성**
- **504줄**: `this.currentTarget = castTarg` - **Verb의 currentTarget 업데이트**

### 2. SetStance은 기존 Stance를 단순 교체

**Pawn_StanceTracker.SetStance** (`RimworldSource/Verse/Pawn_StanceTracker.cs:114-132`):

```114:132:RimworldSource/Verse/Pawn_StanceTracker.cs
public void SetStance(Stance newStance)
{
	if (this.debugLog)
	{
		Log.Message(string.Format("{0} {1} SetStance {2} -> {3}", new object[]
		{
			Find.TickManager.TicksGame,
			this.pawn,
			this.curStance,
			newStance
		}));
	}
	newStance.stanceTracker = this;
	this.curStance = newStance;
	if (this.pawn.jobs.curDriver != null)
	{
		this.pawn.jobs.curDriver.Notify_StanceChanged();
	}
}
```

**핵심 포인트**:
- **127줄**: `this.curStance = newStance` - **기존 stance를 단순 교체**
- 기존 warmup stance는 **참조가 끊겨서 GC 대상이 됨**
- 기존 stance의 `Expire()`는 호출되지 않음 (참조가 끊겨서 더 이상 Tick되지 않음)

### 3. Stance_Warmup 구조

**Stance_Warmup** (`RimworldSource/Verse/Stance_Warmup.cs:31-62`):

```31:62:RimworldSource/Verse/Stance_Warmup.cs
public Stance_Warmup(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
{
	if (focusTarg.HasThing)
	{
		Pawn pawn = focusTarg.Thing as Pawn;
		if (pawn != null)
		{
			this.targetStartedDowned = pawn.Downed;
			if (pawn.apparel != null)
			{
				Verb_CastAbility verb_CastAbility = verb as Verb_CastAbility;
				if (verb_CastAbility == null || verb_CastAbility.Ability.def.hostile)
				{
					for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
					{
						List<ThingComp> allComps = pawn.apparel.WornApparel[i].AllComps;
						for (int j = 0; j < allComps.Count; j++)
						{
							CompShield compShield = allComps[j] as CompShield;
							if (compShield != null)
							{
								compShield.KeepDisplaying();
							}
						}
					}
				}
			}
		}
	}
	this.InitEffects(false);
	this.drawAimPie = (verb != null && verb.verbProps.drawAimPie);
}
```

**Stance_Busy** (`RimworldSource/Verse/Stance_Busy.cs:33-39`):

```33:39:RimworldSource/Verse/Stance_Busy.cs
public Stance_Busy(int ticks, LocalTargetInfo focusTarg, Verb verb)
{
	this.ticksLeft = ticks;
	this.startedTick = Find.TickManager.TicksGame;
	this.focusTarg = focusTarg;
	this.verb = verb;
}
```

**핵심 포인트**:
- `focusTarg`: 생성 시점의 타겟 정보를 저장
- `verb`: Verb 참조 저장
- `ticksLeft`: 남은 틱 수 (매 틱마다 감소)

## 대상 변경 감지 메커니즘

### 현재 프로젝트의 구현

**HediffComp_RatHolicGun.CompPostTick** (`Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs:471-558`):

```471:558:Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs
public override void CompPostTick(ref float severityAdjustment)
{
	base.CompPostTick(ref severityAdjustment);

	Pawn pawn = this.Pawn;
	if (pawn == null || pawn.Dead || !pawn.Spawned)
	{
		Log.Message($"[RatHolicGun] CompPostTick: Pawn is null, dead, or not spawned. Pawn: {pawn?.LabelShort}");
		return;
	}

	// RatHolic Gun을 장착하고 있는지 확인
	ThingWithComps ratHolicGun = GetRatHolicGun(pawn);
	if (ratHolicGun == null || ratHolicGun.def != RatkinWeaponDefOf.RK_Weapon_RatHolicGun)
	{
		// 무기를 장착하지 않았으면 타겟 변경 제거 목록의 모든 Hediff 제거
		Log.Message($"[RatHolicGun] CompPostTick: No RatHolic Gun equipped. Pawn: {pawn.LabelShort}, Removing hediffs: {Props?.targetChangeRemoveHediffs?.Count ?? 0}");
		RemoveHediffs(pawn, Props?.targetChangeRemoveHediffs);
		return;
	}

	// 현재 타겟 확인 (조준 중이거나 발사 중일 때)
	LocalTargetInfo? currentAimingTarget = GetCurrentAimingTarget(pawn, ratHolicGun);

	// 재장전 중이면 타겟 변경 체크 안 함 (계속 유지)
	bool isReloading = IsPawnReloading(pawn, ratHolicGun);
	if (isReloading)
	{
		// 재장전 중이면 타겟은 유지하되 변경 체크는 하지 않음
		Log.Message($"[RatHolicGun] CompPostTick: Reloading. Pawn: {pawn.LabelShort}, CurrentTarget: {currentAimingTarget?.ToString() ?? "null"}, LastTarget: {lastAimingTarget?.ToString() ?? "null"}");
		return;
	}

	// 조준 중이 아니고 발사 중도 아니면 타겟 초기화 (다음 조준 시작 시 감지 가능하도록)
	if (!currentAimingTarget.HasValue)
	{
		if (lastAimingTarget.HasValue)
		{
			Log.Message($"[RatHolicGun] CompPostTick: No current target, clearing last target. Pawn: {pawn.LabelShort}, LastTarget: {lastAimingTarget.Value}");
		}
		lastAimingTarget = null;
		return;
	}

	// 타겟이 변경되었는지 확인
	if (lastAimingTarget.HasValue)
	{
		LocalTargetInfo lastTarget = lastAimingTarget.Value;
		LocalTargetInfo currentTarget = currentAimingTarget.Value;

		// LocalTargetInfo 비교: Thing 참조, Cell, Pawn 등을 비교
		bool targetChanged = lastTarget != currentTarget;
		bool targetInvalid = !currentTarget.IsValid;
		
		// 상세 비교 정보
		bool thingChanged = lastTarget.Thing != currentTarget.Thing;
		bool cellChanged = lastTarget.Cell != currentTarget.Cell;
		bool pawnChanged = lastTarget.Pawn != currentTarget.Pawn;

		Log.Message($"[RatHolicGun] CompPostTick: Comparing targets. Pawn: {pawn.LabelShort}, " +
			$"LastTarget: {lastTarget} (Thing: {lastTarget.Thing?.LabelShort ?? "null"}, Cell: {lastTarget.Cell}, Pawn: {lastTarget.Pawn?.LabelShort ?? "null"}), " +
			$"CurrentTarget: {currentTarget} (Thing: {currentTarget.Thing?.LabelShort ?? "null"}, Cell: {currentTarget.Cell}, Pawn: {currentTarget.Pawn?.LabelShort ?? "null"}), " +
			$"ThingChanged: {thingChanged}, CellChanged: {cellChanged}, PawnChanged: {pawnChanged}, " +
			$"TargetChanged: {targetChanged}, TargetInvalid: {targetInvalid}");

		// 타겟이 변경되었거나 삭제되었으면 Hediff 제거
		if (targetChanged || targetInvalid)
		{
			Log.Message($"[RatHolicGun] CompPostTick: Target changed or invalid! Removing hediffs. Pawn: {pawn.LabelShort}, " +
				$"Hediffs to remove: {Props?.targetChangeRemoveHediffs?.Count ?? 0}");
			RemoveHediffs(pawn, Props?.targetChangeRemoveHediffs);
			lastAimingTarget = null;
			return;
		}
		else
		{
			Log.Message($"[RatHolicGun] CompPostTick: Target unchanged. Pawn: {pawn.LabelShort}");
		}
	}
	else
	{
		Log.Message($"[RatHolicGun] CompPostTick: First target detected. Pawn: {pawn.LabelShort}, " +
			$"CurrentTarget: {currentAimingTarget.Value} (Thing: {currentAimingTarget.Value.Thing?.LabelShort ?? "null"}, Cell: {currentAimingTarget.Value.Cell})");
	}

	// 현재 타겟 저장
	lastAimingTarget = currentAimingTarget;
}
```

**GetCurrentAimingTarget** (`Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs:748-789`):

```748:789:Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs
private LocalTargetInfo? GetCurrentAimingTarget(Pawn pawn, ThingWithComps ratHolicGun)
{
	if (pawn?.stances == null)
	{
		return null;
	}

	Stance curStance = pawn.stances.curStance;

	// 1. 조준 중인지 확인 (Stance_Warmup)
	if (curStance is Stance_Warmup warmupStance)
	{
		// RatHolic Gun의 Verb인지 확인
		if (warmupStance.verb != null && warmupStance.verb.EquipmentSource == ratHolicGun)
		{
			Log.Message($"[RatHolicGun] GetCurrentAimingTarget: Warmup stance. Pawn: {pawn.LabelShort}, Target: {warmupStance.focusTarg} (Thing: {warmupStance.focusTarg.Thing?.LabelShort ?? "null"})");
			return warmupStance.focusTarg;
		}
	}

	// 2. 발사 중인지 확인 (VerbState.Bursting) - 발사 중에도 타겟 확인 가능
	CompEquippable compEquippable = ratHolicGun.GetComp<CompEquippable>();
	if (compEquippable != null)
	{
		foreach (Verb verb in compEquippable.AllVerbs)
		{
			if (verb.state == VerbState.Bursting && verb.EquipmentSource == ratHolicGun)
			{
				// 발사 중일 때는 Verb의 CurrentTarget 사용 (public property)
				LocalTargetInfo currentTarget = verb.CurrentTarget;
				if (currentTarget.IsValid)
				{
					Log.Message($"[RatHolicGun] GetCurrentAimingTarget: Bursting state. Pawn: {pawn.LabelShort}, Target: {currentTarget} (Thing: {currentTarget.Thing?.LabelShort ?? "null"})");
					return currentTarget;
				}
			}
		}
	}

	Log.Message($"[RatHolicGun] GetCurrentAimingTarget: No target found. Pawn: {pawn.LabelShort}, Stance: {curStance?.GetType().Name ?? "null"}");
	return null;
}
```

**핵심 포인트**:
- **763줄**: `warmupStance.focusTarg` - **현재 warmup stance의 focusTarg를 읽음**
- **522줄**: `lastTarget != currentTarget` - **LocalTargetInfo 비교로 타겟 변경 감지**
- 매 프레임마다 체크하여 타겟 변경을 감지

## 동작 흐름

### 시나리오: Warmup 중 다른 대상으로 조준 변경

```
1. Pawn이 Target A를 조준 중 (Stance_Warmup 생성, focusTarg = A)
   ↓
2. 다른 대상 Target B로 조준 변경 명령
   ↓
3. Verb.TryStartCastOn(B) 호출
   - state == VerbState.Bursting 체크만 함 (Warmup은 체크 안 함)
   - currentTarget = B로 업데이트
   - new Stance_Warmup(ticks, B, verb) 생성
   ↓
4. Pawn_StanceTracker.SetStance(newStance) 호출
   - curStance = newStance (기존 warmup stance는 참조 끊김)
   ↓
5. 기존 Stance_Warmup (Target A)
   - 참조가 끊겨서 더 이상 Tick되지 않음
   - Expire() 호출되지 않음
   - GC 대상이 됨
   ↓
6. 새로운 Stance_Warmup (Target B)
   - 매 틱마다 StanceTick() 호출
   - ticksLeft 감소
   - 0이 되면 Expire() → WarmupComplete() 호출
```

### 타겟 변경 감지 타이밍

**현재 구현의 감지 방식**:
- `HediffComp_RatHolicGun.CompPostTick`에서 매 프레임 체크
- `warmupStance.focusTarg`를 읽어서 이전 타겟과 비교
- 타겟이 변경되면 즉시 감지 가능

**문제점**:
- Warmup stance가 새로 생성되므로, `focusTarg`가 즉시 변경됨
- 하지만 `CompPostTick`은 매 프레임 호출되므로, 다음 프레임에 감지됨
- 즉, **1프레임 지연**이 발생할 수 있음

## 결론

### 1. 대상 변경 감지 방법

**답**: `Stance_Warmup.focusTarg`를 읽어서 이전 타겟과 비교

- `pawn.stances.curStance`가 `Stance_Warmup`인지 확인
- `warmupStance.focusTarg`로 현재 조준 타겟 확인
- 이전에 저장한 `lastAimingTarget`과 비교하여 변경 감지

### 2. Warmup Stance 생성 방식

**답**: **매번 새로운 인스턴스가 생성됨**

- `Verb.TryStartCastOn`에서 `new Stance_Warmup(ticks, castTarg, this)` 호출
- `Pawn_StanceTracker.SetStance`에서 기존 stance를 단순 교체
- 기존 warmup stance는 참조가 끊겨서 GC 대상이 됨
- **기존 warmup의 진행 상황은 무시되고, 처음부터 다시 시작**

### 3. 로직 개선 방향

**현재 문제점**:
- Warmup 중 타겟 변경 시 기존 warmup 진행 상황이 무시됨
- 매번 처음부터 warmup을 다시 시작해야 함

**개선 가능한 방향**:
1. **같은 Verb, 다른 타겟**: Warmup 시간을 일부 유지 (예: 50% 유지)
2. **다른 Verb**: 완전히 새로 시작
3. **타겟 변경 감지 최적화**: `SetStance` 호출 시점에 감지하여 즉시 처리

## 참고 코드

- `RimworldSource/Verse/Verb.cs:481-532` - TryStartCastOn
- `RimworldSource/Verse/Pawn_StanceTracker.cs:114-132` - SetStance
- `RimworldSource/Verse/Stance_Warmup.cs` - Stance_Warmup 클래스
- `RimworldSource/Verse/Stance_Busy.cs` - Stance_Busy 베이스 클래스
- `Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs:471-558` - 현재 구현


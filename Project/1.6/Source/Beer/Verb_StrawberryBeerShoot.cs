using RimWorld;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// 맥주 원거리 공격 Verb - 근접 공격 시 발동되지 않도록 보호
	/// </summary>
	public class Verb_StrawberryBeerShoot : Verb_Shoot
	{
	private bool IsInMeleeRange(LocalTargetInfo targ)
	{
		if (this.caster != null && this.caster.Spawned && targ.IsValid)
		{
			float distance = this.caster.Position.DistanceTo(targ.Cell);
			return distance <= 1.42f; // 근접 공격 범위
		}
		return false;
	}

	public override bool CanHitTarget(LocalTargetInfo targ)
	{
		// 근접 공격 범위 내에 있으면 원거리 공격 불가
		if (IsInMeleeRange(targ))
		{
			return false;
		}
		return base.CanHitTarget(targ);
	}

	public override bool IsUsableOn(Thing target)
	{
		if (target != null && this.caster != null && this.caster.Spawned)
		{
			float distance = this.caster.Position.DistanceTo(target.Position);
			if (distance <= 1.42f) // 근접 공격 범위
			{
				return false;
			}
		}
		return base.IsUsableOn(target);
	}

	protected override bool TryCastShot()
	{
		// 근접 공격 범위 내에 있으면 발동하지 않음
		if (this.currentTarget.IsValid && IsInMeleeRange(this.currentTarget))
		{
			return false; // 근접 공격 범위 내에서는 발동하지 않음
		}
		return base.TryCastShot();
	}
	}
}

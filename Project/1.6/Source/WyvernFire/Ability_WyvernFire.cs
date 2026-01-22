using RimWorld;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// Wyvern Fire 전용 Ability 클래스
	/// CompEquippableAbilityReloadable이 있고 ammoDef가 설정된 경우
	/// 쿨다운과 탄약 시스템을 완전히 분리하여 탄약 기반 재장전만 작동하도록 함
	/// </summary>
	public class Ability_WyvernFire : Ability
	{
		public Ability_WyvernFire()
		{
		}

		public Ability_WyvernFire(Pawn pawn, AbilityDef def) : base(pawn, def)
		{
		}

		/// <summary>
		/// CanCast 오버라이드
		/// 쿨다운 중에는 탄약이 있어도 무조건 발사 불가
		/// </summary>
		public override AcceptanceReport CanCast
		{
			get
			{
				// 쿨다운 중에는 무조건 발사 불가
				if (this.OnCooldown)
				{
					return false;
				}

				// 기본 CanCast 체크 (탄약 확인 등)
				return base.CanCast;
			}
		}

		/// <summary>
		/// PreActivate 오버라이드
		/// 매 발사마다 쿨다운 시작, charge 자동 회복 방지
		/// </summary>
		protected override void PreActivate(LocalTargetInfo? target)
		{
			// charge 소모 (나중에 복원할 값 저장)
			int chargeBeforeConsume = this.RemainingCharges;
			if (this.UsesCharges)
			{
				this.RemainingCharges--;
			}
			int chargeAfterConsume = this.RemainingCharges;

			// 매번 쿨다운 시작 (cooldownPerCharge 설정과 무관하게)
			// 주의: StartCooldown()은 cooldownPerCharge: False일 때 charges를 maxCharges로 복원함!
			if (this.HasCooldown)
			{
				this.StartCooldown(this.def.cooldownTicksRange.RandomInRange);
			}

			// StartCooldown()이 charge를 회복했다면, 소모된 상태로 복원
			if (this.RemainingCharges > chargeAfterConsume)
			{
				this.RemainingCharges = chargeAfterConsume;
			}

			// 기본 PreActivate의 나머지 로직 (로그 기록 등)
			Pawn pawn = this.ConstantCaster as Pawn;
			if (pawn != null)
			{
				Pawn_EquipmentTracker equipment = pawn.equipment;
				if (equipment != null)
				{
					equipment.Notify_AbilityUsed(this);
				}
			}

			if (this.def.writeCombatLog)
			{
				Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(this.pawn, (target != null) ? target.GetValueOrDefault().Thing : null, this.def, RulePackDefOf.Event_AbilityUsed));
			}
		}

		/// <summary>
		/// AbilityTick 오버라이드
		/// StartCooldown에서 charge 자동 회복 방지
		/// </summary>
		public override void AbilityTick()
		{
			bool shouldPreventAutoRecharge = ShouldPreventAutoRecharge();
			int chargeBefore = this.RemainingCharges;

			// 기본 AbilityTick 호출
			base.AbilityTick();

			// cooldownPerCharge: False일 때 StartCooldown()에서 charges = maxCharges로 회복하는 걸 방지
			if (shouldPreventAutoRecharge)
			{
				int chargeAfter = this.RemainingCharges;

				// charge가 증가했다면 원래 값으로 복원
				if (chargeAfter > chargeBefore)
				{
					this.RemainingCharges = chargeBefore;
				}
			}
		}

		/// <summary>
		/// CompEquippableAbilityReloadable이 있고 ammoDef가 설정된 경우인지 확인
		/// </summary>
		private bool ShouldPreventAutoRecharge()
		{
			// Ability의 소유자(Pawn) 확인
			if (this.pawn == null || this.pawn.equipment == null)
			{
				return false;
			}

			// 장착된 무기 확인
			ThingWithComps equipment = this.pawn.equipment.Primary;
			if (equipment == null)
			{
				return false;
			}

			// CompEquippableAbilityReloadable 확인
			CompEquippableAbilityReloadable reloadableComp = equipment.GetComp<CompEquippableAbilityReloadable>();
			if (reloadableComp == null)
			{
				return false;
			}

			// ammoDef가 설정되어 있고, 이 Ability가 reloadableComp의 Ability인지 확인
			return reloadableComp.Props.ammoDef != null && reloadableComp.AbilityForReading == this;
		}
	}
}

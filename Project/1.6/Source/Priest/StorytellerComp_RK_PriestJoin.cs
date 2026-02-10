using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	/// <summary>
	/// 랫킨 사제 합류 이벤트 StorytellerComp (1회성)
	/// 조건:
	/// 1. 게임 시작 후 fireAfterDaysPassed 일이 지나야 트리거 가능
	/// 2. 한 번만 트리거됨 (이미 발생했으면 다시 발생하지 않음)
	/// </summary>
	public class StorytellerComp_RK_PriestJoin : StorytellerComp
	{
		private const string LogPrefix = "[RK_PriestJoin]";

		private StorytellerCompProperties_RK_PriestJoin Props
		{
			get
			{
				return (StorytellerCompProperties_RK_PriestJoin)this.props;
			}
		}

		/// <summary>
		/// 현재 게임 틱을 일수로 변환
		/// </summary>
		private float DaysPassed
		{
			get
			{
				return (float)GenTicks.TicksGame / 60000f;
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			// 1. 타겟 유효성 검사 (World vs Map 등)
			if (!this.Props.incident.TargetAllowed(target))
			{
				yield break;
			}

			// 2. 이미 발생한 적이 있으면 다시 발생하지 않음 (1회성)
			int lastFireTick;
			bool hasFiredBefore = target.StoryState.lastFireTicks.TryGetValue(this.Props.incident, out lastFireTick);
			
			if (hasFiredBefore)
			{
				// 이미 발생했으므로 더 이상 트리거하지 않음
				yield break;
			}

			float currentDays = this.DaysPassed;
			float minDays = this.Props.fireAfterDaysPassed;
			
			// 3. 최소 경과 일수 체크
			if (currentDays < minDays)
			{
				if (Prefs.DevMode)
				{
					Log.Message($"{LogPrefix} 스킵: 최소 경과 일수 미충족 (현재: {currentDays:F1}일, 필요: {minDays}일)");
				}
				yield break;
			}
			
			// 4. MTB 방식으로 발생
			if (!Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f))
			{
				yield break;
			}

			// 5. CanFireNow 체크
			IncidentParms parms = this.GenerateParms(this.Props.incident.category, target);
			if (!this.Props.incident.Worker.CanFireNow(parms))
			{
				Log.Message($"{LogPrefix} 스킵: CanFireNow 실패 (incident={this.Props.incident.defName})");
				yield break;
			}

			// 모든 조건 충족 - 이벤트 발생 (1회성)
			Log.Message($"{LogPrefix} 이벤트 트리거! (경과 일수: {currentDays:F1}일) - 1회성 이벤트 완료");
			yield return new FiringIncident(this.Props.incident, this, parms);
		}

		public override string ToString()
		{
			string str = base.ToString();
			string str2 = " ";
			IncidentDef incident = this.Props.incident;
			return str + str2 + ((incident != null) ? incident.ToString() : null);
		}
	}
}

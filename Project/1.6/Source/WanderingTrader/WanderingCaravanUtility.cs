using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// 유랑단 캐러반 관련 유틸. IncidentDefExtension_WanderingCaravan의 settlerPawnKinds를 참조.
	/// </summary>
	public static class WanderingCaravanUtility
	{
		private const string IncidentDefName = "RK_Incident_WanderingTrader";

		/// <summary>해당 PawnKind가 유랑민 풀(정착 제안 대상)에 포함되는지.</summary>
		public static bool IsSettlerPoolKind(PawnKindDef kind)
		{
			if (kind == null) return false;
			var kinds = GetSettlerPawnKinds();
			return kinds != null && kinds.Contains(kind);
		}

		/// <summary>유랑민 풀에 사용할 PawnKind 목록. XML 미지정 시 Nomad, Wanderer 기본.</summary>
		public static List<PawnKindDef> GetSettlerPawnKinds()
		{
			var weights = GetSettlerPawnKindWeights();
			if (weights == null || weights.Count == 0)
				return new List<PawnKindDef> { RatkinPawnKindDefOf.RK_PawnKind_Nomad, RatkinPawnKindDefOf.RK_PawnKind_Wanderer };
			return weights.ConvertAll(w => w.kindDef);
		}

		/// <summary>유랑민 풀 가중치 목록. XML 미지정 시 null.</summary>
		private static List<PawnKindDefWeight> GetSettlerPawnKindWeights()
		{
			var incident = DefDatabase<IncidentDef>.GetNamedSilentFail(IncidentDefName);
			var ext = incident?.GetModExtension<IncidentDefExtension_WanderingCaravan>();
			return ext?.settlerPawnKinds;
		}

		/// <summary>유랑민 풀에서 가중치에 따라 랜덤 PawnKind 선택.</summary>
		public static PawnKindDef RandomSettlerKind()
		{
			var weights = GetSettlerPawnKindWeights();
			if (weights == null || weights.Count == 0)
				return RatkinPawnKindDefOf.RK_PawnKind_Nomad;
			var w = weights.RandomElementByWeight(x => x.weight);
			return w?.kindDef ?? RatkinPawnKindDefOf.RK_PawnKind_Nomad;
		}
	}
}

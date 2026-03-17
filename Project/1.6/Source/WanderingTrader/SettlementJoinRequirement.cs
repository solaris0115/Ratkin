using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace NewRatkin
{
	/// <summary>
	/// 유랑민 정착지 합류 조건의 기반 추상 클래스.
	/// 인스턴스별로 생성되어 풀에 저장되며 세이브/로드 시 영속.
	/// </summary>
	public abstract class SettlementJoinRequirement : IExposable
	{
		protected string desc;
		protected string descShort;

		public string Desc => desc ?? "";
		public string DescShort => descShort ?? "";

		public abstract bool IsMet(Map map);

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref desc, "desc", "");
			Scribe_Values.Look(ref descShort, "descShort", "");
		}

		/// <summary>랜덤 합류 조건 생성 (팩토리)</summary>
		public static SettlementJoinRequirement GenerateRandom()
		{
			int roll = Rand.RangeInclusive(0, 2);
			switch (roll)
			{
				case 0: return new SilverJoinRequirement();
				case 1: return new SkillMentorJoinRequirement();
				default: return new ItemJoinRequirement();
			}
		}
	}

	/// <summary>정착지 은 보유량 조건</summary>
	public class SilverJoinRequirement : SettlementJoinRequirement
	{
		private int requiredAmount;

		public SilverJoinRequirement()
		{
			requiredAmount = Rand.RangeInclusive(2000, 8000);
			desc = "RK_JoinReq_Silver_Desc".Translate(requiredAmount);
			descShort = "RK_JoinReq_Silver_DescShort".Translate(requiredAmount);
		}

		public override bool IsMet(Map map)
		{
			if (map == null) return false;
			ThingDef silverDef = ThingDefOf.Silver;
			if (silverDef == null) return false;
			int total = 0;
			foreach (Thing t in map.listerThings.ThingsOfDef(silverDef))
				total += t.stackCount;
			return total >= requiredAmount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requiredAmount, "requiredAmount", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(desc))
			{
				desc = "RK_JoinReq_Silver_Desc".Translate(requiredAmount);
				descShort = "RK_JoinReq_Silver_DescShort".Translate(requiredAmount);
			}
		}
	}

	/// <summary>특정 스킬 멘토 보유 조건 (Melee, Shooting 등)</summary>
	public class SkillMentorJoinRequirement : SettlementJoinRequirement
	{
		private SkillDef skillA;
		private SkillDef skillB;
		private int minLevel;

		public SkillMentorJoinRequirement()
		{
			skillA = SkillDefOf.Melee;
			skillB = SkillDefOf.Shooting;
			minLevel = Rand.RangeInclusive(6, 10);
			desc = "RK_JoinReq_SkillMentor_Desc".Translate(skillA.label, skillB.label, minLevel);
			descShort = "RK_JoinReq_SkillMentor_DescShort".Translate(skillA.label, skillB.label, minLevel);
		}

		public override bool IsMet(Map map)
		{
			if (map == null || skillA == null || skillB == null) return false;
			foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
			{
				if (p != null && !p.Dead && p.skills != null)
				{
					var skA = p.skills.GetSkill(skillA);
					var skB = p.skills.GetSkill(skillB);
					if (skA != null && skB != null && !skA.TotallyDisabled && !skB.TotallyDisabled &&
						skA.Level >= minLevel && skB.Level >= minLevel)
						return true;
				}
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref skillA, "skillA");
			Scribe_Defs.Look(ref skillB, "skillB");
			Scribe_Values.Look(ref minLevel, "minLevel", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(desc) && skillA != null && skillB != null)
			{
				desc = "RK_JoinReq_SkillMentor_Desc".Translate(skillA.label, skillB.label, minLevel);
				descShort = "RK_JoinReq_SkillMentor_DescShort".Translate(skillA.label, skillB.label, minLevel);
			}
		}
	}

	/// <summary>특정 아이템 보유량 조건</summary>
	public class ItemJoinRequirement : SettlementJoinRequirement
	{
		private ThingDef thingDef;
		private int requiredCount;

		private static readonly string[] ItemPool = new[]
		{
			"RK_Food_Hardtack",
			"MedicineHerbal",
			"ComponentIndustrial"
		};

		public ItemJoinRequirement()
		{
			string defName = ItemPool.RandomElement();
			thingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
			if (thingDef == null)
				thingDef = DefDatabase<ThingDef>.GetNamed("RK_Food_Hardtack", false);
			if (thingDef == null)
				thingDef = DefDatabase<ThingDef>.GetNamed("Pemmican", false);
			if (thingDef == null)
			{
				requiredCount = 100;
				desc = "RK_JoinReq_Item_Desc".Translate("?", requiredCount);
				descShort = "RK_JoinReq_Item_DescShort".Translate("?", requiredCount);
				return;
			}
			requiredCount = thingDef.defName == "RK_Food_Hardtack" ? Rand.RangeInclusive(500, 1500)
				: thingDef.defName == "MedicineHerbal" ? Rand.RangeInclusive(50, 200)
				: Rand.RangeInclusive(20, 80);
			desc = "RK_JoinReq_Item_Desc".Translate(thingDef.label, requiredCount);
			descShort = "RK_JoinReq_Item_DescShort".Translate(thingDef.label, requiredCount);
		}

		public override bool IsMet(Map map)
		{
			if (map == null || thingDef == null) return false;
			int total = 0;
			foreach (Thing t in map.listerThings.ThingsOfDef(thingDef))
				total += t.stackCount;
			return total >= requiredCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref requiredCount, "requiredCount", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(desc) && thingDef != null)
			{
				desc = "RK_JoinReq_Item_Desc".Translate(thingDef.label, requiredCount);
				descShort = "RK_JoinReq_Item_DescShort".Translate(thingDef.label, requiredCount);
			}
		}
	}
}

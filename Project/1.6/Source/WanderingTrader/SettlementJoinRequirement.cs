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

		/// <summary>분위기 멘트 (랜덤 택1). line1.</summary>
		public string Desc => desc ?? "";
		/// <summary>세부 필요사항. line2 또는 GetRequirementDetails 대체.</summary>
		public string DescShort => descShort ?? "";

		public abstract bool IsMet(Map map);

		/// <summary>합류 제안 UI용. DescShort 아래에 표시할 구체 수치(열거형). 비어있으면 Desc 사용.</summary>
		public virtual string GetRequirementDetails() => "";

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref desc, "desc", "");
			Scribe_Values.Look(ref descShort, "descShort", "");
			// 로드 후 번역 키가 저장돼 있으면 재번역 (Keyed 미로드/구 세이브 호환)
			if (Scribe.mode == LoadSaveMode.PostLoadInit && !string.IsNullOrEmpty(desc) && desc.StartsWith(JoinReqKeys.Prefix))
			{
				desc = desc.Translate().RawText;
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && !string.IsNullOrEmpty(descShort) && descShort.StartsWith(JoinReqKeys.Prefix))
			{
				descShort = descShort.Translate().RawText;
			}
		}

		/// <summary>PawnKindDef 전용 조건 생성. extension 있으면 conditions 택1, conditions 비어있으면 무조건 영입. extension 없으면 무조건 영입.</summary>
		public static SettlementJoinRequirement GenerateForPawnKind(PawnKindDef kind, IncidentDefExtension_WanderingCaravan ext = null)
		{
			var joinExt = kind?.GetModExtension<PawnKindDefExtension_WanderingCaravanJoin>();
			if (joinExt == null)
				return new SettlementJoinRequirementAlwaysMet();

			if (joinExt.conditions == null || joinExt.conditions.Count == 0)
				return new SettlementJoinRequirementAlwaysMet();

			int totalWeight = joinExt.conditions.Sum(c => c != null ? Math.Max(0, c.weight) : 0);
			if (totalWeight <= 0)
				return new SettlementJoinRequirementAlwaysMet();

			int roll = Rand.RangeInclusive(1, totalWeight);
			JoinConditionBase opt = null;
			foreach (var c in joinExt.conditions)
			{
				if (c == null) continue;
				int w = Math.Max(0, c.weight);
				if (roll <= w) { opt = c; break; }
				roll -= w;
			}
			if (opt == null)
				return new SettlementJoinRequirementAlwaysMet();

			return opt.CreateRequirement();
		}
	}

	/// <summary>조건 없음. 항상 영입 가능.</summary>
	public class SettlementJoinRequirementAlwaysMet : SettlementJoinRequirement
	{
		/// <summary>Scribe 역직렬화용. 매개변수 없는 생성자 필수.</summary>
		public SettlementJoinRequirementAlwaysMet() : this(null, null) { }

		public SettlementJoinRequirementAlwaysMet(string descShortOverride = null, string descOverride = null)
		{
			// desc=분위기(line1), descShort=세부(line2). Def에서 키로 넘어옴.
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate().RawText : "";
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate().RawText : "";
		}

		public override bool IsMet(Map map) => true;
	}

	/// <summary>부상 환자 수 조건 (hp 손실된 식민지원 N명 이상)</summary>
	public class InjuredPatientCountRequirement : SettlementJoinRequirement
	{
		private int requiredCount;

		public InjuredPatientCountRequirement() { }

		public InjuredPatientCountRequirement(int count, string descShortOverride = null, string descOverride = null)
		{
			requiredCount = count;
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate(count) : JoinReqKeys.Injured_DescShort.Translate(count);
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate() : JoinReqKeys.InjuredPatient_Desc1.Translate();
		}

		public override bool IsMet(Map map)
		{
			if (map == null) return false;
			int injured = 0;
			foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
			{
				if (p != null && !p.Dead && p.health?.summaryHealth != null)
				{
					if (p.health.summaryHealth.SummaryHealthPercent < 1f)
						injured++;
				}
			}
			return injured >= requiredCount;
		}

		public override string GetRequirementDetails() => JoinReqKeys.Injured_DescShort.Translate(requiredCount).RawText;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requiredCount, "requiredCount", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort))
			{
				descShort = JoinReqKeys.Injured_DescShort.Translate(requiredCount);
				desc = JoinReqKeys.InjuredPatient_Desc1.Translate();
			}
		}
	}

	/// <summary>약품 수량 조건 (허브+산업 의약품 합계 N 이상)</summary>
	public class MedicineQuantityRequirement : SettlementJoinRequirement
	{
		private int requiredCount;

		public MedicineQuantityRequirement() { }

		public MedicineQuantityRequirement(int count, string descShortOverride = null, string descOverride = null)
		{
			requiredCount = count;
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate(count) : JoinReqKeys.Medicine_DescShort.Translate(count);
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate() : JoinReqKeys.Medicine_Desc1.Translate();
		}

		public override bool IsMet(Map map)
		{
			if (map == null) return false;
			int total = 0;
			foreach (Thing t in map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine))
				total += t.stackCount;
			return total >= requiredCount;
		}

		public override string GetRequirementDetails() => JoinReqKeys.Medicine_DescShort.Translate(requiredCount).RawText;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requiredCount, "requiredCount", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort))
			{
				descShort = JoinReqKeys.Medicine_DescShort.Translate(requiredCount);
				desc = JoinReqKeys.Medicine_Desc1.Translate();
			}
		}
	}

	/// <summary>정착지 부(WealthTotal) 조건</summary>
	public class ColonyWealthJoinRequirement : SettlementJoinRequirement
	{
		private int requiredWealth;

		public ColonyWealthJoinRequirement() { }

		public ColonyWealthJoinRequirement(int wealth, string descShortOverride = null, string descOverride = null)
		{
			requiredWealth = wealth;
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate(wealth) : JoinReqKeys.ColonyWealth_DescShort.Translate(wealth);
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate() : JoinReqKeys.ColonyWealth_Desc1.Translate();
		}

		public override bool IsMet(Map map)
		{
			if (map == null || map.wealthWatcher == null) return false;
			return map.wealthWatcher.WealthTotal >= requiredWealth;
		}

		public override string GetRequirementDetails() => JoinReqKeys.Wealth_DescShort.Translate(requiredWealth).RawText;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requiredWealth, "requiredWealth", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort))
			{
				descShort = JoinReqKeys.ColonyWealth_DescShort.Translate(requiredWealth);
				desc = JoinReqKeys.ColonyWealth_Desc1.Translate();
			}
		}
	}

	/// <summary>정착지 식민지원 중 지정 백스토리 보유자가 없어야 하는 조건 (귀족 등 피하고 싶을 때)</summary>
	public class ColonistBackstoryJoinRequirement : SettlementJoinRequirement
	{
		private List<string> backstoryDefNames;

		public ColonistBackstoryJoinRequirement() { }

		public ColonistBackstoryJoinRequirement(List<string> defNames, string descShortOverride = null, string descOverride = null)
		{
			backstoryDefNames = defNames ?? new List<string>();
			string summary = BuildBackstorySummary();
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate(summary) : JoinReqKeys.Backstory_DescShort.Translate(summary);
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate() : JoinReqKeys.Backstory_Desc1.Translate();
		}

		private string BuildBackstorySummary()
		{
			if (backstoryDefNames == null || backstoryDefNames.Count == 0) return "";
			var labels = new List<string>();
			foreach (string d in backstoryDefNames)
			{
				var bd = DefDatabase<BackstoryDef>.GetNamedSilentFail(d);
				labels.Add(bd != null ? ((TaggedString)bd.TitleFor(Gender.Male)).RawText : d);
			}
			return string.Join(", ", labels);
		}

		public override bool IsMet(Map map)
		{
			if (map == null || backstoryDefNames == null || backstoryDefNames.Count == 0) return true;
			foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
			{
				if (p == null || p.Dead || p.story == null) continue;
				var child = p.story.Childhood;
				var adult = p.story.Adulthood;
				if (child != null && backstoryDefNames.Contains(child.defName)) return false;
				if (adult != null && backstoryDefNames.Contains(adult.defName)) return false;
			}
			return true;
		}

		public override string GetRequirementDetails()
		{
			if (backstoryDefNames == null || backstoryDefNames.Count == 0) return "";
			var labels = new List<string>();
			foreach (string d in backstoryDefNames)
			{
				var bd = DefDatabase<BackstoryDef>.GetNamedSilentFail(d);
				labels.Add(bd != null ? ((TaggedString)bd.TitleFor(Gender.Male)).RawText : d);
			}
			return JoinReqKeys.Backstory_DescShort.Translate(string.Join(", ", labels)).RawText;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref backstoryDefNames, "backstoryDefNames", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && backstoryDefNames == null)
				backstoryDefNames = new List<string>();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort))
			{
				descShort = JoinReqKeys.Backstory_DescShort.Translate(BuildBackstorySummary());
				desc = JoinReqKeys.Backstory_Desc1.Translate();
			}
		}
	}

	/// <summary>특정 물건/건물 보유 수량 조건. mode=OR이면 하나라도 충족, AND면 전부 충족 시.</summary>
	public class ThingQuantityJoinRequirement : SettlementJoinRequirement
	{
		private List<ThingDefCountClass> thingCounts;
		private List<ThingCategoryCountEntry> categoryCounts;
		private bool modeOr;

		public ThingQuantityJoinRequirement() { }

		public ThingQuantityJoinRequirement(List<ThingDefCountClass> counts, List<ThingCategoryCountPair> categoryPairs, string mode, string descShortOverride = null, string descOverride = null)
		{
			thingCounts = counts ?? new List<ThingDefCountClass>();
			categoryCounts = new List<ThingCategoryCountEntry>();
			foreach (var p in categoryPairs ?? new List<ThingCategoryCountPair>())
			{
				if (string.IsNullOrEmpty(p?.group)) continue;
				if (System.Enum.TryParse<ThingRequestGroup>(p.group, true, out var grp))
					categoryCounts.Add(new ThingCategoryCountEntry { group = grp, requiredCount = Rand.RangeInclusive(p.countRange.min, p.countRange.max) });
			}
			modeOr = string.IsNullOrEmpty(mode) || mode.ToUpperInvariant() == "OR";
			string summary = BuildItemSummary();
			descShort = !string.IsNullOrEmpty(descShortOverride) ? descShortOverride.Translate(summary) : JoinReqKeys.ThingQuantity_DescShort.Translate(summary);
			desc = !string.IsNullOrEmpty(descOverride) ? descOverride.Translate() : JoinReqKeys.ThingQuantity_Desc1.Translate();
		}

		private string BuildItemSummary()
		{
			var parts = new List<string>();
			foreach (var tc in thingCounts ?? new List<ThingDefCountClass>())
			{
				if (tc?.thingDef != null)
					parts.Add(tc.thingDef.label + " x" + tc.count);
			}
			foreach (var cc in categoryCounts ?? new List<ThingCategoryCountEntry>())
				parts.Add(cc.group.ToString() + " x" + cc.requiredCount);
			return string.Join(", ", parts);
		}

		public override bool IsMet(Map map)
		{
			if (map == null) return false;
			foreach (var tc in thingCounts ?? new List<ThingDefCountClass>())
			{
				if (tc?.thingDef == null) continue;
				int total = 0;
				foreach (Thing t in map.listerThings.ThingsOfDef(tc.thingDef))
					total += t.stackCount;
				bool met = total >= tc.count;
				if (modeOr && met) return true;
				if (!modeOr && !met) return false;
			}
			foreach (var cc in categoryCounts ?? new List<ThingCategoryCountEntry>())
			{
				int total = 0;
				foreach (Thing t in map.listerThings.ThingsInGroup(cc.group))
					total += t.stackCount;
				bool met = total >= cc.requiredCount;
				if (modeOr && met) return true;
				if (!modeOr && !met) return false;
			}
			return !modeOr && (thingCounts?.Count ?? 0) == 0 && (categoryCounts?.Count ?? 0) == 0 ? false : !modeOr;
		}

		public override string GetRequirementDetails()
		{
			var parts = new List<string>();
			foreach (var tc in thingCounts ?? new List<ThingDefCountClass>())
			{
				if (tc?.thingDef != null)
					parts.Add(tc.thingDef.label + " x" + tc.count);
			}
			foreach (var cc in categoryCounts ?? new List<ThingCategoryCountEntry>())
			{
				parts.Add(cc.group.ToString() + " x" + cc.requiredCount);
			}
			if (parts.Count == 0) return "";
			return JoinReqKeys.Items_DescShort.Translate(string.Join(", ", parts)).RawText;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref thingCounts, "thingCounts", LookMode.Deep);
			Scribe_Collections.Look(ref categoryCounts, "categoryCounts", LookMode.Deep);
			Scribe_Values.Look(ref modeOr, "modeOr", true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && thingCounts == null)
				thingCounts = new List<ThingDefCountClass>();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && categoryCounts == null)
				categoryCounts = new List<ThingCategoryCountEntry>();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort))
			{
				descShort = JoinReqKeys.ThingQuantity_DescShort.Translate(BuildItemSummary());
				desc = JoinReqKeys.ThingQuantity_Desc1.Translate();
			}
		}
	}

	internal class ThingCategoryCountEntry : IExposable
	{
		public ThingRequestGroup group;
		public int requiredCount;

		public void ExposeData()
		{
			Scribe_Values.Look(ref group, "group");
			Scribe_Values.Look(ref requiredCount, "requiredCount", 0);
		}
	}

	/// <summary>스킬 멘토 다중 조건. AND=한 pawn이 전부 충족, OR=아무 pawn이 하나라도 충족.</summary>
	public class SkillMentorMultiJoinRequirement : SettlementJoinRequirement
	{
		private List<SkillLevelEntry> skillEntries;
		private bool modeOr;

		public SkillMentorMultiJoinRequirement() { }

		public SkillMentorMultiJoinRequirement(List<SkillLevelRangePair> pairs, string mode, string descShortOverride = null, string descOverride = null)
		{
			skillEntries = new List<SkillLevelEntry>();
			foreach (var p in pairs ?? new List<SkillLevelRangePair>())
			{
				if (p?.skillDef == null) continue;
				skillEntries.Add(new SkillLevelEntry { skillDef = p.skillDef, requiredLevel = Rand.RangeInclusive(p.levelRange.min, p.levelRange.max) });
			}
			modeOr = !string.IsNullOrEmpty(mode) && mode.ToUpperInvariant() == "OR";
			string summary = BuildSkillSummary();
			if (!string.IsNullOrEmpty(descShortOverride))
				descShort = descShortOverride.Translate(summary);
			else
				descShort = modeOr ? JoinReqKeys.SkillMentorMulti_Or_DescShort.Translate(summary) : JoinReqKeys.SkillMentorMulti_And_DescShort.Translate(summary);
			if (!string.IsNullOrEmpty(descOverride))
				desc = descOverride.Translate();
			else
				desc = modeOr ? JoinReqKeys.SkillMentorMulti_Or_Desc1.Translate() : JoinReqKeys.SkillMentorMulti_And_Desc1.Translate();
		}

		private string BuildSkillSummary()
		{
			if (skillEntries == null || skillEntries.Count == 0) return "";
			return string.Join(", ", skillEntries
				.Where(e => e.skillDef != null)
				.Select(e => e.skillDef.label + " " + e.requiredLevel + "+"));
		}

		public override bool IsMet(Map map)
		{
			if (map == null || skillEntries == null || skillEntries.Count == 0) return false;
			if (modeOr)
			{
				foreach (var entry in skillEntries)
				{
					foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
					{
						if (p == null || p.Dead || p.skills == null) continue;
						var sk = p.skills.GetSkill(entry.skillDef);
						if (sk != null && !sk.TotallyDisabled && sk.Level >= entry.requiredLevel)
							return true;
					}
				}
				return false;
			}
			else
			{
				foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
				{
					if (p == null || p.Dead || p.skills == null) continue;
					bool allMet = true;
					foreach (var entry in skillEntries)
					{
						var sk = p.skills.GetSkill(entry.skillDef);
						if (sk == null || sk.TotallyDisabled || sk.Level < entry.requiredLevel)
						{ allMet = false; break; }
					}
					if (allMet) return true;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref skillEntries, "skillEntries", LookMode.Deep);
			Scribe_Values.Look(ref modeOr, "modeOr", false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && skillEntries == null)
				skillEntries = new List<SkillLevelEntry>();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && string.IsNullOrEmpty(descShort) && skillEntries?.Count > 0)
			{
				string summary = BuildSkillSummary();
				descShort = modeOr ? JoinReqKeys.SkillMentorMulti_Or_DescShort.Translate(summary) : JoinReqKeys.SkillMentorMulti_And_DescShort.Translate(summary);
				desc = modeOr ? JoinReqKeys.SkillMentorMulti_Or_Desc1.Translate() : JoinReqKeys.SkillMentorMulti_And_Desc1.Translate();
			}
		}

		public override string GetRequirementDetails()
		{
			if (skillEntries == null || skillEntries.Count == 0) return "";
			var parts = skillEntries.Select(e => e.skillDef != null ? e.skillDef.label + " " + e.requiredLevel + "+" : "").Where(s => !string.IsNullOrEmpty(s)).ToList();
			return JoinReqKeys.Skills_DescShort.Translate(string.Join(", ", parts)).RawText;
		}
	}

	internal class SkillLevelEntry : IExposable
	{
		public SkillDef skillDef;
		public int requiredLevel;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref skillDef, "skillDef");
			Scribe_Values.Look(ref requiredLevel, "requiredLevel", 0);
		}
	}

}

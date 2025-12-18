using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun용 Comp 속성
    /// </summary>
    public class CompProperties_RatHolicGun : CompProperties
    {
        /// <summary>
        /// Spooling 효과를 위한 HediffDef
        /// XML에서 설정 가능: &lt;hediffDef&gt;RK_Hediff_RatHolicGunSpooling&lt;/hediffDef&gt;
        /// </summary>
        public HediffDef hediffDef;

        /// <summary>
        /// 최대 중첩 수
        /// XML에서 설정 가능: &lt;maxStacks&gt;5&lt;/maxStacks&gt;
        /// </summary>
        public int maxStacks = 5;

        public CompProperties_RatHolicGun()
        {
            this.compClass = typeof(Comp_RatHolicGun);
        }
    }

    /// <summary>
    /// RatHolic Gun용 Comp - 장착/해제 시 Spooling Hediff 관리
    /// </summary>
    public class Comp_RatHolicGun : ThingComp
    {
        public CompProperties_RatHolicGun Props => 
            (CompProperties_RatHolicGun)this.props;

        /// <summary>
        /// 무기 장착 시 기존 Spooling Hediff 제거
        /// </summary>
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            RemoveAllSpoolingHediffs(pawn);
        }

        /// <summary>
        /// 무기 해제 시 Spooling Hediff 제거
        /// </summary>
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            RemoveAllSpoolingHediffs(pawn);
        }

        /// <summary>
        /// Pawn의 Spooling Hediff 제거 (하나만 존재)
        /// </summary>
        private void RemoveAllSpoolingHediffs(Pawn pawn)
        {
            if (Props?.hediffDef == null || pawn?.health?.hediffSet == null)
            {
                return;
            }

            // HediffSet에서 해당 Hediff를 찾아서 제거
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }

    /// <summary>
    /// RatHolic Gun Spooling Hediff용 Comp 속성
    /// 발사 중이 아닐 때 빠르게 Severity 감소
    /// </summary>
    public class HediffCompProperties_RatHolicGunSpooling : HediffCompProperties
    {
        public HediffCompProperties_RatHolicGunSpooling()
        {
            this.compClass = typeof(HediffComp_RatHolicGunSpooling);
        }
    }

    /// <summary>
    /// RatHolic Gun Spooling Hediff용 Comp
    /// 발사 중이 아닐 때 1초만에 Severity가 0이 되도록 빠르게 감소
    /// HediffComp_SeverityModifierBase와 동일한 패턴 사용 (CompPostTickInterval + IsHashIntervalTick)
    /// </summary>
    public class HediffComp_RatHolicGunSpooling : HediffComp
    {
        /// <summary>
        /// RatHolic Gun의 defName
        /// </summary>
        private const string RatHolicGunDefName = "RK_Weapon_RatHolicGun";

        /// <summary>
        /// Severity 업데이트 간격 (HediffComp_SeverityModifierBase와 동일)
        /// </summary>
        private const int SeverityUpdateInterval = 200;

        /// <summary>
        /// 틱 간격마다 호출 - HediffComp_SeverityModifierBase와 동일한 패턴 사용
        /// IsHashIntervalTick을 사용하여 200틱마다만 체크
        /// </summary>
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            Pawn pawn = this.Pawn;
            if (pawn == null || pawn.Dead || !pawn.Spawned)
            {
                return;
            }

            // HediffComp_SeverityModifierBase와 동일하게 IsHashIntervalTick 사용
            if (!pawn.IsHashIntervalTick(SeverityUpdateInterval, delta))
            {
                return;
            }

            // RatHolic Gun을 장착하고 있는지 확인
            ThingWithComps ratHolicGun = GetRatHolicGun(pawn);
            if (ratHolicGun == null)
            {
                // 무기를 장착하지 않았으면 즉시 제거
                severityAdjustment = -this.parent.Severity;
                return;
            }

            // 발사 중인지 확인 (조준, 발사, 재장전)
            bool isFiring = IsPawnFiring(pawn, ratHolicGun);

            if (!isFiring)
            {
                // 발사 중이 아니면 빠르게 Severity 감소 (1초만에 0이 되도록)
                // 200틱 동안의 감소량을 한 번에 적용
                // 1초(60틱)에 maxSeverity만큼 감소하므로, 200틱 동안은 maxSeverity * (200/60)만큼 감소
                float maxSeverity = this.parent.def.maxSeverity;
                float severityPerCheck = maxSeverity * (SeverityUpdateInterval / 60f);
                severityAdjustment = -severityPerCheck;
            }
            // 발사 중이면 severityAdjustment = 0 (감소하지 않음)
        }

        /// <summary>
        /// Pawn이 RatHolic Gun을 장착하고 있는지 확인하고 무기 반환
        /// </summary>
        private ThingWithComps GetRatHolicGun(Pawn pawn)
        {
            if (pawn?.equipment == null)
            {
                return null;
            }

            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                if (equipment.def.defName == RatHolicGunDefName)
                {
                    return equipment;
                }
            }

            return null;
        }

        /// <summary>
        /// Pawn이 현재 발사 중인지 확인
        /// 1. 조준 중 (Stance_Warmup)
        /// 2. 발사 중 (VerbState.Bursting)
        /// 3. 재장전 중 (Stance_Cooldown)
        /// </summary>
        private bool IsPawnFiring(Pawn pawn, ThingWithComps ratHolicGun)
        {
            if (pawn?.stances == null)
            {
                return false;
            }

            Stance curStance = pawn.stances.curStance;

            // 1. 조준 중인지 확인
            if (curStance is Stance_Warmup warmupStance)
            {
                // RatHolic Gun의 Verb인지 확인
                if (warmupStance.verb != null && warmupStance.verb.EquipmentSource == ratHolicGun)
                {
                    return true;
                }
            }

            // 2. 재장전 중인지 확인 (Stance_Cooldown)
            if (curStance is Stance_Cooldown cooldownStance)
            {
                // RatHolic Gun의 Verb인지 확인
                if (cooldownStance.verb != null && cooldownStance.verb.EquipmentSource == ratHolicGun)
                {
                    return true;
                }
            }

            // 3. 발사 중인지 확인 (VerbState.Bursting)
            CompEquippable compEquippable = ratHolicGun.GetComp<CompEquippable>();
            if (compEquippable != null)
            {
                foreach (Verb verb in compEquippable.AllVerbs)
                {
                    if (verb.state == VerbState.Bursting)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}


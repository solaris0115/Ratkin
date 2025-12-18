using RimWorld;
using System;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun용 커스텀 Verb - 매 발사마다 재장전 속도 감소 Hediff 부여 (burstShotCount로 나눠서 증가)
    /// </summary>
    public class Verb_RatHolicGun : Verb_Shoot
    {
        /// <summary>
        /// Comp에서 HediffDef와 MaxStacks 가져오기
        /// </summary>
        private Comp_RatHolicGun GetRatHolicGunComp()
        {
            return base.EquipmentSource?.GetComp<Comp_RatHolicGun>();
        }

        protected override bool TryCastShot()
        {
            // 기본 발사 로직 실행
            bool shotSuccess = base.TryCastShot();

            // 매 발사마다 Hediff 증가 (burstShotCount로 나눠서 증가)
            if (shotSuccess && this.CasterIsPawn && this.CasterPawn != null)
            {
                AddSpoolingHediff(this.CasterPawn);
            }

            return shotSuccess;
        }

        /// <summary>
        /// Spooling Hediff 추가 또는 Severity 증가
        /// 매 발사마다 1스택을 burstShotCount로 나눈 만큼 증가
        /// 예: 20발 버스트면 각 발사마다 0.2/20 = 0.01 Severity 증가
        /// Severity: 0.2 = 1스택, 0.4 = 2스택, ..., 1.2 = 6스택
        /// </summary>
        private void AddSpoolingHediff(Pawn pawn)
        {
            Comp_RatHolicGun comp = GetRatHolicGunComp();
            if (comp?.Props?.hediffDef == null || pawn?.health == null)
            {
                return;
            }

            HediffDef hediffDef = comp.Props.hediffDef;
            int maxStacks = comp.Props.maxStacks;
            float maxSeverity = hediffDef.maxSeverity;
            int burstShotCount = this.BurstShotCount;

            // 한 스택의 Severity 값
            float severityPerStack = maxSeverity / maxStacks;
            // 매 발사마다 증가할 Severity (burstShotCount로 나눔)
            float severityPerShot = severityPerStack / burstShotCount;

            // 기존 Hediff 확인
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (hediff == null)
            {
                // Hediff가 없으면 새로 추가
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(hediff);
                hediff.Severity = severityPerShot; // 첫 발사분
            }
            else
            {
                // Hediff가 있으면 Severity 증가 (매 발사마다 증가)
                // 최대 Severity까지 증가 가능
                if (hediff.Severity < maxSeverity)
                {
                    hediff.Severity = System.Math.Min(maxSeverity, hediff.Severity + severityPerShot);
                }
            }
        }
    }
}


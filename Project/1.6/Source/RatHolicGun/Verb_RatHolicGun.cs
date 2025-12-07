using RimWorld;
using System;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun용 커스텀 Verb - 매 발사마다 재장전 속도 감소 Hediff 부여
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

            // 발사 성공 시 Hediff 추가
            if (shotSuccess && this.CasterIsPawn && this.CasterPawn != null)
            {
                AddSpoolingHediff(this.CasterPawn);
            }

            return shotSuccess;
        }

        /// <summary>
        /// Spooling Hediff 추가 또는 Severity 증가
        /// Severity: 0.2 = 1스택, 0.4 = 2스택, ..., 1.0 = maxStacks
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

            // 기존 Hediff 확인
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (hediff == null)
            {
                // Hediff가 없으면 새로 추가 (첫 스택)
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(hediff);
                hediff.Severity = 1f / maxStacks; // 첫 스택
            }
            else
            {
                // Hediff가 있으면 Severity 증가 (스택 증가)
                // Severity가 1.0 미만이면 증가, 최대 1.0 (maxStacks)
                if (hediff.Severity < 1f)
                {
                    hediff.Severity = System.Math.Min(1f, hediff.Severity + (1f / maxStacks));
                }
            }
        }
    }
}


using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NewRatkin
{
    public class JobDriver_ShieldFaceDirection : JobDriver
    {
        public override string GetReport()
        {
            return "RK_ShieldFaceDirectionReport".Translate();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
                pawn.pather?.StopDead();
            };
            toil.tickAction = delegate
            {
                if (!pawn.Drafted)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }
                if (job.targetA.IsValid)
                {
                    if (job.targetA.HasThing && job.targetA.Thing.Spawned)
                    {
                        pawn.rotationTracker.FaceTarget(job.targetA);
                    }
                    else if (job.targetA.Cell.IsValid)
                    {
                        pawn.rotationTracker.FaceTarget(job.targetA);
                    }
                }
            };
            toil.handlingFacing = true;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}

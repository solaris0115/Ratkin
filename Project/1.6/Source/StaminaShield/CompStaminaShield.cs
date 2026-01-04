using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace NewRatkin
{
    [StaticConstructorOnStartup]
    public class CompStaminaShield : ThingComp
    {
        protected float stamina;

        protected int ticksToReset = -1;

        protected int lastKeepDisplayTick = -9999;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private int KeepDisplayingTicks = 1000;

        private float ApparelScorePerStaminaMax = 0.25f;

        public CompProperties_StaminaShield Props
        {
            get
            {
                return (CompProperties_StaminaShield)this.props;
            }
        }

        public float StaminaMax
        {
            get
            {
                float value = this.parent.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldStamina, true, -1);
                
                // 디버그: 상세 정보 로그 출력
                if (Find.TickManager != null && Find.TickManager.TicksGame % 300 == 0) // 5초마다
                {
                    float baseValue = this.parent?.def?.statBases?.FirstOrDefault(s => s.stat == RatkinStatDefOf.RK_Stat_ShieldStamina)?.value ?? 0f;
                    QualityCategory? quality = null;
                    if (this.parent.TryGetQuality(out QualityCategory qc))
                    {
                        quality = qc;
                    }
                    string stuffName = this.parent.Stuff?.defName ?? "null";
                    
                    Log.Message($"[CompStaminaShield] StaminaMax Debug - Parent: {this.parent?.def?.defName ?? "null"}, " +
                        $"BaseValue: {baseValue:F2}, Quality: {quality?.ToString() ?? "null"}, " +
                        $"Stuff: {stuffName}, FinalValue: {value:F2}");
                }
                
                // 디버그: 값이 비정상적으로 크면 로그 출력
                if (value > 100f)
                {
                    float baseValue = this.parent?.def?.statBases?.FirstOrDefault(s => s.stat == RatkinStatDefOf.RK_Stat_ShieldStamina)?.value ?? 0f;
                    QualityCategory? quality = null;
                    if (this.parent.TryGetQuality(out QualityCategory qc))
                    {
                        quality = qc;
                    }
                    string stuffName = this.parent.Stuff?.defName ?? "null";
                    
                    Log.Warning($"[CompStaminaShield] StaminaMax is abnormally high: {value:F2} for {this.parent?.def?.defName ?? "null"}. " +
                        $"BaseValue: {baseValue:F2}, Quality: {quality?.ToString() ?? "null"}, Stuff: {stuffName}");
                }
                return value;
            }
        }

        private float StaminaGainPerTick
        {
            get
            {
                float gainPerSecond = this.parent.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldStaminaRechargeRate, true, -1);
                return gainPerSecond / 60f;
            }
        }

        public float Stamina
        {
            get
            {
                return this.stamina;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null && (pawn.IsCharging() || pawn.IsSelfShutdown()))
                {
                    return ShieldState.Disabled;
                }
                CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake)
                {
                    return ShieldState.Disabled;
                }
                // 스태미나 고갈인 경우만 Resetting 상태
                if (this.stamina <= 0f && this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                // ticksToReset이 0 이하이면 Active 상태 (초기 장착 시 -1, 재충전 완료 시 -1)
                if (this.ticksToReset <= 0)
                {
                    return ShieldState.Active;
                }
                return ShieldState.Resetting;
            }
        }

        protected bool ShouldDisplay
        {
            get
            {
                Pawn pawnOwner = this.PawnOwner;
                return pawnOwner.Spawned && !pawnOwner.Dead && !pawnOwner.Downed && (pawnOwner.InAggroMentalState || pawnOwner.Drafted || (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks || (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == pawnOwner));
            }
        }

        protected Pawn PawnOwner
        {
            get
            {
                Apparel apparel = this.parent as Apparel;
                if (apparel != null)
                {
                    return apparel.Wearer;
                }
                Pawn pawn = this.parent as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
                return null;
            }
        }

        public bool IsApparel
        {
            get
            {
                return this.parent is Apparel;
            }
        }

        private bool IsBuiltIn
        {
            get
            {
                return !this.IsApparel;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.stamina, "stamina", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            // 방패 장착 시 스태미나를 0으로 초기화
            this.stamina = 0f;
            this.ticksToReset = -1;
            Log.Message($"[CompStaminaShield] Notify_Equipped: Pawn={pawn?.LabelShort ?? "null"}, Parent={this.parent?.def?.defName ?? "null"}, Stamina={this.stamina:F2}, ticksToReset={this.ticksToReset}, StaminaMax={this.StaminaMax:F2}, StaminaGainPerTick={this.StaminaGainPerTick:F4}");
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }
            if (this.IsApparel)
            {
                foreach (Gizmo gizmo2 in this.GetGizmos())
                {
                    yield return gizmo2;
                }
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Break",
                    action = new Action(this.Break)
                };
                if (this.ticksToReset > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Clear reset",
                        action = delegate()
                        {
                            this.ticksToReset = 0;
                        }
                    };
                }
            }
            yield break;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (this.IsBuiltIn)
            {
                foreach (Gizmo gizmo2 in this.GetGizmos())
                {
                    yield return gizmo2;
                }
            }
            yield break;
        }

        private IEnumerable<Gizmo> GetGizmos()
        {
            if (this.PawnOwner.Faction != Faction.OfPlayer)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn == null || !pawn.RaceProps.IsMechanoid)
                {
                    goto IL_82;
                }
            }
            if (Find.Selector.SingleSelectedThing == this.PawnOwner)
            {
                yield return new Gizmo_StaminaShieldStatus
                {
                    shield = this
                };
            }
            IL_82:
            yield break;
        }

        public override float CompGetSpecialApparelScoreOffset()
        {
            return this.StaminaMax * this.ApparelScorePerStaminaMax;
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // 디버그 로그 (60틱마다 출력, 1초마다)
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                Log.Message($"[CompStaminaShield] CompTick called. Parent: {this.parent?.def?.defName ?? "null"}, PawnOwner: {this.PawnOwner?.LabelShort ?? "null"}, Stamina: {this.stamina:F2}, StaminaMax: {this.StaminaMax:F2}, ticksToReset: {this.ticksToReset}, ShieldState: {this.ShieldState}, StaminaGainPerTick: {this.StaminaGainPerTick:F4}");
            }
            
            if (this.PawnOwner == null)
            {
                this.stamina = 0f;
                return;
            }
            
            ShieldState currentState = this.ShieldState;
            if (currentState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                    return;
                }
            }
            else if (currentState == ShieldState.Active)
            {
                float oldStamina = this.stamina;
                this.stamina += this.StaminaGainPerTick;
                if (this.stamina > this.StaminaMax)
                {
                    this.stamina = this.StaminaMax;
                }
                
                // 스테미나 회복 로그 (변화가 있을 때만)
                if (Find.TickManager.TicksGame % 60 == 0 && oldStamina != this.stamina)
                {
                    Log.Message($"[CompStaminaShield] Stamina recovering: {oldStamina:F2} -> {this.stamina:F2} (Max: {this.StaminaMax:F2})");
                }
            }
            else if (currentState == ShieldState.Disabled)
            {
                // Disabled 상태일 때 로그
                if (Find.TickManager.TicksGame % 60 == 0)
                {
                    Log.Message($"[CompStaminaShield] Shield is Disabled. Stamina: {this.stamina:F2}");
                }
            }
        }


        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
            {
                return;
            }
            
            // EMP 공격은 통과 (스태미나 감소 없음)
            if (dinfo.Def == DamageDefOf.EMP)
            {
                return;
            }
            
            // ignoreShields 속성은 통과 (스태미나 감소 없음)
            if (dinfo.Def.ignoreShields)
            {
                return;
            }
            
            // 근접/원거리/폭발 공격 처리
            float damageReductionPercent = 0f;
            float staminaLossPerDamage = 0f;
            bool shouldProcess = false;
            
            if (dinfo.Def.isRanged)
            {
                damageReductionPercent = this.Props.damageReductionPercentRanged;
                staminaLossPerDamage = this.Props.staminaLossPerDamageRanged;
                shouldProcess = true;
            }
            else if (dinfo.Def.isExplosive)
            {
                damageReductionPercent = this.Props.damageReductionPercentExplosive;
                staminaLossPerDamage = this.Props.staminaLossPerDamageExplosive;
                shouldProcess = true;
            }
            else
            {
                // 근접 공격 (isRanged = false, isExplosive = false)
                damageReductionPercent = this.Props.damageReductionPercentMelee;
                staminaLossPerDamage = this.Props.staminaLossPerDamageMelee;
                shouldProcess = true;
            }
            
            if (shouldProcess && damageReductionPercent > 0f)
            {
                // 원래 데미지를 기준으로 스태미나 소모 (감쇄 전)
                float originalDamage = dinfo.Amount;
                float staminaLoss = originalDamage * staminaLossPerDamage;
                this.stamina -= staminaLoss;
                
                // 그 다음 피해 감소율만큼 데미지 감쇄
                float reducedDamage = originalDamage * damageReductionPercent;
                float remainingDamage = originalDamage - reducedDamage;
                
                // 스태미나가 0 이하가 되면 쉴드 파괴
                if (this.stamina <= 0f)
                {
                    this.Break();
                }
                else
                {
                    // 흡수 효과 표시
                    this.AbsorbedDamage(dinfo);
                }
                
                // 감쇄된 데미지만 차단, 나머지는 통과
                if (remainingDamage > 0f)
                {
                    dinfo.SetAmount(remainingDamage);
                }
                else
                {
                    absorbed = true;
                }
            }
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            // 금속성 방패 충돌 사운드 (고대 메카노이드 잔해 등 건물 충돌 사운드 사용)
            SoundDef bulletImpactMetal = DefDatabase<SoundDef>.GetNamed("BulletImpact_Metal", false);
            if (bulletImpactMetal != null)
            {
                bulletImpactMetal.PlayOneShot(new TargetInfo(this.PawnOwner.Position, this.PawnOwner.Map, false));
            }
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.PawnOwner.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, this.PawnOwner.Map, FleckDefOf.ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                FleckMaker.ThrowDustPuff(loc, this.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        private void Break()
        {
            if (this.parent.Spawned)
            {
                EffecterDefOf.Shield_Break.SpawnAttached(this.parent, this.parent.MapHeld, 1.0f);
                FleckMaker.Static(this.PawnOwner.TrueCenter(), this.PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(this.PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), this.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
                }
                // 사운드 제거 (계획서 요구사항)
            }
            this.stamina = 0f;
            this.ticksToReset = this.Props.startingTicksToReset;
        }

        private void Reset()
        {
            // 재충전 사운드 및 번개 글로우 제거 (계획서 요구사항)
            this.ticksToReset = -1;
            this.stamina = this.Props.staminaOnReset;
        }

        // 쉴드 버블 렌더링 제거 (계획서 요구사항)
        // public override void CompDrawWornExtras() 및 PostDraw() 제거됨

        // 원거리 무기 차단 기능 제거 (계획서 요구사항)
        // CompAllowVerbCast 제거됨
    }
}


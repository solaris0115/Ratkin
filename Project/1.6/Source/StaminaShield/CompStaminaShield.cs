using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace NewRatkin
{
    public enum AttackType
    {
        Melee,      // 근접 공격
        Ranged,     // 원거리 공격
        Explosive,  // 폭발 공격
        Etc         // 그 외 (통과)
    }

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

        private bool isApplyingDurabilityDamage = false;

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
                return this.parent.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldStamina, true, -1);
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
                    action = delegate()
                    {
                        this.Break(0f); // DEV 명령어는 기본 스턴 시간만 적용
                    }
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
                this.stamina += this.StaminaGainPerTick;
                if (this.stamina > this.StaminaMax)
                {
                    this.stamina = this.StaminaMax;
                }
            }
        }


        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            
            float incomingDamage = dinfo.Amount;
            string damageType = dinfo.Def.defName;
            
            // 내구도 데미지 적용 중이면 무시 (무한 루프 방지)
            if (this.isApplyingDurabilityDamage)
            {
                return;
            }
            
            if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
            {
                return;
            }
            
            // 소집 상태이고 행동 가능한 상태에서만 흡수 동작
            Pawn pawn = this.PawnOwner;
            if (!pawn.Drafted || pawn.Dead || pawn.Downed || !pawn.Awake())
            {
                return;
            }
            
            // 근접/원거리/폭발 공격 처리
            AttackType attackType = this.GetAttackType(dinfo);
            float damageReductionPercent = 0f;
            float staminaLossPerDamage = 0f;
            bool shouldProcess = false;
            string classificationType = "미분류";
            
            switch (attackType)
            {
                case AttackType.Melee:
                    classificationType = "근접";
                    damageReductionPercent = this.Props.damageReductionPercentMelee;
                    staminaLossPerDamage = this.Props.staminaLossPerDamageMelee;
                    shouldProcess = true;
                    break;
                    
                case AttackType.Ranged:
                    classificationType = "원거리";
                    damageReductionPercent = this.Props.damageReductionPercentRanged;
                    staminaLossPerDamage = this.Props.staminaLossPerDamageRanged;
                    shouldProcess = true;
                    break;
                    
                case AttackType.Explosive:
                    classificationType = "폭발";
                    damageReductionPercent = this.Props.damageReductionPercentExplosive;
                    staminaLossPerDamage = this.Props.staminaLossPerDamageExplosive;
                    shouldProcess = true;
                    break;
                    
                case AttackType.Etc:
                default:
                    classificationType = "ETC (통과)";
                    shouldProcess = false;  // ETC는 통과 처리
                    break;
            }
            
            if (!shouldProcess)
            {
                return;
            }
            
            if (damageReductionPercent <= 0f)
            {
                return;
            }
            
            if (shouldProcess && damageReductionPercent > 0f)
            {
                // 원래 데미지를 기준으로 스태미나 소모 (감쇄 전)
                float originalDamage = dinfo.Amount;
                
                float staminaLoss = originalDamage * staminaLossPerDamage;
                float currentStaminaBefore = this.stamina;
                
                // 스태미나가 충분한지 확인
                bool hasEnoughStamina = this.stamina >= staminaLoss;
                
                // 스태미나 차감
                this.stamina -= staminaLoss;
                
                float reducedDamage = 0f;
                float remainingDamage = originalDamage;
                
                // 스태미나가 충분한 경우에만 피해 감소 적용
                if (hasEnoughStamina)
                {
                    // 피해 감소율만큼 데미지 감쇄
                    reducedDamage = originalDamage * damageReductionPercent;
                    remainingDamage = originalDamage - reducedDamage;
                    
                    // 감쇄된 데미지만 차단, 나머지는 통과
                    if (remainingDamage > 0f)
                    {
                        dinfo.SetAmount(remainingDamage);
                    }
                }
                else
                {
                    // 스태미나 부족 시 피해 감소 적용 안 함 (원래 데미지 그대로)
                    reducedDamage = 0f;
                    remainingDamage = originalDamage;
                    // dinfo는 그대로 유지 (원래 데미지)
                }
                
                // 스태미나가 0 이하가 되면 쉴드 파괴
                if (this.stamina <= 0f)
                {
                    // 흡수 못한 피해 비율 계산 (스턴 시간 증가용)
                    float remainingStaminaRatio = 0f;
                    if (staminaLoss > 0f)
                    {
                        // 잔여 스태미나 비율 = 브레이크 전 스태미나 / 필요한 스태미나 손실량
                        // 흡수 못한 비율 = 1 - 잔여 스태미나 비율
                        remainingStaminaRatio = Mathf.Clamp01(currentStaminaBefore / staminaLoss);
                    }
                    this.Break(remainingStaminaRatio);
                }
                else if (hasEnoughStamina)
                {
                    // 흡수 효과 표시 (스태미나가 충분했을 때만)
                    this.AbsorbedDamage(dinfo);
                }
                
                // 의류 내구도 손상 처리 (흡수된 데미지량 기준, 스태미나가 충분했을 때만)
                if (hasEnoughStamina && this.Props.durabilityDamagePercent > 0f && this.IsApparel && this.parent.Spawned)
                {
                    // 실제로 방패가 흡수한 데미지량(reducedDamage)을 기준으로 내구도 손상 계산
                    float durabilityDamage = reducedDamage * this.Props.durabilityDamagePercent;
                    if (durabilityDamage > 0f)
                    {
                        this.isApplyingDurabilityDamage = true;
                        try
                        {
                            DamageInfo durabilityDinfo = new DamageInfo(dinfo.Def, durabilityDamage, dinfo.ArmorPenetrationInt, dinfo.Angle, dinfo.Instigator, null, dinfo.Weapon, dinfo.Category, dinfo.IntendedTarget);
                            this.parent.TakeDamage(durabilityDinfo);
                        }
                        finally
                        {
                            this.isApplyingDurabilityDamage = false;
                        }
                    }
                }
                
                // 피격 처리 후 absorbed = true 설정
                absorbed = true;
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

        private void Break(float remainingStaminaRatio = 0f)
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
            
            // 스태미나 브레이크 시 stun 부여 및 재생 대기 시간 설정
            int totalStunTicks = this.Props.startingTicksToReset; // 기본값 (스턴이 부여되지 않는 경우)
            
            if (this.Props.stunDurationTicks > 0 && this.PawnOwner != null && this.PawnOwner.stances != null && this.PawnOwner.stances.stunner != null)
            {
                // 흡수 못한 피해 비율에 따라 스턴 시간 증가 (최대 100%)
                // remainingStaminaRatio: 잔여 스태미나 비율 (0.0 ~ 1.0)
                // 흡수 못한 비율 = 1.0 - remainingStaminaRatio
                // 예: 스태미나 피해량 50, 잔여 스태미나 25 → remainingStaminaRatio = 0.5 → 흡수 못한 비율 = 0.5 → 스턴 시간 50% 증가
                float unabsorbedRatio = 1.0f - remainingStaminaRatio;
                unabsorbedRatio = Mathf.Clamp01(unabsorbedRatio); // 최대 100%로 제한
                
                int baseStunTicks = this.Props.stunDurationTicks;
                int additionalStunTicks = Mathf.RoundToInt(baseStunTicks * unabsorbedRatio);
                totalStunTicks = baseStunTicks + additionalStunTicks;
                
                this.PawnOwner.stances.stunner.StunFor(totalStunTicks, this.parent, true, true, false);
            }
            
            // 스턴 시간과 재생 대기 시간을 동일하게 설정
            this.ticksToReset = totalStunTicks;
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

        /// <summary>
        /// DamageInfo를 통해 공격 타입을 구분합니다.
        /// </summary>
        private AttackType GetAttackType(DamageInfo dinfo)
        {
            // 1순위: 폭발 공격 확인 (최우선, 근접/원거리 무시)
            if (dinfo.Def.isExplosive)
            {
                return AttackType.Explosive;
            }
            
            // 2순위: ignoreShields 또는 EMP는 ETC로 처리 (통과)
            if (dinfo.Def.ignoreShields || dinfo.Def == DamageDefOf.EMP)
            {
                return AttackType.Etc;
            }
            
            // 3순위: Tool 확인 (근접 무기는 항상 Tool을 가짐)
            if (dinfo.Tool != null)
            {
                return AttackType.Melee;
            }
            
            // 4순위: Weapon의 Verbs 확인
            if (dinfo.Weapon != null && dinfo.Weapon.Verbs != null)
            {
                bool hasMeleeVerb = false;
                bool hasRangedVerb = false;
                
                foreach (VerbProperties verbProps in dinfo.Weapon.Verbs)
                {
                    if (verbProps.IsMeleeAttack)
                    {
                        hasMeleeVerb = true;
                    }
                    if (verbProps.Ranged)
                    {
                        hasRangedVerb = true;
                    }
                }
                
                // 근접과 원거리 모두 있으면 근접 우선 (근접 무기로 원거리 공격 불가)
                if (hasMeleeVerb)
                {
                    return AttackType.Melee;
                }
                if (hasRangedVerb)
                {
                    return AttackType.Ranged;
                }
            }
            
            // 5순위: Instigator의 현재 Verb 확인
            if (dinfo.Instigator is Pawn attacker)
            {
                // 현재 작업 중인 Verb
                Verb currentVerb = attacker.CurJob?.verbToUse;
                if (currentVerb != null)
                {
                    if (currentVerb.verbProps.IsMeleeAttack)
                    {
                        return AttackType.Melee;
                    }
                    if (currentVerb.verbProps.Ranged)
                    {
                        return AttackType.Ranged;
                    }
                }
                
                // 장비 중인 무기의 PrimaryVerb
                Verb primaryVerb = attacker.equipment?.PrimaryEq?.PrimaryVerb;
                if (primaryVerb != null)
                {
                    if (primaryVerb.verbProps.IsMeleeAttack)
                    {
                        return AttackType.Melee;
                    }
                    if (primaryVerb.verbProps.Ranged)
                    {
                        return AttackType.Ranged;
                    }
                }
                
                // CurrentEffectiveVerb 확인 (폴백)
                Verb effectiveVerb = attacker.CurrentEffectiveVerb;
                if (effectiveVerb != null)
                {
                    if (effectiveVerb.IsMeleeAttack)
                    {
                        return AttackType.Melee;
                    }
                    if (effectiveVerb.verbProps.Ranged)
                    {
                        return AttackType.Ranged;
                    }
                }
            }
            
            // 6순위: DamageDef의 isRanged 속성 확인 (폴백)
            if (dinfo.Def.isRanged)
            {
                return AttackType.Ranged;
            }
            
            // 7순위: 그 외 (ETC - 통과)
            return AttackType.Etc;
        }
    }
}


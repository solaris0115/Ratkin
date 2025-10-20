using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// 배너 Apparel의 조건부 드로잉을 담당하는 Component
    /// RimWorld CompShield 패턴을 따름
    /// </summary>
    public class CompProperties_BannerDrawer : CompProperties
    {
        /// <summary>
        /// 배너 텍스처 경로 (방향별 텍스처용 Graphic_Multi)
        /// 예: "Apparel/Util/RK_TextureApparel_BannerArm"
        /// </summary>
        public string graphicPath;

        /// <summary>
        /// 드로잉 크기 (기본값: 1.0)
        /// </summary>
        public Vector2 drawSize = new Vector2(1f, 1f);

        public CompProperties_BannerDrawer()
        {
            this.compClass = typeof(CompBannerDrawer);
        }
    }

    [StaticConstructorOnStartup]
    public class CompBannerDrawer : ThingComp
    {
        private Graphic bannerGraphic;

        // 위치 오프셋 정의 (소집 시 - 팔/어깨)
        private static readonly Vector3 draftedOffsetNorth = new Vector3(-0.25f, 0.15f, -0.08f);
        private static readonly Vector3 draftedOffsetSouth = new Vector3(0.25f, 0.15f, -0.08f);
        private static readonly Vector3 draftedOffsetEast = new Vector3(0.22f, 0.12f, -0.12f);
        private static readonly Vector3 draftedOffsetWest = new Vector3(-0.22f, 0.12f, -0.12f);

        // 위치 오프셋 정의 (평상시 - 등)
        private static readonly Vector3 backOffsetNorth = new Vector3(0f, -0.18f, -0.08f);
        private static readonly Vector3 backOffsetSouth = new Vector3(0f, -0.12f, -0.12f);
        private static readonly Vector3 backOffsetEast = new Vector3(-0.12f, -0.15f, -0.08f);
        private static readonly Vector3 backOffsetWest = new Vector3(0.12f, -0.15f, -0.08f);

        /// <summary>
        /// CompProperties 캐스팅
        /// </summary>
        public CompProperties_BannerDrawer Props => (CompProperties_BannerDrawer)this.props;

        /// <summary>
        /// 부모 Thing을 Apparel로 캐스팅
        /// </summary>
        private Apparel Apparel => this.parent as Apparel;

        /// <summary>
        /// Apparel을 착용한 Pawn
        /// </summary>
        private Pawn Wearer => Apparel?.Wearer;

        /// <summary>
        /// 팔/어깨에 배너를 표시해야 하는지 여부 (소집 상태)
        /// CompShield의 ShouldDisplay 패턴을 따름
        /// </summary>
        private bool ShouldShowOnArm
        {
            get
            {
                Pawn wearer = Wearer;
                if (wearer == null || !wearer.Spawned || wearer.Dead || wearer.Downed)
                {
                    return false;
                }

                // CompShield와 동일한 조건
                return wearer.Drafted ||
                       wearer.InAggroMentalState ||
                       (wearer.CurJob != null && wearer.CurJob.def.alwaysShowWeapon) ||
                       (wearer.mindState.duty != null && wearer.mindState.duty.def.alwaysShowWeapon);
            }
        }

        /// <summary>
        /// Comp가 생성될 때 Graphic 로딩
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            LoadGraphic();
        }

        /// <summary>
        /// Graphic 로딩 (비동기 처리)
        /// </summary>
        private void LoadGraphic()
        {
            if (bannerGraphic != null) return;

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                if (parent == null) return;

                // CompProperties에서 graphicPath 가져오기
                string graphicPath = Props.graphicPath;
                
                // graphicPath가 설정되지 않은 경우 기본값 사용
                if (graphicPath.NullOrEmpty())
                {
                    Log.Warning($"[CompBannerDrawer] {parent.def.defName}: graphicPath가 설정되지 않았습니다. 기본 경로 사용.");
                    graphicPath = "Apparel/Util/RK_TextureApparel_BannerArm";
                }

                // drawSize 가져오기 (Props 또는 parent.def.graphicData에서)
                Vector2 drawSize = Props.drawSize;
                if (drawSize == Vector2.zero && parent.def.graphicData != null)
                {
                    drawSize = parent.def.graphicData.drawSize;
                }
                if (drawSize == Vector2.zero)
                {
                    drawSize = new Vector2(1f, 1f);
                }
                
                bannerGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    graphicPath,
                    ShaderDatabase.Cutout,
                    drawSize,
                    parent.DrawColor);
            });
        }

        /// <summary>
        /// Apparel 착용 시 추가 드로잉 처리
        /// RimWorld의 표준 패턴: CompDrawWornExtras 오버라이드
        /// </summary>
        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();

            // 유효성 검증
            if (bannerGraphic == null || Wearer == null || !Wearer.Spawned)
            {
                return;
            }

            Pawn pawn = Wearer;
            Vector3 rootLoc = pawn.DrawPos;

            // 소집 상태에 따라 다른 위치에 그리기
            if (ShouldShowOnArm)
            {
                // 소집 시 - 팔/어깨에 표시
                switch (pawn.Rotation.AsInt)
                {
                    case 0: // North
                        DrawBanner(bannerGraphic.MatNorth, rootLoc + draftedOffsetNorth, 0);
                        break;
                    case 1: // East
                        DrawBanner(bannerGraphic.MatEast, rootLoc + draftedOffsetEast, 0);
                        break;
                    case 2: // South
                        DrawBanner(bannerGraphic.MatSouth, rootLoc + draftedOffsetSouth, 0);
                        break;
                    case 3: // West
                        DrawBanner(bannerGraphic.MatWest, rootLoc + draftedOffsetWest, 0);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // 평상시 - 등에 표시
                if (!pawn.Dead && pawn.GetPosture() == PawnPosture.Standing)
                {
                    switch (pawn.Rotation.AsInt)
                    {
                        case 0: // North
                            DrawBanner(bannerGraphic.MatNorth, rootLoc + backOffsetNorth, 0);
                            break;
                        case 1: // East
                            DrawBanner(bannerGraphic.MatEast, rootLoc + backOffsetEast, 0);
                            break;
                        case 2: // South
                            DrawBanner(bannerGraphic.MatSouth, rootLoc + backOffsetSouth, 0);
                            break;
                        case 3: // West
                            DrawBanner(bannerGraphic.MatWest, rootLoc + backOffsetWest, 0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 배너를 실제로 그리는 메서드 (WoodenShield.DrawShield 패턴)
        /// </summary>
        /// <param name="mat">Material</param>
        /// <param name="drawLoc">그릴 위치</param>
        /// <param name="angle">회전 각도</param>
        private void DrawBanner(Material mat, Vector3 drawLoc, float angle)
        {
            Mesh mesh = MeshPool.plane10;
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(angle, Vector3.up), mat, 0);
        }

        /// <summary>
        /// Save/Load 처리
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            // 로드 후 Graphic 재로딩
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                LoadGraphic();
            }
        }
    }
}


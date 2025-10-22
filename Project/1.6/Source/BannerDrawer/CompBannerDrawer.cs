using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// 방향별 드로잉 데이터 (그래픽 경로 + 오프셋 + 각도)
    /// </summary>
    public class BannerDrawData
    {
        public string graphicPath;
        
        public Vector3 offsetNorth = Vector3.zero;
        public Vector3 offsetEast = Vector3.zero;
        public Vector3 offsetSouth = Vector3.zero;
        public Vector3 offsetWest = Vector3.zero;

        public float angleNorth = 0f;
        public float angleEast = 0f;
        public float angleSouth = 0f;
        public float angleWest = 0f;
    }

    /// <summary>
    /// 배너 Apparel의 조건부 드로잉을 담당하는 Component
    /// RimWorld CompShield 패턴을 따름
    /// </summary>
    public class CompProperties_BannerDrawer : CompProperties
    {
        /// <summary>
        /// 드로잉 크기 (기본값: 1.0)
        /// </summary>
        public Vector2 drawSize = new Vector2(1f, 1f);

        /// <summary>
        /// 소집 시 드로잉 데이터 (그래픽 경로 + 팔/어깨 위치)
        /// </summary>
        public BannerDrawData draftedDrawData;

        /// <summary>
        /// 비소집 시 드로잉 데이터 (그래픽 경로 + 등 위치)
        /// </summary>
        public BannerDrawData backDrawData;

        public CompProperties_BannerDrawer()
        {
            this.compClass = typeof(CompBannerDrawer);

            // 기본값 설정 (이전 하드코딩 값)
            draftedDrawData = new BannerDrawData
            {
                graphicPath = "Apparel/Util/RK_TextureApparel_BannerArm",
                offsetNorth = new Vector3(-0.25f, 0.15f, -0.08f),
                offsetSouth = new Vector3(0.25f, 0.15f, -0.08f),
                offsetEast = new Vector3(0.22f, 0.12f, -0.12f),
                offsetWest = new Vector3(-0.22f, 0.12f, -0.12f),
                angleNorth = 0f,
                angleEast = 0f,
                angleSouth = 0f,
                angleWest = 0f
            };

            backDrawData = new BannerDrawData
            {
                graphicPath = "Apparel/Util/RK_TextureApparel_BannerUnarm",
                offsetNorth = new Vector3(0f, -0.18f, -0.08f),
                offsetSouth = new Vector3(0f, -0.12f, -0.12f),
                offsetEast = new Vector3(-0.12f, -0.15f, -0.08f),
                offsetWest = new Vector3(0.12f, -0.15f, -0.08f),
                angleNorth = 0f,
                angleEast = 0f,
                angleSouth = 0f,
                angleWest = 0f
            };
        }
    }

    [StaticConstructorOnStartup]
    public class CompBannerDrawer : ThingComp
    {
        private Graphic bannerGraphicDrafted;  // 소집 시 그래픽
        private Graphic bannerGraphicBack;     // 비소집 시 그래픽

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
            if (bannerGraphicDrafted != null && bannerGraphicBack != null) return;

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                if (parent == null) return;

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

                // 소집 시 그래픽 로딩
                string graphicPathDrafted = Props.draftedDrawData?.graphicPath;
                if (graphicPathDrafted.NullOrEmpty())
                {
                    Log.Warning($"[CompBannerDrawer] {parent.def.defName}: draftedDrawData.graphicPath가 설정되지 않았습니다. 기본 경로 사용.");
                    graphicPathDrafted = "Apparel/Util/RK_TextureApparel_BannerArm";
                }
                bannerGraphicDrafted = GraphicDatabase.Get<Graphic_Multi>(
                    graphicPathDrafted,
                    ShaderDatabase.Cutout,
                    drawSize,
                    parent.DrawColor);

                // 비소집 시 그래픽 로딩
                string graphicPathBack = Props.backDrawData?.graphicPath;
                if (graphicPathBack.NullOrEmpty())
                {
                    Log.Warning($"[CompBannerDrawer] {parent.def.defName}: backDrawData.graphicPath가 설정되지 않았습니다. 기본 경로 사용.");
                    graphicPathBack = "Apparel/Util/RK_TextureApparel_BannerUnarm";
                }
                bannerGraphicBack = GraphicDatabase.Get<Graphic_Multi>(
                    graphicPathBack,
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
            if (Wearer == null || !Wearer.Spawned)
            {
                return;
            }

            Pawn pawn = Wearer;
            Vector3 rootLoc = pawn.DrawPos;

            // 소집 상태에 따라 다른 위치와 그래픽으로 그리기
            if (ShouldShowOnArm)
            {
                // 소집 시 - 팔/어깨에 표시 (Arm 그래픽)
                if (bannerGraphicDrafted == null) return;
                
                BannerDrawData drawData = Props.draftedDrawData;
                switch (pawn.Rotation.AsInt)
                {
                    case 0: // North
                        DrawBanner(bannerGraphicDrafted.MatNorth, rootLoc + drawData.offsetNorth, drawData.angleNorth);
                        break;
                    case 1: // East
                        DrawBanner(bannerGraphicDrafted.MatEast, rootLoc + drawData.offsetEast, drawData.angleEast);
                        break;
                    case 2: // South
                        DrawBanner(bannerGraphicDrafted.MatSouth, rootLoc + drawData.offsetSouth, drawData.angleSouth);
                        break;
                    case 3: // West
                        DrawBanner(bannerGraphicDrafted.MatWest, rootLoc + drawData.offsetWest, drawData.angleWest);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // 평상시 - 등에 표시 (Unarm 그래픽)
                if (bannerGraphicBack == null) return;
                
                if (!pawn.Dead && pawn.GetPosture() == PawnPosture.Standing)
                {
                    BannerDrawData drawData = Props.backDrawData;
                    switch (pawn.Rotation.AsInt)
                    {
                        case 0: // North
                            DrawBanner(bannerGraphicBack.MatNorth, rootLoc + drawData.offsetNorth, drawData.angleNorth);
                            break;
                        case 1: // East
                            DrawBanner(bannerGraphicBack.MatEast, rootLoc + drawData.offsetEast, drawData.angleEast);
                            break;
                        case 2: // South
                            DrawBanner(bannerGraphicBack.MatSouth, rootLoc + drawData.offsetSouth, drawData.angleSouth);
                            break;
                        case 3: // West
                            DrawBanner(bannerGraphicBack.MatWest, rootLoc + drawData.offsetWest, drawData.angleWest);
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


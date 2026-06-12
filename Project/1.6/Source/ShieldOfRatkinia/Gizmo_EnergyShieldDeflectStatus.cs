using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>바닐라 Gizmo_EnergyShieldStatus와 동일 표시(×100, ShieldPersonalTip).</summary>
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyShieldDeflectStatus : Gizmo
    {
        public CompShieldDeflectEnergy shield;

        private static readonly Texture2D FullBarTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyBarTex =
            SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_EnergyShieldDeflectStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Pawn pawn = (shield.parent as Apparel)?.Wearer;
            float max = pawn != null ? shield.EffectiveMax(pawn) : shield.EnergyMax;
            float fillPercent = shield.Energy / Mathf.Max(1f, max);

            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect inner = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);

            Rect labelRect = inner;
            labelRect.height = inner.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, shield.parent.LabelCap);

            Rect barRect = inner;
            barRect.yMin = inner.y + inner.height / 2f;
            Widgets.FillableBar(barRect, fillPercent, FullBarTex, EmptyBarTex, false);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect,
                (shield.Energy * 100f).ToString("F0") + " / " + (max * 100f).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(inner, "ShieldPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}

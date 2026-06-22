using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace NewRatkin
{
    public class RKUniqueArtVariant
    {
        public int graphicIndex = -1;
        public string uniquePawnName;
        public List<string> extraRules;
    }

    public class CompProperties_RKUniqueArt : CompProperties_Art
    {
        public float engravedDaysBefore = 15f;
        public bool useUniquePawnAsAuthor = true;
        public string fallbackUniquePawnName = "unknown unique pawn";
        public List<string> uniquePawnNames;
        public List<string> extraRules;
        public List<RKUniqueArtVariant> variants;

        public CompProperties_RKUniqueArt()
        {
            compClass = typeof(CompRKUniqueArt);
        }
    }

    public class CompRKUniqueArt : CompArt
    {
        private const int TicksPerDay = 60000;

        private static readonly FieldInfo AuthorNameField = typeof(CompArt).GetField("authorNameInt", BindingFlags.Instance | BindingFlags.NonPublic);

        private int engravedTickAbs = -1;
        private int uniquePawnIndex = -1;

        private CompProperties_RKUniqueArt RKProps => (CompProperties_RKUniqueArt)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref engravedTickAbs, "rkEngravedTickAbs", -1);
            Scribe_Values.Look(ref uniquePawnIndex, "rkUniquePawnIndex", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureEngravingInitialized();
                EnsureBaseAuthorName();
            }
        }

        protected override void InitializeArtInternal(Thing relatedThing, ArtGenerationContext source)
        {
            EnsureEngravingInitialized();
            base.InitializeArtInternal(relatedThing, source);
            EnsureBaseAuthorName();
        }

        public override void JustCreatedBy(Pawn pawn)
        {
            if (!RKProps.useUniquePawnAsAuthor)
            {
                base.JustCreatedBy(pawn);
                return;
            }

            EnsureEngravingInitialized();
            EnsureBaseAuthorName();
        }

        public override TaggedString GenerateImageDescription()
        {
            if (RKProps.descriptionMaker == null)
            {
                return base.GenerateImageDescription();
            }

            EnsureInitializedForText();
            return TaleRef.GenerateText(TextGenerationPurpose.ArtDescription, RKProps.descriptionMaker, ExtraRulesForText());
        }

        protected override string GenerateTitle(ArtGenerationContext context)
        {
            if (RKProps.nameMaker == null)
            {
                return base.GenerateTitle(context);
            }

            EnsureInitializedForText();
            return GenText.CapitalizeAsTitle(TaleRef.GenerateText(TextGenerationPurpose.ArtName, RKProps.nameMaker, ExtraRulesForText()));
        }

        public override string CompInspectStringExtra()
        {
            if (!Active)
            {
                return null;
            }

            if (!RKProps.useUniquePawnAsAuthor)
            {
                return base.CompInspectStringExtra();
            }

            EnsureEngravingInitialized();
            EnsureBaseAuthorName();
            return (string)("Author".Translate() + ": " + UniquePawnName) + ("\n" + "Title".Translate() + ": " + Title);
        }

        public override string GetDescriptionPart()
        {
            if (!Active)
            {
                return null;
            }

            if (!RKProps.useUniquePawnAsAuthor)
            {
                return base.GetDescriptionPart();
            }

            EnsureEngravingInitialized();
            EnsureBaseAuthorName();
            return string.Concat(string.Concat("" + Title, "\n\n") + GenerateImageDescription(), "\n\n") + ("Author".Translate() + ": " + UniquePawnName);
        }

        private void EnsureInitializedForText()
        {
            EnsureEngravingInitialized();
            EnsureBaseAuthorName();

            if (TaleRef == null)
            {
                InitializeArt(ArtGenerationContext.Outsider);
            }
        }

        private void EnsureEngravingInitialized()
        {
            if (engravedTickAbs < 0)
            {
                int currentTick = Find.TickManager != null ? Find.TickManager.TicksAbs : 0;
                engravedTickAbs = currentTick - Math.Max(0, Mathf.RoundToInt(RKProps.engravedDaysBefore * TicksPerDay));
            }

            if (uniquePawnIndex < 0)
            {
                uniquePawnIndex = Math.Max(0, parent.OverrideGraphicIndex ?? 0);
            }
        }

        private void EnsureBaseAuthorName()
        {
            if (!RKProps.useUniquePawnAsAuthor || AuthorNameField == null)
            {
                return;
            }

            AuthorNameField.SetValue(this, (TaggedString)UniquePawnName);
        }

        private List<Rule> ExtraRulesForText()
        {
            List<Rule> rules = new List<Rule>
            {
                new Rule_String("RK_UNIQUEPAWN", UniquePawnName),
                new Rule_String("RK_DATE", EngravedDateString),
                new Rule_String("RK_DAYS_BEFORE", RKProps.engravedDaysBefore.ToString("0.##")),
                new Rule_String("RK_VARIANT_INDEX", (uniquePawnIndex + 1).ToString())
            };

            AddRulesFromStrings(rules, RKProps.extraRules);
            AddRulesFromStrings(rules, CurrentVariant?.extraRules);
            return rules;
        }

        private void AddRulesFromStrings(List<Rule> rules, List<string> rulesStrings)
        {
            if (rulesStrings.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < rulesStrings.Count; i++)
            {
                if (!rulesStrings[i].NullOrEmpty())
                {
                    rules.Add(new Rule_String(rulesStrings[i]));
                }
            }
        }

        private string UniquePawnName
        {
            get
            {
                RKUniqueArtVariant variant = CurrentVariant;
                if (variant != null && !variant.uniquePawnName.NullOrEmpty())
                {
                    return variant.uniquePawnName;
                }

                if (!RKProps.uniquePawnNames.NullOrEmpty())
                {
                    int index = Math.Max(0, uniquePawnIndex);
                    return RKProps.uniquePawnNames[index % RKProps.uniquePawnNames.Count];
                }

                return RKProps.fallbackUniquePawnName;
            }
        }

        private RKUniqueArtVariant CurrentVariant
        {
            get
            {
                if (RKProps.variants.NullOrEmpty())
                {
                    return null;
                }

                for (int i = 0; i < RKProps.variants.Count; i++)
                {
                    if (RKProps.variants[i].graphicIndex == uniquePawnIndex)
                    {
                        return RKProps.variants[i];
                    }
                }

                int index = Math.Max(0, uniquePawnIndex);
                return index < RKProps.variants.Count ? RKProps.variants[index] : null;
            }
        }

        private string EngravedDateString
        {
            get
            {
                Vector2 location = Vector2.zero;
                if (parent?.MapHeld != null && parent.MapHeld.Tile.Valid)
                {
                    location = Find.WorldGrid.LongLatOf(parent.MapHeld.Tile);
                }
                else if (parent != null && parent.Tile.Valid)
                {
                    location = Find.WorldGrid.LongLatOf(parent.Tile);
                }

                return GenDate.DateFullStringAt(engravedTickAbs, location);
            }
        }
    }
}

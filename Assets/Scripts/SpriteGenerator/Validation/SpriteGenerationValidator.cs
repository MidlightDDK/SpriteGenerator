using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteGenerator
{
    public sealed class SpriteGenerationValidationResult
    {
        internal SpriteGenerationValidationResult(CharacterSpriteSettings settings, IReadOnlyList<string> warnings)
        {
            Settings = settings;
            Warnings = warnings;
        }

        public CharacterSpriteSettings Settings { get; }

        public IReadOnlyList<string> Warnings { get; }
    }

    public static class SpriteGenerationValidator
    {
        public static SpriteGenerationValidationResult Validate(CharacterSpriteSettings source)
        {
            var warnings = new List<string>();
            CharacterSpriteSettings settings;

            if (source == null)
            {
                settings = new CharacterSpriteSettings();
                warnings.Add("Generation settings were missing, so safe defaults were used.");
            }
            else
            {
                settings = source.Clone();
            }

            settings.Width = ClampWithWarning(
                settings.Width,
                CharacterSpriteSettings.MinimumDimension,
                CharacterSpriteSettings.MaximumDimension,
                "Sprite width",
                warnings);

            settings.Height = ClampWithWarning(
                settings.Height,
                CharacterSpriteSettings.MinimumDimension,
                CharacterSpriteSettings.MaximumDimension,
                "Sprite height",
                warnings);

            if (float.IsNaN(settings.PixelsPerUnit) || float.IsInfinity(settings.PixelsPerUnit) || settings.PixelsPerUnit <= 0f)
            {
                settings.PixelsPerUnit = 8f;
                warnings.Add("Pixels per unit must be positive and finite; the value was reset to 8.");
            }
            else if (settings.PixelsPerUnit > 1024f)
            {
                settings.PixelsPerUnit = 1024f;
                warnings.Add("Pixels per unit exceeded the supported maximum and was clamped to 1024.");
            }

            Vector2 pivot = settings.Pivot;
            if (!IsFinite(pivot.x) || !IsFinite(pivot.y))
            {
                settings.Pivot = new Vector2(0.5f, 0.5f);
                warnings.Add("The sprite pivot was not finite, so a centered pivot was used.");
            }
            else
            {
                Vector2 clampedPivot = new(Mathf.Clamp01(pivot.x), Mathf.Clamp01(pivot.y));
                if (clampedPivot != pivot)
                {
                    settings.Pivot = clampedPivot;
                    warnings.Add("The sprite pivot was clamped to the normalized 0-1 range.");
                }
            }

            settings.OutlineThickness = ClampFinite(
                settings.OutlineThickness,
                0.5f,
                3f,
                1f,
                "Outline thickness",
                warnings);

            CharacterProportions proportions = settings.Proportions;
            proportions.HeadSize = ClampFinite(proportions.HeadSize, 0.8f, 1.2f, 1f, "Head size", warnings);
            proportions.BodyWidth = ClampFinite(proportions.BodyWidth, 0.75f, 1.25f, 1f, "Body width", warnings);
            proportions.LegLength = ClampFinite(proportions.LegLength, 0.8f, 1.2f, 1f, "Leg length", warnings);

            CharacterDesign design = settings.Design;
            EnsureDefined(ref design, warnings);
            design.SkinColor = ValidateVisibleColor(design.SkinColor, new Color(0.76f, 0.49f, 0.30f, 1f), "Skin", warnings);
            design.HairColor = ValidateVisibleColor(design.HairColor, new Color(0.18f, 0.10f, 0.06f, 1f), "Hair", warnings);
            design.OutfitPrimaryColor = ValidateVisibleColor(design.OutfitPrimaryColor, new Color(0.18f, 0.42f, 0.25f, 1f), "Primary outfit", warnings);
            design.OutfitSecondaryColor = ValidateVisibleColor(design.OutfitSecondaryColor, new Color(0.72f, 0.56f, 0.25f, 1f), "Secondary outfit", warnings);
            design.AccessoryColor = ValidateVisibleColor(design.AccessoryColor, new Color(0.42f, 0.24f, 0.12f, 1f), "Accessory", warnings);
            design.EyeColor = ValidateVisibleColor(design.EyeColor, new Color(0.08f, 0.07f, 0.06f, 1f), "Eye", warnings);
            design.OutlineColor = ValidateVisibleColor(design.OutlineColor, new Color(0.07f, 0.055f, 0.07f, 1f), "Outline", warnings);

            return new SpriteGenerationValidationResult(settings, warnings);
        }

        private static void EnsureDefined(ref CharacterDesign design, ICollection<string> warnings)
        {
            if (!Enum.IsDefined(typeof(BodyBuild), design.BodyBuild))
            {
                design.BodyBuild = BodyBuild.Average;
                warnings.Add("Unknown body build was replaced with Average.");
            }

            if (!Enum.IsDefined(typeof(FaceStyle), design.FaceStyle))
            {
                design.FaceStyle = FaceStyle.Friendly;
                warnings.Add("Unknown face style was replaced with Friendly.");
            }

            if (!Enum.IsDefined(typeof(HairStyle), design.HairStyle))
            {
                design.HairStyle = HairStyle.Short;
                warnings.Add("Unknown hair style was replaced with Short.");
            }

            if (!Enum.IsDefined(typeof(OutfitStyle), design.OutfitStyle))
            {
                design.OutfitStyle = OutfitStyle.Tunic;
                warnings.Add("Unknown outfit style was replaced with Tunic.");
            }

            if (!Enum.IsDefined(typeof(AccessoryStyle), design.AccessoryStyle))
            {
                design.AccessoryStyle = AccessoryStyle.None;
                warnings.Add("Unknown accessory style was removed.");
            }

            if (!Enum.IsDefined(typeof(PalettePreset), design.PalettePreset))
            {
                design.PalettePreset = PalettePreset.Forest;
                warnings.Add("Unknown palette preset was replaced with Forest.");
            }
        }

        private static Color ValidateVisibleColor(Color value, Color fallback, string label, ICollection<string> warnings)
        {
            if (!IsFinite(value.r) || !IsFinite(value.g) || !IsFinite(value.b) || !IsFinite(value.a))
            {
                warnings.Add($"{label} color contained a non-finite component and was reset.");
                return fallback;
            }

            Color clamped = new(
                Mathf.Clamp01(value.r),
                Mathf.Clamp01(value.g),
                Mathf.Clamp01(value.b),
                Mathf.Clamp01(value.a));

            if (clamped.a < 0.05f)
            {
                warnings.Add($"{label} color was fully transparent and was reset to keep the character visible.");
                return fallback;
            }

            return clamped;
        }

        private static int ClampWithWarning(int value, int minimum, int maximum, string label, ICollection<string> warnings)
        {
            int clamped = Mathf.Clamp(value, minimum, maximum);
            if (clamped != value)
            {
                warnings.Add($"{label} was clamped to the supported range {minimum}-{maximum}.");
            }

            return clamped;
        }

        private static float ClampFinite(
            float value,
            float minimum,
            float maximum,
            float fallback,
            string label,
            ICollection<string> warnings)
        {
            if (!IsFinite(value))
            {
                warnings.Add($"{label} was not finite and was reset to {fallback}.");
                return fallback;
            }

            float clamped = Mathf.Clamp(value, minimum, maximum);
            if (!Mathf.Approximately(clamped, value))
            {
                warnings.Add($"{label} was clamped to the supported range {minimum:0.##}-{maximum:0.##}.");
            }

            return clamped;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

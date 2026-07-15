using System.Collections.Generic;
using UnityEngine;

namespace SpriteGenerator
{
    internal sealed class CharacterDrawingContext
    {
        internal const int LogicalSize = 32;
        internal const int CenterX = 16;

        internal CharacterDrawingContext(PixelCanvas canvas, CharacterSpriteSettings settings)
        {
            Canvas = canvas;
            Settings = settings;
            Design = settings.Design;

            var proportions = settings.Proportions;
            var headScale = Mathf.Clamp(proportions.HeadSize, 0.8f, 1.2f);
            var bodyScale = Mathf.Clamp(proportions.BodyWidth, 0.75f, 1.25f);
            var legScale = Mathf.Clamp(proportions.LegLength, 0.8f, 1.2f);

            HeadWidth = Mathf.Clamp(Mathf.RoundToInt(11f * headScale), 8, 14);
            HeadHeight = Mathf.Clamp(Mathf.RoundToInt(10f * headScale), 8, 12);
            HeadLeft = CenterX - (HeadWidth / 2);
            HeadBottom = 20;

            var buildWidth = Design.BodyBuild switch
            {
                BodyBuild.Slim => 7,
                BodyBuild.Broad => 11,
                _ => 9
            };

            TorsoWidth = Mathf.Clamp(Mathf.RoundToInt(buildWidth * bodyScale), 6, 14);
            TorsoLeft = CenterX - (TorsoWidth / 2);
            LegTop = Mathf.Clamp(Mathf.RoundToInt(11f + ((legScale - 1f) * 5f)), 10, 12);
            TorsoBottom = LegTop - 1;
            LegWidth = Mathf.Clamp((TorsoWidth - 2) / 2, 2, 5);
            LeftLegX = CenterX - LegWidth - 1;
            RightLegX = CenterX + 1;

            OutlineThickness = Mathf.Clamp(Mathf.RoundToInt(settings.OutlineThickness), 1, 3);

            Skin = PixelColor.FromColor(Design.SkinColor);
            SkinShadow = PixelColor.Darken(Skin, 0.23f);
            SkinHighlight = PixelColor.Lighten(Skin, 0.2f);
            Hair = PixelColor.FromColor(Design.HairColor);
            HairShadow = PixelColor.Darken(Hair, 0.3f);
            HairHighlight = PixelColor.Lighten(Hair, 0.24f);
            Primary = PixelColor.FromColor(Design.OutfitPrimaryColor);
            PrimaryShadow = PixelColor.Darken(Primary, 0.28f);
            PrimaryHighlight = PixelColor.Lighten(Primary, 0.23f);
            Secondary = PixelColor.FromColor(Design.OutfitSecondaryColor);
            SecondaryShadow = PixelColor.Darken(Secondary, 0.28f);
            SecondaryHighlight = PixelColor.Lighten(Secondary, 0.28f);
            Accessory = PixelColor.FromColor(Design.AccessoryColor);
            AccessoryShadow = PixelColor.Darken(Accessory, 0.3f);
            AccessoryHighlight = PixelColor.Lighten(Accessory, 0.28f);
            Eye = PixelColor.FromColor(Design.EyeColor);
            Outline = PixelColor.FromColor(Design.OutlineColor);
        }

        internal PixelCanvas Canvas { get; }

        internal CharacterSpriteSettings Settings { get; }

        internal CharacterDesign Design { get; }

        internal int HeadWidth { get; }

        internal int HeadHeight { get; }

        internal int HeadLeft { get; }

        internal int HeadRight => HeadLeft + HeadWidth - 1;

        internal int HeadBottom { get; }

        internal int HeadTop => HeadBottom + HeadHeight - 1;

        internal int TorsoWidth { get; }

        internal int TorsoLeft { get; }

        internal int TorsoRight => TorsoLeft + TorsoWidth - 1;

        internal int TorsoBottom { get; }

        internal int TorsoTop => HeadBottom;

        internal int LegBottom => 3;

        internal int LegTop { get; }

        internal int LegWidth { get; }

        internal int LeftLegX { get; }

        internal int RightLegX { get; }

        internal int ArmWidth => 3;

        internal int ArmBottom => 10;

        internal int ArmTop => 19;

        internal int LeftArmX => TorsoLeft - 2;

        internal int RightArmX => TorsoRight;

        internal int OutlineThickness { get; }

        internal Color32 Skin { get; }

        internal Color32 SkinShadow { get; }

        internal Color32 SkinHighlight { get; }

        internal Color32 Hair { get; }

        internal Color32 HairShadow { get; }

        internal Color32 HairHighlight { get; }

        internal Color32 Primary { get; }

        internal Color32 PrimaryShadow { get; }

        internal Color32 PrimaryHighlight { get; }

        internal Color32 Secondary { get; }

        internal Color32 SecondaryShadow { get; }

        internal Color32 SecondaryHighlight { get; }

        internal Color32 Accessory { get; }

        internal Color32 AccessoryShadow { get; }

        internal Color32 AccessoryHighlight { get; }

        internal Color32 Eye { get; }

        internal Color32 Outline { get; }

        internal void FillOutlinedRect(int x, int y, int width, int height, Color32 fill)
        {
            FillOutlinedShape(x, y, width, height, fill, false);
        }

        internal void FillOutlinedRoundedRect(int x, int y, int width, int height, Color32 fill)
        {
            FillOutlinedShape(x, y, width, height, fill, true);
        }

        internal void FillOutlinedEllipse(int x, int y, int width, int height, Color32 fill)
        {
            if (!Settings.DrawOutline)
            {
                Canvas.FillEllipse(x, y, width, height, fill);
                return;
            }

            Canvas.FillEllipse(x, y, width, height, Outline);
            var insetX = CalculateInset(width);
            var insetY = CalculateInset(height);
            Canvas.FillEllipse(
                x + insetX,
                y + insetY,
                Mathf.Max(1, width - (insetX * 2)),
                Mathf.Max(1, height - (insetY * 2)),
                fill);
        }

        internal void FillPolygon(IReadOnlyList<Vector2Int> points, Color32 fill, bool outline = true)
        {
            Canvas.FillPolygon(points, fill);
            if (!outline || !Settings.DrawOutline)
            {
                return;
            }

            for (var index = 0; index < points.Count; index++)
            {
                var next = (index + 1) % points.Count;
                Canvas.DrawLine(
                    points[index].x,
                    points[index].y,
                    points[next].x,
                    points[next].y,
                    Outline,
                    OutlineThickness);
            }
        }

        private void FillOutlinedShape(int x, int y, int width, int height, Color32 fill, bool rounded)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (!Settings.DrawOutline)
            {
                if (rounded)
                {
                    Canvas.FillRoundedRect(x, y, width, height, fill);
                }
                else
                {
                    Canvas.FillRect(x, y, width, height, fill);
                }

                return;
            }

            if (rounded)
            {
                Canvas.FillRoundedRect(x, y, width, height, Outline);
            }
            else
            {
                Canvas.FillRect(x, y, width, height, Outline);
            }

            var insetX = CalculateInset(width);
            var insetY = CalculateInset(height);
            var innerWidth = Mathf.Max(1, width - (insetX * 2));
            var innerHeight = Mathf.Max(1, height - (insetY * 2));
            if (rounded)
            {
                Canvas.FillRoundedRect(x + insetX, y + insetY, innerWidth, innerHeight, fill);
            }
            else
            {
                Canvas.FillRect(x + insetX, y + insetY, innerWidth, innerHeight, fill);
            }
        }

        private int CalculateInset(int dimension)
        {
            return Mathf.Min(OutlineThickness, Mathf.Max(0, (dimension - 1) / 2));
        }
    }
}

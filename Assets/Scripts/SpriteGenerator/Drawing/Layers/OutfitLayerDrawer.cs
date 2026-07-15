using UnityEngine;

namespace SpriteGenerator
{
    internal static class OutfitLayerDrawer
    {
        internal static void Draw(CharacterDrawingContext context)
        {
            switch (context.Design.OutfitStyle)
            {
                case OutfitStyle.Armor:
                    DrawArmor(context);
                    break;
                case OutfitStyle.Robe:
                    DrawRobe(context);
                    break;
                case OutfitStyle.Traveler:
                    DrawTraveler(context);
                    break;
                default:
                    DrawTunic(context);
                    break;
            }
        }

        private static void DrawTunic(CharacterDrawingContext context)
        {
            DrawBoots(context, context.AccessoryShadow, 4);
            DrawSeparatedLegwear(context, context.PrimaryShadow, 6, context.LegTop);
            DrawSleeves(context, context.Primary, 14, context.ArmTop);

            var torsoHeight = context.TorsoTop - context.TorsoBottom + 1;
            context.FillOutlinedRoundedRect(
                context.TorsoLeft,
                context.TorsoBottom,
                context.TorsoWidth,
                torsoHeight,
                context.Primary);

            context.Canvas.DrawVerticalLine(
                context.TorsoRight - 1,
                context.TorsoBottom + 2,
                context.TorsoTop - 2,
                context.PrimaryShadow);
            context.Canvas.DrawVerticalLine(
                context.TorsoLeft + 1,
                context.TorsoTop - 4,
                context.TorsoTop - 2,
                context.PrimaryHighlight);

            var beltY = context.TorsoBottom + 2;
            context.Canvas.DrawHorizontalLine(
                context.TorsoLeft + 1,
                context.TorsoRight - 1,
                beltY,
                context.SecondaryShadow);
            context.Canvas.FillRect(CharacterDrawingContext.CenterX - 1, beltY, 2, 2, context.SecondaryHighlight);
            context.Canvas.DrawHorizontalLine(
                context.TorsoLeft + 2,
                context.TorsoRight - 2,
                context.TorsoBottom + 1,
                context.Secondary);
        }

        private static void DrawArmor(CharacterDrawingContext context)
        {
            DrawBoots(context, context.PrimaryShadow, 4);
            DrawSeparatedLegwear(context, context.SecondaryShadow, 5, context.LegTop);
            context.Canvas.DrawVerticalLine(
                context.LeftLegX + 1,
                6,
                context.LegTop - 1,
                context.SecondaryHighlight);
            context.Canvas.DrawVerticalLine(
                context.RightLegX + 1,
                6,
                context.LegTop - 1,
                context.SecondaryHighlight);

            DrawSleeves(context, context.PrimaryShadow, 14, context.ArmTop - 1);
            context.FillOutlinedRoundedRect(
                context.TorsoLeft,
                context.TorsoBottom,
                context.TorsoWidth,
                context.TorsoTop - context.TorsoBottom + 1,
                context.PrimaryShadow);

            context.FillOutlinedRoundedRect(
                context.TorsoLeft + 1,
                context.TorsoBottom + 2,
                context.TorsoWidth - 2,
                context.TorsoTop - context.TorsoBottom - 2,
                context.Secondary);

            context.FillOutlinedEllipse(context.LeftArmX - 1, 17, 5, 4, context.Secondary);
            context.FillOutlinedEllipse(context.RightArmX - 1, 17, 5, 4, context.Secondary);

            context.Canvas.DrawVerticalLine(
                CharacterDrawingContext.CenterX,
                context.TorsoBottom + 4,
                context.TorsoTop - 2,
                context.SecondaryHighlight);
            context.Canvas.DrawHorizontalLine(
                context.TorsoLeft + 2,
                context.TorsoRight - 2,
                context.TorsoBottom + 4,
                context.SecondaryShadow);
            context.Canvas.FillRect(CharacterDrawingContext.CenterX - 1, context.TorsoBottom + 5, 2, 2, context.Primary);
        }

        private static void DrawRobe(CharacterDrawingContext context)
        {
            DrawBoots(context, context.AccessoryShadow, 3);

            var robe = new[]
            {
                new Vector2Int(context.TorsoLeft, context.TorsoTop),
                new Vector2Int(context.TorsoRight, context.TorsoTop),
                new Vector2Int(context.TorsoRight + 1, 12),
                new Vector2Int(context.TorsoRight + 3, 4),
                new Vector2Int(CharacterDrawingContext.CenterX + 1, 3),
                new Vector2Int(CharacterDrawingContext.CenterX - 2, 3),
                new Vector2Int(context.TorsoLeft - 3, 4),
                new Vector2Int(context.TorsoLeft - 1, 12)
            };

            DrawSleeves(context, context.Primary, 12, context.ArmTop);
            context.FillPolygon(robe, context.Primary);
            context.Canvas.DrawLine(
                CharacterDrawingContext.CenterX,
                5,
                CharacterDrawingContext.CenterX,
                context.TorsoTop - 2,
                context.PrimaryShadow);
            context.Canvas.DrawLine(
                CharacterDrawingContext.CenterX - 1,
                5,
                CharacterDrawingContext.CenterX - 1,
                context.TorsoTop - 2,
                context.Secondary);
            context.Canvas.DrawHorizontalLine(
                context.TorsoLeft - 1,
                context.TorsoRight + 1,
                5,
                context.SecondaryShadow);

            context.Canvas.FillRect(CharacterDrawingContext.CenterX - 2, 18, 4, 2, context.Secondary);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX - 2, 19, context.SecondaryHighlight);
        }

        private static void DrawTraveler(CharacterDrawingContext context)
        {
            DrawBoots(context, context.AccessoryShadow, 5);
            DrawSeparatedLegwear(context, context.PrimaryShadow, 6, context.LegTop);
            DrawSleeves(context, context.Primary, 13, context.ArmTop);

            context.FillOutlinedRoundedRect(
                context.TorsoLeft,
                context.TorsoBottom,
                context.TorsoWidth,
                context.TorsoTop - context.TorsoBottom + 1,
                context.SecondaryShadow);

            var jacketHalfWidth = Mathf.Max(2, (context.TorsoWidth - 1) / 2);
            context.Canvas.FillRect(
                context.TorsoLeft + 1,
                context.TorsoBottom + 1,
                jacketHalfWidth,
                context.TorsoTop - context.TorsoBottom - 1,
                context.Primary);
            context.Canvas.FillRect(
                context.TorsoRight - jacketHalfWidth,
                context.TorsoBottom + 1,
                jacketHalfWidth,
                context.TorsoTop - context.TorsoBottom - 1,
                context.PrimaryShadow);
            context.Canvas.DrawVerticalLine(
                CharacterDrawingContext.CenterX,
                context.TorsoBottom + 2,
                context.TorsoTop - 2,
                context.SecondaryHighlight);

            var beltY = context.TorsoBottom + 2;
            context.Canvas.DrawHorizontalLine(
                context.TorsoLeft + 1,
                context.TorsoRight - 1,
                beltY,
                context.Accessory);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX, beltY, context.AccessoryHighlight);

            context.Canvas.FillRect(context.TorsoLeft + 1, context.TorsoTop - 1, context.TorsoWidth - 2, 2, context.Secondary);
            context.Canvas.SetPixel(context.TorsoRight - 2, context.TorsoTop - 1, context.SecondaryHighlight);
        }

        private static void DrawSeparatedLegwear(
            CharacterDrawingContext context,
            Color32 color,
            int bottom,
            int top)
        {
            var height = Mathf.Max(1, top - bottom + 1);
            context.FillOutlinedRect(context.LeftLegX, bottom, context.LegWidth, height, color);
            context.FillOutlinedRect(context.RightLegX, bottom, context.LegWidth, height, color);
        }

        private static void DrawBoots(CharacterDrawingContext context, Color32 color, int top)
        {
            var height = Mathf.Max(1, top - context.LegBottom + 1);
            context.FillOutlinedRoundedRect(context.LeftLegX - 1, context.LegBottom, context.LegWidth + 1, height, color);
            context.FillOutlinedRoundedRect(context.RightLegX, context.LegBottom, context.LegWidth + 1, height, color);
            context.Canvas.SetPixel(context.LeftLegX, context.LegBottom + 1, context.AccessoryHighlight);
            context.Canvas.SetPixel(context.RightLegX + 1, context.LegBottom + 1, context.AccessoryHighlight);
        }

        private static void DrawSleeves(
            CharacterDrawingContext context,
            Color32 color,
            int bottom,
            int top)
        {
            var height = Mathf.Max(1, top - bottom + 1);
            context.FillOutlinedRoundedRect(context.LeftArmX, bottom, context.ArmWidth, height, color);
            context.FillOutlinedRoundedRect(context.RightArmX, bottom, context.ArmWidth, height, color);
        }
    }
}

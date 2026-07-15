using UnityEngine;

namespace SpriteGenerator
{
    internal static class AccessoryLayerDrawer
    {
        internal static void DrawBack(CharacterDrawingContext context)
        {
            if (context.Design.AccessoryStyle != AccessoryStyle.Cape)
            {
                return;
            }

            var cape = new[]
            {
                new Vector2Int(context.TorsoLeft - 2, 19),
                new Vector2Int(context.TorsoRight + 2, 19),
                new Vector2Int(context.TorsoRight + 4, 8),
                new Vector2Int(CharacterDrawingContext.CenterX + 4, 6),
                new Vector2Int(CharacterDrawingContext.CenterX, 8),
                new Vector2Int(CharacterDrawingContext.CenterX - 4, 6),
                new Vector2Int(context.TorsoLeft - 4, 8)
            };

            context.FillPolygon(cape, context.Accessory);
            context.Canvas.DrawLine(
                context.TorsoRight + 1,
                17,
                CharacterDrawingContext.CenterX + 4,
                8,
                context.AccessoryShadow);
            context.Canvas.DrawLine(
                context.TorsoLeft - 1,
                17,
                CharacterDrawingContext.CenterX - 3,
                8,
                context.AccessoryHighlight);
        }

        internal static void DrawFront(CharacterDrawingContext context)
        {
            switch (context.Design.AccessoryStyle)
            {
                case AccessoryStyle.Glasses:
                    DrawGlasses(context);
                    break;
                case AccessoryStyle.Headband:
                    DrawHeadband(context);
                    break;
                case AccessoryStyle.Cape:
                    DrawCapeClasp(context);
                    break;
                case AccessoryStyle.Satchel:
                    DrawSatchel(context);
                    break;
            }
        }

        private static void DrawGlasses(CharacterDrawingContext context)
        {
            var eyeY = context.HeadBottom + (context.HeadHeight / 2);
            var offset = context.HeadWidth >= 11 ? 3 : 2;
            var leftX = CharacterDrawingContext.CenterX - offset - 1;
            var rightX = CharacterDrawingContext.CenterX + offset - 1;
            var frameColor = context.Accessory.a == 0 ? context.Outline : context.Accessory;

            context.Canvas.DrawRect(leftX, eyeY - 1, 4, 3, frameColor);
            context.Canvas.DrawRect(rightX, eyeY - 1, 4, 3, frameColor);
            context.Canvas.DrawHorizontalLine(leftX + 3, rightX, eyeY, frameColor);
            context.Canvas.SetPixel(leftX + 1, eyeY + 1, context.AccessoryHighlight);
            context.Canvas.SetPixel(rightX + 1, eyeY + 1, context.AccessoryHighlight);
        }

        private static void DrawHeadband(CharacterDrawingContext context)
        {
            // The outline brush expands below the requested row, so anchor the band near
            // the crown to keep even the thickest outline clear of the eyes.
            var bandY = context.HeadTop - 1;
            if (context.Settings.DrawOutline)
            {
                context.Canvas.DrawLine(
                    context.HeadLeft,
                    bandY,
                    context.HeadRight,
                    bandY,
                    context.Outline,
                    Mathf.Min(3, context.OutlineThickness + 1));
            }

            context.Canvas.DrawHorizontalLine(context.HeadLeft, context.HeadRight, bandY, context.Accessory);
            context.Canvas.DrawHorizontalLine(
                context.HeadLeft + 2,
                CharacterDrawingContext.CenterX,
                bandY + 1,
                context.AccessoryHighlight);
            context.Canvas.DrawLine(context.HeadRight, bandY, context.HeadRight + 2, bandY - 2, context.AccessoryShadow);
        }

        private static void DrawCapeClasp(CharacterDrawingContext context)
        {
            context.FillOutlinedEllipse(CharacterDrawingContext.CenterX - 3, 18, 3, 3, context.Secondary);
            context.FillOutlinedEllipse(CharacterDrawingContext.CenterX + 1, 18, 3, 3, context.Secondary);
            context.Canvas.DrawHorizontalLine(
                CharacterDrawingContext.CenterX - 1,
                CharacterDrawingContext.CenterX + 1,
                19,
                context.AccessoryHighlight);
        }

        private static void DrawSatchel(CharacterDrawingContext context)
        {
            context.Canvas.DrawLine(
                context.TorsoLeft + 1,
                context.TorsoTop - 1,
                context.TorsoRight + 2,
                context.TorsoBottom + 1,
                context.AccessoryShadow,
                2);
            context.Canvas.DrawLine(
                context.TorsoLeft + 1,
                context.TorsoTop - 1,
                context.TorsoRight + 2,
                context.TorsoBottom + 1,
                context.Accessory,
                1);

            context.FillOutlinedRoundedRect(
                context.TorsoRight,
                context.TorsoBottom - 1,
                6,
                6,
                context.Accessory);
            context.Canvas.DrawHorizontalLine(
                context.TorsoRight + 1,
                context.TorsoRight + 4,
                context.TorsoBottom + 2,
                context.AccessoryShadow);
            context.Canvas.SetPixel(context.TorsoRight + 2, context.TorsoBottom + 1, context.AccessoryHighlight);
        }
    }
}

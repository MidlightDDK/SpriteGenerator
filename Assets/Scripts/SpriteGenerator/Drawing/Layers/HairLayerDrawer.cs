using UnityEngine;

namespace SpriteGenerator
{
    internal static class HairLayerDrawer
    {
        internal static void DrawBack(CharacterDrawingContext context)
        {
            switch (context.Design.HairStyle)
            {
                case HairStyle.Bob:
                    context.FillOutlinedRoundedRect(
                        context.HeadLeft - 1,
                        context.HeadBottom - 1,
                        context.HeadWidth + 2,
                        context.HeadHeight,
                        context.HairShadow);
                    break;
                case HairStyle.Ponytail:
                    DrawPonytailBack(context);
                    break;
            }
        }

        internal static void DrawFront(CharacterDrawingContext context)
        {
            switch (context.Design.HairStyle)
            {
                case HairStyle.None:
                    return;
                case HairStyle.Bob:
                    DrawBob(context);
                    return;
                case HairStyle.Spiky:
                    DrawSpiky(context);
                    return;
                case HairStyle.Mohawk:
                    DrawMohawk(context);
                    return;
                case HairStyle.Ponytail:
                    DrawSweptCap(context);
                    return;
                default:
                    DrawShort(context);
                    return;
            }
        }

        private static void DrawPonytailBack(CharacterDrawingContext context)
        {
            var attachmentX = Mathf.Min(28, context.HeadRight + 1);
            var attachmentY = context.HeadTop - 3;
            var tail = new[]
            {
                new Vector2Int(attachmentX - 1, attachmentY),
                new Vector2Int(attachmentX + 2, attachmentY - 1),
                new Vector2Int(attachmentX + 3, attachmentY - 5),
                new Vector2Int(attachmentX + 1, attachmentY - 9),
                new Vector2Int(attachmentX - 1, attachmentY - 7),
                new Vector2Int(attachmentX, attachmentY - 4)
            };

            context.FillPolygon(tail, context.Hair);
            context.FillOutlinedEllipse(attachmentX - 1, attachmentY - 2, 4, 4, context.Hair);
            context.Canvas.DrawLine(
                attachmentX + 1,
                attachmentY - 3,
                attachmentX + 1,
                attachmentY - 7,
                context.HairHighlight);
        }

        private static void DrawShort(CharacterDrawingContext context)
        {
            var cap = CreateCap(context, 0);
            context.FillPolygon(cap, context.Hair);
            DrawHairHighlights(context, context.HeadLeft + 3, context.HeadTop - 2, 4);
        }

        private static void DrawBob(CharacterDrawingContext context)
        {
            var cap = CreateCap(context, 1);
            context.FillPolygon(cap, context.Hair);
            context.FillOutlinedRoundedRect(
                context.HeadLeft,
                context.HeadBottom + 1,
                2,
                context.HeadHeight - 3,
                context.Hair);
            context.FillOutlinedRoundedRect(
                context.HeadRight - 1,
                context.HeadBottom + 1,
                2,
                context.HeadHeight - 3,
                context.HairShadow);
            context.Canvas.SetPixel(context.HeadLeft + 1, context.HeadBottom + 3, context.HairHighlight);
            DrawHairHighlights(context, context.HeadLeft + 3, context.HeadTop - 2, 3);
        }

        private static void DrawSpiky(CharacterDrawingContext context)
        {
            var left = context.HeadLeft;
            var right = context.HeadRight;
            var top = context.HeadTop;
            var baseY = context.HeadBottom + (context.HeadHeight / 2) + 1;
            var spikes = new[]
            {
                new Vector2Int(left, baseY),
                new Vector2Int(left + 1, top - 2),
                new Vector2Int(left + 2, Mathf.Min(31, top + 1)),
                new Vector2Int(left + 4, top - 1),
                new Vector2Int(left + 6, Mathf.Min(31, top + 2)),
                new Vector2Int(left + 8, top - 1),
                new Vector2Int(right - 1, Mathf.Min(31, top + 1)),
                new Vector2Int(right, top - 2),
                new Vector2Int(right, baseY),
                new Vector2Int(right - 2, baseY + 1),
                new Vector2Int(right - 4, baseY),
                new Vector2Int(CharacterDrawingContext.CenterX, baseY + 1),
                new Vector2Int(left + 3, baseY)
            };

            context.FillPolygon(spikes, context.Hair);
            context.Canvas.DrawLine(left + 3, top - 1, left + 5, top, context.HairHighlight);
            context.Canvas.SetPixel(left + 2, baseY + 1, context.HairShadow);
        }

        private static void DrawMohawk(CharacterDrawingContext context)
        {
            var top = context.HeadTop;
            var bottom = top - 4;
            var mohawk = new[]
            {
                new Vector2Int(CharacterDrawingContext.CenterX - 3, bottom),
                new Vector2Int(CharacterDrawingContext.CenterX - 2, top),
                new Vector2Int(CharacterDrawingContext.CenterX - 1, Mathf.Min(31, top + 2)),
                new Vector2Int(CharacterDrawingContext.CenterX, top),
                new Vector2Int(CharacterDrawingContext.CenterX + 1, 31),
                new Vector2Int(CharacterDrawingContext.CenterX + 3, top - 1),
                new Vector2Int(CharacterDrawingContext.CenterX + 3, bottom)
            };

            context.FillPolygon(mohawk, context.Hair);
            context.Canvas.DrawVerticalLine(
                CharacterDrawingContext.CenterX,
                bottom + 1,
                Mathf.Min(31, top + 1),
                context.HairHighlight);
            context.Canvas.SetPixel(context.HeadLeft + 1, top - 2, context.HairShadow);
            context.Canvas.SetPixel(context.HeadRight - 1, top - 2, context.HairShadow);
        }

        private static void DrawSweptCap(CharacterDrawingContext context)
        {
            var cap = CreateCap(context, 0);
            context.FillPolygon(cap, context.Hair);
            context.Canvas.DrawLine(
                context.HeadLeft + 2,
                context.HeadTop - 2,
                context.HeadRight - 2,
                context.HeadTop - 5,
                context.HairHighlight);

            var tieX = Mathf.Min(29, context.HeadRight + 1);
            context.FillOutlinedEllipse(tieX, context.HeadTop - 5, 3, 3, context.Accessory);
        }

        private static Vector2Int[] CreateCap(CharacterDrawingContext context, int fringeDepth)
        {
            var left = context.HeadLeft;
            var right = context.HeadRight;
            var top = context.HeadTop;
            var eyeLine = context.HeadBottom + (context.HeadHeight / 2);
            var baseY = eyeLine + 1 - fringeDepth;
            return new[]
            {
                new Vector2Int(left, baseY),
                new Vector2Int(left + 1, top - 1),
                new Vector2Int(left + 3, top),
                new Vector2Int(right - 2, top),
                new Vector2Int(right, top - 2),
                new Vector2Int(right, baseY),
                new Vector2Int(right - 2, baseY + 1),
                new Vector2Int(right - 4, baseY),
                new Vector2Int(CharacterDrawingContext.CenterX + 1, baseY + 1),
                new Vector2Int(CharacterDrawingContext.CenterX - 2, baseY),
                new Vector2Int(left + 2, baseY + 1)
            };
        }

        private static void DrawHairHighlights(CharacterDrawingContext context, int x, int y, int length)
        {
            context.Canvas.DrawHorizontalLine(x, x + length - 1, y, context.HairHighlight);
            context.Canvas.SetPixel(x, y - 1, context.HairHighlight);
        }
    }
}

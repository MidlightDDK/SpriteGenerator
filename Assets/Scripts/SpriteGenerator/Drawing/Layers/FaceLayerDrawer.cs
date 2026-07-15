namespace SpriteGenerator
{
    internal static class FaceLayerDrawer
    {
        internal static void Draw(CharacterDrawingContext context)
        {
            var eyeY = context.HeadBottom + (context.HeadHeight / 2);
            var eyeOffset = context.HeadWidth >= 11 ? 3 : 2;
            var leftEye = CharacterDrawingContext.CenterX - eyeOffset;
            var rightEye = CharacterDrawingContext.CenterX + eyeOffset;

            switch (context.Design.FaceStyle)
            {
                case FaceStyle.Determined:
                    DrawDetermined(context, leftEye, rightEye, eyeY);
                    break;
                case FaceStyle.Cheerful:
                    DrawCheerful(context, leftEye, rightEye, eyeY);
                    break;
                default:
                    DrawFriendly(context, leftEye, rightEye, eyeY);
                    break;
            }
        }

        private static void DrawFriendly(
            CharacterDrawingContext context,
            int leftEye,
            int rightEye,
            int eyeY)
        {
            DrawOpenEyes(context, leftEye, rightEye, eyeY);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX, eyeY - 2, context.SkinShadow);

            var mouthY = eyeY - 4;
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX - 1, mouthY + 1, context.Outline);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX, mouthY, context.Outline);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX + 1, mouthY + 1, context.Outline);
            context.Canvas.SetPixel(context.HeadLeft + 2, mouthY + 1, context.SkinHighlight);
        }

        private static void DrawDetermined(
            CharacterDrawingContext context,
            int leftEye,
            int rightEye,
            int eyeY)
        {
            context.Canvas.DrawLine(leftEye - 1, eyeY + 2, leftEye + 1, eyeY + 1, context.HairShadow);
            context.Canvas.DrawLine(rightEye - 1, eyeY + 1, rightEye + 1, eyeY + 2, context.HairShadow);
            DrawOpenEyes(context, leftEye, rightEye, eyeY);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX, eyeY - 2, context.SkinShadow);
            context.Canvas.DrawHorizontalLine(
                CharacterDrawingContext.CenterX - 2,
                CharacterDrawingContext.CenterX + 2,
                eyeY - 4,
                context.Outline);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX + 2, eyeY - 3, context.SkinShadow);
        }

        private static void DrawCheerful(
            CharacterDrawingContext context,
            int leftEye,
            int rightEye,
            int eyeY)
        {
            context.Canvas.SetPixel(leftEye - 1, eyeY, context.Eye);
            context.Canvas.SetPixel(leftEye, eyeY - 1, context.Eye);
            context.Canvas.SetPixel(leftEye + 1, eyeY, context.Eye);
            context.Canvas.SetPixel(rightEye - 1, eyeY, context.Eye);
            context.Canvas.SetPixel(rightEye, eyeY - 1, context.Eye);
            context.Canvas.SetPixel(rightEye + 1, eyeY, context.Eye);

            var mouthY = eyeY - 4;
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX - 2, mouthY + 1, context.Outline);
            context.Canvas.DrawHorizontalLine(
                CharacterDrawingContext.CenterX - 1,
                CharacterDrawingContext.CenterX + 1,
                mouthY,
                context.Outline);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX + 2, mouthY + 1, context.Outline);
            context.Canvas.SetPixel(CharacterDrawingContext.CenterX, mouthY + 1, context.SkinHighlight);
            context.Canvas.SetPixel(context.HeadLeft + 2, mouthY + 2, context.SkinHighlight);
            context.Canvas.SetPixel(context.HeadRight - 2, mouthY + 2, context.SkinHighlight);
        }

        private static void DrawOpenEyes(
            CharacterDrawingContext context,
            int leftEye,
            int rightEye,
            int eyeY)
        {
            var eyeWhite = PixelColor.Lighten(context.SkinHighlight, 0.78f);
            context.Canvas.SetPixel(leftEye, eyeY, eyeWhite);
            context.Canvas.SetPixel(leftEye, eyeY - 1, context.Eye);
            context.Canvas.SetPixel(rightEye, eyeY, eyeWhite);
            context.Canvas.SetPixel(rightEye, eyeY - 1, context.Eye);
        }
    }
}

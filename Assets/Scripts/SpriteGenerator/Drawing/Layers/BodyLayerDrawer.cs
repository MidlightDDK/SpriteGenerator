namespace SpriteGenerator
{
    internal static class BodyLayerDrawer
    {
        internal static void Draw(CharacterDrawingContext context)
        {
            DrawLegs(context);
            DrawArms(context);
            DrawNeckAndEars(context);
            DrawHead(context);
        }

        private static void DrawLegs(CharacterDrawingContext context)
        {
            var height = context.LegTop - context.LegBottom + 1;
            context.FillOutlinedRoundedRect(
                context.LeftLegX,
                context.LegBottom,
                context.LegWidth,
                height,
                context.Skin);
            context.FillOutlinedRoundedRect(
                context.RightLegX,
                context.LegBottom,
                context.LegWidth,
                height,
                context.Skin);

            context.Canvas.DrawVerticalLine(
                context.LeftLegX + context.LegWidth - 1,
                context.LegBottom + 1,
                context.LegTop - 1,
                context.SkinShadow);
            context.Canvas.DrawVerticalLine(
                context.RightLegX + context.LegWidth - 1,
                context.LegBottom + 1,
                context.LegTop - 1,
                context.SkinShadow);
        }

        private static void DrawArms(CharacterDrawingContext context)
        {
            var armHeight = context.ArmTop - context.ArmBottom + 1;
            context.FillOutlinedRoundedRect(
                context.LeftArmX,
                context.ArmBottom,
                context.ArmWidth,
                armHeight,
                context.Skin);
            context.FillOutlinedRoundedRect(
                context.RightArmX,
                context.ArmBottom,
                context.ArmWidth,
                armHeight,
                context.Skin);

            context.Canvas.SetPixel(context.LeftArmX + 1, context.ArmBottom + 1, context.SkinHighlight);
            context.Canvas.SetPixel(context.RightArmX + 1, context.ArmBottom + 1, context.SkinHighlight);
        }

        private static void DrawNeckAndEars(CharacterDrawingContext context)
        {
            context.FillOutlinedRoundedRect(CharacterDrawingContext.CenterX - 2, 18, 4, 5, context.Skin);

            var earY = context.HeadBottom + (context.HeadHeight / 2) - 1;
            context.FillOutlinedEllipse(context.HeadLeft - 1, earY, 3, 4, context.Skin);
            context.FillOutlinedEllipse(context.HeadRight - 1, earY, 3, 4, context.Skin);
            context.Canvas.SetPixel(context.HeadLeft, earY + 1, context.SkinShadow);
            context.Canvas.SetPixel(context.HeadRight, earY + 1, context.SkinShadow);
        }

        private static void DrawHead(CharacterDrawingContext context)
        {
            context.FillOutlinedRoundedRect(
                context.HeadLeft,
                context.HeadBottom,
                context.HeadWidth,
                context.HeadHeight,
                context.Skin);

            var shadowX = context.HeadRight - 1;
            context.Canvas.DrawVerticalLine(
                shadowX,
                context.HeadBottom + 2,
                context.HeadTop - 3,
                context.SkinShadow);
            context.Canvas.SetPixel(context.HeadLeft + 2, context.HeadTop - 2, context.SkinHighlight);
            context.Canvas.SetPixel(context.HeadLeft + 3, context.HeadTop - 2, context.SkinHighlight);
        }
    }
}

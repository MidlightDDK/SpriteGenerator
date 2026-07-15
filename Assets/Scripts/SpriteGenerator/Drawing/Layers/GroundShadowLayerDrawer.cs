using UnityEngine;

namespace SpriteGenerator
{
    internal static class GroundShadowLayerDrawer
    {
        internal static void Draw(CharacterDrawingContext context)
        {
            var softShadow = new Color32(13, 15, 22, 64);
            var coreShadow = new Color32(9, 11, 17, 74);
            context.Canvas.FillEllipse(7, 1, 18, 4, softShadow, true);
            context.Canvas.FillEllipse(10, 2, 12, 2, coreShadow, true);
        }
    }
}

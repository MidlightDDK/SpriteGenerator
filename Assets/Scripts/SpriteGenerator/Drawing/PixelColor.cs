using UnityEngine;

namespace SpriteGenerator
{
    internal static class PixelColor
    {
        internal static Color32 FromColor(Color color)
        {
            return color;
        }

        internal static Color32 Darken(Color32 color, float amount)
        {
            var factor = 1f - Mathf.Clamp01(amount);
            return new Color32(
                (byte)Mathf.RoundToInt(color.r * factor),
                (byte)Mathf.RoundToInt(color.g * factor),
                (byte)Mathf.RoundToInt(color.b * factor),
                color.a);
        }

        internal static Color32 Lighten(Color32 color, float amount)
        {
            var blend = Mathf.Clamp01(amount);
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(color.r, byte.MaxValue, blend)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(color.g, byte.MaxValue, blend)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(color.b, byte.MaxValue, blend)),
                color.a);
        }

        internal static Color32 WithAlpha(Color32 color, byte alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}

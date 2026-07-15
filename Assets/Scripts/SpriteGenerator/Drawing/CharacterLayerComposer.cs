using System;

namespace SpriteGenerator
{
    /// <summary>
    /// Composes each deterministic character layer on a 32-pixel reference grid, then
    /// maps that result to the requested canvas with aspect-preserving point sampling.
    /// </summary>
    internal static class CharacterLayerComposer
    {
        internal static void Draw(PixelCanvas canvas, CharacterSpriteSettings settings)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            canvas.Clear(PixelCanvas.Transparent);
            var logicalCanvas = canvas.Width == CharacterDrawingContext.LogicalSize &&
                canvas.Height == CharacterDrawingContext.LogicalSize
                    ? canvas
                    : new PixelCanvas(CharacterDrawingContext.LogicalSize, CharacterDrawingContext.LogicalSize);

            var context = new CharacterDrawingContext(logicalCanvas, settings);

            if (settings.DrawGroundShadow)
            {
                GroundShadowLayerDrawer.Draw(context);
            }

            // Back passes are deliberately ordered before the body silhouette.
            AccessoryLayerDrawer.DrawBack(context);
            HairLayerDrawer.DrawBack(context);
            BodyLayerDrawer.Draw(context);
            OutfitLayerDrawer.Draw(context);
            FaceLayerDrawer.Draw(context);
            HairLayerDrawer.DrawFront(context);
            AccessoryLayerDrawer.DrawFront(context);

            if (!ReferenceEquals(canvas, logicalCanvas))
            {
                ScalePointSampled(logicalCanvas, canvas);
            }
        }

        private static void ScalePointSampled(PixelCanvas source, PixelCanvas destination)
        {
            var destinationPixels = destination.Pixels;
            var sourcePixels = source.Pixels;
            var scale = Math.Min(
                destination.Width / (double)source.Width,
                destination.Height / (double)source.Height);
            var scaledWidth = Math.Max(1, Math.Min(
                destination.Width,
                (int)Math.Round(source.Width * scale, MidpointRounding.AwayFromZero)));
            var scaledHeight = Math.Max(1, Math.Min(
                destination.Height,
                (int)Math.Round(source.Height * scale, MidpointRounding.AwayFromZero)));
            var offsetX = (destination.Width - scaledWidth) / 2;
            var offsetY = (destination.Height - scaledHeight) / 2;

            for (var y = 0; y < scaledHeight; y++)
            {
                var sourceY = (y * source.Height) / scaledHeight;
                var sourceRow = sourceY * source.Width;
                var destinationRow = (y + offsetY) * destination.Width + offsetX;
                for (var x = 0; x < scaledWidth; x++)
                {
                    var sourceX = (x * source.Width) / scaledWidth;
                    destinationPixels[destinationRow + x] = sourcePixels[sourceRow + sourceX];
                }
            }
        }
    }
}

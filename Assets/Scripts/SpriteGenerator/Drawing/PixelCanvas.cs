using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteGenerator
{
    /// <summary>
    /// A small, CPU-side pixel buffer used while composing generated characters.
    /// Every drawing operation clips to the canvas, so layers can safely touch an edge.
    /// </summary>
    internal sealed class PixelCanvas
    {
        internal static readonly Color32 Transparent = new(0, 0, 0, 0);

        private readonly Color32[] pixels;

        internal PixelCanvas(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Canvas width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Canvas height must be positive.");
            }

            Width = width;
            Height = height;
            pixels = new Color32[checked(width * height)];

            // Do not rely on the runtime's default struct initialization. The generated
            // texture starts from an explicitly transparent Color32 buffer every time.
            Clear(Transparent);
        }

        internal int Width { get; }

        internal int Height { get; }

        internal Color32[] Pixels => pixels;

        internal bool IsInBounds(int x, int y)
        {
            return (uint)x < (uint)Width && (uint)y < (uint)Height;
        }

        internal void Clear(Color32 color)
        {
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = color;
            }
        }

        internal Color32 GetPixel(int x, int y)
        {
            return IsInBounds(x, y) ? pixels[(y * Width) + x] : Transparent;
        }

        internal void SetPixel(int x, int y, Color32 color)
        {
            if (IsInBounds(x, y))
            {
                pixels[(y * Width) + x] = color;
            }
        }

        internal void BlendPixel(int x, int y, Color32 source)
        {
            if (!IsInBounds(x, y) || source.a == 0)
            {
                return;
            }

            if (source.a == byte.MaxValue)
            {
                SetPixel(x, y, source);
                return;
            }

            var destination = pixels[(y * Width) + x];
            var sourceAlpha = source.a / 255f;
            var destinationAlpha = destination.a / 255f;
            var outputAlpha = sourceAlpha + (destinationAlpha * (1f - sourceAlpha));

            if (outputAlpha <= 0f)
            {
                pixels[(y * Width) + x] = Transparent;
                return;
            }

            var destinationFactor = destinationAlpha * (1f - sourceAlpha);
            pixels[(y * Width) + x] = new Color32(
                ToByte(((source.r * sourceAlpha) + (destination.r * destinationFactor)) / outputAlpha),
                ToByte(((source.g * sourceAlpha) + (destination.g * destinationFactor)) / outputAlpha),
                ToByte(((source.b * sourceAlpha) + (destination.b * destinationFactor)) / outputAlpha),
                ToByte(outputAlpha * 255f));
        }

        internal void FillRect(int x, int y, int width, int height, Color32 color)
        {
            if (!TryClipRect(x, y, width, height, out var left, out var bottom, out var right, out var top))
            {
                return;
            }

            for (var pixelY = bottom; pixelY < top; pixelY++)
            {
                var row = pixelY * Width;
                for (var pixelX = left; pixelX < right; pixelX++)
                {
                    pixels[row + pixelX] = color;
                }
            }
        }

        internal void BlendRect(int x, int y, int width, int height, Color32 color)
        {
            if (!TryClipRect(x, y, width, height, out var left, out var bottom, out var right, out var top))
            {
                return;
            }

            for (var pixelY = bottom; pixelY < top; pixelY++)
            {
                for (var pixelX = left; pixelX < right; pixelX++)
                {
                    BlendPixel(pixelX, pixelY, color);
                }
            }
        }

        internal void DrawHorizontalLine(int xStart, int xEnd, int y, Color32 color)
        {
            if ((uint)y >= (uint)Height)
            {
                return;
            }

            if (xStart > xEnd)
            {
                (xStart, xEnd) = (xEnd, xStart);
            }

            var left = Mathf.Max(0, xStart);
            var right = Mathf.Min(Width - 1, xEnd);
            if (left > right)
            {
                return;
            }

            var row = y * Width;
            for (var x = left; x <= right; x++)
            {
                pixels[row + x] = color;
            }
        }

        internal void DrawVerticalLine(int x, int yStart, int yEnd, Color32 color)
        {
            if ((uint)x >= (uint)Width)
            {
                return;
            }

            if (yStart > yEnd)
            {
                (yStart, yEnd) = (yEnd, yStart);
            }

            var bottom = Mathf.Max(0, yStart);
            var top = Mathf.Min(Height - 1, yEnd);
            for (var y = bottom; y <= top; y++)
            {
                pixels[(y * Width) + x] = color;
            }
        }

        internal void DrawRect(int x, int y, int width, int height, Color32 color, int thickness = 1)
        {
            if (width <= 0 || height <= 0 || thickness <= 0)
            {
                return;
            }

            var safeThickness = Mathf.Min(thickness, Mathf.CeilToInt(Mathf.Min(width, height) * 0.5f));
            FillRect(x, y, width, safeThickness, color);
            FillRect(x, (int)Math.Min(int.MaxValue, (long)y + height - safeThickness), width, safeThickness, color);
            FillRect(x, y, safeThickness, height, color);
            FillRect((int)Math.Min(int.MaxValue, (long)x + width - safeThickness), y, safeThickness, height, color);
        }

        internal void FillRoundedRect(int x, int y, int width, int height, Color32 color, int cornerSize = 1)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var radius = Mathf.Clamp(cornerSize, 0, Mathf.Min(width, height) / 2);
            if (radius == 0)
            {
                FillRect(x, y, width, height, color);
                return;
            }

            FillRect(x + radius, y, width - (radius * 2), height, color);
            FillRect(x, y + radius, radius, height - (radius * 2), color);
            FillRect(x + width - radius, y + radius, radius, height - (radius * 2), color);

            for (var offset = 0; offset < radius; offset++)
            {
                var inset = radius - offset;
                FillRect(x + inset, y + offset, width - (inset * 2), 1, color);
                FillRect(x + inset, y + height - 1 - offset, width - (inset * 2), 1, color);
            }
        }

        internal void FillEllipse(int x, int y, int width, int height, Color32 color, bool blend = false)
        {
            if (!TryClipRect(x, y, width, height, out var left, out var bottom, out var right, out var top))
            {
                return;
            }

            var radiusX = width * 0.5d;
            var radiusY = height * 0.5d;
            var centerX = x + ((width - 1) * 0.5d);
            var centerY = y + ((height - 1) * 0.5d);

            for (var pixelY = bottom; pixelY < top; pixelY++)
            {
                var normalizedY = (pixelY - centerY) / radiusY;
                for (var pixelX = left; pixelX < right; pixelX++)
                {
                    var normalizedX = (pixelX - centerX) / radiusX;
                    if ((normalizedX * normalizedX) + (normalizedY * normalizedY) > 1d)
                    {
                        continue;
                    }

                    if (blend)
                    {
                        BlendPixel(pixelX, pixelY, color);
                    }
                    else
                    {
                        SetPixel(pixelX, pixelY, color);
                    }
                }
            }
        }

        internal void DrawLine(int xStart, int yStart, int xEnd, int yEnd, Color32 color, int thickness = 1)
        {
            if (thickness <= 0)
            {
                return;
            }

            var deltaX = Math.Abs(xEnd - xStart);
            var stepX = xStart < xEnd ? 1 : -1;
            var deltaY = -Math.Abs(yEnd - yStart);
            var stepY = yStart < yEnd ? 1 : -1;
            var error = deltaX + deltaY;
            var brushOffset = -(thickness / 2);

            while (true)
            {
                FillRect(xStart + brushOffset, yStart + brushOffset, thickness, thickness, color);
                if (xStart == xEnd && yStart == yEnd)
                {
                    break;
                }

                var doubledError = error * 2;
                if (doubledError >= deltaY)
                {
                    error += deltaY;
                    xStart += stepX;
                }

                if (doubledError <= deltaX)
                {
                    error += deltaX;
                    yStart += stepY;
                }
            }
        }

        internal void FillPolygon(IReadOnlyList<Vector2Int> points, Color32 color)
        {
            if (points == null || points.Count < 3)
            {
                return;
            }

            var minX = points[0].x;
            var maxX = points[0].x;
            var minY = points[0].y;
            var maxY = points[0].y;
            for (var index = 1; index < points.Count; index++)
            {
                minX = Mathf.Min(minX, points[index].x);
                maxX = Mathf.Max(maxX, points[index].x);
                minY = Mathf.Min(minY, points[index].y);
                maxY = Mathf.Max(maxY, points[index].y);
            }

            minX = Mathf.Max(0, minX);
            maxX = Mathf.Min(Width - 1, maxX);
            minY = Mathf.Max(0, minY);
            maxY = Mathf.Min(Height - 1, maxY);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (IsPointInsidePolygon(x + 0.5d, y + 0.5d, points))
                    {
                        SetPixel(x, y, color);
                    }
                }
            }

            for (var index = 0; index < points.Count; index++)
            {
                var next = (index + 1) % points.Count;
                DrawLine(points[index].x, points[index].y, points[next].x, points[next].y, color);
            }
        }

        private static bool IsPointInsidePolygon(double x, double y, IReadOnlyList<Vector2Int> points)
        {
            var inside = false;
            for (int current = 0, previous = points.Count - 1; current < points.Count; previous = current++)
            {
                var currentPoint = points[current];
                var previousPoint = points[previous];
                var crossesScanline = (currentPoint.y > y) != (previousPoint.y > y);
                if (!crossesScanline)
                {
                    continue;
                }

                var intersection = ((previousPoint.x - currentPoint.x) * (y - currentPoint.y) /
                    (previousPoint.y - currentPoint.y)) + currentPoint.x;
                if (x < intersection)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private bool TryClipRect(
            int x,
            int y,
            int width,
            int height,
            out int left,
            out int bottom,
            out int right,
            out int top)
        {
            left = bottom = right = top = 0;
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            var longRight = (long)x + width;
            var longTop = (long)y + height;
            left = Mathf.Max(0, x);
            bottom = Mathf.Max(0, y);
            right = (int)Math.Min(Width, Math.Max(0L, longRight));
            top = (int)Math.Min(Height, Math.Max(0L, longTop));
            return left < right && bottom < top;
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(value), 0, byte.MaxValue);
        }
    }
}

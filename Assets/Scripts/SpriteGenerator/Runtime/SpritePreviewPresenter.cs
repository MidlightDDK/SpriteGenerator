using System;
using UnityEngine;

namespace SpriteGenerator
{
    public sealed class SpritePreviewPresenter : IDisposable
    {
        public const string PreviewObjectName = "GeneratedCharacterPreview";

        private readonly Transform previewTransform;
        private readonly SpriteRenderer spriteRenderer;
        private readonly float maximumWorldSize;
        private GeneratedSpriteResource currentResource;

        public SpritePreviewPresenter(
            Transform owner,
            Vector3 localPosition,
            int sortingOrder,
            float maximumWorldSize = 4f)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            previewTransform = owner.Find(PreviewObjectName);
            if (previewTransform == null)
            {
                var previewObject = new GameObject(PreviewObjectName);
                previewTransform = previewObject.transform;
                previewTransform.SetParent(owner, false);
            }

            previewTransform.localPosition = localPosition;
            previewTransform.localRotation = Quaternion.identity;
            previewTransform.localScale = Vector3.one;
            this.maximumWorldSize = IsPositiveFinite(maximumWorldSize) ? maximumWorldSize : 4f;

            spriteRenderer = previewTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = previewTransform.gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = false;
        }

        public SpriteRenderer Renderer => spriteRenderer;

        public Texture2D CurrentTexture => currentResource?.Texture;

        public Sprite CurrentSprite => currentResource?.Sprite;

        public void Present(CharacterPixelData pixelData)
        {
            GeneratedSpriteResource nextResource = GeneratedSpriteResource.Create(pixelData);
            GeneratedSpriteResource previousResource = currentResource;

            currentResource = nextResource;
            spriteRenderer.sprite = nextResource.Sprite;
            spriteRenderer.enabled = true;
            FitLargePreviewInsideMaximumSize();
            previousResource?.Dispose();
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            previewTransform.localPosition = localPosition;
        }

        public void Dispose()
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
            currentResource?.Dispose();
            currentResource = null;
        }

        private void FitLargePreviewInsideMaximumSize()
        {
            previewTransform.localScale = Vector3.one;
            Vector3 worldSize = spriteRenderer.bounds.size;
            float largestDimension = Mathf.Max(worldSize.x, worldSize.y);
            if (largestDimension > maximumWorldSize)
            {
                float scale = maximumWorldSize / largestDimension;
                previewTransform.localScale = Vector3.one * scale;
            }
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

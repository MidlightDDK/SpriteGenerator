using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SpriteGenerator.Tests.PlayMode
{
    public sealed class SampleScenePlayModeTests
    {
        [UnityTest]
        public IEnumerator SampleScene_PlayAutomaticallyShowsGenerated32By32SpriteInsideCameraViewport()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Assets/Scenes/SampleScene.unity", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, "SampleScene could not be loaded from build settings.");

            yield return load;
            yield return null;

            GameObject manager = GameObject.Find("GameManager");
            Assert.That(manager, Is.Not.Null, "The scene must contain an active GameManager object.");

            Transform preview = manager.transform.Find(SpritePreviewPresenter.PreviewObjectName);
            Assert.That(preview, Is.Not.Null,
                $"Play startup did not create the {SpritePreviewPresenter.PreviewObjectName} child.");

            SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.enabled, Is.True);
            Assert.That(renderer.gameObject.activeInHierarchy, Is.True);
            Assert.That(renderer.sprite, Is.Not.Null);
            Assert.That(renderer.sprite.texture, Is.Not.Null);
            Assert.That(renderer.sprite.texture.width, Is.EqualTo(32));
            Assert.That(renderer.sprite.texture.height, Is.EqualTo(32));
            Assert.That(renderer.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(renderer.sprite.texture.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));

            Color32[] pixels = renderer.sprite.texture.GetPixels32();
            bool hasVisiblePixel = false;
            for (int index = 0; index < pixels.Length; index++)
            {
                if (pixels[index].a > 0)
                {
                    hasVisiblePixel = true;
                    break;
                }
            }

            Assert.That(hasVisiblePixel, Is.True, "The automatically generated texture is fully transparent.");

            Camera mainCamera = Camera.main;
            Assert.That(mainCamera, Is.Not.Null, "SampleScene must provide a MainCamera-tagged camera.");
            AssertBoundsInsideViewport(renderer.bounds, mainCamera);
        }

        private static void AssertBoundsInsideViewport(Bounds bounds, Camera camera)
        {
            var worldCorners = new[]
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.center.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.center.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.center.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.center.z)
            };

            for (int index = 0; index < worldCorners.Length; index++)
            {
                Vector3 viewport = camera.WorldToViewportPoint(worldCorners[index]);
                Assert.That(viewport.z, Is.GreaterThan(0f), $"Sprite corner {index} is behind the main camera.");
                Assert.That(viewport.x, Is.InRange(0f, 1f), $"Sprite corner {index} is horizontally outside the viewport.");
                Assert.That(viewport.y, Is.InRange(0f, 1f), $"Sprite corner {index} is vertically outside the viewport.");
            }
        }
    }
}

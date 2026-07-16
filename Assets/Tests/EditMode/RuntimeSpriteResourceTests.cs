using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SpriteGenerator.Tests.EditMode
{
    public sealed class RuntimeSpriteResourceTests
    {
        private GameObject owner;

        [TearDown]
        public void TearDown()
        {
            if (owner != null)
            {
                UnityEngine.Object.DestroyImmediate(owner);
                owner = null;
            }
        }

        [Test]
        public void Create_BuildsPointFilteredClampedFullSizeRuntimeSprite()
        {
            var settings = new CharacterSpriteSettings
            {
                Width = 31,
                Height = 47,
                PixelsPerUnit = 13f,
                Pivot = new Vector2(0.25f, 0.75f)
            };
            CharacterPixelData data = new CharacterPixelGenerator().Generate(settings);
            GeneratedSpriteResource resource = null;

            try
            {
                resource = GeneratedSpriteResource.Create(data);

                Assert.That(resource.Texture, Is.Not.Null);
                Assert.That(resource.Texture.width, Is.EqualTo(31));
                Assert.That(resource.Texture.height, Is.EqualTo(47));
                Assert.That(resource.Texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(resource.Texture.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
                Assert.That(resource.Texture.mipmapCount, Is.EqualTo(1));
                Assert.That(resource.Texture.anisoLevel, Is.Zero);

                Assert.That(resource.Sprite, Is.Not.Null);
                Assert.That(resource.Sprite.texture, Is.SameAs(resource.Texture));
                Assert.That(resource.Sprite.rect, Is.EqualTo(new Rect(0f, 0f, 31f, 47f)));
                Assert.That(resource.Sprite.pixelsPerUnit, Is.EqualTo(13f));
                Assert.That(resource.Sprite.pivot, Is.EqualTo(new Vector2(31f * 0.25f, 47f * 0.75f)));
                Assert.That(resource.Sprite.vertices, Has.Length.EqualTo(4), "FullRect sprites should use four corner vertices.");
                Assert.That(resource.Sprite.triangles, Has.Length.EqualTo(6), "FullRect sprites should use two triangles.");
                Assert.That(resource.Sprite.bounds.size.x, Is.EqualTo(31f / 13f).Within(0.0001f));
                Assert.That(resource.Sprite.bounds.size.y, Is.EqualTo(47f / 13f).Within(0.0001f));
            }
            finally
            {
                resource?.Dispose();
            }
        }

        [Test]
        public void Dispose_ReleasesSpriteAndTextureAndIsIdempotent()
        {
            CharacterPixelData data = new CharacterPixelGenerator().Generate(new CharacterSpriteSettings());
            GeneratedSpriteResource resource = GeneratedSpriteResource.Create(data);
            Texture2D texture = resource.Texture;
            Sprite sprite = resource.Sprite;

            Assert.DoesNotThrow(resource.Dispose);
            Assert.DoesNotThrow(resource.Dispose);

            Assert.That(resource.Texture, Is.Null);
            Assert.That(resource.Sprite, Is.Null);
            Assert.That(texture == null, Is.True, "The generated Texture2D was not destroyed.");
            Assert.That(sprite == null, Is.True, "The generated Sprite was not destroyed.");
        }

        [Test]
        public void Presenter_RegenerationReplacesAndDestroysPreviousRuntimeResource()
        {
            owner = new GameObject("PreviewOwner");
            var presenter = new SpritePreviewPresenter(owner.transform, new Vector3(1f, 2f, 0f), 17);
            var firstSettings = new CharacterSpriteSettings();
            var secondSettings = new CharacterSpriteSettings
            {
                Width = 64,
                Height = 48
            };

            try
            {
                presenter.Present(new CharacterPixelGenerator().Generate(firstSettings));
                Texture2D previousTexture = presenter.CurrentTexture;
                Sprite previousSprite = presenter.CurrentSprite;

                Assert.That(presenter.Renderer.enabled, Is.True);
                Assert.That(presenter.Renderer.sortingOrder, Is.EqualTo(17));
                Assert.That(presenter.Renderer.transform.localPosition, Is.EqualTo(new Vector3(1f, 2f, 0f)));

                presenter.Present(new CharacterPixelGenerator().Generate(secondSettings));

                Assert.That(presenter.CurrentTexture, Is.Not.Null);
                Assert.That(presenter.CurrentTexture.width, Is.EqualTo(64));
                Assert.That(presenter.CurrentTexture.height, Is.EqualTo(48));
                Assert.That(presenter.CurrentSprite, Is.SameAs(presenter.Renderer.sprite));
                Assert.That(
                    Mathf.Max(presenter.Renderer.bounds.size.x, presenter.Renderer.bounds.size.y),
                    Is.EqualTo(4f).Within(0.0001f),
                    "Large sprites should be scaled only on the preview transform so they remain visible.");
                Assert.That(previousTexture == null, Is.True, "Regeneration leaked the previous Texture2D.");
                Assert.That(previousSprite == null, Is.True, "Regeneration leaked the previous Sprite.");
            }
            finally
            {
                presenter.Dispose();
            }

            Assert.That(presenter.CurrentTexture, Is.Null);
            Assert.That(presenter.CurrentSprite, Is.Null);
            Assert.That(presenter.Renderer.sprite, Is.Null);
            Assert.That(presenter.Renderer.enabled, Is.False);
        }

        [Test]
        public void Presenter_ReusesNamedPreviewChildInsteadOfDuplicatingIt()
        {
            owner = new GameObject("PreviewOwner");
            var existingPreview = new GameObject(SpritePreviewPresenter.PreviewObjectName);
            existingPreview.transform.SetParent(owner.transform, false);
            SpritePreviewPresenter presenter = null;

            try
            {
                presenter = new SpritePreviewPresenter(owner.transform, Vector3.zero, 0);

                Assert.That(owner.transform.childCount, Is.EqualTo(1));
                Assert.That(presenter.Renderer.gameObject, Is.SameAs(existingPreview));
            }
            finally
            {
                presenter?.Dispose();
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void PresentationSettings_EmptyExportDirectory_UsesCurrentFolder(string configuredDirectory)
        {
            var presentation = new RuntimePresentationSettings
            {
                ExportDirectory = configuredDirectory
            };

            Assert.That(
                presentation.ResolveExportDirectory(),
                Is.EqualTo(Path.GetFullPath(Directory.GetCurrentDirectory())));
        }

        [Test]
        public void PresentationSettings_PastedQuotedDirectory_IsTrimmed()
        {
            string currentDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
            var presentation = new RuntimePresentationSettings
            {
                ExportDirectory = $"  \"{currentDirectory}\"  "
            };

            Assert.That(presentation.ResolveExportDirectory(), Is.EqualTo(currentDirectory));
        }

        [Test]
        public void Controller_DefaultExportName_UsesConfiguredPrefixAndSeed()
        {
            owner = new GameObject("ControllerOwner");
            string directory = Path.Combine(
                Path.GetTempPath(),
                "SpriteGenerator_ControllerExport_" + Guid.NewGuid().ToString("N"));
            string expectedPath = Path.Combine(directory, "custom_2468.png");
            Directory.CreateDirectory(directory);

            var settings = new CharacterSpriteSettings
            {
                Seed = 2468
            };
            var presentation = new RuntimePresentationSettings
            {
                ExportFilePrefix = "custom"
            };
            var controller = new CharacterGeneratorController(
                settings,
                new CharacterLayerLocks(),
                presentation,
                owner.transform);
            controller.ExportDirectory = directory;

            try
            {
                Assert.That(controller.ExportDirectory, Is.EqualTo(directory));
                Assert.That(controller.Generate().Succeeded, Is.True);
                settings.Seed = 9999;
                PngExportResult export = controller.Export();

                Assert.That(export.Succeeded, Is.True, export.Message);
                Assert.That(export.Path, Is.EqualTo(expectedPath));
                Assert.That(File.Exists(expectedPath), Is.True);
            }
            finally
            {
                controller.Dispose();
                if (File.Exists(expectedPath))
                {
                    File.Delete(expectedPath);
                }

                if (Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}

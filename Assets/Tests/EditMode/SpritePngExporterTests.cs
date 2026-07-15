using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SpriteGenerator.Tests.EditMode
{
    public sealed class SpritePngExporterTests
    {
        private const string TemporaryDirectoryPrefix = "SpriteGenerator_EditModeTests_";
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private readonly List<string> createdFiles = new List<string>();
        private string temporaryDirectory;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                TemporaryDirectoryPrefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            string expectedDirectory = Path.GetFullPath(temporaryDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (string createdFile in createdFiles)
            {
                string fullFile = Path.GetFullPath(createdFile);
                string parent = Path.GetDirectoryName(fullFile)?.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);

                if (string.Equals(parent, expectedDirectory, StringComparison.OrdinalIgnoreCase) && File.Exists(fullFile))
                {
                    File.Delete(fullFile);
                }
            }

            createdFiles.Clear();

            string directoryName = Path.GetFileName(expectedDirectory);
            bool isOwnedDirectory = directoryName.StartsWith(TemporaryDirectoryPrefix, StringComparison.Ordinal) &&
                                    string.Equals(
                                        Path.GetDirectoryName(expectedDirectory)?.TrimEnd(
                                            Path.DirectorySeparatorChar,
                                            Path.AltDirectorySeparatorChar),
                                        Path.GetFullPath(Path.GetTempPath()).TrimEnd(
                                            Path.DirectorySeparatorChar,
                                            Path.AltDirectorySeparatorChar),
                                        StringComparison.OrdinalIgnoreCase);

            if (isOwnedDirectory && Directory.Exists(expectedDirectory) &&
                Directory.GetFileSystemEntries(expectedDirectory).Length == 0)
            {
                Directory.Delete(expectedDirectory, false);
            }
        }

        [Test]
        public void Export_WritesPngWithValidSignatureAndSanitizedLeafName()
        {
            CharacterPixelData data = new CharacterPixelGenerator().Generate(new CharacterSpriteSettings());
            GeneratedSpriteResource resource = GeneratedSpriteResource.Create(data);

            try
            {
                PngExportResult result = SpritePngExporter.Export(
                    resource.Texture,
                    temporaryDirectory,
                    "nested/hero\u0001.PNG");
                Track(result);

                Assert.That(result.Succeeded, Is.True, result.Message);
                Assert.That(File.Exists(result.Path), Is.True);
                Assert.That(Path.GetDirectoryName(Path.GetFullPath(result.Path)),
                    Is.EqualTo(Path.GetFullPath(temporaryDirectory)));
                Assert.That(Path.GetFileName(result.Path), Is.EqualTo("hero_.png"));

                byte[] bytes = File.ReadAllBytes(result.Path);
                Assert.That(bytes.Length, Is.GreaterThan(PngSignature.Length));
                for (int index = 0; index < PngSignature.Length; index++)
                {
                    Assert.That(bytes[index], Is.EqualTo(PngSignature[index]), $"PNG signature byte {index} differed.");
                }
            }
            finally
            {
                resource.Dispose();
            }
        }

        [Test]
        public void SanitizeFileName_UsesSafeDefaultForNullBlankAndDots()
        {
            Assert.That(SpritePngExporter.SanitizeFileName(null), Is.EqualTo("generated_character.png"));
            Assert.That(SpritePngExporter.SanitizeFileName("   "), Is.EqualTo("generated_character.png"));
            Assert.That(SpritePngExporter.SanitizeFileName("..."), Is.EqualTo("generated_character.png"));
        }

        [Test]
        public void Export_NullTexture_ReturnsActionableFailureWithoutCreatingAFile()
        {
            PngExportResult result = SpritePngExporter.Export(null, temporaryDirectory, "character");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Path, Is.Empty);
            StringAssert.Contains("no generated texture", result.Message.ToLowerInvariant());
            Assert.That(Directory.GetFiles(temporaryDirectory), Is.Empty);
        }

        [Test]
        public void Export_EmptyDirectory_ReturnsActionableFailure()
        {
            Texture2D texture = CreateReadableTexture();

            try
            {
                PngExportResult result = SpritePngExporter.Export(texture, "   ", "character");

                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Path, Is.Empty);
                StringAssert.Contains("directory is empty", result.Message.ToLowerInvariant());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void Export_InvalidDirectory_ReturnsFailureInsteadOfThrowing()
        {
            Texture2D texture = CreateReadableTexture();
            PngExportResult result = default;

            try
            {
                Assert.DoesNotThrow(() => result = SpritePngExporter.Export(texture, "\0", "character"));
                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Path, Is.Empty);
                StringAssert.Contains("export failed", result.Message.ToLowerInvariant());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private void Track(PngExportResult result)
        {
            if (!string.IsNullOrEmpty(result.Path))
            {
                createdFiles.Add(result.Path);
            }
        }

        private static Texture2D CreateReadableTexture()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels32(new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255),
                new Color32(0, 0, 0, 0)
            });
            texture.Apply();
            return texture;
        }
    }
}

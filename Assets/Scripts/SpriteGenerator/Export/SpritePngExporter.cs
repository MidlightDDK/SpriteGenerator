using System;
using System.IO;
using System.Linq;
using System.Security;
using UnityEngine;

namespace SpriteGenerator
{
    public readonly struct PngExportResult
    {
        private PngExportResult(bool succeeded, string path, string message)
        {
            Succeeded = succeeded;
            Path = path;
            Message = message;
        }

        public bool Succeeded { get; }

        public string Path { get; }

        public string Message { get; }

        public static PngExportResult Success(string path)
        {
            return new PngExportResult(true, path, $"Saved PNG to {path}");
        }

        public static PngExportResult Failure(string message)
        {
            return new PngExportResult(false, string.Empty, message);
        }
    }

    public static class SpritePngExporter
    {
        public static PngExportResult Export(Texture2D texture, string directory, string requestedFileName)
        {
            if (texture == null)
            {
                return PngExportResult.Failure("There is no generated texture to export.");
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                return PngExportResult.Failure("The export directory is empty.");
            }

            try
            {
                string fullDirectory = Path.GetFullPath(directory.Trim());
                Directory.CreateDirectory(fullDirectory);

                string fileName = SanitizeFileName(requestedFileName);
                string fullPath = Path.Combine(fullDirectory, fileName);
                byte[] pngBytes = texture.EncodeToPNG();

                if (pngBytes == null || pngBytes.Length == 0)
                {
                    return PngExportResult.Failure("Unity could not encode the generated texture as PNG.");
                }

                File.WriteAllBytes(fullPath, pngBytes);
                return PngExportResult.Success(fullPath);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is NotSupportedException ||
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is SecurityException)
            {
                return PngExportResult.Failure($"PNG export failed: {exception.Message}");
            }
        }

        internal static string SanitizeFileName(string requestedFileName)
        {
            string name = string.IsNullOrWhiteSpace(requestedFileName)
                ? "generated_character"
                : requestedFileName.Trim();

            // Treat the input as a display name, never as a path. Strip either style of
            // separator manually so malformed characters cannot reach System.IO first.
            int lastSeparator = Math.Max(name.LastIndexOf('/'), name.LastIndexOf('\\'));
            if (lastSeparator >= 0)
            {
                name = name.Substring(lastSeparator + 1);
            }

            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            name = new string(name
                .Select(character => invalidCharacters.Contains(character) ||
                                     char.IsControl(character) ||
                                     "<>:\"/\\|?*".Contains(character)
                    ? '_'
                    : character)
                .ToArray())
                .Trim(' ', '.');

            int extensionStart = name.LastIndexOf('.');
            if (extensionStart > 0)
            {
                name = name.Substring(0, extensionStart).Trim(' ', '.');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "generated_character";
            }

            return $"{name}.png";
        }
    }
}

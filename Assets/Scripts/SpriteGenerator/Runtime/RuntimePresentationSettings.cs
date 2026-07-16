using System;
using System.IO;
using UnityEngine;

namespace SpriteGenerator
{
    [Serializable]
    public sealed class RuntimePresentationSettings
    {
        [SerializeField]
        [Tooltip("Generate a controlled random appearance as soon as Play starts.")]
        private bool randomizeOnPlay = true;

        [SerializeField]
        [Tooltip("Use a time-varying seed on Play instead of the serialized seed.")]
        private bool uniqueSeedOnPlay;

        [SerializeField]
        [Tooltip("Show a zero-setup control panel while the game is running.")]
        private bool showRuntimeControls = true;

        [SerializeField]
        private Vector3 previewLocalPosition = new(1.25f, 0f, 0f);

        [SerializeField, Min(0.25f)]
        [Tooltip("Largest world-space dimension used by the scene preview. The generated Sprite and PNG keep their exact size.")]
        private float previewMaximumWorldSize = 4f;

        [SerializeField]
        private int sortingOrder = 10;

        [SerializeField]
        [Tooltip("Leave empty to export to the process's current working folder.")]
        private string exportDirectory = string.Empty;

        [SerializeField]
        private string exportFilePrefix = "character";

        public bool RandomizeOnPlay
        {
            get => randomizeOnPlay;
            set => randomizeOnPlay = value;
        }

        public bool UniqueSeedOnPlay
        {
            get => uniqueSeedOnPlay;
            set => uniqueSeedOnPlay = value;
        }

        public bool ShowRuntimeControls
        {
            get => showRuntimeControls;
            set => showRuntimeControls = value;
        }

        public Vector3 PreviewLocalPosition
        {
            get => previewLocalPosition;
            set => previewLocalPosition = value;
        }

        public float PreviewMaximumWorldSize
        {
            get => previewMaximumWorldSize;
            set => previewMaximumWorldSize = value;
        }

        public int SortingOrder
        {
            get => sortingOrder;
            set => sortingOrder = value;
        }

        public string ExportDirectory
        {
            get => exportDirectory;
            set => exportDirectory = value;
        }

        public string ExportFilePrefix
        {
            get => exportFilePrefix;
            set => exportFilePrefix = value;
        }

        public string ResolveExportDirectory()
        {
            return string.IsNullOrWhiteSpace(exportDirectory)
                ? ResolveDefaultExportDirectory()
                : NormalizeDirectoryInput(exportDirectory);
        }

        public static string ResolveDefaultExportDirectory()
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                if (!string.IsNullOrWhiteSpace(currentDirectory))
                {
                    return Path.GetFullPath(currentDirectory);
                }
            }
            catch (Exception)
            {
                // Fall through to Unity-owned paths when the host does not expose a working directory.
            }

            try
            {
                string applicationDirectory = Directory.GetParent(Application.dataPath)?.FullName;
                if (!string.IsNullOrWhiteSpace(applicationDirectory))
                {
                    return applicationDirectory;
                }
            }
            catch (Exception)
            {
                // Application.persistentDataPath is the final platform-safe fallback.
            }

            return Application.persistentDataPath;
        }

        private static string NormalizeDirectoryInput(string value)
        {
            string normalized = value.Trim();
            if (normalized.Length >= 2 &&
                ((normalized[0] == '"' && normalized[normalized.Length - 1] == '"') ||
                 (normalized[0] == '\'' && normalized[normalized.Length - 1] == '\'')))
            {
                normalized = normalized.Substring(1, normalized.Length - 2).Trim();
            }

            return string.IsNullOrWhiteSpace(normalized)
                ? ResolveDefaultExportDirectory()
                : normalized;
        }
    }
}

using System;
using SpriteGenerator;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    [Header("Runtime Sprite")]
    [SerializeField] private CharacterSpriteSettings spriteSettings = new();

    [Header("Randomization Locks")]
    [SerializeField] private CharacterLayerLocks layerLocks = new();

    [Header("Preview and Export")]
    [SerializeField] private RuntimePresentationSettings presentation = new();

    private CharacterGeneratorController controller;
    private CharacterGeneratorRuntimePanel runtimePanel;

    public CharacterGeneratorController Controller => controller;

    private void Awake()
    {
        spriteSettings ??= new CharacterSpriteSettings();
        layerLocks ??= new CharacterLayerLocks();
        presentation ??= new RuntimePresentationSettings();

        if (presentation.RandomizeOnPlay)
        {
            int seed = presentation.UniqueSeedOnPlay
                ? unchecked(Environment.TickCount ^ (int)DateTime.UtcNow.Ticks)
                : spriteSettings.Seed;
            CharacterRandomizer.RandomizeUnlocked(spriteSettings, layerLocks, seed);
        }

        controller = new CharacterGeneratorController(spriteSettings, layerLocks, presentation, transform);
        Report(controller.Generate());

        if (presentation.ShowRuntimeControls)
        {
            var panelObject = new GameObject("CharacterGeneratorControls");
            panelObject.transform.SetParent(transform, false);
            runtimePanel = panelObject.AddComponent<CharacterGeneratorRuntimePanel>();
            runtimePanel.Initialize(controller);
        }
    }

    private void OnDestroy()
    {
        controller?.Dispose();
        controller = null;
    }

    private static void Report(GenerationOperationResult result)
    {
        if (!result.Succeeded)
        {
            Debug.LogError(result.Message);
            return;
        }

        Debug.Log(result.Message);
        foreach (string warning in result.Warnings)
        {
            Debug.LogWarning(warning);
        }
    }
}

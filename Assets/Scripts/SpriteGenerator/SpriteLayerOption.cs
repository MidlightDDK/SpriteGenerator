using System;
using UnityEngine;

[Serializable]
public class SpriteLayerOption
{
    [SerializeField] private string id;
    [SerializeField] private SpriteLayerCategory category;
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool enabledForRandomization = true;

    public string Id => id;
    public SpriteLayerCategory Category => category;
    public Sprite Sprite => sprite;
    public bool EnabledForRandomization => enabledForRandomization;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(id) && sprite != null;
}

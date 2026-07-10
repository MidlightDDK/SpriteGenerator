using System;
using UnityEngine;

[Serializable]
public class LayerLockState
{
    [SerializeField] private SpriteLayerCategory category;
    [SerializeField] private bool locked;
    [SerializeField] private string lockedOptionId;

    public LayerLockState()
    {
    }

    public LayerLockState(SpriteLayerCategory category, bool locked = false, string lockedOptionId = "")
    {
        this.category = category;
        this.locked = locked;
        this.lockedOptionId = lockedOptionId;
    }

    public SpriteLayerCategory Category => category;
    public bool Locked => locked;
    public string LockedOptionId => lockedOptionId;
}

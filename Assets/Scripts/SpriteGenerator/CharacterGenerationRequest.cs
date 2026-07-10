using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharacterGenerationRequest
{
    public static CharacterGenerationRequest CreateDefault()
    {
        var request = new CharacterGenerationRequest();

        foreach (SpriteLayerCategory category in Enum.GetValues(typeof(SpriteLayerCategory)))
        {
            request.layerLocks.Add(new LayerLockState(category));
        }

        return request;
    }

    [SerializeField] private List<LayerLockState> layerLocks = new();

    public IReadOnlyList<LayerLockState> LayerLocks => layerLocks;

    public bool TryGetLockState(SpriteLayerCategory category, out LayerLockState lockState)
    {
        lockState = layerLocks.FirstOrDefault(state => state.Category == category);
        return lockState != null;
    }
}

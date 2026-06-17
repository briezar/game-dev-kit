using UnityEngine;

public static class LayerMaskExtension
{
    /// <summary> Check if a layer (GameObject.layer) is included in the LayerMask </summary>
    public static bool Contains(this LayerMask layerMask, int layer) => (layerMask & (1 << layer)) != 0;
}
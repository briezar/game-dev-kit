using UnityEngine;

public static class LayerMaskExtension
{
    /// <summary> Check if a layer is included in the LayerMask </summary>
    public static bool Contains(this LayerMask layerMask, int layer) => (layerMask.value & (1 << layer)) != 0;
}
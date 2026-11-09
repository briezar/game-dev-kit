using GameDevKit.Pool;
using UnityEngine;

public static class CameraUtils
{
    public const string MainCameraTag = "MainCamera";

    public static Camera FindCameraByTag(string tag)
    {
        using var _ = ArrayPool<Camera>.Get(Camera.allCamerasCount, out var cameras);
        var count = Camera.GetAllCameras(cameras);
        for (int i = 0; i < count; i++)
        {
            if (cameras[i].CompareTag(tag)) { return cameras[i]; }
        }
        return null;
    }
}
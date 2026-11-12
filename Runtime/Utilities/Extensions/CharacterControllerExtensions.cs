using UnityEngine;

public static class CharacterControllerExtensions
{
    public static void Warp(this CharacterController controller, Vector3 position)
    {
        controller.enabled = false;
        controller.transform.position = position;
        controller.enabled = true;
    }
}
using System;
using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class InputExtensions
{
#if ENABLE_INPUT_SYSTEM
    public static void Enable(this InputAction inputAction, bool enable)
    {
        if (enable)
        {
            inputAction.Enable();
        }
        else
        {
            inputAction.Disable();
        }
    }

    public static void Enable(this InputActionAsset inputActionAsset, bool enable)
    {
        if (enable)
        {
            inputActionAsset.Enable();
        }
        else
        {
            inputActionAsset.Disable();
        }
    }
#endif
}
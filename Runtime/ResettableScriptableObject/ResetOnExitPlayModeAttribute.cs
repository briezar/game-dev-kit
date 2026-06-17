using System;
using System.Collections.Generic;

namespace GameDevKit
{
    /// <summary>
    /// Implement this attribute on a ScriptableObject to save its data when entering play mode, and revert upon exiting play mode.
    /// This is useful for ScriptableObjects that are used as data containers and need to retain their design-time values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ResetOnExitPlayModeAttribute : Attribute { }
}
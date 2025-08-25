using System;

namespace GameDevKit.Attributes
{
    /// <summary>
    /// Indicates that a method is being used by UnityEvent (e.g. Button onClick).
    /// Such methods should not be removed or renamed, as this would break the UnityEvent references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UsedByUnityEventAttribute : Attribute
    {

    }
}
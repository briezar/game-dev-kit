using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(ObservableInt), true)]
    [CustomPropertyDrawer(typeof(ObservableFloat), true)]
    public class ObservableNumberDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => "_value";
    }

}
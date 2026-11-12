using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(ObservableInt))]
    public class ObservableIntDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => ObservableInt.EditorProps.ValueProp;
    }

    [CustomPropertyDrawer(typeof(ObservableFloat))]
    public class ObservableFloatDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => ObservableFloat.EditorProps.ValueProp;
    }
}
using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(ObservableNumber<>), true)]
    public class ObservableNumberDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => ObservableNumber<int>.EditorProps.ValueProp;
    }

}
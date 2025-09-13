using UnityEditor;
using GameDevKit.ObjectReferences;

namespace GameDevKit.Editor.ObjectReferences
{
    [CustomPropertyDrawer(typeof(FolderReference), true)]
    public class FolderReferenceDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_folderAsset";
    }

}
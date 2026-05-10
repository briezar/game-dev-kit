using UnityEditor;
using GameDevKit.ObjectReferences;

namespace GameDevKit.Editor.ObjectReferences
{
    [CustomPropertyDrawer(typeof(FolderReference), true)]
    public class FolderReferenceDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => "_folderAsset";
    }

}
using System.Collections;
using System.Collections.Generic;
using GameDevKit.Attributes;
using UnityEngine;

namespace GameDevKit
{
    [CreateAssetMenu(menuName = EditorConstants.MenuPath + "ObjectConverter")]
    public class ObjectConverterSO : ScriptableObject, IObjectConverter
    {
        [SerializeReference, SubclassPicker] private IObjectConverter _objectConverter;

        public object Convert(object data) => _objectConverter.Convert(data);
    }
}
using System.Collections;
using System.Collections.Generic;
using GameDevKit.Attributes;
using UnityEngine;

namespace GameDevKit
{
    [CreateAssetMenu(menuName = "GameDevKit/ObjectConverter")]
    public class ObjectConverterSO : ScriptableObject, IObjectConverter
    {
        [SerializeReference, SubclassPicker] private IObjectConverter _objectConverter;

        public object Convert(object data) => _objectConverter.Convert(data);
    }
}
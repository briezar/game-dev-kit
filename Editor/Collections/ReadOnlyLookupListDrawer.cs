using System;
using System.Collections;
using System.Collections.Generic;
using GameDevKit.Collections;
using GameDevKit.Editor.ObjectReferences;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Collections
{
    [CustomPropertyDrawer(typeof(ReadOnlyLookupList<,>), true)]
    public class ReadOnlyLookupListDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_values";
    }
}
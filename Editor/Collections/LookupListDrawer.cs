using System;
using System.Collections;
using System.Collections.Generic;
using GameDevKit.Collections;
using GameDevKit.Editor.ObjectReferences;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Collections
{
    [CustomPropertyDrawer(typeof(LookupList<,>), true)]
    public class LookupListDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_values";
    }
}
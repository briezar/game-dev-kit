using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public class DebugLogger : MonoBehaviour
    {
        public void Log(string msg) => Debug.Log(msg, this);
        public void LogWarning(string msg) => Debug.LogWarning(msg, this);
        public void LogError(string msg) => Debug.LogError(msg, this);
    }
}
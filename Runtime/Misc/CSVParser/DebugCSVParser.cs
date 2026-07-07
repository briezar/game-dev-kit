using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Parser
{
    public abstract class DebugCSVParser<T>
    {
#if DEV_BUILD
        private readonly HashSet<string> _errors = new();
        private readonly HashSet<string> _warnings = new();
        private readonly HashSet<string> _infos = new();
#endif

        protected List<List<string>> ParseArray2D(string text)
        {
            var parsed = CSVParser.LoadFromString(text);
            return parsed;
        }

        public abstract T Parse(string text);

        protected void ClearLogs()
        {
#if DEV_BUILD
            _errors.Clear();
            _warnings.Clear();
            _infos.Clear();
#endif
        }

        protected void PrintLogs()
        {
#if DEV_BUILD
            var typeName = GetType().Name;
            if (!_errors.IsNullOrEmpty())
            {
                Debug.LogError($"[{typeName}] Errors: {_errors.Count}\n{_errors.JoinToString("\n")}");
            }
            if (!_warnings.IsNullOrEmpty())
            {
                Debug.LogWarning($"[{typeName}] Warnings: {_warnings.Count}\n{_warnings.JoinToString("\n")}");
            }
            if (!_infos.IsNullOrEmpty())
            {
                Debug.Log($"[{typeName}] Infos: {_infos.Count}\n{_infos.JoinToString("\n")}");
            }
#endif
        }

        protected void AddLog(LogType logType, string message)
        {
#if DEV_BUILD
            switch (logType)
            {
                case LogType.Log:
                case LogType.Assert:
                    _infos.Add(message);
                    break;
                case LogType.Warning:
                    _warnings.Add(message);
                    break;
                case LogType.Error:
                case LogType.Exception:
                    _errors.Add(message);
                    break;
            }
#endif
        }
    }
}

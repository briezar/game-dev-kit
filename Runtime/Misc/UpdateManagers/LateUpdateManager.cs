using System;
using System.Collections.Generic;
using GameDevKit.Collections;
using UnityEngine;

namespace GameDevKit.UpdateManagers
{
    /// <summary>
    /// Interface for objects that need to be updated during the LateUpdate phase.
    /// </summary>
    public interface ILateUpdatable { void OnLateUpdate(float deltaTime); }

    public static class LateUpdateManagerExtensions
    {
        public static void RegisterLateUpdatable(this ILateUpdatable updatable) => LateUpdateManager.Register(updatable);
        public static void UnregisterLateUpdatable(this ILateUpdatable updatable) => LateUpdateManager.Unregister(updatable);
    }

    /// <summary>
    /// Manages the update cycle for all objects implementing the <see cref="ILateUpdatable"/> interface.
    /// </summary>
    public class LateUpdateManager : UpdateManagerBase<LateUpdateManager, ILateUpdatable>
    {
        protected override void UpdateUpdatable(ILateUpdatable updatable, float deltaTime) => updatable.OnLateUpdate(deltaTime);

        private void LateUpdate() => Poll(Time.deltaTime);
    }

}
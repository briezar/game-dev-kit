using System;
using System.Collections.Generic;
using GameDevKit.Collections;
using UnityEngine;

namespace GameDevKit.UpdateManagers
{
    /// <summary>
    /// Interface for objects that need to be updated during the FixedUpdate phase.
    /// </summary>
    public interface IFixedUpdatable { void OnFixedUpdate(float deltaTime); }

    public static class FixedUpdateManagerExtensions
    {
        public static void RegisterFixedUpdatable(this IFixedUpdatable updatable) => FixedUpdateManager.Register(updatable);
        public static void UnregisterFixedUpdatable(this IFixedUpdatable updatable) => FixedUpdateManager.Unregister(updatable);
    }

    /// <summary>
    /// Manages the update cycle for all objects implementing the <see cref="IFixedUpdatable"/> interface.
    /// </summary>
    public class FixedUpdateManager : UpdateManagerBase<FixedUpdateManager, IFixedUpdatable>
    {
        protected override void UpdateUpdatable(IFixedUpdatable updatable, float deltaTime) => updatable.OnFixedUpdate(deltaTime);

        private void FixedUpdate() => Poll(Time.fixedDeltaTime);
    }

}
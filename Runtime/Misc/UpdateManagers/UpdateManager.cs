using System;
using System.Collections.Generic;
using GameDevKit.Collections;
using UnityEngine;

namespace GameDevKit.UpdateManagers
{
    /// <summary>
    /// Interface for objects that need to be updated during the Update phase.
    /// </summary>
    public interface IUpdatable { void OnUpdate(float deltaTime); }

    public static class UpdateManagerExtensions
    {
        public static void RegisterUpdatable(this IUpdatable updatable) => UpdateManager.Register(updatable);
        public static void UnregisterUpdatable(this IUpdatable updatable) => UpdateManager.Unregister(updatable);
    }

    /// <summary>
    /// Manages the update cycle for all objects implementing the <see cref="IUpdatable"/> interface.
    /// </summary>
    public class UpdateManager : UpdateManagerBase<UpdateManager, IUpdatable>
    {
        protected override void UpdateUpdatable(IUpdatable updatable, float deltaTime) => updatable.OnUpdate(deltaTime);

        private void Update() => Poll(Time.deltaTime);
    }

}
using System;
using UnityEngine;

namespace GameDevKit.Pool
{
    [Serializable]
    public class SmartParticleSystemPool : SmartComponentPool<ParticleSystem>
    {
        private bool _isInit;

        public SmartParticleSystemPool(ParticleSystem template, float autoReleaseTime = -1) : base(template, autoReleaseTime) { }

        public override ParticleSystem GetAndAutoPool(Vector3? position = null, TimeSpan? customAutoReleaseTime = null)
        {
            if (_template == null) { return null; }
            if (!_isInit)
            {
                _isInit = true;
                var sceneTemplate = Pool.Template;
                var mainModule = sceneTemplate.main;
                if (mainModule.stopAction == ParticleSystemStopAction.Destroy)
                {
                    Debug.Log($"{GetPoolName(_template)} scene template has a stop action of Destroy. Changing it to None.");
                    mainModule.stopAction = ParticleSystemStopAction.None;
                }
                mainModule.playOnAwake = true;
            }
            var pooledObj = base.GetAndAutoPool(position, customAutoReleaseTime);
            return pooledObj;
        }
    }

}
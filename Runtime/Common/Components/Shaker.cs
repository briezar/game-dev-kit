using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

namespace GameDevKit
{
    public class Shaker : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _strength = 12;
        [SerializeField] private int _vibrato = 15;
        [SerializeField] private float _randomness = 30;
        [SerializeField] private bool _snapping = false;
        [SerializeField] private bool _fadeOut = true;
        [SerializeField] private ShakeRandomnessMode _randomnessMode = ShakeRandomnessMode.Harmonic;

        public void Shake(float duration, float strength = 12, int vibrato = 15)
        {
            transform.DOKill(true);
            transform.DOShakePosition(duration, strength, vibrato, 30, false, true, ShakeRandomnessMode.Harmonic);
        }

        [Button]
        private void Shake()
        {
            transform.DOKill(true);
            transform.DOShakePosition(_duration, _strength, _vibrato, _randomness, _snapping, _fadeOut, _randomnessMode);
        }

    }
}
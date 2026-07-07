using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class RandomRotation : MonoBehaviour
    {
        public Vector3 From, To;
        public bool RandomOnEnable = true;

        private void OnEnable()
        {
            if (RandomOnEnable)
            {
                RotateRandomly();
            }
        }

        [Button]
        public void RotateRandomly()
        {
            var rot = transform.rotation.eulerAngles;
            rot.x = Random.Range(From.x, To.x);
            rot.y = Random.Range(From.y, To.y);
            rot.z = Random.Range(From.z, To.z);
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}
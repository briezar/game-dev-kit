using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public abstract class Spawner : MonoBehaviour
    {
        public abstract void Spawn(Transform target);
    }

    public class GameObjectSpawner : Spawner
    {
        [SerializeField] private GameObject _objToSpawn;

        public override void Spawn(Transform target)
        {
            var obj = Instantiate(_objToSpawn, target.position, target.rotation);
            obj.SetActive(true);
        }
    }
}
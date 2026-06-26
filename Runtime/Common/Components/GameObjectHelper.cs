using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public class GameObjectHelper : MonoBehaviour
    {
        public void SetInactive(bool value) => gameObject.SetActive(!value);

        public void Destroy(GameObject otherGameObject) => Destroy(otherGameObject);
        public void DestroySelf() => Destroy(gameObject);
    }
}
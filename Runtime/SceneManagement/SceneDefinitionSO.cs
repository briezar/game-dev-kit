using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using GameDevKit.Attributes;
using UnityEngine;

namespace GameDevKit.SceneManagement
{
    [CreateAssetMenu(menuName = "GameDevKit/SceneManagement/SceneDefinition")]
    public class SceneDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public SceneReference Scene { get; set; }
        [field: SerializeField] public SceneReference[] ScenesToUnload { get; set; }

        [SubclassPicker]
        [SerializeReference] private ISceneTransition _transition = new UIManagerFadeTransition();

        public UniTask LoadScene(SceneTransitionOptions options = default) => _transition.Execute(this, options);
        public UniTask LoadScene(ISceneTransition transition, SceneTransitionOptions options = default) => transition.Execute(this, options);
    }

}

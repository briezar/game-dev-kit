using System;
using UnityEngine;
using TMPro;

namespace GameDevKit.Text
{
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public class TextPreprocessor : MonoBehaviour
    {
        [SerializeField] private TextPreprocessorInstruction _instruction;

#if UNITY_EDITOR
        private void OnValidate()
        {
            Start();
        }
#endif

        protected virtual void Start()
        {
            if (_instruction == null)
            {
                Debug.LogWarning("Instruction is null, will not preprocess text", this);
                return;
            }

            var textComponent = GetComponent<TMP_Text>();
            textComponent.textPreprocessor = _instruction;
            textComponent.ForceMeshUpdate();
        }
    }
}
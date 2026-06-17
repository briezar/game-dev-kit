using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using GameDevKit.Attributes;
using EditorAttributes;
using static GameDevKit.UI.UIAnimationInstruction;

namespace GameDevKit.UI
{
    public class UIAnimation : MonoBehaviour
    {
        [SubclassPicker]
        [SerializeReference] public List<Instruction> Instructions;

        [Button]
        public void Play(int index = 0) => PlayInternal(Instructions[index]);

        public UniTask PlayAsync(int index) => PlayInternal(Instructions[index]);
        public UniTask PlayAsync<T>() where T : Instruction, new() => PlayInternal(GetInstruction<T>());

        public T GetInstruction<T>() where T : Instruction, new()
        {
            if (!Instructions.TryGet(instruction => instruction is T, out var instruction))
            {
                instruction = new T();
                Instructions.Add(instruction);
            }
            return instruction as T;
        }

        private UniTask PlayInternal(Instruction instruction)
        {
            if (instruction.transform == null)
            {
                instruction.SetTransform(transform);
            }

            return instruction.Play();
        }

    }
}
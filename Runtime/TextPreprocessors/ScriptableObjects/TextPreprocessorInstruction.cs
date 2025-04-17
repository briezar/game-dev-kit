using TMPro;
using UnityEngine;

namespace GameDevKit.Text
{
    public abstract class TextPreprocessorInstruction : ScriptableObject, ITextPreprocessor
    {
        public abstract string PreprocessText(string text);

    }
}
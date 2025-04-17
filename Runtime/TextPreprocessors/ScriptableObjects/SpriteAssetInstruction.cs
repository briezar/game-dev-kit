using TMPro;
using UnityEngine;

namespace GameDevKit.Text
{
    [CreateAssetMenu(menuName = "TextPreprocessors/SpriteAssetInstruction")]
    public class SpriteAssetInstruction : TextPreprocessorInstruction
    {
        [SerializeField] private TMP_SpriteAsset _spriteAsset;
        [SerializeField] private string _characterMap = "$.0123456789";

        public string SpriteAssetFormat => $"<sprite=\"{_spriteAsset.name}\"" + " index={0}>";

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spriteAsset == null) { return; }
            var spriteCharLength = _spriteAsset.spriteCharacterTable.Count;
            var charMapLength = _characterMap.Length;

            if (_characterMap.Length != spriteCharLength)
            {
                Debug.LogWarning($"CharacterMap ({charMapLength} characters) should have the same character count as SpriteAsset ({spriteCharLength} characters)!");
            }
        }
#endif

        public override string PreprocessText(string text)
        {
            var output = string.Empty;
            foreach (var character in text)
            {
                if (character == ' ') { output += character; }
                if (!_characterMap.Contains(character)) { continue; }

                var index = _characterMap.IndexOf(character);
                var spriteCharacter = SpriteAssetFormat.Format(index);
                output += spriteCharacter;
            }
            return output;
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameDevKit.UI
{
    public class InputFieldChain : MonoBehaviour
    {
        private class TextButton
        {
            public readonly Button Btn;
            private readonly TextMeshProUGUI _textComponent;

            public string text
            {
                get => _textComponent.text;
                set
                {
                    _textComponent.text = value;
                }
            }

            public TextButton(Button button)
            {
                Btn = button;
                _textComponent = button.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        [SerializeField] private TouchScreenKeyboardType _touchKeyboardType;
        [SerializeField] private Button[] _buttons;

        private TextButton[] _textButtons;

        protected TouchScreenKeyboard _touchKeyboard;
        private System.Text.StringBuilder _stringBuilder = new();

        private TextButton _currentBtn;

        public Action OnValueChanged, OnSubmit;

        public string CombinedText { get; private set; } = "";
        public int CharacterLimit => _textButtons.Length;


        private void Awake()
        {
            _textButtons = new TextButton[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++)
            {
                var textBtn = new TextButton(_buttons[i]);
                textBtn.Btn.onClick.AddListener(() => Click_Button(textBtn));
                textBtn.text = "";

                _textButtons[i] = textBtn;
            }
        }

        private void LateUpdate()
        {
            if (_currentBtn == null)
            {
                StopUpdate();
                return;
            }

            if (LostFocus())
            {
                StopUpdate();
                return;
            }

            if (TouchScreenKeyboard.isSupported)
            {
                HandleTouchKeyboard();
            }
            else
            {
                HandleNormalKeyboard();
            }

        }

        private void StopUpdate()
        {
            enabled = false;
            Deselect();
            HideKeyboard();
        }

        private bool LostFocus()
        {
            if (TouchScreenKeyboard.isSupported && _touchKeyboard != null)
            {
                switch (_touchKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Done:
                        OnSubmit?.Invoke();
                        return true;

                    case TouchScreenKeyboard.Status.Canceled:
                        return true;

                    case TouchScreenKeyboard.Status.Visible:
                    case TouchScreenKeyboard.Status.LostFocus:
                        break;
                }
            }

            if (Input.GetMouseButtonDown(0) && !ClickedAnyButton())
            {
                return true;
            }

            return false;
        }

        private void HandleTouchKeyboard()
        {
            if (_touchKeyboard == null || _touchKeyboard.active == false) { return; }

            var keyboardText = _touchKeyboard.text ?? "";
            if (keyboardText == CombinedText) { return; }

            var validatedText = "";

            foreach (var c in keyboardText)
            {
                if (validatedText.Length >= CharacterLimit) { break; }

                if (c is SpecialCharacters.Return or SpecialCharacters.EndOfText or SpecialCharacters.NewLine)
                {
                    SetCombinedText(validatedText);
                    OnSubmit?.Invoke();
                    StopUpdate();
                    return;
                }

                // if (c is SpecialCharacter.Backspace)
                // {
                //     if (_deleteAllBehindCurrent) { break; }
                // }

                if (IsValidChar(c))
                {
                    validatedText += c;
                }
            }

            if (validatedText != keyboardText)
            {
                _touchKeyboard.text = validatedText;
            }

            var increase = CombinedText.Length < validatedText.Length;

            SetCombinedText(validatedText);

            var btnIndex = Array.IndexOf(_textButtons, _currentBtn);
            btnIndex += increase ? 1 : -1;

            if (btnIndex > _textButtons.Length - 1)
            {
                StopUpdate();
                return;
            }

            btnIndex = btnIndex.ClampMin(0);
            Select(btnIndex);
        }

        private bool IsValidChar(char inputChar)
        {
            switch (_touchKeyboardType)
            {
                case TouchScreenKeyboardType.NumberPad:
                case TouchScreenKeyboardType.OneTimeCode:
                    if (inputChar < '0' || inputChar > '9')
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        #region NormalKeyboard
        private void HandleNormalKeyboard()
        {
            var inputString = Input.inputString;
            if (inputString.IsNullOrEmpty()) { return; }

            var inputChar = inputString.GetLastChar();

            switch (inputChar)
            {
                case '\b':
                    HandleBackspace();
                    return;
                case '\n':
                    HandleEnter();
                    return;
            }

            if (!IsValidChar(inputChar)) { return; }

            _currentBtn.text = inputChar.ToString();
            UpdateCombinedText();

            var btnIndex = Array.IndexOf(_textButtons, _currentBtn);
            if (btnIndex < _textButtons.Length - 1)
            {
                Select(_textButtons[btnIndex + 1]);
            }
            else
            {
                StopUpdate();
            }
        }

        private void HandleBackspace()
        {
            // Delete the last non-empty field
            for (int i = _textButtons.Length - 1; i >= 0; i--)
            {
                var btn = _textButtons[i];
                if (btn == _currentBtn) { break; }

                if (!IsEmpty(btn))
                {
                    _currentBtn = btn;
                    break;
                }
            }

            _currentBtn.text = "";
            UpdateCombinedText();

            var btnIndex = Array.IndexOf(_textButtons, _currentBtn);
            if (btnIndex > 0)
            {
                Select(_textButtons[btnIndex - 1]);
            }
            else
            {
                _currentBtn.text = "_";
            }
        }
        private void HandleEnter()
        {
            StopUpdate();
        }
        #endregion

        private void SetCombinedText(string text, bool notify = true)
        {
            if (text == CombinedText) { return; }

            text ??= "";
            if (text.Length > CharacterLimit)
            {
                text = text.Substring(0, CharacterLimit);
            }

            for (int i = 0; i < _textButtons.Length; i++)
            {
                var charText = "";
                if (i < text.Length)
                {
                    charText = $"{text[i]}";
                }
                _textButtons[i].text = charText;
            }

            CombinedText = text;
            if (notify) { OnValueChanged?.Invoke(); }
        }

        public void Clear()
        {
            CombinedText = "";
            foreach (var textBtn in _textButtons)
            {
                textBtn.text = "";
            }
        }

        private void UpdateCombinedText(bool notify = true)
        {
            _stringBuilder.Clear();
            foreach (var textBtn in _textButtons)
            {
                if (IsEmpty(textBtn)) { continue; }
                _stringBuilder.Append(textBtn.text);
            }
            CombinedText = _stringBuilder.ToString();
            if (notify) { OnValueChanged?.Invoke(); }
        }

        private bool ClickedAnyButton()
        {
            foreach (var textBtn in _textButtons)
            {
                var btnTransform = textBtn.Btn.targetGraphic.rectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(btnTransform, Input.mousePosition, Camera.main, out var localPoint);
                var clicked = btnTransform.rect.Contains(localPoint);
                if (clicked) { return true; }
            }
            return false;
        }

        private void Click_Button(TextButton textButton)
        {
            // Select the lowest empty field instead of the clicked field
            foreach (var btn in _textButtons)
            {
                if (btn == textButton) { break; }
                if (IsEmpty(btn))
                {
                    textButton = btn;
                    break;
                }
            }

            Select(textButton);
            SetTouchKeyboardCaretToButton(textButton);
        }

        private void SetTouchKeyboardCaretToButton(TextButton textButton)
        {
            if (!TouchScreenKeyboard.isSupported) { return; }
            if (_touchKeyboard == null || _touchKeyboard.status != TouchScreenKeyboard.Status.Visible) { return; }

            var index = Array.IndexOf(_textButtons, textButton);
            _touchKeyboard.selection = new RangeInt(index, 0);
        }

        private bool IsEmpty(TextButton textButton)
        {
            var text = textButton.text;
            return text.Trim().IsNullOrEmpty() || text == "_";
        }

        private void Select(TextButton textButton)
        {
            _currentBtn = textButton;
            textButton.Btn.Select();
            if (IsEmpty(textButton))
            {
                textButton.text = "_";
            }
            enabled = true;

            if (!TouchScreenKeyboard.isSupported) { return; }
            if (_touchKeyboard != null && _touchKeyboard.active) { return; }

            _touchKeyboard ??= TouchScreenKeyboard.Open(CombinedText, _touchKeyboardType, false, false, false, false, "", CharacterLimit);
        }

        public void Select(int index)
        {
            index = index.ClampCollection(_textButtons);
            Select(_textButtons[index]);
        }

        private void Deselect()
        {
            if (_currentBtn == null) { return; }
            var eventSystem = EventSystem.current;
            if (!eventSystem.alreadySelecting && eventSystem.currentSelectedGameObject == _currentBtn.Btn.gameObject) eventSystem.SetSelectedGameObject(null);

            if (IsEmpty(_currentBtn))
            {
                _currentBtn.text = "";
            }

            _currentBtn = null;
        }

        private void HideKeyboard()
        {
            if (_touchKeyboard == null) { return; }
            _touchKeyboard.active = false;
            _touchKeyboard = null;
        }
    }
}
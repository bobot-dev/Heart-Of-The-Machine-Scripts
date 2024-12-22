using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeartOfTheMachineScripts
{
    [DefaultExecutionOrder(-300)]
    public class Keypad : MonoBehaviour
    {
        private void Awake()
        {
            _codeLength = code.Length;

            clearButton.onClick.AddListener(Backspace);
            checkButton.onClick.AddListener(CheckCode);

            _checkShopButton = checkButton.GetComponent<ShopButton>();
            _clearShopButton = clearButton.GetComponent<ShopButton>();

            foreach (var button in GetComponentsInChildren<Button>())
            {
                if (int.TryParse(button.name, out int digit))
                {
                    _inputButtons.Add(button);
                    button.onClick.AddListener(() => { InputDigit(digit); });
                }
            }

            _currentCode = new string('-', _codeLength);
            _startColor = codeDisplay.color;
            UpdateDisplay();
        }

        private void Start()
        {
            UpdateKnownCodeDisplay();
            ReadableCode.codeFound += UpdateKnownCodeDisplay;
        }

        private void OnDestroy()
        {
            ReadableCode.codeFound -= UpdateKnownCodeDisplay;
        }

        private void UpdateKnownCodeDisplay()
        {
            bool valueOrDefault = MapVarManager.Instance.GetBool(ReadableCode.codePrefix + code).GetValueOrDefault();
            _knownCodeText = knownCodeDisplay.GetComponentInChildren<TMP_Text>();
            _knownCodeText.text = code;
            knownCodeDisplay.SetActive(valueOrDefault);
        }


        public void InputDigit(int digit)
        {
            if (_currentCodeIndex >= _codeLength) 
                return;

            var sound = Instantiate(press, transform.position, Quaternion.identity);
            sound.pitch += ((float)digit - 5) / 40;

            _currentCode = new StringBuilder(_currentCode) { [_currentCodeIndex] = char.Parse($"{digit}") }.ToString();
            _currentCodeIndex++;

            UpdateDisplay();
        }

        public void Clear()
        {
            _currentCode = new string('-', _codeLength);
            codeDisplay.text = _currentCode;
            _currentCodeIndex = 0;

            UpdateDisplay();
        }

        public void Backspace()
        {
            if (_currentCodeIndex < 1)
                return;

            _currentCodeIndex--;
            _currentCode = new StringBuilder(_currentCode) { [_currentCodeIndex] = '-' }.ToString();


            UpdateDisplay();
        }


        public void CheckCode()
        {
            if (_currentCode == code)
            {
                Success();
                return;
            }
            if (_currentCode == "0451")
            {
                on0451?.Invoke();
            }
            Failure();
        }


        private void UpdateDisplay()
        {
            // string code = _currentCode;
            //
            // for (int i = 0; i < _codeLength - _currentCodeIndex; i++)
            // {
            //     code += "-";
            // }

            codeDisplay.text = _currentCode;

            checkButton.interactable = (_currentCodeIndex >= _codeLength);
            _checkShopButton.deactivated = !(_currentCodeIndex >= _codeLength);

            clearButton.interactable = (_currentCodeIndex > 0);
            _clearShopButton.deactivated = !(_currentCodeIndex > 0);

            ToggleDigitButtons(_currentCodeIndex < _codeLength);
            
        }


        private void Success()
        {
            Instantiate(success, transform.position, Quaternion.identity);
            onCorrect?.Invoke();

            codeDisplay.color = successColor;
            ToggleAllButtons(false);
        }

        private void Failure()
        {
            Instantiate(failure, transform.position, Quaternion.identity);

            _failureLockoutActive = true;
            _failureLockoutElapsed = 0;
        }

        public void ToggleDigitButtons(bool enable)
        {
            foreach (var button in _inputButtons)
            {
                button.interactable = enable;
            }
        }

        public void ToggleAllButtons(bool enable)
        {
            ToggleDigitButtons(enable);

            checkButton.interactable = enable;
            _checkShopButton.deactivated = !enable;
            clearButton.interactable = enable;
            _clearShopButton.deactivated = !enable;
        }

        private void Update()
        {
            if (_failureLockoutActive)
            {
                _failureLockoutElapsed += Time.deltaTime;

                codeDisplay.color = Color.Lerp(failureColor, _startColor, _failureLockoutElapsed / failureLockout);

                if (_failureLockoutElapsed >= failureLockout)
                {
                    _failureLockoutActive = false;

                    Clear();
                    ToggleAllButtons(true);
                    UpdateDisplay();

                    codeDisplay.color = _startColor;
                }
            }
        }


        private TMP_Text _knownCodeText;
        public GameObject knownCodeDisplay;

        public Button checkButton;
        public Button clearButton;

        private ShopButton _checkShopButton;
        private ShopButton _clearShopButton;

        public TMP_Text codeDisplay;
        public GameObject failure;
        public GameObject success;
        public AudioSource press;

        public float failureLockout;
        public Color failureColor;
        public Color successColor;

        private int _codeLength;

        //[SerializeField]
        //public Keycode[] codes;

        //i fucking hate this engine jfc
        public string code;
        public UnityEvent onCorrect;
        public UnityEvent on0451;

        private bool _failureLockoutActive;
        private float _failureLockoutElapsed;

        private int _currentCodeIndex;
        private string _currentCode = "";

        private Color _startColor;
        private List<Button> _inputButtons = new List<Button>();
    }
}

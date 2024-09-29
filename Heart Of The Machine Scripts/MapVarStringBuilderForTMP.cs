using Logic;
using System.Text;
using TMPro;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class MapVarStringBuilderForTmp : MonoBehaviour
    {
        private void OnEnable()
        {
            if (_buildOnEnable)
            {
                BuildString();
            }
        }

        private void Update()
        {
            if (_buildOnUpdate)
            {
                BuildString();
            }
        }

        public void BuildString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StringPart stringPart in stringParts)
            {
                stringBuilder.Append(stringPart.GetString());
            }
            if (_textTarget != null)
            {
                if (_textMethod == TextSetMethod.SetText)
                {
                    _textTarget.text = stringBuilder.ToString();
                }
                else if (_textMethod == TextSetMethod.AppendText)
                {
                    TMP_Text text = _textTarget;
                    text.text += stringBuilder.ToString();
                }
                else if (_textMethod == TextSetMethod.PrependText)
                {
                    _textTarget.text = stringBuilder + _textTarget.text;
                }
            }
            if (!string.IsNullOrEmpty(_stringVariableName))
            {
                MonoSingleton<MapVarManager>.Instance.SetString(_stringVariableName, stringBuilder.ToString(), false);
            }
        }

        [Header("Input")]
        public StringPart[] stringParts;

        [Header("Output")]
        public string _stringVariableName;

        [SerializeField]
        private TextSetMethod _textMethod;

        [SerializeField]
        private TMP_Text _textTarget;

        [Header("Events")]
        [SerializeField]
        private bool _buildOnEnable;

        [SerializeField]
        private bool _buildOnUpdate;
    }
}

using System.Text;
using Logic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeartOfTheMachineScripts
{
    public class MapVarStringBuilderForTmp : MonoBehaviour
    {
        private void OnEnable()
        {
            if (buildOnEnable)
            {
                BuildString();
            }
        }

        private void Update()
        {
            if (buildOnUpdate)
            {
                BuildString();
            }
        }

        public void BuildString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var stringPart in stringParts)
            {
                stringBuilder.Append(stringPart.GetString());
            }
            if (textTarget)
            {
                switch (textMethod)
                {
                    case TextSetMethod.SetText:
                        textTarget.text = stringBuilder.ToString();
                        break;
                    case TextSetMethod.AppendText:
                        {
                            var text = textTarget;
                            text.text += stringBuilder.ToString();
                            break;
                        }
                    case TextSetMethod.PrependText:
                        textTarget.text = stringBuilder + textTarget.text;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(stringVariableName))
            {
                MonoSingleton<MapVarManager>.Instance.SetString(stringVariableName, stringBuilder.ToString(), false);
            }
        }

        [Header("Input")]
        public StringPart[] stringParts;

        [FormerlySerializedAs("_stringVariableName")]
        [Header("Output")]
        public string stringVariableName;

        [FormerlySerializedAs("_textMethod")]
        [SerializeField]
        private TextSetMethod textMethod;

        [FormerlySerializedAs("_textTarget")]
        [SerializeField]
        private TMP_Text textTarget;

        [FormerlySerializedAs("_buildOnEnable")]
        [Header("Events")]
        [SerializeField]
        private bool buildOnEnable;

        [FormerlySerializedAs("_buildOnUpdate")]
        [SerializeField]
        private bool buildOnUpdate;
    }
}

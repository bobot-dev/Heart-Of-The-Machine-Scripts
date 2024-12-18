using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace HeartOfTheMachineScripts
{
    //[ExecuteInEditMode]
    public class ReadableCode : MonoBehaviour
    {
        private void Awake()
        {
            if (ScanningStuff.Instance?.readingText?.styleSheet != StaticRefs.Instance?.styleSheet)
                ScanningStuff.Instance.readingText.styleSheet = StaticRefs.Instance.styleSheet;



            var foundCodes = new List<string>();

            if (TryGetComponent(out Readable readable))
            {
                string pattern = @"<style=Code>(?'code'\d+)<\/style>";
                RegexOptions options = RegexOptions.Multiline;

               

                foreach (Match m in Regex.Matches(readable.content, pattern, options))
                {
                    foreach (Group mGroup in m.Groups)
                    {
                        Debug.Log(mGroup.Value);
                        foundCodes.Add(mGroup.Value);
                    }
                }
            }

            _codes = foundCodes.ToArray();

            foreach (var code in _codes)
            {
                if (MapVarManager.Instance.GetBool(codePrefix + code) == null)
                {
                    MapVarManager.Instance.SetBool(codePrefix + code, false, true);
                }
            }
            

            if (TryGetComponent(out ItemIdentifier itemId))
            {
                itemId.onPickUp.onActivate.AddListener(BookPickedUp);
            }
        }


        public void BookPickedUp()
        {
            foreach (var code in _codes)
            {
                Debug.Log("found code: " + code);
                MapVarManager.Instance.SetBool(codePrefix + code, true, true);
            }

            codeFound?.Invoke();
        }

        public static string codePrefix = "HOTMKeypadCode";

        private string[] _codes;

        public static Action codeFound;
    }
}

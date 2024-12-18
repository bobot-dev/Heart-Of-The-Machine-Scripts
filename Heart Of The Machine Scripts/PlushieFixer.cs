using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class PlushieFixer : MonoBehaviour
    {
        private void Awake()
        {
            _itemIdentifier = GetComponent<ItemIdentifier>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_itemIdentifier.pickedUp || !_itemIdentifier.ipz)
            {
                if (TryToFindRoom(collision.transform, out var room) && Regex.IsMatch(room.name, "\\d+\\w* - .*"))
                {
                    transform.SetParent(room, true);
                }
            }
        }

        public bool TryToFindRoom(Transform hitTransform, out Transform root)
        {
            root = hitTransform.root;
            Debug.Log(root.name);
            return root;
        }

        private ItemIdentifier _itemIdentifier;
    }
}
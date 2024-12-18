using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeartOfTheMachineScripts
{
    public class ItemPlaceZoneThingForHotmChallenge : MonoBehaviour
    {
        public void Check(ItemPlaceZone zone)
        {
            ItemIdentifier componentInChildren = zone.GetComponentInChildren<ItemIdentifier>();
            if (componentInChildren)
            {
                if (acceptedTypes.Contains(componentInChildren.itemType))
                {
                    onValidPlacedItem?.Invoke();
                }
                _lastType = componentInChildren.itemType;
            }
            else
            {
                if (acceptedTypes.Contains(_lastType))
                {
                    onValidRemovedItem?.Invoke();
                }
            }
        }

        public ItemType[] acceptedTypes;

        public UnityEvent onValidPlacedItem;

        public UnityEvent onValidRemovedItem;

        private ItemType _lastType;
    }
}
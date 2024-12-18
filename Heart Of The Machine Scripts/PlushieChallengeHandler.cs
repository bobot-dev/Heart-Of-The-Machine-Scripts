using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeartOfTheMachineScripts
{
    public class PlushieChallengeHandler : MonoBehaviour
    {
        private void Start()
        {
            _maxPlushAmount = itemZones.Length;
            _shopButton = button.GetComponent<ShopButton>();
            foreach (ItemPlaceZoneThingForHotmChallenge itemPlaceZoneThingForHotmChallenge in itemZones)
            {
                itemPlaceZoneThingForHotmChallenge.onValidPlacedItem.AddListener(ItemPlaced);
                itemPlaceZoneThingForHotmChallenge.onValidRemovedItem.AddListener(ItemRemoved);
            }
            UpdateStuff();
        }

        private void OnDestroy()
        {
            foreach (ItemPlaceZoneThingForHotmChallenge itemPlaceZoneThingForHotmChallenge in itemZones)
            {
                itemPlaceZoneThingForHotmChallenge.onValidPlacedItem.RemoveListener(ItemPlaced);
                itemPlaceZoneThingForHotmChallenge.onValidRemovedItem.RemoveListener(ItemRemoved);
            }
        }

        private void ItemPlaced()
        {
            _currentPlushAmount++;
            UpdateStuff();
        }

        private void ItemRemoved()
        {
            _currentPlushAmount--;
            UpdateStuff();
        }

        private void UpdateStuff()
        {
            textDisplay.text = $"{_currentPlushAmount}/{_maxPlushAmount}";
            button.interactable = (_currentPlushAmount > 0);
            _shopButton.failure = (_currentPlushAmount <= 0);
            var text = button.GetComponentInChildren<TMP_Text>();
            if (text)
            {
                text.color = ((_currentPlushAmount > 0) ? button.colors.normalColor : button.colors.disabledColor);
            }
            if (!_eventTriggered && _currentPlushAmount >= _maxPlushAmount)
            {
                onMaxPlushies?.Invoke();
                _eventTriggered = true;
            }
            if (_eventTriggered && _currentPlushAmount < _maxPlushAmount)
            {
                onMaxPlushies?.Revert();
                _eventTriggered = false;
            }
        }

        public ItemPlaceZoneThingForHotmChallenge[] itemZones;

        public TMP_Text textDisplay;

        public Button button;

        private ShopButton _shopButton;

        public UltrakillEvent onMaxPlushies;

        private bool _eventTriggered;

        private int _currentPlushAmount = 0;

        private int _maxPlushAmount;
    }
}

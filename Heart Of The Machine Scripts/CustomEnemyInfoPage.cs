using Logic;
using System;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;


namespace HeartOfTheMachineScripts
{


    public class CustomEnemyInfoPage : ListComponent<CustomEnemyInfoPage>
    {
        public static bool forceLocked = false;

        private void Start()
        {
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            if (_enemyList.childCount > 1)
            {
                for (int i = _enemyList.childCount - 1; i > 0; i--)
                {
                    Destroy(_enemyList.GetChild(i).gameObject);
                }
            }

            for (var i = 0; i < _objects.enemies.Length; i++)
            {
                var spawnableObject = _objects.enemies[i];
                if (spawnableObject)
                {
                    bool locked = (MapVarManager.Instance.GetBool(spawnableObject.identifier) ?? false) && !forceLocked;

                    if (locked)
                    {
                        _buttonTemplateBackground.color = spawnableObject.backgroundColor;
                        _buttonTemplateForeground.sprite = spawnableObject.gridIcon;
                        _stringBuilder.stringParts[0].value = $"{spawnableObject.identifier}.kills";
                        _stringBuilder.stringVariableName = $"{spawnableObject.identifier}.kills.output";
                    }
                    else
                    {
                        _buttonTemplateBackground.color = Color.gray;
                        _buttonTemplateForeground.sprite = _lockedSprites[i];

                    }

                    GameObject button = Instantiate(_buttonTemplate, _enemyList);
                    button.SetActive(true);

                    if (locked)
                    {

                        button.GetComponentInChildren<ShopButton>().deactivated = false;

                        button.GetComponentInChildren<Button>().onClick.AddListener(delegate ()
                        {
                            _currentSpawnable = spawnableObject;
                            DisplayInfo(spawnableObject);
                        });
                    }
                    else
                    {
                        button.GetComponentInChildren<ShopButton>().deactivated = true;
                        button.GetComponentInChildren<MapVarStringBuilderForTmp>().gameObject.SetActive(false);
                    }
                }
            }

            _buttonTemplate.SetActive(false);
        }

        private void SwapLayers(Transform target, int layer)
        {
            foreach (object obj in target)
            {
                Transform transform = (Transform)obj;
                transform.gameObject.layer = layer;
                if (transform.childCount > 0)
                {
                    SwapLayers(transform, layer);
                }
            }
        }

        private void DisplayInfo(SpawnableObject source)
        {
            _enemyPageTitle.text = source.objectName;
            string text = "<color=orange>TYPE: " + source.type + "\n\nDATA:</color>\n";
            if (MonoSingleton<BestiaryData>.Instance.GetEnemy(source.enemyType) > 1)
            {
                text += source.description;
            }
            else
            {
                text += "???";
            }

            if (!string.IsNullOrEmpty(source.strategy))
                text = text + "\n\n<color=orange>STRATEGY:</color>\n" + source.strategy;

            _enemyPageContent.text = text;
            _enemyPageContent.rectTransform.localPosition = new Vector3(_enemyPageContent.rectTransform.localPosition.x, 0f, _enemyPageContent.rectTransform.localPosition.z);
            for (int i = 0; i < _enemyPreviewWrapper.childCount; i++)
            {
                Destroy(_enemyPreviewWrapper.GetChild(i).gameObject);
            }
            GameObject gameObject = Instantiate(source.preview, _enemyPreviewWrapper);
            int layer = _enemyPreviewWrapper.gameObject.layer;
            SwapLayers(gameObject.transform, layer);
            gameObject.layer = layer;
            gameObject.transform.localPosition = source.menuOffset;
            Spin spin = gameObject.AddComponent<Spin>();
            spin.spinDirection = new Vector3(0f, 1f, 0f);
            spin.speed = 10f;
        }

        public void DisplayInfo()
        {
            if (_currentSpawnable == null)
            {
                return;
            }
            DisplayInfo(_currentSpawnable);
        }

        public void UndisplayInfo()
        {
            _currentSpawnable = null;
        }

        [SerializeField]
        private TMP_Text _enemyPageTitle;

        [SerializeField]
        private TMP_Text _enemyPageContent;

        [SerializeField]
        private Transform _enemyPreviewWrapper;

        [Space]
        [SerializeField]
        private Transform _enemyList;

        [SerializeField]
        private GameObject _buttonTemplate;

        [SerializeField]
        private Image _buttonTemplateBackground;

        [SerializeField]
        private Image _buttonTemplateForeground;

        [SerializeField]
        private MapVarStringBuilderForTmp _stringBuilder;

        [SerializeField]
        private Sprite[] _lockedSprites;

        [Space]
        [SerializeField]
        private SpawnableObjectsDatabase _objects;

        private SpawnableObject _currentSpawnable;
    }

}

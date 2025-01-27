using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.UI;
using static ServerManager;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.Storage
{
    public class InvFront : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text _sortText;
        [SerializeField] private Transform _content;
        [SerializeField] private BaseScrollRect _scrollRect;
        [SerializeField] private GameObject _infoSlot;
        [SerializeField] private GameObject _loadingText;
        [SerializeField] private GameObject _topButtons;
        [SerializeField] private TMP_Text _totalValueText;

        [Header("Placeholders")] // These should not remain to the finalized game
        [SerializeField] private Sprite _furnImagePlaceholder;

        [Header("Non-scene objects")]
        [SerializeField] private GameObject _invSlot;
        [SerializeField] private GameObject _buySlot;
        [SerializeField] private List<Sprite> _icons; // Place images in this list for use as icons, but also, the exact name of the image must be set in the GameFurniture string Filename
        [SerializeField] private StorageFurnitureReference _furnitureReference;


        [Header("Information GameObject")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _weight;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private TMP_Text _material;
        [SerializeField] private Image _type;
        [SerializeField] private TMP_Text _typeText;
        [SerializeField] private GameObject _inSoulHome;
        [SerializeField] private TMP_Text _artist;
        [SerializeField] private TMP_Text _artisticDescription;
        [SerializeField] private TMP_Text _rarity;

        [Header("Rarity Color")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color antiqueColor = Color.yellow;

        private List<StorageFurniture> _items;
        private List<GameObject> _slotsList = new();

        bool _startCompleted = false;
        bool _updatingInventory = false;

        private int _maxSortingBy = 5;
        private int _sortingBy = -1; // used as a carrier for info on how to sort
        private bool _descendingOrder = false;

        private const string INVENTORY_EMPTY_TEXT = "Varasto tyhjä";

        private void OnEnable()
        {
            if (!_startCompleted) { StartCoroutine(Begin()); }

            ServerManager.OnClanInventoryChanged += UpdateInventory;
            UpdateInventory();
        }

        private void OnDisable()
        {
            ServerManager.OnClanInventoryChanged -= UpdateInventory;
        }

        private IEnumerator Begin()
        {
            Debug.Log("Starting Storage Load.");
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            yield return StartCoroutine(GetFurnitureFromClanInventory(playerGuid));

            MakeSlots();
            SortStored(); // Sorting before setting the slots / SetSlots is already in SortStored so no need to do it here

            if (_items.Count == 0)
            {
                _loadingText.GetComponent<TextMeshProUGUI>().text = INVENTORY_EMPTY_TEXT;
            }
            else
            {
                _loadingText.SetActive(false);
                _topButtons.SetActive(true); // The sorting button should not be available if items are yet to be loaded
            }


            _startCompleted = true;
        }

        private void Reset()
        {
            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            _scrollRect.NormalizedPosition = new Vector2(0, 1);

            _items.Clear();
            _slotsList.Clear();

            _loadingText.SetActive(true);
            _topButtons.SetActive(false); // The sorting button should not be available if items are yet to be loaded
        }

        public void UpdateInventory()
        {
            if (_updatingInventory)
                return;

            _updatingInventory = true;
            StartCoroutine(UpdateInventoryCoroutine());
        }

        private IEnumerator UpdateInventoryCoroutine()
        {
            yield return new WaitUntil(() => _startCompleted);

            Reset();

            if (ServerManager.Instance.Stock != null)
            {
                yield return StartCoroutine(ServerManager.Instance.SaveClanFromServerToDataStorage(ServerManager.Instance.Clan));
            }

            yield return StartCoroutine(Begin());
            _totalValueText.text = $"Varaston arvo: {GetTotalInventoryValue()}";
            _updatingInventory = false;
        }

        private IEnumerator GetFurnitureFromClanInventory(string playerGuid)
        {
            var store = Storefront.Get();

            // Get clan furniture from inventory.
            List<ClanFurniture> clanFurnitureList = null;
            store.GetPlayerData(playerGuid, playerData =>
            {
                if (playerData == null || !playerData.HasClanId)
                {
                    clanFurnitureList = new List<ClanFurniture>();
                    return;
                }
                store.GetClanData(playerData.ClanId, clanData =>
                {
                    clanFurnitureList = clanData?.Inventory.Furniture ?? new List<ClanFurniture>();
                });
            });
            // Wait for list to arrive.
            yield return new WaitUntil(() => clanFurnitureList != null);

            // Create furniture list for UI.
            _items = new List<StorageFurniture>();
            if (clanFurnitureList.Count == 0)
            {
                Debug.Log($"found clan items {_items.Count}");
                yield break;
            }

            // Find actual furniture pieces for the UI.
            ReadOnlyCollection<GameFurniture> allItems = null;
            yield return store.GetAllGameFurnitureYield(result => allItems = result);
            Debug.Log($"all items {allItems.Count}");
            foreach (var clanFurniture in clanFurnitureList)
            {
                //Debug.LogWarning(clanFurniture.GameFurnitureName);
                var gameFurnitureId = clanFurniture.GameFurnitureName;
                var furniture = allItems.FirstOrDefault(x => x.Name == gameFurnitureId);
                if (furniture == null)
                {
                    continue;
                }
                StorageFurniture storageFurniture = new(clanFurniture,furniture);
                _items.Add(storageFurniture);
            }
            Debug.Log($"found clan items {_items.Count}");
        }

        private void MakeSlots()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                GameObject newSlot = Instantiate(_invSlot, _content);
                var capturedSlotVal = i;

                // Default rarity to "common" since no rarity system is implemented yet
                string rarity = "common";

                // Set color based on rarity
                var backgroundImage = newSlot.GetComponent<Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.color = GetColorByRarity(rarity);
                }

                newSlot.GetComponent<Button>().onClick.AddListener(() =>
                {
                // C# variable capture in the body of anonymous function!
                OnShowInfo(capturedSlotVal);
                });
                _slotsList.Add(newSlot);
            }
            //Instantiate(_buySlot, _content);
        }

        private void SetSlots()
        {
            int i = 0;
            foreach (StorageFurniture _furn in _items)
            {
                Transform toSet = _slotsList[i].transform;

                // Icon
                toSet.GetChild(0).GetComponent<Image>().sprite = _furn.Sprite;
                ScaleSprite(_furn, toSet.GetChild(0).GetComponent<RectTransform>());

                // Name
                if(_sortingBy != 0) toSet.GetChild(1).GetComponent<TMP_Text>().text = _furn.VisibleName;
                else toSet.GetChild(1).GetComponent<TMP_Text>().text = "";

                // Weight
                switch (_sortingBy)
                {
                    case 0:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = _furn.VisibleName;
                        break;
                    case 1:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = "Arvo " + _furn.Value.ToString();
                        break;
                    case 2:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = _furn.Weight + " KG";
                        break;
                    case 3:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = _furn.Material;
                        break;
                    case 4:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = _furn.Rarity.ToString();
                        break;
                    case 5:
                        toSet.GetChild(3).GetComponent<TMP_Text>().text = _furn.SetName;
                        break;
                }
                // Shape
                toSet.GetChild(4).GetComponent<Image>().sprite = GetIcon("");

                // SetName
                toSet.GetChild(5).GetComponent<TMP_Text>().text = _furn.SetName;

                // Id
                toSet.GetChild(6).GetComponent<TMP_Text>().text = _furn.Info.DiagnoseNumber;
                // Name
                toSet.GetChild(7).GetChild(0).GetComponent<TMP_Text>().text = "Sielunkodissa";
                if (_furn.Position == new Vector2Int(-1, -1))
                {
                    toSet.GetChild(7).gameObject.SetActive(false);
                }
                else
                {
                    toSet.GetChild(7).gameObject.SetActive(true);
                }

                // Coin
                if(_sortingBy == 1) toSet.GetChild(8).gameObject.SetActive(true);
                else toSet.GetChild(8).gameObject.SetActive(false);

                i++;
            }
        }

        public void SwitchSortOrder() {
            _descendingOrder = !_descendingOrder;
            _sortingBy--; // Decrement here because SortStored will increment it again
            SortStored();
        }

        public void SortStored() // A very much hardcoded system for sorting 
        {
            if (_sortingBy < _maxSortingBy) { _sortingBy++; }
            else { _sortingBy = 0; }

            switch (_sortingBy)
            {
                case 0:
                    _sortText.text = "Jarjestetty\nAakkoset";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.VisibleName.CompareTo(a.VisibleName); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.VisibleName.CompareTo(b.VisibleName); });

                    break;
                case 1:
                    _sortText.text = "Jarjestetty\nArvo";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.Value.CompareTo(a.Value); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.Value.CompareTo(b.Value); });

                    break;
                case 2:
                    _sortText.text = "Jarjestetty\nPaino";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.Weight.CompareTo(a.Weight); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.Weight.CompareTo(b.Weight); });

                    break;
                case 3:
                    _sortText.text = "Jarjestetty\nMateriaali";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.Material.CompareTo(a.Material); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.Material.CompareTo(b.Material); });

                    break;
                case 4:
                    _sortText.text = "Jarjestetty\nHarvinaisuus";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.Rarity.CompareTo(a.Rarity); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.Rarity.CompareTo(b.Rarity); });

                    break;
                case 5:
                    _sortText.text = "Jarjestetty\nLinjasto";

                    if (_descendingOrder)
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return b.SetName.CompareTo(a.SetName); });
                    else
                        _items.Sort((StorageFurniture a, StorageFurniture b) => { return a.SetName.CompareTo(b.SetName); });

                    break;
            }
            SetSlots();
        }

        void OnShowInfo(int slotVal)
        {
            Transform parentSlot = _infoSlot.transform;
            StorageFurniture _furn = _items[slotVal];

            // Icon
            _icon.sprite = _furn.Sprite;
            ScaleSprite(_furn, _icon.rectTransform);

            // Name
            _name.text = _furn.Info.DiagnoseNumber + _furn.SetName + ": " + _furn.VisibleName;
            
            //Artists name
            _artist.text = _furn.Info != null ? "Suunnittelu: " + _furn.Info.ArtistName : "Unknown Artist";
            
            //Artistic description
            _artisticDescription.text = _furn.Info.ArtisticDescription;

            // Weight
            _weight.text = "Paino:" + _furn.Weight + " KG";

            // Material text
            _material.text = $"{_furn.Material}";

            // VAlue text
            _value.text = $"{_furn.Value}";

            if (_furn.Position == new Vector2Int(-1, -1))
                _inSoulHome.SetActive(false);
            else
                _inSoulHome.SetActive(true);

            // Type
            _type.sprite = GetIcon("");

            // Type Text
            _typeText.text = "";

            _rarity.text = _furn.Rarity.ToString();

            _infoSlot.SetActive(true);
        }

        private void ScaleSprite(StorageFurniture furn, RectTransform rTransform)
        {
            rTransform.sizeDelta = new(0, 0);
            Rect imageRect = rTransform.rect;
            if (furn.Sprite.bounds.size.x > furn.Sprite.bounds.size.y)
            {
                float diff = furn.Sprite.bounds.size.x / furn.Sprite.bounds.size.y;
                float newHeight = imageRect.height / diff;
                rTransform.sizeDelta = new(0, (newHeight - imageRect.height));
            }
            if (furn.Sprite.bounds.size.x < furn.Sprite.bounds.size.y)
            {
                float diff = furn.Sprite.bounds.size.y / furn.Sprite.bounds.size.x;
                float newWidth = imageRect.width / diff;
                rTransform.sizeDelta = new((newWidth - imageRect.width), 0);
            }
        }

        private Sprite GetIcon(string name)
        {
            //Sprite returned = _icons.Find(x => x.name == name);
            FurnitureInfo furnitureInfo = null/*_furnitureReference.GetFurnitureInfo(name)*/;
            Sprite returned = furnitureInfo?.Image;
            if (returned == null)
            {
                return _furnImagePlaceholder;
            }
            return returned;
        }

        private float GetTotalInventoryValue()
        {
            return _items.Sum(item => item.Value);
        }

        private Color GetColorByRarity(string rarity)
        {
            return rarity switch
            {
                "common" => commonColor,
                "rare" => rareColor,
                "epic" => epicColor,
                "antique" => antiqueColor,
                _ => commonColor, // Default to common color
            };
        }

        private FurnitureSetInfo GetFurnitureSetInfo(string furnitureName)
        {
            if (string.IsNullOrWhiteSpace(furnitureName))
                return null;

            string[] parts = furnitureName.Split('_');
            if (parts.Length != 2)
                return null;

            string setName = parts[1];
            foreach (FurnitureSetInfo setInfo in _furnitureReference.Info)
            {
                if (setInfo.SetName == setName)
                    return setInfo;
            }

            return null;
        }
    }
}

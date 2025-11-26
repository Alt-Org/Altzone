using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

namespace MenuUi.Scripts.Storage
{
    public class InvFront : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform _content;
        [SerializeField] private BaseScrollRect _scrollRect;
        [SerializeField] private GameObject _infoSlot;
        [SerializeField] private GameObject _loadingText;
        [SerializeField] private GameObject _topButtons;
        [SerializeField] private TMP_Text _totalValueText;
        [SerializeField] private TMP_Text _sortingByText;

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
        [SerializeField] private TMP_Text _diagnoseNumber;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private TMP_Text _material;
        [SerializeField] private Image _type;
        [SerializeField] private TMP_Text _typeText;
        [SerializeField] private GameObject _inSoulHome;
        [SerializeField] private GameObject _inVoting;
        [SerializeField] private TMP_Text _artist;
        [SerializeField] private TMP_Text _artisticDescription;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private Image _rarityImage;
        [SerializeField] private FurnitureSellHandler _sellHandler;

        [Header("Filtering")]
        [SerializeField] private Toggle[] _rarityToggles;
        [SerializeField] private SetFilterHandler _setFilterHandler;
        [SerializeField] private Toggle _inSoulHomeToggle;
        //[SerializeField] private Toggle _onSaleToggle;
        [SerializeField] private ValueSlider _maxValueSlider;
        [SerializeField] private ValueSlider _minValueSlider;

        private List<StorageFurniture> _items;
        private List<GameObject> _slotsList = new();

        bool _startCompleted = false;
        bool _updatingInventory = false;

        private int _maxSortingBy = 4;
        private int _sortingBy = 0; // used as a carrier for info on how to sort
        private bool _descendingOrder = false;

        private const string INVENTORY_EMPTY_TEXT = "Varasto tyhjä";

        private void Start()
        {
            // Make the filter buttons update the inventory
            _setFilterHandler.CreateSetFilterButtons();
            foreach (Toggle toggle in _setFilterHandler.toggleList)
            {
                toggle.onValueChanged.AddListener(delegate { UpdateInventory(); });
            }
        }

        private void OnEnable()
        {
            if (!_startCompleted) { StartCoroutine(Begin()); }

            _sellHandler.UpdateInfoAction += UpdateInVotingText;

            ServerManager.OnClanInventoryChanged += UpdateInventory;
            UpdateInventory();
        }

        private void OnDisable()
        {
            _sellHandler.UpdateInfoAction -= UpdateInVotingText;
            ServerManager.OnClanInventoryChanged -= UpdateInventory;

            // Hide the info window when exiting the view
            if (_infoSlot != null && _infoSlot.activeSelf)
            {
                _infoSlot.SetActive(false);
            }
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
            _updatingInventory = false;
        }

        private bool CheckFilters(StorageFurniture furn)
        {
            bool setCheck = false;
            for (int i = 0; i < _setFilterHandler.toggleList.Count; i++)
            {
                if (_furnitureReference.Info[i].SetName == furn.SetName && _setFilterHandler.toggleList[i].isOn) setCheck = true;
            }

            bool rarityCheck = false;
            switch (furn.Rarity)
            {
                case FurnitureRarity.Common:
                    if (_rarityToggles[0].isOn) rarityCheck = true;
                    break;
                case FurnitureRarity.Rare:
                    if (_rarityToggles[1].isOn) rarityCheck = true;
                    break;
                case FurnitureRarity.Epic:
                    if (_rarityToggles[2].isOn) rarityCheck = true;
                    break;
                case FurnitureRarity.Antique:
                    if (_rarityToggles[3].isOn) rarityCheck = true;
                    break;
            }

            float maxValue = _maxValueSlider.GetSliderValue();
            float minValue = _minValueSlider.GetSliderValue();
            bool valueCheck = (furn.Value <= maxValue && furn.Value >= minValue) || (maxValue == 0 && furn.Value >= minValue);

            // Soul home check
            if (furn.Position != new Vector2Int(-1, -1) && !_inSoulHomeToggle.isOn)
            {
                return false;
            }
            else
            {
                return setCheck && rarityCheck && valueCheck;
            }
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

            float totalValue = 0;

            foreach (var clanFurniture in clanFurnitureList)
            {
                //Debug.LogWarning(clanFurniture.GameFurnitureName);
                var gameFurnitureId = clanFurniture.GameFurnitureName;
                var furniture = allItems.FirstOrDefault(x => x.Name == gameFurnitureId);
                if (furniture == null)
                {
                    continue;
                }
                StorageFurniture storageFurniture = new(clanFurniture, furniture);

                // Take total value before filtering so it always stays the same
                totalValue += storageFurniture.Value;

                // Skip if no filters match this furniture
                if (CheckFilters(storageFurniture) == false)
                {
                    continue;
                }

                _items.Add(storageFurniture);
            }
            
            _totalValueText.text = $"{totalValue}";

            _minValueSlider.SetSliderMaxValue(allItems.Max(item => item.Value));
            _maxValueSlider.SetSliderMaxValue(allItems.Max(item => item.Value));

            Debug.Log($"found clan items {_items.Count}");
        }

        private void MakeSlots()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                GameObject newSlot = Instantiate(_invSlot, _content);
                var capturedSlotVal = i;

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

                InvSlotInfoHandler infoHandler = toSet.GetComponent<InvSlotInfoHandler>();
                infoHandler.SetSlotInfo(_furn, _sortingBy);

                ScaleSprite(_furn, infoHandler.Icon.GetComponent<RectTransform>());

                i++;
            }
        }

        public void IncrementSort() {
            if (_sortingBy < _maxSortingBy) { _sortingBy++; }
            else { _sortingBy = 0; }

            SortStored();
        }

        public void SwitchSortOrder() {
            _descendingOrder = !_descendingOrder;

            SortStored();
        }

        private void SortStored() // A very much hardcoded system for sorting, language options contain a fallback if the chosen language is null
        {
            var lang = SettingsCarrier.Instance.Language; // using helper might be a better way [switch (lang)],

            switch (_sortingBy)
            {
                case 0:
                    if (lang == SettingsCarrier.LanguageType.Finnish)
                    {
                        _sortingByText.text = "Järjestetty: Aakkoset";
                    }
                    else if (lang == SettingsCarrier.LanguageType.English)
                    {
                        _sortingByText.text = "Sorting: Alphabetical";
                    }
                    else
                    {
                        _sortingByText.text = "Sorting: Alphabetical";
                    }

                    _items.Sort((StorageFurniture a, StorageFurniture b) =>
                    {
                        StorageFurniture first = _descendingOrder ? b : a;
                        StorageFurniture second = _descendingOrder ? a : b;

                        int primaryResult = first.VisibleName.CompareTo(second.VisibleName);
                        if (primaryResult != 0) return primaryResult;

                        int idResult = first.Id.CompareTo(second.Id);
                        if (idResult != 0) return idResult;

                        return 0;
                    });
                    break;

                case 1:
                    if (lang == SettingsCarrier.LanguageType.Finnish)
                    {
                        _sortingByText.text = "Järjestetty: Arvo";
                    }
                    else if (lang == SettingsCarrier.LanguageType.English)
                    {
                        _sortingByText.text = "Sorting: Value";
                    }
                    else
                    {
                        _sortingByText.text = "Sorting: Value";
                    }

                    _items.Sort((StorageFurniture a, StorageFurniture b) =>
                    {
                        StorageFurniture first = _descendingOrder ? b : a;
                        StorageFurniture second = _descendingOrder ? a : b;

                        int primaryResult = first.Value.CompareTo(second.Value);
                        if (primaryResult != 0) return primaryResult;

                        int nameResult = first.VisibleName.CompareTo(second.VisibleName);
                        if (nameResult != 0) return nameResult;

                        int idResult = first.Id.CompareTo(second.Id);
                        if (idResult != 0) return idResult;

                        return 0;
                    });
                    break;

                case 2:
                    if (lang == SettingsCarrier.LanguageType.Finnish)
                    {
                        _sortingByText.text = "Järjestetty: Paino";
                    }
                    else if (lang == SettingsCarrier.LanguageType.English)
                    {
                        _sortingByText.text = "Sorting: Weight";
                    }
                    else
                    {
                        _sortingByText.text = "Sorting: Weight";
                    }

                    _items.Sort((StorageFurniture a, StorageFurniture b) =>
                    {
                        StorageFurniture first = _descendingOrder ? b : a;
                        StorageFurniture second = _descendingOrder ? a : b;

                        int primaryResult = first.Weight.CompareTo(second.Weight);
                        if (primaryResult != 0) return primaryResult;

                        int nameResult = first.VisibleName.CompareTo(second.VisibleName);
                        if (nameResult != 0) return nameResult;

                        int idResult = first.Id.CompareTo(second.Id);
                        if (idResult != 0) return idResult;

                        return 0;
                    });
                    break;

                case 3:
                    if (lang == SettingsCarrier.LanguageType.Finnish)
                    {
                        _sortingByText.text = "Järjestetty: Harvinaisuus";
                    }
                    else if (lang == SettingsCarrier.LanguageType.English)
                    {
                        _sortingByText.text = "Sorting: Rarity";
                    }
                    else
                    {
                        _sortingByText.text = "Sorting: Rarity";
                    }

                    _items.Sort((StorageFurniture a, StorageFurniture b) =>
                    {
                        StorageFurniture first = _descendingOrder ? b : a;
                        StorageFurniture second = _descendingOrder ? a : b;

                        int primaryResult = first.Rarity.CompareTo(second.Rarity);
                        if (primaryResult != 0) return primaryResult;

                        int nameResult = first.VisibleName.CompareTo(second.VisibleName);
                        if (nameResult != 0) return nameResult;

                        int idResult = first.Id.CompareTo(second.Id);
                        if (idResult != 0) return idResult;

                        return 0;
                    });
                    break;

                case 4:
                    if (lang == SettingsCarrier.LanguageType.Finnish)
                    {
                        _sortingByText.text = "Järjestetty: Linjasto";
                    }
                    else if (lang == SettingsCarrier.LanguageType.English)
                    {
                        _sortingByText.text = "Sorting: Set";
                    }
                    else
                    {
                        _sortingByText.text = "Sorting: Set";
                    }

                    _items.Sort((StorageFurniture a, StorageFurniture b) =>
                    {
                        StorageFurniture first = _descendingOrder ? b : a;
                        StorageFurniture second = _descendingOrder ? a : b;

                        int primaryResult = first.SetName.CompareTo(second.SetName);
                        if (primaryResult != 0) return primaryResult;

                        int nameResult = first.VisibleName.CompareTo(second.VisibleName);
                        if (nameResult != 0) return nameResult;

                        int idResult = first.Id.CompareTo(second.Id);
                        if (idResult != 0) return idResult;

                        return 0;
                    });
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
            var lang = SettingsCarrier.Instance.Language;

            string setNameLocalized; // Localizing name
            if (lang == SettingsCarrier.LanguageType.Finnish)
            {
                setNameLocalized = _furn.Info.SetName; // Finnish default (Setname)
            }
            else if (lang == SettingsCarrier.LanguageType.English)
            {
                setNameLocalized = !string.IsNullOrEmpty(_furn.Info.SetNameEnglish)
                    ? _furn.Info.SetNameEnglish
                    : _furn.Info.SetName;
            }
            else
            {
                setNameLocalized = _furn.Info.SetName; // Fallback if the language is null
            }

            // Set the furniture name based on the localization
            string furnitureNameLocalized = lang == SettingsCarrier.LanguageType.English
                ? _furn.Info.EnglishName   // Englishname for english
                : _furn.Info.VisibleName;  // Visiblename for Finnish

            // Combine set name and furniture name
            _name.text = setNameLocalized + " " + furnitureNameLocalized;

            //Artists name
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
            {
                _artist.text = _furn.Info != null
                    ? "Suunnittelu: " + _furn.Info.ArtistName
                    : "Tuntematon suunnittelija";
            }
            else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                _artist.text = _furn.Info != null
                    ? "Design: " + _furn.Info.ArtistName
                    : "Unknown Artist";
            }
            else
            {
                // Fallback if the language is Null 
                _artist.text = _furn.Info != null
                    ? "Design: " + _furn.Info.ArtistName
                    : "Unknown Artist";
            }

            //Artistic description
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
            {
                _artisticDescription.text = _furn.Info != null
                    ? _furn.Info.ArtisticDescription
                    : null;
            }
            else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                _artisticDescription.text = _furn.Info != null
                    ? _furn.Info.EnglishArtisticDescription
                    : null;
            }
            else
            {
                // Fallback if the language is null
                _artisticDescription.text = _furn.Info != null
                    ? _furn.Info.ArtisticDescription
                    : null;
            }

            // Weight
            _weight.text = _furn.Weight + " KG";

            // Diagnose number
            _diagnoseNumber.text = _furn.Info.DiagnoseNumber;

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

            _rarityText.text = _furn.Rarity.ToString();

            // Get rarity color from the selected furniture
            _rarityImage.color = _slotsList[slotVal].transform.GetChild(0).GetComponent<Image>().color;

            _sellHandler.Furniture = _furn;

            _sellHandler.UpdateInfoAction?.Invoke(_furn.ClanFurniture.InVoting);

            _infoSlot.SetActive(true);
        }

        private void UpdateInVotingText(bool inVoting)
        {
            _inVoting.SetActive(inVoting);

            SortStored(); // Called here to update InvSlotObject overlay panels
        }

        private void ScaleSprite(StorageFurniture furn, RectTransform rTransform)
        {
            rTransform.sizeDelta = new(0, 0);
            Sprite sprite = furn.Info.RibbonImage;
            if(sprite == null) sprite = furn.Sprite;
            Rect imageRect = rTransform.rect;
            if (sprite.bounds.size.x > sprite.bounds.size.y)
            {
                float diff = sprite.bounds.size.x / sprite.bounds.size.y;
                float newHeight = imageRect.height / diff;
                rTransform.sizeDelta = new(0, (newHeight - imageRect.height));
            }
            else if (sprite.bounds.size.x < sprite.bounds.size.y)
            {
                float diff = sprite.bounds.size.y / sprite.bounds.size.x;
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

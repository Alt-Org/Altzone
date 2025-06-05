using System;
using System.Collections.ObjectModel;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.CharacterGallery;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Holds methods which are used to access player data and custom character in stats window.
    /// </summary>
    public class StatsWindowController : AltMonoBehaviour
    {
        [SerializeField] private ClassColorReference _classColorReference;
        [SerializeField] private StatsReference _statsReference;
        [SerializeField] private GameObject _swipeBlocker;
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private GalleryView _galleryView;

        private PlayerData _playerData;
        private CharacterID _characterId;
        private CustomCharacter _customCharacter;
        private BaseCharacter _baseCharacter;
        private bool _unlimitedDiamonds;
        private bool _unlimitedErasers;
        private SwipeUI _swipe;

        public CharacterID CurrentCharacterID { get { return _characterId; } }
        [HideInInspector] public bool UnlimitedDiamonds {
            get
            {
                return _unlimitedDiamonds;
            }
            set
            {
                _unlimitedDiamonds = value;
                PlayerPrefs.SetInt("UnlimitedDiamonds", value ? 1 : 0);
                OnDiamondAmountChanged?.Invoke();
            }
        }

        [HideInInspector] public bool UnlimitedErasers
        {
            get
            {
                return _unlimitedErasers;
            }
            set
            {
                _unlimitedErasers = value;
                PlayerPrefs.SetInt("UnlimitedErasers", value ? 1 : 0);
                OnEraserAmountChanged?.Invoke();
            }
        }

        public event Action OnEraserAmountChanged;
        public event Action OnDiamondAmountChanged;

        public delegate void StatUpdatedHandler(StatType statType);
        public event StatUpdatedHandler OnStatUpdated;


        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            _swipe.OnCurrentPageChanged += ClosePopup;

            _statsPanel.SetActive(false);
            // Getting unlimited diamonds and erasers value from playerPrefs
            _unlimitedDiamonds = PlayerPrefs.GetInt("UnlimitedDiamonds", 0) == 1;
            _unlimitedErasers = PlayerPrefs.GetInt("UnlimitedErasers", 0) == 1;
        }


        private void OnEnable()
        {
            _characterId = SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
            SetPlayerData();
            SetCurrentCharacter();
        }

        private void OnDisable()
        {
            ClosePopup();
        }

        private void OnDestroy()
        {
            _swipe.OnCurrentPageChanged -= ClosePopup;
        }
        public void OpenPopup()
        {
            gameObject.SetActive(true);
            _swipeBlocker.SetActive(true);

            if (_statsPanel != null) _statsPanel.SetActive(true);

            if (_infoPanel != null) _infoPanel.SetActive(false);

            if (_galleryView != null) _galleryView.ShowFilterButton(false);
        }


        public void ClosePopup()
        {
            gameObject.SetActive(false);
            _swipeBlocker.SetActive(false);
            if (_galleryView != null) _galleryView.ShowFilterButton(true);
        }


        private void SetPlayerData() // Get player data from data store and set it to variable
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                _playerData = playerData;
            }));
        }


        private void SetCurrentCharacter() // Get current custom character from player data and set it to variable
        {
            _customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

            if (_customCharacter == null)
            {
                DataStore store = Storefront.Get();

                ReadOnlyCollection<BaseCharacter> allItems = null;
                store.GetAllBaseCharacterYield(result => allItems = result);

                _baseCharacter = allItems.FirstOrDefault(c => c.Id == _characterId);
            }
            else
            {
                _baseCharacter = _customCharacter.CharacterBase;
            }
        }


        /// <summary>
        /// Reload stat window data and trigger onenable function for children.
        /// </summary>
        public void ReloadStatWindow()
        {
            SetPlayerData();
            SetCurrentCharacter();

            // Triggering onenable functions
            ClosePopup();
            OpenPopup();
        }


        /// <summary>
        /// Check if current character is locked or no.
        /// </summary>
        /// <returns>True if current character is locked (player does not own it) and false if not (player owns this character).</returns>
        public bool IsCurrentCharacterLocked()
        {
            return (_customCharacter == null);
        }


        /// <summary>
        /// Get currently displayed character's class id.
        /// </summary>
        /// <returns>Current character's CharacterClassID.</returns>
        public CharacterClassID GetCurrentCharacterClass()
        {
            return CustomCharacter.GetClassID(_characterId);
        }

        /// <summary>
        /// Get currently displayed character's class name.
        /// </summary>
        /// <returns>Current character's ClassName.</returns>
        public string GetCurrentCharacterClassName()//To do: make this scritable object
        {
            switch (CustomCharacter.GetClassID(_characterId))
            {
                case CharacterClassID.Confluent:
                    return "Sulautujat";
                case CharacterClassID.Desensitizer:
                    return "Tunnottomat";
                case CharacterClassID.Intellectualizer:
                    return "ƒlyllist‰j‰t";
                case CharacterClassID.Projector:
                    return "Peilaajat";
                case CharacterClassID.Retroflector:
                    return "Torjujat";
                case CharacterClassID.Obedient:
                    return "Tottelijat";
                case CharacterClassID.Trickster:
                    return "H‰m‰‰j‰t";
                default:
                    return string.Empty;
            }
        }


        /// <summary>
        /// Get current character class alternative color from character class color reference sheet.
        /// </summary>
        /// <returns>Current character class alternative color as Color.</returns>
        public Color GetCurrentCharacterClassAlternativeColor()
        {
            CharacterClassID classID = GetCurrentCharacterClass();
            return _classColorReference.GetAlternativeColor(classID);
        }

        /// <summary>
        /// Get current character class color from character class color reference sheet.
        /// </summary>
        /// <returns>Current character class color as Color.</returns>
        public Color GetCurrentCharacterClassColor()
        {
            CharacterClassID classID = GetCurrentCharacterClass();
            return _classColorReference.GetColor(classID);
        }
        /// <summary>
        /// Get StatInfo from Reference sheet
        /// </summary>
        /// <param name="statType">The StatInfo which to get</param>
        /// <returns>Returns StatInfo object</returns>
        public StatInfo GetStatInfo(StatType statType)
        {
            return _statsReference.GetStatInfo(statType);

        }


        /// <summary>
        /// Get currently displayed character's name.
        /// </summary>
        /// <returns>Current character's name as string.</returns>
        public string GetCurrentCharacterName()
        {
            var info = PlayerCharacterPrototypes.GetCharacter(((int)_characterId).ToString());
            if (info == null)
            {
                return "";
            }
            else
            {
                return info.Name;
            }
        }


        /// <summary>
        /// Get currently displayed character's sprite image.
        /// </summary>
        /// <returns>Current character's sprite.</returns>
        public Sprite GetCurrentCharacterSprite()
        {
            var info = PlayerCharacterPrototypes.GetCharacter(((int)_characterId).ToString());
            if (info == null)
            {
                return null;
            }
            else
            {
                return info.GalleryImage;
            }
        }


        /// <summary>
        /// Get currently displayed character's description.
        /// </summary>
        /// <returns>Current character's description as string.</returns>
        public string GetCurrentCharacterDescription()
        {
            var info = PlayerCharacterPrototypes.GetCharacter(((int)_characterId).ToString());
            if (info == null)
            {
                return null;
            }
            else
            {
                return info.ShortDescription;
            }
        }


        /// <summary>
        /// Get currently displayed character's special ability description.
        /// </summary>
        /// <returns>Current character's special ability as string.</returns>
        public string GetCurrentCharacterSpecialAbilityDescription()
        {
            return "Hahmon erikoistaidon kuvaus";
        }


        /// <summary>
        /// Get currently displayed character's special ability sprite.
        /// </summary>
        /// <returns>Current character's special ability icon as Sprite.</returns>
        public Sprite GetCurrentCharacterSpecialAbilitySprite()
        {
            return null;
        }


        /// <summary>
        /// Get currently displayed character's story.
        /// </summary>
        /// <returns>Current character's story as string.</returns>
        public string GetCurrentCharacterStory()
        {
            return "Hahmon taustatarina";
        }


        /// <summary>
        /// Get currently displayed character's wins.
        /// </summary>
        /// <returns>Current character's wins as int.</returns>
        public int GetCurrentCharacterWins()
        {
            return -1;
        }


        /// <summary>
        /// Get currently displayed character's losses.
        /// </summary>
        /// <returns>Current character's losses as int.</returns>
        public int GetCurrentCharacterLosses()
        {
            return -1;
        }


        /// <summary>
        /// Get player's eraser amount from player data.
        /// </summary>
        /// <returns>Player's eraser amount as int.</returns>
        public int GetEraserAmount()
        {
            return _playerData.Eraser;
        }

        public void InvokeOnEraserAmountChanged()
        {
            OnEraserAmountChanged.Invoke();
        }


        /// <summary>
        /// Try to decrease player's eraser amount by 1.
        /// </summary>
        /// <returns>If decreasing was successful. If true, player's erasers were decreased by 1. If false, player didn't have enough erasers.</returns>
        private bool TryDecreaseEraser()
        {
            if (CheckIfEnoughErasers(1))
            {
                _playerData.Eraser--;
                OnEraserAmountChanged.Invoke();
                return true;
            }
            else
            {
                PopupSignalBus.OnChangePopupInfoSignal("Ei tarpeeksi pyyhekumeja.");
                return false;
            }
        }

        /// <summary>
        /// Checks if player has enough erasers.
        /// </summary>
        /// <param name="eraserCost">Eraser cost to check</param>
        /// <returns>True if the player does, false if doesn't</returns>
        public bool CheckIfEnoughErasers(int eraserCost)
        {
            if (_playerData.Eraser >= eraserCost || UnlimitedErasers) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Get player's diamond amount from player data.
        /// </summary>
        /// <returns>Player's diamond amount as int.</returns>
        public int GetDiamondAmount()
        {
            return _playerData.DiamondSpeed; // using DiamondSpeed as a placeholder
        }


        public void InvokeOnDiamondAmountChanged()
        {
            OnDiamondAmountChanged.Invoke();
        }


        /// <summary>
        /// Try to decrease player's diamonds by the amount specified.
        /// </summary>
        /// <param name="amount">The amount how many diamonds will be decreased from the player.</param>
        /// <returns>If decreasing was successful. If true, player's diamonds were decreased by amount. If false, player didn't have enough diamonds.</returns>
        private bool TryDecreaseDiamonds(int amount)
        {
            if (CheckIfEnoughDiamonds(amount)) 
            {
                _playerData.DiamondSpeed -= amount;
                OnDiamondAmountChanged.Invoke();
                return true;
            }
            else
            {
                PopupSignalBus.OnChangePopupInfoSignal("Ei tarpeeksi timantteja.");
                return false;
            }
        }


        /// <summary>
        /// Get how many diamonds it costs to upgrade currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which diamond cost will be checked.</param>
        /// <returns>Stat diamond upgrade cost as int.</returns>
        public int GetDiamondCost(StatType statType)
        {
            if (_customCharacter == null) return 0;

            return _customCharacter.GetPriceToNextLevel(statType);
        }
        /// <summary>
        /// Checks if player has enough diamonds.
        /// </summary>
        /// <param name="diamondCost">Diamond cost to check</param>
        /// <returns>True if the player does, false if doesn't</returns>
        public bool CheckIfEnoughDiamonds(int diamondCost) 
        {
            if (_playerData.DiamondSpeed >= diamondCost || UnlimitedDiamonds) // using DiamondSpeed as a placeholder
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get currently displayed character's stat level according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to get.</param>
        /// <returns>Stat level as int.</returns>
        public int GetStat(StatType statType)
        {
            if (_customCharacter == null) return GetBaseStat(statType);

            switch (statType)
            {
                case StatType.None:
                    return -1;
                case StatType.Speed:
                    return _customCharacter.Speed;
                case StatType.Attack:
                    return _customCharacter.Attack;
                case StatType.Hp:
                    return _customCharacter.Hp;
                case StatType.CharacterSize:
                    return _customCharacter.CharacterSize;
                case StatType.Defence:
                    return _customCharacter.Defence;
            }
            return -1;
        }


        /// <summary>
        /// Get currently displayed character's base stat without upgrades according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to get.</param>
        /// <returns>Base stat level as int.</returns>
        public int GetBaseStat(StatType statType)
        {
            switch (statType)
            {
                case StatType.None:
                    return -1;
                case StatType.Speed:
                    return _baseCharacter.DefaultSpeed;
                case StatType.Attack:
                    return _baseCharacter.DefaultAttack;
                case StatType.Hp:
                    return _baseCharacter.DefaultHp;
                case StatType.CharacterSize:
                    return _baseCharacter.DefaultCharacterSize;
                case StatType.Defence:
                    return _baseCharacter.DefaultDefence;
            }
            return -1;
        }


        /// <summary>
        /// Get currently displayed character's stat value according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which value to get.</param>
        /// <returns>Stat value as int.</returns>
        public int GetStatValue(StatType statType)
        {
            return (int)BaseCharacter.GetStatValueFP(statType, GetStat(statType));
        }

        /// <summary>
        /// Get stat value according to the stat type and level.
        /// </summary>
        /// <param name="statType">The stat type which value to get.</param>
        /// <param name="level">The stat's level.</param>
        /// <returns>Stat value as int.</returns>
        public int GetStatValue(StatType statType, int level)
        {
            return (int)BaseCharacter.GetStatValueFP(statType, level);
        }

        /// <summary>
        /// Get stat's strength.
        /// </summary>
        /// <param name="statType">The stat type which strength to get./param>
        /// <returns>ValueStrength enum value.</returns>
        public ValueStrength GetStatStrength(StatType statType)
        {
            switch (statType)
            {
                case StatType.Speed:
                    return _baseCharacter.SpeedStrength;
                case StatType.Attack:
                    return _baseCharacter.AttackStrength;
                case StatType.Hp:
                    return _baseCharacter.HpStrength;
                case StatType.CharacterSize:
                    return _baseCharacter.CharacterSizeStrength;
                case StatType.Defence:
                    return _baseCharacter.DefenceStrength;
                case StatType.None:
                default:
                    return ValueStrength.None;
            }
        }



        /// <summary>
        /// Check if stat can be increased. (all stats combined is less than the combined level cap, stat is less than individual stat level cap, and player has increased stats less times than the maximum allowed amount)
        /// </summary>
        /// <param name="statType">The stat type to check.</param>
        /// <param name="showPopupMessages">Optionally show popup error messages why stat can't be increased.</param>
        /// <returns>True if stat can be increased false if stat can't be increased.</returns>
        public bool CanIncreaseStat(StatType statType, bool showPopupMessages = false)
        {
            if (showPopupMessages)
            {
                if (!CheckCombinedLevelCap())
                {
                    PopupSignalBus.OnChangePopupInfoSignal($"Et voi p‰ivitt‰‰ taitoa, taitojen summa on enint‰‰n {CustomCharacter.STATMAXCOMBINED}.");
                }
                else if (!CheckStatLevelCap(statType))
                {
                    PopupSignalBus.OnChangePopupInfoSignal($"Et voi p‰ivitt‰‰ taitoa, maksimitaso on {CustomCharacter.STATMAXLEVEL}.");
                }
            }

            return statType != StatType.None && CheckCombinedLevelCap() && CheckStatLevelCap(statType);
        }


        /// <summary>
        /// Try to increase currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to increase.</param>
        /// <returns>If increasing the stat was successful as bool.</returns>
        public bool TryIncreaseStat(StatType statType)
        {
            if (_customCharacter == null) return false;

            bool success = false;

            if (CanIncreaseStat(statType, true))
            {
                bool diamondsDecreased = UnlimitedDiamonds || TryDecreaseDiamonds(GetDiamondCost(statType));

                if (diamondsDecreased)
                {
                    switch (statType)
                    {
                        case StatType.Speed:
                            _customCharacter.Speed++;
                            break;
                        case StatType.Attack:
                            _customCharacter.Attack++;
                            break;
                        case StatType.Hp:
                            _customCharacter.Hp++;
                            break;
                        case StatType.CharacterSize:
                            _customCharacter.CharacterSize++;
                            break;
                        case StatType.Defence:
                            _customCharacter.Defence++;
                            break;
                    }
                    success = true;
                }
            }

            if (success)
            {
                _playerData.UpdateCustomCharacter(_customCharacter);
                OnStatUpdated.Invoke(statType);
            }

            return success;
        }


        /// <summary>
        /// Check if stat can be decreased. (stat is more than minimum allowed level and stat is more than base level)
        /// </summary>
        /// <param name="statType">The stat which to check.</param>
        /// <param name="showPopupMessages">Optionally show popup error messages why stat can't be decreased.</param>
        /// <returns>True if stat can be decreased false if stat can't be decreased.</returns>
        public bool CanDecreaseStat(StatType statType, bool showPopupMessages = false)
        {
            if (showPopupMessages && !(GetStat(statType) > GetBaseStat(statType)))
            {
                PopupSignalBus.OnChangePopupInfoSignal($"Et voi v‰hent‰‰ pohjataitoa.");
            }

            return GetStat(statType) > CustomCharacter.STATMINLEVEL && GetStat(statType) > GetBaseStat(statType);
        }


        /// <summary>
        /// Try to decrease currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to decrease.</param>
        /// <returns>If decreasing the stat was successful as bool.</returns>
        public bool TryDecreaseStat(StatType statType)
        {
            if (_customCharacter == null) return false;

            bool success = false;

            if (CanDecreaseStat(statType, true))
            {
                bool eraserDecreased = UnlimitedErasers || TryDecreaseEraser();

                if (eraserDecreased)
                {
                    switch (statType)
                    {
                        case StatType.Speed:
                            _customCharacter.Speed--;
                            break;
                        case StatType.Attack:
                            _customCharacter.Attack--;
                            break;
                        case StatType.Hp:
                            _customCharacter.Hp--;
                            break;
                        case StatType.CharacterSize:
                            _customCharacter.CharacterSize--;
                            break;
                        case StatType.Defence:
                            _customCharacter.Defence--;
                            break;
                    }
                    success = true;
                }
            }

            if (success)
            {
                _playerData.UpdateCustomCharacter(_customCharacter);
                OnStatUpdated.Invoke(statType);
            }

            return success;
        }


        // Get all character stats combined
        private int GetStatsCombined()
        {
            if (_customCharacter == null) return GetBaseStatsCombined();
            return _customCharacter.Speed + _customCharacter.CharacterSize + _customCharacter.Attack + _customCharacter.Defence + _customCharacter.Hp;
        }


        // Get all character base stats combined
        private int GetBaseStatsCombined()
        {
            return _baseCharacter.DefaultSpeed + _baseCharacter.DefaultCharacterSize + _baseCharacter.DefaultAttack + _baseCharacter.DefaultDefence + _baseCharacter.DefaultHp; 
        }


        // Checks if stat levels combined are less than level cap
        private bool CheckCombinedLevelCap()
        {
            if (GetStatsCombined() < CustomCharacter.STATMAXCOMBINED)
            {
                return true;
            }
            return false;
        }


        // Checks if individual stat can be increased according to stat max level cap
        private bool CheckStatLevelCap(StatType statType)
        {
            bool allowedToIncrease = false;

            int statValue = GetStat(statType);

            if (statValue < CustomCharacter.STATMAXLEVEL)
            {
                allowedToIncrease = true;
            }

            return allowedToIncrease;
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.CharacterGallery;
using MenuUi.Scripts.Signals;
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
        [SerializeField] private ClassReference _classReference;
        [SerializeField] private StatsReference _statsReference;
        [SerializeField] private GameObject _swipeBlocker;
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private GalleryView _galleryView;

        private bool _statsUpdated = false;
        private bool _isSelected = false;
        private PlayerData _playerData;
        private CharacterID _characterId;
        private CustomCharacter _customCharacter;
        private BaseCharacter _baseCharacter;
        private SwipeUI _swipe;

        public CharacterID CurrentCharacterID => _characterId;

        public event Action OnUpgradeMaterialAmountChanged;

        public delegate void StatUpdatedHandler(StatType statType);
        public event StatUpdatedHandler OnStatUpdated;

        public delegate void DebugModeChanged();
        public event DebugModeChanged OnDebugModeChanged;


        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            _swipe.OnCurrentPageChanged += ClosePopup;

            _statsPanel.SetActive(false);
        }

        private void OnDisable()
        {
            ClosePopup();
        }

        private void OnDestroy()
        {
            _swipe.OnCurrentPageChanged -= ClosePopup;
        }

        public void OpenPopup(CharacterID characterId)
        {
            // Setting visibility to popup game objects
            gameObject.SetActive(true);
            _swipeBlocker.SetActive(true);

            // Initializing variables for stats window controller functionality
            _statsUpdated = false;
            _characterId = characterId;
            SetPlayerData();
            SetCurrentCharacter();

            // Setting visibility to panel game objects
            if (_statsPanel != null) _statsPanel.SetActive(true);

            if (_infoPanel != null) _infoPanel.SetActive(false);

            // Hiding filter button from gallery since it shouldn't be shown TODO: needs to be refactored
            if (_galleryView != null) _galleryView.ShowFilterButton(false);
        }


        public void ClosePopup()
        {
            // If stats got updated and character is currently selected, reloading character gallery so that the stats update to the selected characters
            if (_isSelected && _statsUpdated) SignalBus.OnReloadCharacterGalleryRequestedSignal();

            gameObject.SetActive(false);
            _swipeBlocker.SetActive(false);

            if (_statsPanel != null) _statsPanel.SetActive(false);
            if (_infoPanel != null) _infoPanel.SetActive(false);

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
            _isSelected = false;

            // Getting custom character from playerdata's CustomCharacters
            _customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

            // Getting base character from storefront if character is null (locked)
            if (_customCharacter == null)
            {
                DataStore store = Storefront.Get();

                ReadOnlyCollection<BaseCharacter> allItems = null;
                store.GetAllBaseCharacterYield(result => allItems = result);

                _baseCharacter = allItems.FirstOrDefault(c => c.Id == _characterId);
                return;
            }
            else // else setting base character from the _customCharacter's variable
            {
                _baseCharacter = _customCharacter.CharacterBase;
            }

            // Setting _isSelected if _customCharacter is one of the characters the player has selected. Checking also for character id for test characters 
            _isSelected = _playerData.SelectedCharacterIds.FirstOrDefault(c=> !c.IsTestCharacter ? c.ServerID == _customCharacter.ServerID : c.CharacterID == _customCharacter.Id) != null;
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
            OpenPopup(_characterId);
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
        public CharacterClassType GetCurrentCharacterClass()
        {
            return CustomCharacter.GetClass(_characterId);
        }


        /// <summary>
        /// Get currently displayed character's class name.
        /// </summary>
        /// <returns>Current character's ClassName.</returns>
        public string GetCurrentCharacterClassName()
        {
            return _classReference.GetName(CustomCharacter.GetClass(_characterId));
        }


        /// <summary>
        /// Get current character class alternative color from character class color reference sheet.
        /// </summary>
        /// <returns>Current character class alternative color as Color.</returns>
        public Color GetCurrentCharacterClassAlternativeColor()
        {
            CharacterClassType classType = GetCurrentCharacterClass();
            return _classReference.GetAlternativeColor(classType);
        }


        /// <summary>
        /// Get current character class color from character class color reference sheet.
        /// </summary>
        /// <returns>Current character class color as Color.</returns>
        public Color GetCurrentCharacterClassColor()
        {
            CharacterClassType classType = GetCurrentCharacterClass();
            return _classReference.GetColor(classType);
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


        public void InvokeOnUpgradeMaterialAmountChanged()
        {
            OnUpgradeMaterialAmountChanged.Invoke();
        }

        public void InvokeOnDebugModeChanged()
        {
            OnDebugModeChanged.Invoke();
        }


        /// <summary>
        /// Try to decrease player's upgrade material by the amount specified.
        /// </summary>
        /// <param name="amount">The amount how many upgrade material will be decreased from the player.</param>
        /// <returns>If decreasing was successful. If true, player's upgrade material was decreased by amount. If false, player didn't have enough upgrade material.</returns>
        private bool TryDecreaseUpgradeMaterial(int amount)
        {
            if (CheckIfEnoughUpgradeMaterial(amount)) 
            {
                //_playerData.DiamondSpeed -= amount;
                //OnUpgradeMaterialAmountChanged.Invoke();
                return true;
            }
            else
            {
                PopupSignalBus.OnChangePopupInfoSignal("Ei tarpeeksi kyyneliä.");
                return false;
            }
        }


        /// <summary>
        /// Get how many upgrade material it costs to upgrade currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which upgrade material cost will be checked.</param>
        /// <returns>Stat upgrade material cost as int.</returns>
        public int GetUpgradeMaterialCost(StatType statType)
        {
            if (_customCharacter == null) return 0;

            return _customCharacter.GetPriceToNextLevel(statType);
        }


        /// <summary>
        /// Checks if player has enough upgrade material.
        /// </summary>
        /// <param name="cost">Upgrade material cost to check</param>
        /// <returns>True if the player does, false if doesn't</returns>
        public bool CheckIfEnoughUpgradeMaterial(int cost) 
        {
            if (_playerData.DiamondSpeed >= cost || SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials) // using DiamondSpeed as a placeholder
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

        public Sprite GetCurrentCharacterPhotoSeries()
        {
            var info = PlayerCharacterPrototypes.GetCharacter(((int)_characterId).ToString());
            if (info == null)
            {
                return null;
            }
            else
            {
                return info.CharPhotoSeries;
            }
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
            if (!SettingsCarrier.Instance.StatDebuggingMode)
            {
                if (!CheckCombinedLevelCap())
                {
                    if (showPopupMessages) PopupSignalBus.OnChangePopupInfoSignal($"Et voi päivittää taitoa, taitojen summa on enintään {CustomCharacter.STATMAXCOMBINED}.");
                    return false;
                }
                else if (GetStatStrength(statType) == ValueStrength.None)
                {
                    if (showPopupMessages) PopupSignalBus.OnChangePopupInfoSignal($"Tätä taitoa ei voi muokata.");
                    return false;
                }
            }
            if (!CheckStatLevelCap(statType))
            {
                if (showPopupMessages) PopupSignalBus.OnChangePopupInfoSignal($"Et voi päivittää taitoa, maksimitaso on {CustomCharacter.STATMAXLEVEL}.");
                return false;
            }

            return statType != StatType.None;
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
                bool diamondsDecreased = SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials || TryDecreaseUpgradeMaterial(GetUpgradeMaterialCost(statType));

                if (diamondsDecreased)
                {
                    success = _customCharacter.IncreaseStat(statType);
                }
            }

            if (success)
            {
                _statsUpdated = true;
                _playerData.UpdateCustomCharacter(_customCharacter);
                OnUpgradeMaterialAmountChanged.Invoke();
                OnStatUpdated.Invoke(statType);
                gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
                switch (statType)
                {
                    case StatType.Attack:
                        if (TaskEducationActionType.MakeCharacterStrong == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationActionType
                            && _customCharacter.Attack == CustomCharacter.STATMAXLEVEL)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationActionType.MakeCharacterStrong, "1");
                        break;
                    case StatType.Defence:
                        if (TaskEducationActionType.MakeCharacterDurable == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationActionType
                            && _customCharacter.Defence == CustomCharacter.STATMAXLEVEL)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationActionType.MakeCharacterDurable, "1");
                        break;
                    case StatType.CharacterSize:
                        if (TaskEducationActionType.MakeCharacterBig == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationActionType
                            && _customCharacter.CharacterSize == CustomCharacter.STATMAXLEVEL)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationActionType.MakeCharacterBig, "1");
                        break;
                    case StatType.Hp:
                        if (TaskEducationActionType.MakeCharacterDurable == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationActionType
                            && _customCharacter.Hp == CustomCharacter.STATMAXLEVEL)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationActionType.MakeCharacterDurable, "1");
                        break;
                    case StatType.Speed:
                        if (TaskEducationActionType.MakeCharacterFast == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationActionType
                            && _customCharacter.Speed == CustomCharacter.STATMAXLEVEL)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationActionType.MakeCharacterFast, "1");
                        break;
                    default:
                        break;
                }
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
            if(!SettingsCarrier.Instance.StatDebuggingMode){
                if (showPopupMessages && !(GetStat(statType) > GetBaseStat(statType)))
                {
                    PopupSignalBus.OnChangePopupInfoSignal($"Et voi vähentää pohjataitoa.");
                    return false;
                }
                else if (GetStatStrength(statType) == ValueStrength.None)
                {
                    PopupSignalBus.OnChangePopupInfoSignal($"Tätä taitoa ei voi muokata.");
                    return false;
                }
            }
            if (GetStat(statType) <= CustomCharacter.STATMINLEVEL)
            {
                if (showPopupMessages) PopupSignalBus.OnChangePopupInfoSignal($"Et voi päivittää taitoa, minimi on {CustomCharacter.STATMINLEVEL}.");
                return false;
            }

            return GetStat(statType) > CustomCharacter.STATMINLEVEL;
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
                success = _customCharacter.DecreaseStat(statType);
            }

            if (success)
            {
                _statsUpdated = true;
                _playerData.UpdateCustomCharacter(_customCharacter);
                OnStatUpdated.Invoke(statType);
                gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
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

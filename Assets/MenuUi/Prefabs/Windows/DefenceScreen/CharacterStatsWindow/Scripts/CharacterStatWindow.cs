using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Prefabs.Windows.DefenceScreen;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using System.Linq;
using MenuUi.Scripts.CharacterGallery;
using System;
using Altzone.Scripts.Config;


public class CharacterStatWindow : MonoBehaviour
{

    public Sprite[] CharacterArtWork;
    public Image CharacterArtWorkToShow;

    private int UnusedStats;

    private int DiamondSpeedAmount = 100;
    private int DiamondResistanceAmount = 100;
    private int DiamondAttackAmount = 100;
    private int DiamondDefenceAmount = 100;
    private int DiamondHPAmount = 100;
    private int EraserAmount = 100;

    private int SpeedCostAmount = 5;
    private int ResistanceCostAmount = 5;
    private int AttackCostAmount = 5;
    private int DefenceCostAmount = 5;
    private int HPCostAmount = 5;

    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CustomCharacterName;

    [Header("Current stat level?")]
    public TextMeshProUGUI SpeedNumber;
    public TextMeshProUGUI ResistanceNumber;
    public TextMeshProUGUI AttackNumber;
    public TextMeshProUGUI DefenceNumber;
    public TextMeshProUGUI HPNumber;

    [Header("Amount of diamonds that can be used")]
    public TextMeshProUGUI DiamondSpeedAmountNumber;
    public TextMeshProUGUI DiamondResistanceAmountNumber;
    public TextMeshProUGUI DiamondAttackAmountNumber;
    public TextMeshProUGUI DiamondDefenceAmountNumber;
    public TextMeshProUGUI DiamondHPAmountNumber;
    public TextMeshProUGUI EraserAmountNumber;

    [Header("Descriptions")]
    public TextMeshProUGUI CharDescription;//hahmon kuvaus
    public TextMeshProUGUI DefClassSpecial;//defenssiluokan erikoistaidon kuvaus


    private DemoCharacterForStatWindow _demoCharacterWindowCharacter;

    private int CurrentlySelectedStat = -1;
    [Header("Increase and decrease buttons")]
    [SerializeField] public Button statImpactoforceIncreaseButton;
    [SerializeField] public Button statImpactforceDecreaseButton;
   
    [SerializeField] public TextMeshProUGUI UpgradeCostAmountNumber;
    [SerializeField] private Image UpgradeDiamondImage;

/*     [SerializeField] private Image _statSpeedSelectedBackground;
    [SerializeField] private Image _statResistanceSelectedBackground;
    [SerializeField] private Image _statAttackSelectedBackground;
    [SerializeField] private Image _statDefenceSelectedBackground;
    [SerializeField] private Image _statHPSelectedBackground; */

    [SerializeField] private GalleryCharacterReference _galleryCharacterReference;

    //private BaseCharacter _currentCharacter;
    private PlayerData _playerData;
    private CharacterID _characterId;

    //Nouseeko progressbarin arvo siinä tilanteessa kun ostetaan timanteilla uusi taso?
    //Kun ostetaan uusi taso palkki nousee ja kun se menee täyteen niin tulee uusi leveli.

    //Mistä löytyy hahmonkuvaus? -saatavilla, jahka valmistuu
    //Mistä löytyy defenssiluokan kuvaus? -saatavilla, jahka valmistuu

    //Onko olemassa jo tieto käytetyistä tasopykälistä jossain?
    //Ei ole. Palataan myöhemmin.

    //Virheilmoitus rivillä 86, mikä sen aiheuttaa? Onko meistä riippumaton asia?
    //Mitä tarkoittaa stat selected backround?
    //Pitääkö tähän lisätä jokaisen stattipaneelin plus ja miinusnapit. Onko helpompi tehdä niin?

    //Luokan saa customcharacter get character id

    private void OnEnable()
    {
        

        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange += HandleCharacterGalleryCharacterStatWindowToShowChange;
        Debug.Log("CharacterStatWindow enabled");

        // Hae CustomCharacter tiedot PlayerDatasta
        _characterId = (CharacterID)SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
        Debug.Log($"Searching for character with ID: {_characterId}");
        
        DataStore dataStore = Storefront.Get();
        dataStore.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData =>

        //Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, playerData =>
        {
            if (playerData == null)
            {

                Debug.Log("GetPlayerData is null");
                return;
            }

            Debug.Log("PlayerData tiedot haettu onnistuneesti");
            _playerData = playerData;

            DiamondSpeedAmount = playerData.DiamondSpeed;
            DiamondResistanceAmount = playerData.DiamondResistance;
            DiamondAttackAmount = playerData.DiamondAttack;
            DiamondDefenceAmount = playerData.DiamondDefence;
            DiamondHPAmount = playerData.DiamondHP;
            EraserAmount = playerData.Eraser;

            Debug.Log("Timanttiarvot asetettu onnistuneesti");

        });
        HandleCharacterGalleryCharacterStatWindowToShowChange(SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow);

    }


    private void OnDisable()
    {
        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange -= HandleCharacterGalleryCharacterStatWindowToShowChange;
        Debug.Log("CharacterStatWindow disabled");
    }

    private void HandleCharacterGalleryCharacterStatWindowToShowChange(CharacterID newValue)
    {
        _decideWhatCharacterToShow(newValue);
        SetCharacterStats();
        Debug.Log("handled window change");
    }

   
    //  upgrade
    private void UpgradeCharacterSpeed()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondSpeedAmount >= SpeedCostAmount)
            {
                DiamondSpeedAmount -= SpeedCostAmount;
                _playerData.DiamondSpeed = DiamondSpeedAmount;
                DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondSpeedAmount.ToString() + "/" + SpeedCostAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed += 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
            

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Speed = _demoCharacterWindowCharacter.CharacterSpeed;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }



    private void UpgradeCharacterResistance()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondResistanceAmount >= ResistanceCostAmount)
            {
                DiamondResistanceAmount -= ResistanceCostAmount;
                _playerData.DiamondResistance = DiamondResistanceAmount;
                DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondResistanceAmount.ToString() + "/" + ResistanceCostAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance += 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Resistance = _demoCharacterWindowCharacter.CharacterResistance;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterAttack()
    {
        if (CheckMaxLevel() == true)
        {

            if (DiamondAttackAmount >= AttackCostAmount)
            {
                DiamondAttackAmount -= AttackCostAmount;
                _playerData.DiamondAttack = DiamondAttackAmount;
                DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondAttackAmount.ToString() + "/" + AttackCostAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack += 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Attack = _demoCharacterWindowCharacter.CharacterAttack;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterDefence()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondDefenceAmount >= DefenceCostAmount)
            {
                DiamondDefenceAmount -= DefenceCostAmount;
                _playerData.DiamondDefence = DiamondDefenceAmount;
                DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondDefenceAmount.ToString() + "/" + DefenceCostAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence += 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Defence = _demoCharacterWindowCharacter.CharacterDefence;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterHP()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondHPAmount >= HPCostAmount)
            {
                DiamondHPAmount -= HPCostAmount;
                _playerData.DiamondHP = DiamondHPAmount;
                DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondHPAmount.ToString() + "/" + HPCostAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP += 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Hp = _demoCharacterWindowCharacter.CharacterHP;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }

    // degrade
    private void DegradeCharacterSpeed()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterSpeed > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed -= 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Speed = _demoCharacterWindowCharacter.CharacterSpeed;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterResistance()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterResistance > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance -= 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Resistance = _demoCharacterWindowCharacter.CharacterResistance;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }

    private void DegradeCharacterAttack()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterAttack > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack -= 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Attack = _demoCharacterWindowCharacter.CharacterAttack;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterDefence()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterDefence > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence -= 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Defence = _demoCharacterWindowCharacter.CharacterDefence;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterHP()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterHP > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP -= 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Hp = _demoCharacterWindowCharacter.CharacterHP;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }


    // doing this at awake                                     
    private void _decideWhatCharacterToShow(CharacterID _characterId) //index
    {
        // Etsi CustomCharacter -tiedot valitulle hahmolle

        //Metodia muutettu niin, että nyt se käyttää valmiiksi asetettua _characterId -muuttujaa. Vanhat koodit kommentoitu pois.
        //Aikaisemmin käytetty muuttujaa "index".

        //var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == index);
        var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
        if (customCharacter == null)
        {                                                          
            Debug.LogError($"CustomCharacter not found for index {_characterId}");  //index
            _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
            CharacterArtWorkToShow.sprite = CharacterArtWork[0];
            return;
        }

        //var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)index);
        var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)_characterId);
        if (galleryCharacter == null)
        {
                                                                            
            Debug.LogError($"GalleryCharacterReference not found for index {_characterId}"); //index
            _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
            CharacterArtWorkToShow.sprite = CharacterArtWork[0];
            return;
        }


        
        switch (_characterId) //index
        {
            //Kutsutaan metodia joka casessa
            case CharacterID.IntellectualizerResearcher:
                SetCharacterInfo();
                break;
            case CharacterID.RetroflectorOvereater:
                SetCharacterInfo();
                break;
            case CharacterID.TricksterComedian:
                SetCharacterInfo();
                break;
            case CharacterID.TricksterConman:
                SetCharacterInfo();
                break;
            case CharacterID.DesensitizerBodybuilder:
                SetCharacterInfo();
                break;
            case CharacterID.ObedientPreacher:
                SetCharacterInfo();
                break;
            case CharacterID.ProjectorGrafitiartist:
                SetCharacterInfo();
                break;
            case CharacterID.ConfluentBesties:
                SetCharacterInfo();
                break;
            case CharacterID.RetroflectorAlcoholic:
                SetCharacterInfo();
                break;
            default:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
                CharacterArtWorkToShow.sprite = CharacterArtWork[0];
                break;
        }
    }

    private void SetCharacterStats()
    {
        if (_demoCharacterWindowCharacter != null)
        {
            CharacterName.text = _demoCharacterWindowCharacter.CharacterName;

            SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
            ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
            AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
            DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
            HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();

            //DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
            DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
            DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
            DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
            DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
            //EraserAmountNumber.text = EraserAmount.ToString();

            UpdatePieChart();
            //UpdateUpgradeButtons();
            //DisableAllStatSelectedBackground();
            Debug.Log("CharacterGallery SetCharacterStats ran");
        }
        else
        {
            Debug.Log("Demo Character Class empty");
        }
    }

    // Checks if levels combined are less than level cap
    private bool CheckMaxLevel()
    {
        if (_demoCharacterWindowCharacter.CharacterSpeed + _demoCharacterWindowCharacter.CharacterResistance + _demoCharacterWindowCharacter.CharacterAttack + _demoCharacterWindowCharacter.CharacterDefence + _demoCharacterWindowCharacter.CharacterHP < 100)
        {
            return true;
        }
        return false;
    }

    // Is used to get an integer for the grey part in pie chart
    public int CheckUnusedStatsAmount()
    {
        UnusedStats = 100 - (_demoCharacterWindowCharacter.CharacterSpeed + _demoCharacterWindowCharacter.CharacterResistance + _demoCharacterWindowCharacter.CharacterAttack + _demoCharacterWindowCharacter.CharacterDefence + _demoCharacterWindowCharacter.CharacterHP);
        return UnusedStats;
    }

    // update pie chart
    private void UpdatePieChart()
    {
        FindObjectOfType<CharacterStatsPieChart>().SetPieChartValues(CharacterStatsIntValuesToFloatValues(_demoCharacterWindowCharacter.CharacterSpeed, _demoCharacterWindowCharacter.CharacterResistance, _demoCharacterWindowCharacter.CharacterAttack, _demoCharacterWindowCharacter.CharacterDefence, _demoCharacterWindowCharacter.CharacterHP, CheckUnusedStatsAmount()));
    }

    private float[] CharacterStatsIntValuesToFloatValues(int characterSpeed, int characterResistance, int characterAttack, int characterDefence, int characterHP, int unusedStats)
    {
        // pie chart works with the floats being in reverse order compared to the image array which holds the pie chart circles, atleast for now 
        float[] characterStatsFloatValues = { characterHP, characterDefence, characterAttack, characterResistance, characterSpeed, unusedStats };
        return characterStatsFloatValues;
    }
    private void SetCharacterInfo()
    {
       //Tällä haetaan hahmon tiedot statti-ikkunaan. Toimivuudesta ei vielä ole varmuutta.

        var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

        var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)_characterId);


        //Timanttien määrän saa playerdatasta
        int DiamondSpeedAmount = _playerData.DiamondSpeed;
        int diamondAttackAmount = _playerData.DiamondAttack;
        int diamondDefenceAmount = _playerData.DiamondDefence;
        int diamondHPAmount = _playerData.DiamondHP;
        //pyyhekumien määrän saa playerdatasta
        int eraserAmount = _playerData.Eraser;


        _demoCharacterWindowCharacter = new DemoCharacterForStatWindow(galleryCharacter.Name, false,
                   customCharacter.Speed, customCharacter.Resistance, customCharacter.Attack, customCharacter.Defence, customCharacter.Hp);
        CharacterArtWorkToShow.sprite = galleryCharacter.Image;
        Debug.Log($"loaded {galleryCharacter.Name}");
    }



    // Character Name. DISABLED FOR THE SAKE OF TESTERS
    /*
    public void CharacterNameChange()
    {
        _demoCharacterWindowCharacter.CharacterName = CharacterName.text;
    }
    */
}

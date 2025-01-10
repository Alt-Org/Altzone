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

public class CharInfo : MonoBehaviour
{

    public Sprite[] CharacterArtWork;
    public Image CharacterArtWorkToShow;

    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CustomCharacterName;


    [Header("Descriptions")]
    public TextMeshProUGUI CharDescription;//hahmon kuvaus
    public TextMeshProUGUI DefClassSpecial;//defenssiluokan erikoistaidon kuvaus

    private DemoCharacterForStatWindow _demoCharacterWindowCharacter;


    [SerializeField] private GalleryCharacterReference _galleryCharacterReference;

    //private BaseCharacter _currentCharacter;
    private PlayerData _playerData;
    private CharacterID _characterId;

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
        Debug.Log("handled window change");
    }

    private void UpdateCharacterInfo()
    {
        // Varmista, että data on saatavilla
        if (_playerData == null || _galleryCharacterReference == null)
        {
            Debug.LogWarning("Player data or gallery character reference is not set");
            return;
        }

        _decideWhatCharacterToShow(_characterId);
    }


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
            case CharacterID.ProjectorGraffitiArtist:
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
}

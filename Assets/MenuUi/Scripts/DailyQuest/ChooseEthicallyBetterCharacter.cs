using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseEthicallyBetterCharacter : DailyTaskProgressListener
{
    [SerializeField] private GameObject _popup;
    [SerializeField] private Image _characterImage1;
    [SerializeField] private Image _characterImage2;
    [SerializeField] private TextMeshProUGUI _description1;
    [SerializeField] private TextMeshProUGUI _description2;
    [SerializeField] private Button _chooseButton1;
    [SerializeField] private Button _chooseButton2;

    private List<string> _characterIDs = new List<string>();
    private int _charactersSelected = 0;

    protected override void Start()
    {
        base.Start();

        if (On)
        {
            SetupTaskPopup();
        }
    }

    public override void SetState(PlayerTask task)
    {
        base.SetState(task);

        if (On)
        {
            SetupTaskPopup();
        }
    }

    private void SetupTaskPopup()
    {
        _chooseButton1.onClick.RemoveAllListeners();
        _chooseButton2.onClick.RemoveAllListeners();

        _chooseButton1.onClick.AddListener(OnCharacterSelection);
        _chooseButton2.onClick.AddListener(OnCharacterSelection);

        // Pick every character that isn't a test character, has a short description and image
        foreach (var prototype in PlayerCharacterPrototypes.Prototypes)
        {
            if (prototype.Id[prototype.Id.Length - 1] != '0' && !string.IsNullOrEmpty(prototype.ShortDescription) && prototype.GalleryImage != null)
                _characterIDs.Add(prototype.Id);
        }

        UpdateCharacters();

        _popup.SetActive(true);
    }

    private void OnCharacterSelection()
    {
        _charactersSelected++;

        if (_charactersSelected >= 3)
        {
            UpdateProgress("1");
            _charactersSelected = 0;
            _popup.SetActive(false);
        }
        else
        {
            UpdateCharacters();
        }
    }

    private void UpdateCharacters()
    {
        int index = Random.Range(0, _characterIDs.Count);
        var character1 = PlayerCharacterPrototypes.GetCharacter(_characterIDs[index]);
        _characterIDs.RemoveAt(index);

        index = Random.Range(0, _characterIDs.Count);
        var character2 = PlayerCharacterPrototypes.GetCharacter(_characterIDs[index]);
        _characterIDs.RemoveAt(index);

        _characterImage1.sprite = character1.GalleryImage;
        _characterImage2.sprite = character2.GalleryImage;

        _description1.text = character1.ShortDescription;
        _description2.text = character2.ShortDescription;
    }
}

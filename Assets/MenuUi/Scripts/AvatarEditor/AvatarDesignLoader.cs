using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

public class AvatarDesignLoader : AltMonoBehaviour
{
    public static AvatarDesignLoader Instance { get; private set; }

    [SerializeField] private float _timeoutSeconds = 10f;
    [Space]
    [SerializeField] private AvatarPartsReference _avatarPartsReference;
    [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
    [Space]
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisualDataScriptableObject;

    public delegate void AvatarDesignUpdate();
    public static event AvatarDesignUpdate OnAvatarDesignUpdate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        StartCoroutine(LoadAvatarDesign());
    }

    private IEnumerator LoadAvatarDesign()
    {
        bool? timeout = null;
        PlayerData playerData = null;

        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => playerData = data));

        yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

        if (playerData == null)
            yield break;

        List<Sprite> sprites = new List<Sprite>();
        PlayerAvatar playerAvatar = null;

        if (playerData.AvatarData == null || !playerData.AvatarData.Validate())
        {
            Debug.Log("AvatarData is null. Using default data.");
            playerAvatar = new(_avatarDefaultReference.GetByCharacterId(playerData.SelectedCharacterId)[0]);
            playerData.AvatarData = new(playerAvatar.Name, playerAvatar.FeatureIds, playerAvatar.Color, playerAvatar.Scale);
        }

        List<AvatarPiece> pieceIDs = Enum.GetValues(typeof(AvatarPiece)).Cast<AvatarPiece>().ToList();
        foreach (AvatarPiece id in pieceIDs)
        {
            int pieceId= playerData.AvatarData.GetPieceID(id);
            var partInfo = _avatarPartsReference.GetAvatarPartById(pieceId.ToString());
            if (partInfo != null)
                _avatarVisualDataScriptableObject.SetAvatarPiece(id, partInfo.AvatarImage);
            else
                _avatarVisualDataScriptableObject.SetAvatarPiece(id, null);
        }

        Color color = Color.white;
        ColorUtility.TryParseHtmlString(playerData.AvatarData.Color,out color);

        _avatarVisualDataScriptableObject.color = color;

        InvokeOnAvatarDesignUpdate();
    }

    public void InvokeOnAvatarDesignUpdate()
    {
        OnAvatarDesignUpdate?.Invoke();
    }
}

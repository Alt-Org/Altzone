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

    [SerializeField] private AvatarEditorController _avatarEditorController; //The reference for the avatar editor controller, used to get the reference to the _playerAvatar

    public delegate void AvatarDesignUpdate();
    public static event AvatarDesignUpdate OnAvatarDesignUpdate;

    
    private static readonly AvatarPiece[] AllAvatarPieces =
        Enum.GetValues(typeof(AvatarPiece)).Cast<AvatarPiece>().ToArray();

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        StartCoroutine(LoadAvatarDesignCoroutine());
    }

    #endregion

    #region Singleton Management

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Avatar Design Loading

    private IEnumerator LoadAvatarDesignCoroutine()
    {
        bool? timeout = null;
        PlayerData playerData = null;

        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds,
            timeoutResult => timeout = timeoutResult,
            dataResult => playerData = dataResult));

        yield return new WaitUntil(() => timeout.HasValue || playerData != null);

        if (playerData == null)
        {
            Debug.LogWarning("Failed to load player data within timeout period.");
            yield break;
        }

        var avatarVisualData = CreateAvatarVisualData(playerData);
        if (avatarVisualData == null)
        {
            Debug.LogError("Failed to create avatar visual data.");
            yield break;
        }

        ApplyAvatarVisualData(avatarVisualData);
        InvokeOnAvatarDesignUpdate();
    }

    private void ApplyAvatarVisualData(AvatarVisualData avatarVisualData)
    {
        foreach (var pieceId in AllAvatarPieces)
        {
            _avatarVisualDataScriptableObject.SetAvatarPiece(pieceId, avatarVisualData.GetAvatarPiece(pieceId));
        }

        _avatarVisualDataScriptableObject.Color = avatarVisualData.Color;
    }

    #endregion

    #region Avatar Visual Data Creation

    public AvatarVisualData CreateAvatarVisualData(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData is null.");
            return null;
        }

        EnsureValidAvatarData(playerData);

        var avatarVisualData = new AvatarVisualData();
        PopulateAvatarPieces(avatarVisualData, playerData.AvatarData);
        SetAvatarColor(avatarVisualData, playerData.AvatarData);

        return avatarVisualData;
    }

    public AvatarVisualData CreateAvatarVisualData(AvatarData avatarData)
    {
        if (avatarData == null)
        {
            Debug.LogError("AvatarData is null.");
            return null;
        }

        //EnsureValidAvatarData(playerData);

        var avatarVisualData = new AvatarVisualData();
        PopulateAvatarPieces(avatarVisualData, avatarData);
        SetAvatarColor(avatarVisualData, avatarData);

        return avatarVisualData;
    }

    private void EnsureValidAvatarData(PlayerData playerData)
    {
        if (playerData.AvatarData?.Validate() == true)
            return;

        Debug.Log("AvatarData is null or invalid. Using default data.");

        var defaultAvatars = _avatarDefaultReference.GetByCharacterId(playerData.SelectedCharacterId);
        if (defaultAvatars == null || defaultAvatars.Count == 0)
        {
            Debug.LogError($"No default avatar found for character ID: {playerData.SelectedCharacterId}");
            return;
        }

        var playerAvatar = _avatarEditorController.PlayerAvatar;
        //old declaration -> new PlayerAvatar(defaultAvatars[0]);

        //MCGYVERED TOGETHER NEEDS TO CHANGE VVVVVVVVVVVVVVV
        List<string> featureIds = new List<string>();
        var list = Enum.GetValues(typeof(FeatureSlot));
        foreach (FeatureSlot feature in list)
        {
            featureIds.Add(playerAvatar.GetPartId(feature));
            Debug.Log("The added featureId is " + playerAvatar.GetPartId(feature));
        }
        //END OF MCGYVERING

        playerData.AvatarData = new(
            playerAvatar.Name,
            featureIds,
            playerAvatar.Color,
            playerAvatar.Scale
        );
    }

    private void PopulateAvatarPieces(AvatarVisualData avatarVisualData, AvatarData avatarData)
    {
        foreach (var pieceId in AllAvatarPieces)
        {
            var pieceIdValue = avatarData.GetPieceID(pieceId);
            var partInfo = _avatarPartsReference.GetAvatarPartById(pieceIdValue.ToString());

            var avatarImage = partInfo?.AvatarImage;
            avatarVisualData.SetAvatarPiece(pieceId, avatarImage);
        }
    }

    private static void SetAvatarColor(AvatarVisualData avatarVisualData, AvatarData avatarData)
    {
        var color = Color.white;

        if (!string.IsNullOrEmpty(avatarData.Color))
        {
            if (!ColorUtility.TryParseHtmlString(avatarData.Color, out color))
            {
                Debug.LogWarning($"Failed to parse color: {avatarData.Color}. Using white as default.");
                color = Color.white;
            }
        }

        avatarVisualData.Color = color;
    }

    #endregion

    #region Public Methods

  
    [Obsolete("Use CreateAvatarVisualData instead.")]
    public AvatarVisualData LoadAvatarDesign(PlayerData playerData)
    {
        return CreateAvatarVisualData(playerData);
    }

    public void InvokeOnAvatarDesignUpdate()
    {
        OnAvatarDesignUpdate?.Invoke();
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion
}


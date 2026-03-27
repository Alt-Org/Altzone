using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
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

    private bool _loginStatusChanged = false;
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
        ServerManager.OnLogInStatusChanged += UpdateOnLoginStatusChanged;
    }

    private void OnEnable()
    {
        if (_loginStatusChanged)
        {
            _loginStatusChanged = false;
            StartCoroutine(LoadAvatarDesignCoroutine());
        }
    }

    private void UpdateOnLoginStatusChanged(bool isLoggedIn)
    {
        _loginStatusChanged = isLoggedIn;
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
            _avatarVisualDataScriptableObject.SetColor(pieceId, avatarVisualData.GetColor(pieceId));
        }

        _avatarVisualDataScriptableObject.SkinColor = avatarVisualData.SkinColor;
        _avatarVisualDataScriptableObject.ClassColor = avatarVisualData.ClassColor;
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
        int classId = BaseCharacter.GetClass(playerData.SelectedCharacterId);
        SetAvatarColor(avatarVisualData, playerData.AvatarData, classId);

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
        int? classId = ServerManager.Instance.Player.currentAvatarId != null? BaseCharacter.GetClass((int)ServerManager.Instance.Player.currentAvatarId) : null;
        SetAvatarColor(avatarVisualData, avatarData, classId);

        return avatarVisualData;
    }

    private void EnsureValidAvatarData(PlayerData playerData)
    {
        AvatarData avatarData = playerData?.AvatarData;
        List<AvatarPiece> invalidPieces = null;
        List<AvatarPiece> invalidColors = null;

        if (avatarData != null)
        {
            invalidPieces = GetInvalidAvatarPieces(_avatarPartsReference, avatarData);
            invalidColors = GetInvalidAvatarPieceColors(avatarData);
        }

        if (invalidPieces?.Count == 0 && invalidColors?.Count == 0)
        {
            //Debug.LogError($"Player {playerData.Name} - all pieces valid. ");
            return;
        }

        var defaultAvatars = _avatarDefaultReference.GetAvatar(playerData.SelectedCharacterId);
        if (defaultAvatars == null)
        {
            Debug.LogError($"No default avatar found for character ID: {playerData.SelectedCharacterId}");
            return;
        }
        AvatarData defaultAvatarData = new(defaultAvatars);

        if (avatarData != null)
        {
            var replacedPieces = new System.Text.StringBuilder();
            foreach (AvatarPiece piece in invalidPieces)
            {
                var oldId = playerData.AvatarData?.GetPieceID(piece);
                playerData.AvatarData?.SetPieceID(piece, defaultAvatarData.GetPieceID(piece));
                var newId = playerData.AvatarData?.GetPieceID(piece);
                replacedPieces.Append($"{piece}:{oldId} to {newId}  ");
            }

            var replacedColors = new System.Text.StringBuilder();
            foreach (AvatarPiece piece in invalidColors)
            {
                var oldcolor = playerData.AvatarData?.GetPieceColor(piece);
                playerData.AvatarData?.SetPieceColor(piece, defaultAvatarData.GetPieceColor(piece));
                var newColor = playerData.AvatarData?.GetPieceColor(piece);
                replacedColors.Append($"{piece}:{oldcolor} to {newColor}  ");
            }
            Debug.LogWarning($"Player {playerData.Name} - replaced {invalidPieces.Count} piece(s): {replacedPieces} and {invalidColors.Count} color(s): {replacedColors}");
        }
        else
        {
            playerData.AvatarData = defaultAvatarData;
            avatarData = defaultAvatarData;
            Debug.LogWarning($"Player {playerData.Name} - AvatarData was null, assigned full default.");
        }

        var playerAvatar = _avatarEditorController?.PlayerAvatar;
        if (playerAvatar == null)
        {
            playerAvatar = new PlayerAvatar(avatarData);
        }

        var list = Enum.GetValues(typeof(AvatarPiece));
        /*foreach (AvatarPiece feature in list) //This could possibly be replaced with turning the partlist into ServerAvatar and then giving that to the AvatarData.
        {
            playerData.AvatarData.SetPieceID((AvatarPiece)feature, int.Parse(playerAvatar.GetPartId(feature)));
            Debug.Log("The added featureId is " + playerAvatar.GetPartId(feature));
            playerData.AvatarData.SetPieceColor(feature, playerAvatar.GetPartColor(feature));
        }*/
        //}

    }

    public bool ValidateAvatarPiece(AvatarPiece piece, AvatarPartsReference partsReference, AvatarData avatarData)
    {
        int pieceId = avatarData.GetPieceID(piece);
        string pieceIdString = pieceId.ToString();
        // I assume the length should always be 7 but I'm not sure
        if (pieceIdString.Length < 4) return false;
        return partsReference.GetAvatarPartById(pieceIdString) != null;
    }

    public List<AvatarPiece> GetInvalidAvatarPieces(AvatarPartsReference partsReference, AvatarData avatarData)
    {
        List<AvatarPiece> invalidPieces = new();

        foreach (AvatarPiece piece in AllAvatarPieces)
        {
            if (!ValidateAvatarPiece(piece, partsReference, avatarData))
            {
                invalidPieces.Add(piece);
            }
        }
        return invalidPieces;
    }

    public List<AvatarPiece> GetInvalidAvatarPieceColors(AvatarData avatarData)
    {
        List<AvatarPiece> invalidColors = new();

        foreach (AvatarPiece piece in AllAvatarPieces)
        {
            string color = avatarData.GetPieceColor(piece);
            if (!ColorUtility.TryParseHtmlString(color, out _))
                invalidColors.Add(piece);
        }
        return invalidColors;
    }

    private void PopulateAvatarPieces(AvatarVisualData avatarVisualData, AvatarData avatarData)
    {
        foreach (var pieceId in AllAvatarPieces)
        {
            var pieceIdValue = avatarData.GetPieceID(pieceId);

            if (pieceIdValue == 0)
            {
                continue;
            }

            var partInfo = _avatarPartsReference.GetAvatarPartById(pieceIdValue.ToString());

            var avatarImage = partInfo;
            avatarVisualData.SetAvatarPiece(pieceId, avatarImage);
            ColorUtility.TryParseHtmlString(avatarData.GetPieceColor(pieceId), out Color color);
            avatarVisualData.SetColor(pieceId, color);
        }
    }

    private static void SetAvatarColor(AvatarVisualData avatarVisualData, AvatarData avatarData, int? classId)
    {
        var color = Color.white;

        string correctedColor = avatarData.Color;
        if (!correctedColor.StartsWith("#"))
        {
            correctedColor = "#" + correctedColor;
        }

        if (!ColorUtility.TryParseHtmlString(correctedColor, out color))
        {
            Debug.LogWarning($"Failed to parse color: {avatarData.Color}. Using white as default.");
            color = Color.white;
        }

        avatarVisualData.SkinColor = color;
        if (classId != null)
        {
            avatarVisualData.ClassColor = ClassReference.Instance.GetColor(BaseCharacter.GetClass((CharacterID)classId));
        }
        else
        {
            avatarVisualData.ClassColor = Color.white;
        }
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
        ServerManager.OnLogInStatusChanged -= UpdateOnLoginStatusChanged;
    }

    #endregion
}


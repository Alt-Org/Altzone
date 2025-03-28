using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
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
        List<Color> colors = new List<Color>();
        PlayerAvatar playerAvatar = null;

        if (playerData.AvatarData == null)
        {
            Debug.Log("AvatarData is null. Using default data.");
            playerAvatar = new(_avatarDefaultReference.GetByCharacterId(playerData.SelectedCharacterId)[0]);

            playerAvatar.Colors.Add("#ffffff");
        }
        else
            playerAvatar = new(playerData.AvatarData);

        foreach (string id in playerAvatar.FeatureIds)
        {
            var partInfo = _avatarPartsReference.GetAvatarPartById(id);
            if (partInfo != null)
                sprites.Add(partInfo.AvatarImage);
            else
                sprites.Add(null);
        }

        Color color = Color.white;
        ColorUtility.TryParseHtmlString(playerAvatar.Colors[0],out color);
        colors.Add(color);

        _avatarVisualDataScriptableObject.sprites = sprites;
        _avatarVisualDataScriptableObject.colors = colors;

        InvokeOnAvatarDesignUpdate();
    }

    public void InvokeOnAvatarDesignUpdate()
    {
        OnAvatarDesignUpdate?.Invoke();
    }
}

using System;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidEventLogEntryHandler : MonoBehaviour
{
    private static readonly Color[] IconColors =
    {
        new(1f, 0.62f, 0.12f, 1f),
        new(0.71f, 0.36f, 1f, 1f),
        new(0.21f, 0.78f, 1f, 1f),
        new(0.46f, 0.82f, 0.37f, 1f)
    };

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image iconImage;
    [SerializeField] private AvatarFaceLoader avatarFaceLoader;

    private RectTransform EntryRectTransform => transform as RectTransform;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    public void Configure(string actorName, string message, Color messageColor, CharacterID characterId, AvatarData avatarData, Sprite systemMessageSprite, bool useSystemIcon)
    {
        ResolveReferences();
        ApplyMessage(message, messageColor);

        if (useSystemIcon)
        {
            ConfigureSystemIcon(systemMessageSprite);
            return;
        }

        ConfigureActorIcon(actorName, characterId, avatarData);
    }

    public void ApplyLayout(float rowHeight)
    {
        if (TryGetComponent(out LayoutElement layoutElement))
        {
            layoutElement.ignoreLayout = false;
            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;
            layoutElement.flexibleHeight = 0f;
        }

        EntryRectTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
    }

    private void ApplyMessage(string message, Color messageColor)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageText.color = messageColor;
    }

    private void ConfigureActorIcon(string actorName, CharacterID characterId, AvatarData avatarData)
    {
        if (TryConfigureAvatarIcon(characterId, avatarData))
        {
            return;
        }

        SetAvatarHeadVisible(false);
        if (iconImage == null)
        {
            return;
        }

        iconImage.enabled = true;
        iconImage.color = ResolveIconColor(actorName);
        iconImage.preserveAspect = false;
    }

    private void ConfigureSystemIcon(Sprite systemMessageSprite)
    {
        SetAvatarHeadVisible(false);
        if (iconImage == null)
        {
            return;
        }

        iconImage.enabled = true;
        iconImage.raycastTarget = false;
        iconImage.sprite = systemMessageSprite;
        iconImage.color = systemMessageSprite == null ? ResolveIconColor("System") : Color.white;
        iconImage.preserveAspect = systemMessageSprite != null;
    }

    private bool TryConfigureAvatarIcon(CharacterID characterId, AvatarData avatarData)
    {
        if (avatarData == null || avatarFaceLoader == null)
        {
            return false;
        }

        AvatarVisualData visualData = AvatarDesignLoader.CreateAvatarVisualDataFallback(avatarData, characterId);
        if (visualData == null)
        {
            return false;
        }

        avatarFaceLoader.SetUseOwnAvatarVisuals(false);

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.raycastTarget = false;
        }

        avatarFaceLoader.gameObject.SetActive(true);
        avatarFaceLoader.UpdateVisuals(visualData);
        return true;
    }

    private void SetAvatarHeadVisible(bool isVisible)
    {
        if (avatarFaceLoader != null)
        {
            avatarFaceLoader.gameObject.SetActive(isVisible);
        }
    }

    private void ResolveReferences()
    {
        if (messageText == null)
        {
            messageText = transform.Find("Message")?.GetComponent<TMP_Text>()
                ?? GetComponent<TMP_Text>()
                ?? GetComponentInChildren<TMP_Text>(true);
        }

        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon") ?? transform.Find("Profile Image");
            iconImage = iconTransform != null
                ? iconTransform.GetComponent<Image>()
                : GetComponentInChildren<Image>(true);
        }

        if (avatarFaceLoader == null && iconImage != null)
        {
            avatarFaceLoader = iconImage.GetComponentInChildren<AvatarFaceLoader>(true);
        }
    }

    private static Color ResolveIconColor(string actorName)
    {
        int hash = 0;
        if (!string.IsNullOrWhiteSpace(actorName))
        {
            foreach (char character in actorName)
            {
                hash = hash * 31 + character;
            }
        }

        int colorIndex = Math.Abs(hash % IconColors.Length);
        return IconColors[colorIndex];
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchInviteCandidateHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Button _button;

    public void SetData(ServerOnlinePlayer player, Action<ServerOnlinePlayer> inviteAction)
    {
        string displayName = GetDisplayName(player);
        string idText = player == null ? string.Empty : player._id;
        _text.text = string.IsNullOrEmpty(idText) || idText == displayName
            ? displayName
            : $"{displayName}\n<size=72%>{idText}</size>";

        _button.onClick.AddListener(() => inviteAction.Invoke(player));
    }

    private static string GetDisplayName(ServerOnlinePlayer player)
    {
        if (player == null)
        {
            return "Tuntematon";
        }

        if (!string.IsNullOrWhiteSpace(player.name))
        {
            return player.name;
        }

        return string.IsNullOrEmpty(player._id) ? "Tuntematon" : player._id;
    }
}

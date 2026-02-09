using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanPlayerFetcher : MonoBehaviour
{
    private readonly List<PlayerData> _players = new();
    public IReadOnlyList<PlayerData> Players { get { return _players; } }
    public bool PlayersLoaded { get; private set; } = false;

    public event Action<PlayerData> OnLocalAvatarUpdated;

    private void Start()
    {
        RefreshClanMembersPlayerData();
    }

    private void OnEnable()
    {
        UpdateLocalAvatar();
    }

    public void RefreshClanMembersPlayerData()
    {
        StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(members =>
        {
            _players.Clear();

            if (members == null)
            {
                Debug.LogError("Failed to load clan members");
            }
            else
            {
                foreach (var member in members)
                {
                    _players.Add(member.GetPlayerData());
                }
            }

            PlayersLoaded = true;
        }));
    }

    private void UpdateLocalAvatar()
    {
        PlayerData data = AvatarEvents.UpdatedPlayerData;
        if (data == null)
        {
            return;
        }

        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Id != data.Id)
            {
                continue;
            }

            if (!AvatarEquals(_players[i].AvatarData, data.AvatarData))
            {
                _players[i] = data;
                OnLocalAvatarUpdated(data);
            }
            break;
        }
    }

    private bool AvatarEquals(AvatarData a,  AvatarData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return
            a.Hair == b.Hair &&
            a.Eyes == b.Eyes &&
            a.Nose == b.Nose &&
            a.Mouth == b.Mouth &&
            a.Clothes == b.Clothes &&
            a.Feet == b.Feet &&
            a.Hands == b.Hands &&
            a.Color == b.Color &&
            a.HairColor == b.HairColor &&
            a.EyesColor == b.EyesColor &&
            a.NoseColor == b.NoseColor &&
            a.MouthColor == b.MouthColor &&
            a.ClothesColor == b.ClothesColor &&
            a.FeetColor == b.FeetColor &&
            a.HandsColor == b.HandsColor;
    }
}

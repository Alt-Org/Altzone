using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Extension methods to Photon <c>Player</c> and <c>Room</c> (<c>RoomInfo</c>) objects.
/// </summary>
/// <remarks>
/// Delete property and <c>null</c> values has a bit fuzzy semantics and this need more work to enforce strict rules.<br />
/// For now it might be better not to use <c>null </c> values.
/// </remarks>
public static class PhotonExtensions
{
    #region Room

    public static IEnumerable<Player> GetPlayersByActorNumber(this Room room)
    {
        return room.Players.Values.OrderBy(x => x.ActorNumber);
    }

    public static IEnumerable<Player> GetPlayersByNickName(this Room room)
    {
        return room.Players.Values.OrderBy(x => x.NickName);
    }

    public static string GetUniquePlayerNameForRoom(this Room room, Player player, string playerName, string separator)
    {
        if (!PhotonNetwork.InRoom)
        {
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }
        if (room.PlayerCount > 0)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = $"Player{separator}{player.ActorNumber}";
            }
            foreach (var otherPlayer in PhotonNetwork.PlayerListOthers)
                if (string.Equals(otherPlayer.NickName, playerName, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Assign new name to current player.
                    playerName = $"{playerName}{separator}{PhotonNetwork.LocalPlayer.ActorNumber}";
                    break;
                }
        }
        return playerName;
    }

    #endregion

    #region CustomProperties

    public static bool HasCustomProperty(this Player player, string key)
    {
        return player.CustomProperties.ContainsKey(key);
    }

    public static void SetCustomProperty(this Player player, string key, object value)
    {
        Assert.IsNotNull(value);
        var props = new Hashtable { { key, value } };
        player.SetCustomProperties(props);
    }

    public static void SafeSetCustomProperty<T>(this Player player, string key, T newValue, T currentValue) where T : struct
    {
        CheckIsTypeAcceptable(newValue);
        Assert.IsTrue(newValue.GetType() == currentValue.GetType(),"newValue.GetType() == currentValue.GetType()");
        DoSetCustomProperty(player, key, newValue, currentValue);
    }

    public static void SafeSetCustomProperty(this Player player, string key, string newValue, string currentValue)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(newValue));
        Assert.IsFalse(string.IsNullOrWhiteSpace(currentValue));
        DoSetCustomProperty(player, key, newValue, currentValue);
    }

    public static void RemoveCustomProperty(this Player player, string key)
    {
        if (player.CustomProperties.ContainsKey(key))
        {
            var props = new Hashtable { { key, null } };
            player.SetCustomProperties(props);
        }
    }

    public static T GetCustomProperty<T>(this Player player, string key, T defaultValue = default)
    {
        if (player.CustomProperties.TryGetValue(key, out var propValue) && propValue != null)
        {
            if (propValue is T valueOfCorrectType)
            {
                return valueOfCorrectType;
            }
            throw new UnityException(
                $"GetCustomProperty value {propValue} ({propValue.GetType().FullName}) is not correct type: {typeof(T).FullName}");
        }
        return defaultValue;
    }

    public static bool HasCustomProperty(this RoomInfo room, string key)
    {
        return room.CustomProperties.ContainsKey(key);
    }

    public static void SetCustomProperty(this Room room, string key, object value)
    {
        Assert.IsNotNull(value);
        var props = new Hashtable { { key, value } };
        room.SetCustomProperties(props);
    }

    public static void SafeSetCustomProperty<T>(this Room room, string key, T newValue, T currentValue) where T : struct
    {
        CheckIsTypeAcceptable(newValue);
        Assert.IsTrue(newValue.GetType() == currentValue.GetType(), "newValue.GetType() == currentValue.GetType()");
        DoSetCustomProperty(room, key, newValue, currentValue);
    }

    public static void SafeSetCustomProperty(this Room room, string key, string newValue, string currentValue)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(newValue));
        Assert.IsFalse(string.IsNullOrWhiteSpace(currentValue));
        DoSetCustomProperty(room, key, newValue, currentValue);
    }

    public static T GetCustomProperty<T>(this RoomInfo room, string key, T defaultValue = default)
    {
        if (room.CustomProperties.TryGetValue(key, out var propValue) && propValue != null)
        {
            if (propValue is T valueOfCorrectType)
            {
                return valueOfCorrectType;
            }
            throw new UnityException(
                $"GetCustomProperty value {propValue} ({propValue.GetType().FullName}) is not correct type: {typeof(T).FullName}");
        }
        return defaultValue;
    }

    public static void RemoveCustomProperty(this Room room, string key)
    {
        if (room.CustomProperties.ContainsKey(key))
        {
            var props = new Hashtable { { key, null } };
            room.SetCustomProperties(props);
        }
    }

    private static void DoSetCustomProperty(this Player player, string key, object newValue, object currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        if (!player.CustomProperties.TryGetValue(key, out var propValue))
        {
            player.SetCustomProperties(props);
            return;
        }
        Assert.IsTrue(newValue.GetType() == propValue.GetType(), "newValue.GetType() == propValue.GetType()");
        Assert.IsTrue(!newValue.Equals(currentValue), "!newValue.Equals(currentValue)");
        Assert.IsTrue(currentValue.Equals(propValue), "currentValue.Equals(propValue)");
        var expectedProps = new Hashtable { { key, currentValue } };
        player.SetCustomProperties(props, expectedProps);
    }

    private static void DoSetCustomProperty(this Room room, string key, object newValue, object currentValue)
    {
        var props = new Hashtable { { key, newValue } };
        if (!room.CustomProperties.TryGetValue(key, out var propValue))
        {
            room.SetCustomProperties(props);
            return;
        }
        Assert.IsTrue(newValue.GetType() == propValue.GetType(), "newValue.GetType() == propValue.GetType()");
        Assert.IsTrue(!newValue.Equals(currentValue), "!newValue.Equals(currentValue)");
        Assert.IsTrue(currentValue.Equals(propValue), "currentValue.Equals(propValue)");
        var expectedProps = new Hashtable { { key, currentValue } };
        room.SetCustomProperties(props, expectedProps);
    }

    private static void CheckIsTypeAcceptable<T>(T value) where T : struct
    {
        // T is limited to some "value types" which struct represents!
        var isTypeAcceptable = value is bool ||
                               value is byte ||
                               value is short ||
                               value is int;
        Assert.IsTrue(isTypeAcceptable, $"SafeSetCustomProperty type is not supported: {typeof(T)}");
    }

    #endregion

    #region Debugging

    public static string GetDebugLabel(this Player player, bool verbose = true)
    {
        if (player == null)
        {
            return string.Empty;
        }
        var status = $"{player.ActorNumber}";
        if (player.IsMasterClient)
        {
            status += ",m";
        }
        status += player.IsLocal ? ",l" : ",r";
        if (player.IsInactive)
        {
            status += ",out";
        }
        if (verbose)
        {
            status += $" {player.CustomProperties.AsSorted()}";
        }
        var playerName = verbose ? $"Player: {player.NickName}" : player.NickName;
        return $"{playerName} {status}";
    }

    public static string GetDebugLabel(this RoomInfo room) // Works for Room too!
    {
        // Replacement for room.ToString()
        return $"{room}{(room.RemovedFromList ? " removed." : string.Empty)} {room.CustomProperties.AsSorted()}";
    }

    private static string AsSorted(this Hashtable dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return "{}";
        }
        var keys = dictionary.Keys.ToList();
        keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
        var builder = new StringBuilder("{");
        foreach (var key in keys)
        {
            var propValue = dictionary[key].ToString();
            if (propValue.Length > 12)
            {
                propValue = propValue.GetHashCode().ToString("X");
            }
            builder.Append(key).Append('=').Append(propValue).Append(", ");
        }
        builder.Length -= 2;
        builder.Append('}');
        return builder.ToString();
    }

    #endregion
}
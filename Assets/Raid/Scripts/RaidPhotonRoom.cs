using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;

public static class RaidPhotonRoom
{
    public const int RoomCapacity = 4;
    public const int RequiredPlayers = 4;
    public const int MaxPlayersPerClan = 2;

    public const string RaidMatchmakingKey = "raid_mm";
    public const string RaidClanCountsKey = "raid_clans";
    public const string RaidStateKey = "raid_state";
    public const string RaidSetupReadyKey = "raid_ready";
    public const string RaidStartTimeKey = "raid_start";
    public const string RaidInventorySizeKey = "raid_inv_size";
    public const string RaidInventorySeedKey = "raid_seed";
    public const string RaidTrapSlotsKey = "raid_traps";
    public const string RaidClanWeightLimitsKey = "raid_w_limits";
    public const string RaidPlayerWeightLimitsKey = "raid_p_limits";

    public const string PlayerIdKey = "raid_player_id";
    public const string PlayerClanIdKey = "raid_clan_id";
    public const string PlayerClanNameKey = "raid_clan_name";
    public const string PlayerCharacterIdKey = "raid_char_id";
    public const string PlayerAvatarDataKey = "raid_avatar";

    public const byte LootRequestEvent = 80;
    public const byte LootAcceptedEvent = 81;
    public const byte RemoveLootRequestEvent = 82;
    public const byte RemoveLootAcceptedEvent = 83;

    public const int StateMatchmaking = 0;
    public const int StateLobby = 1;
    public const int StateStarted = 2;

    public struct TrapData
    {
        public int Index;
        public int Type;

        public TrapData(int index, int type)
        {
            Index = index;
            Type = type;
        }
    }

    public struct ClanEntry
    {
        public string ClanId;
        public string ClanName;
        public int Count;

        public ClanEntry(string clanId, string clanName, int count)
        {
            ClanId = clanId ?? string.Empty;
            ClanName = clanName ?? string.Empty;
            Count = count;
        }
    }

    public struct ClanWeightLimit
    {
        public string ClanId;
        public float MaxWeight;

        public ClanWeightLimit(string clanId, float maxWeight)
        {
            ClanId = clanId ?? string.Empty;
            MaxWeight = maxWeight;
        }
    }

    public struct PlayerWeightLimit
    {
        public string PlayerId;
        public float MaxWeight;

        public PlayerWeightLimit(string playerId, float maxWeight)
        {
            PlayerId = playerId ?? string.Empty;
            MaxWeight = maxWeight;
        }
    }

    public static string EncodeTraps(IEnumerable<TrapData> traps)
    {
        if (traps == null)
        {
            return string.Empty;
        }

        return string.Join("|", traps.Select(trap => $"{trap.Index}:{trap.Type}"));
    }

    public static TrapData[] DecodeTraps(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<TrapData>();
        }

        List<TrapData> traps = new();
        string[] entries = value.Split('|');
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int type))
            {
                traps.Add(new TrapData(index, type));
            }
        }

        return traps.ToArray();
    }

    public static string EncodeClanCounts(IEnumerable<ClanEntry> clans)
    {
        if (clans == null)
        {
            return string.Empty;
        }

        return string.Join("|", clans
            .Where(clan => !string.IsNullOrWhiteSpace(clan.ClanId))
            .Select(clan => $"{Escape(clan.ClanId)}:{clan.Count}:{Escape(clan.ClanName)}"));
    }

    public static ClanEntry[] DecodeClanCounts(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<ClanEntry>();
        }

        List<ClanEntry> clans = new();
        string[] entries = value.Split('|');
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
            {
                continue;
            }

            string clanName = parts.Length >= 3 ? Unescape(parts[2]) : string.Empty;
            clans.Add(new ClanEntry(Unescape(parts[0]), clanName, count));
        }

        return clans.ToArray();
    }

    public static string EncodeClanWeightLimits(IEnumerable<ClanWeightLimit> limits)
    {
        if (limits == null)
        {
            return string.Empty;
        }

        return string.Join("|", limits
            .Where(limit => !string.IsNullOrWhiteSpace(limit.ClanId))
            .Select(limit => $"{Escape(limit.ClanId)}:{limit.MaxWeight.ToString(CultureInfo.InvariantCulture)}"));
    }

    public static ClanWeightLimit[] DecodeClanWeightLimits(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<ClanWeightLimit>();
        }

        List<ClanWeightLimit> limits = new();
        string[] entries = value.Split('|');
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float maxWeight))
            {
                limits.Add(new ClanWeightLimit(Unescape(parts[0]), maxWeight));
            }
        }

        return limits.ToArray();
    }

    public static string EncodePlayerWeightLimits(IEnumerable<PlayerWeightLimit> limits)
    {
        if (limits == null)
        {
            return string.Empty;
        }

        return string.Join("|", limits
            .Where(limit => !string.IsNullOrWhiteSpace(limit.PlayerId))
            .Select(limit => $"{Escape(limit.PlayerId)}:{limit.MaxWeight.ToString(CultureInfo.InvariantCulture)}"));
    }

    public static PlayerWeightLimit[] DecodePlayerWeightLimits(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<PlayerWeightLimit>();
        }

        List<PlayerWeightLimit> limits = new();
        string[] entries = value.Split('|');
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float maxWeight))
            {
                limits.Add(new PlayerWeightLimit(Unescape(parts[0]), maxWeight));
            }
        }

        return limits.ToArray();
    }

    public static string EncodeAvatarData(AvatarData avatarData)
    {
        if (avatarData == null)
        {
            return string.Empty;
        }

        string[] values =
        {
            avatarData.Name,
            avatarData.Hair.ToString(CultureInfo.InvariantCulture),
            avatarData.Eyes.ToString(CultureInfo.InvariantCulture),
            avatarData.Nose.ToString(CultureInfo.InvariantCulture),
            avatarData.Mouth.ToString(CultureInfo.InvariantCulture),
            avatarData.Clothes.ToString(CultureInfo.InvariantCulture),
            avatarData.Feet.ToString(CultureInfo.InvariantCulture),
            avatarData.Hands.ToString(CultureInfo.InvariantCulture),
            avatarData.Color,
            avatarData.HairColor,
            avatarData.EyesColor,
            avatarData.NoseColor,
            avatarData.MouthColor,
            avatarData.ClothesColor,
            avatarData.FeetColor,
            avatarData.HandsColor
        };

        return string.Join("|", values.Select(Escape));
    }

    public static AvatarData DecodeAvatarData(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string[] parts = value.Split('|');
        if (parts.Length < 16)
        {
            return null;
        }

        return new AvatarData
        {
            Name = DecodeString(parts, 0),
            Hair = DecodeInt(parts, 1),
            Eyes = DecodeInt(parts, 2),
            Nose = DecodeInt(parts, 3),
            Mouth = DecodeInt(parts, 4),
            Clothes = DecodeInt(parts, 5),
            Feet = DecodeInt(parts, 6),
            Hands = DecodeInt(parts, 7),
            Color = DecodeString(parts, 8),
            HairColor = DecodeString(parts, 9),
            EyesColor = DecodeString(parts, 10),
            NoseColor = DecodeString(parts, 11),
            MouthColor = DecodeString(parts, 12),
            ClothesColor = DecodeString(parts, 13),
            FeetColor = DecodeString(parts, 14),
            HandsColor = DecodeString(parts, 15)
        };
    }

    private static int DecodeInt(string[] parts, int index)
    {
        return index >= 0
            && index < parts.Length
            && int.TryParse(Unescape(parts[index]), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : 0;
    }

    private static string DecodeString(string[] parts, int index)
    {
        return index >= 0 && index < parts.Length
            ? Unescape(parts[index])
            : string.Empty;
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }

    private static string Unescape(string value)
    {
        return Uri.UnescapeDataString(value ?? string.Empty);
    }
}

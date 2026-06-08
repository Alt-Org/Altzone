using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class RaidPhotonRoom
{
    public const int RoomCapacity = 4;
    public const int RequiredPlayers = 2;
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

    public const string PlayerClanIdKey = "raid_clan_id";
    public const string PlayerClanNameKey = "raid_clan_name";

    public const byte LootRequestEvent = 80;
    public const byte LootAcceptedEvent = 81;

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

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }

    private static string Unescape(string value)
    {
        return Uri.UnescapeDataString(value ?? string.Empty);
    }
}

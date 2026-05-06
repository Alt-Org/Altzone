using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/Clan/Clan Role Catalog", fileName = "ClanRoleCatalog")]
public class ClanRoleCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Match by server role name (e.g. 'sisustaja')")]
        public string roleName;

        public Sprite icon;

        [Tooltip("Optional override shown in UI")]
        public string displayNameOverride;
    }

    [SerializeField] private List<Entry> entries = new();

    public Sprite GetIcon(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) return null;

        var e = entries.Find(x =>
            !string.IsNullOrEmpty(x.roleName) &&
            string.Equals(x.roleName, roleName, StringComparison.OrdinalIgnoreCase)
        );

        return e?.icon;
    }

    public string GetDisplayName(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) return "";

        var e = entries.Find(x =>
            !string.IsNullOrEmpty(x.roleName) &&
            string.Equals(x.roleName, roleName, StringComparison.OrdinalIgnoreCase)
        );

        if (e != null && !string.IsNullOrEmpty(e.displayNameOverride))
            return e.displayNameOverride;

        return roleName;
    }
}

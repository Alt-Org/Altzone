using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;

public class ClanRightsPanel : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Toggle[] _clanMemberRightToggles;
    [SerializeField] private Toggle[] _clanElderRightToggles;
    [SerializeField] private Toggle[] _clanAdminRightToggles;

    public ClanRoleRights[] ClanRights { get; private set; } = new ClanRoleRights[3];

    public void InitializeRightsToggles(ClanRoleRights[] initialRights)
    {
        Toggle[][] allToggles = { _clanMemberRightToggles, _clanElderRightToggles, _clanAdminRightToggles };

        if (initialRights.Length == 0)
        {
            initialRights = new ClanRoleRights[3]  {
                ClanRoleRights.None,
                ClanRoleRights.EditSoulHome,
                ClanRoleRights.EditClanSettings | ClanRoleRights.EditSoulHome | ClanRoleRights.EditMemberRights
            };
        }
        ClanRights = (ClanRoleRights[])initialRights.Clone();

        for (int i = 0; i < allToggles.Length; i++)
        {
            int role = i;
            for (int j = 0; j < allToggles[i].Length; j++)
            {
                ClanRoleRights right = (ClanRoleRights)(1 << j);

                allToggles[i][j].isOn = ClanRights[role].Contains(right);
                allToggles[i][j].onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        ClanRights[role] = ClanRights[role].Add(right);
                    }
                    else
                    {
                        ClanRights[role] = ClanRights[role].Remove(right);
                    }
                });
            }
        }
    }
}

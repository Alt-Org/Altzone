using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Altzone.Scripts.GA
{
    public enum SectionName
    {
        None,
        MainMenu,
        SoulHome,
        DailyTask,
        Leaderboard,
        ClanPage,
        ClanSelection,
        CharacterScreen,
        AvatarModification,
        Settings,
        Voting
    }


    public class GameAnalyticsSectionHandler : MonoBehaviour
    {
        [SerializeField] private SectionName _sectionName = SectionName.None;

        private void OnEnable()
        {
            if (_sectionName == SectionName.None) return;
            string name = GetSectionName();
            if (GameAnalyticsManager.Instance != null) GameAnalyticsManager.Instance.EnterSection(name);
        }

        private void OnDisable()
        {
            if (_sectionName == SectionName.None) return;
            string name = GetSectionName();
            if (GameAnalyticsManager.Instance != null) GameAnalyticsManager.Instance.ExitSection(name);
        }

        private string GetSectionName()
        {
            switch (_sectionName)
            {
               case SectionName.MainMenu:
                    return "MainMenu";
                case SectionName.SoulHome:
                    return "SoulHome";
                case SectionName.DailyTask:
                    return "DailyTask";
                case SectionName.Leaderboard:
                    return "Leaderboard";
                case SectionName.ClanPage:
                    return "ClanPage";
                case SectionName.ClanSelection:
                    return "ClanSelection";
                case SectionName.CharacterScreen:
                    return "CharacterScreen";
                case SectionName.Settings:
                    return "Settings";
                case SectionName.Voting:
                    return "Voting";
                default:
                    return "";
            }
        }
    }
}

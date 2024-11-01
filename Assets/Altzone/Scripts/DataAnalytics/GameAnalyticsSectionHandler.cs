using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.GA
{
    /// <summary>
    /// Section name for Game Analytics.
    /// </summary>
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

    /// <summary>
    /// Sends Game Analytics section name event when this object is enabled and disabled (in UI).
    /// </summary>
    public class GameAnalyticsSectionHandler : MonoBehaviour
    {
        [SerializeField] private SectionName _sectionName = SectionName.None;

        private void Awake()
        {
            Assert.AreNotEqual(SectionName.None, _sectionName, "section name is required");
        }

        private void OnEnable() => HandleSection(_sectionName, isEnter: true);

        private void OnDisable() => HandleSection(_sectionName, isEnter: false);

        private static void HandleSection(SectionName sectionName, bool isEnter)
        {
            var instance = GameAnalyticsManager.Instance;
            if (instance == null)
            {
                return;
            }
            if (isEnter)
            {
                instance.EnterSection(GetSectionName(sectionName));
            }
            else
            {
                instance.ExitSection(GetSectionName(sectionName));
            }
        }

        private static string GetSectionName(SectionName sectionName) => sectionName.ToString();
    }
}

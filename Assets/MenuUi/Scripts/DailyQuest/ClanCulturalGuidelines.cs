using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class ClanCulturalPractices : DailyTaskProgressListener
{
    public void SettingsChanged(ClanData data)
    {
        if (!string.IsNullOrEmpty(data.Phrase) && data.Language != Language.None && data.Goals != Goals.None && data.ClanAge != ClanAge.None && data.Values.Count > 0)
        {
            UpdateProgress("1");
        }
    }
}

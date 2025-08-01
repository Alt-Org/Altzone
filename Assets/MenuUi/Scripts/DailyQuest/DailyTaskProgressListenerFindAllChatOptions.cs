using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public class DailyTaskProgressListenerFindAllChatOptions : DailyTaskProgressListener
{
    private bool _globalChatFound = false;
    private bool _languageChatFound = false;
    private bool _clanChatFound = false;

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Social;
        _educationCategorySocialType = TaskEducationSocialType.FindAllChatOptions;
    }

    public void ChatOptionFound(string chat)
    {
        switch (chat)
        {
            case "Global": _globalChatFound = true; break;
            case "Language": _languageChatFound = true; break;
            case "Clan": _clanChatFound = true; break;
            default: break;
        }

        if (_globalChatFound && _languageChatFound && _clanChatFound)
        {
            UpdateProgress("1");
        }
    }
}

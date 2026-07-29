using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public class FindAllChatOptions : DailyTaskProgressListener
{
    private bool _globalChatFound = false;
    private bool _languageChatFound = false;
    private bool _clanChatFound = false;

    public enum ChatType
    {
        Global,
        Language,
        Clan
    }

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Social;
        _educationCategorySocialType = TaskEducationSocialType.FindAllChatOptions;
    }

    public void ChatOptionFound(ChatType chat)
    {
        switch (chat)
        {
            case ChatType.Global: _globalChatFound = true; break;
            case ChatType.Language: _languageChatFound = true; break;
            case ChatType.Clan: _clanChatFound = true; break;
            default: break;
        }

        if (_globalChatFound && _languageChatFound && _clanChatFound)
        {
            UpdateProgress("1");
        }
    }
}

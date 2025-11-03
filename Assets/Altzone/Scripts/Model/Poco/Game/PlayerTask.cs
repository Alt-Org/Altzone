using System.Collections.Generic;

namespace Altzone.Scripts.Model.Poco.Game
{
    public enum TaskNormalType
    {
        Undefined,
        Test,
        PlayBattle,
        WinBattle,
        WriteChatMessage,
        StartBattleDifferentCharacter,
        Vote,
    }

    #region Education tasks

    public enum EducationCategoryType
    {
        None,
        Social,
        Story,
        Culture,
        Ethical,
        Action
    }

    public enum TaskEducationActionType
    {
        PlayBattle,
        WinBattle,
        EditCharacterStats,
        BlowUpYourCharacter,
        SwitchSoulhomeMusic,
        MakeMusicWithButtons,
        MakeCharacterFast,
        MakeCharacterDurable,
        MakeCharacterStrong,
        MakeCharacterBig,
        ChangeAvatarClothes,
        ChangeItemsPosition,
        UseAllItemsSoulhome,
        FindVariableValueInGame,
        Find3ImportantButtons,
        FindBug,
    }

    public enum TaskEducationSocialType
    {
        EmoteDuringBattle,
        AddNewFriend,
        CreateNewVote,
        EditCharacterAvatar,
        ShareBattleReplay,
        WriteChatMessageClan,
        ChatAddReaction,
        FindAllChatOptions,
        UseAllChatFeelings,
        DefinePlayerStyle,
        WriteChatMessageGlobal,
        ClanVote,
        SuggestItemFleaMarket,
        AddItemFleaMarket,
        ChangeClanMotto,
    }

    public enum TaskEducationStoryType
    {
        FindSymbolicalGraphics,
        ContinueClanStory,
        FindSybolicalFurniture,
        ClickCharacterDescription,
        RecognizeSoundClue,
        CreateUnifiedInterior,
        RecognizeCharacterMechanic,
        WhereGameHappens,
    }

    public enum TaskEducationCultureType
    {
        ClickKnownArtIdeaPerson,
        GamesGenreTypes,
        ClickKnownCharacter,
        SimiliarToAGame,
        SetProfilePlayerType,
        FindPowerOrEqualityWindow,
    }

    public enum TaskEducationEthicalType
    {
        ClickBuyable,
        UseOnlyPositiveEmotes,
        UseOnlyNegativeEmotes,
        ClickQuestionable,
        ClickEthical,
        PressSustainableConsumptionObjects,
        PressValuesObjects,
    }

    #endregion

    public class PlayerTasks
    {
        private List<PlayerTask> _daily;
        private List<PlayerTask> _week;
        private List<PlayerTask> _month;

        public PlayerTasks(ServerPlayerTasks tasks)
        {
            _daily = new();
            foreach (ServerPlayerTask task in tasks.daily)
            {
                _daily.Add(new(task));
            }
            _week = new();
            foreach (ServerPlayerTask task in tasks.weekly)
            {
                _week.Add(new(task));
            }
            _month = new();
            foreach (ServerPlayerTask task in tasks.monthly)
            {
                _month.Add(new(task));
            }
        }

        public List<PlayerTask> Daily { get => _daily; }
        public List<PlayerTask> Week { get => _week; }
        public List<PlayerTask> Month { get => _month; }
    }

    public class PlayerTask
    {
        private string _id;
        private TaskTitle _title;
        private TaskContent _content;
        private int _amount;
        private int _amountLeft;
        private TaskNormalType _normalTaskType;
        private int _coins;
        private int _points;
        private int _taskProgress;
        private string _playerId = "";
        private string _startedAt;
        private EducationCategoryType _educationCategory;
        private TaskEducationActionType _educationActionType;
        private TaskEducationSocialType _educationSocialType;
        private TaskEducationStoryType _educationStoryType;
        private TaskEducationCultureType _educationCultureType;
        private TaskEducationEthicalType _educationEthicalType;

        public string Id { get => _id;}
        public int Amount { get => _amount;}
        public TaskNormalType Type { get => _normalTaskType;}
        public int Coins { get => _coins;}
        public int Points { get => _points;}

        public string Title {
            get
            {
                return _title.Fi;
            }
        }

        public string EnglishTitle
        {
            get
            {
                return _title.En; 
            }
        }

        public string Content
        {
            get
            {
                if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
                    return _content.En;
                return _content.Fi; // default Finnish
            }
        }

        public int TaskProgress { get => _taskProgress;}
        public string PlayerId { get => _playerId; }
        public int AmountLeft { get => _amountLeft; }
        public string StartedAt { get => _startedAt; }
        public EducationCategoryType EducationCategory { get => _educationCategory;}
        public TaskEducationActionType EducationActionType { get => _educationActionType;}
        public TaskEducationSocialType EducationSocialType { get => _educationSocialType;}
        public TaskEducationStoryType EducationStoryType { get => _educationStoryType;}
        public TaskEducationCultureType EducationCultureType { get => _educationCultureType;}
        public TaskEducationEthicalType EducationEthicalType {  get => _educationEthicalType;}

        public PlayerTask(ServerPlayerTask task)
        {
            _id = task._id;
            _title = new(task.title);
            _content = new(task.content);
            _amount = task.amount;
            _amountLeft = task.amountLeft;
            _coins = task.coins;
            _points = task.points;
            _normalTaskType = GetTaskTypeEnum(task.type);
            _playerId = string.IsNullOrWhiteSpace(task.player_id) ? "" : task.player_id;
            _startedAt = task.startedAt;
            _educationCategory = GetEducationTypeEnum(task.educationCategoryType);

            switch (task.educationCategoryType)
            {
                case "action": _educationActionType = GetEducationActionTypeEnum(task.educationCategoryTaskType); break;
                case "social": _educationSocialType = GetEducationSocialTypeEnum(task.educationCategoryTaskType); break;
                case "story": _educationStoryType = GetEducationStoryTypeEnum(task.educationCategoryTaskType); break;
                case "culture": _educationCultureType = GetEducationCultureTypeEnum(task.educationCategoryTaskType); break;
                case "ethical": _educationEthicalType = GetEducationEthicalTypeEnum(task.educationCategoryTaskType); break;
                default: break;
            }
        }

        public int Progress(int amount)
        {
            if (amount <= 0) return _taskProgress;
            _taskProgress =- amount;
            if(_taskProgress < 0) _taskProgress = 0;
            return _taskProgress;
        }

        private TaskNormalType GetTaskTypeEnum(string type)
        {
            switch (type)
            {
                case "play_battle":
                    {
                        return TaskNormalType.PlayBattle;
                    }
                case "win_battle":
                    {
                        return TaskNormalType.WinBattle;
                    }
                case "write_chat_message":
                    {
                        return TaskNormalType.WriteChatMessage;
                    }
                case "start_battle_with_new_character":
                    {
                        return TaskNormalType.StartBattleDifferentCharacter;
                    }
                case "participate_clan_voting":
                    {
                        return TaskNormalType.Vote;
                    }
                default:
                    {
                        return TaskNormalType.Undefined;
                    }
            }
        }

        #region Get Education Enum

        private EducationCategoryType GetEducationTypeEnum(string type)
        {
            switch (type)
            {
                case "social":
                    {
                        return EducationCategoryType.Social;
                    }
                case "story":
                    {
                        return EducationCategoryType.Story;
                    }
                case "culture":
                    {
                        return EducationCategoryType.Culture;
                    }
                case "ethical":
                    {
                        return EducationCategoryType.Ethical;
                    }
                case "action":
                    {
                        return EducationCategoryType.Action;
                    }
                default:
                    {
                        return EducationCategoryType.None;
                    }
            }
        }

        private TaskEducationActionType GetEducationActionTypeEnum(string type)
        {
            switch (type)
            {
                case "play_battle":
                    {
                        return TaskEducationActionType.PlayBattle;
                    }
                case "win_battle":
                    {
                        return TaskEducationActionType.WinBattle;
                    }
                case "change_song_soulhome":
                    {
                        return TaskEducationActionType.SwitchSoulhomeMusic;
                    }
                case "change_character_stats":
                    {
                        return TaskEducationActionType.EditCharacterStats;
                    }
                case "explode_character_battle":
                    {
                        return TaskEducationActionType.BlowUpYourCharacter;
                    }
                case "make_music_with_buttons":
                    {
                        return TaskEducationActionType.MakeMusicWithButtons;
                    }
                case "change_character_be_fast":
                    {
                        return TaskEducationActionType.MakeCharacterFast;
                    }
                case "change_character_be_resistant":
                    {
                        return TaskEducationActionType.MakeCharacterDurable;
                    }
                case "change_character_be_strong":
                    {
                        return TaskEducationActionType.MakeCharacterStrong;
                    }
                case "change_character_be_large":
                    {
                        return TaskEducationActionType.MakeCharacterBig;
                    }
                case "change_avatar_clothes":
                    {
                        return TaskEducationActionType.ChangeAvatarClothes;
                    }
                case "change_items_position":
                    {
                        return TaskEducationActionType.ChangeItemsPosition;
                    }
                case "use_all_items_soulhome":
                    {
                        return TaskEducationActionType.UseAllItemsSoulhome;
                    }
                case "find_variable_value_in_game":
                    {
                        return TaskEducationActionType.FindVariableValueInGame;
                    }
                case "find_3_important_buttons":
                    {
                        return TaskEducationActionType.Find3ImportantButtons;
                    }
                case "find_bug":
                    {
                        return TaskEducationActionType.FindBug;
                    }
                default:
                    {
                        return TaskEducationActionType.PlayBattle;
                    }
            }
        }

        private TaskEducationSocialType GetEducationSocialTypeEnum(string type)
        {
            switch (type)
            {
                case "react_emoji_battle":
                    {
                        return TaskEducationSocialType.EmoteDuringBattle;
                    }
                case "write_chat_message_clan":
                    {
                        return TaskEducationSocialType.WriteChatMessageClan;
                    }
                case "change_avatar_outlook":
                    {
                        return TaskEducationSocialType.EditCharacterAvatar;
                    }
                case "share_battle_replay_clan_chat":
                    {
                        return TaskEducationSocialType.ShareBattleReplay;
                    }
                case "add_friend":
                    {
                        return TaskEducationSocialType.AddNewFriend;
                    }
                case "create_clan_voting":
                    {
                        return TaskEducationSocialType.CreateNewVote;
                    }
                case "react_emoji_chat":
                    {
                        return TaskEducationSocialType.ChatAddReaction;
                    }
                case "find_all_chat_options":
                    {
                        return TaskEducationSocialType.FindAllChatOptions;
                    }
                case "use_all_chat_feelings":
                    {
                        return TaskEducationSocialType.UseAllChatFeelings;
                    }
                case "define_player_style":
                    {
                        return TaskEducationSocialType.DefinePlayerStyle;
                    }
                case "write_chat_message_global":
                    {
                        return TaskEducationSocialType.WriteChatMessageGlobal;
                    }
                case "participate_clan_voting":
                    {
                        return TaskEducationSocialType.ClanVote;
                    }
                case "suggest_item_to_flea_market":
                    {
                        return TaskEducationSocialType.SuggestItemFleaMarket;
                    }
                case "add_item_to_flea_market":
                    {
                        return TaskEducationSocialType.AddItemFleaMarket;
                    }
                case "change_clan_motto":
                    {
                        return TaskEducationSocialType.ChangeClanMotto;
                    }
                default:
                    {
                        return TaskEducationSocialType.EmoteDuringBattle;
                    }
            }
        }

        private TaskEducationStoryType GetEducationStoryTypeEnum(string type)
        {
            switch (type)
            {
                case "press_character_description":
                    {
                        return TaskEducationStoryType.ClickCharacterDescription;
                    }
                case "continue_clan_story":
                    {
                        return TaskEducationStoryType.ContinueClanStory;
                    }
                case "recognize_audio_hints":
                    {
                        return TaskEducationStoryType.RecognizeSoundClue;
                    }
                case "find_symbolical_furniture":
                    {
                        return TaskEducationStoryType.FindSybolicalFurniture;
                    }
                case "find_ui_symbolics":
                    {
                        return TaskEducationStoryType.FindSymbolicalGraphics;
                    }
                case "create_unified_interior":
                    {
                        return TaskEducationStoryType.CreateUnifiedInterior;
                    }
                case "recognize_character_mechanic":
                    {
                        return TaskEducationStoryType.RecognizeCharacterMechanic;
                    }
                case "where_game_happens":
                    {
                        return TaskEducationStoryType.WhereGameHappens;
                    }
                default:
                    {
                        return TaskEducationStoryType.ClickCharacterDescription;
                    }
            }
        }

        private TaskEducationCultureType GetEducationCultureTypeEnum(string type)
        {
            switch (type)
            {
                case "press_famous_thing_referring_objects":
                    {
                        return TaskEducationCultureType.ClickKnownArtIdeaPerson;
                    }
                case "press_famous_character":
                    {
                        return TaskEducationCultureType.ClickKnownCharacter;
                    }
                case "what_style_types_game_has":
                    {
                        return TaskEducationCultureType.GamesGenreTypes;
                    }
                case "define_player_type":
                    {
                        return TaskEducationCultureType.SetProfilePlayerType;
                    }
                case "what_famous_game_reminding":
                    {
                        return TaskEducationCultureType.SimiliarToAGame;
                    }
                case "find_power_or_equality_referring_window":
                    {
                        return TaskEducationCultureType.FindPowerOrEqualityWindow;
                    }
                default:
                    {
                        return TaskEducationCultureType.ClickKnownArtIdeaPerson;
                    }
            }
        }

        private TaskEducationEthicalType GetEducationEthicalTypeEnum(string type)
        {
            switch (type)
            {
                case "press_money_stuff":
                    {
                        return TaskEducationEthicalType.ClickBuyable;
                    }
                case "click_ethical":
                    {
                        return TaskEducationEthicalType.ClickEthical;
                    }
                case "press_ethic_questionable_objects":
                    {
                        return TaskEducationEthicalType.ClickQuestionable;
                    }
                case "use_only_negative_gestures_in_battle":
                    {
                        return TaskEducationEthicalType.UseOnlyNegativeEmotes;
                    }
                case "use_only_positive_gestures_in_battle":
                    {
                        return TaskEducationEthicalType.UseOnlyPositiveEmotes;
                    }
                case "press_sustainable_consumption_objects":
                    {
                        return TaskEducationEthicalType.PressSustainableConsumptionObjects;
                    }
                case "press_values_objects":
                    {
                        return TaskEducationEthicalType.PressValuesObjects;
                    }
                default:
                    {
                        return TaskEducationEthicalType.ClickBuyable;
                    }
            }
        }

        #endregion

        public class TaskTitle
        {
            private readonly string _fi;
            private readonly string _en;

            public string Fi { get => _fi;}
            public string En { get => _en; }

            public TaskTitle(ServerPlayerTask.TaskTitle title)
            {
                _fi = title.fi;
                _en = title.en;
            }
        }

        public class TaskContent
        {
            private readonly string _fi;
            private readonly string _en;

            public string Fi { get => _fi;}
            public string En { get => _en;}
            public TaskContent(ServerPlayerTask.TaskContent content)
            {
                _fi = content?.fi ?? "";
                _en = content?.en ?? "";
            }
        }

        //#region Delegates, Events & Invoke Functions

        //public delegate void TaskSelected();
        //public event TaskSelected OnTaskSelected;
        //public void InvokeOnTaskSelected()
        //{
        //    OnTaskSelected.Invoke();
        //}

        //public delegate void TaskDeselected();
        //public event TaskDeselected OnTaskDeselected;
        //public void InvokeOnTaskDeselected()
        //{
        //    OnTaskDeselected.Invoke();
        //}

        //public delegate void TaskUpdated();
        //public event TaskUpdated OnTaskUpdated;
        //public void InvokeOnTaskUpdated()
        //{
        //    OnTaskUpdated.Invoke();
        //}

        //#endregion

        #region Add & clear from outside.

        public void AddProgress(int value)
        {
            _taskProgress += value;
        }

        public void ClearProgress()
        {
            _taskProgress = 0;
        }

        public void AddPlayerId(string id)
        {
            _playerId = id;
        }

        public void ClearPlayerId()
        {
            _playerId = "";
        }

        #endregion
    }

    public enum TaskVersionType
    {
        Normal,
        Education
    }

    public class ClanTasks
    {
        private TaskVersionType _taskVersionType;
        private List<PlayerTask> _tasks;

        public TaskVersionType TaskVersionType { get => _taskVersionType;}
        public List<PlayerTask> Tasks { get => _tasks;}

        public ClanTasks(TaskVersionType versionType, List<PlayerTask> tasks)
        {
            _taskVersionType = versionType;
            _tasks = tasks;
        }
    }

    public class ServerPlayerTasks
    {
        public List<ServerPlayerTask> daily;
        public List<ServerPlayerTask> weekly;
        public List<ServerPlayerTask> monthly;
    }

    public class ServerPlayerTask
    {
        public string _id;
        public TaskTitle title;
        public TaskContent content;
        public int amount;
        public int amountLeft;
        public string type;
        public int coins;
        public int points;
        public string player_id;
        public string startedAt;
        public string educationCategoryType;
        public string educationCategoryTaskType;

        public class TaskTitle
        {
            public string fi;
            public string en;
        }
        public class TaskContent
        {
            public string fi;
            public string en;
        }
    }
}

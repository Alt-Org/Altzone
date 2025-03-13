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
    }

    public enum TaskEducationSocialType
    {
        EmoteDuringBattle,
        AddNewFriend,
        CreateNewVote,
        EditCharacterAvatar,
        ShareBattleReplay,
        WriteChatMessageClan,
    }

    public enum TaskEducationStoryType
    {
        FindSymbolicalGraphics,
        ContinueClanStory,
        FindSybolicalFurniture,
        ClickCharacterDescription,
        RecognizeSoundClue,
    }

    public enum TaskEducationCultureType
    {
        ClickKnownArtIdeaPerson,
        GamesGenreTypes,
        ClickKnownCharacter,
        SimiliarToAGame,
        SetProfilePlayerType,
    }

    public enum TaskEducationEthicalType
    {
        ClickBuyable,
        UseOnlyPositiveEmotes,
        UseOnlyNegativeEmotes,
        ClickQuestionable,
        ClickEthical,
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
        //private TaskContent _content;
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
        //public string Content { get => _content.Fi;}
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
            //_content = new(task.content);
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
                case "vote":
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
                case "switch_soulhome_music":
                    {
                        return TaskEducationActionType.SwitchSoulhomeMusic;
                    }
                case "edit_character_stats":
                    {
                        return TaskEducationActionType.EditCharacterStats;
                    }
                case "blow_up_your_character":
                    {
                        return TaskEducationActionType.BlowUpYourCharacter;
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
                case "emote_during_battle":
                    {
                        return TaskEducationSocialType.EmoteDuringBattle;
                    }
                case "write_chat_message_clan":
                    {
                        return TaskEducationSocialType.WriteChatMessageClan;
                    }
                case "edit_character_avatar":
                    {
                        return TaskEducationSocialType.EditCharacterAvatar;
                    }
                case "share_battle_replay":
                    {
                        return TaskEducationSocialType.ShareBattleReplay;
                    }
                case "add_new_friend":
                    {
                        return TaskEducationSocialType.AddNewFriend;
                    }
                case "create_new_vote":
                    {
                        return TaskEducationSocialType.CreateNewVote;
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
                case "click_character_description":
                    {
                        return TaskEducationStoryType.ClickCharacterDescription;
                    }
                case "continue_clan_story":
                    {
                        return TaskEducationStoryType.ContinueClanStory;
                    }
                case "recognize_sound_clue":
                    {
                        return TaskEducationStoryType.RecognizeSoundClue;
                    }
                case "find_symbolical_furniture":
                    {
                        return TaskEducationStoryType.FindSybolicalFurniture;
                    }
                case "find_symbolic_graphics":
                    {
                        return TaskEducationStoryType.FindSymbolicalGraphics;
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
                case "click_known_art_idea_person":
                    {
                        return TaskEducationCultureType.ClickKnownArtIdeaPerson;
                    }
                case "click_known_character":
                    {
                        return TaskEducationCultureType.ClickKnownCharacter;
                    }
                case "games_genre_types":
                    {
                        return TaskEducationCultureType.GamesGenreTypes;
                    }
                case "set_profile_player_type":
                    {
                        return TaskEducationCultureType.SetProfilePlayerType;
                    }
                case "similiar_to_a_game":
                    {
                        return TaskEducationCultureType.SimiliarToAGame;
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
                case "click_buyable":
                    {
                        return TaskEducationEthicalType.ClickBuyable;
                    }
                case "click_ethical":
                    {
                        return TaskEducationEthicalType.ClickEthical;
                    }
                case "click_questionable":
                    {
                        return TaskEducationEthicalType.ClickQuestionable;
                    }
                case "use_only_negative_emotes":
                    {
                        return TaskEducationEthicalType.UseOnlyNegativeEmotes;
                    }
                case "use_only_positive_emotes":
                    {
                        return TaskEducationEthicalType.UseOnlyPositiveEmotes;
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

            public string Fi { get => _fi;}

            public TaskTitle(ServerPlayerTask.TaskTitle title)
            {
                _fi = title.fi;
            }
        }

        public class TaskContent
        {
            private readonly string _fi;

            public string Fi { get => _fi;}
            public TaskContent(ServerPlayerTask.TaskContent content)
            {
                _fi = content.fi;
            }
        }

        #region Delegates, Events & Invoke Functions

        public delegate void TaskSelected();
        public event TaskSelected OnTaskSelected;
        public void InvokeOnTaskSelected()
        {
            OnTaskSelected.Invoke();
        }

        public delegate void TaskDeselected();
        public event TaskDeselected OnTaskDeselected;
        public void InvokeOnTaskDeselected()
        {
            OnTaskDeselected.Invoke();
        }

        public delegate void TaskUpdated();
        public event TaskUpdated OnTaskUpdated;
        public void InvokeOnTaskUpdated()
        {
            OnTaskUpdated.Invoke();
        }

        #endregion

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
        //public TaskContent content;
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
        }
        public class TaskContent
        {
            public string fi;
        }
    }
}

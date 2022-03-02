using System;

namespace UiProto.Scripts.Window
{
    [Serializable]
    public class LevelId // Read only 1 liner for Editor config popups
    {
        public int levelId;
        public string levelName => levelIdDef.levelName;
        public string unityName => levelIdDef.unityName;
        public bool isNetwork => levelIdDef.isNetwork;
        public bool isExcluded => levelIdDef.isExcluded;

        [NonSerialized]
        private LevelIdDef _def;

        private LevelIdDef levelIdDef => _def ?? (_def = LevelNames.getLevelIdDef(levelId));

        public static LevelId CreateFrom(LevelIdDef levelIdDef)
        {
            return new LevelId
            {
                levelId = levelIdDef.levelId,
                _def = levelIdDef,
            };
        }

        public override string ToString()
        {
            return levelIdDef.ToString();
        }
    }

    [Serializable]
    public class LevelIdDef // Full editing capabilities
    {
        public string levelName;
        public string unityName;
        public bool isNetwork;
        public bool isExcluded;
        public int levelId;

        public override string ToString()
        {
            if (levelId == 0 && string.IsNullOrEmpty(levelName))
            {
                return "current[0]";
            }
            return $"{levelName}({(isExcluded ? "EX:" : "")}{(isNetwork ? "NET:" : "")}{unityName})[{levelId}]";
        }
    }
}
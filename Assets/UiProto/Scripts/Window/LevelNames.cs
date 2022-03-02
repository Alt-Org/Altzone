using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UiProto.Scripts.Window
{
    //[CreateAssetMenu(menuName = "ALT-Zone/LevelNames")]
    public class LevelNames : ScriptableObject
    {
        public List<LevelIdDef> levels;

        public static LevelId getLevel(int levelId)
        {
            var levels = Resources.Load<LevelNames>(nameof(LevelNames));
            if (levels != null)
            {
                var level = levels.levels.FirstOrDefault(x => x.levelId == levelId);
                if (level != null)
                {
                    return new LevelId
                    {
                        levelId = level.levelId,
                    };
                }
            }
            return new LevelId();
        }

        public static LevelIdDef getLevelIdDef(int levelId)
        {
            var levels = Resources.Load<LevelNames>(nameof(LevelNames));
            if (levels != null)
            {
                var level = levels.levels.FirstOrDefault(x => x.levelId == levelId);
                if (level != null)
                {
                    return level;
                }
            }
            return new LevelIdDef();
        }

        public static List<LevelIdDef> loadLevelDefs()
        {
            var levels = Resources.Load<LevelNames>(nameof(LevelNames));
            if (levels != null)
            {
                return levels.levels;
            }
            return new List<LevelIdDef>();
        }
    }
}

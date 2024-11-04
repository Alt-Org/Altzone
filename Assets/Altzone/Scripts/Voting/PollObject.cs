using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    public enum PollType
    {
        Kauppa,
        Kirpputori,
        MemberPromote,
        MemberKick
    }

    public enum FurniturePollType
    {
        Buying,
        Selling,
        Recycling
    }

    public enum EsinePollType
    {
        Buying,
        Selling
    }

    public class PollObject
    {
        public PollType PollType;
        public string Id;
        public string Name;
        public long EndTime;
        public Sprite Sprite;

        public PollObject(PollType pollType, string id, string name, long endTime, Sprite sprite)
        {
            PollType = pollType;
            Id = id;
            Name = name;
            EndTime = endTime;
            Sprite = sprite;
        }
    }


    public class FurniturePollObject : PollObject
    {
        public FurniturePollType FurniturePollType;
        public GameFurniture Furniture;
        public double Weight;
        public float Value;

        public FurniturePollObject(PollType pollType, string id, string name, long endTime, Sprite sprite, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
        : base(pollType, id, name, endTime, sprite)
        {
            FurniturePollType = furniturePollType;
            Furniture = furniture;
            Weight = weight;
            Value = value;
        }
    }

    public class EsinePollObject : PollObject
    {
        public EsinePollType EsinePollType;
        public float Value;

        public EsinePollObject(PollType pollType, string id, string name, long endTime, Sprite sprite, EsinePollType esinePollType, float value)
        : base(pollType, id, name, endTime, sprite)
        {
            EsinePollType = esinePollType;
            Value = value;
        }
    }
}

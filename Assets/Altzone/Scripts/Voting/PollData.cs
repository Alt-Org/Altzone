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

    public class PollData
    {
        public PollType PollType;
        public string Id;
        public string Name;
        public long EndTime;
        public Sprite Sprite;

        public PollData(PollType pollType, string id, string name, long endTime, Sprite sprite)
        {
            PollType = pollType;
            Id = id;
            Name = name;
            EndTime = endTime;
            Sprite = sprite;
        }
    }


    public class FurniturePollData : PollData
    {
        public FurniturePollType FurniturePollType;
        public GameFurniture Furniture;
        public double Weight;
        public float Value;

        public FurniturePollData(PollType pollType, string id, string name, long endTime, Sprite sprite, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
        : base(pollType, id, name, endTime, sprite)
        {
            FurniturePollType = furniturePollType;
            Furniture = furniture;
            Weight = weight;
            Value = value;
        }
    }

    public class EsinePollData : PollData
    {
        public EsinePollType EsinePollType;
        public float Value;

        public EsinePollData(PollType pollType, string id, string name, long endTime, Sprite sprite, EsinePollType esinePollType, float value)
        : base(pollType, id, name, endTime, sprite)
        {
            EsinePollType = esinePollType;
            Value = value;
        }
    }
}
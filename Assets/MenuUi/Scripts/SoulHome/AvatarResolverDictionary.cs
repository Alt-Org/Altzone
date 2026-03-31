using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;

public static class AvatarResolverDictionary
{
    public static readonly Dictionary<AvatarPart, AvatarPartSetter.AvatarResolverStruct> Instance = new()
        {
            {
                AvatarPart.Body,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Clothes,
                    _category = "Body",
                    _suffix = "",
                }
            },
            {
                AvatarPart.R_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.L_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category ="Eyes",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.R_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.R_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyes",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.Nose,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Nose,
                    _category = "Nose",
                    _suffix = ""
                }
            },
            {
                AvatarPart.Mouth,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Mouth,
                    _category = "Mouth",
                    _suffix = ""
                }
            },
            {
                AvatarPart.R_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix= "R"
                }
            },
            {
                AvatarPart.L_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix = "L"
                }
            },
        };
}

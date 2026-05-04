using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "AvatarDefaults", menuName = "Avatar/Default Reference")]
public class AvatarDefaultReference : ScriptableObject
{
    [SerializeField] private List<AvatarDefault> _avatarDefaults = new List<AvatarDefault>();
    
    [SerializeField, HideInInspector] private List<AvatarDefaultClassCategoryInfo> _info;
    
    private void OnValidate()
    {
        if ((_avatarDefaults == null || _avatarDefaults.Count == 0) && _info != null && _info.Count > 0)
        {
            MigrateOldData();
        }
    }
    
    private void MigrateOldData()
    {
        _avatarDefaults = new List<AvatarDefault>();
        
        foreach (var classCategory in _info)
        {
            foreach (var character in classCategory.Characters)
            {
                if (character.Variations != null && character.Variations.Count > 0)
                {
                    var firstVariation = character.Variations[0];
                    var avatarDefault = new AvatarDefault
                    {
                        CharacterName = character.Name,
                        CharacterId = int.TryParse(classCategory.Id + character.Id, out int id) ? id : 701,
                        HairId = firstVariation.HairId,
                        EyesId = firstVariation.EyesId,
                        NoseId = firstVariation.NoseId,
                        MouthId = firstVariation.MouthId,
                        BodyId = firstVariation.BodyId,
                        HandsId = firstVariation.HandsId,
                        FeetId = firstVariation.FeetId
                    };
                    
                    for (int i = 1; i < character.Variations.Count; i++)
                    {
                        var variation = character.Variations[i];
                        avatarDefault.AlternativeVariations.Add(new AvatarPartVariation
                        {
                            VariationName = variation.Name,
                            HairId = variation.HairId,
                            EyesId = variation.EyesId,
                            NoseId = variation.NoseId,
                            MouthId = variation.MouthId,
                            BodyId = variation.BodyId,
                            HandsId = variation.HandsId,
                            FeetId = variation.FeetId
                        });
                    }
                    
                    _avatarDefaults.Add(avatarDefault);
                }
            }
        }
        
        Debug.Log($"Migrated {_avatarDefaults.Count} avatar defaults from old format");
    }
    
    public List<AvatarDefaultPartInfo> Get(string Id)
    {
        try
        {
            var avatar = GetAvatar(Id);
            if (avatar == null) 
            {
                avatar = _avatarDefaults.FirstOrDefault(a => a.AlternativeVariations.Count > 0) 
                        ?? _avatarDefaults.FirstOrDefault();
                
                if (avatar == null) return null;
            }
            
            var result = new List<AvatarDefaultPartInfo>();
            
            result.Add(new AvatarDefaultPartInfo
            {
                Name = avatar.CharacterName,
                HairId = avatar.HairId,
                EyesId = avatar.EyesId,
                NoseId = avatar.NoseId,
                MouthId = avatar.MouthId,
                BodyId = avatar.BodyId,
                HandsId = avatar.HandsId,
                FeetId = avatar.FeetId
            });
            
            foreach (var altVariation in avatar.AlternativeVariations)
            {
                result.Add(new AvatarDefaultPartInfo
                {
                    Name = altVariation.VariationName,
                    HairId = altVariation.HairId,
                    EyesId = altVariation.EyesId,
                    NoseId = altVariation.NoseId,
                    MouthId = altVariation.MouthId,
                    BodyId = altVariation.BodyId,
                    HandsId = altVariation.HandsId,
                    FeetId = altVariation.FeetId
                });
            }
            
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not find avatar part with ID: {Id}. Error: {e.Message}");
            return null;
        }
    }
    
    public List<AvatarDefaultPartInfo> GetByCharacterId(int Id)
    {
        if (!Enum.IsDefined(typeof(CharacterID), Id)) Id = 701;
        else if (CustomCharacter.IsTestCharacter((CharacterID)Id)) Id = 701;
        return (Get("0" + Id.ToString()[0] + Id.ToString()));
    }
    
    // Overloaded method for string ID
    public AvatarDefault GetAvatar(string stringId)
    {
        try
        {
            if (stringId.Length == 3)
            {
                if (int.TryParse(stringId.Substring(1), out int id))
                    return GetAvatar(id);
            }
            else if (int.TryParse(stringId, out int directId))
            {
                return GetAvatar(directId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not find avatar part with ID: {stringId}. Error: {e.Message}");
        }
        
        return _avatarDefaults.FirstOrDefault();
    }
    
    // Overloaded method for int ID
    public AvatarDefault GetAvatar(int characterId)
    {
        if (!Enum.IsDefined(typeof(CharacterID), characterId)) 
            characterId = 701;
            
        return _avatarDefaults.FirstOrDefault(a => a.CharacterId/100 == characterId/100) 
               ?? _avatarDefaults.FirstOrDefault();
    }
    
    [System.Serializable]
    public class AvatarDefaultPartInfo
    {
        public string Name = "Default";
        [Space]
        public string HairId;
        public string EyesId;
        public string NoseId;
        public string MouthId;
        public string BodyId;
        public string HandsId;
        public string FeetId;
    }
    
    [System.Serializable]
    public class AvatarDefaultCharacterInfo
    {
        public string Name;
        public string Id;
        public List<AvatarDefaultPartInfo> Variations;
    }
    
    [System.Serializable]
    public class AvatarDefaultClassCategoryInfo
    {
        public string Name;
        public string Id;
        public List<AvatarDefaultCharacterInfo> Characters;
    }
}

[System.Serializable]
public class AvatarDefault
{
    [Header("Character Info")]
    public string CharacterName = "Default Character";
    public int CharacterId = 701;
    
    [Header("Avatar Parts")]
    public string HairId = "";
    public string EyesId = "";
    public string NoseId = "";
    public string MouthId = "";
    public string BodyId = "";
    public string HandsId = "";
    public string FeetId = "";
    
    [Header("Alternative Parts (Optional)")]
    public List<AvatarPartVariation> AlternativeVariations = new List<AvatarPartVariation>();
}

[System.Serializable]
public class AvatarPartVariation
{
    public string VariationName = "Variation 1";
    [Space]
    public string HairId = "";
    public string EyesId = "";
    public string NoseId = "";
    public string MouthId = "";
    public string BodyId = "";
    public string HandsId = "";
    public string FeetId = "";
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AvatarDefault))]
public class AvatarDefaultEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        // Create property fields.
        var CharacterId = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.CharacterId)));
        var hairProperty = property.FindPropertyRelative(nameof(AvatarDefault.HairId));
        var hairField = new PropertyField(hairProperty, "Hair");
        var hairBox = new ScrollView();
        var eyeProperty = property.FindPropertyRelative(nameof(AvatarDefault.EyesId));
        var EyesField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.EyesId)), "Eyes");
        var eyesBox = new ScrollView();
        var noseProperty = property.FindPropertyRelative(nameof(AvatarDefault.NoseId));
        var NoseField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.NoseId)), "Nose");
        var noseBox = new ScrollView();
        var mouthProperty = property.FindPropertyRelative(nameof(AvatarDefault.MouthId));
        var MouthField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.MouthId)), "Mouth");
        var mouthBox = new ScrollView();
        var bodyProperty = property.FindPropertyRelative(nameof(AvatarDefault.BodyId));
        var BodyField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.BodyId)), "Body");
        var bodyBox = new ScrollView();
        var handsProperty = property.FindPropertyRelative(nameof(AvatarDefault.HandsId));
        var HandsField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.HandsId)), "Hands");
        var handsBox = new ScrollView();
        var feetProperty = property.FindPropertyRelative(nameof(AvatarDefault.FeetId));
        var FeetField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.FeetId)), "Feet");
        var feetBox = new ScrollView();

        // Add fields to the container.
        container.Add(CharacterId);

        container.Add(hairField);
        hairField.SetEnabled(false);
        container.Add(hairBox);
        hairField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box=hairBox, Piece=AvatarPiece.Hair, Field=hairProperty });
        hairBox.style.flexGrow = 1;
        hairBox.style.backgroundColor = Color.white;
        hairBox.style.height = 100f;
        hairBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(EyesField);
        EyesField.SetEnabled(false);
        container.Add(eyesBox);
        EyesField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = eyesBox, Piece = AvatarPiece.Eyes, Field = eyeProperty });
        eyesBox.style.flexGrow = 1;
        eyesBox.style.backgroundColor = Color.white;
        eyesBox.style.height = 100f;
        eyesBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(NoseField);
        NoseField.SetEnabled(false);
        container.Add(noseBox);
        NoseField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = noseBox, Piece = AvatarPiece.Nose, Field = noseProperty });
        noseBox.style.flexGrow = 1;
        noseBox.style.backgroundColor = Color.white;
        noseBox.style.height = 100f;
        noseBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(MouthField);
        MouthField.SetEnabled(false);
        container.Add(mouthBox);
        MouthField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = mouthBox, Piece = AvatarPiece.Mouth, Field = mouthProperty });
        mouthBox.style.flexGrow = 1;
        mouthBox.style.backgroundColor = Color.white;
        mouthBox.style.height = 100f;
        mouthBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(BodyField);
        BodyField.SetEnabled(false);
        container.Add(bodyBox);
        BodyField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = bodyBox, Piece = AvatarPiece.Clothes, Field = bodyProperty });
        bodyBox.style.flexGrow = 1;
        bodyBox.style.backgroundColor = Color.white;
        bodyBox.style.height = 100f;
        bodyBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(HandsField);
        HandsField.SetEnabled(false);
        container.Add(handsBox);
        HandsField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = handsBox, Piece = AvatarPiece.Hands, Field = handsProperty });
        handsBox.style.flexGrow = 1;
        handsBox.style.backgroundColor = Color.white;
        handsBox.style.height = 100f;
        handsBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(FeetField);
        FeetField.SetEnabled(false);
        container.Add(feetBox);
        FeetField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = feetBox, Piece = AvatarPiece.Feet, Field = feetProperty });
        feetBox.style.flexGrow = 1;
        feetBox.style.backgroundColor = Color.white;
        feetBox.style.height = 100f;
        feetBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;



        return container;
    }

    private void PartChanged(ChangeEvent<string> @event, BoxData boxData)
    {
        VisualElement box = boxData.Box;
        box.Clear();

        var value = @event.newValue;
        if (value == null) return;

        var bigBox = new Box();
        List<AvatarPartInfo> info = boxData.Piece switch
        {
            AvatarPiece.Hair => AvatarPartsReference.Instance.Hair.AvatarParts,
            AvatarPiece.Eyes => AvatarPartsReference.Instance.Eyes.AvatarParts,
            AvatarPiece.Nose => AvatarPartsReference.Instance.Nose.AvatarParts,
            AvatarPiece.Mouth => AvatarPartsReference.Instance.Mouth.AvatarParts,
            AvatarPiece.Clothes => AvatarPartsReference.Instance.Body.AvatarParts,
            AvatarPiece.Feet => AvatarPartsReference.Instance.Legs.AvatarParts,
            AvatarPiece.Hands => AvatarPartsReference.Instance.Arms.AvatarParts,
            _ => null,
        };

        foreach (var data in info)
        {
            var hairBox = new Button();
            hairBox.Clear();
            Image image = new();
            image.sprite = data.IconImage;
            hairBox.style.width = 50f;
            hairBox.style.height = 50f;
            if (data.Id == value) hairBox.style.backgroundColor = Color.blue;
            else hairBox.style.backgroundColor = Color.gray;
            hairBox.Add(image);
            hairBox.clicked += ClickedThis;
            bigBox.Add(hairBox);
            bigBox.style.width = 500f;
            bigBox.style.backgroundColor = Color.white;
            bigBox.style.flexDirection = FlexDirection.Row;
            bigBox.style.flexWrap = Wrap.Wrap;
            box.Add(bigBox);

            void ClickedThis() => Clicked(data);
        }

        void Clicked(AvatarPartInfo data)
        {
            boxData.Field.stringValue = data.Id;
            boxData.Field.serializedObject.ApplyModifiedProperties();
            boxData.Field.serializedObject.Update();
        }
    }

    private class BoxData
    {
        public VisualElement Box;
        public AvatarPiece Piece;
        public SerializedProperty Field;
    }
#endif
}

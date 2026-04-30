using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ModelV2.Internal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static AvatarPartsReference;

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

[CustomPropertyDrawer(typeof(AvatarDefault))]
public class AvatarDefaultEditor : PropertyDrawer
{

    PropertyField CharacterName;
    SerializedProperty CharacterId;
    PropertyField HairField;
    SerializedProperty EyesId;
    SerializedProperty NoseId;
    SerializedProperty MouthId;
    SerializedProperty BodyId;
    SerializedProperty HandsId;
    SerializedProperty FeetId;

    /*private void OnEnable()
    {
        SerializedObject serializedObject = new SerializedObject(AvatarDefault);

        CharacterName = serializedObject.FindProperty(nameof(AvatarDefault.CharacterName));
        CharacterId = serializedObject.FindProperty(nameof(AvatarDefault.CharacterId));
        HairId = serializedObject.FindProperty(nameof(AvatarDefault.HairId));
        EyesId = serializedObject.FindProperty(nameof(AvatarDefault.EyesId));
        NoseId = serializedObject.FindProperty(nameof(AvatarDefault.NoseId));
        MouthId = serializedObject.FindProperty(nameof(AvatarDefault.MouthId));
        BodyId = serializedObject.FindProperty(nameof(AvatarDefault.BodyId));
        HandsId = serializedObject.FindProperty(nameof(AvatarDefault.HandsId));
        FeetId = serializedObject.FindProperty(nameof(AvatarDefault.FeetId));
    }*/

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        // Create property fields.
        CharacterName = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.CharacterName)));
        PropertyField unitField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.CharacterId)));
        HairField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.HairId)), "Hair");
        var hairBox = new ScrollView();
        PropertyField eyeField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.EyesId)), "Eyes");
        PropertyField noseField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.NoseId)), "Nose");
        PropertyField mouthField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.MouthId)), "Mouth");
        PropertyField bodyField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.BodyId)), "Body");
        PropertyField handsField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.HandsId)), "Hands");
        PropertyField feetField = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefault.FeetId)), "Feet");

        // Add fields to the container.
        container.Add(CharacterName);
        container.Add(unitField);
        container.Add(HairField);
        container.Add(hairBox);

        HairField.RegisterCallback<ChangeEvent<string>, VisualElement>(PartChanged, hairBox);
        hairBox.style.flexGrow = 1;
        hairBox.style.backgroundColor = Color.white;
        hairBox.style.height = 100f;
        hairBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        container.Add(eyeField);
        container.Add(noseField);
        container.Add(mouthField);
        container.Add(bodyField);
        container.Add(handsField);
        container.Add(feetField);



        return container;
    }

    private void PartChanged(ChangeEvent<string> @event, VisualElement box)
    {
        box.Clear();

        var value = @event.newValue;
        if(value == null) return;

        var bigBox = new Box();

        List<AvatarPartInfo> info = AvatarPartsReference.Instance.Hair.AvatarParts;

        foreach(var data in info)
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
            bigBox.Add(hairBox);
            bigBox.style.width = 500f;
            bigBox.style.backgroundColor = Color.white;
            bigBox.style.flexDirection = FlexDirection.Row;
            bigBox.style.flexWrap = Wrap.Wrap;
            box.Add(bigBox);
            //box.style.alignContent = Align.Auto;
        }


        //Image image = new();
        //image.sprite = AvatarPartsReference.Instance.GetAvatarPartById(value).AvatarImage;

        //box.Add(image);
    }
}

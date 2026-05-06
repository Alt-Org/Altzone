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
    [SerializeField] private List<AvatarDefaultParts> _avatarDefaults = new List<AvatarDefaultParts>();
    
}

[System.Serializable]
public class AvatarDefaultParts
{
    [Header("Character Info")]
    public int CharacterId = 701;
    
    [Header("Avatar Parts")]
    public string HairId = "";
    public Color HairColour = Color.white;
    public string EyesId = "";
    public Color EyesColour = Color.white;
    public string NoseId = "";
    public Color NoseColour = Color.white;
    public string MouthId = "";
    public Color MouthColour = Color.white;
    public string BodyId = "";
    public Color BodyColour = Color.white;
    public string HandsId = "";
    public Color HandsColour = Color.white;
    public string FeetId = "";
    public Color FeetColour = Color.white;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AvatarDefaultParts))]
public class AvatarDefaultEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        // Create property fields.
        var CharacterId = new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.CharacterId)));
        var hairProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.HairId));
        var hairField = new PropertyField(hairProperty, "Hair");
        var hairBox = new ScrollView();
        var eyeProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.EyesId));
        var EyesField = new PropertyField(eyeProperty, "Eyes");
        var eyesBox = new ScrollView();
        var noseProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.NoseId));
        var NoseField = new PropertyField(noseProperty, "Nose");
        var noseBox = new ScrollView();
        var mouthProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.MouthId));
        var MouthField = new PropertyField(mouthProperty, "Mouth");
        var mouthBox = new ScrollView();
        var bodyProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.BodyId));
        var BodyField = new PropertyField(bodyProperty, "Body");
        var bodyBox = new ScrollView();
        var handsProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.HandsId));
        var HandsField = new PropertyField(handsProperty, "Hands");
        var handsBox = new ScrollView();
        var feetProperty = property.FindPropertyRelative(nameof(AvatarDefaultParts.FeetId));
        var FeetField = new PropertyField(feetProperty, "Feet");
        var feetBox = new ScrollView();

        // Add fields to the container.
        container.Add(CharacterId);

        container.Add(hairField);
        hairField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.HairColour))));
        container.Add(hairBox);
        hairField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box=hairBox, Piece=AvatarPiece.Hair, Field=hairProperty });
        hairBox.style.flexGrow = 1;
        hairBox.style.backgroundColor = Color.white;
        hairBox.style.height = 100f;
        hairBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(EyesField);
        EyesField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.EyesColour))));
        container.Add(eyesBox);
        EyesField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = eyesBox, Piece = AvatarPiece.Eyes, Field = eyeProperty });
        eyesBox.style.flexGrow = 1;
        eyesBox.style.backgroundColor = Color.white;
        eyesBox.style.height = 100f;
        eyesBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(NoseField);
        NoseField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.NoseColour))));
        container.Add(noseBox);
        NoseField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = noseBox, Piece = AvatarPiece.Nose, Field = noseProperty });
        noseBox.style.flexGrow = 1;
        noseBox.style.backgroundColor = Color.white;
        noseBox.style.height = 100f;
        noseBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(MouthField);
        MouthField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.MouthColour))));
        container.Add(mouthBox);
        MouthField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = mouthBox, Piece = AvatarPiece.Mouth, Field = mouthProperty });
        mouthBox.style.flexGrow = 1;
        mouthBox.style.backgroundColor = Color.white;
        mouthBox.style.height = 100f;
        mouthBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(BodyField);
        BodyField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.BodyColour))));
        container.Add(bodyBox);
        BodyField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = bodyBox, Piece = AvatarPiece.Clothes, Field = bodyProperty });
        bodyBox.style.flexGrow = 1;
        bodyBox.style.backgroundColor = Color.white;
        bodyBox.style.height = 100f;
        bodyBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(HandsField);
        HandsField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.HandsColour))));
        container.Add(handsBox);
        HandsField.RegisterCallback<ChangeEvent<string>, BoxData>(PartChanged, new BoxData { Box = handsBox, Piece = AvatarPiece.Hands, Field = handsProperty });
        handsBox.style.flexGrow = 1;
        handsBox.style.backgroundColor = Color.white;
        handsBox.style.height = 100f;
        handsBox.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        container.Add(FeetField);
        FeetField.SetEnabled(false);
        container.Add(new PropertyField(property.FindPropertyRelative(nameof(AvatarDefaultParts.FeetColour))));
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

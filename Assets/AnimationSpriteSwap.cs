using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using MenuUI.Scripts.SoulHome;
using UnityEngine;
using UnityEngine.U2D.Animation;
using static MenuUI.Scripts.SoulHome.AvatarPartSetter;

public class AnimationSpriteSwap : MonoBehaviour
{
    [SerializeField]
    private SpriteResolver _rHandResolver;

    private string _rHandLabel;

    private void Start()
    {
        _rHandLabel = _rHandResolver.GetLabel();
    }

    private bool IntelectualizerAnimationSwap(string id)
    {
        if (id != string.Empty)
        {
            ChangeSprite(_rHandResolver.GetComponent<AvatarPartResolver>(), id);
        }
        else
        {
            string category = _rHandResolver.GetCategory();
            _rHandResolver.SetCategoryAndLabel(category, _rHandLabel);
        }
        return false;
    }

    private void ChangeSprite(AvatarPartResolver resolver, string id)
    {
        if (resolver == null)
        {
            return;
        }

        if (!AvatarResolverDictionary.Instance.TryGetValue(resolver.Part, out AvatarPartSetter.AvatarResolverStruct resolverStruct))
        {
            return;
        }

        string category = resolver.Resolver.GetCategory();

        string label = int.TryParse(id, out int value)?ResolveLabel(resolverStruct, id):id;

        resolver.Resolver.SetCategoryAndLabel(category, label);
    }

    private static string ResolveLabel(AvatarResolverStruct resolverStruct, string id)
    {
        if (id == null || resolverStruct._dataGetter == null)
        {
            return "0000000" + resolverStruct._suffix;
        }

        if (id.Length != 7)
        {
            return "0000000" + resolverStruct._suffix;
        }

        return id + resolverStruct._suffix;
    }
}

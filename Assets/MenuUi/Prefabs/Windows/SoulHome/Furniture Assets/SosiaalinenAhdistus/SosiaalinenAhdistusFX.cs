using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SosiaalinenAhdistusFX : MonoBehaviour
{

    [SerializeField] private float _alphaTime = 0.75f;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private int _alphaAmount = Shader.PropertyToID("_AlphaAmount");


    private void Start()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        _materials = new Material[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    private IEnumerator Vanish()
    {
        float elapsedTime = 0f;
        while(elapsedTime < _alphaTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedAlpha = Mathf.Lerp(0f, 1f, (elapsedTime / _alphaTime));

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_alphaAmount, lerpedAlpha);
            
            }

            yield return null;
        }
    }

    private IEnumerator Appear()
    {
        float elapsedTime = 0f;
        while(elapsedTime < _alphaTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedAlpha = Mathf.Lerp(1f, 0f, (elapsedTime / _alphaTime));

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_alphaAmount, lerpedAlpha);
            }

            yield return null;
        }
    }
    
}

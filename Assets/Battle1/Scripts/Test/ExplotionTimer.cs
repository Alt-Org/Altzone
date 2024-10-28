using System.Collections;
using UnityEngine;

namespace Battle1.Scripts.Test
{
    public class ExplotionTimer : MonoBehaviour
    {
        [SerializeField] GameObject _explotionAnimationGameObject;
        [SerializeField] SpriteRenderer _shadowSpriteRenderer;
        Animator _explotionAnimator;

        void Awake()
        {
            _explotionAnimator = _explotionAnimationGameObject.GetComponent<Animator>();
            //Debug.Log(""+transform.position);
            /*
        StartCoroutine(ExplotionAnimation());
        StartCoroutine(ShadowAnimation());
        */
            StartCoroutine(Die());
        }

        /*
    private IEnumerator ExplotionAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        _explotionAnimator.enabled = true;
        yield return new WaitForSeconds(2f);
        Destroy(_explotionAnimationGameObject);
    }

    private IEnumerator ShadowAnimation()
    {
        float alpha = 0;
        while (alpha < 0.4f)
        {
            yield return null;
            alpha += (0.4f / 0.3f) * Time.deltaTime;
            _shadowSpriteRenderer.color = new Color(0, 0, 0,alpha);
        }
        yield return new WaitForSeconds(2f);
        while (alpha > 0.0f)
        {
            yield return null;
            alpha += (-0.4f / 0.5f) * Time.deltaTime;
            _shadowSpriteRenderer.color = new Color(0, 0, 0, alpha);
        }
    }
    */

        private IEnumerator Die()
        {
            _explotionAnimator.enabled = true;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }
}

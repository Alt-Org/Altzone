using System.Collections;
using TMPro;
using UnityEngine;

namespace Battle1.Scripts.Ui
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMeshpro;

        private void Awake()
        {
            StartCoroutine(Wait());
        }
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(0.5f);
            _textMeshpro.SetText("Ready");
            yield return new WaitForSeconds(0.5f);
            _textMeshpro.SetText("Set");
            yield return new WaitForSeconds(0.5f);
            _textMeshpro.SetText("Go");
            yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
        }
    }
}

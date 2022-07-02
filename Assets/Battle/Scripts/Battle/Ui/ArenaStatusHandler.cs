using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Battle.Ui
{
    public class ArenaStatusHandler : MonoBehaviour
    {
        [Header("TeamAreaStatus"), SerializeField] private Sprite _frozen;

        [Header("TeamAreas"), SerializeField] private GameObject _redArea;
        [SerializeField] private GameObject _blueArea;

        private void Awake()
        {
            SetSprite();
        }

        private void SetSprite()
        {
            _redArea.GetComponent<Image>().sprite = _frozen;
            _blueArea.GetComponent<Image>().sprite = _frozen;
            _redArea.SetActive(false);
            _blueArea.SetActive(false);
        }

        public void ChangeArenaState(bool red, bool blue)
        {
            _redArea.SetActive(red);
            _blueArea.SetActive(blue);
        }
    }
}
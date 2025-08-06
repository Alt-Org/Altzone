/// @file BattleScreenEffectViewController.cs
/// <summary>
/// Handles screen overlay and particle effect changes.
/// </summary>
///
/// This script:<br/>
/// Changes screen overlay's color.<br/>
/// Toggles particle systems on both sides of the screen to active/inactive.

using UnityEngine;

namespace Battle.View.Effect
{
    /// <summary>
    /// <span class="brief-h">Screen effect <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles screen overlay and particle effect changes.
    /// </summary>
    public class BattleScreenEffectViewController : MonoBehaviour
    {
        /// <value>[SerializeField] The screen overlay GameObject.</value>
        [SerializeField] private GameObject _overlay;

        /// <value>[SerializeField] An array of all particleSources.</value>
        [SerializeField] private GameObject[] _particleSources;

        /// <value>[SerializeField] An array of usable overlay colors.</value>
        [SerializeField] private Color[] _colors;

        /// <value>Are screen effects active or not.</value>
        public bool IsActive {get ; private set ; } = false;

        /// <summary>
        /// Sets screen overlay and all particleSources active or inactive.
        /// </summary>
        /// 
        /// <param name="active">True/False : active/inactive</param>
        public void SetActive(bool active)
        {
            foreach (GameObject particleSource in _particleSources) particleSource.SetActive(active);
            _overlay.SetActive(active);
            IsActive = active;
        }

        /// <summary>
        /// Public method that changes screen overlay's color.
        /// </summary>
        /// 
        /// <param name="colorIndex">Index number of wanted color</param>
        public void ChangeColor(int colorIndex)
        {
            _spriteRenderer.color = _colors[colorIndex];
        }

        /// <value>Screen overlay GameObject's SpriteRenderer component.</value>
        private SpriteRenderer _spriteRenderer;

        /// <summary>
        /// Private method that gets called when script instance is being loaded. Gets overlay's SpriteRenderer component.
        /// </summary>
        private void Awake()
        {
            _spriteRenderer = _overlay.GetComponent<SpriteRenderer>();
        }
    }
}

/// @file BattleCameraTest.cs
/// <summary>
/// Contains @cref{Battle.View.Game,BattleCameraTest} class for BattleCamera testing.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle camera testing functionality.

// Unity usings
using UnityEngine;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">%Battle camera test <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles %Battle camera testing functionality.
    /// </summary>
    public class BattleCameraTest : MonoBehaviour
    {
        /// @anchor BattleCameraTest-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] If the camera should be active or not.</summary>
        /// @ref BattleCameraTest-SerializeFields
        [SerializeField] private bool _active;

        /// <summary>[SerializeField] %Battle arena scale.</summary>
        /// @ref BattleCameraTest-SerializeFields
        [SerializeField] private float _scale;

        /// <summary>[SerializeField] %Battle arena offset.</summary>
        /// @ref BattleCameraTest-SerializeFields
        [SerializeField] private Vector2 _offset;

        /// <summary>[SerializeField] If the camera should rotate or not.</summary>
        /// @ref BattleCameraTest-SerializeFields
        [SerializeField] private bool _rotate;

        /// @}

        /// <value>Previous #_active value.</value>
        private bool _activePrev;

        /// <value>Previous #_scale value.</value>
        private float _scalePrev;

        /// <value>Previous #_offset value.</value>
        private Vector2 _offsetPrev;

        /// <value>Previous #_rotate value.</value>
        private bool _rotatePrev;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method which calls BattleCamera::SetView if the SerializeField variables changed.
        /// </summary>
        private void Update()
        {
            if (!_active)
            {
                if (_active == _activePrev) return;
                BattleCamera.UnsetView();
                _activePrev = _active;
                return;
            }

            if (
                _active == _activePrev &&
                _scale  == _scalePrev  &&
                _offset == _offsetPrev &&
                _rotate == _rotatePrev
            ) return;


            if (_scale <= 0.0f) _scale = _scalePrev;

            _offset = new Vector2(
                Mathf.Clamp01(_offset.x),
                Mathf.Clamp01(_offset.y)
            );

            BattleCamera.SetView(_scale, _offset, _rotate);

            _activePrev = _active;
            _scalePrev = _scale;
            _offsetPrev = _offset;
            _rotatePrev = _rotate;
        }
    }
}

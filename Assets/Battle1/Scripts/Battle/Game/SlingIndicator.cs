using UnityEngine;

namespace Battle1.Scripts.Battle.Game
{
    internal class SlingIndicator : MonoBehaviour
    {
        #region Public Methods

        /// <summary>
        /// Shows or hides sling indicator.
        /// </summary>
        /// <param name="show">true = show, false = hide</param>
        public void SetShow(bool show)
        {
            _spriteRenderer.enabled = show;
            //foreach (Wing wing in _wings) wing.SpriteRenderer.enabled = show;
            _pusher.GameObject.SetActive(show);
        }

        /// <summary>
        /// Sets the position of the sling indicator.
        /// </summary>
        /// <param name="position">The new position of the sling indicator.</param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Sets the rotation/angle of the sling indicator in radians.
        /// </summary>
        /// <param name="angle">The new angle of sling indicator in radians.</param>
        public void SetRotationRadians(float angle) { SetRotationDegrees(angle * (360 / (Mathf.PI * 2.0f))); }

        /// <summary>
        /// Sets the rotation/angle of the sling indicator in degrees.
        /// </summary>
        /// <param name="angle">The new angle of sling indicator in degrees.</param>
        public void SetRotationDegrees(float angle)
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// Sets the length of the sling indicator.
        /// </summary>
        /// <param name="length">The new length of the sling indicator.</param>
        public void SetLength(float length)
        {
            _length = length;
            _spriteRenderer.size = new Vector2(_length * (1 / Scale), 5);

            /*
            const float WING_OFFSET = -1f;
            float wingX = _length * (0.5f / Scale) + WING_OFFSET;
            _wings[WING_LEFT].Transform.localPosition = new Vector3(wingX, 0.6f);
            _wings[WING_RIGHT].Transform.localPosition = new Vector3(wingX, -0.6f);
            */

            UpdatePusherPosition();
        }

        /*
        public void SetWingAngleRadians(float angle) { SetRotationDegrees(angle * (360 / (Mathf.PI * 2.0f))); }
        public void SetWingAngleDegrees(float angle)
        {
            _wings[WING_LEFT].Transform.localRotation = Quaternion.AngleAxis(-90 + angle, Vector3.forward);
            _wings[WING_RIGHT].Transform.localRotation = Quaternion.AngleAxis(-90 - angle, Vector3.forward);
        }
        */

        /// <summary>
        /// Sets the position of the pusher. min 0, max 1
        /// </summary>
        /// <param name="position">The new position of the pusher. min 0, max 1</param>
        public void SetPusherPosition(float position)
        {
            _pusher.Position = position;
            UpdatePusherPosition();
        }

        #endregion Public Methods

        #region Private

        #region Private - Constants
        const float Scale = 0.35f; // this should match transform scale
        #endregion Private - Constants

        #region Private - Fields

        // Components
        private SpriteRenderer _spriteRenderer;

        private float _length;

        /*
        // Wings
        private const int WING_LEFT  = 0;
        private const int WING_RIGHT = 1;
        private struct Wing
        {
            public Transform Transform;
            public SpriteRenderer SpriteRenderer;

            public Wing(Transform transform)
            {
                Transform = transform;
                SpriteRenderer = transform.GetComponent<SpriteRenderer>();
            }
        }
        private readonly Wing[] _wings = new Wing[2];
        */

        // Pusher
        private struct Pusher
        {
            public GameObject GameObject;
            public Transform Transform;
            public float Position;

            public Pusher(Transform transform)
            {
                GameObject = transform.gameObject;
                Transform = transform;
                Position = 0;
            }
        }
        Pusher _pusher;

        #endregion Private - Fields

        #region Private - Methods

        private void Start()
        {
            // get components
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _length = 0;

            /*
            // setup wings
            _wings[WING_LEFT]  = new(transform.Find("WingLeft"));
            _wings[WING_RIGHT] = new(transform.Find("WingRight"));
            */

            // setup pusher
            _pusher = new(transform.Find("Pusher"));
        }

        private void UpdatePusherPosition()
        {
            const float PusherStartOffset = 5.4f;
            float pusherX =
                _length * (-0.5f / Scale) + PusherStartOffset
                + ((_length * (1f / Scale) - PusherStartOffset) * _pusher.Position);
            _pusher.Transform.localPosition = new Vector3(pusherX, 0);
        }

        #endregion Private - Methods

        #endregion Private
    }
}

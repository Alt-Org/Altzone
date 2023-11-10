using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingIndicatorTest : MonoBehaviour
{
    #region Public Methods 

    public void SetShow(bool show)
    {
        _spriteRenderer.enabled = show;
        foreach (Wing wing in _wings) wing.SpriteRenderer.enabled = show;
        _pusher.GameObject.SetActive(show);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetRotationRadians(float angle) { SetRotationDegrees(angle * (360 / (Mathf.PI * 2.0f))); }
    public void SetRotationDegrees(float angle)
    {
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetLength(float length)
    {
        _length = length;
        _spriteRenderer.size = new Vector2(_length * (1 / SCALE), 5);

        const float WING_OFFSET = -1f;
        float wingX = _length * (0.5f / SCALE) + WING_OFFSET;
        _wings[WING_LEFT].Transform.localPosition = new Vector3(wingX, 0.6f);
        _wings[WING_RIGHT].Transform.localPosition = new Vector3(wingX, -0.6f);

        UpdatePusherPosition();
    }

    public void SetWingAngleRadians(float angle) { SetRotationDegrees(angle * (360 / (Mathf.PI * 2.0f))); }
    public void SetWingAngleDegrees(float angle)
    {
        _wings[WING_LEFT].Transform.localRotation = Quaternion.AngleAxis(-90 + angle, Vector3.forward);
        _wings[WING_RIGHT].Transform.localRotation = Quaternion.AngleAxis(-90 - angle, Vector3.forward);
    }

    public void SetPusherPosition(float position)
    {
        _pusher.Position = position;
        UpdatePusherPosition();
    }

    #endregion Public Methods

    // Private Constants
    const float SCALE = 0.35f; // this should match transfor scale

    // Components
    private SpriteRenderer _spriteRenderer;

    private float _length;

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
    private Wing[] _wings = new Wing[2];

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

    void Start()
    {
        // get components
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _length = 0;

        // setup wings
        _wings[WING_LEFT]  = new(transform.Find("WingLeft"));
        _wings[WING_RIGHT] = new(transform.Find("WingRight"));

        // setup pusher
        _pusher = new(transform.Find("Pusher"));
    }

    private void UpdatePusherPosition()
    {
        const float PUSHER_START_OFFSET = 5.4f;
        float pusherX =
            (_length * (-0.5f / SCALE) + PUSHER_START_OFFSET)
            + ((_length * (1f / SCALE) - PUSHER_START_OFFSET) * _pusher.Position);
        _pusher.Transform.localPosition = new Vector3(pusherX, 0);
    }

}

using UnityEngine;

public class RotateAura : MonoBehaviour
{
    [SerializeField] private float Speed;

    private void Update()
    {
        transform.Rotate(0f, 0f, 90f * Time.deltaTime * Speed);
    }
}

using UnityEngine;

[RequireComponent(typeof(BaseScrollRect))]
public class ShopScrollViewResetter : MonoBehaviour
{
    private BaseScrollRect myScrollRect;
    private void Awake()
    {
        myScrollRect = GetComponent<BaseScrollRect>();
        Debug.Log("Awake");
    }
    public void OnEnable()
    {
        Debug.Log("OnEnable");
        //Change the current vertical scroll position.
        myScrollRect.VerticalNormalizedPosition = 1f;
    }
}

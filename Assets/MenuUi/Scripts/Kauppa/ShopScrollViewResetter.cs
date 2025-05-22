using UnityEngine;

[RequireComponent(typeof(BaseScrollRect))]
public class ShopScrollViewResetter : MonoBehaviour
{
    private BaseScrollRect myScrollRect;
    private void Start()
    {
        myScrollRect = GetComponent<BaseScrollRect>();
        ResetScroll();
    }
    public void OnEnable()
    {
        if(myScrollRect == null)
            myScrollRect = GetComponent<BaseScrollRect>();

        ResetScroll();
    } 

    private void ResetScroll() => myScrollRect.VerticalNormalizedPosition = 1f;
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //Määritetään mikä on hiiren pohjassa pito ja irrottaminen. Tällä tavalla se tunnistaa milloin nappia painetaan ja pohjassa pito aika. 
    private bool pointerDown;
    private float pointerDownTimer;

    public float requiredHoldTime;

    //Tähän luodaan kokonaan oma Click systeemi. Tässä tapauksessa se on "On long click". Eli nyt pitää nappia pitää pohjassa jotta se toimii.
    public UnityEvent onLongClick;

    //Tässä on jos haluaa asettaa napille semmosen animaation kun sitä painaa. (Minulla se toimii nyt vähän eritavalla)
    [SerializeField]
    private Image fillImage;
    

    //Määritetään että kun nappi on pohjassa ja ei paineta enää. Plus tulostaa tekstin josta tietää että se toimii niinkuin pitää. 
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        Debug.Log("OnPointerDown");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
        Debug.Log("OnPointerUp");
    }

    //Määritetään jos nappi on ollu pohjassa sen ajan mitä pitää, silloin tapahtuu jotain (määritetään inspector) mitä halutaan.
    private void Update()
    {
        if (pointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                if (onLongClick != null)
                    onLongClick.Invoke();

                Reset();
            }
            fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
        }
    }

    //Jotta nappi toimii uudelleen laitetaan reset. Silloin se loop takaisin että nappia voi pitää pohjassa uudestaan.
    private void Reset()
    {
        pointerDown = false;
        pointerDownTimer = 0;
        fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
    }
}
/* https://www.youtube.com/watch?v=7caRQKg0FfY */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatWindowTabManager : MonoBehaviour
{
    [Header("Stat window tabs")]
    [SerializeField] private GameObject impactforceTab;
    [SerializeField] private GameObject healthPointsTab;
    [SerializeField] private GameObject defenceTab;
    [SerializeField] private GameObject resistanceTab;
    [SerializeField] private GameObject charSizeTab;
    [SerializeField] private GameObject speedTab;


    [Header("Buttons for opening tabs")]
    [SerializeField] private Button impactforce;
    [SerializeField] private Button healthPoints;
    [SerializeField] private Button defence;
    [SerializeField] private Button resistance;
    [SerializeField] private Button charSize;
    [SerializeField] private Button speed;

    //Ajatuksena, että tällä scriptillä hallitaan kehitysvälilehtien vaihtaminen.
    //Toiminnasta ei ole tietoa, kun en ole päässyt vielä testaamaan ennen kuin 
    //uusi statti-ikkuna on valmis.

    private void OnEnable()
    {   
        impactforce.onClick.AddListener(() => SwitchToTab(impactforceTab));
        healthPoints.onClick.AddListener(() => SwitchToTab(healthPointsTab));
        defence.onClick.AddListener(() => SwitchToTab(defenceTab));
        resistance.onClick.AddListener(() => SwitchToTab(resistanceTab));
        charSize.onClick.AddListener(() => SwitchToTab(charSizeTab));
        speed.onClick.AddListener(() => SwitchToTab(speedTab));
    }
    private void SwitchToTab(GameObject tab) //Tällä hoidetaan välilehden vaihto.
    {
        HideAll(); //Aluksi piilotetaan kaikki ikkunat.
        tab.SetActive(true); //Asetetaan haluttu välilheti aktiiviseksi.
        tab.transform.SetAsLastSibling(); //Vaihdetaan haluttu välilehti päällimäiseksi.
    }
    private void HideAll()
    {
        impactforceTab.SetActive(false);
        healthPointsTab.SetActive(false);
        defenceTab.SetActive(false);
        resistanceTab.SetActive(false);
        charSizeTab.SetActive(false);
        speedTab.SetActive(false);
    }
}

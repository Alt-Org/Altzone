using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ClanValues{
    Eläinrakkaat,
    Maahanmuuttomyönteiset,
    Lgbtq,
    Raittiit,
    Kohteliaat,
    Kiusaamisenvastaiset,
    Urheilevat,
    Syvälliset,
    Oikeudenmukaiset,
    Kaikkienkaverit,
    Itsenäiset,
    Retkeilijät,
    Suomenruotsalaiset,
    Huumorintajuiset,
    Rikkaat,
    Ikiteinit,
    Juoruilevat,
    Rakastavat,
    Oleilijat,
    Nörtit,
    Musadiggarit,
    Tunteelliset,
    Gamerit,
    Animefanit,
    Sinkut,
    Monikulttuuriset,
    Kauniit,
    Järjestelmälliset,
    Epäjärjestelmälliset,
    Tasaarvoiset,
    Somepersoonat,
    Kädentaitajat,
    Muusikot,
    Taiteilijat,
    Spämmääjät,
    Kasvissyöjät,
    Tasapainoiset,


}
public class ValuePanelController : MonoBehaviour
{
    [SerializeField]
    private GameObject _LabelPrefab;
    [SerializeField] private Transform _layouttransform;
    // Start is called before the first frame update
    void Start()
    {
        CreateLabels();
    }
    public void CreateLabels()
    {
        for (int i = _layouttransform.childCount - 1; i >= 0; i--)
        {
            Destroy(_layouttransform.GetChild(i).gameObject);
        }
        foreach (ClanValues values in Enum.GetValues(typeof(ClanValues)))
        {
            GameObject labelPanel=Instantiate(_LabelPrefab, _layouttransform);
            labelPanel.GetComponent<ValuelabelHandle>().SetLabelInfo(values);
        }
    }
}

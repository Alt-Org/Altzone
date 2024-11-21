using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ClanValues{
    El�inrakkaat,
    Maahanmuuttomy�nteiset,
    Lgbtq,
    Raittiit,
    Kohteliaat,
    Kiusaamisenvastaiset,
    Urheilevat,
    Syv�lliset,
    Oikeudenmukaiset,
    Kaikkienkaverit,
    Itsen�iset,
    Retkeilij�t,
    Suomenruotsalaiset,
    Huumorintajuiset,
    Rikkaat,
    Ikiteinit,
    Juoruilevat,
    Rakastavat,
    Oleilijat,
    N�rtit,
    Musadiggarit,
    Tunteelliset,
    Gamerit,
    Animefanit,
    Sinkut,
    Monikulttuuriset,
    Kauniit,
    J�rjestelm�lliset,
    Ep�j�rjestelm�lliset,
    Tasaarvoiset,
    Somepersoonat,
    K�dentaitajat,
    Muusikot,
    Taiteilijat,
    Sp�mm��j�t,
    Kasvissy�j�t,
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

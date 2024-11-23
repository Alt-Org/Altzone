using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ClanValues{
    Elainrakkaat,
    Maahanmuuttomyonteiset,
    Lgbtq,
    Raittiit,
    Kohteliaat,
    Kiusaamisenvastaiset,
    Urheilevat,
    Syvalliset,
    Oikeudenmukaiset,
    Kaikkienkaverit,
    Itsenaiset,
    Retkeilijat,
    Suomenruotsalaiset,
    Huumorintajuiset,
    Rikkaat,
    Ikiteinit,
    Juoruilevat,
    Rakastavat,
    Oleilijat,
    Nortit,
    Musadiggarit,
    Tunteelliset,
    Gamerit,
    Animefanit,
    Sinkut,
    Monikulttuuriset,
    Kauniit,
    Jarjestelmalliset,
    Epajarjestelmalliset,
    Tasaarvoiset,
    Somepersoonat,
    Kadentaitajat,
    Muusikot,
    Taiteilijat,
    Spammaajat,
    Kasvissyojat,
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

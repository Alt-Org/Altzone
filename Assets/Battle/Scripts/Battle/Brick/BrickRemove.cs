using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BrickRemove : MonoBehaviour
{
    private int _shieldHP = 0;

    void OnCollisionEnter2D(Collision2D col)
    {
        if(_shieldHP > 0)
        {
            --_shieldHP;
            return;
        }
        if (col.gameObject && _shieldHP == 0)
        {
           PhotonNetwork.Destroy(this.gameObject);
        }
    }
}

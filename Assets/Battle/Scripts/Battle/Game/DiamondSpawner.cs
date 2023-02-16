using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSpawner : MonoBehaviour
{

    //viholliset ilmestyvät pelaajan scenelle
    [SerializeField] float X;
    [SerializeField] float minY;
    [SerializeField] float maxY;

    // mitä spawner spawnaa
    [SerializeField] GameObject Diamond;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnDiamond());
    }

    //vihollisten ajastus
    //- käyttää "Coroutineja", joilla ajastetaan tapahtumia
    //- näiden kanssa oltava varovainen, ei saa laittaa montaa "sisäkkäin"
    IEnumerator SpawnDiamond()
    {
        //esim. 0.5-1.5 sek välein syntyy uusi vihollinen
        yield return new WaitForSeconds(Random.Range(5f, 10f));

        //Instantiate tekee prefabista kopion, haluttuun sijaintiin
        //- sallittujen X-arvojen sisällä
        //Quaternion.identity on rotaatio-komento, joka asettaa viholliselle rotaatiot
        var DiamondParent = GameObject.Instantiate(Diamond, new Vector3(X, Random.Range(minY, maxY)), Quaternion.Euler (0f, 0f, 90f));
        DiamondParent.transform.parent = transform;

        //kutsuu itseään uudelleen
        StartCoroutine(SpawnDiamond());
    }

    /*public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
    }*/
}

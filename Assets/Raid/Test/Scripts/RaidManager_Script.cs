using UnityEngine;

public class RaidManager_Script : MonoBehaviour
{
    public int minefieldWidth;
    public int minefieldHeight;
    public int numberOfBombs;
    //public int number;

    private Field_Script field;
    private Hexa_struct[,] state;
    private bool raidIsOver;

    private void Awake()
    {
        field = GetComponentInChildren<Field_Script>();
    }

    private void Start()
    {
        StartRaid();      
    }
    private void Update()
    {
        // (DEV) Bugi inputissa: Kokeile Project Settings > Player > Active Input > Both!!!

        if(!raidIsOver) //(DEV) Vaihda jos ei toimi, kuten esim. == false. Tai kokeile tuplanegatiivi booleania. (Kts.MTISUMB isOutOfFlashlight)
        {
            //if (Input.GetMouseButtonDown(0))
            //{
            //    Reveal();
            //}
            //else if (Input.GetMouseButtonDown(1))
            //{
            //    Flag();
            //}
        }
    }

    private void StartRaid()
    {
        //(DEV) Kovakoodataan minefieldHeight/Width kun gridien koot varmistuvat (pienenee mitä vähemmän pelaajia klaanissa)
        state = new Hexa_struct[minefieldWidth, minefieldHeight];
        raidIsOver = false;
        Camera.main.transform.position = new Vector3(minefieldWidth / 2f, minefieldHeight / 2f, -15f); //(DEV) Kameran testi default. Siirtää sen vastaamaan ruutuja. Poistetaan kun siirrytään oikeaan Sceneen.
        GenerateHexas();
        GenerateBombs();
        GenerateNumbers();
        field.Draw(state);
    }

    private void GenerateHexas()
    {
        for (int x = 0; x < minefieldWidth; x++)
        {
            for(int y = 0; y <minefieldHeight; y++)
            {
                Hexa_struct hexa = new Hexa_struct();
                hexa.position = new Vector3Int(x, y, 0);
                hexa.type = Hexa_struct.Type.Neutral;
                state[x, y] = hexa;
            }
        }
    }

    private void GenerateBombs()
    {
        for(int i = 0; i < numberOfBombs; i++)
        {
            //(DEV) Randomoitu pommien sijoittelu. Täytyy vaihtaa kun Loot asettelu testiin!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);

            //Check if hexa already has an Bomb. Use this for other checking!!!
            while(state[x, y].type == Hexa_struct.Type.Bomb)
            {
                x++;
                if(x >= minefieldWidth)
                {
                    x = 0;
                    y++;

                    if(y >= minefieldHeight)
                    {
                        y = 0;
                    }
                }
            }
            state[x, y].type = Hexa_struct.Type.Bomb;
            state[x, y].revealed = true; // (DEV) Paljastaa kaikki pommit
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < minefieldWidth; x++)
        {
            for (int y = 0; y < minefieldHeight; y++)
            {
                Hexa_struct hexa = state[x, y];
                if(hexa.type == Hexa_struct.Type.Bomb)
                {
                    continue;
                }
                hexa.number = CalculateBombs(x, y);

                if(hexa.number > 0)
                {
                    hexa.type = Hexa_struct.Type.Number;
                }
                hexa.revealed = true; // (DEV) Paljastaa kaikki numerot
                state[x, y] = hexa;
            }
        }
    }

    private int CalculateBombs(int hexaX, int hexaY)
    {
        int count = 0; //Testaa ilman 0

        //(DEV) Korjaa välikön lasku! Laskee oudosti pommit! Tarkista, että kaikista neljästä väliköstä tulee luku! Ei laske X välikköä ollenkaan!

        for (int neighborX = -1; neighborX <= 1; neighborX++)
        {
            for(int neighborY = -1; neighborY <= 1; neighborY++)
            {
                if(neighborX == 0 && neighborY == 0)
                {
                    continue;
                }
                int x = hexaX + neighborX;
                int y = hexaY + neighborY;

                ////Checks if neighbor tiles are out of bounds (no more hexas)
                //if(x < 0 || x >= minefieldWidth || y < 0 || y >= minefieldHeight)
                //{
                //    continue;
                //}

                //Checks if a hexa is indeed a bomb hexa, and add it to count
                if (GetHexa(x, y).type == Hexa_struct.Type.Bomb)
                {
                    count++;
                }

                ////Checks if a hexa is indeed a bomb hexa, and add it to count
                //if(state[x, y].type == Hexa_struct.Type.Bomb)
                //{
                //    count++;
                //}
            }
        }
            return count;
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int hexaPosition = field.tilemap.WorldToCell(worldPosition);
        Hexa_struct hexa = GetHexa(hexaPosition.x, hexaPosition.y);

        if(hexa.type == Hexa_struct.Type.Invalid || hexa.revealed)
        {
            return;
        }
        hexa.flagged = !hexa.flagged;
        state[hexaPosition.x, hexaPosition.y] = hexa;
        field.Draw(state);
    }

    private void Reveal()
    {
        //(DEV) Täytyy vaihtaa kun Android build!
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int hexaPosition = field.tilemap.WorldToCell(worldPosition);
        Hexa_struct hexa = GetHexa(hexaPosition.x, hexaPosition.y);

        if (hexa.type == Hexa_struct.Type.Invalid || hexa.revealed || hexa.flagged)
        {
            return;
        }

        switch (hexa.type)
        {
            case Hexa_struct.Type.Bomb:
                Detonate(hexa);
                break;

            default:
                hexa.revealed = true;
                state[hexaPosition.x, hexaPosition.y] = hexa;
                break;
        }
        //hexa.revealed = true;
        //state[hexaPosition.x, hexaPosition.y] = hexa;
        field.Draw(state);
    }

    private void Detonate(Hexa_struct hexa)
    {
        raidIsOver = true;
        hexa.revealed = true;
        hexa.detonated = true;

        state[hexa.position.x, hexa.position.y] = hexa;

    }

    private Hexa_struct GetHexa(int x, int y)
    {
        if(IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Hexa_struct();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < minefieldWidth && y >= 0 && y < minefieldHeight;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class RaidManager_Script : MonoBehaviour
{
    public int minefieldWidth;
    public int minefieldHeight;
    public int numberOfBombs;
    public int numberOfTestLoot;
    public int lootCollected;
    public int maxLootCondition;
    //public int number;

    /*(DEV) Show Bombs*/
    public bool showBombs;
    /*(DEV) Show Hexas*/
    public bool showHexas;
    /*(DEV) Number of loot*/
    public bool showTestLoot;


    private Field_Script field;
    private Hexa_Struct[,] state;
    private bool raidIsOver;

    private void Awake()
    {
        field = GetComponentInChildren<Field_Script>();
    }

    private void Start()
    {
        StartRaid();
        //n‰yt‰Pommit = false; //(DEV) Poistetaan kun valmis
        //n‰yt‰Hexat = false; //(DEV) Poistetaan kun valmis
    }
    private void Update()
    {
        // (DEV) Bugi inputissa: Kokeile Project Settings > Player > Active Input > Both!!!

        if (!raidIsOver) //(DEV) Vaihda jos ei toimi, kuten esim. == false. Tai kokeile tuplanegatiivi booleania. (Kts.MTISUMB isOutOfFlashlight)
        {
            //if (Input.GetMouseButtonDown(0))
            //{
            //    Reveal();
            //}

            if (Mouse.current.leftButton.isPressed)
            {
                Reveal(Mouse.current.position.ReadValue());
            }

            else if (Mouse.current.rightButton.isPressed)
            {
                Flag(Mouse.current.position.ReadValue());
            }

            //else if (Input.GetMouseButtonDown(1))
            //{
            //    Flag();
            //}
        }
    }

    private void StartRaid()
    {
        //(DEV) Kovakoodataan minefieldHeight/Width kun gridien koot varmistuvat (pienenee mit‰ v‰hemm‰n pelaajia klaanissa)
        state = new Hexa_Struct[minefieldWidth, minefieldHeight];
        raidIsOver = false;
        Camera.main.transform.position = new Vector3(minefieldWidth / 2f, minefieldHeight / 2f, -15f); //(DEV) Kameran testi default. Siirt‰‰ sen vastaamaan ruutuja. Poistetaan kun siirryt‰‰n oikeaan Sceneen.
        GenerateHexas();
        GenerateBombs();
        GenerateTestLoot();
        GenerateNumbers();
        field.Draw(state);
    }

    private void GenerateHexas()
    {
        for (int x = 0; x < minefieldWidth; x++)
        {
            for(int y = 0; y <minefieldHeight; y++)
            {
                Hexa_Struct hexa = new Hexa_Struct();
                hexa.position = new Vector3Int(x, y, 0);
                hexa.type = Hexa_Struct.Type.Neutral;
                state[x, y] = hexa;
            }
        }
    }

    private void GenerateBombs()
    {
        for(int i = 0; i < numberOfBombs; i++)
        {
            //(DEV) Randomoitu pommien sijoittelu. Must be changed when testing of Loot Placement begins!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);

            //Check if hexa already has an Bomb. Use this for other checking!!!
            while(state[x, y].type == Hexa_Struct.Type.Bomb )
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
            state[x, y].type = Hexa_Struct.Type.Bomb;

            //(DEV) N‰ytt‰‰ pommit
            if (showBombs)
            {
                state[x, y].revealed = true;
            }
            
            /*state[x, y].revealed = true;*/ // (DEV) Paljastaa kaikki pommit
        }
    }

    private void GenerateTestLoot()
    {
        for (int i = 0; i < numberOfTestLoot; i++)
        {
            //(DEV) Randomoitu TestLoot sijoittelu. Change when Loot placement testing!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);

            //Check if hexa already has a TestLoot OR a Bomb. (DEV): Use this everywhere you need hexa checking!!!
            while (state[x, y].type == Hexa_Struct.Type.Loot || state[x, y].type == Hexa_Struct.Type.Bomb || state[x, y].type == Hexa_Struct.Type.Number)
            {
                x++;
                if (x >= minefieldWidth)
                {
                    x = 0;
                    y++;

                    if (y >= minefieldHeight)
                    {
                        y = 0;
                    }
                }
            }
            state[x, y].type = Hexa_Struct.Type.Loot;

            //(DEV) Shows testloot
            if (showTestLoot)
            {
                state[x, y].revealed = true;
            }

            /*state[x, y].revealed = true;*/ // (DEV) Paljastaa kaikki pommit
        }
    }


    private void GenerateNumbers()
    {
        for (int x = 0; x < minefieldWidth; x++)
        {
            for (int y = 0; y < minefieldHeight; y++)
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Bomb || hexa.type == Hexa_Struct.Type.Loot)
                {
                    continue;
                }
                hexa.number = CalculateBombs(x, y);

                if (hexa.number > 0)
                {
                    hexa.type = Hexa_Struct.Type.Number;
                }

                //(DEV) Paljastaa hexat
                if (showHexas)
                {
                    hexa.revealed = true;
                }
                /*hexa.revealed = true;*/ // (DEV) Paljastaa kaikki numerot
                state[x, y] = hexa;
            }
        }
    }

    private int CalculateBombs(int hexaX, int hexaY)
    {
        int count = 0; //Testaa ilman 0?

        //(DEV) Fix the calculation! No through corridor calculation.
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
                if (GetHexa(x, y).type == Hexa_Struct.Type.Bomb)
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

    private void Flag(Vector3 position)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position); //Camera.main.ScreenToWorldPoint(Input.mousePosition)
        Vector3Int hexaPosition = field.tilemap.WorldToCell(worldPosition);
        Hexa_Struct hexa = GetHexa(hexaPosition.x, hexaPosition.y);

        if(hexa.type == Hexa_Struct.Type.Invalid || hexa.revealed)
        {
            return;
        }
        hexa.flagged = !hexa.flagged;
        state[hexaPosition.x, hexaPosition.y] = hexa;
        field.Draw(state);
    }

    private void Reveal(Vector3 position)
    {
        //(DEV) Change when build for Android!
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position); //Camera.main.ScreenToWorldPoint(Input.mousePosition)

        Vector3Int hexaPosition = field.tilemap.WorldToCell(worldPosition);
        Hexa_Struct hexa = GetHexa(hexaPosition.x, hexaPosition.y);

        if (hexa.type == Hexa_Struct.Type.Invalid || hexa.revealed || hexa.flagged)
        {
            return;
        }
        

        switch (hexa.type)
        {
            case Hexa_Struct.Type.Bomb:
                Detonate(hexa);
                break;

            case Hexa_Struct.Type.Loot:
                lootCollected++;
                UnityEngine.Debug.Log(lootCollected);
                break;

            default:
                hexa.revealed = true;
                state[hexaPosition.x, hexaPosition.y] = hexa;
                break;
        }
        hexa.revealed = true;
        //UnityEngine.Debug.Log("revealed!");
        state[hexaPosition.x, hexaPosition.y] = hexa;
        field.Draw(state);

        // Win condition. Change the number when needed!
        if(lootCollected == maxLootCondition)
        {
            UnityEngine.Debug.Log("No more loot. You win!");
            raidIsOver = true;
        }
    }

    private void Detonate(Hexa_Struct hexa)
    {
        UnityEngine.Debug.Log("Raid is Over!");
        raidIsOver = true;
        hexa.revealed = true;
        hexa.detonated = true;

        state[hexa.position.x, hexa.position.y] = hexa;

    }

    private Hexa_Struct GetHexa(int x, int y)
    {
        if(IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Hexa_Struct();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < minefieldWidth && y >= 0 && y < minefieldHeight;
    }
}

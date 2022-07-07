using UnityEngine;

public class RaidManager_Script : MonoBehaviour
{
    public int minefieldWidth;
    public int minefieldHeight;
    public int numberOfBombs;
    //public int number;

    private Field_Script field;
    private Hexa_struct[,] state;

    private void Awake()
    {
        field = GetComponentInChildren<Field_Script>();
    }

    private void Start()
    {
        StartRaid();
        
    }

    private void StartRaid()
    {
        state = new Hexa_struct[minefieldWidth, minefieldHeight];
        Camera.main.transform.position = new Vector3(minefieldWidth / 2f, minefieldHeight / 2f, -15f); //Default camera when testing! Remove this when integrating into the real game scene!
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
            //Random bomb position, change when testing Loot!
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
            state[x, y].revealed = true; //Paljastaa kaikki pommit (dev)
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
                hexa.revealed = true; // Paljastaa kaikki numerot (dev)
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

                //Checks if neighbor tiles are out of bounds (no more hexas)
                if(x < 0 || x >= minefieldWidth || y < 0 || y >= minefieldHeight)
                {
                    continue;
                }

                //Checks if a hexa is indeed a bomb hexa, and add it to count
                if(state[x, y].type == Hexa_struct.Type.Bomb)
                {
                    count++;
                }

            }
        }
            return count;
    }
}

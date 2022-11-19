using System.Collections;
using Raid.Test.Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class RaidManager_Script : MonoBehaviour
{
    [SerializeField] private int minefieldWidth; //must be int
    [SerializeField] private int minefieldHeight; //must be int
    [SerializeField] private int numberOfBombs;
    [SerializeField] private int numberOfNewTestBombs;//newtest bombs
    [SerializeField] private int numberOfTestLoot;
    [SerializeField] private int lootCollected;
    [SerializeField] private int maxLootCondition;
    [SerializeField] private int floodRadius;
    [SerializeField] private int editorRandomSeed;
    [SerializeField] private float raidTime;
    [SerializeField] private float flashTime;
    [SerializeField] private RaidManager_View view;

    /*(DEV) Show Bombs*/
    [SerializeField] private bool showBombs;
    /*(DEV) Show new test bombs*/
    [SerializeField] private bool showNewTestBombs;
    /*(DEV) Show Hexas*/
    [SerializeField] private bool showHexas;
    /*(DEV) Number of loot*/
    [SerializeField] private bool showTestLoot;


    private Field_Script field;
    private Hexa_Struct[,] state;
    private bool raidIsOver;
    private Camera cameraMain;

    private int floodingTurn;
    private int topBombChance;
    private int rightBombChance;
    private int leftBombChance;
    private int downBombChance;
    private int topRightBombChance;
    private int topLeftBombChance;
    private int downRightBombChance;
    private int downLeftBombChance;
    private string prevDebugString;
    private bool isMobile;
    private bool hasView;

    private float GetBackpackPercentage() => lootCollected == 0 ? 0 : (float)lootCollected / maxLootCondition * 100f;
    private void Awake()
    {
        field = GetComponentInChildren<Field_Script>();
        cameraMain = Camera.main;
        isMobile = AppPlatform.IsMobile || AppPlatform.IsSimulator;
        if (AppPlatform.IsEditor)
        {
            // Use well known seed for testing in Editor.
            if (editorRandomSeed == 0)
            {
                editorRandomSeed = System.Environment.TickCount % 100000;
            }
            UnityEngine.Debug.Log($"seed {editorRandomSeed}");
            Random.InitState(editorRandomSeed);
        }
    }

    private void OnEnable()
    {
        // Enhanced touch support provides automatic finger tracking and touch history recording. It is an API designed for polling!
        EnhancedTouchSupport.Enable();
        StartCoroutine(WaitForView());
    }

    private IEnumerator WaitForView()
    {
        // Unfortunately here are levels that do not have view attached to them :-(
        yield return new WaitUntil(() => (view = FindObjectOfType<RaidManager_View>()) != null);
        hasView = true;
        view.ResetView();
        view.SetTimer(raidTime);
        view.SetBackpack(GetBackpackPercentage());
        view.SetSeed(editorRandomSeed);
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
    
    private void Start()
    {
        StartRaid();
    }
    private void Update()
    {
        if(flashTime >= 0)
        {
            FlashTimer();
        }
        else

        if (raidTime >= 0 && !raidIsOver)
        {          
            RaidTimer();
        }
        else
        if (raidIsOver)
        {
            return;
        }
        if (isMobile)
        {
            CheckTouch();
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Reveal(Mouse.current.position.ReadValue());

            }
        }
    }

    private void CheckTouch()
    {
        if (Touch.activeTouches.Count != 1)
        {
            return;
        }
        var touch = Touch.activeTouches[0];
        if (touch.phase == TouchPhase.Began)
        {
            Reveal(touch.screenPosition);
        }
    }
    
    private void StartRaid()
    {
        //(DEV) Change when minefieldHeight/Width size is certain. (gets smaller as clan´s size changes)
        state = new Hexa_Struct[minefieldWidth, minefieldHeight];
        raidIsOver = false;
        Transform cameraMainTransform = cameraMain.transform;
        cameraMainTransform.position = new Vector3(3.0f, 5.5f, -15f); //(DEV) Camera test default. (2f, 2f, -15f), or "minefieldWidth / 2f, minefieldHeight / 2f, -15f" Moves Camera to follow grid manually.
        GenerateHexas();
        GenerateTestLoot();//Change where comment is if not working
        //GenerateBombsNextToLootTop();
        //GenerateBombsNextToLootRight();
        //GenerateBombsNextToLootTopRight();
        //GenerateBombsNextToLootDown();
        //GenerateBombsNextToLootLeft();
        //GenerateBombsNextToLootDownLeft();
        GenerateBombs(); //Change to comment when testing GenerateBombsNextToLoot
        //GenerateNewTestBombs();
        //GenerateTestLoot(); //used to be here
        GenerateNumbers();
        GenerateLootNumbers(); //Uncomment this when testing lootNumber calculation
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

    private void GenerateNewTestBombs()
    {
        for (int i = 0; i < numberOfNewTestBombs; i++)
        {
            //(DEV) Random bomb placement. Must be changed when testing of Loot Placement begins!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);

            //Check if hexa already has an Bomb. Use this for other checking!!!
            while (state[x, y].type == Hexa_Struct.Type.NewTestBomb)
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
            state[x, y].type = Hexa_Struct.Type.NewTestBomb;

            //(DEV) Shows bombs
            if (showNewTestBombs)
            {
                state[x, y].revealed = true;
            }
        }
    }

    private void GenerateBombs()
    {
        for(int i = 0; i < numberOfBombs; i++)
        {
            //(DEV) Random bomb placement. Must be changed when testing of Loot Placement begins!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);

            //Check if hexa already has an Bomb or Loot. Use this for other checking!!!
            while (state[x, y].type == Hexa_Struct.Type.Bomb || state[x, y].type == Hexa_Struct.Type.Loot)
            {
                x++;
                //y++; //This to comment
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
            if (state[x, y].type != Hexa_Struct.Type.Loot)
            {
                state[x, y].type = Hexa_Struct.Type.Bomb;
            }
            else continue;

            if (showBombs)
            {
                state[x, y].revealed = true;
            }
        }
    }

    private void GenerateTestLoot()
    {
        for (int i = 0; i < numberOfTestLoot; i++)
        {
            //(DEV) Random loot placement. Change when Loot placement testing!
            int x = Random.Range(0, minefieldWidth);
            int y = Random.Range(0, minefieldHeight);


            //Check if hexa already has a TestLoot OR a Bomb. (DEV): Use this everywhere you need hexa checking!!!
            while (state[x, y].type == Hexa_Struct.Type.Loot || state[x, y].type == Hexa_Struct.Type.Bomb/* || state[x, y].type == Hexa_Struct.Type.Number*/)
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
            if (state[x, y].type != Hexa_Struct.Type.Bomb)
            {
                state[x, y].type = Hexa_Struct.Type.Loot;
            }
            else continue;

            //(DEV) Shows testloot
            if (showTestLoot)
            {
                state[x, y].revealed = true;
            }
        }
    }


    private void GenerateNumbers()
    {
        for (int y = 0; y < minefieldHeight; y++)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 0; x < minefieldWidth; x++)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Bomb)
                {
                    continue;
                }
                hexa.number = CalculateBombs(x, y);
                //hexa.lootNumber = CalculateBombs(x, y); //Remove if does not work! Try to create own method for lootCount.

                if (hexa.number > 0)
                {
                    hexa.type = Hexa_Struct.Type.Number;
                }

                //(DEV) Reveals hexas
                if (showHexas)
                {
                    hexa.revealed = true;
                }
                state[x, y] = hexa;
            }
        }
    }
    private void GenerateLootNumbers()
    {
        for (int y = 0; y < minefieldHeight; y++)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 0; x < minefieldWidth; x++)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    continue;
                }
                hexa.lootNumber = CalculateLoots(x, y);
                //hexa.lootNumber = CalculateBombs(x, y); //Remove if does not work! Try to create own method for lootCount.

                if (hexa.lootNumber > 0)
                {
                    hexa.type = Hexa_Struct.Type.LootNumber;
                }

                //(DEV) Reveals hexas
                if (showHexas)
                {
                    hexa.revealed = true;
                }
                state[x, y] = hexa;
            }
        }
    }

    private void GenerateBombsNextToLootTop()
    {
        //for loop numberOfBombs!
        for (int y = 0; y < minefieldHeight; y++)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 0; x < minefieldWidth; x++)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    x++;
                    if (x >= minefieldWidth) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        x--;
                        y++;

                        if (y >= minefieldHeight)
                        {
                            y--;
                        }
                        //Here it has returned to the same loot hexa
                        continue;
                    }
                    topBombChance = Random.Range(0, 101);
                    if (topBombChance <= 40)
                    {
                        state[x, y].type = Hexa_Struct.Type.Bomb;
                    }
                    else
                    {
                        continue;
                    }
                    //state[x, y].type = Hexa_Struct.Type.Bomb;
                    //continue;
                }
            }
        }
    }
    private void GenerateBombsNextToLootRight()
    {
        //for loop numberOfBombs!
        for (int y = 0; y < minefieldHeight; y++)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 0; x < minefieldWidth; x++)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    y++;
                    if (y >= minefieldWidth || y >= minefieldHeight) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        y--;
                        x++;
                        
                        if (x >= minefieldHeight || x >= minefieldWidth)
                        {
                            x--;
                        }
                        //Here it has returned to the same loot hexa
                        continue;
                    }
                    rightBombChance = Random.Range(0, 101);
                    if (rightBombChance <= 40)
                    {
                        state[x, y].type = Hexa_Struct.Type.Bomb;
                    }
                    else
                    {
                        continue;
                    }
                    //state[x, y].type = Hexa_Struct.Type.Bomb; //(DEV)Error on this line!
                    //continue; 
                }
            }
        }
    }

    private void GenerateBombsNextToLootTopRight()
    {
        //for loop numberOfBombs!
        for (int y = 0; y < minefieldHeight; y++)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 0; x < minefieldWidth; x++)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    y++;
                    x++;
                    if (y >= minefieldWidth || y >= minefieldHeight) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        y--;
                        //x--;

                        if (x >= minefieldHeight || x >= minefieldWidth)
                        {
                            x--;
                        }
                        //Here it has returned to the same loot hexa
                        continue;
                    }
                    else
                    {
                        topRightBombChance = Random.Range(0, 101);
                        if (topRightBombChance <= 40 && hexa.type != Hexa_Struct.Type.Bomb || hexa.type != Hexa_Struct.Type.Loot)
                        {
                            state[x, y].type = Hexa_Struct.Type.Bomb;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }

    private void GenerateBombsNextToLootDown()
    {
        //for loop numberOfBombs!
        for (int y = 6; y > 0; y--)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 11; x > 0; x--)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    x--;
                    if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        x++;
                        y--;

                        if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight)
                        {
                            y++;
                        }
                        //Here it has returned to the same loot hexa
                        //continue;
                    }
                    downBombChance = Random.Range(0, 101);
                    if (downBombChance <= 40)
                    {
                        x--;
                        state[x, y].type = Hexa_Struct.Type.Bomb;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }

    private void GenerateBombsNextToLootLeft()
    {
        //for loop numberOfBombs!
        for (int y = 6; y > 0; y--)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 11; x > 0; x--)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    y--;
                    if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        y++;
                        x--;

                        if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight)
                        {
                            x++;
                        }
                        //Here it has returned to the same loot hexa
                        //continue;
                    }
                    leftBombChance = Random.Range(0, 101);
                    if (leftBombChance <= 40)
                    {
                        y--;
                        if (x <= 11 && x >= 0 && y <= 6 && y >= 0) state[x, y].type = Hexa_Struct.Type.Bomb;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }

    private void GenerateBombsNextToLootDownLeft()
    {
        for (int y = 6; y > 0; y--)//int x = 0; x < minefieldWidth; x++
        {
            for (int x = 11; x > 0; x--)//int y = 0; y < minefieldHeight; y++
            {
                Hexa_Struct hexa = state[x, y];
                if (hexa.type == Hexa_Struct.Type.Loot)
                {
                    x--;
                    y--;
                    if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight) //This is for checking if hexa is out of bounds and would cause an error.
                    {
                        x++;
                        //y--;

                        if (x >= minefieldWidth || y >= minefieldHeight || y <= minefieldHeight || x <= minefieldHeight)
                        {
                            y++;
                        }
                        //Here it has returned to the same loot hexa
                        //continue;
                        //break;
                    }
                    downLeftBombChance = Random.Range(0, 101);
                    if (downLeftBombChance <= 40)
                    {
                        x--;
                        y--;
                        if(hexa.type != Hexa_Struct.Type.Bomb || hexa.type != Hexa_Struct.Type.Loot) state[x, y].type = Hexa_Struct.Type.Bomb;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }


    private int CalculateBombs(int hexaX, int hexaY)
    {
        int bombCount = 0;

        ////(DEV) Fix the calculation! No through corridor calculation.
        for (int neighborX = -1; neighborX <= 1; neighborX++)//int neighborX = -1; neighborX <= 1; neighborX++
        {
            for (int neighborY = -1; neighborY <= 1; neighborY++) //int neighborY = -1; neighborY <= 1; neighborY++
            {
                if (neighborX == 0 && neighborY == 0)
                {
                    continue;
                }

                int x = hexaX + neighborX;
                int y = hexaY + neighborY;

                //Checks if a hexa is indeed a bomb hexa, and add it to count. This is the right one!
                if (GetHexa(x, y).type == Hexa_Struct.Type.Bomb)
                {
                    bombCount++;
                }
            }
        }
        return bombCount; //(DEV) Change all to count if does not work
    }

    private int CalculateLoots(int hexaX, int hexaY)
    {
        int lootCount = 0;

        ////(DEV) Fix the calculation! No through corridor calculation.
        for (int neighborX = -1; neighborX <= 1; neighborX++)//int neighborX = -1; neighborX <= 1; neighborX++
        {
            for (int neighborY = -1; neighborY <= 1; neighborY++) //int neighborY = -1; neighborY <= 1; neighborY++
            {
                if (neighborX == 0 && neighborY == 0)
                {
                    continue;
                }

                int x = hexaX + neighborX;
                int y = hexaY + neighborY;

                //Checks if a hexa is indeed a bomb hexa, and add it to count. This is the right one!
                if (GetHexa(x, y).type == Hexa_Struct.Type.Loot)
                {
                    lootCount++;
                }
            }
        }
        return lootCount; //(DEV) Change all to count if does not work
    }

    private void Flag(Vector3 position)
    {
        Vector3 worldPosition = cameraMain.ScreenToWorldPoint(position); //Camera.main.ScreenToWorldPoint(Input.mousePosition)
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

    private void Reveal(Vector2 position)
    {
        Vector3 worldPosition = cameraMain.ScreenToWorldPoint(position);
        // Fix origo by adding half of the cell size (1f).
        worldPosition.x += .5f;
        worldPosition.y += .5f;

        Vector3Int hexaPosition = field.tilemap.WorldToCell(worldPosition);
        Hexa_Struct hexa = GetHexa(hexaPosition.x, hexaPosition.y);
        var debugString = $"Reveal world x={position.x} y={position.y} hexa x={hexaPosition.x} y={hexaPosition.y}";
        //if (debugString != prevDebugString)
        //{
        //    prevDebugString = debugString;
        //    UnityEngine.Debug.Log(prevDebugString);
        //}
        if (hexa.type == Hexa_Struct.Type.Invalid || hexa.revealed || hexa.flagged)
        {
            return;
        }

        if (hexa.type == Hexa_Struct.Type.Neutral) //Flooding
        {
            Flood(hexa);
            floodingTurn = 0;
        }

        switch (hexa.type)
        {
            case Hexa_Struct.Type.Bomb:
                Detonate(hexa);
                break;

            case Hexa_Struct.Type.Loot:
                lootCollected++;
                if (hasView) view.SetBackpack(GetBackpackPercentage());
                UnityEngine.Debug.Log(lootCollected);
                break;

            default:
                hexa.revealed = true;
                state[hexaPosition.x, hexaPosition.y] = hexa;
                break;
        }
        hexa.revealed = true;
        state[hexaPosition.x, hexaPosition.y] = hexa;
        field.Draw(state);

        // Win condition. Change the number when needed!
        if (lootCollected >= maxLootCondition)
        {
            UnityEngine.Debug.Log("Too much loot! Your backbag has been overloaded! You lose!");
            raidIsOver = true;
            if (hasView) view.SetGameOVer();
        }
    }

    private void Flood(Hexa_Struct hexa)
    {
        floodingTurn++;
        if (hexa.revealed) return;
        if (hexa.type == Hexa_Struct.Type.Bomb || hexa.type == Hexa_Struct.Type.Invalid) return;
        if (hexa.type == Hexa_Struct.Type.Loot) lootCollected++; UnityEngine.Debug.Log(lootCollected);

        hexa.revealed = true;
        state[hexa.position.x, hexa.position.y] = hexa;

        if (hexa.type == Hexa_Struct.Type.Neutral && floodingTurn <= floodRadius)
        {
            //UnityEngine.Debug.Log(floodingTurn);
            Flood(GetHexa(hexa.position.x - 1, hexa.position.y));
            Flood(GetHexa(hexa.position.x + 1, hexa.position.y));
            Flood(GetHexa(hexa.position.x, hexa.position.y - 1));
            Flood(GetHexa(hexa.position.x, hexa.position.y + 1));
        }
        else return;
    }

    private void Detonate(Hexa_Struct hexa)
    {
        UnityEngine.Debug.Log("Raid is Over!");
        raidIsOver = true;
        hexa.revealed = true;
        hexa.detonated = true;

        state[hexa.position.x, hexa.position.y] = hexa;

        RevealAll(hexa);
        raidTime = 0;

        //for (int x = 0; x < minefieldWidth; x++)
        //{
        //    for (int y = 0; y < minefieldHeight; y++)
        //    {
        //        hexa = state[x, y];
        //        if(hexa.type == Hexa_Struct.Type.Bomb || hexa.type == Hexa_Struct.Type.Loot || hexa.type == Hexa_Struct.Type.NumberAndLootNumber || hexa.type == Hexa_Struct.Type.Neutral || hexa.type == Hexa_Struct.Type.Number || hexa.type == Hexa_Struct.Type.LootNumber)
        //        {
        //            hexa.revealed = true;
        //            state[x, y] = hexa;
        //        }
        //    }
        //}
    }

    private void RevealAll(Hexa_Struct hexa)
    {
        state[hexa.position.x, hexa.position.y] = hexa;

        for (int x = 0; x < minefieldWidth; x++)
        {
            for (int y = 0; y < minefieldHeight; y++)
            {
                hexa = state[x, y];
                if (hexa.type != Hexa_Struct.Type.Invalid)
                {
                    hexa.revealed = true;
                    state[x, y] = hexa;
                }
            }
        }
    }
    public void TimeReveal()
    {
        for (int y = 0; y < minefieldHeight; y++)
        {
            for (int x = 0; x < minefieldWidth; x++)
            {
                Hexa_Struct hexa = state[x, y];
                hexa.revealed = true;
                state[x, y] = hexa;
                field.Draw(state);
            }
        }
    }

    private void RaidTimer()
    {
        raidTime -= Time.deltaTime;
        if (hasView) view.SetTimer(raidTime);
        if (raidTime <= 0)
        {
            RaidTimeEnds();
        }
    }
    private void FlashTimer()
    {
        if(!raidIsOver)
        {
            flashTime -= Time.deltaTime;
            if (flashTime <= 0)
            {
                HideAll();
            }
            else return;
        }
    }

    private void HideAll()
    {
        for (int y = 0; y < minefieldHeight; y++)
        {
            for (int x = 0; x < minefieldWidth; x++)
            {
                Hexa_Struct hexa = state[x, y];
                hexa.revealed = false;
                state[x, y] = hexa;
                field.Draw(state);
            }
        }
    }

    private void RaidTimeEnds()
    {
        raidIsOver = true;
        UnityEngine.Debug.Log("Raiding time is up! Raid has ended!");

        showHexas = true;
        showBombs = true;      
        showTestLoot = true;
        TimeReveal();
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

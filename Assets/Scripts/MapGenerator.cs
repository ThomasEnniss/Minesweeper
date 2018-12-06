using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour {

    public int height;
    public int width;
    public int numberOfMines;
    public int seed = 0;

    public Text tilesToUncover;
    public Text gameStatus;
    public Text timer;

    public GameObject[] tiles;
    public GameObject marker;

    private int[,] mines;
    private List<Vector2> open;
    private List<Vector2> chosenMines;
    private bool busy;
    private bool[,] visited;

    private int goodMovesLeft = 0;
    private bool gameOver = false;
    private bool gameWon = false;
    private int timeTaken = 0;

    bool markersDisplayed = false;

    private void Start()
    {
        /*TE: Set up the variables.*/
        mines = new int[width, height];
        visited = new bool[width, height];
        Random.InitState(seed);
        open = new List<Vector2>();
        chosenMines = new List<Vector2>();

        goodMovesLeft = height * width - numberOfMines;

        /*TE: Begin by generating the map.*/
        GenerateMap();
        DisplayMap();
    }

    private void Update()
    {
        UpdateUI();

        if (!gameOver && !gameWon)
        {
            timeTaken = (int)Time.time;


        }
        else {
            if (!markersDisplayed)
            {
                markersDisplayed = true;
                DisplayMarkers();
            }
        }

        if (goodMovesLeft==0) {            
            gameWon = true;
        }         
    }

    void UpdateUI()
    {
        tilesToUncover.text = "Safe Tiles Left: " + goodMovesLeft;
        gameStatus.text = !gameOver ? (gameWon ? "^_^" : "o_o") : "X_X";
        timer.text = timeTaken.ToString();
    }

    void GenerateMap()
    {
        SetupMap();
        PlaceMines();
        CalculateAdjacentSquares();
    }

    /*TE: Initiate the map values. Populate the open list with vectors for choosing mine locations.*/
    void SetupMap() { 
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mines[x, y] = 0;
                visited[x, y] = false;
                open.Add(new Vector2(x, y));
            }
        }
    }
    
    /*TE: Places mines on the map.*/
    void PlaceMines() {

        for (int mine = 0; mine < numberOfMines; mine++) {

            int randomPick = Random.Range(0, open.Count);

            Vector2 nextMine = open[randomPick];

            Debug.Log("Placing mine at X:" + nextMine.x + " Y:" + nextMine.y);

            mines[(int)nextMine.x, (int)nextMine.y] = 9;
            visited[(int)nextMine.x, (int)nextMine.y] = true;

            open.RemoveAt(randomPick);

            chosenMines.Add(nextMine);
        }
    }

    /*TE: After mines are placed, we iterate over the remaining squares and calculate how mines they are adjaccent to.*/
    void CalculateAdjacentSquares() {

        for (int i = 0;i<open.Count;i++)
        {
            Vector2 current = open[i];
            mines[(int)current.x, (int)current.y] = GetAdjacentMines(current);            
        }
    }

    /*TE: Gets the number of mines next to a square*/
    int GetAdjacentMines(Vector2 current) {

        int numberOfAdjacentMines = 0;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {  
                /*TE: Not edge.*/
                if (current.x+i >= 0 && current.x + i < width && current.y + j >= 0 && current.y + j < height) {
                    if (mines[(int)current.x + i, (int)current.y + j] == 9) {
                        numberOfAdjacentMines++;
                    } 
                }                
            }
        }
        return numberOfAdjacentMines;
    }

    /*TE: Plots some cubes on the map to show where a mine is.*/
    void DisplayMap() {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newTile = Instantiate(tiles[10], new Vector2(x, y), Quaternion.identity);
                newTile.GetComponent<CoveredTile>().OnClick += TileClicked;
            }
        }
    }

    /*TE: Handle wwhen a tile is clicked on.*/
    void TileClicked(Vector2 start) {

        if (!gameOver)
        {
            if (!busy)
            {
                visited[(int)start.x, (int)start.y] = true;
                busy = true;
                Debug.Log("A tile was clicked. Checking what needs to be done...");

                /*TE: Check what type of tile was clicked*/
                if (mines[(int)start.x, (int)start.y] == 0)
                {
                    /*TE: Flood fill.*/
                    Debug.Log("Flooding tiles...");
                    FloodFill(start);
                    //StartCoroutine(FloodFill(start));                    
                }
                else if (mines[(int)start.x, (int)start.y] == 9)
                {
                    /*TE: Hit a mine. GG*/
                    Debug.Log("Game Over!");
                    gameOver = true;
                }
                else
                {
                    Debug.Log("Revealing tile");
                    Reveal(start);
                }
                busy = false;
            }
            else
            {
                Debug.Log("A tile was clicked. Too busy atm though...");
            }
        }
    }

    /*TE: Reveals tile or flood fills map*/
    void FloodFill(Vector2 start) {

        int tilesRevealed = 0;

        /*TE: Beginning to flood fill.*/
        Debug.Log("-Beginning to flood fill.");
        Debug.Log("-Start is: " + start.ToString());

        Queue<Vector2> tilesToCheck = new Queue<Vector2>();
        tilesToCheck.Enqueue(start);        

        while (tilesToCheck.Count > 0) {

            Vector2 current = tilesToCheck.Dequeue();

            Debug.Log("--Current is: " + current.ToString());

            Reveal(current);

            tilesRevealed++;            

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {                    
                    if (current.x + i >= 0 && current.x + i < width && current.y + j >= 0 && current.y + j < height)
                    {
                        if (!visited[(int)current.x + i, (int)current.y + j])
                        {
                            Vector2 temp = new Vector2(current.x + i, current.y + j);

                            if (mines[(int)temp.x, (int)temp.y] == 0)
                            {                               
                                Debug.Log("---Adding at: " + temp.ToString());
                                tilesToCheck.Enqueue(temp);
                            }

                            if (mines[(int)temp.x, (int)temp.y] > 0 && mines[(int)temp.x, (int)temp.y] < 9)
                            {
                                Debug.Log("---Revealing at: " + temp.ToString());
                                Reveal(temp);
                                tilesRevealed++;
                            }

                            visited[(int)temp.x, (int)temp.y] = true;

                            //GameObject newMarker = Instantiate(marker, new Vector3(temp.x, temp.y, -2), Quaternion.identity);

                           // markers.Add(newMarker);
                        }
                    }                    
                }
            }
        }
        Debug.Log("Flood fille complete! " + tilesRevealed + " tiles revealed!");
        //CleanUpMarkers();
    }

    void DisplayMarkers() {

        for (int i = 0; i < chosenMines.Count; i++) {
            Instantiate(marker, new Vector3(chosenMines[i].x, chosenMines[i].y, -2), Quaternion.identity);
        }
    }

    /*TE: Exposes the real value of a tile.*/
    void Reveal(Vector2 tileToPlace)
    {
        Debug.Log("Revealing a " + mines[(int)tileToPlace.x, (int)tileToPlace.y] + " tile!");

        Instantiate(tiles[mines[(int)tileToPlace.x, (int)tileToPlace.y]], new Vector3(tileToPlace.x, tileToPlace.y,-1), Quaternion.identity);
        goodMovesLeft--;
    }
}
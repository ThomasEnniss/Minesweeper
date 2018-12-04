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

    private int[,] mines;
    private List<Vector2> open;
    private List<Vector2> closed;
    private bool busy;
    private bool[,] visited;

    private int minesLeft = 0;
    private bool alive = true;

    private void Start()
    {
        /*TE: Set up the variables.*/
        mines = new int[width, height];
        visited = new bool[width, height];
        Random.InitState(seed);
        open = new List<Vector2>();
        closed = new List<Vector2>();

        minesLeft = height * width - numberOfMines;

        /*TE: Begin by generating the map.*/
        GenerateMap();
        DisplayMap();
    }

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI() {

        tilesToUncover.text = "Safe Tiles Left: " + minesLeft;
        gameStatus.text = alive ? "O_O" : "X_X";
        timer.text = ((int)Time.time).ToString();
    }

    void GenerateMap() {
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

            closed.Add(nextMine);
        }
    }

    /*TE: After mines are placed, we iterate over the remaining squares and calculate how mines they are adjaccent to.*/
    void CalculateAdjacentSquares() {

        for (int i = 0;i<open.Count;i++) {
            Vector2 current = open[i];
            mines[(int)current.x, (int)current.y] = GetAdjacentMines(current);            
        }
    }

    /*TE: Gets the number of mines next to a square*/
    int GetAdjacentMines(Vector2 current) {

        int numberOfAdjacentMines = 0;

        for (int i = -1; i < 2; i++) {
            for (int j = -1; j < 2; j++) {               
                
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

        if (!busy)
        {
            busy = true;
            Debug.Log("A tile was clicked. Checking what needs to be done...");

            /*TE: Check what type of tile was clicked*/
            if (mines[(int)start.x, (int)start.y] == 0)
            {
                /*TE: Flood fill.*/
                Debug.Log("Flooding tiles...");
                FloodFill(start);
            }
            else if (mines[(int)start.x, (int)start.y] == 9)
            {
                /*TE: Hit a mine. GG*/
                Debug.Log("Game Over!");
                alive = false;
            }
            else
            {
                Debug.Log("Revealing tile");
                Reveal(start);
            }
            busy = false;
        }
        else {
            Debug.Log("A tile was clicked. Too busy atm though...");
        }
    }

    /*TE: Reveals tile or flood fills map*/
    void FloodFill(Vector2 start) {

        /*TE: Beginning to flood fill.*/
        Debug.Log("-Beginning to flood fill.");
        Debug.Log("-Start is: " + start.ToString());

        Queue<Vector2> tilesToCheck = new Queue<Vector2>();
        tilesToCheck.Enqueue(start);

        visited[(int)start.x, (int)start.y] = true;

        while (tilesToCheck.Count > 0) {

            Vector2 current = tilesToCheck.Dequeue();            
            Reveal(current);
            Debug.Log("--Current is: " + current.ToString());

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {                    
                    if (current.x + i >= 0 && current.x + i < width && current.y + j >= 0 && current.y + j < height)
                    {
                        if (!visited[(int)current.x + i, (int)current.y + j]) {

                            Vector2 temp = new Vector2(current.x + i, current.y + j);

                            if (mines[(int)current.x + i, (int)current.y + j] == 0)
                            {
                               
                                Debug.Log("---Adding at: " + temp.ToString());

                                tilesToCheck.Enqueue(temp);
                            }

                            if (mines[(int)current.x + i, (int)current.y + j] > 0 && mines[(int)current.x + i, (int)current.y + j] < 9)
                            {
                                Debug.Log("---Revealing at: " + current.ToString());

                                Reveal(temp);
                            }
                            visited[(int)temp.x, (int)temp.y] = true;
                        }
                    }
                }
            }
        }       
    }

    /*TE: Exposes the real value of a tile.*/
    void Reveal(Vector2 tileToPlace) {

        Debug.Log("Revealing a " + mines[(int)tileToPlace.x, (int)tileToPlace.y] + " tile!");

        Instantiate(tiles[mines[(int)tileToPlace.x, (int)tileToPlace.y]], new Vector3(tileToPlace.x, tileToPlace.y,-1), Quaternion.identity);
        minesLeft--;
    }
}
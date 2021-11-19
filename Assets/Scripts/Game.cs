using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{

    private int width = 31;
    private int height = 31;
    [SerializeField] private float startMoveDelay = 0.35f;
    private float currentMoveDelay;
    [SerializeField] private float moveDelayMultiplier = 0.998f;
    [SerializeField] private float minMoveDelay = 0.1f;
    [SerializeField] private float appleDelay = 2f;
    private Vector3 startPos = Vector3.zero;
    [SerializeField] private int snakeStartSize = 5;
    private int score;
    private int highScore;
    private bool gameOver;
    private bool justAte;
    [SerializeField] private bool wrapOnEdges = true;

    public Sprite snakeSprite;
    public Sprite appleSprite;
    public Sprite wallSprite;
    public GameObject boardPrefab;
    public GameObject tilePrefab;
    private GameObject board;

    public TextMeshProUGUI scoreBox;
    public TextMeshProUGUI highScoreBox;
    public List<TextMeshProUGUI> gameOverUIText;
    public Button playAgainButton;

    private Vector3 north = new Vector3(0, 1, 0);
    private Vector3 south = new Vector3(0, -1, 0);
    private Vector3 west = new Vector3(-1, 0, 0);
    private Vector3 east = new Vector3(1, 0, 0);
    private Vector3 currentDirection;

    private LinkedList<Vector3> snake;
    private Dictionary<Vector3, GameObject> tiles;
    private List<Vector3> apples;
    private List<Vector3> walls;
    private Level level;
    
    private void Awake()
    {
        board = Instantiate(boardPrefab);
        board.transform.localScale = new Vector3(width, height, 0);
        board.transform.position = new Vector3(0, 0, 0);
        
        snake = new LinkedList<Vector3>();
        tiles = new Dictionary<Vector3, GameObject>();
        apples = new List<Vector3>();
        walls = new List<Vector3>(); 
        
        CreateTiles();
        LoadHighScoreFromPlayerPrefs();
        highScoreBox.text = highScore.ToString();
    }
    
    void Start()
    {
        currentDirection = north;
        currentMoveDelay = startMoveDelay;
        score = 0;
        gameOver = false;
        justAte = false;

        LoadLevelFromResources();
        if (walls != null) { CreateWalls(); }
        CreateSnake();
        InactivateGameOverUIObjects();
        
        StartCoroutine(Mover());
        StartCoroutine(AppleSpawner());
    }


    void Update()
    {
        GetDirectionInput();
    }
    
    void InactivateGameOverUIObjects()
    {
        playAgainButton.gameObject.SetActive(false);
        foreach (var item in gameOverUIText)
        {
            item.gameObject.SetActive(false);
        }
    }

    
    void CreateSnake()
    {
        snake.AddNewHead(startPos);
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;
        for (int i = 1; i < snakeStartSize; i++)
        {
            snake.AddToEnd(startPos + south * i);
            tiles[snake.Tail()].GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void CreateTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var tile = Instantiate(tilePrefab);
                var position = new Vector3(i - width / 2, j - height / 2, 0);
                tile.transform.position = position;
                tile.GetComponent<SpriteRenderer>().enabled = false;
                tiles.Add(position, tile);
            }
        }
    }

    void CreateWalls()
    {
        var currentNrInLevelMap = 0;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (level.map[currentNrInLevelMap] == 1)
                {
                    var position = new Vector3(i - width / 2, j - height / 2, 0);
                    tiles[position].transform.localScale = new Vector3(1, 1, 0);
                    var spriteRenderer = tiles[position].GetComponent<SpriteRenderer>();
                    spriteRenderer.enabled = true;
                    spriteRenderer.sprite = wallSprite;
                    walls.Add(position);
                }
                currentNrInLevelMap++;
            }
        }
    }
    
    void GameOver()
    {
        gameOver = true;
        
        StopAllCoroutines();
        
        playAgainButton.gameObject.SetActive(true);
        foreach (var item in gameOverUIText)
        {
            item.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        snake.Clear();
        apples.Clear();
        walls.Clear();
        
        foreach (var tile in tiles)
        {
            tile.Value.transform.localScale = new Vector3(0.9f, 0.9f, 0f);
            tile.Value.GetComponent<SpriteRenderer>().enabled = false;
        }
        Start();
    }

    void LoadLevelFromResources()
    {
        var path = "level1";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null) { return; }
        level = (Level)JsonUtility.FromJson(textAsset.text, typeof(Level));
    }
    


    void LoadHighScoreFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetInt("HighScore");
        }
    }
    
    void SaveHighScoreToPlayerPrefs()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    void GetDirectionInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentDirection = north;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentDirection = south;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentDirection = west;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentDirection = east;
        }
    }

    void Move()
    {
        var nextPosition = snake.Head() + currentDirection;
        CheckCrashWithSelf(nextPosition);
        if (wrapOnEdges)
        {
            nextPosition = GetScreenWrapPosition(nextPosition);
        }
        else
        {
            CheckIfOutOfBounds(nextPosition);
        }
        CheckCrashWithWall(nextPosition);
        CheckForApple(nextPosition);
        if (gameOver) {return;}
        
        snake.AddNewHead(nextPosition);
        
        tiles[snake.Head()].GetComponent<SpriteRenderer>().sprite = snakeSprite;
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;

        if (!justAte)
        {
            if (!snake.Head().Equals(snake.Tail()))
            {
                tiles[snake.Tail()].GetComponent<SpriteRenderer>().enabled = false;
            }
            snake.RemoveTail();
        }
        else
        {
            justAte = false;
        }
    }

    void CheckIfOutOfBounds(Vector3 position)
    {
        if (position.x > width / 2 || 
            position.x < -width / 2 || 
            position.y > height / 2 ||
            position.y < -height / 2)
        {
            GameOver();
        }
    }

    Vector3 GetScreenWrapPosition(Vector3 position)
    {
        if (position.x > width/2)
        {
            return new Vector3(-width/2, position.y, 0);
        }

        if (position.x < -width/2)
        {
            return new Vector3(width/2, position.y, 0);
        }
        
        if (position.y > height/2)
        {
            return new Vector3(position.x, -height / 2, 0);
        }

        if (position.y < -height/2)
        {
            return new Vector3(position.x, height / 2, 0);
        }
        
        return position;
    }

    void CheckCrashWithWall(Vector3 position)
    {
        if (!gameOver)
        {
            foreach (var wallPosition in walls)
            {
                if (wallPosition.Equals(position))
                {
                    GameOver();
                }
            }
        }
    }

    void CheckCrashWithSelf(Vector3 position)
    {
        if (!gameOver)
        {
            for (int i = 0; i < snake.Count; i++)
            {
                if (position.Equals(snake.GetByIndex(i)))
                {
                    GameOver();
                }
            }
        }
    }

    void CheckForApple(Vector3 position)
    {
        for (int i = 0; i < apples.Count; i++)
        {
            if (position.Equals(apples[i]))
            {
                IncreaseScore(1);
                apples.Remove(position);
                justAte = true;
            }
        }
    }

    void IncreaseScore(int points)
    {
        score += points;
        scoreBox.text = score.ToString();
        if (score > highScore)
        {
            highScore = score;
            SaveHighScoreToPlayerPrefs();
            highScoreBox.text = highScore.ToString();
        }
    }

    private void SpawnApple()
    {
        Vector3 position = new Vector3(0,0,0);
        var crash = true;
        while (crash)
        {
            crash = false;
            position = new Vector3(Random.Range(-width/2, width/2 -1), Random.Range(-height/2, height/2 -1), 0);
            for (int i = 0; i < snake.Count; i++)
            {
                if (position == snake.GetByIndex(i))
                {
                    crash = true;
                    break;
                }
            }

            foreach (var applePos in apples)
            {
                if (position == applePos)
                {
                    crash = true;
                    break;
                }
            }
            foreach (var wallPosition in walls)
            {
                if (position == wallPosition)
                {
                    crash = true;
                    break;
                }
            }
        }
        apples.Add(position);
        tiles[position].GetComponent<SpriteRenderer>().sprite = appleSprite;
        tiles[position].GetComponent<SpriteRenderer>().enabled = true;
    }

    private IEnumerator AppleSpawner()
    {
        while (!gameOver)
        {
            SpawnApple();
            yield return new WaitForSeconds(appleDelay);
        }
    }

    private IEnumerator Mover()
    {
        while (!gameOver)
        {
            Move();
            if (currentMoveDelay > minMoveDelay)
            {
                currentMoveDelay *= moveDelayMultiplier;
            }
            yield return new WaitForSeconds(currentMoveDelay);
        }
        yield break;
    }
}


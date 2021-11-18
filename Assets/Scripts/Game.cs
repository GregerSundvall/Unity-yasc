using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Color = System.Drawing.Color;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{

    private int width = 31;
    private int height = 31;
    private float moveDelay = 0.4f;
    private float moveDelayMultiplier = 0.999f;
    private float minMoveDelay = 0.1f;
    private float appleDelay = 2f;
    private Vector3 startPos = Vector3.zero;
    private int startSize = 5;
    private int score = 0;
    private int highScore = 10;
    private bool gameOver = false;
    private bool justAte = false;

    public Sprite squareSprite;
    public Sprite circleSprite;
    public GameObject boardPrefab;
    public GameObject tilePrefab;
    private GameObject board;

    public TextMeshProUGUI scoreBox;
    public TextMeshProUGUI highScoreBox;
    public List<TextMeshProUGUI> gameOverUIText;

    private Vector3 north = new Vector3(0, 1, 0);
    private Vector3 south = new Vector3(0, -1, 0);
    private Vector3 west = new Vector3(-1, 0, 0);
    private Vector3 east = new Vector3(1, 0, 0);
    private Vector3 currentDirection;

    private LinkedList<Vector3> snake = new LinkedList<Vector3>();
    private Dictionary<Vector3, GameObject> tiles = new Dictionary<Vector3, GameObject>();
    private List<Vector3> apples = new List<Vector3>();


    private void Awake()
    {
        board = Instantiate(boardPrefab);
        board.transform.localScale = new Vector3(width, height, 0);
        board.transform.position = new Vector3(-0.5f, -0.5f, 0);

        currentDirection = north;
        
        GetHighScore();
    }

    void Start()
    {
        highScoreBox.text = highScore.ToString();
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var tile = Instantiate(tilePrefab);
                var position = new Vector3(i - width / 2, j - height / 2, 0);
                tile.transform.position = position;
                tiles.Add(position, tile);
            }
        }
        
        snake.AddNewHead(startPos);
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;
        for (int i = 1; i < startSize; i++)
        {
            snake.AddToEnd(startPos + south * i);
            tiles[snake.Tail()].GetComponent<SpriteRenderer>().enabled = true;

        }

        StartCoroutine(Mover());
        StartCoroutine(AppleSpawner());
    }

    
    void Update()
    {
        GetDirection();
        // if (gameOver)
        // {
        //     foreach (var item in gameOverUIText)
        //     {
        //         item.enabled = true;
        //     }
        //     StopAllCoroutines();
        //     
        //     
        // }
    }

    void GameOver()
    {
        StopAllCoroutines();
        gameOver = true;
        foreach (var item in gameOverUIText)
        {
            item.enabled = true;
        }
    }

    // private void OnDestroy()
    // {
    //     SaveHighScore();
    // }

    void GetHighScore()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetInt("HighScore");
        }
    }
    
    void SaveHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    void GetDirection()
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
        CheckForCrash(snake.Head() + currentDirection);
        CheckBounds(snake.Head() + currentDirection);
        CheckForApple(snake.Head() + currentDirection);
        if (gameOver) {return;}
        
        snake.AddNewHead(snake.Head() + currentDirection);
        
        tiles[snake.Head()].GetComponent<SpriteRenderer>().sprite = squareSprite;
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

    void CheckBounds(Vector3 position)
    {
        if (!gameOver)
        {
            gameOver = position.x > width / 2 -1 || 
                       position.x < -width / 2 || 
                       position.y > height / 2 -1 ||
                       position.y < -height / 2;
        }
    }

    void CheckForCrash(Vector3 position)
    {
        if (!gameOver)
        {
            for (int i = 0; i < snake.Count; i++)
            {
                if (position.Equals(snake.GetByIndex(i)))
                {
                    gameOver = true;
                    GameOver();
                    Debug.Log(gameOver);
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
            SaveHighScore();
            highScoreBox.text = highScore.ToString();
        }
    }

    private void SpawnApple()
    {
        Vector3 pos = new Vector3(0,0,0);
        var crash = true;
        while (crash)
        {
            crash = false;
            pos = new Vector3(Random.Range(-width/2, width/2 -1), Random.Range(-height/2, height/2 -1), 0);
            for (int i = 0; i < snake.Count; i++)
            {
                if (pos == snake.GetByIndex(i))
                {
                    crash = true;
                    break;
                }
            }

            foreach (var t in apples)
            {
                if (pos == t)
                {
                    crash = true;
                    break;
                }
            }
        }
        apples.Add(pos);
        tiles[pos].GetComponent<SpriteRenderer>().sprite = circleSprite;
        tiles[pos].GetComponent<SpriteRenderer>().enabled = true;
    }

    private IEnumerator AppleSpawner()
    {
        while (true)
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
            if (moveDelay > minMoveDelay)
            {
                moveDelay *= moveDelayMultiplier;
            }
            yield return new WaitForSeconds(moveDelay);
        }
        yield break;
    }
}


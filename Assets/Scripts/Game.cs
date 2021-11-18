using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Color = System.Drawing.Color;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{

    private int width = 31;
    private int height = 31;
    private float startMoveDelay = 0.4f;
    private float currentMoveDelay;
    private float moveDelayMultiplier = 0.999f;
    private float minMoveDelay = 0.1f;
    private float appleDelay = 2f;
    private Vector3 startPos = Vector3.zero;
    private int startSize = 5;
    private int score;
    private int highScore;
    private bool gameOver;
    private bool justAte;

    public Sprite squareSprite;
    public Sprite circleSprite;
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


    private void Awake()
    {
        board = Instantiate(boardPrefab);
        board.transform.localScale = new Vector3(width, height, 0);
        board.transform.position = new Vector3(-0.5f, -0.5f, 0);
        
        snake = new LinkedList<Vector3>();
        tiles = new Dictionary<Vector3, GameObject>();
        apples = new List<Vector3>();
        
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

        GetHighScore();
        highScoreBox.text = highScore.ToString();
    }

    void Start()
    {
        currentDirection = north;
        currentMoveDelay = startMoveDelay;
        score = 0;
        gameOver = false;
        justAte = false;

        foreach (var tile in tiles)
        {
            tile.Value.GetComponent<SpriteRenderer>().enabled = false;
        }

        snake.AddNewHead(startPos);
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;
        for (int i = 1; i < startSize; i++)
        {
            snake.AddToEnd(startPos + south * i);
            tiles[snake.Tail()].GetComponent<SpriteRenderer>().enabled = true;

        }
        
        playAgainButton.gameObject.SetActive(false);
        foreach (var item in gameOverUIText)
        {
            item.gameObject.SetActive(false);
        }

        StartCoroutine(Mover());
        StartCoroutine(AppleSpawner());
    }

    
    void Update()
    {
        GetDirection();
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
        Start();
    }
    

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
        if (position.x > width / 2 -1 || 
            position.x < -width / 2 || 
            position.y > height / 2 -1 ||
            position.y < -height / 2)
        {
            GameOver();
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


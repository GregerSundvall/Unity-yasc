using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Color = System.Drawing.Color;

public class Game : MonoBehaviour
{

    public int width = 25;

    public int height = 25;

    public float moveDelay = .3f;
    private Vector3 startPos = Vector3.zero;
    
    public GameObject boardPrefab;
    public GameObject tilePrefab;
    private GameObject board;

    private Vector3 north = new Vector3(0, 1, 0);
    private Vector3 south = new Vector3(0, -1, 0);
    private Vector3 west = new Vector3(-1, 0, 0);
    private Vector3 east = new Vector3(1, 0, 0);
    private Vector3 currentDirection;

    private LinkedList<Vector3> snake = new LinkedList<Vector3>();
    private Dictionary<Vector3, GameObject> tiles = new Dictionary<Vector3, GameObject>();


    private void Awake()
    {
        board = Instantiate(boardPrefab);
        board.transform.localScale = new Vector3(width, height, 0);
        board.transform.position = new Vector3(-0.5f, -0.5f, 0);

        currentDirection = north;
    }

    void Start()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var part = Instantiate(tilePrefab);
                var position = new Vector3(i - width / 2, j - height / 2, 0);
                part.transform.position = position;
                tiles.Add(position, part);
            }
        }
        snake.AddToStart(startPos);
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;
        StartCoroutine(Mover(moveDelay));
    }

    // Update is called once per frame
    void Update()
    {
        // for (int i = 0; i < snake.Count; i++)
        // {
        //     placeholders[snake.GetByIndex(i)].GetComponent<SpriteRenderer>().enabled = true;
        // }
    }

    void Move()
    {
        snake.AddToStart(snake.Head() + currentDirection);
        tiles[snake.Head()].GetComponent<SpriteRenderer>().enabled = true;
        tiles[snake.Tail()].GetComponent<SpriteRenderer>().enabled = false;
        snake.RemoveLast();
    }

    private IEnumerator Mover(float delay)
    {
        while (true)
        {
            Move();

            yield return new WaitForSeconds(moveDelay);
        }
    }
}


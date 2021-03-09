using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [Header("Size Configuration")]

    [Tooltip("How many rooms a world has in a row.")]
    public int WORLD_WIDTH;
    [Tooltip("How many rooms a world has in a column.")]
    public int WORLD_HEIGHT;
    [Tooltip("How many grids a room has in a row.")]
    public int ROOM_WIDHT;
    [Tooltip("How many grids a room has in a column.")]
    public int ROOM_HEIGHT;

    [Space()]
    [Header("Initial Properties")]

    [Tooltip("Player's startup position.")]
    public Vector2 INIT_PLAYER_POS;
    [Tooltip("First room where player is. (0, 0) for left top room.")]
    public Vector2Int INIT_ROOM_INDEX;
    [Tooltip("Camera")]
    public Camera CAMERA;

    [Space()]
    [Header("Dungeon Settings")]
    [Tooltip("Dungeon prefab.")]
    public GameObject DUNGEON_PREFAB;
    [Tooltip("Prefabs that used to build all kinds of rooms.")]
    public GameObject[] ROOM_PREFABS;
    [Tooltip("How many rooms are required for each type.")]
    public int[] ROOM_NUMBERS;

    private Grid grid;

    private bool[,] occupied;
    private bool[,] connected;

    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateDungeon()
    {
        if (DUNGEON_PREFAB == null)
        {
            Debug.LogError("Please set dungeon prefab in DungeonCreator before running!");
            return;
        }

        grid = DUNGEON_PREFAB.GetComponent<Grid>();
        if (grid == null)
        {
            Debug.Log("Missing grid component in dungeon prefab!");
            return;
        }

        if (ROOM_NUMBERS.Length != ROOM_PREFABS.Length)
        {
            Debug.Log("Please make sure room numbers and room prefabs have the same length!");
            return;
        }

        if (ROOM_NUMBERS.Length > WORLD_WIDTH * WORLD_HEIGHT)
        {
            Debug.Log("Too many rooms are specified!");
            return;
        }

        int total = 0;
        foreach (GameObject prefab in ROOM_PREFABS)
        {
            var room = prefab.GetComponent<DungeonRoom>();
            if (room == null)
            {
                Debug.Log("Missing DungeonRoom component in dungeon room prefab!");
            }
            else
            {
                total += room.REQUIRED_IN_LEVEL;
            }
        }

        occupied = new bool[WORLD_WIDTH, WORLD_HEIGHT];
        connected = new bool[WORLD_WIDTH, WORLD_HEIGHT];
        for (int i = 0; i < WORLD_WIDTH; i++)
        {
            for (int j = 0; j < WORLD_HEIGHT; j++)
            {
                occupied[i, j] = false;
                connected[i, j] = false;
            }
        }

        occupied[0, 0] = true; connected[0, 1] = true;
        ArrayList open_list = new ArrayList();
        open_list.Add(1);

        
    }
}

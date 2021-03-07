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
    public GameObject[] ROOM_PREFAB;

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

        // TODO: 
    }
}

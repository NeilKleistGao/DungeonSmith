using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [Header("Size Options")]
    [Tooltip("Max dungeon's width")]
    public int MAX_WIDHT;
    [Tooltip("Max dungeon's height")]
    public int MAX_HEIGHT;

    [Header("Initial Properties")]
    [Tooltip("Player's startup position.")]
    public Vector2 INIT_PLAYER_POS;
    [Tooltip("Camera")]
    public Camera CAMERA;

    [Space()]
    [Header("Dungeon Settings")]
    [Tooltip("Dungeon prefab.")]
    public GameObject DUNGEON_PREFAB;
    [Tooltip("Prefabs that used to build all kinds of general rooms.")]
    public GameObject[] GENERAL_ROOM_PREFABS;
    [Tooltip("Prefabs that used to build all kinds of special rooms.")]
    public GameObject[] SPECIAL_ROOM_PREFABS;
    [Tooltip("Init room's prefab.")]
    public GameObject INIT_ROOM_PREFABS;
    [Tooltip("Default fighting room's prefab.")]
    public GameObject DEFAULT_ROOM_PREFABS;

    private Grid grid;
    private Vector2Int init_room_pos;
    private Vector2 cell_size;
    private bool[,] visit;

    // Start is called before the first frame update
    void Start()
    {
        if (DUNGEON_PREFAB == null)
        {
            Debug.LogError("Please set dungeon prefab in DungeonCreator before running!");
            return;
        }

        grid = DUNGEON_PREFAB.GetComponent<Grid>();
        if (grid == null)
        {
            Debug.LogError("Missing grid component in dungeon prefab!");
            return;
        }

        cell_size = grid.cellSize;

        if (CAMERA == null)
        {
            Debug.LogError("Missing default camera!");
            return;
        }

        CreateDungeon();

        var temp = grid.CellToWorld(new Vector3Int(init_room_pos.x,
                                    init_room_pos.y, 0));
        CAMERA.transform.position = new Vector3(
            temp.x - cell_size.x / 2, temp.y + cell_size.y / 2,
            CAMERA.transform.position.z
            );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateRoom(GameObject prefab, Vector2Int index)
    {
        var room = Instantiate(prefab);
        room.GetComponent<DungeonRoom>().init();
        Vector3 pos = grid.CellToWorld(new Vector3Int(index.x, index.y, 0));
        room.transform.position = new Vector3(pos.x - cell_size.x / 2, pos.y + cell_size.y / 2, 0);
    }

    void GenerateRoom(GameObject[] prefabs, ref ArrayList require_list, Vector2Int index)
    {
        if (require_list.Count == 0)
        {
            GenerateRoom(DEFAULT_ROOM_PREFABS, index);
        }
        else
        {
            System.Random random = new System.Random();
            int next = random.Next(0, require_list.Count);
            var data = (Vector2Int)require_list.ToArray()[next];
            require_list.RemoveAt(next);

            GenerateRoom(prefabs[data.x], index);

            data.y--;
            if (data.y != 0)
            {
                require_list.Add(data);
            }
        }
    }

    void CreateDungeon()
    {
        visit = new bool[MAX_WIDHT, MAX_HEIGHT];
        for (int i = 0; i < MAX_WIDHT; i++)
        {
            for (int j = 0; j < MAX_HEIGHT; j++)
            {
                visit[i, j] = false;
            }
        }

        var random = new System.Random();
        init_room_pos = new Vector2Int(random.Next(0, MAX_WIDHT),
            random.Next(0, MAX_HEIGHT));
        visit[init_room_pos.x, init_room_pos.y] = true;

        GenerateRoom(INIT_ROOM_PREFABS, init_room_pos);

        foreach (var prefab in SPECIAL_ROOM_PREFABS)
        {
            var room = prefab.GetComponent<DungeonRoom>();
            for (int i = 0; i < room.REQUIRED_IN_LEVEL; i++)
            {
                Vector2Int pos;
                bool flag = false;
                do
                {
                    flag = false;
                    pos = new Vector2Int(random.Next(0, MAX_WIDHT),
                    random.Next(0, MAX_HEIGHT));
                    flag = visit[pos.x, pos.y];

                    if (pos.x - 1 > -1)
                    {
                        flag = flag || visit[pos.x - 1, pos.y];
                    }
                    if (pos.x + 1 < MAX_WIDHT)
                    {
                        flag = flag || visit[pos.x + 1, pos.y];
                    }
                    if (pos.y - 1 > -1)
                    {
                        flag = flag || visit[pos.x, pos.y - 1];
                    }
                    if (pos.y + 1 < MAX_HEIGHT)
                    {
                        flag = flag || visit[pos.x, pos.y + 1];
                    }
                }
                while (flag);

                visit[pos.x, pos.y] = true;
                GenerateRoom(prefab, pos);
            }
        }
    }
}

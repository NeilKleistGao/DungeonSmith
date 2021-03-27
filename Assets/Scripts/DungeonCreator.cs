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
    public int MAX_WIDTH;
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
    private bool[,] available;
    private UnionSet set = null;
    private ArrayList general_list = new ArrayList();
    private ArrayList special_list = new ArrayList();

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

    void GenerateRoom(GameObject[] prefabs, ArrayList id_list)
    {
        var random = new System.Random();
        foreach (var prefab in prefabs)
        {
            var room = prefab.GetComponent<DungeonRoom>();
            for (int i = 0; i < room.REQUIRED_IN_LEVEL; i++)
            {
                int next = random.Next(0, id_list.Count);
                int index = (int)id_list.ToArray()[next];
                id_list.RemoveAt(next);

                GenerateRoom(prefab,
                    new Vector2Int(index % MAX_WIDTH, index / MAX_WIDTH));
            }
        }

        foreach (int index in id_list)
        {
            GenerateRoom(DEFAULT_ROOM_PREFABS,
                new Vector2Int(index % MAX_WIDTH, index / MAX_WIDTH));
        }
    }

    void connect()
    {
        bool flag = true;
        var random = new System.Random();
        while (flag)
        {
            flag = false;
            int init = init_room_pos.y * MAX_WIDTH + init_room_pos.x;
            foreach (int index in special_list)
            {
                if (set.Find(init) != set.Find(index))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                break;
            }


            int next1, next2;
            do
            {
                next1 = random.Next(0, MAX_WIDTH * MAX_HEIGHT);
                next2 = random.Next(0, MAX_WIDTH * MAX_HEIGHT);
            } while (!available[next1 % MAX_WIDTH, next1 / MAX_WIDTH]
                        || !available[next2 % MAX_WIDTH, next2 / MAX_WIDTH]);

            if (set.Union(next1, next2))
            {
                if (!visit[next1 % MAX_WIDTH, next1 / MAX_WIDTH])
                {
                    visit[next1 % MAX_WIDTH, next1 / MAX_WIDTH] = true;
                    general_list.Add(next1);
                }
                if (!visit[next2 % MAX_WIDTH, next2 / MAX_WIDTH])
                {
                    visit[next2 % MAX_WIDTH, next2 / MAX_WIDTH] = true;
                    general_list.Add(next2);
                }
            }
        }
    }

    void CreateDungeon()
    {
        visit = new bool[MAX_WIDTH, MAX_HEIGHT];
        available = new bool[MAX_WIDTH, MAX_HEIGHT];
        set = new UnionSet(MAX_WIDTH * MAX_HEIGHT);
        for (int i = 0; i < MAX_WIDTH; i++)
        {
            for (int j = 0; j < MAX_HEIGHT; j++)
            {
                visit[i, j] = false;
                available[i, j] = true;
            }
        }

        var random = new System.Random();
        init_room_pos = new Vector2Int(random.Next(0, MAX_WIDTH),
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
                ArrayList near = new ArrayList();
                do
                {
                    flag = false;
                    pos = new Vector2Int(random.Next(0, MAX_WIDTH),
                    random.Next(0, MAX_HEIGHT));
                    flag = visit[pos.x, pos.y];

                    near.Clear();

                    if (pos.x - 1 > -1)
                    {
                        if (!visit[pos.x - 1, pos.y])
                        {
                            near.Add(pos.y * MAX_WIDTH + pos.x - 1);
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (pos.x + 1 < MAX_WIDTH)
                    {
                        if (!visit[pos.x + 1, pos.y])
                        {
                            near.Add(pos.y * MAX_WIDTH + pos.x + 1);
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (pos.y - 1 > -1)
                    {
                        if (!visit[pos.x, pos.y - 1])
                        {
                            near.Add((pos.y - 1) * MAX_WIDTH + pos.x);
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (pos.y + 1 < MAX_HEIGHT)
                    {
                        if (!visit[pos.x, pos.y + 1])
                        {
                            near.Add((pos.y + 1) * MAX_WIDTH + pos.x);
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                }
                while (flag);

                visit[pos.x, pos.y] = true;
                GenerateRoom(prefab, pos);

                int next = random.Next(near.Count);
                int index = (int)near.ToArray()[next];
                int curent = pos.y * MAX_WIDTH + pos.x;

                general_list.Add(index);
                special_list.Add(curent);
                set.Union(curent, index);

                foreach (int n in near)
                {
                    if (n != index)
                    {
                        available[n % MAX_WIDTH, n / MAX_WIDTH] = false;
                    }
                }
            }
        }

        connect();
        GenerateRoom(GENERAL_ROOM_PREFABS, general_list);
    }
}

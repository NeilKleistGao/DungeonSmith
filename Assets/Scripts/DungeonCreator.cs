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
    [Tooltip("Init room's prefab.")]
    public GameObject INIT_ROOM_PREFABS;

    private Grid grid;

    private bool[,] occupied;
    private bool[,] connected;
    private ArrayList leaf;

    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int GetIndex(int r, int c)
    {
        return r * WORLD_WIDTH + c;
    }

    void Crop(int require)
    {
        leaf = new ArrayList();
        int[] degrees = new int[WORLD_WIDTH * WORLD_HEIGHT];
        for (int i = 0; i < WORLD_WIDTH; i++)
        {
            for (int j = 0; j < WORLD_HEIGHT; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int degree = 0;
                if (i != 0 && connected[GetIndex(i, j), GetIndex(i - 1, j)])
                {
                    degree++;
                }
                if (i != WORLD_WIDTH - 1 && connected[GetIndex(i, j), GetIndex(i + 1, j)])
                {
                    degree++;
                }
                if (j != 0 && connected[GetIndex(i, j), GetIndex(i, j - 1)])
                {
                    degree++;
                }
                if (j != WORLD_HEIGHT - 1 && connected[GetIndex(i, j), GetIndex(i, j + 1)])
                {
                    degree++;
                }

                degrees[GetIndex(i, j)] = degree;

                if (degree == 1)
                {
                    leaf.Add(GetIndex(i, j));
                }
            }
        }

        int last = WORLD_WIDTH * WORLD_HEIGHT - 1;
        var random = new System.Random();
        while (last > require)
        {
            int next = random.Next(leaf.Count);
            int index = (int)leaf.ToArray()[next];
            leaf.RemoveAt(next);
            occupied[index / WORLD_WIDTH, index % WORLD_WIDTH] = false;
            last--;
            if (index + 1 < WORLD_WIDTH * WORLD_HEIGHT && connected[index, index + 1])
            {
                connected[index, index + 1] = connected[index + 1, index] = false;
                degrees[index + 1]--;
                if (degrees[index + 1] == 1)
                {
                    leaf.Add(index + 1);
                }
            }
            if (index - 1 > 1 && connected[index, index - 1])
            {
                connected[index, index - 1] = connected[index - 1, index] = false;
                degrees[index - 1]--;
                if (degrees[index - 1] == 1)
                {
                    leaf.Add(index - 1);
                }
            }
            if (index + WORLD_WIDTH < WORLD_WIDTH * WORLD_HEIGHT && connected[index, index + WORLD_WIDTH])
            {
                connected[index, index + WORLD_WIDTH] = connected[index + WORLD_WIDTH, index] = false;
                degrees[index + WORLD_WIDTH]--;
                if (degrees[index + WORLD_WIDTH] == 1)
                {
                    leaf.Add(index + WORLD_WIDTH);
                }
            }
            if (index - WORLD_WIDTH > 1 && connected[index, index - WORLD_WIDTH])
            {
                connected[index, index - WORLD_WIDTH] = connected[index - WORLD_WIDTH, index] = false;
                degrees[index - WORLD_WIDTH]--;
                if (degrees[index - WORLD_WIDTH] == 1)
                {
                    leaf.Add(index - WORLD_WIDTH);
                }
            }
        }
    }

    void SetRoom(GameObject prefab, int x, int y)
    {
        var room = Instantiate(prefab);
        room.name = "room";
        var pos = grid.CellToWorld(new Vector3Int(x * ROOM_WIDHT, -y * ROOM_HEIGHT, 0));
        var size = grid.cellSize;
        room.transform.position =
            new Vector3(pos.x + size.x * WORLD_WIDTH / 2, pos.y - size.y * WORLD_HEIGHT, 0);
    }

    void ArrangeUnique()
    {
        foreach (GameObject prefab in ROOM_PREFABS)
        {
            var room = prefab.GetComponent<DungeonRoom>();
            if (room.UNIQUE_ENTRY)
            {
                var random = new System.Random();
                for (int i = 0; i < room.REQUIRED_IN_LEVEL; i++)
                {
                    int next = random.Next(0, leaf.Count);
                    SetRoom(prefab, next / WORLD_WIDTH, next % WORLD_WIDTH);
                }
            }
        }
    }

    void Arrange()
    {
        int roomIndex = 0, roomCount = ROOM_PREFABS[roomIndex].GetComponent<DungeonRoom>().REQUIRED_IN_LEVEL;

        for (int i = 0; i < WORLD_WIDTH; i++) 
        {
            for (int j = 0; j < WORLD_HEIGHT; j++)
            {
                if (occupied[i, j])
                {
                    if (i == 0 && j == 0)
                    {
                        SetRoom(INIT_ROOM_PREFABS, i, j);
                        roomCount--;
                    }
                    else if (!ROOM_PREFABS[roomIndex].GetComponent<DungeonRoom>().UNIQUE_ENTRY) 
                    {
                        if (roomCount == 0)
                        {
                            roomIndex++;
                            if (roomIndex == ROOM_PREFABS.Length)
                            {
                                return;
                            }
                            roomCount = ROOM_PREFABS[roomIndex].GetComponent<DungeonRoom>().REQUIRED_IN_LEVEL;
                        }

                        SetRoom(ROOM_PREFABS[roomIndex], i, j);
                        roomCount--;
                    }
                }
            }
        }
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

        connected = new bool[WORLD_WIDTH * WORLD_HEIGHT, WORLD_WIDTH * WORLD_HEIGHT];
        occupied = new bool[WORLD_WIDTH, WORLD_HEIGHT];
        for (int i = 0; i < WORLD_WIDTH; i++)
        {
            for (int j = 0; j < WORLD_HEIGHT; j++)
            {
                occupied[i, j] = true;
                connected[i, j] = false;
            }
        }

        connected[0, 1] = connected[1, 0] = true;
        int last = WORLD_WIDTH * WORLD_HEIGHT - 1;
        var set = new UnionSet(last + 1);
        var list = new ArrayList();

        for (int i = 0; i < WORLD_WIDTH; i++)
        {
            for (int j = 0; j < WORLD_HEIGHT; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (i != WORLD_WIDTH - 1)
                {
                    list.Add(new KeyValuePair<int, int>(GetIndex(i, j), GetIndex(i + 1, j)));
                }
                if (j != WORLD_HEIGHT - 1)
                {
                    list.Add(new KeyValuePair<int, int>(GetIndex(i, j), GetIndex(i, j + 1)));
                }
            }
        }

        var random = new System.Random();
        while (last > 1)
        {
            int next = random.Next(0, list.Count);
            KeyValuePair<int, int> edge = (KeyValuePair<int, int>)list.ToArray()[next];
            list.RemoveAt(next);

            if (set.Union(edge.Key, edge.Value))
            {
                connected[edge.Key, edge.Value] = connected[edge.Value, edge.Key] = true;
                last--;
            }
        }
        Crop(total);

        ArrangeUnique();
        Arrange();
    }
}

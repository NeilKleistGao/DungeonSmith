using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{

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

    private Grid grid;
    private Vector2Int init_room_pos;
    private bool[,] visit;
    private int max_width;
    private ArrayList general_require;
    private ArrayList special_require;

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
            Debug.Log("Missing grid component in dungeon prefab!");
            return;
        }

        if (CAMERA == null)
        {
            Debug.Log("Missing default camera!");
            return;
        }

        CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateRoom(GameObject[] prefabs, ref ArrayList require_list, Vector2Int index)
    {
        System.Random random = new System.Random();
        int next = random.Next(0, require_list.Count);
        var data = (Vector2Int)require_list.ToArray()[next];
        require_list.RemoveAt(next);

        var room = Instantiate(prefabs[data.x]);
        room.GetComponent<DungeonRoom>().init();
        Vector3 pos = grid.CellToWorld(new Vector3Int(index.x, index.y, 0));
        room.transform.position = pos;

        data.y--;
        if (data.y > 0)
        {
            require_list.Add(data);
        }
    }

    void GeneratePath(Vector2Int current, int general_count, int special_count)
    {
        visit[current.x, current.y] = true;
        if (general_count == 1)
        {
            GenerateRoom(GENERAL_ROOM_PREFABS, ref general_require, current);
            return;
        }
        else if (special_count == 1)
        {
            GenerateRoom(SPECIAL_ROOM_PREFABS, ref special_require, current);

            return;
        }

        GenerateRoom(GENERAL_ROOM_PREFABS, ref general_require, current);

        ArrayList fwd = new ArrayList();
        fwd.Add(new Vector2Int(current.x + 1, current.y));
        fwd.Add(new Vector2Int(current.x - 1, current.y));
        fwd.Add(new Vector2Int(current.x, current.y + 1));
        fwd.Add(new Vector2Int(current.x, current.y - 1));

        System.Random random = new System.Random();
        for (int i = 0; i < 4; i++)
        {
            int next = random.Next(0, 4 - i);
            Vector2Int pos = (Vector2Int)fwd.ToArray()[next];
            fwd.RemoveAt(next);
            if (pos.x > -1 && pos.y > -1 && pos.x < max_width && pos.y < max_width && !visit[pos.x, pos.y])
            {
                int next_gen = random.Next(0, general_count),
                    next_spe = random.Next(0, Math.Min(special_count + 1, next_gen));

                if (i == 3)
                {
                    next_gen = general_count;
                    next_spe = special_count;
                }

                if (next_gen + next_spe > 0)
                {
                    GeneratePath(pos, next_gen, special_count);
                    general_count -= next_gen; special_count -= next_spe;
                }
            }
        }
    }

    void CreateDungeon()
    {
        var camera_pos = CAMERA.transform.position;
        var temp = grid.WorldToCell(camera_pos);
        init_room_pos = new Vector2Int(temp.x, temp.y);

        var room = Instantiate(INIT_ROOM_PREFABS);
        room.GetComponent<DungeonRoom>().init();
        room.transform.position = new Vector3(camera_pos.x, camera_pos.y, 0);

        general_require = new ArrayList();
        special_require = new ArrayList();

        int general_count = 0, special_count = 0, it = 0;
        foreach (var pre in GENERAL_ROOM_PREFABS)
        {
            int r = pre.GetComponent<DungeonRoom>().REQUIRED_IN_LEVEL;
            general_count += r;
            general_require.Add(new Vector2Int(it, r));
            it++;
        }

        it = 0;
        foreach (var pre in SPECIAL_ROOM_PREFABS)
        {
            int r = pre.GetComponent<DungeonRoom>().REQUIRED_IN_LEVEL;
            special_count += r;
            special_require.Add(new Vector2Int(it, r));
            it++;
        }

        max_width = (general_count + special_count) * 2 + 1;
        visit = new bool[max_width, max_width];
        for (int i = 0; i < max_width; i++)
        {
            for (int j = 0; j < max_width; j++)
            {
                visit[i, j] = false;
            }
        }

        GeneratePath(new Vector2Int(max_width / 2, max_width / 2), general_count, special_count);
    }
}

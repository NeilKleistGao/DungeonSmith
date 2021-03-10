using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Generation Options")]
    [Tooltip("How many rooms in this form are needed in the map.")]
    public int REQUIRED_IN_LEVEL;
    [Tooltip("Should this room have a unique entry.")]
    public bool UNIQUE_ENTRY;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

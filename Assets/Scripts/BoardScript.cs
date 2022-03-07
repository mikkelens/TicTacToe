using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script is used on each board, and a new board is spawned every round.
/// </summary>
public class BoardScript : MonoBehaviour
{
    [Header("Settings")] // assigned in prefab
    // [SerializeField] private float horizontalSpawnDistance = 4f;
    [SerializeField] private float verticalSpawnDistance = 6f;
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private Color redColor = Color.red;
    
    private Manager _manager;
    
    private SpaceScript[,] _cubes = new SpaceScript[3, 3];
    private PlayerColors[,] _board = new PlayerColors[3, 3];
    private Vector2Int _lastBlueCoords;
    public Vector2Int LastBlueCoords => _lastBlueCoords;
    private Vector2Int _lastRedCoords;
    public Vector2Int LastRedCoords => _lastRedCoords;

    private int _turns;
    public PlayerColors PlayerTurn => _turns % 2 == 0 ? PlayerColors.Blue : PlayerColors.Red;

    public void OnSpawn(PlayerColors[,] permanents) // called with 
    {
        _manager = Manager.Main;
        _turns = 0;

        // show permanent
        _board = permanents;
        AddCubeColors(permanents);

        // start
        // GameStart();
    }
    
    private void AddCubeColors(PlayerColors[,] addedColors)
    {
        for (int x = 0; x < addedColors.Length; x++)
        {
            for (int y = 0; y < addedColors.Length; y++) // assumed square dimensions
            {
                PlayerColors playerColor = addedColors[x, y];
                if (playerColor == PlayerColors.None) return;
                
                Material material = _cubes[x, y].GetComponent<MeshRenderer>().sharedMaterial;

                Color color = playerColor == PlayerColors.Blue ? blueColor : redColor;
                material.color = color;
            }
        }
    }

    // "Game" is defined as a tic tac toe game within the metagame (multiple "games"),
    // Games are run by player interaction directly (each turn).
    // "Turn" is a player turn

    /// <summary>
    /// Called by a space that got pressed. Calling this switches turn.
    /// </summary>
    /// <param name="space"></param>
    public void PlacedShapeOnSpace(SpaceScript space)
    {
        // spawn prefab
        if (PlayerTurn == PlayerColors.Blue)
        {
            SpawnShapePrefab(bluePrefab, space.transform.position);
            _lastBlueCoords = space.Coords;
        }
        else
        {
            SpawnShapePrefab(redPrefab, space.transform.position);
            _lastRedCoords = space.Coords;
        }

        // turn ends
        _turns++;
    }

    public void SpawnShapePrefab(GameObject prefab, Vector3 pos)
    {
        Vector3 verticalOffset = Vector3.up * verticalSpawnDistance;
        
        Transform shapeTransform = Instantiate(prefab).transform; // spawn, get transform
        shapeTransform.position = pos + verticalOffset; // set up in the air
        shapeTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
    }
}

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
    
    private Transform[,] _pieces = new Transform[3, 3];
    private PlayerColors[,] _boardColors = new PlayerColors[3, 3];
    public PlayerColors[,] BoardColors => _boardColors;
    private Vector2Int _lastBlueCoords;
    public Vector2Int LastBlueCoords => _lastBlueCoords;
    private Vector2Int _lastRedCoords;
    public Vector2Int LastRedCoords => _lastRedCoords;

    private int _roundTurns;
    public PlayerColors PlayerTurn => _roundTurns % 2 == 0 ? PlayerColors.Blue : PlayerColors.Red;
    public PlayerColors LastPlayerTurn => _roundTurns % 2 == 1 ? PlayerColors.Blue : PlayerColors.Red;
    
    public void StartNewRound()
    {
        _manager = Manager.Main;
        CleanBoard();
        //_roundTurns++;
        _boardColors = (PlayerColors[,])_manager.PermanentColors.Clone();
    }

    private void CleanBoard()
    {
        for (int x = 0; x < _boardColors.GetLength(0); x++)
        {
            for (int y = 0; y < _boardColors.GetLength(1); y++)
            {
                // Debug.Log($"X: {x}, Y: {y}, LENGTH 0: {_boardColors.GetLength(0)}, LENGTH 1: {_boardColors.GetLength(1)}");
                _boardColors[x, y] = PlayerColors.None;
                Transform piece = _pieces[x, y];
                
                if (piece != null)
                {
                    if (_manager.PermanentColors[x, y] == PlayerColors.None) // a piece and not permanent
                    {
                        Destroy(piece.gameObject);
                    }
                }
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
    public void PlacedShape(SpaceScript space)
    {
        // spawn prefab
        if (PlayerTurn == PlayerColors.Blue)
        {
            SpawnShapeOnSpace(bluePrefab, space);
            _lastBlueCoords = space.Coords;
        }
        else
        {
            SpawnShapeOnSpace(redPrefab, space);
            _lastRedCoords = space.Coords;
        }

        // turn ends
        _boardColors[space.Coords.x, space.Coords.y] = PlayerTurn;
        _roundTurns++;
    }

    private void SpawnShapeOnSpace(GameObject prefab, SpaceScript space)
    {
        Vector3 verticalOffset = Vector3.up * verticalSpawnDistance;
        
        Transform shapeTransform = Instantiate(prefab).transform; // spawn, get transform
        shapeTransform.position = space.transform.position + verticalOffset; // set up in the air
        shapeTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        
        Vector2Int coords = space.Coords;
        _pieces[coords.x, coords.y] = shapeTransform;
        
        Material material = shapeTransform.GetComponent<MeshRenderer>().material;
        material.color = PlayerTurn == PlayerColors.Blue ? blueColor : redColor;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerColors
{
    None,
    Blue,
    Red
}

public class Manager : MonoBehaviour
{
    public static Manager Main;
    
    [Header("Settings")]
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private Transform piecesParentParent;
    public Transform PiecesParent => piecesParentParent;
    
    
    // assigned in script just shown for convenience
    private BoardScript _board;
    private Camera _cam;

    // readonly is assuming only one metagame will be played
    // (metagames are the full funny thing, games are just normal tic tac toe type events)
    private readonly PlayerColors[,] _permanentColors = new PlayerColors[3, 3];

    private void Start()
    {
        if (piecesParentParent == null) piecesParentParent = transform;
        Main = this; // assuming there is only one
        _cam = Camera.main;
        
        // start game, spawn board
        SpawnNewBoard();
    }

    private void SpawnNewBoard()
    {
        _board = Instantiate(boardPrefab).GetComponent<BoardScript>();
        _board.OnSpawn(_permanentColors);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastForSpace();
        }
    }

    private void RaycastForSpace()
    {

        // raycast on space, call spacescript.pressspace with board
        int mask = LayerMask.GetMask("Space");
        if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, mask))
        {
            SpaceScript space = hit.transform.GetComponent<SpaceScript>();
            if (space != null)
            {
                space.SpacePressed(_board);
            }
        }
    }

    
    /// <summary>
    /// Called by pressing "move on" button (not implemented)
    /// </summary>
    public void EndAGame()
    {
        PlayerColors color = _board.PlayerTurn;
        Vector2Int permPos = color == PlayerColors.Blue ? _board.LastBlueCoords : _board.LastRedCoords;
        AddNewPermanent(color, permPos);
        Destroy(_board.gameObject);
    }

    private void AddNewPermanent(PlayerColors color, Vector2Int permPos)
    {
        int x, y;
        x = permPos.x + 1;
        y = permPos.y + 1;
        _permanentColors[x, y] = color;
    }
}

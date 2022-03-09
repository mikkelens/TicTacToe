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
    [SerializeField] private BoardScript board;
    [SerializeField] private Transform piecesParentParent;
    public Transform PiecesParent => piecesParentParent;
    
    // assigned in script just shown for convenience
    private Camera _cam;

    // readonly is assuming only one metagame will be played
    // (metagames are the full funny thing, games are just normal tic tac toe type events)
    private PlayerColors[,] _permanentColors = new PlayerColors[3, 3];
    private bool[,] _Higlights = new bool[3, 3];
    public PlayerColors[,] PermanentColors => _permanentColors;

    private Manager() { }

    private void Awake()
    {
        if (piecesParentParent == null) piecesParentParent = transform;
        Main = this; // assuming there is only one
        _cam = Camera.main;
    }

    private void Start()
    {
        RestartGame();
    }

    // potentially called by button?, but always called on scene load
    public void RestartGame()
    {
        // Debug.Log("Started a new game.");
        board.StartNewRound(); // start round with empty board
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
                space.SpacePressed(board);
            }
        }
    }
    
    /// <summary>
    /// Called by pressing "end round" button
    /// </summary>
    public void EndRound()
    {
        Debug.Log("Ended a round");
        
        PlayerColors color = board.PlayerTurn; // not last player turn because game is jank and "ahead"
        Vector2Int newPermPos = color == PlayerColors.Blue ? board.LastBlueCoords : board.LastRedCoords;
        AddNewPermanent(color, newPermPos);
        
        board.StartNewRound();
    }

    private void AddNewPermanent(PlayerColors color, Vector2Int permPos)
    {
        Debug.Log($"New permanent: {permPos - Vector2Int.one}");
        _permanentColors[permPos.x, permPos.y] = color;

        _Higlights[permPos.x, permPos.y] = true;
        board.pieces[permPos.x, permPos.y].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white * board.Witnes);
    }
}
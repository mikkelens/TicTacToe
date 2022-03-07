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

public class PermanentManager : MonoBehaviour
{
    public static PermanentManager Main;
    
    [Header("Settings")]
    [SerializeField] private Color blueColor;
    public Color Blue => blueColor;
    [SerializeField] private Color redColor;
    public Color Red => redColor;
    
    [Header("Debug")]
    // assigned in script just shown for convenience
    [SerializeField] private PermanentBoardScript currentBoard;
    
    // readonly is assuming only one metagame will be played
    // (metagames are the full funny thing, games are just normal tic tac toe type events)
    private readonly PlayerColors[,] _currentPermanents = new PlayerColors[3, 3];

    private void Start()
    {
        Main = this; // assuming there is only one
        
        // start game, spawn board
        currentBoard.BoardSpawn(_currentPermanents);
    }

    /// <summary>
    /// Called from the current board
    /// </summary>
    public void EndGame(PlayerColors[,] newPermanents)
    {
        for (int x = 0; x < newPermanents.Length; x++)
        {
            for (int y = 0; y < newPermanents.Length; y++)
            {
                if (newPermanents[x, y] != PlayerColors.None)
                {
                    _currentPermanents[x, y] = newPermanents[x, y];
                }
            }
        }
    }
}

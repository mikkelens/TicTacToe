using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script is used on each board, and a new board is spawned every round.
/// </summary>
public class PermanentBoardScript : MonoBehaviour
{
    private PermanentManager _manager;
    
    private PermanentCubeScript[,] _cubes = new PermanentCubeScript[3, 3];
    private PlayerColors[,] _currentBoard = new PlayerColors[3, 3];

    public void BoardSpawn(PlayerColors[,] permanents) // called with 
    {
        _manager = PermanentManager.Main;
        // spawn prefab

        // show permanent
        _currentBoard = permanents;
        AddCubeColors(permanents);

        // start
        // GameStart();
    }
    

    void AddCubeColors(PlayerColors[,] addedColors)
    {
        for (int x = 0; x < addedColors.Length; x++)
        {
            for (int y = 0; y < addedColors.Length; y++) // assumed square dimensions
            {
                PlayerColors color = addedColors[x, y];
                if (color == PlayerColors.None) return;
                
                Material material = _cubes[x, y].GetComponent<MeshRenderer>().sharedMaterial;
                
                if (color == PlayerColors.Blue) material.color = _manager.Blue;
                else if (color == PlayerColors.Red) material.color = _manager.Red;
            }
        }
    }

    // "Game" is defined as a tic tac toe game within the metagame (multiple "games"),
    // Games are run by player interaction directly
    // "Turn" is a player turn

    void GameEnd()
    {
        _manager.EndGame(_currentBoard);
        // destroy/disable?
    }
}

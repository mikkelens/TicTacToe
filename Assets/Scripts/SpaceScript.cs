using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceScript : MonoBehaviour
{
    // this has to be set in the inspector!
    [SerializeField] private Vector2Int coords = Vector2Int.zero;
    public Vector2Int Coords => coords;
    
    private bool _filled;

    // called by raycast
    public void SpacePressed(BoardScript board)
    {
        if (_filled) return;
        _filled = true; // can only be filled once

        board.PlacedShapeOnSpace(this);
    }
}

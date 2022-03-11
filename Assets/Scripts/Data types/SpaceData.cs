using UnityEngine;

/// <summary>
/// Instances of this class represent the spaces on the board.
/// Example use in 2D array:
/// representing a board, storing what pieces are on them.
/// </summary>
public class SpaceData
{
    public Transform PhysicalSpaceTransform;
    public SpaceScript Script;
    public Vector2Int Coords; // position of the spaceData on the grid
    public PieceData CurrentPieceData; // if not null, references a physical pieceData
    public SpaceData() { }
}
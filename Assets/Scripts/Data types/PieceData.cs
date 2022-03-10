using UnityEngine;

/// <summary>
/// Instances of this class represent pieces on the board.
/// They are not physically connected to the pieceData gameobjects.
/// </summary>
public class PieceData
{
    public Transform PTransform; // reference to the physical transform
    public PlayerColor ColorType; // color (blue or red)
    public bool IsPermanent; // whether it is a permanent pieceData or not.
}
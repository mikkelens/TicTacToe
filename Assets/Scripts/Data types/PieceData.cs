using UnityEngine;

/// <summary>
/// Instances of this class represent pieces on the board.
/// They are not physically connected to the pieceData gameobjects.
/// </summary>
public class PieceData
{
    public Transform PTransform; // reference to the physical transform
    public Rigidbody Rb;
    public bool IsPermanent; // whether it is a permanent pieceData or not.
    public PlayerType Type; // color (blue or red)
    public PlayerShapeInfo Info;
}
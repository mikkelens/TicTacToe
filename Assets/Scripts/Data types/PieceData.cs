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
    public PlayerType Type; // type (blue or red)
    public PlayerShapeInfo Info;
    private AudioClip _landSfx;
    public AudioClip LandSfx
    {
        get => _landSfx != null ? _landSfx : Info.landSfx;
        set => _landSfx = value;
    }
}
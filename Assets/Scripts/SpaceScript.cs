using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The script added to physical spaces.
/// </summary>
public class SpaceScript : MonoBehaviour
{
    // this has to be set in the inspector!
    [Min(-1), MaxValue(1)]
    [SerializeField, Required] private Vector2Int placementOnBoard = Vector2Int.zero;
    private Vector2Int Coords => placementOnBoard + new Vector2Int(1, 1); // offset to be used for arrays
    public SpaceData SpaceData;
    
    private void Start()
    {
        SpaceData = Manager.Main.board.Spaces[Coords.x, Coords.y];
        SpaceData.Coords = Coords;
        SpaceData.PhysicalSpaceTransform = transform;
    }
}

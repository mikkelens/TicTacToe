using System;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The script added to physical spaces.
/// </summary>
public class SpaceScript : MonoBehaviour
{
    [UsedImplicitly]
    private Vector2Int[] AllowedV2IValues
    {
        get
        {
            Vector2Int[] values = new Vector2Int[9];
            int i = 0;
            for (int y = 1; y >= -1; y--)
            {
                for (int x = -1; x <= 1; x++)
                {
                    values[i] = new Vector2Int(x, y);
                    i++;
                }
            }
            return values;
        }
    }
    [UsedImplicitly]
    private bool CheckIfValidV2I(Vector2Int value)
    {
        return AllowedV2IValues.Contains(value);
    }
    [ValueDropdown("AllowedV2IValues")]
    [ValidateInput("CheckIfValidV2I", 
        "This can only contain values [-1], [0] or [1].")]
    [SerializeField] private Vector2Int placementOnBoard = Vector2Int.zero;

    private AudioSource _audio;
    public bool CanPlayAudio { get; set; }
    
    // offset to be used for arrays
    private Vector2Int Coords => placementOnBoard + new Vector2Int(1, 1);
    
    public SpaceData SpaceData;
    
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        CanPlayAudio = true;
        SpaceData = Manager.Main.board.Spaces[Coords.x, Coords.y];
        SpaceData.Script = this;
        SpaceData.PhysicalSpaceTransform = transform;
        SpaceData.Coords = Coords;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!CanPlayAudio) return;
        
        int layer = LayerMask.NameToLayer("Shape");
        
        if (col.gameObject.layer != layer) return;
        
        Debug.Log("Shape hit space.");
        PlaySfx();
        CanPlayAudio = false;
    }

    private void PlaySfx()
    {
        PieceData currentPiece = SpaceData.CurrentPieceData;
        if (currentPiece == null) return;
        // if piece exists
        
        // set piece audio clip
        _audio.clip = currentPiece.Info.landSfx;
        _audio.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script is used on each board, and a new board is spawned every round.
/// </summary>
public class BoardScript : MonoBehaviour
{
    [Header("Settings")] // assigned in prefab
    // [SerializeField] private float horizontalSpawnDistance = 4f;
    [SerializeField] private float verticalSpawnDistance = 6f;
    [SerializeField] private float verticalSpawnVelocity = -5f;
    [SerializeField] private float waitAfterWinDetection = 1.5f;
    
    [SerializeField] private PlayerShapeInfo bluePlayerShapeInfo;
    [SerializeField] private PlayerShapeInfo redPlayerShapeInfo;

    [SerializeField] private Material permanentMaterial;
    
    [SerializeField] private Image shapeIcon;
    
    private bool _ending;
    private int _roundTurns;

    private PlayerShapeInfo Current => _roundTurns % 2 == 0 ? B : R;
    private PlayerShapeInfo B => bluePlayerShapeInfo;
    private PlayerShapeInfo R => redPlayerShapeInfo;
    
    private int TotalSpaceCount => Spaces.GetLength(0) * Spaces.GetLength(1);
    public readonly SpaceData[,] Spaces = new SpaceData[3, 3];
    private Manager _manager;

    private BoardScript() { }

    private void Awake()
    {
        CreateBoard();
        _manager = Manager.Main;
        _ending = false;

        void CreateBoard()
        {
            for (int x = 0; x < Spaces.GetLength(0); x++)
            {
                for (int y = 0; y < Spaces.GetLength(1); y++)
                {
                    Spaces[x, y] = new SpaceData();
                }
            }
        }
    }
    
    public void StartNewRound()
    {
        if (_ending) return;

        CleanBoard();
        if (CheckInfiniteWin() != EndState.Continue)
        {
            Debug.Log("Infinite win detected.");
            _ending = true;
            MetaWin();
        }
        UpdateIcon();
    }


    /// <summary>
    /// Checks all permanent spaces if they have a situation where player has won infinitely
    /// </summary>
    /// <returns></returns>
    private EndState CheckInfiniteWin()
    {
        int spacesFilled = 0;
        
        for (int x = 0; x < Spaces.GetLength(0); x++)
        {
            for (int y = 0; y < Spaces.GetLength(1); y++)
            {
                SpaceData originSpaceData = Spaces[x, y];
                PieceData originPieceData = originSpaceData.CurrentPieceData;

                if (originPieceData == null || !originPieceData.IsPermanent) continue;
                // if pieceData is permanent

                spacesFilled++;
                
                if (originPieceData.Type != Current.type) continue;
                // if player can place immediately (needed for instant infinite win)

                // check if surrounding pieces have spaceData and that spaceData is same color type
                foreach ((PieceData data, Vector2Int offset) nextPermanent in GetSurroundingAlikePieces(originSpaceData, true))
                {
                    if (nextPermanent.data.Type != Current.type) continue;

                    // try to access the opposite spaceData of the one under nextPermanent
                    Vector2Int oppositePos = originSpaceData.Coords - nextPermanent.offset;
                    if (!IsValidSpace(oppositePos)) continue;
                    // if space exists

                    SpaceData oppositeSpaceData = Spaces[oppositePos.x, oppositePos.y];
                    PieceData oppositePieceData = oppositeSpaceData.CurrentPieceData;
                    if (oppositePieceData != null) continue;
                    // if there is space for a piece to be put

                    // place it!
                    PlaceShape(oppositeSpaceData);
                    
                    return EndState.Win; // detected that a player has "infinitely" won.
                }
            }
        }

        if (spacesFilled == TotalSpaceCount) return EndState.Draw;
        
        return EndState.Continue;
    }

    private (PieceData, Vector2Int)[] GetSurroundingAlikePieces(SpaceData originSpaceData, bool permanentOnly)
    {
        PieceData originPieceData = originSpaceData.CurrentPieceData;
        List<(PieceData, Vector2Int)> surroundingPieces = new List<(PieceData, Vector2Int)>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int target = originSpaceData.Coords + offset;
                
                if (!IsValidSpace(target)) continue;
                // if spaceData *can* exist
                
                SpaceData spaceData = Spaces[target.x, target.y];
                if (spaceData == null) continue;
                PieceData pieceData = spaceData.CurrentPieceData;
                if (pieceData == null) continue;
                // if piece exists

                if (permanentOnly && !pieceData.IsPermanent) continue;
                if (pieceData.Type != originPieceData.Type) continue;
                // if spaceData is one we are looking for
                
                // add it to the list of surrounding pieces
                surroundingPieces.Add((pieceData, offset));
            }
        }
        return surroundingPieces.ToArray();
    }

    private void MetaWin()
    {
        // throw new System.NotImplementedException();
    }
    
    private void CleanBoard()
    {
        for (int x = 0; x < Spaces.GetLength(0); x++)
        {
            for (int y = 0; y < Spaces.GetLength(1); y++)
            {
                SpaceData spaceData = Spaces[x, y];
                ref PieceData pieceData = ref spaceData.CurrentPieceData;

                if (pieceData == null) continue;
                // piece exists
                
                if (pieceData.IsPermanent) continue;
                // piece is not permanent

                Destroy(pieceData.PTransform.gameObject);
                pieceData = null;
                spaceData.Script.CanPlayAudio = true;
            }
        }
    }

    /// <summary>
    /// Called by a spaceData that got pressed. Calling this switches turn.
    /// </summary>
    /// <param name="spaceData"></param>
    public void PlaceShape(SpaceData spaceData)
    {
        if (_ending) return;
        
        PlayerShapeInfo playerShapeInfo = Current; // get correct shape

        // spawn pieceData
        PieceData placedPieceData = SpawnShapeOnSpace(playerShapeInfo.prefab, spaceData); // spawn it
        placedPieceData.Type = Current.type;
        spaceData.CurrentPieceData = placedPieceData;
        
        playerShapeInfo.SpaceDataLastSpawnedOn = spaceData; // store last used spaceData

        // turn ends
        IncrementTurn();
        EndState endState = CheckForWin();
        if (endState != EndState.Continue)
        {
            _ending = true;
            
            StartCoroutine(WinRoutine());
        }
    }

    /// <summary>
    /// Physically spawns a shape in the world on the spaceData provided.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="spaceData"></param>
    private PieceData SpawnShapeOnSpace(GameObject prefab, SpaceData spaceData)
    {
        PieceData newPieceData = new PieceData(); // to store pieceData information
        
        Vector2Int coords = spaceData.Coords;
        Spaces[coords.x, coords.y].CurrentPieceData = newPieceData;

        Vector3 spawnOffset = Vector3.up * verticalSpawnDistance;
        Transform pTransform = Instantiate(prefab).transform; // spawn, get transform
        pTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        pTransform.position = spaceData.PhysicalSpaceTransform.position + spawnOffset; // set up in the air
        newPieceData.PTransform = pTransform;
        
        
        Rigidbody rb = pTransform.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0f, verticalSpawnVelocity, 0f);
        newPieceData.Rb = rb;

        Material material = pTransform.GetComponent<MeshRenderer>().material;
        material.color = Current.color;

        return newPieceData;
    }

    #region win/end check
    /// <summary>
    /// Checks if game ends.
    /// </summary>
    /// <returns>The end state that should be used.</returns>
    private EndState CheckForWin()
    {
        // algorithm checks for every spaceData
        int spacesFilled = 0;
        for (int x = 0; x < Spaces.GetLength(0); x++)
        {
            for (int y = 0; y < Spaces.GetLength(1); y++)
            {
                SpaceData spaceData = Spaces[x, y];
                PieceData pieceData = spaceData.CurrentPieceData;
                if (pieceData == null) continue;
                // if pieceData exists
                
                spacesFilled++;

                // check for win
                if (CheckDirectionsFromSpace(spaceData))
                    return EndState.Win;
            }
        }

        // check for draw
        if (spacesFilled == TotalSpaceCount)
            return EndState.Draw;
        
        // if nothing indicating game should end, it continues (and players can continue placing)
        return EndState.Continue;

    }
    #region win check
    private bool CheckDirectionsFromSpace(SpaceData origin)
    {
        // check for every spaceData the originSpaceData spaceData.
        // Think of it as a 3x3 grid (with no middle), centered on originSpaceData coords, with each outer check as a "direction"
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // x and y are treated as direction vector components
                if (x == 0 && y == 0) continue; // no direction is not a direction

                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int target = origin.Coords + offset;
                Vector2Int opposite = origin.Coords - offset;
                
                // Debug.Log($"Origin: {originCoords}, Target: {targetCoords}, Opposite: {oppositeCoords}");
                
                if (!IsValidSpace(target)) continue;
                if (!IsValidSpace(opposite)) continue;
                // a line of spaces exist

                // we already know spaces exist at coordinates so get them without extra checks
                PieceData targetPiece = Spaces[target.x, target.y].CurrentPieceData;
                PieceData oppositePiece = Spaces[opposite.x, opposite.y].CurrentPieceData;
                if (targetPiece == null || oppositePiece == null) continue;
                // if both target and opposite piece exist

                if (CheckPieceSimilarity(targetPiece, oppositePiece, origin.CurrentPieceData.Type))
                    return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Returns true if target spaceData and opposite spaceData is the same color
    /// </summary>
    /// <param name="target"></param>
    /// <param name="opposite"></param>
    /// <param name="originType"></param>
    /// <returns></returns>
    private bool CheckPieceSimilarity(PieceData target, PieceData opposite, PlayerType originType)
    {
        PlayerType targetType = target.Type;
        PlayerType oppositeType = opposite.Type;
        
        if (targetType != originType) return false; // target spaceData is not same originCoords spaceData
        return targetType == oppositeType; // if target spaceData is same as opposite spaceData
    }
    #endregion // win
    #endregion // end

    private bool IsValidSpace(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0) return false;
        if (coords.x >= Spaces.GetLength(0)) return false;
        if (coords.y >= Spaces.GetLength(1)) return false;
        return true;
    }
    
    private void IncrementTurn()
    {
        _roundTurns++;
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        shapeIcon.sprite = Current.icon;
    }

    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(waitAfterWinDetection);
        _ending = false;
        EndRound();
    }
    
    /// <summary>
    /// Called by pressing "end round" button
    /// </summary>
    private void EndRound()
    {
        if (_ending) return;

        TryAddPermanent(Current.SpaceDataLastSpawnedOn);
        IncrementTurn();
        StartNewRound();
    }

    private void TryAddPermanent(SpaceData spaceData)
    {
        if (spaceData == null) return;
        // if space exists
        
        PieceData pieceData = spaceData.CurrentPieceData;
        if (pieceData == null) return;
        // if pieceData exists (and can be made permanent)
        
        MakePiecePermanent(pieceData);
    }

    private void MakePiecePermanent(PieceData pieceData)
    {
        Transform pieceTransform = pieceData.PTransform;
        
        pieceData.IsPermanent = true;
        
        // make pieceData shine
        MeshRenderer meshRenderer = pieceTransform.GetComponent<MeshRenderer>();
        meshRenderer.material = permanentMaterial;
        meshRenderer.material.color = Current.color;
        meshRenderer.material.SetColor("_EmissionColor", Current.color);
    }
}

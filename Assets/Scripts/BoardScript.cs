using System.Collections;
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
    [SerializeField] private float waitAfterWin = 3f;
    
    [SerializeField] private PlayerData bluePlayer;
    [SerializeField] private PlayerData redPlayer;

    [SerializeField] private Material permanentMaterial;


    [SerializeField] private Image iconImage;

    private PlayerData B => bluePlayer;
    private PlayerData R => redPlayer;
    
    public readonly PieceData[,] Pieces = new PieceData[3, 3];

    private Vector2Int LastBlueCoords => _lastBlueCoords;
    private Vector2Int _lastBlueCoords;
    private Vector2Int LastRedCoords => _lastRedCoords;
    private Vector2Int _lastRedCoords;
    
    private bool _ending;
    private int _roundTurns;

    private Manager _manager;
    
    private PlayerColor PlayerTurn => _roundTurns % 2 == 1 ? PlayerColor.Blue : PlayerColor.Red;
    private PlayerData CurrentPlayer => _roundTurns % 2 == 1 ? B : R;

    private BoardScript() { }

    private void Awake()
    {
        InizializePieces();
        _manager = Manager.Main;
        _ending = false;

        void InizializePieces()
        {
            for (int x = 0; x < Pieces.GetLength(0); x++)
            {
                for (int y = 0; y < Pieces.GetLength(1); y++)
                {
                    Pieces[x, y] = new PieceData();
                }
            }
        }
    }
    
    public void StartNewRound(bool yes = false)
    {
        if (_ending && yes) return;
        
        IncrementTurn(); // <- only because game always ends on loser's turn, we increment to make it the winner's turn
        CleanBoard();
        for (int x = 0; x < Pieces.GetLength(0); x++)
        {
            for (int y = 0; y < Pieces.GetLength(1); y++)
            {
                Pieces[x, y].BoardColor = Pieces[x, y].PermanentColor; 
            }
        }
    }

    private void CleanBoard()
    {
        for (int x = 0; x < Pieces.GetLength(0); x++)
        {
            for (int y = 0; y < Pieces.GetLength(1); y++)
            {
                Pieces[x, y].BoardColor = PlayerColor.None;
                Transform piece = Pieces[x, y].Piece;
                
                if (piece != null)
                {
                    if (Pieces[x, y].PermanentColor != PlayerColor.None) continue;
                    
                    Pieces[x, y].Piece = null;
                    Destroy(piece.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Called by a space that got pressed. Calling this switches turn.
    /// </summary>
    /// <param name="space"></param>
    public void PlacedShape(SpaceScript space)
    {
        if (_ending) return;
        
        // spawn prefab
        if (PlayerTurn == PlayerColor.Blue)
        {
            SpawnShapeOnSpace(B.prefab, space);
            _lastBlueCoords = space.Coords;
        }
        else
        {
            SpawnShapeOnSpace(R.prefab, space);
            _lastRedCoords = space.Coords;
        }

        // turn ends
        Pieces[space.Coords.x, space.Coords.y].BoardColor = PlayerTurn;
        IncrementTurn();

        EndState endState = CheckForEnd();
        if (endState != EndState.Continue)
        {
            _ending = true;
            
            StartCoroutine(WinRoutine());
        }
    }

    /// <summary>
    /// Physically spawns a shape in the world on the space provided.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="space"></param>
    private void SpawnShapeOnSpace(GameObject prefab, SpaceScript space)
    {
        Vector3 verticalOffset = Vector3.up * verticalSpawnDistance;
        
        Transform shapeTransform = Instantiate(prefab).transform; // spawn, get transform
        shapeTransform.position = space.transform.position + verticalOffset; // set up in the air
        shapeTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        
        Vector2Int coords = space.Coords;
        Pieces[coords.x, coords.y].Piece = shapeTransform;
        
        Material material = shapeTransform.GetComponent<MeshRenderer>().material;
        material.color = CurrentPlayer.color;
    }


    #region win/end check
    enum EndState
    {
        Continue,
        Win,
        Draw
    }
    
    /// <summary>
    /// Checks if game ends.
    /// </summary>
    /// <returns>
    /// The end state that should be used.
    /// </returns>
    private EndState CheckForEnd()
    {
        // algorithm checks for every space
        int spacesFilled = 0;
        for (int x = 0; x < Pieces.GetLength(0); x++)
        {
            for (int y = 0; y < Pieces.GetLength(1); y++)
            {
                PlayerColor spaceColor = Pieces[x, y].BoardColor;
                if (spaceColor == PlayerColor.None) continue;
             
                spacesFilled++;

                // check for win
                if (CheckDirectionsFromSpace(new Vector2Int(x, y), spaceColor))
                    return EndState.Win;
            }
        }

        // check for draw
        int spaceCount = Pieces.GetLength(0) * Pieces.GetLength(1);
        if (spacesFilled == spaceCount)
            return EndState.Draw;
        
        // if nothing indicating game should end, it continues (and players can continue placing)
        return EndState.Continue;

    }

    #region win check
    private bool CheckDirectionsFromSpace(Vector2Int originCoords, PlayerColor color)
    {
        // check for every space the origin space.
        // Think of it as a 3x3 grid (with no middle), centered on origin coords, with each outer check as a "direction"
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // x and y are treated as direction vector components
                if (x == 0 && y == 0) continue; // no direction is not a direction

                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int targetCoords = originCoords + offset;
                Vector2Int oppositeCoords = originCoords - offset;
                
                // Debug.Log($"Origin: {originCoords}, Target: {targetCoords}, Opposite: {oppositeCoords}");
                
                if (!IsValidSpace(targetCoords)) continue;
                if (!IsValidSpace(oppositeCoords)) continue;
                
                if (CheckInDirection(targetCoords, oppositeCoords, color))
                    return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Returns true if target space and opposite space is the same color
    /// </summary>
    /// <param name="target"></param>
    /// <param name="opposite"></param>
    /// <param name="originColor"></param>
    /// <returns></returns>
    private bool CheckInDirection(Vector2Int target, Vector2Int opposite, PlayerColor originColor)
    {
        PlayerColor targetColor = Pieces[target.x, target.y].BoardColor;
        PlayerColor oppositeColor = Pieces[opposite.x, opposite.y].BoardColor;
        
        if (targetColor != originColor) return false; // target space is not same originCoords space
        return targetColor == oppositeColor; // if target space is same as opposite space
    }
    #endregion // win
    #endregion // end

    private bool IsValidSpace(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0) return false;
        if (coords.x >= Pieces.GetLength(0)) return false;
        if (coords.y >= Pieces.GetLength(1)) return false;
        return true;
    }
    
    private void IncrementTurn()
    {
        _roundTurns++;
        iconImage.sprite = CurrentPlayer.icon;
    }
    
    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(waitAfterWin);
        _ending = false;
        EndRound();
    }
    
    /// <summary>
    /// Called by pressing "end round" button
    /// </summary>
    private void EndRound()
    {
        if (_ending) return;

        PlayerColor color = PlayerTurn; // not last player turn because game is jank and "ahead"
        Vector2Int newPermPos = color == PlayerColor.Blue ? LastBlueCoords : LastRedCoords;
        AddNewPermanent(color, newPermPos);
        StartNewRound();
    }

    private void AddNewPermanent(PlayerColor color, Vector2Int permPos)
    {
        Pieces[permPos.x, permPos.y].PermanentColor = color;

        Pieces[permPos.x, permPos.y].IsHigligthed = true;
        Transform piece = Pieces[permPos.x, permPos.y].Piece;
        MeshRenderer meshRenderer = piece.GetComponent<MeshRenderer>();
        meshRenderer.material = permanentMaterial;
        meshRenderer.material.color = CurrentPlayer.color;
        meshRenderer.material.SetColor("_EmissionColor", CurrentPlayer.color);
    }
}

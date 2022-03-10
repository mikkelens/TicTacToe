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
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Material permanentMaterial;

    [SerializeField] private float waitAfterWin = 3f;

    [SerializeField] private Image shapeIcon;
    [SerializeField] private Sprite blueIcon;
    [SerializeField] private Sprite redIcon;
    // public float Whiteness = 0.1f;
    
    private readonly bool[,] _higlights = new bool[3, 3];
    private readonly PlayerColors[,] _permanentColors = new PlayerColors[3, 3];
    
    private readonly Transform[,] _pieces = new Transform[3, 3];
    private PlayerColors[,] _boardColors = new PlayerColors[3, 3];
    public PlayerColors[,] BoardColors => _boardColors;

    private Vector2Int LastBlueCoords => _lastBlueCoords;
    private Vector2Int _lastBlueCoords;
    private Vector2Int LastRedCoords => _lastRedCoords;
    private Vector2Int _lastRedCoords;
    
    private bool _ending;
    private int _roundTurns;

    private Manager _manager;
    
    private PlayerColors PlayerTurn => _roundTurns % 2 == 1 ? PlayerColors.Blue : PlayerColors.Red;

    private void Awake()
    {
        _manager = Manager.Main;
        _ending = false;
    }

    public void StartNewRound()
    {
        if (_ending) return;
        
        IncrementTurn(); // <- only because game always ends on loser's turn, we increment to make it the winner's turn
        CleanBoard();
        _boardColors = (PlayerColors[,])_permanentColors.Clone();
    }

    private void CleanBoard()
    {
        for (int x = 0; x < _boardColors.GetLength(0); x++)
        {
            for (int y = 0; y < _boardColors.GetLength(1); y++)
            {
                _boardColors[x, y] = PlayerColors.None;
                Transform piece = _pieces[x, y];
                
                if (piece != null)
                {
                    if (_permanentColors[x, y] != PlayerColors.None) continue;
                    
                    _pieces[x, y] = null;
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
        if (PlayerTurn == PlayerColors.Blue)
        {
            SpawnShapeOnSpace(bluePrefab, space);
            _lastBlueCoords = space.Coords;
        }
        else
        {
            SpawnShapeOnSpace(redPrefab, space);
            _lastRedCoords = space.Coords;
        }

        // turn ends
        _boardColors[space.Coords.x, space.Coords.y] = PlayerTurn;
        IncrementTurn();
        
        if (CheckForEnd())
        {
            _ending = true;
            StartCoroutine(WinRoutine());
        }
    }

    private void SpawnShapeOnSpace(GameObject prefab, SpaceScript space)
    {
        Vector3 verticalOffset = Vector3.up * verticalSpawnDistance;
        
        Transform shapeTransform = Instantiate(prefab).transform; // spawn, get transform
        shapeTransform.position = space.transform.position + verticalOffset; // set up in the air
        shapeTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        
        Vector2Int coords = space.Coords;
        _pieces[coords.x, coords.y] = shapeTransform;
        
        Material material = shapeTransform.GetComponent<MeshRenderer>().material;
        material.color = PlayerTurn == PlayerColors.Blue ? blueColor : redColor;
    }


    #region win/end check

    enum EndState // refactor CheckForEnd to use this
    {
        Continue,
        BlueWin,
        RedWin,
        Draw
    }
    private bool CheckForEnd()
    {
        int filled = 0;
        // Debug.Log("! STARTED WIN CHECK");
        // algorithm checks for every space, then for every space around it. if space is filled, then check opposite.
        for (int x = 0; x < _boardColors.GetLength(0); x++)
        {
            for (int y = 0; y < _boardColors.GetLength(1); y++)
            {
                
                PlayerColors spaceColor = _boardColors[x, y];
                if (spaceColor == PlayerColors.None) continue;
                filled++;
                
                if (CheckDirectionsFromSpace(new Vector2Int(x, y), spaceColor))
                    return true;
            }
        }
        return false;
    }
    private bool CheckDirectionsFromSpace(Vector2Int originCoords, PlayerColors color)
    {
        // check 3x3 grid centered on space coords as a "direction"
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
    private bool CheckInDirection(Vector2Int target, Vector2Int opposite, PlayerColors originColor)
    {
        PlayerColors targetColor = _boardColors[target.x, target.y];
        PlayerColors oppositeColor = _boardColors[opposite.x, opposite.y];

        if (targetColor != originColor) return false; // target space is not same originCoords space
        return targetColor == oppositeColor; // if target space is same as opposite space
    }
    #endregion

    private bool IsValidSpace(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0) return false;
        if (coords.x >= _boardColors.GetLength(0)) return false;
        if (coords.y >= _boardColors.GetLength(1)) return false;
        return true;
    }
    
    private void IncrementTurn()
    {
        _roundTurns++;
        shapeIcon.sprite = PlayerTurn == PlayerColors.Blue ? blueIcon : redIcon;
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

        PlayerColors color = PlayerTurn; // not last player turn because game is jank and "ahead"
        Vector2Int newPermPos = color == PlayerColors.Blue ? LastBlueCoords : LastRedCoords;
        AddNewPermanent(color, newPermPos);
        StartNewRound();
    }

    private void AddNewPermanent(PlayerColors color, Vector2Int permPos)
    {
        _permanentColors[permPos.x, permPos.y] = color;

        _higlights[permPos.x, permPos.y] = true;
        MeshRenderer meshRenderer = _pieces[permPos.x, permPos.y].GetComponent<MeshRenderer>();
        meshRenderer.material = permanentMaterial;
        Color actualColor = color == PlayerColors.Blue ? blueColor : redColor;
        meshRenderer.material.color = actualColor;
        meshRenderer.material.SetColor("_EmissionColor", actualColor);
    }
}

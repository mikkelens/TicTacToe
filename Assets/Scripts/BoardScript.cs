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
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private Color redColor = Color.red;

    [SerializeField] private float waitAfterWin = 3f;

    [SerializeField] private Image shapeIcon;
    [SerializeField] private Sprite blueIcon;
    [SerializeField] private Sprite redIcon;

    public Material PermanentMaterial => permanentMaterial;
    [SerializeField] private Material permanentMaterial;
    // public float Whiteness = 0.1f;
    
    private readonly bool[,] _higlights = new bool[3, 3];
    private readonly PlayerColors[,] _permanentColors = new PlayerColors[3, 3];
    
    public Transform[,] Pieces => _pieces;
    private readonly Transform[,] _pieces = new Transform[3, 3];
    private PlayerColors[,] _boardColors = new PlayerColors[3, 3];
    public PlayerColors[,] BoardColors => _boardColors;
    
    public Vector2Int LastBlueCoords => _lastBlueCoords;
    private Vector2Int _lastBlueCoords;
    public Vector2Int LastRedCoords => _lastRedCoords;
    private Vector2Int _lastRedCoords;

    public bool Ending => _ending;
    private bool _ending;
    
    private int _roundTurns;
    public PlayerColors PlayerTurn => _roundTurns % 2 == 1 ? PlayerColors.Blue : PlayerColors.Red;

    private Manager _manager;

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
                // Debug.Log($"X: {x}, Y: {y}, LENGTH 0: {_boardColors.GetLength(0)}, LENGTH 1: {_boardColors.GetLength(1)}");
                _boardColors[x, y] = PlayerColors.None;
                Transform piece = _pieces[x, y];
                
                if (piece != null)
                {
                    if (_permanentColors[x, y] == PlayerColors.None) // a piece and not permanent
                    {
                        Destroy(piece.gameObject);
                    }
                }
            }
        }
    }

    // "Game" is defined as a tic tac toe game within the metagame (multiple "games"),
    // Games are run by player interaction directly (each turn).
    // "Turn" is a player turn

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
        
        if (CheckForWin())
        {
            _ending = true;
            Debug.Log("A player has won this round.");
            StartCoroutine(WinState());
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

    private IEnumerator WinState()
    {
        yield return new WaitForSeconds(waitAfterWin);
        _ending = false;
        _manager.EndRound();
    }

    #region win check
    private bool CheckForWin()
    {
        // Debug.Log("! STARTED WIN CHECK");
        // algorithm checks for every space, then for every space around it. if space is filled, then check opposite.
        for (int x = 0; x < _boardColors.GetLength(0); x++)
        {
            for (int y = 0; y < _boardColors.GetLength(1); y++)
            {
                PlayerColors spaceColor = _boardColors[x, y];
                if (spaceColor == PlayerColors.None) continue;
                
                if (CheckDirectionsFromSpace(new Vector2Int(x, y), spaceColor))
                    return true;
            }
        }
        return false;
    }
    private bool CheckDirectionsFromSpace(Vector2Int originCoords, PlayerColors color)
    {
        // Debug.Log($"Started check on space: {originCoords}");
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

    public void AddNewPermanent(PlayerColors color, Vector2Int permPos)
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

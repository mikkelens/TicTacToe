using UnityEngine;

public enum PlayerColors
{
    None,
    Blue,
    Red
}

public class Manager : MonoBehaviour
{
    public static Manager Main;
    
    [Header("Settings")]
    [SerializeField] private BoardScript board;
    [SerializeField] private Transform piecesParentParent;
    public Transform PiecesParent => piecesParentParent;
    
    // assigned in script just shown for convenience
    private Camera _cam;

    private Manager() { }

    private void Awake()
    {
        if (piecesParentParent == null) piecesParentParent = transform;
        Main = this; // assuming there is only one
        _cam = Camera.main;
    }

    private void Start()
    {
        RestartGame();
    }

    // potentially called by button?, but always called on scene load
    public void RestartGame()
    {
        // Debug.Log("Started a new game.");
        board.StartNewRound(); // start round with empty board
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastForSpaceOnMouse();
        }
    }

    private void RaycastForSpaceOnMouse()
    {
        // raycast on space, call spacescript.pressspace with board
        int mask = LayerMask.GetMask("Space");
        if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, mask))
        {
            SpaceScript space = hit.transform.GetComponent<SpaceScript>();
            if (space != null)
            {
                space.SpacePressed(board);
            }
        }
    }
}
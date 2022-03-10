using UnityEngine;
using UnityEngine.SceneManagement;

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
    private void RestartGame()
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            // jank but eh lmao
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void RaycastForSpaceOnMouse()
    {
        // raycast on space, call spacescript.pressspace with board
        const string layer = "Space";

        int mask = LayerMask.GetMask(layer);
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
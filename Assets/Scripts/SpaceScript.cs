using UnityEngine;

public class SpaceScript : MonoBehaviour
{
    // this has to be set in the inspector!
    [SerializeField] private Vector2Int placement = Vector2Int.zero;
    public Vector2Int Coords => placement + new Vector2Int(1, 1);

    // called by raycast
    public void SpacePressed(BoardScript board)
    {
        if (board.Pieces[Coords.x, Coords.y].ColorType != PlayerColor.None) return;

        // Debug.Log($"Pressed space called {name}");
        board.PlacedShape(this);
    }
}

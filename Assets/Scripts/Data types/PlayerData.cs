using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerData", fileName = "PlayerData example")]
class PlayerData : ScriptableObject
{
    public GameObject prefab;
    public Color color;
    public Sprite icon;

    public Vector2Int LastPlacement { get; set; }
}

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ShapeSettings", fileName = "ShapeSettings 1")]
class PlayerShapeInfo : ScriptableObject
{
    public GameObject prefab;
    public Color color;
    public Color permColor;
    public Color outline;
    public Sprite icon;
    public PlayerType type;
    public SpaceData SpaceDataLastSpawnedOn { get; set; }
}

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ShapeSettings", fileName = "ShapeSettings 1")]
class PlayerShapeInfo : ScriptableObject
{
    public GameObject prefab;
    public Color color;
    public Sprite icon;

    public SpaceData SpaceDataLastSpawnedOn { get; set; }
}

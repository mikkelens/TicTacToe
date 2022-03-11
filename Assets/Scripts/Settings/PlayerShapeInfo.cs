using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ShapeSettings", fileName = "ShapeSettings 1")]
public class PlayerShapeInfo : ScriptableObject
{
    [Required] public PlayerType type;
    [Required] public GameObject prefab;
    [Required] public Color color;
    [Required] public Color permColor;
    [Required] public Color outline;
    [Required] [ColorUsage(false, true)] public Color emission;
    [Required] public Sprite icon;
    [Required] public AudioClip landSfx;
    public SpaceData SpaceDataLastSpawnedOn { get; set; }
}

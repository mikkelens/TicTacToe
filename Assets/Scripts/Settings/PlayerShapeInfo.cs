using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ShapeSettings", fileName = "ShapeSettings 1")]
public class PlayerShapeInfo : ScriptableObject
{
    [Required] public PlayerType type;
    [Required] public GameObject prefab;
    [Required] public Color normalColor;
    [Required] [ColorUsage(false, true)] public Color normalEmission;
    [Required] public Color permColor;
    [Required] [ColorUsage(false, true)] public Color permEmission;
    [Required] public Color permOutline;
    [Required] public Sprite icon;
    [Required] public AudioClip landSfx;
    public SpaceData LastSpacePlacedOn { get; set; }
}

using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script is used on each board, and a new board is spawned every round.
/// </summary>
public partial class BoardScript : MonoBehaviour
{
    [Header("Settings")] // assigned in prefab
    [SerializeField] private float verticalSpawnDistance = 6f;
    [SerializeField] private float verticalSpawnVelocity = -5f;
    [SerializeField] private float winWait = 1.5f;
    [SerializeField] private float metaWinWait = 0.75f;
    [SerializeField] private float metaWinSpawnVelocity = -5f;

    [Header("Player shape references")]
    [SerializeField, Required] private PlayerShapeInfo bluePlayerShapeInfo;
    [SerializeField, Required] private PlayerShapeInfo redPlayerShapeInfo;

    [Header("Visual options")]
    [SerializeField] private Material permanentMaterial;
    [SerializeField] private Image shapeIcon;

    [Header("Land SFX references")]
    [SerializeField] private AudioClip roundWinSfx;
    [SerializeField] private AudioClip permanentWinSfx;
    [SerializeField] private AudioClip drawSfx;
    [SerializeField] private AudioClip permanentDrawSfx;
    
    private Manager _manager;
    
    private bool _ending; // game is ending
    private int _turnCount; // amount of turns in game (only resets on scene reload)
    private EndData _endData;
    
    [ShowInInspector]
    private PlayerShapeInfo Current => _turnCount % 2 == 0 ? bluePlayerShapeInfo : redPlayerShapeInfo;

    // board can only be 3x3 grid without breaking
    public static readonly SpaceData[,] Spaces = new SpaceData[3, 3];
    private static Vector2Int Lenghts { get; set; }

    private void Awake()
    {
        _manager = Manager.Main;
        Lenghts = new Vector2Int(Spaces.GetLength(0), Spaces.GetLength(1));
        _endData = new EndData { State = EndState.Playing };
        _ending = false;
        BoardUtilities.CreateBoard();
    }

    /// <summary>
    /// Call this to start a new round.
    /// </summary>
    public void StartNewRound()
    {
        if (_ending) return;

        BoardUtilities.CleanBoard();
    }

    private void EndTurn()
    {
        _endData = UniversalEndCheck();
        EndState endState = _endData.State;
        
        IncrementTurn();
        if (endState == EndState.Playing) return; // let game play out untill end
        _ending = true;

        Debug.Log($"Round or game end detected: Endstate: {endState}");
        
        AudioClip sfx = endState switch
        {
            EndState.WonPermanently => permanentWinSfx,
            EndState.Won => roundWinSfx,
            EndState.DrawPermanently => permanentDrawSfx,
            EndState.Draw => drawSfx,
            _ => null
        };
        if (sfx != null)
        {
            Current.LastSpacePlacedOn.CurrentPieceData.LandSfx = sfx;
        }
        
        if (endState == EndState.DrawPermanently || endState == EndState.WonPermanently) return; // game is finished
        
        Debug.Log("Starting new round.");
        StartCoroutine(WaitForNewRound());
    }

    private IEnumerator WaitForNewRound()
    {
        float waitTime = _endData.State == EndState.WonPermanently || _endData.State == EndState.DrawPermanently ? metaWinWait : winWait;
        yield return new WaitForSeconds(waitTime);
        _ending = false;

        // new round
        SetupNewRound();
    }

    private void SetupNewRound()
    {
        AddPermanentOnSpace(Current.LastSpacePlacedOn);
        IncrementTurn();
        StartNewRound();
    }

    private void IncrementTurn()
    {
        _turnCount++;
        shapeIcon.sprite = Current.icon;
    }
    
    private void AddPermanentOnSpace(SpaceData spaceData)
    {
        // check if piece exists
        PieceData pieceData = spaceData?.CurrentPieceData;
        if (pieceData == null) return;

        // if pieceData exists (and can be made permanent)
        MakePiecePermanent(pieceData);
    }

    private void MakePiecePermanent(PieceData pieceData)
    {
        Transform pieceTransform = pieceData.PTransform;

        pieceData.IsPermanent = true;

        // make pieceData shine
        MeshRenderer meshRenderer = pieceTransform.GetComponent<MeshRenderer>();
        SetMrPermanent(meshRenderer);
    }

    private void SetMrPermanent(Renderer meshRenderer)
    {
        meshRenderer.material = permanentMaterial;
        meshRenderer.material.SetColor("_Color", Current.permColor);
        meshRenderer.material.SetColor("_OutlineColor", Current.permOutline);
        meshRenderer.material.SetColor("_EmissionColor", Current.permEmission);
    }
    private void SetMrStandard(Renderer meshRenderer)
    {
        meshRenderer.material.color = Current.normalColor;
        meshRenderer.material.SetColor("_EmissionColor", Current.normalEmission);
    }
}
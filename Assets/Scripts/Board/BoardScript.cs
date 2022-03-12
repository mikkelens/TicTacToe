using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script is used on each board, and a new board is spawned every round.
/// </summary>
public partial class BoardScript : MonoBehaviour
{
    private Manager _manager;

    [Header("Settings")] // assigned in prefab
    // [SerializeField] private float horizontalSpawnDistance = 4f;
    [SerializeField] private float verticalSpawnDistance = 6f;
    [SerializeField] private float verticalSpawnVelocity = -5f;
    [SerializeField] private float winWait = 1.5f;
    [SerializeField] private float metaWinWait = 0.75f;
    [SerializeField] private float metaWinSpawnVelocity = -5f;

    [SerializeField, Required] private PlayerShapeInfo bluePlayerShapeInfo;
    [SerializeField, Required] private PlayerShapeInfo redPlayerShapeInfo;

    [SerializeField] private Material permanentMaterial;

    [SerializeField] private Image shapeIcon;

    [SerializeField] private AudioClip winLandSfx;
    [SerializeField] private AudioClip drawLandSfx;

    private static bool _ending;
    private static int _roundTurns;
    private bool _metaWinAchieved;

    private PlayerShapeInfo Current => _roundTurns % 2 == 0 ? B : R;
    private PlayerShapeInfo B => bluePlayerShapeInfo;
    private PlayerShapeInfo R => redPlayerShapeInfo;

    // board can only be 3x3 grid without breaking
    public static readonly SpaceData[,] Spaces = new SpaceData[3, 3];
    private Vector2Int _lenghts;
    private Vector2Int Lenghts => _lenghts;

    private BoardScript() { }

    private void Awake()
    {
        _lenghts = new Vector2Int(Spaces.GetLength(0), Spaces.GetLength(1));
        LineConstructor.Spaces = Spaces;
        LineConstructor.Lenghts = _lenghts;
        SpaceChecks.CurrentPlayer = Current;
        SpaceChecks.Spaces = Spaces;
        SpaceChecks.Lenghts = _lenghts;
        CreateBoard();
        _manager = Manager.Main;
        _metaWinAchieved = false;
        _ending = false;

        void CreateBoard()
        {
            for (int x = 0; x < Lenghts.x; x++)
            {
                for (int y = 0; y < Lenghts.y; y++)
                {
                    Spaces[x, y] = new SpaceData();
                }
            }
        }
    }

    /// <summary>
    /// Call this to start a new round.
    /// </summary>
    public void StartNewRound()
    {
        if (_ending) return;

        CleanBoard();
    }

    private void MetaWin(SpaceData winSpot)
    {
        // this will be repeatedly called bc autowin below

        PlaceShape(winSpot);
        _metaWinAchieved = true;
    }

    private void CleanBoard()
    {
        for (int x = 0; x < Lenghts.x; x++)
        {
            for (int y = 0; y < Lenghts.y; y++)
            {
                SpaceData spaceData = Spaces[x, y];
                ref PieceData pieceData = ref spaceData.CurrentPieceData;

                if (pieceData == null) continue;

                // piece exists

                if (pieceData.IsPermanent) continue;

                // piece is not permanent

                Destroy(pieceData.PTransform.gameObject);
                pieceData = null;
                spaceData.Script.CanPlayAudio = true;
            }
        }
    }

    private void EndTurn()
    {
    }

    private void IncrementTurn()
    {
        _roundTurns++;
        shapeIcon.sprite = Current.icon;
    }

    private IEnumerator WinRoutine()
    {
        float waitTime = _metaWinAchieved ? metaWinWait : winWait;
        yield return new WaitForSeconds(waitTime);
        _ending = false;
        EndRound();
    }

    private void EndRound()
    {
        if (_ending) return;

        TryAddPermanent(Current.SpaceDataLastSpawnedOn);
        IncrementTurn();
        StartNewRound();
    }

    private void TryAddPermanent(SpaceData spaceData)
    {
        if (spaceData == null) return;

        // if space exists

        PieceData pieceData = spaceData.CurrentPieceData;
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

    void SetMrPermanent(MeshRenderer meshRenderer)
    {
        meshRenderer.material = permanentMaterial;
        meshRenderer.material.SetColor("_Color", Current.permColor);
        meshRenderer.material.SetColor("_OutlineColor", Current.permOutline);
        meshRenderer.material.SetColor("_EmissionColor", Current.permEmission);
    }

    void SetMrStandard(MeshRenderer meshRenderer)
    {
        meshRenderer.material.color = Current.normalColor;
        meshRenderer.material.SetColor("_EmissionColor", Current.normalEmission);
    }
}
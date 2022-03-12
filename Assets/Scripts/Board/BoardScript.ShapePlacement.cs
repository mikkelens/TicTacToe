using UnityEngine;

public partial class BoardScript
{
    /// <summary>
    /// Called by a spaceData that got pressed. Calling this switches turn.
    /// </summary>
    /// <param name="spaceData"></param>
    public void PlaceShape(SpaceData spaceData)
    {
        if (_ending) return;

        PlayerShapeInfo playerShapeInfo = Current; // get correct shape

        // spawn pieceData
        PieceData placedPieceData = SpawnShapeOnSpace(playerShapeInfo.prefab, spaceData); // spawn it
        placedPieceData.Info = Current;
        placedPieceData.Type = Current.type;
        spaceData.CurrentPieceData = placedPieceData;

        playerShapeInfo.LastSpacePlacedOn = spaceData; // store last used spaceData

        // turn ends
        CheckIfEnd();
    }
    /// <summary>
    /// Physically spawns a shape in the world on the spaceData provided.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="spaceData"></param>
    private PieceData SpawnShapeOnSpace(GameObject prefab, SpaceData spaceData)
    {
        PieceData newPieceData = new PieceData(); // to store pieceData information

        Vector2Int coords = spaceData.Coords;
        Spaces[coords.x, coords.y].CurrentPieceData = newPieceData;

        Vector3 spawnOffset = Vector3.up * verticalSpawnDistance;
        Transform pTransform = Instantiate(prefab).transform; // spawn, get transform
        pTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        pTransform.position = spaceData.PhysicalSpaceTransform.position + spawnOffset; // set up in the air
        newPieceData.PTransform = pTransform;


        Rigidbody rb = pTransform.GetComponent<Rigidbody>();
        float spawnVelocity = _metaWinAchieved ? metaWinSpawnVelocity : verticalSpawnVelocity;
        rb.velocity = new Vector3(0f, spawnVelocity, 0f);
        newPieceData.Rb = rb;

        MeshRenderer meshRenderer = pTransform.GetComponent<MeshRenderer>();
        if (_metaWinAchieved)
        {
            SetMrPermanent(meshRenderer);
        }
        else
        {
            SetMrStandard(meshRenderer);
        }

        return newPieceData;
    }
}
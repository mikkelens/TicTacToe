using UnityEngine;

public partial class BoardScript
{
    /// <summary>
    /// Called by a spaceData that got pressed. Calling this switches turn.
    /// </summary>
    /// <param name="spaceData"></param>
    public void PlayerPlaceShape(SpaceData spaceData)
    {
        if (_ending) return;

        PlayerInfo playerInfo = Current; // get correct shape

        // spawn pieceData
        PieceData placedPieceData = SpawnShapeOnSpace(playerInfo.prefab, spaceData); // spawn it
        // give relevant information
        placedPieceData.Info = Current;
        placedPieceData.Type = Current.type;
        // store it to the space
        spaceData.CurrentPieceData = placedPieceData;

        playerInfo.LastSpacePlacedOn = spaceData; // store last used spaceData

        // turn ends
        EndTurn();
    }
    /// <summary>
    /// Physically spawns a shape in the world on the spaceData provided (called by the space when a player presses on it).
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="spaceData"></param>
    private PieceData SpawnShapeOnSpace(GameObject prefab, SpaceData spaceData)
    {
        PieceData newPieceData = new PieceData(); // to store pieceData information

        Vector3 spawnOffset = Vector3.up * verticalSpawnDistance;
        Transform pTransform = Instantiate(prefab).transform; // spawn, get transform
        pTransform.parent = _manager.PiecesParent; // set in a parent (for editor convenience)
        pTransform.position = spaceData.PhysicalSpaceTransform.position + spawnOffset; // set up in the air
        newPieceData.PTransform = pTransform;

        Rigidbody rb = pTransform.GetComponent<Rigidbody>();
        EndState endState = _endData.State;
        float spawnVelocity = endState switch
        {
            EndState.WonPermanently => metaWinSpawnVelocity,
            EndState.Playing => verticalSpawnVelocity,
            _ => verticalSpawnVelocity
        };
        rb.velocity = new Vector3(0f, spawnVelocity, 0f);
        newPieceData.Rb = rb;

        MeshRenderer meshRenderer = pTransform.GetComponent<MeshRenderer>();
        if (newPieceData.IsPermanent)
        {
            SetPermanentMaterial(meshRenderer);
        }
        else
        {
            SetStandardMaterial(meshRenderer);
        }

        return newPieceData;
    }
}
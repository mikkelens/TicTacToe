using UnityEngine;

public partial class BoardScript
{
    public static class BoardUtilities
    {
        public static void CreateBoard()
        {
            for (int x = 0; x < Lenghts.x; x++)
            {
                for (int y = 0; y < Lenghts.y; y++)
                {
                    Spaces[x, y] = new SpaceData();
                }
            }
        }
        
        public static void CleanBoard()
        {
            for (int x = 0; x < Lenghts.x; x++)
            {
                for (int y = 0; y < Lenghts.y; y++)
                {
                    SpaceData spaceData = Spaces[x, y];
                    PieceData pieceData = spaceData.CurrentPieceData;

                    if (pieceData == null) continue;
                    // piece exists

                    if (pieceData.IsPermanent) continue;
                    // piece is not permanent
                    
                    Destroy(pieceData.PTransform.gameObject);
                    spaceData.Script.CanPlayAudio = true;
                    spaceData.CurrentPieceData = null;
                }
            }
        }
    }
}
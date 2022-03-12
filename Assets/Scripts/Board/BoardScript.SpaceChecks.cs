using System.Collections.Generic;
using UnityEngine;

public partial class BoardScript
{
    private static class SpaceChecks
    {
        public static SpaceData[,] Spaces
        {
            get => _spaces;
            set => _spaces = value;
        }
        private static SpaceData[,] _spaces;
        public static Vector2Int Lenghts
        {
            get => _lenghts;
            set => _lenghts = value;
        }
        private static Vector2Int _lenghts;

        private static int TotalSpaceCount => Lenghts.x * Lenghts.y;

        private static bool IsValidSpace(Vector2Int coords)
        {
            if (coords.x < 0 || coords.y < 0) return false;
            if (coords.x >= Lenghts.x) return false;
            if (coords.y >= Lenghts.y) return false;
            return true;
        }

        public static EndState UniversalWinCheck(bool metaWin = false)
        {
            SpaceData[][] allLines = LineConstructor.GetAllLines();
            int[] allColorInstances = new int[2];
            foreach (SpaceData[] line in allLines)
            foreach (var space in line)
            {
                PieceData piece = space.CurrentPieceData;
                if (piece == null) continue;

                if (piece.Type == PlayerType.Blue) blues++;
                else if (piece.Type == PlayerType.Red) reds++;
            }

            if (metaWin)
            {
                foreach (int instancesOfColor in allColorInstances)
                {
                    
                }
                if (blues >= _lenghts.x - 1 || reds >= _lenghts.x)
                    return EndState.Win;
            }
            else
            {
                int color;
                if (blues >= _lenghts.x || reds >= _lenghts.x)
                    return EndState.Win;
            }
            
            

            return EndState.Continue;
        }

        /// <summary>
        /// Checks if game ends.
        /// </summary>
        /// <returns>The end state that should be used.</returns>
        // public static EndState CheckForWin()
        // {
        //     // algorithm checks for every spaceData
        //     int spacesFilled = 0;
        //     for (int x = 0; x < Lenghts.x; x++)
        //     for (int y = 0; y < Lenghts.y; y++)
        //     {
        //         SpaceData spaceData = Spaces[x, y];
        //         PieceData pieceData = spaceData.CurrentPieceData;
        //         if (pieceData == null) continue;
        //
        //         // if pieceData exists
        //
        //         spacesFilled++;
        //
        //         // check for win
        //         if (CheckDirectionsFromSpace(spaceData))
        //             return EndState.Win;
        //     }
        //
        //     // check for draw
        //     if (spacesFilled == TotalSpaceCount)
        //         return EndState.Draw;
        //
        //     // if nothing indicating game should end, it continues (and players can continue placing)
        //     return EndState.Continue;
        //
        // }

        private static bool CheckDirectionsFromSpace(SpaceData origin)
        {
            // check for every spaceData the originSpaceData spaceData.
            // Think of it as a 3x3 grid (with no middle), centered on originSpaceData coords, with each outer check as a "direction"
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                // x and y are treated as direction vector components
                if (x == 0 && y == 0) continue; // no direction is not a direction

                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int target = origin.Coords + offset;
                Vector2Int opposite = origin.Coords - offset;

                if (!IsValidSpace(target)) continue;
                if (!IsValidSpace(opposite)) continue;

                // a line of spaces exist

                // we already know spaces exist at coordinates so get them without extra checks
                PieceData targetPiece = Spaces[target.x, target.y].CurrentPieceData;
                PieceData oppositePiece = Spaces[opposite.x, opposite.y].CurrentPieceData;
                if (targetPiece == null || oppositePiece == null) continue;

                // if both target and opposite piece exist

                if (CheckPieceSimilarity(targetPiece, oppositePiece, origin.CurrentPieceData.Type))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns true if target spaceData and opposite spaceData is the same color
        /// </summary>
        /// <param name="target"></param>
        /// <param name="opposite"></param>
        /// <param name="originType"></param>
        /// <returns></returns>
        private static bool CheckPieceSimilarity(PieceData target, PieceData opposite, PlayerType originType)
        {
            PlayerType targetType = target.Type;
            PlayerType oppositeType = opposite.Type;
        
            if (targetType != originType) return false; // target spaceData is not same originCoords spaceData
            return targetType == oppositeType; // if target spaceData is same as opposite spaceData
        }

        private static (PieceData, Vector2Int)[] GetSurroundingAlikePieces(SpaceData originSpaceData, bool permanentOnly)
        {
            PieceData originPieceData = originSpaceData.CurrentPieceData;
            List<(PieceData, Vector2Int)> surroundingPieces = new List<(PieceData, Vector2Int)>();
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int offset = new Vector2Int(x, y);
                Vector2Int target = originSpaceData.Coords + offset;

                if (!IsValidSpace(target)) continue;

                // if spaceData *can* exist

                SpaceData spaceData = Spaces[target.x, target.y];
                if (spaceData == null) continue;
                PieceData pieceData = spaceData.CurrentPieceData;
                if (pieceData == null) continue;

                // if piece exists

                if (permanentOnly && !pieceData.IsPermanent) continue;
                if (pieceData.Type != originPieceData.Type) continue;

                // if spaceData is one we are looking for

                // add it to the list of surrounding pieces
                surroundingPieces.Add((pieceData, offset));
            }
            return surroundingPieces.ToArray();
        }
        
        #region Meta
        /// <summary>
        /// Checks all permanent spaces if they have a situation where player has won infinitely
        /// </summary>
        /// <returns></returns>
        public static (EndState state, SpaceData winSpot) InfiniteWin(PlayerShapeInfo player)
        {
            int spacesFilled = 0;

            for (int x = 0; x < Lenghts.x; x++)
            for (int y = 0; y < Lenghts.y; y++)
            {
                SpaceData originSpaceData = Spaces[x, y];
                PieceData originPieceData = originSpaceData.CurrentPieceData;

                if (originPieceData == null || !originPieceData.IsPermanent) continue;

                // if pieceData is permanent

                spacesFilled++;

                if (originPieceData.Type != player.type) continue;

                // if player can place immediately (needed for instant infinite win)

                // check if surrounding pieces have spaceData and that spaceData is same color type
                foreach ((PieceData data, Vector2Int offset) nextPermanent in GetSurroundingAlikePieces(originSpaceData, true))
                {
                    if (nextPermanent.data.Type != player.type) continue;

                    // try to access the opposite spaceData of the one under nextPermanent
                    Vector2Int oppositePos = originSpaceData.Coords - nextPermanent.offset;
                    if (!IsValidSpace(oppositePos)) continue;

                    // if space exists

                    SpaceData oppositeSpaceData = Spaces[oppositePos.x, oppositePos.y];
                    PieceData oppositePieceData = oppositeSpaceData.CurrentPieceData;
                    if (oppositePieceData != null) continue;

                    // if there is space for a piece to be put

                    return (EndState.Win, oppositeSpaceData); // detected that a player has "infinitely" won.
                }
            }
            if (spacesFilled == TotalSpaceCount) return (EndState.Draw, null);

            return (EndState.Continue, null);
        }
        #endregion
    }
}
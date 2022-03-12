﻿using System.Collections.Generic;
using UnityEngine;

public partial class BoardScript
{
    private static class SpaceChecks
    {
        public static PlayerShapeInfo CurrentPlayer { get; set; }

        private static int TotalSpaceCount => Lenghts.x * Lenghts.y;

        private static bool IsValidSpace(Vector2Int coords)
        {
            if (coords.x < 0 || coords.y < 0) return false;
            if (coords.x >= Lenghts.x) return false;
            if (coords.y >= Lenghts.y) return false;
            return true;
        }

        /// <summary>
        /// Should be called at the end of every turn or placement. Will figure out what should happen.
        /// </summary>
        /// <returns></returns>
        public static EndState UniversalWinCheck()
        {
            List<PieceData> allPieces = new List<PieceData>();

            SpaceData[][] allLines = LineConstructor.GetAllLines();
            foreach (SpaceData[] line in allLines)
            {
                int redsInLine = 0;
                int bluesInLine = 0;
                foreach (SpaceData space in line)
                {
                    PieceData piece = space.CurrentPieceData;
                    if (piece == null) continue;

                    if (piece.Type == PlayerType.Blue) bluesInLine++;
                    else if (piece.Type == PlayerType.Red) redsInLine++;

                    if (!allPieces.Contains(piece)) allPieces.Add(piece);
                }
                if (redsInLine >= 3 || bluesInLine >= 3) return EndState.Win;

                if (redsInLine + bluesInLine >= 3) continue; // continue search if line is blocked

                // the count relevant for the next player
                // (not for the current player: currentPlayer is updated after this method)
                int currentPlayerOccurances = CurrentPlayer.type != PlayerType.Blue ? bluesInLine : redsInLine;
                
                if (currentPlayerOccurances == 2) return EndState.InfiniteWin;
            }

            // draws if board is filled
            if (allPieces.Count == TotalSpaceCount) return EndState.Draw;
            // infinite draws if all the spaces except 1 empty space are all permament pieces
            if (allPieces.Count == TotalSpaceCount)
            {
                foreach (PieceData piece in allPieces)
                {
                    if (!piece.IsPermanent) break;
                }
                return EndState.InfiniteDraw;
            }
            
            return EndState.ContinueRound;
        }

        /// <summary>
        /// Checks if game ends.
        /// </summary>
        /// <returns>The end state that should be used.</returns>
        public static EndState CheckForWin()
        {
            // algorithm checks for every spaceData
            int spacesFilled = 0;
            for (int x = 0; x < Lenghts.x; x++)
            for (int y = 0; y < Lenghts.y; y++)
            {
                SpaceData spaceData = Spaces[x, y];
                PieceData pieceData = spaceData.CurrentPieceData;
                if (pieceData == null) continue;
        
                // if pieceData exists
        
                spacesFilled++;
        
                // check for win
                if (CheckDirectionsFromSpace(spaceData))
                    return EndState.Win;
            }
        
            // check for draw
            if (spacesFilled == TotalSpaceCount)
                return EndState.Draw;
        
            // if nothing indicating game should end, it continues (and players can continue placing)
            return EndState.ContinueRound;
        
        }

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

        // private static (PieceData, Vector2Int)[] GetSurroundingAlikePieces(SpaceData originSpaceData, bool permanentOnly)
        // {
        //     PieceData originPieceData = originSpaceData.CurrentPieceData;
        //     List<(PieceData, Vector2Int)> surroundingPieces = new List<(PieceData, Vector2Int)>();
        //     for (int x = -1; x <= 1; x++)
        //     for (int y = -1; y <= 1; y++)
        //     {
        //         if (x == 0 && y == 0) continue;
        //
        //         Vector2Int offset = new Vector2Int(x, y);
        //         Vector2Int target = originSpaceData.Coords + offset;
        //
        //         if (!IsValidSpace(target)) continue;
        //
        //         // if spaceData *can* exist
        //
        //         SpaceData spaceData = Spaces[target.x, target.y];
        //         if (spaceData == null) continue;
        //         PieceData pieceData = spaceData.CurrentPieceData;
        //         if (pieceData == null) continue;
        //
        //         // if piece exists
        //
        //         if (permanentOnly && !pieceData.IsPermanent) continue;
        //         if (pieceData.Type != originPieceData.Type) continue;
        //         // if spaceData is one we are looking for
        //
        //         // add it to the list of surrounding pieces
        //         surroundingPieces.Add((pieceData, offset));
        //     }
        //     return surroundingPieces.ToArray();
        // }
        
        // #region Meta
        // /// <summary>
        // /// Checks all permanent spaces if they have a situation where player has won infinitely
        // /// </summary>
        // /// <returns></returns>
        // public static (EndState state, SpaceData winSpot) InfiniteWin(PlayerShapeInfo player)
        // {
        //     int spacesFilled = 0;
        //
        //     for (int x = 0; x < Lenghts.x; x++)
        //     for (int y = 0; y < Lenghts.y; y++)
        //     {
        //         SpaceData originSpaceData = Spaces[x, y];
        //         PieceData originPieceData = originSpaceData.CurrentPieceData;
        //
        //         if (originPieceData == null || !originPieceData.IsPermanent) continue;
        //
        //         // if pieceData is permanent
        //
        //         spacesFilled++;
        //
        //         if (originPieceData.Type != player.type) continue;
        //
        //         // if player can place immediately (needed for instant infinite win)
        //
        //         // check if surrounding pieces have spaceData and that spaceData is same color type
        //         foreach ((PieceData data, Vector2Int offset) nextPermanent in GetSurroundingAlikePieces(originSpaceData, true))
        //         {
        //             if (nextPermanent.data.Type != player.type) continue;
        //
        //             // try to access the opposite spaceData of the one under nextPermanent
        //             Vector2Int oppositePos = originSpaceData.Coords - nextPermanent.offset;
        //             if (!IsValidSpace(oppositePos)) continue;
        //
        //             // if space exists
        //
        //             SpaceData oppositeSpaceData = Spaces[oppositePos.x, oppositePos.y];
        //             PieceData oppositePieceData = oppositeSpaceData.CurrentPieceData;
        //             if (oppositePieceData != null) continue;
        //
        //             // if there is space for a piece to be put
        //
        //             return (EndState.Win, oppositeSpaceData); // detected that a player has "infinitely" won.
        //         }
        //     }
        //     if (spacesFilled == TotalSpaceCount) return (EndState.Draw, null);
        //
        //     return (EndState.ContinueRound, null);
        // }
        // #endregion
    }
}
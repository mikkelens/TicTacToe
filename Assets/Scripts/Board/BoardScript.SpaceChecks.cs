using System.Collections.Generic;
using UnityEngine;

public partial class BoardScript
{
    private static int TotalSpaceCount => Lenghts.x * Lenghts.y;
    /// <summary>
    /// Should be called at the end of every turn or placement. Will figure out what should happen.
    /// </summary>
    /// <returns></returns>
    public EndData UniversalEndCheck()
    {
        List<PieceData> allPieces = new List<PieceData>();
        List<PieceData> allPermanentPieces = new List<PieceData>();
        SpaceData lastEmptySpace = null;
        
        SpaceData[][] allLines = LineConstructor.GetAllLines(); // only gets some amount of lines?
        foreach (SpaceData[] line in allLines)
        {
            // Debug.Log($"a: {line[0].Coords} - b: {line[1].Coords} - c: {line[2].Coords}");
            
            List<PieceData> relevantPiecesInLine = new List<PieceData>();
            List<PieceData> relevantPermanentPiecesInLine = new List<PieceData>();
            foreach (SpaceData space in line)
            {
                PieceData piece = space.CurrentPieceData;
                if (piece == null)
                {
                    lastEmptySpace = space;
                }
                else
                {
                    if (Current.type == piece.Type)
                    {
                        relevantPiecesInLine.Add(piece);
                        if (piece.IsPermanent) relevantPermanentPiecesInLine.Add(piece);
                    }
                    
                    if (allPieces.Contains(piece)) continue;
                    // the piece is new
                    
                    Debug.Log($"Piece position: {space.Coords}");
                    allPieces.Add(piece);
                    if (piece.IsPermanent) allPermanentPieces.Add(piece);
                }
            }
            // infinite win
            if (relevantPermanentPiecesInLine.Count >= 2) return new EndData { State = EndState.WonPermanently, TargetSpace = lastEmptySpace };
            // win
            if (relevantPiecesInLine.Count == 3) return new EndData { State = EndState.Won };
        }
        
        // infinite draw
        if (allPermanentPieces.Count == TotalSpaceCount - 1) return new EndData { State = EndState.DrawPermanently, TargetSpace = lastEmptySpace};
        // draw
        if (allPieces.Count == TotalSpaceCount) return new EndData { State = EndState.Draw };
        
        // if nothing else, continue
        return new EndData { State = EndState.Playing } ;
    }
}
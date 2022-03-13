using System.Collections.Generic;
using UnityEngine;

public partial class BoardScript
{
    private static class LineConstructor
    {
        public static SpaceData[][] GetAllLines()
        {
            int expectedLineCount = Lenghts.x + Lenghts.y + 2; // for each direction
            int lineIndex = 0;
            SpaceData[][] allLines = new SpaceData[expectedLineCount][];
            
            // for every space on the board
            Vector2Int origin = Vector2Int.zero; // start at (0, 0)
            for (origin.y = 0; origin.y < Lenghts.y; origin.y++)
            for (origin.x = 0; origin.x < Lenghts.x; origin.x++)
            {
                // only get lines from an edge with a zero-coordinate (optimization)
                if (origin.x > 0 && origin.y > 0) continue;
                
                // creation of one or more lines from origin
                Vector2Int[] directions = GetLineDirections(origin);
                foreach (Vector2Int direction in directions)
                {
                    SpaceData[] line = ConstructLine(origin, direction);
                    allLines[lineIndex] = line;
                    lineIndex++;
                    Debug.Log($"a: {line[0].Coords} - b: {line[1].Coords} - c: {line[2].Coords}");
                }
            }
            return allLines; // send the lines back
        }

        private static Vector2Int[] GetLineDirections(Vector2Int startPos)
        {
            List<Vector2Int> directions = new List<Vector2Int>();

            int cardinals = 0; // 1 or 2
            bool hasMadeDiagonal = false; // 0 or 1
            do
            {
                // cardinal direction
                directions.Add(startPos.x == 0 && cardinals == 0 ? Vector2Int.right : Vector2Int.up);
                cardinals++;

                // if has not made a diagonal, and diagonal direction makes sense
                if (hasMadeDiagonal || !(startPos.y == 0 && (startPos.x == 0 || startPos.x == Lenghts.x - 1))) continue;
                
                // diagonal direction
                hasMadeDiagonal = true;
                directions.Add(startPos.x == 0 ? new Vector2Int(1, 1) : new Vector2Int(-1, 1));
            } while (startPos == Vector2Int.zero && cardinals < 2); // may repeat for extra cardinals
            
            return directions.ToArray();
        }
        
        static SpaceData[] ConstructLine(Vector2Int startPos, Vector2Int direction)
        {
            SpaceData[] line = new SpaceData[Lenghts.x];
            Vector2Int pos = startPos; // starting space is set
            for (int i = 0; i < Lenghts.x; i++)
            {
                line[i] = Spaces[pos.x, pos.y]; // add space to line
                pos += direction; // go to next space
            }
            return line;
        }
    }
}
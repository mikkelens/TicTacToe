using UnityEngine;

public partial class BoardScript
{
    private static class LineConstructor
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
        public static SpaceData[][] GetAllLines()
        {
            int expectedLineCount = _lenghts.x + _lenghts.y + 2; // for each direction
            int lineCount = 0;
            int diagonalCount = 0;
            SpaceData[][] allLines = new SpaceData[expectedLineCount][];
            Vector2Int spacePos = Vector2Int.zero;
            for (spacePos.x = 0; spacePos.x < _lenghts.x; spacePos.x++)
            {
                for (spacePos.y = 0; spacePos.y < _lenghts.y; spacePos.y++)
                {
                    // we only need to search for lines from starting edge
                    if (spacePos.x > 0 && spacePos.y > 0) continue;
                    
                    allLines[lineCount] = GetLine(); // using spacePos and diagonal count.
                    lineCount++;
                }
            }
            return allLines; // send the lines back

            // gets line using ConstructLine()
            SpaceData[] GetLine()
            {
                Vector2Int direction;
                if ((spacePos.x == 0 && spacePos.y == 0) || (diagonalCount < 2 && spacePos.x == _lenghts.x - 1 && spacePos.y == 0)) // do diagonal
                {
                    // first and last space in "bottom" (x is 0) of board spaces:
                    // search for diagonal, up (y is 1) and to the left or right
                    direction = diagonalCount == 0 ? new Vector2Int(1, 1) : new Vector2Int(-1, 1);
                    // do position again after, except cardinal this time
                    spacePos.y--;
                    diagonalCount++;
                }
                else // do cardinal
                {
                    direction = spacePos.x == 0 ? Vector2Int.right : Vector2Int.up;
                }
                return ConstructLine();
                
                // constructs line for GetLine()
                SpaceData[] ConstructLine()
                {
                    SpaceData[] line = new SpaceData[Lenghts.x];
                    // start from origin
                    Vector2Int pos = spacePos;
                    // get first space
                    line[0] = _spaces[spacePos.x, spacePos.y];
                    // get remaining spaces in line
                    for (int i = 1; i < Lenghts.x; i++)
                    {
                        pos += direction;
                        line[i] = _spaces[pos.x, pos.y];
                    }
                    return line;
                }
            }
        }
    }
}
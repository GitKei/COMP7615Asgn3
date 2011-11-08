using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace COMP7615Asgn3
{
    class MazeGenerator
    {
        int[,] cells;
        Texture2D whiteTex;
        Texture2D blackTex;

        public MazeGenerator(Texture2D white, Texture2D black)
        {
            whiteTex = white;
            blackTex = black;

            cells = new int[Defs.MapWidth, Defs.MapHeight];

            GenerateMaze();
        }

        public void GenerateMaze()
        {
            Random random = new Random();

            // Wall Everything
            for (int w = 0; w < Defs.MapWidth; w++)
            {
                for (int h = 0; h < Defs.MapHeight; h++)
                {
                    cells[w, h] = 1;
                }
            }

            // Potential Wall List
            List<Vector2> cellList = new List<Vector2>();

            // Open First Cell
            cellList = Mark(1, 1, cellList);

            while (cellList.Count > 0)
            {
                // Find Random Wall
                int cell = random.Next(cellList.Count);

                // Get Position of Wall
                Vector2 nextCell = cellList[cell];

                // Remove from list
                cellList.RemoveAt(cell);

                // Check if edge
                if (nextCell.X == 0 || nextCell.X == Defs.MapWidth - 1
                 || nextCell.Y == 0 || nextCell.Y == Defs.MapHeight - 1)
                    continue;

                // Check if theres wall on the other side
                if(CheckNextCell(nextCell))
                    cellList = Mark((int)nextCell.X, (int)nextCell.Y, cellList);
            }

            // Make sure Entrance and Exit are open
            cells[0, 1] = 0;
            cells[Defs.MapWidth - 1, Defs.MapHeight - 2] = 0;
            cells[Defs.MapWidth - 2, Defs.MapHeight - 2] = 0;
        }

        private List<Vector2> Mark(int x, int y, List<Vector2> wallList)
        {
            // Remove Wall
            cells[x, y] = 0;

            // Add Potential Walls
            if (cells[x - 1, y] == 1)
                wallList.Add(new Vector2(x - 1, y));

            if (cells[x + 1, y] == 1)
                wallList.Add(new Vector2(x + 1, y));

            if (cells[x, y - 1] == 1)
                wallList.Add(new Vector2(x, y - 1));

            if (cells[x, y + 1] == 1)
                wallList.Add(new Vector2(x, y + 1));

            return wallList;
        }

        private bool CheckNextCell(Vector2 currentCell)
        {
            int paths = 0;
            int cx = (int)currentCell.X;
            int cy = (int)currentCell.Y;

            // Check Each Adjacent Cell
            if (cells[cx + 1, cy] == 0)
            {
                paths++;
            }

            if (cells[cx - 1, cy] == 0)
            {
                paths++;
            }

            if (cells[cx, cy - 1] == 0)
            {
                paths++;
            }

            if (cells[cx, cy + 1] == 0)
            {
                paths++;
            }

            if (paths <= 1)
                return true;

            return false;
        }

        public void DrawMap(SpriteBatch sb)
        {
            for (int w = 0; w < Defs.MapWidth; w++)
            {
                for (int h = 0; h < Defs.MapHeight; h++)
                {
                    if (cells[w, h] == 0)
                        sb.Draw(whiteTex, new Vector2(w * 20, h * 20), Color.White);
                    else
                        sb.Draw(blackTex, new Vector2(w * 20, h * 20), Color.White);
                }
            }
        }
    }
}

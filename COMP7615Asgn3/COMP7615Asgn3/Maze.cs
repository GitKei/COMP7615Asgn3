using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace COMP7615Asgn3
{
    class Maze
    {
        int[,] cells;
        Texture2D whiteTex;
        Texture2D blackTex;
        Texture2D redTex;
        Vector2 position;

        KeyboardState previousKey;

        public Maze(Texture2D white, Texture2D black, Texture2D red)
        {
            whiteTex = white;
            blackTex = black;
            redTex = red;

            cells = new int[Defs.MapWidth, Defs.MapHeight];

            GenerateMaze();
        }

        public void GenerateMaze()
        {
            Random random = new Random();

            position = new Vector2(0, 1);

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

            // Open Random Paths
            for (int i = 0; i < Defs.RandomPath; i++)
            {
                int x = random.Next(1, Defs.MapWidth - 2);
                int y = random.Next(1, Defs.MapHeight - 2);

                cells[x, y] = 0;
            }
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

            // Return true only if there is 1 or less adjacent paths
            if (paths <= 1)
                return true;

            return false;
        }

        public int CheckCell(Vector2 currentCell)
        {
            Random random = new Random();
            int cx = (int)currentCell.X;
            int cy = (int)currentCell.Y;

            List<int> directions = new List<int>();

            // Check Each Adjacent Cell
            if (cx + 1 < Defs.MapWidth && cells[cx + 1, cy] == 0)
                directions.Add((int)Defs.Direction.E);

            if (cx > 0 && cells[cx - 1, cy] == 0)
                directions.Add((int)Defs.Direction.W);

            if (cy > 0 && cells[cx, cy - 1] == 0)
                directions.Add((int)Defs.Direction.N);

            if (cy + 1 < Defs.MapHeight && cells[cx, cy + 1] == 0)
                directions.Add((int)Defs.Direction.S);

            return directions[random.Next(directions.Count)];
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

            sb.Draw(redTex, position * 20, Color.White);
        }

        public void Move(KeyboardState ks)
        {
            if (ks.IsKeyDown(Keys.W) && previousKey.IsKeyUp(Keys.W))
                if(position.Y > 0 && cells[(int)position.X, (int)position.Y - 1] == 0)
                    position.Y -= 1;
            if (ks.IsKeyDown(Keys.S) && previousKey.IsKeyUp(Keys.S))
                if(position.Y < Defs.MapHeight - 1 && cells[(int)position.X, (int)position.Y + 1] == 0)
                    position.Y += 1;
            if (ks.IsKeyDown(Keys.A) && previousKey.IsKeyUp(Keys.A))
                if(position.X > 0 && cells[(int)position.X - 1, (int)position.Y] == 0)
                    position.X -= 1;
            if (ks.IsKeyDown(Keys.D) && previousKey.IsKeyUp(Keys.D))
                if(position.X < Defs.MapWidth - 1 && cells[(int)position.X + 1, (int)position.Y] == 0)
                    position.X += 1;

            previousKey = ks;

            if (position.X == Defs.MapWidth - 1)
                GenerateMaze();
        }

        public int[,] Cells
        {
            get
            {
                return cells;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace COMP7615Asgn3
{
    /// <summary>
    /// This class contains the state and creation logic for the 2D Maze.
    /// </summary>
    class Maze
    {
        int[,] cells;
        Texture2D whiteTex;
        Texture2D blackTex;
        Texture2D redTex;
        Vector2 position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="white">Texture to use for walkable map spaces.</param>
        /// <param name="black">Texture to use for wall map spaces.</param>
        /// <param name="red">Texture to use for current player space.</param>
        public Maze(Texture2D white, Texture2D black, Texture2D red)
        {
            whiteTex = white;
            blackTex = black;
            redTex = red;

            cells = new int[Defs.MapWidth, Defs.MapHeight];

            GenerateMaze();
        }

        /// <summary>
        /// Call this method to generate a new random maze of the size set by the default Map Width and Height.
        /// </summary>
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

        /// <summary>
        /// Call this method to mark a given block as a Wall.
        /// </summary>
        /// <param name="x">The X position of the wall in tiles.</param>
        /// <param name="y">The Y position of the wall in tiles.</param>
        /// <param name="wallList">The list of walls to add it to.</param>
        /// <returns>Returns the new list of walls.</returns>
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
            if (0 < cx && cx < Defs.MapWidth && cy > 0 && cells[cx, cy - 1] == 0)
                directions.Add((int)Defs.Direction.N);

            if (0 < cx && cx < Defs.MapWidth && cy + 1 < Defs.MapHeight - 1 && cells[cx, cy + 1] == 0)
                directions.Add((int)Defs.Direction.S);

            if (0 < cy && cy < Defs.MapHeight && cx + 1 < Defs.MapWidth - 1 && cells[cx + 1, cy] == 0)
                directions.Add((int)Defs.Direction.E);

            if (0 < cy && cy < Defs.MapHeight && cx > 0 && cells[cx - 1, cy] == 0)
                directions.Add((int)Defs.Direction.W);

            if (directions.Count > 0)
                return directions[random.Next(directions.Count)];
            else
                return -1;
        }

        /// <summary>
        /// Call this method to draw the minimap on the screen.
        /// </summary>
        /// <param name="sb">The SpriteBatch to use to draw.</param>
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

        /// <summary>
        /// Call this method to generate a set of cube objects based on the current maze layout.
        /// </summary>
        /// <param name="model">The model to inject into the cubes.</param>
        /// <returns>A list of cube objects.</returns>
        public List<Cube> CreateMaze(Model model)
        {
            List<Cube> cubes;

            // Get Maze Array
            int[,] mazePos = cells;

            cubes = new List<Cube>();

            // Create 3D Maze
            for (int width = 0; width < Defs.MapWidth; width++)
            {
                for (int height = 0; height < Defs.MapHeight; height++)
                {
                    if (mazePos[width, height] == 1)
                        cubes.Add(new Cube(model, new Vector3(width * 2, 0, height * 2)));

                    // Create Floor
                    cubes.Add(new Cube(model, new Vector3(width * 2, -2, height * 2)));
                }
            }

            return cubes;
        }

        /// <summary>
        /// Call this function to update the player's current position on the minimap.
        /// </summary>
        /// <param name="mapPosition">The player's world position in world coordinates.</param>
        public void Update(Vector2 mapPosition)
        {
            position = mapPosition / 2;
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

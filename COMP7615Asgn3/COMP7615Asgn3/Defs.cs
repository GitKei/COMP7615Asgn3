using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace COMP7615Asgn3
{
    /// <summary>
    /// This class holds constants used throughout the program.
    /// </summary>
    class Defs
    {
        public const int RandomPath = 20; // Number of blocks to make walkable to create additional paths
        public const int MapWidth = 10; // Map width (X)
        public const int MapHeight = 10; // Map height (Z)
        public enum Direction { N = 1, S = 2, E = 3, W = 4 };
        public enum Move { Forward, Backward, Left, Right };
    }
}

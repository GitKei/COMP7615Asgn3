using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace COMP7615Asgn3
{
    /// <summary>
    /// This class represents a maze cube, including the 3D model and world position.
    /// </summary>
    class Cube
    {
        private Model model;
        private Vector3 position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cubeModel">The model to be used for the cube.</param>
        /// <param name="pos3D">The world position in XYZ coords.</param>
        public Cube(Model cubeModel, Vector3 pos3D)
        {
            model = cubeModel;
            position = pos3D;
        }

        public Model Model
        {
            get
            {
                return model;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
        }
    }
}

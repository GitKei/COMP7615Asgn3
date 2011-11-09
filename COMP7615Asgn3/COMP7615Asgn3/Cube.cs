using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace COMP7615Asgn3
{
    class Cube
    {
        private Model model;
        private Vector3 position;

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

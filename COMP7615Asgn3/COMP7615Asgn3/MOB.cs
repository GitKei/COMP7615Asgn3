using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace COMP7615Asgn3
{
    class MOB
    {
        public float angleX, angleY, angleZ;
        public float transX, transZ;

        private Vector2 TryMove(Vector2 displacement, List<Cube> walls)
        {
            Vector2 currentPos = new Vector2(-transX, transZ);
            Vector2 movement = currentPos + displacement;

            foreach (Cube wall in walls)
            {
                if (wall.Position.Y < -1)
                    continue;

                // Move X, Z
                if (wall.Position.X - 1.2 <= movement.X && movement.X <= wall.Position.X + 1.2 && -wall.Position.Z - 1.2 <= movement.Y && movement.Y <= -wall.Position.Z + 1.2)
                {
                    displacement.X = 0;
                    displacement.Y = 0;
                }
            }

            return displacement;
        }

        public void Move(Defs.Move dir, bool isClip, List<Cube> walls)
        {
            float xPart, zPart;

            switch (dir)
            {
                case Defs.Move.Forward:
                    xPart = (float)Math.Sin(angleX) * 0.05f;
                    zPart = (float)Math.Cos(angleX) * 0.05f;

                    if (isClip)
                    {
                        transX -= xPart;
                        transZ += zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(xPart, zPart), walls);
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                    break;
                case Defs.Move.Backward:
                    xPart = (float)Math.Sin(angleX) * 0.05f;
                    zPart = (float)Math.Cos(angleX) * 0.05f;

                    if (isClip)
                    {
                        transX += xPart;
                        transZ -= zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(-xPart, -zPart), walls);
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                    break;
                case Defs.Move.Left:
                    xPart = (float)Math.Cos(angleX) * 0.05f;
                    zPart = (float)Math.Sin(angleX) * 0.05f;

                    if (isClip)
                    {
                        transX += xPart;
                        transZ += zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(-xPart, zPart), walls);
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                    break;
                case Defs.Move.Right:
                    xPart = (float)Math.Cos(angleX) * 0.05f;
                    zPart = (float)Math.Sin(angleX) * 0.05f;

                    if (isClip)
                    {
                        
                        transX -= xPart;
                        transZ -= zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(xPart, -zPart), walls);
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                    break;
            }
        }
    }
}

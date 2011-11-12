using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace COMP7615Asgn3
{
    class MOB
    {
        public float angleX, angleY, angleZ; // The facing of the MOB
        public float transX, transZ; // The position of the MOB

        /// <summary>
        /// Call this method when clipping is on to see if the desired movement would run into a wall.
        /// </summary>
        /// <param name="displacement">The desired movement vector.</param>
        /// <param name="walls">The list of walls.</param>
        /// <returns>The modified movement vector.</returns>
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

        /// <summary>
        /// Call this method to have the MOB move in the given direction relative to its facing.
        /// </summary>
        /// <param name="dir">The direction to move.</param>
        /// <param name="isClip">True if collision is off, false otherwise.</param>
        /// <param name="walls">The list of collidable walls.</param>
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

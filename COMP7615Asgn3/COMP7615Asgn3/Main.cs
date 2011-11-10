using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace COMP7615Asgn3
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Maze
        MazeGenerator maze;

        // 3D
        Matrix world, view, projection;

        // Camera
        Vector3 cameraPosition = new Vector3(0, 0, -50);
        Vector3 cameraReference = new Vector3(0, 0, 1);
        float fov = MathHelper.PiOver4;

        // Mouse Camera
        MouseState originalMouse;

        // Switches
        bool isMap;
        bool isFog;
        bool isDay;

        // Cube
        List<Cube> cubes;
        Model cubeModel;
        float angleX, angleY, angleZ, viewdist;
        float transX, transZ;

        // Lighting
        private Vector3 ambientDay = new Vector3(0.6f, 0.6f, 0.6f);
        private Vector3 ambientNight = new Vector3(0.2f, 0.2f, 0.2f);
        private Vector3 diffuseDay = new Vector3(0.9f, 0.9f, 0.7f);
        private Vector3 diffuseNight = new Vector3(0.8f, 0.8f, 0.8f);
        private Vector3 diffuseDirection = new Vector3(0.1f, -1f, 0.1f);

        KeyboardState previousKey;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            isMap = false;
            isFog = false;

            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            originalMouse = Mouse.GetState();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Generate Maze
            maze = new MazeGenerator(Content.Load<Texture2D>("Images/White"),
                                     Content.Load<Texture2D>("Images/Black"),
                                     Content.Load<Texture2D>("Images/Red"));

            // Load Cube Model
            cubeModel = Content.Load<Model>("cube");

            CreateMaze(cubeModel);

            viewdist = -20;
            
            // Set up WVP Matrices
            world = Matrix.Identity;
            view = Matrix.CreateTranslation(10f, 10f, viewdist);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 1f, 200f);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Controls();
            UpdateCamera();

            base.Update(gameTime);
        }

        private void Controls()
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            // Close Program
            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            // Camera Angle
            if (ks.IsKeyDown(Keys.Left))
                angleX -= 0.01f;
            if (ks.IsKeyDown(Keys.Right))
                angleX += 0.01f;
            if (ks.IsKeyDown(Keys.Up))
                angleY -= 0.01f;
            if (ks.IsKeyDown(Keys.Down))
                angleY += 0.01f;

            // Mouse Camera
            if (ms != originalMouse)
            {
                float xDifference = (ms.X - originalMouse.X) / 2;
                float yDifference = (ms.Y - originalMouse.Y) / 2;
                angleX += 0.01f * xDifference * 1;
                angleY += 0.01f * yDifference * 1;
                Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            }

            // Zoom / FOV
            if (ks.IsKeyDown(Keys.Add) || ks.IsKeyDown(Keys.OemPlus))
                viewdist += 0.01f;
            if (ks.IsKeyDown(Keys.Subtract) || ks.IsKeyDown(Keys.OemMinus))
                viewdist -= 0.01f;

            // Activate Map
            if (ks.IsKeyDown(Keys.M) && previousKey.IsKeyUp(Keys.M))
                isMap = !isMap;

            // Activate Fog
            if (ks.IsKeyDown(Keys.F) && previousKey.IsKeyUp(Keys.F))
                isFog = !isFog;

            // Activate Day/Night
            if (ks.IsKeyDown(Keys.L) && previousKey.IsKeyUp(Keys.L))
                isDay = !isDay;

            if (isMap)
            {
                // Show Map
                maze.Move(ks);

                if (ks.IsKeyDown(Keys.R) && previousKey.IsKeyUp(Keys.R))
                {
                    maze.GenerateMaze();
                    CreateMaze(cubeModel);
                }
            }
            else
            {
                if (ks.IsKeyDown(Keys.W))
                {
                    float xPart = (float) Math.Sin(angleX) * 0.05f;
                    float zPart = (float) Math.Cos(angleX) * 0.05f;
                    transX -= xPart;
                    transZ += zPart;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    float xPart = (float)Math.Sin(angleX) * 0.05f;
                    float zPart = (float)Math.Cos(angleX) * 0.05f;
                    transX += xPart;
                    transZ -= zPart;
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    float xPart = (float)Math.Cos(angleX) * 0.05f;
                    float zPart = (float)Math.Sin(angleX) * 0.05f;
                    transX += xPart;
                    transZ += zPart;
                }
                if (ks.IsKeyDown(Keys.D))
                {
                    float xPart = (float)Math.Cos(angleX) * 0.05f;
                    float zPart = (float)Math.Sin(angleX) * 0.05f;
                    transX -= xPart;
                    transZ -= zPart;
                }
            }

            // Set Previous KeyboardState
            previousKey = ks;
        }

        private void UpdateCamera()
        {
            // Camera
            Matrix rotationMatrix = Matrix.CreateRotationX(angleY) * Matrix.Invert(Matrix.CreateRotationY(angleX));

            //// Create a vector pointing the direction the camera is facing.
            //Vector3 transformedReference = Vector3.Transform(cameraReference, rotationMatrix);

            //// Calculate the position the camera is looking at.
            //Vector3 cameraLookat = cameraPosition + transformedReference;

            //// Set up the view matrix and projection matrix.
            //view = Matrix.CreateLookAt(cameraPosition, cameraLookat, Vector3.Up);

            //projection = Matrix.CreatePerspectiveFieldOfView(fov, graphics.GraphicsDevice.Viewport.AspectRatio,
            //                                                 1f, 200f);

            //Matrix T = Matrix.CreateTranslation(transX, 0, transZ);
            
            //view *= T;

            Matrix R = Matrix.CreateRotationY(angleX) * Matrix.CreateRotationX(angleY) * Matrix.CreateRotationZ(angleZ);
            Matrix T = Matrix.CreateTranslation(transX, 0, transZ);
            //Matrix S = Matrix.CreateScale(1.0f);
            view = T * R;

            // Reset Angles
            if (angleX > 2 * Math.PI)
                angleX = 0;

            if (angleY > 2 * Math.PI)
                angleY = 0;

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (isMap)
            {
                // Draw Map
                spriteBatch.Begin();

                maze.DrawMap(spriteBatch);

                spriteBatch.End();

                GraphicsDevice.BlendState = BlendState.Additive;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else
            {
                // Render 3D World
                foreach (Cube cube in cubes)
                {
                    Matrix[] transforms = new Matrix[cube.Model.Bones.Count];
                    cube.Model.CopyAbsoluteBoneTransformsTo(transforms);

                    foreach (ModelMesh mesh in cube.Model.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.LightingEnabled = true;
                            effect.DirectionalLight0.Enabled = true;

                            // Day/Night
                            if (isDay)
                            {
                                effect.DirectionalLight0.DiffuseColor = diffuseDay;
                                effect.AmbientLightColor = ambientDay;
                            }
                            else
                            {
                                effect.DirectionalLight0.DiffuseColor = diffuseNight;
                                effect.AmbientLightColor = ambientNight;
                            }
                            
                            effect.DirectionalLight0.Direction = diffuseDirection;

                            // Fog
                            if (isFog)
                            {
                                effect.FogEnabled = true;
                                //effect.FogStart = 35.0f;
                                //effect.FogEnd = 250.0f;
                                effect.FogColor = new Vector3(250.0f, 250.0f, 250.0f);
                            }
                            else
                                effect.FogEnabled = false;

                            Matrix matrixTrans = Matrix.CreateTranslation(cube.Position);
                            Matrix matrixRot = Matrix.CreateRotationX(-(float)Math.PI);

                            effect.World = transforms[mesh.ParentBone.Index] * matrixTrans * world;
                            effect.View = view;
                            effect.Projection = projection;
                        }
                        mesh.Draw();
                    }
                }
            }

            base.Draw(gameTime);
        }

        private void CreateMaze(Model model)
        {
            // Get Maze Array
            int[,] mazePos = maze.Maze;

            cubes = new List<Cube>();

            // Create 3D Maze
            for (int width = 0; width < Defs.MapWidth; width++)
            {
                for (int height = 0; height < Defs.MapHeight; height++)
                {
                    if (mazePos[width, height] == 1)
                        cubes.Add(new Cube(model, new Vector3(height * 2, 0, -width * 2)));

                    // Create Floor
                    cubes.Add(new Cube(model, new Vector3(height * 2, -2, -width * 2)));
                }
            }
        }
    }
}

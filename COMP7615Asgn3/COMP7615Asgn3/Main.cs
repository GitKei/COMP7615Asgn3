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

        // Switches
        bool isMap;
        bool isFog;
        bool isDay;

        // Cube
        List<Cube> cubes;
        Model cubeModel;
        float angle, angleVert, viewdist;
        float xTrans, yTrans;

        // Lighting
        private Vector3 ambientDay = new Vector3(0.7f, 0.7f, 0.7f);
        private Vector3 ambientNight = new Vector3(250f, 250f, 250f);
        private Vector3 diffuseColor = new Vector3(1f, 1f, 1f);
        private Vector3 diffuseDirection = new Vector3(0f, -1f, 0f);

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
            view = Matrix.CreateTranslation(0f, 0f, viewdist);
            //projection = Matrix.CreateOrthographic(MathHelper.ToRadians(70), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 1f, 20f);
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
            KeyboardState ks = Keyboard.GetState();

            // Close Program
            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            if (ks.IsKeyDown(Keys.Left))
                angle = angle - 0.01f;
            if (ks.IsKeyDown(Keys.Right))
                angle = angle + 0.01f;
            if (ks.IsKeyDown(Keys.Up))
                angleVert -= 0.01f;
            if (ks.IsKeyDown(Keys.Down))
                angleVert += 0.01f;
            if (ks.IsKeyDown(Keys.Add) || ks.IsKeyDown(Keys.OemPlus))
                viewdist += 01.01f;
            if (ks.IsKeyDown(Keys.Subtract) || ks.IsKeyDown(Keys.OemMinus))
                viewdist -= 01.01f;

            // Activate Map
            if (ks.IsKeyDown(Keys.M) && previousKey.IsKeyUp(Keys.M))
                isMap = !isMap;

            // Activate Fog
            if (ks.IsKeyDown(Keys.F) && previousKey.IsKeyUp(Keys.F))
                isFog = !isFog;

            // Activate Day/Night
            if (ks.IsKeyDown(Keys.L) && previousKey.IsKeyUp(Keys.L))
                isDay = !isDay;

            // Show Map
            if (isMap)
            {
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
                    yTrans += 0.1f;
                if (ks.IsKeyDown(Keys.S))
                    yTrans -= 0.1f;
                if (ks.IsKeyDown(Keys.A))
                    xTrans -= 0.1f;
                if (ks.IsKeyDown(Keys.D))
                    xTrans += 0.1f;
            }
            
            if (angle > 2 * Math.PI)
                angle = 0;

            if (angleVert > 2 * Math.PI)
                angleVert = 0;

            Matrix R = Matrix.CreateRotationY(angle) * Matrix.CreateRotationX(angleVert) * Matrix.CreateRotationZ(0.0f);
            Matrix T = Matrix.CreateTranslation(xTrans, yTrans, viewdist);
            Matrix S = Matrix.CreateScale(1.0f);
            view = S * R * T;

            previousKey = ks;

            base.Update(gameTime);
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

                            // Day/Night
                            if (isDay)
                                effect.AmbientLightColor = ambientDay;
                            else
                                effect.AmbientLightColor = ambientNight;

                            effect.DirectionalLight0.Enabled = true;
                            effect.DirectionalLight0.DiffuseColor = diffuseColor;
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
                            Matrix matrixRot = Matrix.CreateRotationX(-(float)Math.PI / 2);

                            effect.World = transforms[mesh.ParentBone.Index] * matrixTrans * matrixRot * world;
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
                        cubes.Add(new Cube(model, new Vector3(width * 2, height * 2, 1)));
                }
            }
        }
    }
}

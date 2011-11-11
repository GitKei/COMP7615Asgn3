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
        Maze maze;

        // 3D
        Matrix world, view, projection;

        // Mouse Camera
        MouseState originalMouse;
        float fov = MathHelper.PiOver4;

        // Switches
        bool isMap;
        bool isFog;
        bool isDay;
        bool isClip;

        // Cube
        List<Cube> cubes;
        Model cubeModel;
        float angleX, angleY, angleZ;
        float transX, transZ;

        // Cartman
        Model cartmanModel;
        Vector3 cartmanPosition;
        Vector2 cartmanDirection;
        float cartmanAngle;
        int cartmanFrames;
        int cartmanMoveFrames;
        const int cartmanFrameDelay = 10;

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
            isClip = false;
            isDay = true;

            angleZ = 0;

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
            maze = new Maze(Content.Load<Texture2D>("Images/White"),
                                     Content.Load<Texture2D>("Images/Black"),
                                     Content.Load<Texture2D>("Images/Red"));

            // Load Cartman
            cartmanModel = Content.Load<Model>("cartman");
            cartmanPosition = new Vector3((Defs.MapWidth - 1) * 2, -0.5f, (Defs.MapHeight - 2) * 2);
            cartmanDirection = Vector2.Zero;
            cartmanMoveFrames = 0;

            // Load Cube Model
            cubeModel = Content.Load<Model>("cube");

            CreateMaze(cubeModel);

            ResetMaze();
            
            // Set up WVP Matrices
            world = Matrix.Identity;
            view = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 0.1f, 100f);
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
            HandleKeyboard();
            HandleMouse();
            UpdateCamera();
            EnemyMovement();

            base.Update(gameTime);
        }


        private Vector2 TryMove(Vector2 displacement)
        {
            Vector2 currentPos = new Vector2(-transX, transZ);
            Vector2 movement = currentPos + displacement;

            foreach (Cube cube in cubes)
            {
                if (cube.Position.Y < -1)
                    continue;

                // Move X, Z
                if (cube.Position.X - 1.2 <= movement.X && movement.X <= cube.Position.X + 1.2 && -cube.Position.Z - 1.2 <= movement.Y && movement.Y <= -cube.Position.Z + 1.2)
                {
                    displacement.X = 0;
                    displacement.Y = 0;
                }
            }   

            return displacement;
        }

        private void HandleMouse()
        {
            MouseState ms = Mouse.GetState();

            // Mouse Locking
            if (ms != originalMouse)
            {
                float xDifference = (ms.X - originalMouse.X) / 2;
                float yDifference = (ms.Y - originalMouse.Y) / 2;
                angleX += 0.01f * xDifference;
                angleY += 0.01f * yDifference;
                Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            }

            // Allow Clipping
            if (ms.LeftButton == ButtonState.Pressed)
            {
                float xPart = (float)Math.Sin(angleX) * 0.05f;
                float zPart = (float)Math.Cos(angleX) * 0.05f;
                transX -= xPart;
                transZ += zPart;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState ks = Keyboard.GetState();
         
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

            // Zoom / FOV
            if (ks.IsKeyDown(Keys.Add) || ks.IsKeyDown(Keys.OemPlus))
                fov += 0.1f;
            if (ks.IsKeyDown(Keys.Subtract) || ks.IsKeyDown(Keys.OemMinus))
                fov -= 0.1f;

            // Toggle Map
            if (ks.IsKeyDown(Keys.M) && previousKey.IsKeyUp(Keys.M))
                isMap = !isMap;

            // Toggle Fog
            if (ks.IsKeyDown(Keys.F) && previousKey.IsKeyUp(Keys.F))
                isFog = !isFog;

            // Toggle Day/Night
            if (ks.IsKeyDown(Keys.L) && previousKey.IsKeyUp(Keys.L))
                isDay = !isDay;

            // Toggle Clipping
            if (ks.IsKeyDown(Keys.C) && previousKey.IsKeyUp(Keys.C))
                isClip = !isClip;

            // Home
            if (ks.IsKeyDown(Keys.Home) && previousKey.IsKeyUp(Keys.Home))
                ResetMaze();

            if (isMap)
            {
                // Show Map
                maze.Move(ks);

                if (ks.IsKeyDown(Keys.R) && previousKey.IsKeyUp(Keys.R))
                {
                    maze.GenerateMaze();
                    CreateMaze(cubeModel);
                    ResetMaze();
                }
            }
            else
            {
                if (ks.IsKeyDown(Keys.W))
                {
                    float xPart = (float)Math.Sin(angleX) * 0.05f;
                    float zPart = (float)Math.Cos(angleX) * 0.05f;

                    if (isClip)
                    {
                        transX -= xPart;
                        transZ += zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(xPart, zPart));
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                    
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    float xPart = (float)Math.Sin(angleX) * 0.05f;
                    float zPart = (float)Math.Cos(angleX) * 0.05f;

                    if (isClip)
                    {
                        transX += xPart;
                        transZ -= zPart;
                    }
                    else
                    {
                        Vector2 displacement = TryMove(new Vector2(-xPart, -zPart));
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    if (isClip)
                    {
                        // Needs Fix
                        float xPart = (float)Math.Cos(angleX) * 0.02f;
                        float zPart = (float)Math.Sin(angleX) * 0.05f;
                        transX -= xPart;
                        transZ += zPart;
                    }
                    else
                    {
                        float xPart = (float)Math.Cos(angleX) * 0.02f;
                        float zPart = (float)Math.Sin(angleX) * 0.02f;
                        Vector2 displacement = TryMove(new Vector2(-xPart, zPart));
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                }
                if (ks.IsKeyDown(Keys.D))
                {
                    if (isClip)
                    {
                        // Needs Fix
                        float xPart = (float)Math.Cos(angleX) * 0.02f;
                        float zPart = (float)Math.Sin(angleX) * 0.02f;
                        transX += xPart;
                        transZ -= zPart;
                    }
                    else
                    {
                        float xPart = (float)Math.Cos(angleX) * 0.02f;
                        float zPart = (float)Math.Sin(angleX) * 0.02f;
                        Vector2 displacement = TryMove(new Vector2(xPart, -zPart));
                        transX -= displacement.X;
                        transZ += displacement.Y;
                    }
                }
            }

            // Set Previous KeyboardState
            previousKey = ks;
        }

        private void UpdateCamera()
        {
            Matrix R = Matrix.CreateRotationY(angleX) * Matrix.CreateRotationX(angleY) * Matrix.CreateRotationZ(angleZ);
            Matrix T = Matrix.CreateTranslation(transX, 0, transZ);
            //Matrix S = Matrix.CreateScale(1.0f);
            view = T * R;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 0.1f, 100f);

            // Reset Angles
            if (angleX > 2 * Math.PI)
                angleX = 0;

            if (angleY > 2 * Math.PI)
                angleY = 0;
        }

        private void EnemyMovement()
        {
            if (cartmanFrames % cartmanFrameDelay == 0)
            {
                if (cartmanMoveFrames <= 0)
                {
                    Random random = new Random();
                    cartmanAngle = (float)random.NextDouble() * MathHelper.TwoPi;

                    

                    cartmanMoveFrames = 20;
                }

                float xPart = (float)Math.Sin(cartmanAngle) * 0.05f;
                float zPart = (float)Math.Cos(cartmanAngle) * 0.05f;

                Vector2 displacement = TryMove(new Vector2(xPart, -zPart));

                cartmanPosition.X -= displacement.X;
                cartmanPosition.Z += displacement.Y;
            }

            cartmanFrames++;
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

                GraphicsDevice.BlendState = BlendState.Opaque;
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

                // Render Cartman
                Matrix[] transform = new Matrix[cartmanModel.Bones.Count];
                cartmanModel.CopyAbsoluteBoneTransformsTo(transform);

                foreach (ModelMesh mesh in cartmanModel.Meshes)
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
                            effect.FogStart = 9.75f;
                            effect.FogEnd = 10.25f;
                            effect.FogColor = Color.WhiteSmoke.ToVector3();
                            effect.FogEnabled = true;
                        }
                        else
                            effect.FogEnabled = false;

                        Matrix matrixTrans = Matrix.CreateTranslation(cartmanPosition);
                        Matrix matrixRot = Matrix.CreateRotationX(-(float)MathHelper.PiOver2) * Matrix.CreateRotationY((float)Math.Cos(cartmanAngle));
                        Matrix matrixScale = Matrix.CreateScale(0.1f);

                        effect.World = transform[mesh.ParentBone.Index] * matrixScale * matrixRot * matrixTrans * world;
                        effect.View = view;
                        effect.Projection = projection;
                    }
                    mesh.Draw();
                }
            }

            base.Draw(gameTime);
        }

        private void CreateMaze(Model model)
        {
            // Get Maze Array
            int[,] mazePos = maze.Cells;

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
        }

        private void ResetMaze()
        {
            fov = 70;
            transX = 0;
            transZ = -2;
            angleX = MathHelper.PiOver2;
        }
    }
}

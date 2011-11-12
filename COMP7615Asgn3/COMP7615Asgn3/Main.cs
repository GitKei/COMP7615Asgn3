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
        bool isFlash;

        // Cube
        List<Cube> cubes;
        Model cubeModel;

        MOB player;
        MOB cartman;

        // Cartman
        Model cartmanModel;
        int cartmanMoveFrames;

        // Lighting
        Vector3 ambientDay = new Vector3(0.7f, 0.7f, 0.7f);
        Vector3 ambientNight = new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 diffuseDay = new Vector3(0.9f, 0.9f, 0.7f);
        Vector3 diffuseNight = new Vector3(0.8f, 0.8f, 0.8f);
        Vector3 diffuseDirection = new Vector3(0.1f, -1f, 0.1f);

        // Flashlight
        Texture2D flashTexture;
        Vector2 flashPosition;

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

            player = new MOB();
            cartman = new MOB();

            // Load Cartman
            cartmanModel = Content.Load<Model>("cartman");
            cartman.transX = (Defs.MapWidth - 1) * 2;
            cartman.transZ = (Defs.MapHeight - 2) * 2;

            cartmanMoveFrames = 0;

            // Load Cube Model
            cubeModel = Content.Load<Model>("cube");

            CreateMaze(cubeModel);

            ResetPosition();

            // Flashlight
            flashTexture = Content.Load<Texture2D>("Images/Flashlight");

            /*Color[] data = new Color[flashTexture.Width * flashTexture.Height];

            flashTexture.GetData<Color>(data);
            for (int k = 0; k < data.Length; k++)
            {
                data[k].A = 127;
            }
            flashTexture.SetData<Color>(data);*/

            flashPosition = new Vector2(this.Window.ClientBounds.Width / 2 - flashTexture.Width / 2,
                                        this.Window.ClientBounds.Height / 2 - flashTexture.Height / 2);
            
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

        private void HandleMouse()
        {
            MouseState ms = Mouse.GetState();

            // Mouse Locking
            if (ms != originalMouse)
            {
                float xDifference = (ms.X - originalMouse.X) / 2;
                float yDifference = (ms.Y - originalMouse.Y) / 2;
                player.angleX += 0.01f * xDifference;
                player.angleY += 0.01f * yDifference;
                Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
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
                player.angleX -= 0.01f;
            if (ks.IsKeyDown(Keys.Right))
                player.angleX += 0.01f;
            if (ks.IsKeyDown(Keys.Up))
                player.angleY -= 0.01f;
            if (ks.IsKeyDown(Keys.Down))
                player.angleY += 0.01f;

            // Zoom / FOV
            if (ks.IsKeyDown(Keys.Add) || ks.IsKeyDown(Keys.OemPlus))
                fov -= 0.1f;
            if (ks.IsKeyDown(Keys.Subtract) || ks.IsKeyDown(Keys.OemMinus))
                fov += 0.1f;

            // Toggle Map
            if (ks.IsKeyDown(Keys.M) && previousKey.IsKeyUp(Keys.M))
                isMap = !isMap;

            // Toggle Fog
            if (ks.IsKeyDown(Keys.G) && previousKey.IsKeyUp(Keys.G))
                isFog = !isFog;

            // Toggle Flashlight
            if (ks.IsKeyDown(Keys.F) && previousKey.IsKeyUp(Keys.F))
                isFlash = !isFlash;

            // Toggle Day/Night
            if (ks.IsKeyDown(Keys.L) && previousKey.IsKeyUp(Keys.L))
                isDay = !isDay;

            // Toggle Clipping
            if (ks.IsKeyDown(Keys.C) && previousKey.IsKeyUp(Keys.C))
                isClip = !isClip;

            // Home
            if (ks.IsKeyDown(Keys.Home) && previousKey.IsKeyUp(Keys.Home))
                ResetPosition();

            // Show Map and Current Position
            maze.Update(new Vector2(-player.transX, -player.transZ));

            // Generate New Map
            if (ks.IsKeyDown(Keys.R) && previousKey.IsKeyUp(Keys.R))
            {
                maze.GenerateMaze();
                CreateMaze(cubeModel);
                ResetPosition();
            }

            if (ks.IsKeyDown(Keys.W))
            {
                player.Move(Defs.Move.Forward, isClip, cubes);
            }
            if (ks.IsKeyDown(Keys.S))
            {
                player.Move(Defs.Move.Backward, isClip, cubes);
            }
            if (ks.IsKeyDown(Keys.A))
            {
                player.Move(Defs.Move.Left, isClip, cubes);
            }
            if (ks.IsKeyDown(Keys.D))
            {
                player.Move(Defs.Move.Right, isClip, cubes);
            }

            // Set Previous KeyboardState
            previousKey = ks;
        }

        private void UpdateCamera()
        {
            Matrix R = Matrix.CreateRotationY(player.angleX) * Matrix.CreateRotationX(player.angleY) * Matrix.CreateRotationZ(player.angleZ);
            Matrix T = Matrix.CreateTranslation(player.transX, 0, player.transZ);
            
            view = T * R;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 0.1f, 100f);

            // Reset Angles
            if (player.angleX > 2 * Math.PI)
                player.angleX = 0;

            if (player.angleY > 2 * Math.PI)
                player.angleY = 0;
        }

        private void EnemyMovement()
        {
            int[,] cells = maze.Cells;

            //if (cartmanMoveFrames <= 0)
            //{
            //    Random random = new Random();
            //    cartmanAngle = (float)random.NextDouble() * MathHelper.TwoPi;

            //    cartmanMoveFrames = 20;
            //}

            //float xPart = (float)Math.Sin(cartmanAngle) * 0.01f;
            //float zPart = (float)Math.Cos(cartmanAngle) * 0.01f;

            //Vector2 displacement = TryMove(new Vector2(xPart, -zPart));

            //cartmanPosition.X -= displacement.X;
            //cartmanPosition.Z += displacement.Y;

            cartmanMoveFrames--;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (isDay)
                GraphicsDevice.Clear(Color.SkyBlue);
            else
                GraphicsDevice.Clear(Color.Black);

            // Render Cubes
            foreach (Cube cube in cubes)
            {
                DrawModel(cube.Model, cube.Position, -(float)Math.PI, 0, 1);
            }

            Vector3 cartmanPosition = new Vector3(cartman.transX, 0.05f, cartman.transZ);
            // Render Cartman
            DrawModel(cartmanModel, cartmanPosition, -(float)MathHelper.PiOver2, -cartman.angleZ, 0.1f);

            // Flashlight
            if (isFlash)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

                spriteBatch.Draw(flashTexture, flashPosition, Color.White);

                spriteBatch.End();

                // Reset States for Rendering
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            if (isMap)
            {
                // Draw Map
                spriteBatch.Begin();

                maze.DrawMap(spriteBatch);

                spriteBatch.End();

                // Reset States for Rendering
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Vector3 position, float rotateOnX, float rotateOnY, float scale)
        {
            // Render Cartman
            Matrix[] transformMat = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transformMat);

            foreach (ModelMesh mesh in model.Meshes)
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
                        effect.FogStart = 0f;
                        effect.FogEnd = 10f;
                        effect.FogColor = new Vector3(1, 1, 1);
                        effect.FogEnabled = true;
                    }
                    else
                        effect.FogEnabled = false;

                    Matrix matrixTrans = Matrix.CreateTranslation(position);
                    Matrix matrixRot = Matrix.CreateRotationX(rotateOnX) * Matrix.CreateRotationY(rotateOnY);
                    Matrix matrixScale = Matrix.CreateScale(scale);

                    effect.World = transformMat[mesh.ParentBone.Index] * matrixScale * matrixRot * matrixTrans * world;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        private void Flashlight(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            sb.End();
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

        private void ResetPosition()
        {
            fov = 70;
            player.transX = 0;
            player.transZ = -2;
            player.angleX = MathHelper.PiOver2;
        }
    }
}

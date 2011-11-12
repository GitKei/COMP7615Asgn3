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

        KeyboardState ksOld;
        GamePadState gsOld;

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
            maze = new Maze(Content.Load<Texture2D>("Images/White"), Content.Load<Texture2D>("Images/Black"), Content.Load<Texture2D>("Images/Red"));

            player = new MOB();
            cartman = new MOB();

            // Load Cartman
            cartmanModel = Content.Load<Model>("cartman");
            
            // Load Cube Model
            cubeModel = Content.Load<Model>("cube");

            cubes = maze.CreateMaze(cubeModel);

            ResetPosition();

            // Flashlight
            flashTexture = Content.Load<Texture2D>("Images/Flashlight");

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
            maze.Update(new Vector2(-player.transX, -player.transZ));

            HandleInput();
            HandleMouse();
            UpdateCamera();
            EnemyMovement();

            base.Update(gameTime);
        }

        /// <summary>
        /// Handle mouse movement.
        /// </summary>
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

        /// <summary>
        /// Handle Keyboard and Gamepad input since they are basically identical.
        /// </summary>
        private void HandleInput()
        {
            KeyboardState ks = Keyboard.GetState();
            GamePadState gs = GamePad.GetState(PlayerIndex.One);
         
            // Close Program
            if (ks.IsKeyDown(Keys.Escape) || gs.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Camera Angle
            if (ks.IsKeyDown(Keys.Left) || gs.ThumbSticks.Right.X < 0)
                player.angleX -= 0.03f;
            if (ks.IsKeyDown(Keys.Right) || gs.ThumbSticks.Right.X > 0)
                player.angleX += 0.03f;
            if (ks.IsKeyDown(Keys.Up) || gs.ThumbSticks.Right.Y > 0)
                player.angleY -= 0.03f;
            if (ks.IsKeyDown(Keys.Down) || gs.ThumbSticks.Right.Y < 0)
                player.angleY += 0.03f;

            // Zoom / FOV
            if (ks.IsKeyDown(Keys.Add) || gs.Triggers.Right > 0)
                fov -= 0.1f;
            if (ks.IsKeyDown(Keys.Subtract) || gs.Triggers.Left > 0)
                fov += 0.1f;

            // Toggle Map
            if ((ks.IsKeyDown(Keys.M) && ksOld.IsKeyUp(Keys.M)) || (gs.Buttons.Start == ButtonState.Pressed && gsOld.Buttons.Start == ButtonState.Released))
                isMap = !isMap;

            // Toggle Fog
            if ((ks.IsKeyDown(Keys.G) && ksOld.IsKeyUp(Keys.G)) || (gs.Buttons.A == ButtonState.Pressed && gsOld.Buttons.A == ButtonState.Released))
                isFog = !isFog;

            // Toggle Flashlight
            if ((ks.IsKeyDown(Keys.F) && ksOld.IsKeyUp(Keys.F)) || (gs.Buttons.B == ButtonState.Pressed && gsOld.Buttons.B == ButtonState.Released))
                isFlash = !isFlash;

            // Toggle Day/Night
            if ((ks.IsKeyDown(Keys.L) && ksOld.IsKeyUp(Keys.L)) || (gs.Buttons.X == ButtonState.Pressed && gsOld.Buttons.X == ButtonState.Released))
                isDay = !isDay;

            // Toggle Clipping
            if ((ks.IsKeyDown(Keys.C) && ksOld.IsKeyUp(Keys.C)) || (gs.Buttons.Y == ButtonState.Pressed && gsOld.Buttons.Y == ButtonState.Released))
                isClip = !isClip;

            // Home
            if ((ks.IsKeyDown(Keys.Home) && ksOld.IsKeyUp(Keys.Home)) || (gs.Buttons.LeftShoulder == ButtonState.Pressed && gsOld.Buttons.LeftShoulder == ButtonState.Released))
                ResetPosition();

            // Generate New Map
            if ((ks.IsKeyDown(Keys.R) && ksOld.IsKeyUp(Keys.R)) || (gs.Buttons.RightShoulder == ButtonState.Pressed && gsOld.Buttons.RightShoulder == ButtonState.Released))
            {
                maze.GenerateMaze();
                cubes = maze.CreateMaze(cubeModel);
                ResetPosition();
            }
             
            // Do Movement
            if (ks.IsKeyDown(Keys.W) || gs.ThumbSticks.Left.Y > 0)
                player.Move(Defs.Move.Forward, isClip, cubes);
            if (ks.IsKeyDown(Keys.S) || gs.ThumbSticks.Left.Y < 0)
                player.Move(Defs.Move.Backward, isClip, cubes);
            if (ks.IsKeyDown(Keys.A) || gs.ThumbSticks.Left.X < 0)
                player.Move(Defs.Move.Left, isClip, cubes);
            if (ks.IsKeyDown(Keys.D) || gs.ThumbSticks.Left.X > 0)
                player.Move(Defs.Move.Right, isClip, cubes);

            // Set Previous KeyboardState
            ksOld = ks;
            gsOld = gs;
        }

        /// <summary>
        /// Update the our view and projection matrices based on which way the player is facing.
        /// </summary>
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

        /// <summary>
        /// Move Cartmant around the map.
        /// </summary>
        private void EnemyMovement()
        {
            if (cartmanMoveFrames == 0)
                cartmanMoveFrames = 120;

            if (60 < cartmanMoveFrames && cartmanMoveFrames <= 120)
                cartman.angleX = 3 * MathHelper.PiOver2;
            else
                cartman.angleX = MathHelper.PiOver2;

            cartman.Move(Defs.Move.Forward, true, cubes);

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

            Vector3 cartmanPosition = new Vector3(cartman.transX, -0.5f, cartman.transZ);
            // Render Cartman
            DrawModel(cartmanModel, cartmanPosition, -(float)MathHelper.PiOver2, -cartman.angleX, 0.1f);

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

        /// <summary>
        /// Draw the given model.
        /// </summary>
        /// <param name="model">The model to draw.</param>
        /// <param name="position">The world position to draw it at.</param>
        /// <param name="rotateOnX">The rotation about the X axis to apply.</param>
        /// <param name="rotateOnY">The rotation about the Y axis to apply.</param>
        /// <param name="scale">Scaling factor to apply.</param>
        private void DrawModel(Model model, Vector3 position, float rotateOnX, float rotateOnY, float scale)
        {
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

        /// <summary>
        /// Reset player and enemy to starting positions.
        /// </summary>
        private void ResetPosition()
        {
            fov = 70;
            player.transX = 0;
            player.transZ = -2;
            player.angleX = MathHelper.PiOver2;
            cartman.transX = (Defs.MapWidth - 1) * 2 - 2.5f;
            cartman.transZ = (Defs.MapHeight - 2) * 2;
            cartmanMoveFrames = 0;
        }
    }
}

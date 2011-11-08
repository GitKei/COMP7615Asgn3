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

        MazeGenerator maze;

        Model cube;
        Matrix world, view, projection;

        float angle, angleVert, viewdist;
        float xTrans, yTrans;

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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            maze = new MazeGenerator(Content.Load<Texture2D>("Images/White"),
                                     Content.Load<Texture2D>("Images/Black"),
                                     Content.Load<Texture2D>("Images/Red"));

            cube = Content.Load<Model>("cube");

            viewdist = -10;
            // Set up WVP Matrices
            world = Matrix.Identity;
            view = Matrix.CreateTranslation(0f, 0f, viewdist);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height, 1f, 20f);
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
            if (ks.IsKeyDown(Keys.Add))
                viewdist += 0.01f;
            if (ks.IsKeyDown(Keys.Subtract))
                viewdist -= 0.01f;

            //if (ks.IsKeyDown(Keys.W))
            //    yTrans += 0.1f;
            //if (ks.IsKeyDown(Keys.S))
            //    yTrans -= 0.1f;
            //if (ks.IsKeyDown(Keys.A))
            //    xTrans -= 0.1f;
            //if (ks.IsKeyDown(Keys.D))
            //    xTrans += 0.1f;

            maze.Move(ks);

            if (ks.IsKeyDown(Keys.R) && previousKey.IsKeyUp(Keys.R))
                maze.GenerateMaze();
            
            if (angle > 2 * Math.PI)
                angle = 0;

            if (angleVert > 2 * Math.PI)
                angleVert = 0;

            Matrix R = Matrix.CreateRotationY(angle) * Matrix.CreateRotationX(angleVert) * Matrix.CreateRotationZ(0.4f);
            Matrix T = Matrix.CreateTranslation(xTrans, yTrans, viewdist);
            //Matrix S = Matrix.CreateScale(scale);
            view = R * T;//S * R * T;

            previousKey = ks;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AliceBlue);

            spriteBatch.Begin();

            maze.DrawMap(spriteBatch);

            spriteBatch.End();

            Matrix[] transforms = new Matrix[cube.Bones.Count];
            cube.CopyAbsoluteBoneTransformsTo(transforms);

            Vector3 ambientCol = new Vector3(0.7f, 0.7f, 0.7f);
            Vector3 diffuseCol = new Vector3(1f, 1f, 1f);
            Vector3 diffuseDir = new Vector3(0f, -1f, 0f);

            foreach (ModelMesh mesh in cube.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = true;
                    effect.AmbientLightColor = ambientCol;
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = diffuseCol;
                    effect.DirectionalLight0.Direction = diffuseDir;
                    
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}

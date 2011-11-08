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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model cube;
        Matrix world, view, projection;

        float angle, angleVert;

        public Game1()
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

            cube = Content.Load<Model>("cube");

            // Set up WVP Matrices
            world = Matrix.Identity;
            view = Matrix.CreateTranslation(0f, 0f, -10f);
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

            if (angle > 2 * Math.PI)
                angle = 0;

            if (angleVert > 2 * Math.PI)
                angleVert = 0;

            Matrix R = Matrix.CreateRotationY(angle) * Matrix.CreateRotationX(angleVert) * Matrix.CreateRotationZ(0.4f);
            Matrix T = Matrix.CreateTranslation(0.0f, 0f, 0f);
            //Matrix S = Matrix.CreateScale(scale);
            world = R * T;//S * R * T;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Matrix[] transforms = new Matrix[cube.Bones.Count];
            cube.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in cube.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    currentEffect.World = transforms[mesh.ParentBone.Index] * world;
                    currentEffect.View = view;
                    currentEffect.Projection = projection;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}

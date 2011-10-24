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

namespace Nodes
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class NodesGame : Microsoft.Xna.Framework.Game
    {

        #region Variables

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice gDevice;

        int screenWidth = 1200;
        int screenHeight = 900;

        string windowTitle = "Nodes";

        Texture2D circle50;


        List<Node> nodeList;
        List<Player> playerList;
        List<Unit> unitList;

        #endregion


        #region Initialization

        public NodesGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. 
        /// </summary>
        protected override void Initialize()
        {
            gDevice = graphics.GraphicsDevice;

            //set application settings
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = windowTitle;


            //get graphics resources
            circle50 = Content.Load<Texture2D>("circle50");


            //get level data
            //TODO

            nodeList = new List<Node>();
            nodeList.Add(new Node(new Vector2(50, 100), 10, 0));
            //load level data into variables


            //enumerate through any components and initialize them
            base.Initialize();
        }

        #endregion


        #region Content Management

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of the content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion


        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //run game logic
            ProcessInput();
            ProcessAI();
            UpdateUnits();
            UpdateNodes();
            CheckCollisions();
            UpdatePlayers();


            base.Update(gameTime);
        }


        private void ProcessInput()
        {

        }

        private void ProcessAI()
        {

        }

        private void UpdateUnits()
        {

        }

        private void UpdateNodes()
        {

        }

        private void CheckCollisions()
        {

        }

        private void UpdatePlayers()
        {

        }


        #endregion


        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawBackground();
            DrawNodes();
            DrawUnits();
            DrawUI();
            DrawFX();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBackground()
        {

        }

        private void DrawNodes()
        {
            foreach(Node node in nodeList){
                int radius = CalcNodeRadius(node.UnitCount);
                float nodeScale = 50.0f/radius;
                spriteBatch.Draw(circle50, node.Position, null, GetPlayerColor(node.OwnerId), 0.0f, new Vector2(50, 50), nodeScale, SpriteEffects.None, 0);
            }
        }

        private int CalcNodeRadius(int unitCount)
        {
            return 15;
        }

        private Color GetPlayerColor(int ownerId)
        {
            //return playerList[ownerId].Color;
            return Color.Blue;
        }

        private void DrawUnits()
        {

        }

        private void DrawUI()
        {

        }

        private void DrawFX()
        {

        }

        #endregion

    }
}

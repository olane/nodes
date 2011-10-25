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

        public Random r = new Random();

        int screenWidth = 1200;
        int screenHeight = 900;

        string windowTitle = "Nodes";

        Color NeutralColor = new Color(100,100,100,255);

        SpriteFont Trebuchet;
        Texture2D circle50;
        Texture2D circle300;
        Texture2D blank;


        List<Node> nodeList;
        List<Player> playerList;
        List<Unit> unitList;

        int humanOwnerId = 0;

        MouseState previousMouseState; //holds last frame's mouse state
        MouseState currentMouseState;  //holds this frame's mouse state


        #endregion


        #region Initialization

        public NodesGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
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
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = windowTitle;


            //get level data
            //TODO


            //load level data into variables

            nodeList = new List<Node>();
            nodeList.Add(new Node(new Vector2(200, 400), 10, 0, 0.01f, r));
            nodeList.Add(new Node(new Vector2(800, 200), 20, 1, 0.01f, r));
            nodeList.Add(new Node(new Vector2(900, 600), 9, 2, 0.03f, r));
            nodeList.Add(new Node(new Vector2(300, 550), 15, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(600, 300), 40, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(300, 800), 27, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(550, 120), 32, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(700, 500), 37, -1, 0.005f, r));

            playerList = new List<Player>();
            playerList.Add(new Player(Color.Red, true, true));
            playerList.Add(new Player(Color.Blue, true, false));
            playerList.Add(new Player(Color.Green, true, false));

            unitList = new List<Unit>();

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


            //get fonts
            Trebuchet = Content.Load<SpriteFont>("Trebuchet");

            //get textures
            //circle50 = Content.Load<Texture2D>("circle50");
            circle50 = DrawCircle(50);
            circle300 = DrawCircle(300);

            //blank 1x1 texture for drawing lines
            blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blank.SetData(new[] { Color.White });
            
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
            //process mouse
            currentMouseState = Mouse.GetState();

            //check if mouse just clicked a node
            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton != ButtonState.Pressed)
            {
                foreach (Node node in nodeList)
                {
                    if (CheckPointCircleCollision(currentMouseState.X, currentMouseState.Y, node.Position.X, node.Position.Y, CalcNodeRadius(node.UnitCount, node.UnitProgress)))
                    {
                        //mouse is clicking a node
                        Node selectedNode = getSelectedNode();

                        //check what we should do with the clicked node
                        if (selectedNode == null && node.OwnerId == humanOwnerId)
                        {
                            node.Selected = true;
                        }
                        else if (selectedNode != null && selectedNode != node)
                        {
                            spawnUnits(selectedNode, node);
                            selectedNode.Selected = false;
                        }
                        else if(selectedNode == node)
                        {
                            selectedNode.Selected = false;
                        }
                    }
                }
            }

            

            //store mouse state so that we can check against it next frame
            previousMouseState = currentMouseState;
        }

        private void ProcessAI()
        {

        }

        private void UpdateUnits()
        {

        }

        private void UpdateNodes()
        {
            foreach (Node node in nodeList)
            {
                //update unit build progress
                node.UnitProgress += node.BuildSpeed;
            }
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

                //draw circle
                float radius = CalcNodeRadius(node.UnitCount, node.UnitProgress);
                float nodeScale = radius/300.0f;
                Texture2D nodeTexture = DrawCircle((int)Math.Round(radius));

                spriteBatch.Draw(circle300, node.Position, null, GetPlayerColor(node.OwnerId), 0.0f, new Vector2(300, 300), nodeScale, SpriteEffects.None, 0);

                //draw unit count
                Vector2 textSize = Trebuchet.MeasureString(node.UnitCount.ToString());

                Color textColor = Color.White;
                if (node.Selected)
                {
                    textColor = new Color(150, 150, 150);
                }

                spriteBatch.DrawString(Trebuchet, node.UnitCount.ToString(), node.Position - textSize/2, textColor);

            }
        }

        private void DrawUnits()
        {

        }

        private void DrawUI()
        {
            //if a node is selected, draw a line from it to the mouse
            Node selectedNode = getSelectedNode();
            if (selectedNode != null)
            {
                DrawLine(blank, 2f, Color.White, selectedNode.Position, new Vector2(currentMouseState.X, currentMouseState.Y));
            }

            //if a node is moused over, draw a border around it
            foreach (Node node in nodeList)
            {
                if (CheckPointCircleCollision(currentMouseState.X, currentMouseState.Y, node.Position.X, node.Position.Y, CalcNodeRadius(node.UnitCount, node.UnitProgress)))
                {
                    //mouse is over a node
                    if (selectedNode == null && node.OwnerId == humanOwnerId)
                    {
                        //node belongs to player and player has not selected a node, so border the node
                        DrawNodeBorder(node, Color.White, 2);

                    }
                    else if (selectedNode != null && node.OwnerId != humanOwnerId)
                    {
                        //node doesn't belong to player and player has already selected a node, so border the node
                        DrawNodeBorder(node, Color.White, 2);
                    }
                }
            }
        }

        private void DrawFX()
        {

        }

        #endregion


        #region Other methods

        private float CalcNodeRadius(int unitCount, float unitProgress)
        {
            return 5 + unitCount + unitProgress;
        }

        private Color GetPlayerColor(int ownerId)
        {
            if (ownerId >= 0)
            {
                return playerList[ownerId].Color;
            }
            else
            {
                return NeutralColor;
            }
        }

        public bool CheckPointCircleCollision(float pointX, float pointY, float circleX, float circleY, float radius)
        {
            float distanceSquared = (pointX - circleX) * (pointX - circleX) + (pointY - circleY) * (pointY - circleY);

            if ((radius * radius) >= distanceSquared)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void spawnUnits(Node sourceNode, Node destinationNode)
        {
            

        }

        private Node getSelectedNode()
        {
            foreach (Node node in nodeList)
            {
                if (node.Selected)
                {
                    return node;
                }
            }
            return null;
        }

        //credit: http://www.xnawiki.com/index.php?title=Drawing_2D_lines_without_using_primitives
        void DrawLine(Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(blank, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }

        public Texture2D DrawCircle(float radius){

            int boxsize = (int)Math.Round(radius * 2) + 2;
            Texture2D texture = new Texture2D(gDevice, boxsize, boxsize);

            Color[] data = new Color[boxsize * boxsize];


            //test each pixel
            for (int i = 0; i < data.Length; i++)
            {
                if (CheckPointCircleCollision(i % boxsize, (i - i % boxsize) / boxsize, radius + 1, radius + 1, (float)radius))
                {
                    //if pixel is in the circle, colour it
                    data[i] = Color.White;
                }
                else
                {
                    //otherwise, make it transparent
                    data[i] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }

        public Texture2D DrawCircleBorder(float radius, float thickness)
        {

            int boxsize = (int)Math.Round(radius * 2) + 2;
            Texture2D texture = new Texture2D(gDevice, boxsize, boxsize);

            Color[] data = new Color[boxsize * boxsize];


            //test each pixel
            for (int i = 0; i < data.Length; i++)
            {
                if (CheckPointCircleCollision(i % boxsize, (i - i % boxsize) / boxsize, radius + 1, radius + 1, radius + thickness / 2) &&
                    !CheckPointCircleCollision(i % boxsize, (i - i % boxsize) / boxsize, radius + 1, radius + 1, radius-thickness / 2))
                {
                    //if pixel is in the circle, colour it
                    data[i] = Color.White;
                }
                else
                {
                    //otherwise, make it transparent
                    data[i] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private void DrawNodeBorder(Node node, Color color, float thickness)
        {
            float radius = CalcNodeRadius(node.UnitCount, node.UnitProgress);
            Texture2D border = DrawCircleBorder(radius, thickness);

            spriteBatch.Draw(border, node.Position, null, Color.White, 0.0f, new Vector2(radius + 1, radius + 1), 1, SpriteEffects.None, 0);
        }


        #endregion


    }
}

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

        /*
         * 0 -> potential field
         * 1 -> kinetic potential
         */
        int pathfindingMethod = 0;

        SpriteFont Trebuchet;
        Texture2D circle50;
        Texture2D circle300;
        Texture2D blank;


        List<Node> nodeList;
        List<Player> playerList;
        List<Unit> unitList;

        int humanOwnerId = 0;
        float attackProportion = 0.5f; //proportion of units that will be sent from an attacking node
        float maxUnitVelocity = 3;
        float maxUnitVelocitySquared;
        float unitAccel = 0.15f;
        float unitStartVelocity = 0.3f;
        float unitFriction = 0.995f;

        float unitRepulsionLimit = 150;
        //float unitRepulsionLimitSquared;
        float unitRepulsionConstant = 250f;

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
            //graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = windowTitle;

            maxUnitVelocitySquared = maxUnitVelocity * maxUnitVelocity;
            //unitRepulsionLimitSquared = unitRepulsionLimit * unitRepulsionLimit;

            //get level data
            //TODO


            //load level data into variables

            nodeList = new List<Node>();
            nodeList.Add(new Node(new Vector2(200, 400), 30, 0, 0.01f, r));
            nodeList.Add(new Node(new Vector2(800, 200), 20, 1, 0.01f, r));
            nodeList.Add(new Node(new Vector2(900, 600), 9, 2, 0.02f, r));
            nodeList.Add(new Node(new Vector2(300, 550), 15, 0, 0.01f, r));
            nodeList.Add(new Node(new Vector2(600, 300), 40, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(300, 800), 27, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(550, 120), 32, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(700, 500), 37, -1, 0.005f, r));

            playerList = new List<Player>();
            playerList.Add(new Player(Color.Blue, true, true));
            playerList.Add(new Player(Color.Purple, true, false));
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
                    if (CheckPointCircleCollision(new Vector2(currentMouseState.X, currentMouseState.Y), node.Position, node.CalcNodeRadius()))
                    {
                        //mouse is clicking a node
                        Node selectedNode = getSelectedNode();

                        //check what we should do with the clicked node
                        if (selectedNode == null)
                        {
                            if (node.OwnerId == humanOwnerId)
                            {
                                node.Selected = true;
                            }
                        }
                        else
                        {
                            if (selectedNode != node)
                            {
                                spawnUnits(selectedNode, node);
                                selectedNode.Selected = false;
                            }
                            else
                            {
                                selectedNode.Selected = false;
                            }
                        }

                        //no need to check the other nodes for clicks, so break the loop
                        break;
                    }
                }
            }

            

            //store mouse state so that we can check against it next frame
            previousMouseState = currentMouseState;
        }

        private void ProcessAI()
        {
            foreach (Player player in playerList)
            {
                if (!player.IsHuman)
                {
                    foreach (Node node in nodeList)
                    {
                        if (node.OwnerId != -1)
                        {
                            if (player == playerList[node.OwnerId] && node.UnitCount > 50)
                            {
                                spawnUnits(node, getWeakestEnemyNode(player));
                            }
                        }
                    }
                }
            }
        }

        private Node getWeakestEnemyNode(Player player)
        {
            Node output = null;
            foreach (Node node in nodeList)
            {
                if (node.OwnerId == -1 || playerList[node.OwnerId] != player)
                {
                    if (output == null || output.UnitCount > node.UnitCount)
                    {
                        output = node;
                    }
                }
            }
            return output;
        }


        private void UpdateUnits()
        {
            switch (pathfindingMethod)
            {
                case 0:
                    potentialFieldPaths();
                    break;
                case 1:
                    kineticPotentialPaths();
                    break;
            }
            


            
            List<Unit> attackList = new List<Unit>();

            //check collision with destination
            foreach (Unit unit in unitList)
            {
                Node destination = nodeList[unit.DestinationId];

                if (CheckPointCircleCollision(unit.Position, destination.Position, destination.CalcNodeRadius()))
                {
                    attackList.Add(unit);
                }
            }

            //deal with nodes that have reached their destination
            foreach (Unit unit in attackList)
            {
                attackNode(unit, nodeList[unit.DestinationId]);
            }


        }

        private void potentialFieldPaths()
        {
        }

        private void kineticPotentialPaths()
        {
            foreach (Unit unit in unitList)
            {

                Node destination = nodeList[unit.DestinationId];

                //add friction
                unit.Velocity *= unitFriction;

                //add attractive force to destination to velocity
                Vector2 direction = new Vector2(destination.Position.X - unit.Position.X, destination.Position.Y - unit.Position.Y);
                direction.Normalize();
                unit.Velocity += direction * unitAccel;


                //add repulsive force from other nodes to velocity
                foreach (Node node in nodeList)
                {
                    //don't repel from destination
                    if (node != destination)
                    {
                        float distanceSquared = (node.Position - unit.Position).LengthSquared();
                        float nodeRadius = node.CalcNodeRadius();

                        //first find nearby nodes
                        if (distanceSquared < (unitRepulsionLimit + nodeRadius) * (unitRepulsionLimit + nodeRadius))
                        {
                            //find the direction of repulsion and normalize
                            direction = new Vector2(unit.Position.X - node.Position.X, unit.Position.Y - node.Position.Y);
                            direction.Normalize();


                            if (distanceSquared < nodeRadius * nodeRadius)
                            {
                                //bounce if colliding
                                unit.Velocity += direction * 1000f;
                            }
                            else
                            {
                                //repel if not colliding
                                unit.Velocity += direction * unitRepulsionConstant / (distanceSquared - nodeRadius * nodeRadius);
                            }
                        }
                    }
                }


                //make sure velocity doesn't go above the maximum
                if (unit.Velocity.LengthSquared() > maxUnitVelocitySquared)
                {
                    Vector2 dir = unit.Velocity;
                    dir.Normalize();
                    unit.Velocity = dir * maxUnitVelocity;
                }

                //update position
                unit.Position += unit.Velocity;
            }
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
                float radius = node.CalcNodeRadius();
                float nodeScale = radius/300.0f;

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
            foreach (Unit unit in unitList)
            {

                //draw circle
                float radius = 5;
                float nodeScale = radius / 300.0f;
                Texture2D nodeTexture = DrawCircle((int)Math.Round(radius));

                spriteBatch.Draw(circle300, unit.Position, null, GetPlayerColor(unit.OwnerId), 0.0f, new Vector2(300, 300), nodeScale, SpriteEffects.None, 0);

                

            }
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
                if (CheckPointCircleCollision(new Vector2(currentMouseState.X, currentMouseState.Y), node.Position, node.CalcNodeRadius()))
                {
                    //mouse is over a node
                    if (node.OwnerId == humanOwnerId)
                    {
                        //node belongs to player, so border the node
                        DrawNodeBorder(node, Color.White, 2);

                    }
                    else if (selectedNode != null && node.OwnerId != humanOwnerId)
                    {
                        //node doesn't belong to player but player has already selected a node, so border the node
                        DrawNodeBorder(node, Color.White, 2);
                    }
                }

                if (node.Selected == true)
                {
                    DrawNodeBorder(node, Color.White, 2);
                }
            }


        }

        private void DrawFX()
        {

        }

        #endregion


        #region Other methods



        #region Geometry and collisions

        public bool CheckPointCircleCollision(Vector2 point, Vector2 circle, float radius)
        {
            float distanceSquared = (point.X - circle.X) * (point.X - circle.X) + (point.Y - circle.Y) * (point.Y - circle.Y);

            if ((radius * radius) >= distanceSquared)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion



        #region Graphics

        
        /// <summary>
        /// Draws a line between two points
        /// credit: http://www.xnawiki.com/index.php?title=Drawing_2D_lines_without_using_primitives
        /// </summary>
        /// <param name="blank">A 1x1 white texture</param>
        /// <param name="width">Thickness of the line (pixels)</param>
        /// <param name="color">Color of the line</param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        void DrawLine(Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(blank, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }


        /// <summary>
        /// Returns a texture containing a circle of white pixels
        /// </summary>
        /// <param name="radius">Radius of the circle</param>
        public Texture2D DrawCircle(float radius)
        {

            int boxsize = (int)Math.Round(radius * 2) + 2;
            Texture2D texture = new Texture2D(gDevice, boxsize, boxsize);

            Color[] data = new Color[boxsize * boxsize];

            Vector2 circleCentre = new Vector2(radius + 1, radius + 1);

            //test each pixel
            for (int i = 0; i < data.Length; i++)
            {
                if (CheckPointCircleCollision(new Vector2(i % boxsize, (i - i % boxsize) / boxsize), circleCentre, (float)radius))
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


        /// <summary>
        /// Returns a texture containing a circular ring of white pixels
        /// </summary>
        /// <param name="radius">Radius of the ring</param>
        /// <param name="thickness">Thickness of the ring</param>
        public Texture2D DrawCircleBorder(float radius, float thickness)
        {

            int boxsize = (int)Math.Round(radius * 2) + 2;
            Texture2D texture = new Texture2D(gDevice, boxsize, boxsize);

            Color[] data = new Color[boxsize * boxsize];

            Vector2 circleCentre = new Vector2(radius + 1, radius + 1);

            //test each pixel
            for (int i = 0; i < data.Length; i++)
            {
                if (CheckPointCircleCollision(new Vector2(i % boxsize, (i - i % boxsize) / boxsize), circleCentre, radius + thickness / 2) &&
                    !CheckPointCircleCollision(new Vector2(i % boxsize, (i - i % boxsize) / boxsize), circleCentre, radius - thickness / 2))
                {
                    //if pixel is in the outer circle but not in the inner circle, colour it
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


        /// <summary>
        /// Draws a border around a given node
        /// </summary>
        /// <param name="node">The node to draw a border around</param>
        /// <param name="color">The color of the border</param>
        /// <param name="thickness">The thickness of the border</param>
        private void DrawNodeBorder(Node node, Color color, float thickness)
        {
            float radius = node.CalcNodeRadius();
            Texture2D border = DrawCircleBorder(radius, thickness);

            spriteBatch.Draw(border, node.Position, null, color, 0.0f, new Vector2(radius + 1, radius + 1), 1, SpriteEffects.None, 0);
        }

        #endregion




        /// <summary>
        /// Spawns units to travel from one node to another
        /// </summary>
        /// <param name="sourceNode">The node to spawn units at</param>
        /// <param name="destinationNode">The destination node for the units</param>
        private void spawnUnits(Node sourceNode, Node destinationNode)
        {
            if (sourceNode != null && destinationNode != null && sourceNode.UnitCount > 1)
            {
                //calculate how many units to send
                int numUnits = (int)Math.Round(sourceNode.UnitCount * attackProportion);

                //subtract the units from the node
                sourceNode.UnitCount -= numUnits;

                //spawn units in a circle slightly inside the spawning node
                float radius = sourceNode.CalcNodeRadius() - 5;

                //add the units to the active units list
                for (int i = 0; i < numUnits; i++)
                {
                    //work out a random place on the circle to spawn this unit on
                    float angle = (float)(r.NextDouble() * Math.PI * 2);
                    float relativeX = (float)(radius * Math.Cos(angle));
                    float relativeY = (float)(radius * Math.Sin(angle));
                    float x = sourceNode.Position.X + relativeX;
                    float y = sourceNode.Position.Y + relativeY;

                    //add an initial velocity directly away from the source node
                    float xVel = relativeX * unitStartVelocity;
                    float yVel = relativeY * unitStartVelocity;

                    //make the unit and add it to the list
                    Unit newUnit = new Unit(sourceNode.OwnerId, new Vector2(x, y), new Vector2(xVel, yVel), getNodeId(destinationNode));
                    unitList.Add(newUnit);
                }
            }

        }


        /// <summary>
        /// Handles the logic for when a unit reaches its destination
        /// </summary>
        /// <param name="attackingUnit">The unit</param>
        /// <param name="defendingNode">The destination node</param>
        private void attackNode(Unit attackingUnit, Node defendingNode)
        {
            unitList.Remove(attackingUnit);

            if (attackingUnit.OwnerId != defendingNode.OwnerId)
            {
                if (defendingNode.UnitCount == 1)
                {
                    defendingNode.OwnerId = attackingUnit.OwnerId;
                }
                else
                {
                    defendingNode.UnitCount -= 1;
                }
            }
            else
            {
                defendingNode.UnitCount += 1;
            }
        }


        /// <summary>
        /// Returns a node's position in nodeList given the node object (uses node's x and y to identify it)
        /// </summary>
        /// <param name="node">Node to identify.</param>
        public int getNodeId(Node node)
        {
            int id = nodeList.FindIndex(
                delegate(Node comparedNode) {
                    return (comparedNode.Position.X == node.Position.X && comparedNode.Position.Y == node.Position.Y); 
                }
            );

            return id;
        }


        /// <summary>
        /// Returns the first node in nodeList whose Selected field = true
        /// </summary>
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


        /// <summary>
        /// Returns a player color given the player's ID
        /// </summary>
        /// <param name="playerId">ID of player to return color of</param>
        private Color GetPlayerColor(int playerId)
        {
            if (playerId >= 0)
            {
                return playerList[playerId].Color;
            }
            else
            {
                return NeutralColor;
            }
        }

        

        #endregion


    }
}

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
        int screenHeight = 700;

        string windowTitle = "Nodes";

        bool drawDebug = true;

        Color NeutralColor = new Color(100, 100, 100, 255);

       


        SpriteFont Trebuchet;
        Texture2D circle50;
        Texture2D circle300;
        Texture2D blank;


        List<Node> nodeList;
        List<Player> playerList;
        List<Unit> unitList;


        int humanOwnerId = 0;
        float attackProportion = 0.5f; //proportion of units that will be sent from an attacking node
        


        // ----------PATHFINDING-------------
        float unitStartVelocity = 0.3f;
        float maxUnitVelocity = 3;
        float maxUnitVelocitySquared;


        /*
         * 0 -> potential field
         * 1 -> kinetic potential
         * 2 -> kinetic steering
         * 3 -> A*
         */
        int pathfindingMethod = 3;


        //--potential field
        float unitDestinationAccel = 0.15f; //acceleration of the units towards their destination
        float unitFriction = 0.995f;


        //--kinetic potential
        float unitDestinationWeighting = 0.15f; //weighting of attraction to destination compared to repulsion from obstacles (potential field)


        //--potential steering
        Vector2[] relativeTestPoints = new Vector2[3];
        float steerForce = 0.2f;
        float brakeForce = 1;


        //--A star
        int gridResolution = 35;
        float nodeGraphLeeway = 5;
        float graphLinkMaxLength = 100;
        List<GraphPoint> navGraph;
        List<NavigationPath> navPaths;


        //--multiple systems
        float unitRepulsionLimit = 100; //added to node radius when calculating repulsion in kinetic potential and potential field systems
        float unitRepulsionConstant = 250f; //weighting for repulsion from objects (potential field) (kinetic potential)


        List<NavigationPoint> debuglist;


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

            //get level data
            //TODO


            //load level data into variables

            nodeList = new List<Node>();
            nodeList.Add(new Node(new Vector2(200, 400), 50, 0, 0.01f, r));
            nodeList.Add(new Node(new Vector2(800, 200), 20, 1, 0.01f, r));
            nodeList.Add(new Node(new Vector2(900, 600), 9, 2, 0.02f, r));
            nodeList.Add(new Node(new Vector2(300, 200), 15, 0, 0.01f, r));
            nodeList.Add(new Node(new Vector2(600, 350), 40, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(300, 800), 27, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(550, 120), 32, -1, 0.005f, r));
            nodeList.Add(new Node(new Vector2(700, 500), 37, 0, 0.005f, r));

            playerList = new List<Player>();
            playerList.Add(new Player(Color.Blue, true, true));
            playerList.Add(new Player(Color.Purple, true, false));
            playerList.Add(new Player(Color.Green, true, false));

            unitList = new List<Unit>();

            relativeTestPoints[0] = new Vector2(75, 0);
            relativeTestPoints[1] = new Vector2(25, 20);
            relativeTestPoints[2] = new Vector2(25, -20);


            navGraph = createNavGraph();
            navPaths = new List<NavigationPath>();
            

            //enumerate through any components and initialize them
            base.Initialize();
        }

        private List<GraphPoint> createNavGraph()
        {
            List<GraphPoint> graph = new List<GraphPoint>();

            //first create a grid of graph points covering the whole space
            for (int x = 0; x < screenWidth; x++)
            {
                if (x % gridResolution == 0)
                {
                    for (int y = 0; y < screenHeight; y++)
                    {
                        if (y % gridResolution == 0)
                        {
                            int xPos = (x + 1) * gridResolution;
                            int yPos = (y + 1) * gridResolution;

                            Vector2 pos = new Vector2(x, y);

                            GraphPoint newPoint = new GraphPoint();
                            newPoint.Position = pos;
                            graph.Add(newPoint);
                        }
                        if (y >= screenHeight)
                        {
                            break;
                        }
                    }
                    
                }

                if (x >= screenWidth)
                {
                    break;
                }

            }

            //cull any points that collide with nodes (or may do in the future)
            List<GraphPoint> invalidPoints = new List<GraphPoint>();
            foreach (GraphPoint point in graph)
            {
                bool spaceFree = true;
                foreach (Node node in nodeList)
                {
                    if (CheckPointCircleCollision(point.Position, node.Position, 75 + nodeGraphLeeway))
                    {
                        spaceFree = false;
                        break;
                    }
                }

                if (!spaceFree)
                {
                    invalidPoints.Add(point);
                }

            }
            foreach (GraphPoint point in invalidPoints)
            {
                graph.Remove(point);
            }


            //insert valid connections between nodes
            foreach (GraphPoint point1 in graph)
            {
                bool finished = false;

                foreach (GraphPoint point2 in graph)
                {
                    if (point1 != point2)
                    {
                        //reject nodes too far away
                        if (!((point1.Position - point2.Position).LengthSquared() > graphLinkMaxLength * graphLinkMaxLength))
                        {
                            bool linkValid = true;
                            
                            //check the link wouldn't go through a node
                            foreach (Node node in nodeList)
                            {
                                if (checkLineCircleCollision(point1.Position, point2.Position, node.Position, 75.0f + nodeGraphLeeway))
                                {
                                    linkValid = false;
                                }
                            }
                            
                            if (linkValid)
                            {
                                point1.connectedNodes.Add(point2);
                            }

                        }

                    }

                    
                }

                if (finished)
                {
                    break;
                }
            }

            return graph;
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
                            if (selectedNode != node && selectedNode.OwnerId == humanOwnerId)
                            {
                                spawnUnits(selectedNode, node);
                            }
                            selectedNode.Selected = false;
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
                case 2:
                    kineticSteeringPaths();
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
            foreach (Unit unit in unitList)
            {

                Node destination = nodeList[unit.DestinationId];

                //add attractive force to destination to velocity
                Vector2 direction = new Vector2(destination.Position.X - unit.Position.X, destination.Position.Y - unit.Position.Y);
                direction.Normalize();
                unit.Velocity = direction * unitDestinationWeighting;


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


                //normalize velocity
                Vector2 dir = unit.Velocity;
                dir.Normalize();
                unit.Velocity = dir * maxUnitVelocity;

                //update position
                unit.Position += unit.Velocity;
            }
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
                unit.Velocity += direction * unitDestinationAccel;


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


        private void kineticSteeringPaths()
        {
            foreach (Unit unit in unitList)
            {

                Node destination = nodeList[unit.DestinationId];

                //add friction
                unit.Velocity *= unitFriction;

                //add attractive force to destination to velocity
                Vector2 direction = new Vector2(destination.Position.X - unit.Position.X, destination.Position.Y - unit.Position.Y);
                direction.Normalize();
                unit.Velocity += direction * unitDestinationAccel;

                //find the bearing of the unit's velocity (clockwise from x axis)
                float bearing = getAngleFromHoriz(unit.Velocity);

                //find the points to test against
                Vector2[] testPoints = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    testPoints[i] = rotateVector2(relativeTestPoints[i], bearing);
                    testPoints[i] += unit.Position;
                }


                bool avoidanceMode = false;

                foreach (Vector2 testPoint in testPoints)
                {
                    foreach (Node node in nodeList)
                    {
                        //for each node, check if each test point is colliding with it
                        if (destination != node && CheckPointCircleCollision(testPoint, node.Position, node.CalcNodeRadius()))
                        {
                            //one of our test points is colliding with a node
                            avoidanceMode = true;

                            //brake
                            unit.Velocity *= brakeForce;

                            //find which way to steer
                            

                            bool steerLeft = isLeft(unit.Position, unit.Velocity + unit.Position, node.Position);


                            Vector2 steerVector;

                            //steer
                            if (steerLeft)
                            {
                                steerVector = rotateVector2(unit.Velocity, (float)-Math.PI / 2);
                            }
                            else
                            {
                                steerVector = rotateVector2(unit.Velocity, (float)Math.PI / 2);
                            }

                            steerVector.Normalize();
                            unit.Velocity += steerVector * steerForce;

                            break;
                        }
                    }
                    if (avoidanceMode)
                    {
                        break;
                    }
                }

                /*
                //bounce off nodes
                foreach (Node node in nodeList)
                {
                    //don't repel from destination
                    if (node != destination)
                    {
                        direction = new Vector2(unit.Position.X - node.Position.X, unit.Position.Y - node.Position.Y);

                        float distanceSquared = (node.Position - unit.Position).LengthSquared();
                        float nodeRadius = node.CalcNodeRadius();

                        if (distanceSquared < nodeRadius * nodeRadius)
                        {
                            //bounce if colliding
                            unit.Velocity += direction * 1000f;
                        }
                        
                    }
                }*/
                

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

        private void buildAStarPath(Node source, Node destination)
        {
            if (getAStarPath(source, destination) == null)
            {
                //add the source and destination to the graph
                addNodeToNav(source);
                addNodeToNav(destination);


                /*
                http://www.policyalmanac.org/games/aStarTutorial.htm
                1) Add the starting square (or node) to the open list.

                2) Repeat the following:

                    a) Look for the lowest F cost square on the open list. We refer to this as the current square.

                    b) Switch it to the closed list.

                    c) For each of the 8 squares adjacent to this current square …

                            If it is not walkable or if it is on the closed list, ignore it. Otherwise do the following.           

                            If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. Record the F, G, and H costs of the square. 

                            If it is on the open list already, check to see if this path to that square is better, using G cost as the measure. A lower G cost means that this is a better path. If so, change the parent of the square to the current square, and recalculate the G and F scores of the square. If you are keeping your open list sorted by F score, you may need to resort the list to account for the change.

                    d) Stop when you:

                            Add the target square to the closed list, in which case the path has been found (see note below), or
                            Fail to find the target square, and the open list is empty. In this case, there is no path. 
                 
                3) Save the path. Working backwards from the target square, go from each square to its parent square until you reach the starting square. That is your path. 
                */

                GraphPoint start = findClosestGraphPoint(source.Position, 75 + nodeGraphLeeway + 15);
                GraphPoint end = findClosestGraphPoint(destination.Position, 75 + nodeGraphLeeway + 15);

                List<NavigationPoint> openList = new List<NavigationPoint>();
                List<NavigationPoint> closedList = new List<NavigationPoint>();

                
                //1) Add the starting square (or node) to the open list.
                NavigationPoint newPoint = new NavigationPoint();
                newPoint.graphRef = start;
                newPoint.Parent = null;
                newPoint.G = 0;

                openList.Add(newPoint);

                bool pathFound = false;
                NavigationPoint currentPoint = null;

                while (!pathFound)
                {
                    float lowestF = 10000000;

                    // a) Look for the lowest F cost square on the open list. We refer to this as the current square.
                    for (int i = 0; i < openList.Count; i++)
                    {
                        if (openList[i].F < lowestF)
                        {
                            currentPoint = openList[i];
                            lowestF = currentPoint.F;
                        }
                    }

                    //b) Switch it to the closed list.
                    openList.Remove(currentPoint);
                    closedList.Add(currentPoint);


                    //Stop when you:
                    //        Add the target square to the closed list, in which case the path has been found
                    if (currentPoint.graphRef == end)
                    {
                        pathFound = true;
                        break;
                    }

                    foreach (GraphPoint p in currentPoint.graphRef.connectedNodes)
                    {
                        //  If it is not walkable or if it is on the closed list, ignore it. Otherwise do the following.   
                        bool ignore = false;
                        foreach (NavigationPoint n in closedList)
                        {
                            if (n.graphRef == p)
                            {
                                ignore = true;
                                break;
                            }
                        }

                       // if (ignore)
                         //   break;


                        //If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. Record the F, G, and H costs of the square. 
                        NavigationPoint openListPoint = null;
                        foreach (NavigationPoint n in openList)
                        {
                            if (n.graphRef == p)
                            {
                                openListPoint = n;
                                break;
                            }
                        }

                        if (openListPoint == null)
                        {
                            NavigationPoint newP = new NavigationPoint();
                            newP.graphRef = p;
                            newP.Parent = currentPoint;
                            newP.G = (newP.graphRef.Position - newP.Parent.graphRef.Position).Length() + newP.Parent.G;

                            newP.H = (newP.graphRef.Position - destination.Position).Length();
                            newP.F = newP.G + newP.H;
                            openList.Add(newP);
                        }
                        else
                        {
                            if (openListPoint.G > (openListPoint.graphRef.Position - currentPoint.graphRef.Position).Length()  + currentPoint.G)
                            {
                                openListPoint.Parent = currentPoint;
                                openListPoint.G = (openListPoint.graphRef.Position - openListPoint.Parent.graphRef.Position).Length()  + openListPoint.Parent.G;

                                openListPoint.H = (openListPoint.graphRef.Position - destination.Position).Length();

                                openListPoint.F = openListPoint.G + openListPoint.H;
                            }
                        }



                    }

                    if (openList.Count == 0)
                    {
                        break;
                    }


                    
                }

                if (pathFound)
                {
                    NavigationPath newPath = walkPath(currentPoint, getNodeId(source), getNodeId(destination));
                    navPaths.Add(newPath);
                }


                //remove the source and destination from the graph
                removeNodeFromNav(source);
                removeNodeFromNav(destination);


                debuglist = closedList;
            }
        }

        private void removeNodeFromNav(Node node)
        {
            GraphPoint nodePoint = new GraphPoint();

            foreach (GraphPoint gp in navGraph)
            {
                if (gp.Position == node.Position)
                {
                    nodePoint = gp;
                    break;
                }
            }

            for (int i = 0; i < navGraph.Count; i++ )
            {
                for (int j = 0; j < navGraph[i].connectedNodes.Count; j++)
                {
                    if (navGraph[i].connectedNodes[j].Position == nodePoint.Position)
                    {
                        navGraph[i].connectedNodes.Remove(navGraph[i].connectedNodes[j]);
                    }
                }
            }

            navGraph.Remove(nodePoint);
        }


        private void addNodeToNav(Node node)
        {
            GraphPoint nodePoint = new GraphPoint();
            nodePoint.Position = node.Position;
            
            foreach (GraphPoint gp in navGraph)
            {
                if ((gp.Position - node.Position).LengthSquared() < (75 + nodeGraphLeeway + 50) * (75 + nodeGraphLeeway + 50))
                {
                    gp.connectedNodes.Add(nodePoint);
                    nodePoint.connectedNodes.Add(gp);
                }
            }

            navGraph.Add(nodePoint);
        }

        private NavigationPath walkPath(NavigationPoint lastPoint, int sourceId, int destId)
        {
            NavigationPath path = new NavigationPath();
            path.startNodeId = sourceId;
            path.endNodeId = destId;

            NavigationPoint currentPoint = lastPoint;
            while (currentPoint != null)
            {
                path.Points.Add(currentPoint.graphRef.Position);
                currentPoint = currentPoint.Parent;
            }

            return path;
        }

        private NavigationPath getAStarPath(Node source, Node destination)
        {
            foreach (NavigationPath path in navPaths)
            {
                Node startNode = nodeList[path.startNodeId];
                Node endNode = nodeList[path.endNodeId];

                //check if start and end nodes match
                if (source.Position== startNode.Position && endNode.Position == destination.Position)
                {
                    return path;
                }
            }
            return null;
        }

        private float getAngleFromHoriz(Vector2 vec)
        {

            return (float)Math.Atan2(vec.Y, vec.X);

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

            if (drawDebug)
            {
                DrawDebug();
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        private void DrawBackground()
        {

        }

        private void DrawNodes()
        {
            foreach (Node node in nodeList)
            {

                //draw circle
                float radius = node.CalcNodeRadius();
                float nodeScale = radius / 300.0f;

                spriteBatch.Draw(circle300, node.Position, null, GetPlayerColor(node.OwnerId), 0.0f, new Vector2(300, 300), nodeScale, SpriteEffects.None, 0);

                //draw unit count
                Vector2 textSize = Trebuchet.MeasureString(node.UnitCount.ToString());

                Color textColor = Color.White;
                if (node.Selected)
                {
                    textColor = new Color(150, 150, 150);
                }

                spriteBatch.DrawString(Trebuchet, node.UnitCount.ToString(), node.Position - textSize / 2, textColor);

            }
        }

        private void DrawUnits()
        {
            foreach (Unit unit in unitList)
            {

                //draw circle
                float radius = 5;
                float nodeScale = radius / 300.0f;
                Texture2D point = DrawCircle(1);

                spriteBatch.Draw(circle300, unit.Position, null, GetPlayerColor(unit.OwnerId), 0.0f, new Vector2(300, 300), nodeScale, SpriteEffects.None, 0);

                if (pathfindingMethod == 2 && drawDebug == true)
                {
                    float bearing = getAngleFromHoriz(unit.Velocity);

                    Vector2[] testPoints = new Vector2[3];
                    for (int i = 0; i < 3; i++)
                    {
                        testPoints[i] = rotateVector2(relativeTestPoints[i], bearing);
                        testPoints[i] += unit.Position;
                    }

                    foreach (Vector2 v in testPoints)
                    {
                        spriteBatch.Draw(point, v, null, Color.White, 0.0f, new Vector2(1, 1), 1, SpriteEffects.None, 0);
                    }
                }


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


        private void DrawDebug()
        {
            if (pathfindingMethod == 3)
            {
                foreach (GraphPoint point in navGraph)
                {
                    foreach (GraphPoint point2 in point.connectedNodes)
                    {
                        DrawLine(blank, 0.5f, new Color(200,200,255), point.Position, point2.Position);
                    }
                }


                foreach (NavigationPath path in navPaths)
                {
                    for (int i = 0; i < path.Points.Count - 1; i++)
                    {
                        DrawLine(blank, 1, Color.Red, path.Points[i], path.Points[i + 1]);
                    }
                }

                
                if (debuglist != null)
                {
                    foreach (NavigationPoint p in debuglist)
                    {

                        Texture2D point = DrawCircle(2);
                        spriteBatch.Draw(point, p.graphRef.Position, null, Color.Red, 0.0f, new Vector2(2, 2), 1, SpriteEffects.None, 0);
                    }
                }
            }
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
            //check source is valid and has enough units to send
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
                    Unit newUnit = new Unit(sourceNode.OwnerId, new Vector2(x, y), new Vector2(xVel, yVel), getNodeId(destinationNode), getNodeId(sourceNode));
                    unitList.Add(newUnit);
                }

                if (pathfindingMethod == 3)
                {
                    buildAStarPath(sourceNode, destinationNode);
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
                delegate(Node comparedNode)
                {
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

        /// <summary>
        /// Rotates a 2d vector clockwise around the origin given an angle
        /// </summary>
        /// <param name="vector">Vector to rotate</param>
        /// <param name="angleInRadians">Angle to rotate through</param>
        private Vector2 rotateVector2(Vector2 vector, float angleInRadians)
        {

            float c = (float)Math.Cos(angleInRadians);
            float s = (float)Math.Sin(angleInRadians);

            float x = vector.X * c - vector.Y * s;
            float y = vector.X * s + vector.Y * c;

            return new Vector2(x, y);
        }

      

        // credit: http://stackoverflow.com/questions/3461453/determine-which-side-of-a-line-a-point-lies
        public bool isLeft(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }


        public bool checkLineCircleCollision(Vector2 point1, Vector2 point2, Vector2 circlePos, float radius)
        {
            float distance = calcPointLineDistance(point1, point2, circlePos);

            if (distance <= radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public float calcPointLineDistance(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {
            //get length along line to take a perpendicular from
            float u = ((point.X - linePoint1.X) * (linePoint2.X - linePoint1.X) + (point.Y - linePoint1.Y) * (linePoint2.Y - linePoint1.Y)) / ((linePoint2 - linePoint1).Length() * (linePoint2 - linePoint1).Length());

            if (u <= 0)
            {
                //point before start of line segement
                return (point - linePoint1).Length();
            }
            else if (u >= 1)
            {
                //point after end of line segment
                return (point - linePoint2).Length();
            }
            else
            {
                //get point on line closest to point
                Vector2 p = linePoint1 + u * (linePoint2 - linePoint1);

                //get distance
                return (p - point).Length();
            }
        }


        public GraphPoint findClosestGraphPoint(Vector2 pos, float boundBoxRadius)
        {
            Vector2 lowerBound = new Vector2(pos.X - boundBoxRadius, pos.Y - boundBoxRadius);
            Vector2 upperBound = new Vector2(pos.X + boundBoxRadius, pos.Y + boundBoxRadius);

            GraphPoint bestMatch = null;
            float bestDistanceSquared = boundBoxRadius * 100;

            foreach (GraphPoint point in navGraph)
            {
                //check point is in the boundingbox
                if (point.Position.X >= lowerBound.X && point.Position.Y >= lowerBound.Y && point.Position.X <= upperBound.X && point.Position.Y <= upperBound.Y)
                {
                    //check if point is closer than current best match
                    if ((point.Position - pos).LengthSquared() < bestDistanceSquared)
                    {
                        bestMatch = point;
                        bestDistanceSquared = (point.Position - pos).LengthSquared();

                        if (bestDistanceSquared == 0)
                        {
                            return bestMatch;
                        }
                    }
                }
            }

            //if no points found in the boundingbox, repeat but with a bigger bounding box
            if (bestMatch == null)
            {
                return findClosestGraphPoint(pos, boundBoxRadius * 1.5f);
            }
            else
            {
                return bestMatch;
            }
        }
        
        #endregion


    }
}

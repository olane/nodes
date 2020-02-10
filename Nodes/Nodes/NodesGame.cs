using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        int screenWidth = 1366;
        int screenHeight = 768;

        string windowTitle = "Nodes";

        bool drawDebug = false;


        Color NeutralColor = new Color(100, 100, 100, 255);
        float NeutralGrowthRate = 0.0005f;


        SpriteFont Trebuchet;
        Texture2D circle50;
        Texture2D circle300;
        Texture2D blank;


        static List<Node> nodeList;
        static List<Player> playerList;
        static List<Unit> unitList;

        int currentLevel = 0;
        List<List<Node>> levelData = new List<List<Node>>();
        List<List<Player>> playerData = new List<List<Player>>();



        int humanOwnerId = 0;
        float attackProportion = 0.5f; //proportion of units that will be sent from an attacking node



        // --------------------PATHFINDING-----------------------
        float unitStartVelocity = 0.4f;
        float maxUnitVelocity = 3;
        float maxUnitVelocitySquared;


        /*
         * 0 -> potential field
         * 1 -> kinetic potential
         * 2 -> kinetic steering
         * 3 -> A*
         * 4 -> A* steering
         */
        int pathfindingMethod = 1;


        //--potential field
        float unitDestinationAccel = 0.15f; //acceleration of the units towards their destination
        float unitFriction = 0.995f;


        //--kinetic potential
        float unitDestinationWeighting = 0.15f; //weighting of attraction to destination compared to repulsion from obstacles (potential field)


        //--potential steering
        Vector2[] relativeTestPoints = new Vector2[3];
        float steerForce = 0.5f;
        float brakeForce = 1;


        //--A star
        int gridResolution = 35;
        float nodeGraphLeeway = 5;
        float graphLinkMaxLength = 100;
        List<GraphPoint> navGraph;
        List<NavigationPath> navPaths;


        //--A star steering
        float pathSteeringTargetDistance = 80f;
        float pathSteeringErrorMargin = 15;
        float pathSteeringForce = 0.2f;


        //--multiple systems
        float unitRepulsionLimit = 100; //added to node radius when calculating repulsion in kinetic potential and potential field systems
        float unitRepulsionConstant = 250f; //weighting for repulsion from objects (potential field) (kinetic potential)


        List<NavigationPoint> debuglist;

        //----------------END PATHFINDING-----------------



        //---------------------AI--------------------------

        /*
         * 0 -> Simple (50 or more attacks smallest node)
         * 1 -> Decision Tree
         * 2 -> Fuzzy Logic
         */
        int AIMethod = 2;


        int AIUpdatePeriod = 30;

        //-------------------END AI------------------------


        MouseState previousMouseState; //holds last frame's mouse state
        MouseState currentMouseState;  //holds this frame's mouse state

        KeyboardState previousKeyboardState; //holds last frame's keyboard state
        KeyboardState currentKeyboardState;  //holds this frame's keyboard state


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

            #region level data

            //============LEVEL 1=================
            levelData.Add(new List<Node>());
            levelData[0].Add(new Node(new Vector2(200, 400), 50, 0, r));
            levelData[0].Add(new Node(new Vector2(800, 200), 20, 1, r));
            levelData[0].Add(new Node(new Vector2(900, 600), 40, 1, r));
            levelData[0].Add(new Node(new Vector2(300, 200), 15, 1, r));
            levelData[0].Add(new Node(new Vector2(600, 350), 10, -1, r));
            levelData[0].Add(new Node(new Vector2(300, 550), 27, 2, r));
            levelData[0].Add(new Node(new Vector2(550, 120), 13, -1, r));
            levelData[0].Add(new Node(new Vector2(700, 500), 37, 0, r));

            playerData.Add(new List<Player>());
            playerData[0].Add(new Player(Color.Blue, true, true, 0.01f));
            playerData[0].Add(new Player(Color.Purple, true, false, 0.01f));
            playerData[0].Add(new Player(Color.Green, true, false, 0.01f));

            //============LEVEL 2=================
            levelData.Add(new List<Node>());
            levelData[1].Add(new Node(new Vector2(100, 350), 10, 0, r));
            levelData[1].Add(new Node(new Vector2(200, 500), 10, 1, r));
            levelData[1].Add(new Node(new Vector2(380, 300), 10, 2, r));
            levelData[1].Add(new Node(new Vector2(530, 400), 5, 0, r));
            levelData[1].Add(new Node(new Vector2(690, 360), 1, -1, r));
            levelData[1].Add(new Node(new Vector2(900, 335), 2, -1, r));
            levelData[1].Add(new Node(new Vector2(1050, 350), 6, 0, r));

            playerData.Add(new List<Player>());
            playerData[1].Add(new Player(Color.Blue, true, true, 0.01f));
            playerData[1].Add(new Player(Color.Purple, true, false, 0.015f));
            playerData[1].Add(new Player(Color.Green, true, false, 0.02f));

            //============LEVEL 3=================
            levelData.Add(new List<Node>());
            levelData[2].Add(new Node(new Vector2(200, 100), 5, 0, r));
            levelData[2].Add(new Node(new Vector2(200, 300), 2, -1, r));
            levelData[2].Add(new Node(new Vector2(200, 500), 1, -1, r));
            levelData[2].Add(new Node(new Vector2(400, 100), 7, -1, r));
            levelData[2].Add(new Node(new Vector2(400, 300), 5, -1, r));
            levelData[2].Add(new Node(new Vector2(400, 500), 4, 1, r));
            levelData[2].Add(new Node(new Vector2(600, 100), 9, -1, r));
            levelData[2].Add(new Node(new Vector2(600, 300), 3, -1, r));
            levelData[2].Add(new Node(new Vector2(600, 500), 6, -1, r));
            levelData[2].Add(new Node(new Vector2(800, 100), 5, -1, r));
            levelData[2].Add(new Node(new Vector2(800, 300), 2, 2, r));
            levelData[2].Add(new Node(new Vector2(800, 500), 11, -1, r));
            levelData[2].Add(new Node(new Vector2(1000, 100), 2, -1, r));
            levelData[2].Add(new Node(new Vector2(1000, 300), 5, -1, r));
            levelData[2].Add(new Node(new Vector2(1000, 500), 4, 2, r));

            playerData.Add(new List<Player>());
            playerData[2].Add(new Player(Color.Blue, true, true, 0.01f));
            playerData[2].Add(new Player(Color.Sienna, true, false, 0.012f));
            playerData[2].Add(new Player(Color.Red, true, false, 0.011f));

            #endregion

            unitList = new List<Unit>();

            relativeTestPoints[0] = new Vector2(75, 0);
            relativeTestPoints[1] = new Vector2(25, 20);
            relativeTestPoints[2] = new Vector2(25, -20);

            setLevel(currentLevel);

            navGraph = createNavGraph();
            navPaths = new List<NavigationPath>();



            //enumerate through any components and initialize them
            base.Initialize();
        }

        private void setLevel(int levelId)
        {

            if (levelId < levelData.Count)
            {
                //playerList = playerData[levelId];

                nodeList = new List<Node>();

                levelData[levelId].ForEach((item) =>
                {
                    nodeList.Add((Node)item.Clone());
                });

                playerList = new List<Player>();

                playerData[levelId].ForEach((item) =>
                {
                    playerList.Add((Player)item.Clone());
                });


                currentLevel = levelId;



                unitList = new List<Unit>();

                navGraph = createNavGraph();
                navPaths = new List<NavigationPath>();
            }
            else
            {
                throw new Exception("Level does not exist");
            }
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
            UpdateUnits();
            UpdateNodes();
            CheckCollisions();
            UpdatePlayers();
            ProcessAI();


            base.Update(gameTime);
        }


        private void ProcessInput()
        {
            //process mouse
            currentMouseState = Mouse.GetState();

            //check if mouse is clicking
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {

                foreach (Node node in nodeList)
                {
                    if (CheckPointCircleCollision(new Vector2(currentMouseState.X, currentMouseState.Y), node.Position, node.CalcNodeRadius()))
                    {
                        //mouse is clicking over a node

                        //check what we should do with the clicked node
                        if (!node.Selected)
                        {
                            if (node.OwnerId == humanOwnerId)
                            {
                                node.Selected = true;
                            }
                        }

                        //no need to check the other nodes for clicks, so break the loop
                        break;
                    }
                }
            }

            //check if mouse just stopped clicking
            else if (previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton != ButtonState.Pressed)
            {
                List<Node> selectedNodes = getSelectedNodes();

                foreach (Node node in nodeList)
                {
                    if (CheckPointCircleCollision(new Vector2(currentMouseState.X, currentMouseState.Y), node.Position, node.CalcNodeRadius()))
                    {
                        //mouse stopped dragging over a node
                        Node targetNode = node;

                        foreach (Node selectedNode in selectedNodes)
                        {
                            if (targetNode != selectedNode)
                            {
                                if (selectedNode.OwnerId == humanOwnerId)
                                {
                                    spawnUnits(selectedNode, targetNode);
                                }
                            }
                        }


                        //no need to check the other nodes, so break the loop
                        break;
                    }
                }

                foreach (Node selectedNode in selectedNodes)
                {
                    selectedNode.Selected = false;
                }
            }

            //process keyboard
            currentKeyboardState = Keyboard.GetState();

            //if 'D' just got pressed toggle debug mode
            if (currentKeyboardState.IsKeyDown(Keys.D) && !previousKeyboardState.IsKeyDown(Keys.D))
            {
                drawDebug = !drawDebug;
            }

            //if 'N' just got pressed, go to next level
            if (currentKeyboardState.IsKeyDown(Keys.N) && !previousKeyboardState.IsKeyDown(Keys.N))
            {
                setLevel(currentLevel + 1);
            }

            //if 'B' just got pressed, go to previous level
            if (currentKeyboardState.IsKeyDown(Keys.B) && !previousKeyboardState.IsKeyDown(Keys.B))
            {
                if (currentLevel > 0)
                {
                    setLevel(currentLevel - 1);
                }
            }

            //if 'R' just got pressed, restart level
            if (currentKeyboardState.IsKeyDown(Keys.R) && !previousKeyboardState.IsKeyDown(Keys.R))
            {
                setLevel(currentLevel);
            }

            //store mouse/keyboard state so that we can check against it next frame
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;

        }

        private void ProcessAI()
        {
            foreach (Player player in playerList)
            {
                if (!player.IsHuman && player.IsAlive)
                {
                    if ((int)r.Next(1, AIUpdatePeriod) == 1)
                    {
                        switch (AIMethod)
                        {
                            case 0:
                                simpleAI(player);
                                break;
                            case 1:
                                decisionTreeAI(player);
                                break;
                            case 2: fuzzyLogicAI(player);
                                break;
                        }
                    }
                }
            }
        }

        private void simpleAI(Player player)
        {
            foreach (Node node in nodeList)
            {
                if (node.OwnerId != -1)
                {
                    if (player == playerList[node.OwnerId] && node.UnitCount > 50)
                    {
                        spawnUnits(node, getNetWeakestEnemyNode(player));
                    }
                }
            }
        }

        private Node getNetWeakestEnemyNode(Player player)
        {
            Node output = null;
            int smallestNum = 5000;

            foreach (Node node in nodeList)
            {
                if (node.OwnerId == -1 || playerList[node.OwnerId] != player)
                {
                    int unitNum = getNetUnitCount(node);

                    if (output == null || smallestNum > unitNum)
                    {
                        output = node;
                        smallestNum = unitNum;
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// Returns a list of the n smallest enemy nodes by net size
        /// </summary>
        /// <param name="player">'Friendly' player</param>
        /// <param name="number">Target number of nodes to return (may return more or less if nodes tie in size or there are less enemy nodes than 'number')</param>
        private List<Node> getNetWeakestEnemyNodes(Player player, int number)
        {
            List<Node> output = new List<Node>();
            List<Node> enemyNodes = new List<Node>();

            //get enemy nodes
            foreach (Node node in nodeList)
            {
                if (node.OwnerId == -1 || playerList[node.OwnerId] != player)
                {
                    enemyNodes.Add(node);
                }
            }
            if (enemyNodes.Count == 0)
            {
                return null;
            }

            //return the whole list if it's the right length
            if (enemyNodes.Count == number)
            {
                return enemyNodes;
            }

            //check we aren't trying to return more nodes than are present.
            if (enemyNodes.Count < number)
            {
                number = enemyNodes.Count;
            }

            //get nth smallest enemy node
            Node nthSmallest = getNthNetSmallestNode(enemyNodes, number);

            int nthSmallestCount = getNetUnitCount(nthSmallest);
            //get nodes smaller than nthSmallest

            foreach (Node node in enemyNodes)
            {
                if (getNetUnitCount(node) <= nthSmallestCount)
                {
                    output.Add(node);
                }
            }
            output.Add(nthSmallest);

            return output;
        }

        //gets the nth smallest node in a list
        //http://pine.cs.yale.edu/pinewiki/QuickSelect
        private Node getNthNetSmallestNode(List<Node> list, int n)
        {
            int p;
            if (list.Count == 1)
            {
                return list[0];
            }
            else
            {
                p = (int)Math.Round((double)r.Next(0, list.Count - 1));
            }
            Node pivotNode = list[p];
            int pivot = getNetUnitCount(pivotNode);
            List<Node> A1 = new List<Node>();
            List<Node> A2 = new List<Node>();

            //split into a pile A1 of small elements and A2 of big elements
            for (int i = 0; i < list.Count; i++)
            {
                if (getNetUnitCount(list[i]) < pivot)
                {
                    A1.Add(list[i]);
                }
                else if (getNetUnitCount(list[i]) > pivot)
                {
                    A2.Add(list[i]);
                }
            }

            if (n <= A1.Count)
            {
                //it's in the pile of smallest elements
                return getNthNetSmallestNode(A1, n);
            }
            else if (n > list.Count - A2.Count)
            {
                //it's in the pile of biggest elements
                return getNthNetSmallestNode(A2, n - (list.Count - A2.Count));
            }
            else
            {
                //it's equal to the pivot
                return pivotNode;
            }
        }

        //gets the unit count of a given node, taking into account any enemy or friendly units on their way to that node
        public static int getNetUnitCount(Node node)
        {
            int unitCount = node.UnitCount;

            if (node.OwnerId != -1)
            {
                //make unit count take into account friendly troops on their way to reinforce or enemy troops on their way to attack
                foreach (Unit unit in unitList)
                {
                    if (unit.DestinationId == getNodeId(node))
                    {
                        if (node.OwnerId == unit.OwnerId)
                        {
                            unitCount += 1;
                        }
                        else
                        {
                            unitCount -= 1;
                        }
                    }
                }
            }

            //return absolute value in case node is about to be captured (value will be negative)
            return Math.Abs(unitCount);
        }

        private Node getNetWeakestFriendlyNode(Player player)
        {
            Node output = null;
            int smallestNum = 500;

            foreach (Node node in nodeList)
            {
                if (node.OwnerId != -1 && playerList[node.OwnerId] == player)
                {
                    int unitNum = getNetUnitCount(node);

                    if (output == null || smallestNum > unitNum)
                    {
                        output = node; //poo
                        smallestNum = unitNum;
                    }
                }
            }
            return output;
        }

        private Node getNetStrongestFriendlyNode(Player player)
        {
            Node output = null;
            int biggestNum = 0;

            foreach (Node node in nodeList)
            {
                if (node.OwnerId != -1 && playerList[node.OwnerId] == player)
                {
                    int unitNum = getNetUnitCount(node);

                    if (output == null || biggestNum < unitNum)
                    {
                        output = node;
                        biggestNum = unitNum;
                    }
                }
            }
            return output;
        }


        private int getTeamSize(int playerId)
        {
            if (playerId == -1)
            {
                return 0;
            }
            int output = 0;
            foreach (Node node in nodeList)
            {
                if (node.OwnerId == playerId)
                {
                    output += node.UnitCount;
                }
            }
            foreach (Unit unit in unitList)
            {
                if (unit.OwnerId == playerId)
                {
                    output += 1;
                }
            }
            return output;
        }


        private void decisionTreeAI(Player player)
        {
            bool defending = false;
            Node smallestNode = getNetWeakestFriendlyNode(player);

            //check for nodes that need defending
            if (smallestNode != null)
            {
                int thresholdWeakness = 10;
                int safeThreshold = 15;
                int attackThreshold = 30;

                int smallestNodeUnitCount = getNetUnitCount(smallestNode);

                if (smallestNodeUnitCount < thresholdWeakness)
                {
                    //one of my nodes is too small or is being attacked by a dangerous amount of units
                    defending = true;

                    //get a list of other nodes that are big enough to reinforce the small node
                    List<Node> reinforcementList = new List<Node>();
                    foreach (Node node in nodeList)
                    {
                        if (node.OwnerId >= 0 && playerList[node.OwnerId] == player && getNetUnitCount(node) > attackThreshold)
                        {
                            reinforcementList.Add(node);
                        }
                    }

                    //if we have any nodes with spare units
                    if (reinforcementList.Count > 0)
                    {
                        //order by distance from smallestNode
                        reinforcementList.Sort((x, y) => (x.Position - smallestNode.Position).LengthSquared().CompareTo((y.Position - smallestNode.Position).LengthSquared()));


                        //send units until smallestNode above safeThreshold
                        int i = 0;
                        while (smallestNodeUnitCount < safeThreshold && i < reinforcementList.Count)
                        {
                            smallestNodeUnitCount += (int)Math.Floor(attackProportion * reinforcementList[i].UnitCount);
                            spawnUnits(reinforcementList[i], smallestNode);
                            i++;
                        }
                    }
                    return;
                }
            }


            if (!defending)
            {


                //TEMPORARY
                float rand = (float)r.NextDouble();
                if (rand < 0.2)
                {
                    Node target = getNetWeakestEnemyNode(player);
                    Node source = getNetStrongestFriendlyNode(player);

                    if (target.UnitCount < source.UnitCount / 2)
                    {
                        spawnUnits(source, target);
                    }
                }



                // TODO-----------------------------------------------------------------------------------







                //check for any good nodes to attack
            }
        }


        private void fuzzyLogicAI(Player player)
        {
            bool defending = false;
            Node smallestNode = getNetWeakestFriendlyNode(player);
            Node targetNode = null;

            float sizeBias = 1;
            float teamSizeBias = 0.01f;
            float randomBias = 5;

            //int attackScoreThreshold = 30 + (int)r.Next(-5, 5);
            int attackThreshold = 25 + (int)r.Next(-10, 10);
            int thresholdWeakness = 10 + (int)r.Next(-2, 2);
            int safeThreshold = 15 + (int)r.Next(-3, 3);
            int captureThreshold = -5 + (int)r.Next(-3, 4);


            //---------try to aquire a target (friendly or enemy)
            if (smallestNode != null)
            {

                int smallestNodeUnitCount = getNetUnitCount(smallestNode);

                if (smallestNodeUnitCount < thresholdWeakness)
                {
                    //one of my nodes is too small or is being attacked by a dangerous amount of units
                    defending = true;
                    targetNode = smallestNode;
                }

            }

            //only consider enemy nodes if not already reinforcing a friendly node (defending outweighs attacking)
            if (!defending)
            {
                //------find the best enemy target
                List<Node> targetList = getNetWeakestEnemyNodes(player, 3);
                List<float> targetScores = new List<float>();

                //score each node (smaller -> weaker)
                foreach (Node node in targetList)
                {
                    float score = getNetUnitCount(node) * sizeBias;
                    score += getTeamSize(node.OwnerId) * teamSizeBias;
                    score += r.Next(0, 1) * randomBias;
                    targetScores.Add(score);
                }

                //get weakest node
                float smallestScore = -1;
                for (int i = 0; i < targetList.Count; i++)
                {
                    if (smallestScore == -1 || targetScores[i] < smallestScore)
                    {
                        targetNode = targetList[i];
                        smallestScore = targetScores[i];
                    }
                }

                //check if attack is wise
                //if (smallestScore > attackScoreThreshold)
                //{
                //    return;
                //}

            }
            //------END target selection


            //---------find best combination of source nodes for attack/reinforcement

            //get list of friendly nodes, order by size descending
            List<Node> sourceList = new List<Node>();
            foreach (Node node in nodeList)
            {
                if (node.OwnerId != -1 && playerList[node.OwnerId] == player && getNetUnitCount(node) > attackThreshold)
                {
                    //add node to sourcelist in the right index to produce an ordered list
                    if (sourceList.Count == 0)
                    {
                        sourceList.Add(node);
                    }
                    else if (getNetUnitCount(sourceList[sourceList.Count - 1]) < getNetUnitCount(node))
                    {
                        sourceList.Add(node);
                    }
                    NodeComparer NC = new NodeComparer();
                    int index = sourceList.BinarySearch(node, NC);
                    if (index < 0)
                    {
                        sourceList.Insert(~index, node);
                    }
                }
            }
            //reverse the list to put in descending order
            sourceList.Reverse();


            int targetSize = getNetUnitCount(targetNode);

            if (defending)
            {
                //keep adding units from source list until target above safe threshold or we run out of reinforcers

                for (int i = 0; i < sourceList.Count; i++)
                {
                    targetSize += sourceList[i].UnitCount / 2;
                    spawnUnits(sourceList[i], targetNode);

                    if (targetSize >= safeThreshold)
                    {
                        break;
                    }
                }
            }
            else
            {
                //keep adding units from source list until target captured
                int n = 0;

                for (int i = 0; i < sourceList.Count; i++)
                {
                    targetSize -= sourceList[i].UnitCount / 2;
                    n++;

                    if (targetSize < captureThreshold)
                    {
                        //we can capture the node using the top n friendly nodes
                        break;
                    }
                }

                if (targetSize < captureThreshold)
                {
                    for (int i = 0; i < n; i++)
                    {
                        spawnUnits(sourceList[i], targetNode);
                    }
                }
                else
                {
                    //we're not strong enough to capture the node, abort
                    return;
                }
            }





        }




        private static int compareDistances(Vector2 x, Vector2 y, Vector2 p)
        {
            float xDist2 = (p - x).LengthSquared();
            float yDist2 = (p - y).LengthSquared();
            if (xDist2 < yDist2)
            {
                //x is closer
                return -1;
            }
            else if (xDist2 > yDist2)
            {
                //y is closer
                return 1;
            }
            else
            {
                return 0;
            }
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
                case 3:
                    AStarPaths();
                    break;
                case 4:
                    AStarSteeringPaths();
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


        private void AStarPaths()
        {
            foreach (Unit unit in unitList)
            {
                int lineSegment = (int)Math.Floor(unit.AStarPathProgress);
                float segmentProgress = unit.AStarPathProgress - lineSegment;

                if (lineSegment < 0)
                {
                    //unit is before start of line
                    //set position to start of path
                    unit.Position = navPaths[unit.AStarPathId].Points[0];

                    segmentProgress += maxUnitVelocity / 10;

                    unit.AStarPathProgress = segmentProgress + lineSegment;
                }
                else if (lineSegment >= navPaths[unit.AStarPathId].Points.Count - 1)
                {
                    //unit is after end of line
                    //set position to end of path and don't update path progress (wasted processing)
                    unit.Position = navPaths[unit.AStarPathId].Points[navPaths[unit.AStarPathId].Points.Count - 1];
                    break;
                }
                else
                {

                    Vector2 lineStart = navPaths[unit.AStarPathId].Points[lineSegment];
                    Vector2 lineEnd = navPaths[unit.AStarPathId].Points[lineSegment + 1];
                    float lineLength = (lineEnd - lineStart).Length();

                    Vector2 position = lineStart + (lineEnd - lineStart) * segmentProgress;

                    segmentProgress += maxUnitVelocity / lineLength;

                    unit.AStarPathProgress = segmentProgress + lineSegment;
                    unit.Position = position;
                }
            }
        }


        private void AStarSteeringPaths()
        {
            foreach (Unit unit in unitList)
            {
                NavigationPath navPath = navPaths[unit.AStarPathId];
                Vector2 heading = unit.Velocity;
                heading.Normalize();
                Vector2 target = heading * pathSteeringTargetDistance + unit.Position;

                float shortestDistance = 10000;
                int nearestSegment = 0;

                for (int i = 0; i < navPath.Points.Count - 1; i++)
                {
                    Vector2 point1 = navPath.Points[i];
                    Vector2 point2 = navPath.Points[i + 1];

                    float dist = calcPointLineDistance(point1, point2, target);
                    if (dist < shortestDistance)
                    {
                        nearestSegment = i;
                        shortestDistance = dist;
                    }
                }

                if (CheckPointCircleCollision(unit.Position, navPath.Points[nearestSegment + 1], 50))
                {
                    //if the end is very close just steer straight for it
                    unit.Velocity *= unitFriction;
                    Vector2 steerVector = navPath.Points[nearestSegment + 1] - unit.Position;
                    steerVector.Normalize();
                    unit.Velocity += steerVector * 0.5f;
                }
                else if (shortestDistance > pathSteeringErrorMargin)
                {

                    //find which way to steer
                    bool steerLeft = isLeft(navPath.Points[nearestSegment], navPath.Points[nearestSegment + 1], target);


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
                    unit.Velocity += steerVector * pathSteeringForce;

                }

                unit.Velocity += heading * 0.02f;

                //make sure velocity doesn't go above the maximum
                if (unit.Velocity.LengthSquared() > maxUnitVelocitySquared)
                {
                    Vector2 dir = unit.Velocity;
                    dir.Normalize();
                    unit.Velocity = dir * maxUnitVelocity;
                }

                unit.Position += unit.Velocity;
            }
        }


        private void buildAStarPath(Node source, Node destination)
        {
            if (getAStarPathId(source, destination) == -1)
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
                        //bool ignore = false;
                        foreach (NavigationPoint n in closedList)
                        {
                            if (n.graphRef == p)
                            {
                                //ignore = true;
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
                            if (openListPoint.G > (openListPoint.graphRef.Position - currentPoint.graphRef.Position).Length() + currentPoint.G)
                            {
                                openListPoint.Parent = currentPoint;
                                openListPoint.G = (openListPoint.graphRef.Position - openListPoint.Parent.graphRef.Position).Length() + openListPoint.Parent.G;

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

            for (int i = 0; i < navGraph.Count; i++)
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

            path.Points.Reverse();

            return path;
        }

        private int getAStarPathId(Node source, Node destination)
        {
            for (int i = 0; i < navPaths.Count; i++)
            {
                Node startNode = nodeList[navPaths[i].startNodeId];
                Node endNode = nodeList[navPaths[i].endNodeId];

                //check if start and end nodes match
                if (source.Position == startNode.Position && endNode.Position == destination.Position)
                {
                    return i;
                }

            }
            return -1;
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
                if (node.OwnerId >= 0)
                {
                    node.UnitProgress += playerList[node.OwnerId].GrowthRate;
                }
                else
                {
                    node.UnitProgress += NeutralGrowthRate;
                }
            }
        }

        private void CheckCollisions()
        {

        }

        private void UpdatePlayers()
        {
            bool allEnemiesDead = true;
            foreach (Player player in playerList)
            {
                if (!player.IsHuman && player.IsAlive)
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead)
            {
                setLevel(currentLevel + 1);
            }
            else if (!playerList[humanOwnerId].IsAlive)
            {
                setLevel(currentLevel);
            }
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
            List<Node> selectedNodes = getSelectedNodes();
            if (selectedNodes.Count != 0)
            {
                foreach (Node selectedNode in selectedNodes)
                {
                    DrawLine(blank, 2f, Color.White, selectedNode.Position, new Vector2(currentMouseState.X, currentMouseState.Y));
                }
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
                    else if (selectedNodes.Count != 0 && node.OwnerId != humanOwnerId)
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
            if (pathfindingMethod == 3 || pathfindingMethod == 4)
            {
                foreach (GraphPoint point in navGraph)
                {
                    foreach (GraphPoint point2 in point.connectedNodes)
                    {
                        DrawLine(blank, 0.5f, new Color(200, 200, 255), point.Position, point2.Position);
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
            //check journey is valid and has enough units to send
            if (sourceNode != null && destinationNode != null && sourceNode.UnitCount > 1 && sourceNode != destinationNode)
            {
                int pathId = -1;
                //if we're using A*, build the path
                if (pathfindingMethod == 3 || pathfindingMethod == 4)
                {
                    buildAStarPath(sourceNode, destinationNode);
                    pathId = getAStarPathId(sourceNode, destinationNode);
                    if (pathId == -1)
                    {
                        throw new Exception("Path not found");
                    }
                }

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
                    float angleToDest = (float)Math.Atan2((destinationNode.Position.Y - sourceNode.Position.Y), (destinationNode.Position.X - sourceNode.Position.X));
                    float angle;

                    if (pathfindingMethod == 4)
                    {
                        angle = angleToDest + (float)(r.NextDouble() * Math.PI - Math.PI / 2);
                    }
                    else
                    {
                        angle = (float)(r.NextDouble() * Math.PI * 2);
                    }

                    float relativeX = (float)(radius * Math.Cos(angle));
                    float relativeY = (float)(radius * Math.Sin(angle));
                    float x = sourceNode.Position.X + relativeX;
                    float y = sourceNode.Position.Y + relativeY;

                    //add an initial velocity directly away from the source node
                    float xVel = relativeX * unitStartVelocity * ((float)r.NextDouble());
                    float yVel = relativeY * unitStartVelocity * ((float)r.NextDouble());

                    //make the unit 
                    Unit newUnit = new Unit(sourceNode.OwnerId, new Vector2(x, y), new Vector2(xVel, yVel), getNodeId(destinationNode), getNodeId(sourceNode));

                    //set up paths for A*
                    if (pathfindingMethod == 3 || pathfindingMethod == 4)
                    {
                        newUnit.AStarPathId = pathId;
                        newUnit.AStarPathProgress = (float)r.NextDouble() * -25;
                    }

                    //add unit to the list
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
                //if enemy node attack
                if (defendingNode.UnitCount == 1)
                {
                    //node taken over

                    if (defendingNode.OwnerId != -1)
                    {
                        //check if this is the defender's last node, if so then set their IsAlive status to false
                        int nodeCount = 0;
                        foreach (Node node in nodeList)
                        {
                            if (node.OwnerId == defendingNode.OwnerId)
                            {
                                nodeCount++;
                            }
                        }
                        if (nodeCount == 1)
                        {
                            playerList[defendingNode.OwnerId].IsAlive = false;
                        }
                    }
                    //set node to attacker's
                    defendingNode.OwnerId = attackingUnit.OwnerId;

                    //unselect the node if it is selected
                    defendingNode.Selected = false;

                }
                else
                {
                    //node unitcount reduced by one
                    defendingNode.UnitCount -= 1;
                }
            }
            else
            {
                //if friendly node reinforce
                defendingNode.UnitCount += 1;
            }
        }


        /// <summary>
        /// Returns a node's position in nodeList given the node object (uses node's x and y to identify it)
        /// </summary>
        /// <param name="node">Node to identify.</param>
        public static int getNodeId(Node node)
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
        private List<Node> getSelectedNodes()
        {
            List<Node> nodes = new List<Node>();

            foreach (Node node in nodeList)
            {
                if (node.Selected)
                {
                    nodes.Add(node);
                }
            }

            return nodes;

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
        public bool isLeft(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {
            return ((linePoint2.X - linePoint1.X) * (point.Y - linePoint1.Y) - (linePoint2.Y - linePoint1.Y) * (point.X - linePoint1.X)) > 0;
        }

        public bool isBeyondEnd(Vector2 startPoint, Vector2 endPoint, Vector2 point)
        {
            Vector2 line = endPoint - startPoint;
            Vector2 line2 = rotateVector2(line, (float)Math.PI / 2);

            return isLeft(endPoint, endPoint - line2, point);
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



    public class NodeComparer : IComparer<Node>
    {

        public int Compare(Node a, Node b)
        {
            int x = NodesGame.getNetUnitCount(a);
            int y = NodesGame.getNetUnitCount(b);

            if (x > y)
            {
                return 1;
            }
            else if (y > x)
            {
                return -1;
            }
            else
            {
                return 0;
            }

        }
    }
}

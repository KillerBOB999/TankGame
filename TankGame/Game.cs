using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TankGame
{
	public class Game
	{
		public Game() { }

		public Game(Player Red, Player Blue, bool display)
		{
			red = Red;
			blue = Blue;
			displayGame = display;
			InitializeData();
			//Timer timer = new Timer();
			//timer.Interval = 5;        //# of milliseconds
			//timer.Tick += Timer_Tick;
			//timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			GameLoop();
		}

		//START GLOBALS----------------------------------------------------------------------------------
		public bool displayGame;

        public int numberOfIterations = 0;
        public int maxIterations = 5000;

		public WorldState worldState;
		public int generationCount = 0;
		public double baseTimeLimitInSeconds = 1;
		public double timeLimitInSeconds = 1;
		public int numRedWins = 0;
		public int numBlueWins = 0;
		public double peakRed = 0;
		public double peakBlue = 0;

		public bool firstTime = true;

		public DateTime startTime = new DateTime();
		public DateTime currentTime = new DateTime();
		public TimeSpan elapsed = new TimeSpan();
		public double baseWinBonus = Player.baseFitness;
		public double currentWinBonus;

		//Pull in external resources and initialize core entity objects
		string terrainMapN = File.ReadAllText("Resources/Maps/TerrainMaps/TerrainMap2.JSON");
		public Player red;
		public Player blue;

		//Define useful variables
		public static int WIDTH_IN_TILES = 25;      //Number of tiles in the width of the world
		public static int HEIGHT_IN_TILES = 25;     //Number of tiles in the height of the world
		public static double MAX_DISTANCE = Math.Pow((double)WIDTH_IN_TILES + (double)HEIGHT_IN_TILES, 0.5);
		public static double MAX_ANGLE = Math.PI;
		public static List<double> MAX_INPUT_VALUES = new List<double>() { MAX_DISTANCE, MAX_ANGLE, MAX_DISTANCE, MAX_ANGLE };
		public int xMapBlock;                      //Used as iterator in mapBlocks[]
		public int yMapBlock;                      //Used as iterator in mapBlocks[]
		public MapBlock[,] mapBlocks = new MapBlock[WIDTH_IN_TILES, HEIGHT_IN_TILES];//The map in gametiles
		public Position offset = new Position();   //Position offset in pixels for use when drawing the bitmap


		//END GLOBALS------------------------------------------------------------------------------------

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	InitializeData()
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Iterates through the JSON map file and defines the starting
		///					points for each entity, initializes the neural network data,
		///					and initializes the entities.
		///	Last Modified:	02 October 2019
		///	Modification:	Successfully implemented logic to initialize the Edges of the
		///	                neural networks.
		/// </summary>
		private void InitializeData()
		{
            ++generationCount;
            worldState = WorldState.GameInProgress;

			startTime = DateTime.Now;
			currentTime = startTime;
			currentWinBonus = Player.baseFitness;

			xMapBlock = 0;
			yMapBlock = 0;
			for (int index = 0; index < terrainMapN.Length; ++index)
			{
				int pass;       //To be passed to the TileType function

				//If the current char at terrainMapN[index] can be parsed as an int, then step in
				if (int.TryParse(terrainMapN[index].ToString(), out pass))
				{
					//Call the function to define the type of tile
					TileType(pass);
				}
			}

			if (firstTime)
			{
				List<List<Edge>> tempEdges = new List<List<Edge>>() { new List<Edge>(), new List<Edge>() };
				List<int> outputLayer = new List<int>();
				//Create the edges of the neural networks to be used

				int inLayerId;
				int outLayerId;

				//The following is for when you use the entire map as an input.

				//for (int x = 0; x < WIDTH_IN_TILES; ++x)
				//{
				//	for (int y = 0; y < HEIGHT_IN_TILES; ++y)
				//	{
				//		for (int z = 0; z < MapBlock.numOfStates; ++z)
				//		{
				//			inputLayer.Add(inLayerId);
				//          for (int command = (int)ControlCommand.NONE; command < (int)ControlCommand.FINAL_UNUSED; ++command)
				//			{
				//              outLayerId = WIDTH_IN_TILES * HEIGHT_IN_TILES * MapBlock.numOfStates + command + 1;
				//				tempEdges.Add(new Edge(inLayerId, outLayerId, 1, 0));
				//			}
				//          inLayerId++;
				//      }
				//	}
				//}

				//The following is for when you use the tankDegreeToTarget and turretDegreeToTarget
				//distanceToTarget attributes of player as inputs

				Random rng = new Random();
				for (int playerCounter = 0; playerCounter < 2; ++playerCounter)
				{
					inLayerId = 0;
					for (int i = 0; i < Player.NUM_OF_INPUTS; ++i)
					{
						for (int command = (int)ControlCommand.NONE; command < (int)ControlCommand.FINAL_UNUSED; ++command)
						{
							outLayerId = Player.NUM_OF_INPUTS + command;
							tempEdges[playerCounter].Add(new Edge(inLayerId, outLayerId, rng.NextDouble(), rng.NextDouble()));
							if (!outputLayer.Contains(outLayerId))
							{
								outputLayer.Add(outLayerId);
							}
						}
						inLayerId++;
					}
				}

				red.organism.botBrain = new NeuralNetwork(tempEdges[0], Player.NUM_OF_INPUTS + (int)ControlCommand.FINAL_UNUSED, outputLayer);
				blue.organism.botBrain = new NeuralNetwork(tempEdges[1], Player.NUM_OF_INPUTS + (int)ControlCommand.FINAL_UNUSED, outputLayer);
			}

			//Initialize the players
			red.reset(MAX_INPUT_VALUES);
			blue.reset(MAX_INPUT_VALUES);

			red.playerController(ControlCommand.NONE, ControlCommand.Right);
			blue.playerController(ControlCommand.NONE, ControlCommand.Left);

			red.updatePlayer(mapBlocks);
			blue.updatePlayer(mapBlocks);

			red.calcDegreeAndDistanceToTarget(blue, MAX_INPUT_VALUES);
			blue.calcDegreeAndDistanceToTarget(red, MAX_INPUT_VALUES);
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	TileType
		/// Parameters:		int typeSpecifier
		///						Use:	Integer value representative of the type of game tile being added
		///								to the bitmap. Ex: wall, floor.
		/// Returns:		None
		/// Description:	Defines the game tiles within the mapBlocks global array.
		/// Last Modified:	19 September 2019
		/// Modification:	Adjusted comments.
		/// </summary>
		private void TileType(int typeSpecifier)
		{
			switch (typeSpecifier)
			{
				case 0:         //Floor Tile
					mapBlocks[xMapBlock, yMapBlock] = new MapBlock(false, true, false, false, false, xMapBlock, yMapBlock, Color.Black, Player.xScale, Player.yScale);
					break;
				case 1:         //Wall Tile
					mapBlocks[xMapBlock, yMapBlock] = new MapBlock(false, false, true, false, false, xMapBlock, yMapBlock, Color.Gray, Player.xScale, Player.yScale);
					break;
				case 8:         //Red Player Spawn
					mapBlocks[xMapBlock, yMapBlock] = new MapBlock(false, true, false, true, false, xMapBlock, yMapBlock, Color.Red, Player.xScale, Player.yScale);
					red.spawnPoint.x = xMapBlock;
					red.spawnPoint.y = yMapBlock;
					mapBlocks[xMapBlock, yMapBlock].isOccupied = true;
					mapBlocks[xMapBlock, yMapBlock].isOccRed = true;
					break;
				case 9:         //Blue Player Spawn
					mapBlocks[xMapBlock, yMapBlock] = new MapBlock(false, true, false, false, true, xMapBlock, yMapBlock, Color.Blue, Player.xScale, Player.yScale);
					blue.spawnPoint.x = xMapBlock;
					blue.spawnPoint.y = yMapBlock;
					mapBlocks[xMapBlock, yMapBlock].isOccupied = true;
					mapBlocks[xMapBlock, yMapBlock].isOccBlue = true;
					break;
			}
			mapBlocks[xMapBlock, yMapBlock].updateStates();

			++xMapBlock;
			if (xMapBlock >= WIDTH_IN_TILES)
			{
				xMapBlock = 0;
				++yMapBlock;
			}
			if (yMapBlock >= HEIGHT_IN_TILES)
			{
				yMapBlock = 0;
			}
		}

		private void DoAIStuff(Player player)
		{
			Dictionary<int, double> outputDict = player.organism.botBrain.feedForward(player.botInput);
			double valueOfHighest = -1;
			int ControlCommandEquivalent = -1;
			for (int nodeID = Player.NUM_OF_INPUTS + 1; nodeID < Player.NUM_OF_INPUTS + (int)ControlCommand.FINAL_UNUSED; ++nodeID)
			{
				if (outputDict[nodeID] > valueOfHighest)
				{
					valueOfHighest = outputDict[nodeID];
					ControlCommandEquivalent = nodeID - (Player.NUM_OF_INPUTS - 1);
				}
			}

			switch (ControlCommandEquivalent)
			{
				case (int)ControlCommand.NONE:
					break;
				case (int)ControlCommand.Up:
					player.keyController(Keys.Up);
					break;
				case (int)ControlCommand.Right:
					player.keyController(Keys.Right);
					break;
				case (int)ControlCommand.Down:
					player.keyController(Keys.Down);
					break;
				case (int)ControlCommand.Left:
					player.keyController(Keys.Left);
					break;
				case (int)ControlCommand.W:
					player.keyController(Keys.W);
					break;
				case (int)ControlCommand.D:
					player.keyController(Keys.D);
					break;
				case (int)ControlCommand.S:
					player.keyController(Keys.S);
					break;
				case (int)ControlCommand.A:
					player.keyController(Keys.A);
					break;
				case (int)ControlCommand.Space:
					player.keyController(Keys.Space);
					break;
			}
		}

		private void handleGameover()
		{
			Random rng = new Random();
			//if (generationCount % 100 == 0)
			//{
			//	timeLimitInSeconds += baseTimeLimitInSeconds;
			//}

			red.calcFitness(blue, numberOfIterations);
			blue.calcFitness(red, numberOfIterations);

			if (red.organism.fitness > peakRed)
			{
				peakRed = red.organism.fitness;
			}
			if (blue.organism.fitness > peakBlue)
			{
				peakBlue = blue.organism.fitness;
			}

			//if (rng.NextDouble() < 0.5)
			//{
			//	NeuralNetwork mama;
			//	NeuralNetwork papa;
			//	if (red.organism.fitness < blue.organism.fitness)
			//	{
			//		mama = new NeuralNetwork(red.organism.botBrain);
			//		papa = new NeuralNetwork(blue.organism.botBrain);
			//		red.organism.botBrain = Mutatinator.cross(mama, papa);
			//	}
			//	else
			//	{
			//		mama = new NeuralNetwork(blue.organism.botBrain);
			//		papa = new NeuralNetwork(red.organism.botBrain);
			//		blue.organism.botBrain = Mutatinator.cross(mama, papa);
			//	}
			//}
			//else
			//{
			//	Mutatinator.mutate(red.organism.botBrain);
			//	Mutatinator.mutate(blue.organism.botBrain);
			//}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	Update
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Updates the state of the world based upon the
		///                 actions of the entities within
		/// Last Modified:	02 October 2019
		/// Modification:	Added comment summary
		/// </summary>
		private void Update()
		{
			if (numberOfIterations >= maxIterations)
			{
				worldState = WorldState.GameOverTimeOut;
				//elapsed = DateTime.Now - DateTime.Now;
			}
			if (worldState == WorldState.GameInProgress)
			{
				currentTime = DateTime.Now;
				elapsed = currentTime - startTime;
				currentWinBonus = baseWinBonus * (1 / (1 + elapsed.TotalSeconds));
				red.calcFitness(blue, elapsed);
				blue.calcFitness(red, elapsed);

				//Do AI stuff
				red.calcDegreeAndDistanceToTarget(blue, MAX_INPUT_VALUES);
				blue.calcDegreeAndDistanceToTarget(red, MAX_INPUT_VALUES);
				if (!red.isHuman)
				{
					DoAIStuff(red);
				}
				if (!blue.isHuman)
				{
					DoAIStuff(blue);
				}

				//Update players and missiles
				red.updatePlayer(mapBlocks);
				blue.updatePlayer(mapBlocks);
				for (int i = 0; i < red.missiles.Length; ++i)
				{
					if (red.missiles[i].isActive)
					{
						red.missiles[i].updateMissile(mapBlocks, ref worldState);
					}
				}
				for (int i = 0; i < blue.missiles.Length; ++i)
				{
					if (blue.missiles[i].isActive)
					{
						blue.missiles[i].updateMissile(mapBlocks, ref worldState);
					}
				}
				red.calcDegreeAndDistanceToTarget(blue, MAX_INPUT_VALUES);
				blue.calcDegreeAndDistanceToTarget(red, MAX_INPUT_VALUES);

				//Update the map
				for (int x = 0; x < Game.WIDTH_IN_TILES; ++x)
				{
					for (int y = 0; y < Game.HEIGHT_IN_TILES; ++y)
					{
						if (!mapBlocks[x, y].isWall)
						{
							mapBlocks[x, y].updateStates();
						}
					}
				}
			}
			switch (worldState)
			{
				case WorldState.GameOverBlueWin:
					blue.winBonus = currentWinBonus;
					red.winBonus = -currentWinBonus;
					++numBlueWins;
					break;
				case WorldState.GameOverRedWin:
					red.winBonus = currentWinBonus;
					blue.winBonus = -currentWinBonus;
					++numRedWins;
					break;
			}
			if (worldState != WorldState.GameInProgress)
			{
				handleGameover();
			}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	GameLoop
		/// Parameters:		None
		/// Returns:		None
		/// Description:	The typical game loop found in games. Calls
		///                 Update() to update the state of the world and
		///                 then calls Render() to display the image
		/// Last Modified:	02 October 2019
		/// Modification:	Added comments.
		/// </summary>
		public void GameLoop()
		{
			Update();
            ++numberOfIterations;
		}
	}
}

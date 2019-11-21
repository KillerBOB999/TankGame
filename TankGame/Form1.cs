using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using TankGame.Entities;

namespace TankGame
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			Focus();
			Timer timer = new Timer();
			timer.Interval = 5;        //# of milliseconds
			timer.Tick += Timer_Tick;
			timer.Start();
			SetUp();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			//Render();
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	ProcessCmdKey
		/// Parameters:		ref Message msg
		///						Use: none
		///					Keys keyData
		///						Use: Represents the key pressed that triggered the event.
		///							Is sent to the Player objects to be handled.
		/// Returns:		True
		/// Description:	Takes a key stroke as an input, determines which player is human,
		///					and passes the key to said player.
		///	Last Modified:	19 September 2019
		///	Modification:	Added comments
		/// </summary>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (games[displayGame].red.isHuman == true)
			{
				games[displayGame].red.keyController(keyData);
			}
			else if (games[displayGame].blue.isHuman == true)
			{
				games[displayGame].blue.keyController(keyData);
			}
			return true;
		}

		public object gameLock = new object();
		private delegate void SafeCallDelegate(Control control, string text);

		//Who's playing?
		const bool isRedHumanPlaying = false;
		const bool isBlueHumanPlaying = false;

		static Player red = new Player("Resources/images/Red_TankBody.png", "Resources/images/Turret.png",
							Orientation.North, Orientation.North, isRedHumanPlaying, true);
		static Player blue = new Player("Resources/images/Blue_TankBody.png", "Resources/images/Turret.png",
								Orientation.North, Orientation.North, isBlueHumanPlaying, false);

		const int populationSize = 50;
        static int nextBrainID = 0;
		static Dictionary<int, Organism> hallOfFame = new Dictionary<int, Organism>();
		static Dictionary<int, Organism> brains = new Dictionary<int, Organism>();
		static Dictionary<int, double> brainFitness = new Dictionary<int, double>();
		Game[] games;
		int displayGame;

		public void SetUp()
		{
			// Create new games
			games = new Game[populationSize / 2];

			// Initialize new games with the players but no brains yet
			for (int i = 0; i < games.Length; ++i)
			{
				games[i] = new Game(new Player(red), new Player(blue), false);
				games[i].red.organism.botBrainID = nextBrainID;
				brains.Add(nextBrainID++, games[i].red.organism);
				games[i].blue.organism.botBrainID = nextBrainID;
				brains.Add(nextBrainID++, games[i].blue.organism);
			}

			displayGame = 0;
			Task simulation = new Task(runSimulation);
			simulation.Start();
		}

		public void runSimulation()
		{
			while (true)
			{
				List<int> activeBrains = new List<int>();
				for (int i = 0; i < games.Length; ++i)
				{
					Random rng = new Random();
					int rand1 = rng.Next(0, nextBrainID);
					int rand2 = rng.Next(0, nextBrainID);
					while (activeBrains.Contains(rand1))
					{
						rand1 = rng.Next(0, nextBrainID);
					}
					activeBrains.Add(rand1);
					while (activeBrains.Contains(rand2))
					{
						rand2 = rng.Next(0, nextBrainID);
					}
					activeBrains.Add(rand2);
					games[i] = new Game(new Player(red), new Player(blue), false);
					games[i].red.organism.botBrainID = rand1;
					red.organism.botBrain = brains[rand1].botBrain;
					games[i].blue.organism.botBrainID = rand2;
					blue.organism.botBrain = brains[rand2].botBrain;
				}
				List<Task> toDo = new List<Task>();
				foreach (Game game in games)
				{
					toDo.Add(Task.Run(() =>
					{
						while (game.worldState == WorldState.GameInProgress)
						{
							game.GameLoop();
							//Need to solve multiple access issues for rendering
							if (game == games[displayGame]) Render();
						}
					}));
				}
				Task.WaitAll(toDo.ToArray());
				updateBrains();
                updateFitness();
			}
		}

        public void updateBrains()
        {
            // Update the brains dictionary
            for (int i = 0; i < games.Length; ++i)
            {
                if (!brains.ContainsKey(games[i].red.organism.botBrainID))
                {
                    brains.Add(games[i].red.organism.botBrainID, games[i].red.organism);
                }
                else
                {
                    brains[games[i].red.organism.botBrainID] = games[i].red.organism;
                }
                if (!brains.ContainsKey(games[i].blue.organism.botBrainID))
                {
                    brains.Add(games[i].blue.organism.botBrainID, games[i].blue.organism);
                }
                else
                {
                    brains[games[i].blue.organism.botBrainID] = games[i].blue.organism;
                }
            }
        }

        public void updateFitness()
        {
            int maxFitnessID = -1;
            double maxFitnessValue = 0;
            foreach (Game game in games)
            {
                if (game.red.organism.fitness > maxFitnessValue)
                {
                    maxFitnessID = game.red.organism.botBrainID;
                    maxFitnessValue = game.red.organism.fitness;
                }
                else if (game.blue.organism.fitness > maxFitnessValue)
                {
                    maxFitnessID = game.blue.organism.botBrainID;
                    maxFitnessValue = game.blue.organism.fitness;
                }
                if (!brainFitness.ContainsKey(game.red.organism.botBrainID))
                {
                    brainFitness.Add(game.red.organism.botBrainID, game.red.organism.fitness);
                }
                else
                {
                    brainFitness[game.red.organism.botBrainID] = game.red.organism.fitness;
                }
                if (!brainFitness.ContainsKey(game.blue.organism.botBrainID))
                {
                    brainFitness.Add(game.blue.organism.botBrainID, game.blue.organism.fitness);
                }
                else
                {
                    brainFitness[game.blue.organism.botBrainID] = game.blue.organism.fitness;
                }
            }
            updateHallOfFame(maxFitnessID);
        }

        public void updateHallOfFame(int maxFitnessID)
        {
			if (!hallOfFame.ContainsKey(maxFitnessID))
			{
				hallOfFame.Add(maxFitnessID, brains[maxFitnessID]);
			}
			else
			{
				hallOfFame[maxFitnessID] = brains[maxFitnessID];
			}
			evolveBrains();
		}

		public void evolveBrains()
		{
			Random rng = new Random();
			double totalFitness = 0;
			List<Tuple<int, double>> rangeOfMating = new List<Tuple<int, double>>();
			List<KeyValuePair<int, double>> sortedFitnesses = brainFitness.ToList();
			sortedFitnesses.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
			Dictionary<int, NeuralNetwork> replacements = new Dictionary<int, NeuralNetwork>();

			int[] parentIDs;

			foreach(var brainID_FitnessPair in brainFitness)
			{
				totalFitness += brainID_FitnessPair.Value;
			}

			foreach (var brainID_FitnessPair in brainFitness)
			{
				if(rangeOfMating.Count == 0)
				{
					rangeOfMating = new List<Tuple<int, double>>(1) { new Tuple<int, double>(brainID_FitnessPair.Key, brainID_FitnessPair.Value / totalFitness) };
				}
				else
				{
					rangeOfMating.Add(new Tuple<int, double>(brainID_FitnessPair.Key, rangeOfMating.Last().Item2 + brainID_FitnessPair.Value / totalFitness));
				}
			}

			for (int replaceIndex = 0; replaceIndex < sortedFitnesses.Count / 2; replaceIndex += 2)
			{
				parentIDs = new int[] { -1, -1 };
				int parentIndex = 0;
				while (parentIDs[1] == -1)
				{
					double rand = rng.NextDouble();

					for (int matingIndex = 1; matingIndex < rangeOfMating.Count; ++matingIndex)
					{
						if (rand < rangeOfMating[matingIndex].Item2 && rangeOfMating[matingIndex - 1].Item2 < rand)
						{
							parentIDs[parentIndex] = rangeOfMating[matingIndex].Item1;
							parentIndex++;
							matingIndex = rangeOfMating.Count;
						}
					}
				}

				int moreFitID;
				int lessFitID;
				if (brainFitness[parentIDs[0]] < brainFitness[parentIDs[1]])
				{
					moreFitID = 1;
					lessFitID = 0;
				}
				else
				{
					moreFitID = 0;
					lessFitID = 1;
				}
				replacements.Add(sortedFitnesses[replaceIndex].Key, 
					Mutatinator.mutate(
						//Mutatinator.cross(
							brains[parentIDs[lessFitID]].botBrain//, 
									  //brains[parentIDs[moreFitID]].botBrain
									  )
						//)
					);
			}

			//foreach(KeyValuePair<int, NeuralNetwork> replacement in replacements)
			//{
			//	brains[replacement.Key] = replacements[replacement.Key];
			//}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddBackGround
		/// Parameters:		Graphics graphics
		///                     Use:    Graphics object is used to update the image of the
		///                             current state of the world
		/// Returns:		None
		/// Description:	Determines the current state of the background image
		///	Last Modified:	02 October 2019
		///	Modification:	Redesigned to used graphics objects as opposed to the previous
		///	                brute force pixel value assignments
		/// </summary>
		private void AddBackground(Graphics graphics)
		{
			//Proportional number of pixels that represent the pixel per game tile
			int x = splitContainer1.Panel2.Width / Game.WIDTH_IN_TILES;
			int y = splitContainer1.Panel2.Height / Game.HEIGHT_IN_TILES;

			games[displayGame].xMapBlock = 0;
			games[displayGame].yMapBlock = 0;

			//Iterate through the terrainMapN string created from the JSON file
			games[displayGame].offset.x = 0;       //reset offsets to 0 to ensure correct
			games[displayGame].offset.y = 0;       //starting position

			for (int index = 0; index < games[displayGame].mapBlocks.Length; ++index)
			{
				//Check to see if you've moved beyond the width of the panel
				if (games[displayGame].offset.x >= Game.WIDTH_IN_TILES * x)
				{
					games[displayGame].offset.x = 0;
					games[displayGame].offset.y += y;
				}

				//Check to see if you've gone beyond the height of the panel
				if (games[displayGame].offset.y >= Game.HEIGHT_IN_TILES * y)
				{
					games[displayGame].offset.y = 0;
				}

				if (games[displayGame].xMapBlock >= Game.WIDTH_IN_TILES)
				{
					games[displayGame].xMapBlock = 0;
					++games[displayGame].yMapBlock;
				}

				if (games[displayGame].yMapBlock >= Game.HEIGHT_IN_TILES)
				{
					games[displayGame].yMapBlock = 0;
				}

				Brush brush = null;
				switch (games[displayGame].mapBlocks[games[displayGame].xMapBlock, games[displayGame].yMapBlock].color)
				{
					case Color.Black:
						brush = new SolidBrush(System.Drawing.Color.Black);
						break;
					case Color.Gray:
						brush = new SolidBrush(System.Drawing.Color.Gray);
						break;
					case Color.Red:
						brush = new SolidBrush(System.Drawing.Color.White);
						break;
					case Color.Blue:
						brush = new SolidBrush(System.Drawing.Color.White);
						break;
				}
				graphics.FillRectangle(brush, games[displayGame].offset.x, games[displayGame].offset.y, x, y);
				brush.Dispose();

				//Increment the offset
				games[displayGame].offset.x += x;
				++games[displayGame].xMapBlock;
			}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddPlayers
		/// Parameters:		Graphics graphics
		///                     Use:    Graphics object is used to update the image of the
		///                             current state of the world
		/// Returns:		None
		/// Description:	Adds the player images to the existing background image
		/// Last Modified:	02 October 2019
		/// Modification:	Redesigned to used graphics objects as opposed to the previous
		///	                brute force pixel value assignments
		/// </summary>
		private void AddPlayers(Graphics graphics)
		{
			lock (gameLock)
			{
				games[displayGame].red.bodyOriented = games[displayGame].red.findOrientedImage(games[displayGame].red.scaleBody(), games[displayGame].red.tankOrientation);
				games[displayGame].red.turretOriented = games[displayGame].red.findOrientedImage(games[displayGame].red.scaleTurret(), games[displayGame].red.turretOrientation);
				games[displayGame].blue.bodyOriented = games[displayGame].blue.findOrientedImage(games[displayGame].blue.scaleBody(), games[displayGame].blue.tankOrientation);
				games[displayGame].blue.turretOriented = games[displayGame].blue.findOrientedImage(games[displayGame].blue.scaleTurret(), games[displayGame].blue.turretOrientation);
				graphics.DrawImage(games[displayGame].red.bodyOriented, new Point(games[displayGame].red.position.x * Player.xScale, games[displayGame].red.position.y * Player.yScale));
				graphics.DrawImage(games[displayGame].red.turretOriented, new Point(games[displayGame].red.position.x * Player.xScale, games[displayGame].red.position.y * Player.yScale));
				graphics.DrawImage(games[displayGame].blue.bodyOriented, new Point(games[displayGame].blue.position.x * Player.xScale, games[displayGame].blue.position.y * Player.yScale));
				graphics.DrawImage(games[displayGame].blue.turretOriented, new Point(games[displayGame].blue.position.x * Player.xScale, games[displayGame].blue.position.y * Player.yScale));
			}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddMissiles
		/// Parameters:		Graphics graphics
		///                     Use:    Graphics object is used to update the image of the
		///                             current state of the world	
		/// Returns:		None
		/// Description:	Adds the missile images to the existing background image
		/// Last Modified:	02 October 2019
		/// Modification:	Redesigned to used graphics objects as opposed to the previous
		///	                brute force pixel value assignments
		/// </summary>
		private void AddMissiles(Graphics graphics)
		{
			for (int i = 0; i < games[displayGame].red.missiles.Length; ++i)
			{
				if (games[displayGame].red.missiles[i].isActive)
				{
                    games[displayGame].red.missiles[i].missileOriented = games[displayGame].red.missiles[i].findOrientedImage(games[displayGame].red.missiles[i].scaleMissile(), games[displayGame].red.missiles[i].missileOrientation);
                    graphics.DrawImage(games[displayGame].red.missiles[i].missileOriented, new Point(games[displayGame].red.missiles[i].position.x * Missile.xScale, games[displayGame].red.missiles[i].position.y * Missile.yScale));
				}
				else if (games[displayGame].red.missiles[i].isContact)
				{
					graphics.DrawImage(games[displayGame].red.missiles[i].scaleExplosion(), new Point(games[displayGame].red.missiles[i].position.x * Missile.xScale, games[displayGame].red.missiles[i].position.y * Missile.yScale));
				}
			}
			for (int i = 0; i < games[displayGame].blue.missiles.Length; ++i)
			{
				if (games[displayGame].blue.missiles[i].isActive)
				{
                    games[displayGame].blue.missiles[i].missileOriented = games[displayGame].blue.missiles[i].findOrientedImage(games[displayGame].blue.missiles[i].scaleMissile(), games[displayGame].blue.missiles[i].missileOrientation);
                    graphics.DrawImage(games[displayGame].blue.missiles[i].missileOriented, new Point(games[displayGame].blue.missiles[i].position.x * Missile.xScale, games[displayGame].blue.missiles[i].position.y * Missile.yScale));
				}
				else if (games[displayGame].blue.missiles[i].isContact)
				{
					graphics.DrawImage(games[displayGame].blue.missiles[i].scaleExplosion(), new Point(games[displayGame].blue.missiles[i].position.x * Missile.xScale, games[displayGame].blue.missiles[i].position.y * Missile.yScale));
				}
			}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	Render
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Displays the state of the world
		/// Last Modified:	02 October 2019
		/// Modification:	Added comment summary and adjusted general
		///                 structure to support use a local bitmap
		///                 object as opposed to global, as well as the
		///                 use of a Graphics object to perform efficient
		///                 display alterations.
		/// </summary>
		public void Render()
		{
			Player.renderPlayer(games[displayGame].mapBlocks, splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);

			Bitmap bmp = new Bitmap(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);
			using (Graphics graphics = Graphics.FromImage(bmp))
			{
				AddBackground(graphics);
				AddPlayers(graphics);
				AddMissiles(graphics);
			}
			using (Graphics graphics = splitContainer1.Panel2.CreateGraphics())
			{
				graphics.DrawImageUnscaled(bmp, 0, 0);
			}
			bmp.Dispose();
			updateInformationDisplay();
		}

		private void updateInformationDisplay()
		{
			handleInvoke(elapsedTimeDisplay, games[displayGame].elapsed.ToString());
			handleInvoke(currentPotDisplay, games[displayGame].currentWinBonus.ToString());
			handleInvoke(redCurrentFitnessDisplay, games[displayGame].red.organism.fitness.ToString());
			handleInvoke(redNumberOfMovesDisplay, games[displayGame].red.numOfMoves.ToString());
			handleInvoke(redMissilesFiredDisplay, games[displayGame].red.numOfMissilesFired.ToString());
			handleInvoke(blueCurrentFitnessDisplay, games[displayGame].blue.organism.fitness.ToString());
			handleInvoke(blueNumberOfMovesDisplay, games[displayGame].blue.numOfMoves.ToString());
			handleInvoke(blueMissilesFiredDisplay, games[displayGame].blue.numOfMissilesFired.ToString());
			handleInvoke(generationCountDisplay, games[displayGame].generationCount.ToString());
			handleInvoke(redNumberOfWinsDisplay, games[displayGame].numRedWins.ToString());
			handleInvoke(blueNumberOfWinsDisplay, games[displayGame].numBlueWins.ToString());
			handleInvoke(redNumberOfNodesDisplay, games[displayGame].red.organism.botBrain.nextID.ToString());
			handleInvoke(blueNumberOfNodesDisplay, games[displayGame].blue.organism.botBrain.nextID.ToString());
			handleInvoke(redNumberOfEdgesDisplay, games[displayGame].red.organism.botBrain.edges.Count.ToString());
			handleInvoke(blueNumberOfEdgesDisplay, games[displayGame].blue.organism.botBrain.edges.Count.ToString());
			handleInvoke(redPeakFitnessDisplay, games[displayGame].peakRed.ToString());
			handleInvoke(bluePeakFitnessDisplay, games[displayGame].peakBlue.ToString());
		}

		private void handleInvoke(Control control, string text)
		{
			if (control.InvokeRequired)
			{
				var d = new SafeCallDelegate(handleInvoke);
				control.Invoke(d, new object[] { control, text });
			}
			else
			{
				control.Text = text;
			}
		}
	}
}

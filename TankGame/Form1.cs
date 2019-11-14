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
			Render();
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
			if (displayGame.red.isHuman == true)
			{
				displayGame.red.keyController(keyData);
			}
			else if (displayGame.blue.isHuman == true)
			{
				displayGame.blue.keyController(keyData);
			}
			return true;
		}

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
		Game displayGame;

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

			displayGame = games[0];
			Task simulation = new Task(runSimulation);
			simulation.Start();
		}

		public void runSimulation()
		{
			while (true)
			{
				for (int i = 0; i < games.Length; ++i)
				{
					List<int> activeBrains = new List<int>();
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
						}
					}));
				}
				Task.WaitAll(toDo.ToArray());
				updateBrains();
			}
		}

		public void updateBrains()
		{
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
					Mutatinator.cross(brains[parentIDs[lessFitID]].botBrain, 
									  brains[parentIDs[moreFitID]].botBrain));
				Mutatinator.mutate(replacements[sortedFitnesses[replaceIndex].Key]);
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

			displayGame.xMapBlock = 0;
			displayGame.yMapBlock = 0;

			//Iterate through the terrainMapN string created from the JSON file
			displayGame.offset.x = 0;       //reset offsets to 0 to ensure correct
			displayGame.offset.y = 0;       //starting position

			for (int index = 0; index < displayGame.mapBlocks.Length; ++index)
			{
				//Check to see if you've moved beyond the width of the panel
				if (displayGame.offset.x >= Game.WIDTH_IN_TILES * x)
				{
					displayGame.offset.x = 0;
					displayGame.offset.y += y;
				}

				//Check to see if you've gone beyond the height of the panel
				if (displayGame.offset.y >= Game.HEIGHT_IN_TILES * y)
				{
					displayGame.offset.y = 0;
				}

				if (displayGame.xMapBlock >= Game.WIDTH_IN_TILES)
				{
					displayGame.xMapBlock = 0;
					++displayGame.yMapBlock;
				}

				if (displayGame.yMapBlock >= Game.HEIGHT_IN_TILES)
				{
					displayGame.yMapBlock = 0;
				}

				Brush brush = null;
				switch (displayGame.mapBlocks[displayGame.xMapBlock, displayGame.yMapBlock].color)
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
				graphics.FillRectangle(brush, displayGame.offset.x, displayGame.offset.y, x, y);
				brush.Dispose();

				//Increment the offset
				displayGame.offset.x += x;
				++displayGame.xMapBlock;
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
            displayGame.red.bodyOriented = displayGame.red.findOrientedImage(displayGame.red.scaleBody(), displayGame.red.tankOrientation);
            displayGame.red.turretOriented = displayGame.red.findOrientedImage(displayGame.red.scaleTurret(), displayGame.red.turretOrientation);
            displayGame.blue.bodyOriented = displayGame.blue.findOrientedImage(displayGame.blue.scaleBody(), displayGame.blue.tankOrientation);
            displayGame.blue.turretOriented = displayGame.blue.findOrientedImage(displayGame.blue.scaleTurret(), displayGame.blue.turretOrientation);
            graphics.DrawImage(displayGame.red.bodyOriented, new Point(displayGame.red.position.x * Player.xScale, displayGame.red.position.y * Player.yScale));
			graphics.DrawImage(displayGame.red.turretOriented, new Point(displayGame.red.position.x * Player.xScale, displayGame.red.position.y * Player.yScale));
			graphics.DrawImage(displayGame.blue.bodyOriented, new Point(displayGame.blue.position.x * Player.xScale, displayGame.blue.position.y * Player.yScale));
			graphics.DrawImage(displayGame.blue.turretOriented, new Point(displayGame.blue.position.x * Player.xScale, displayGame.blue.position.y * Player.yScale));
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
			for (int i = 0; i < displayGame.red.missiles.Length; ++i)
			{
				if (displayGame.red.missiles[i].isActive)
				{
                    displayGame.red.missiles[i].missileOriented = displayGame.red.missiles[i].findOrientedImage(displayGame.red.missiles[i].scaleMissile(), displayGame.red.missiles[i].missileOrientation);
                    graphics.DrawImage(displayGame.red.missiles[i].missileOriented, new Point(displayGame.red.missiles[i].position.x * Missile.xScale, displayGame.red.missiles[i].position.y * Missile.yScale));
				}
				else if (displayGame.red.missiles[i].isContact)
				{
					graphics.DrawImage(displayGame.red.missiles[i].scaleExplosion(), new Point(displayGame.red.missiles[i].position.x * Missile.xScale, displayGame.red.missiles[i].position.y * Missile.yScale));
				}
			}
			for (int i = 0; i < displayGame.blue.missiles.Length; ++i)
			{
				if (displayGame.blue.missiles[i].isActive)
				{
                    displayGame.blue.missiles[i].missileOriented = displayGame.blue.missiles[i].findOrientedImage(displayGame.blue.missiles[i].scaleMissile(), displayGame.blue.missiles[i].missileOrientation);
                    graphics.DrawImage(displayGame.blue.missiles[i].missileOriented, new Point(displayGame.blue.missiles[i].position.x * Missile.xScale, displayGame.blue.missiles[i].position.y * Missile.yScale));
				}
				else if (displayGame.blue.missiles[i].isContact)
				{
					graphics.DrawImage(displayGame.blue.missiles[i].scaleExplosion(), new Point(displayGame.blue.missiles[i].position.x * Missile.xScale, displayGame.blue.missiles[i].position.y * Missile.yScale));
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
			Player.renderPlayer(displayGame.mapBlocks, splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);

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
			elapsedTimeDisplay.Text = displayGame.elapsed.ToString();
			currentPotDisplay.Text = displayGame.currentWinBonus.ToString();
			redCurrentFitnessDisplay.Text = displayGame.red.organism.fitness.ToString();
			redNumberOfMovesDisplay.Text = displayGame.red.numOfMoves.ToString();
			redMissilesFiredDisplay.Text = displayGame.red.numOfMissilesFired.ToString();
			blueCurrentFitnessDisplay.Text = displayGame.blue.organism.fitness.ToString();
			blueNumberOfMovesDisplay.Text = displayGame.blue.numOfMoves.ToString();
			blueMissilesFiredDisplay.Text = displayGame.blue.numOfMissilesFired.ToString();
			generationCountDisplay.Text = displayGame.generationCount.ToString();
			redNumberOfWinsDisplay.Text = displayGame.numRedWins.ToString();
			blueNumberOfWinsDisplay.Text = displayGame.numBlueWins.ToString();
			redNumberOfNodesDisplay.Text = displayGame.red.organism.botBrain.nextID.ToString();
			blueNumberOfNodesDisplay.Text = displayGame.blue.organism.botBrain.nextID.ToString();
			redNumberOfEdgesDisplay.Text = displayGame.red.organism.botBrain.edges.Count.ToString();
			blueNumberOfEdgesDisplay.Text = displayGame.blue.organism.botBrain.edges.Count.ToString();
			redPeakFitnessDisplay.Text = displayGame.peakRed.ToString();
			bluePeakFitnessDisplay.Text = displayGame.peakBlue.ToString();
		}
	}
}

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

namespace TankGame
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			SetUp();
			Focus();
			Timer timer = new Timer();
			timer.Interval = 5;        //# of milliseconds
			timer.Tick += Timer_Tick;
			timer.Start();
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
		int nextBrainID = 0;
		static Dictionary<int, NeuralNetwork> hallOfFame = new Dictionary<int, NeuralNetwork>();
		static Dictionary<int, NeuralNetwork> brains = new Dictionary<int, NeuralNetwork>();
		Game[] games;
		Game displayGame;

		public void SetUp()
		{
			games = new Game[populationSize / 2];
			for (int i = 0; i < games.Length; ++i)
			{
				games[i] = new Game(new Player(red), new Player(blue), false);
				games[i].red.botBrainID = nextBrainID;
				brains.Add(nextBrainID++, games[i].red.botBrain);
				games[i].blue.botBrainID = nextBrainID;
				brains.Add(nextBrainID++, games[i].blue.botBrain);
			}
            List<Task> toDo = new List<Task>();
            foreach (Game game in games)
            {
                toDo.Add(Task.Run(() => {
                    while (game.worldState == WorldState.GameInProgress)
                    {
                        game.GameLoop();
                    }
                }));
            }
            Task.WaitAll(toDo.ToArray());
			games[0].display = false;
			displayGame = games[0];
		}

		public void updateBrains()
		{
			for (int i = 0; i < games.Length; ++i)
			{
				brains[games[i].red.botBrainID] = games[i].red.botBrain;
				brains[games[i].blue.botBrainID] = games[i].blue.botBrain;
			}
			int maxFitnessID;
			double maxFitnessValue = 0;
			foreach (Game game in games)
			{

			}
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
			redCurrentFitnessDisplay.Text = displayGame.red.fitness.ToString();
			redNumberOfMovesDisplay.Text = displayGame.red.numOfMoves.ToString();
			redMissilesFiredDisplay.Text = displayGame.red.numOfMissilesFired.ToString();
			blueCurrentFitnessDisplay.Text = displayGame.blue.fitness.ToString();
			blueNumberOfMovesDisplay.Text = displayGame.blue.numOfMoves.ToString();
			blueMissilesFiredDisplay.Text = displayGame.blue.numOfMissilesFired.ToString();
			generationCountDisplay.Text = displayGame.generationCount.ToString();
			redNumberOfWinsDisplay.Text = displayGame.numRedWins.ToString();
			blueNumberOfWinsDisplay.Text = displayGame.numBlueWins.ToString();
			redNumberOfNodesDisplay.Text = displayGame.red.botBrain.nextID.ToString();
			blueNumberOfNodesDisplay.Text = displayGame.blue.botBrain.nextID.ToString();
			redNumberOfEdgesDisplay.Text = displayGame.red.botBrain.edges.Count.ToString();
			blueNumberOfEdgesDisplay.Text = displayGame.blue.botBrain.edges.Count.ToString();
			redPeakFitnessDisplay.Text = displayGame.peakRed.ToString();
			bluePeakFitnessDisplay.Text = displayGame.peakBlue.ToString();
		}
	}
}

﻿using System;
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
			Focus();
			InitializeData();
            Timer timer = new Timer();
            timer.Interval = 5;        //# of milliseconds
            timer.Tick += Timer_Tick;
            timer.Start();
		}

        private void Timer_Tick(object sender, EventArgs e)
        {
            GameLoop();
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
			if (red.isHuman == true)
			{
				red.keyController(keyData);
			}
			else if (blue.isHuman == true)
			{
				blue.keyController(keyData);
			}
			return true;
		}

		//START GLOBALS----------------------------------------------------------------------------------

		DateTime startTime = DateTime.Now;
		DateTime currentTime = DateTime.Now;
		TimeSpan elapsed = new TimeSpan();

		//Who's playing?
		const bool isRedHumanPlaying = true;
		const bool isBlueHumanPlaying = false;

        //Pull in external resources and initialize core entity objects
        string terrainMapN = File.ReadAllText("Resources/Maps/TerrainMaps/TerrainMap2.JSON");
		Player red = new Player("Resources/images/Red_TankBody.png", "Resources/images/Turret.png", 
								0, 0, isRedHumanPlaying, true);
		Player blue = new Player("Resources/images/Blue_TankBody.png", "Resources/images/Turret.png", 
								0, 0, isBlueHumanPlaying, false);

		//Define useful variables
		const int WIDTH_IN_TILES = 25;		//Number of tiles in the width of the world
		const int HEIGHT_IN_TILES = 25;		//Number of tiles in the height of the world
		int xMapBlock;						//Used as iterator in mapBlocks[]
		int yMapBlock;						//Used as iterator in mapBlocks[]
		MapBlock[,] mapBlocks = new MapBlock[WIDTH_IN_TILES, HEIGHT_IN_TILES];//The map in gametiles
		Position offset = new Position();   //Position offset in pixels for use when drawing the bitmap

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
			List<Edge> tempEdges = new List<Edge>();
			List<int> outputLayer = new List<int>();
			Random rand = new Random();
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

            //Create the edges of the neural networks to be used

            int inLayerId = 1;
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

            for (int i = 0; i < Player.NUM_OF_INPUTS; ++i)
            {
                for (int command = (int)ControlCommand.NONE; command < (int)ControlCommand.FINAL_UNUSED; ++command)
                {
                    outLayerId = Player.NUM_OF_INPUTS + command + 1;
                    tempEdges.Add(new Edge(inLayerId, outLayerId, 1, 0));
                    if (!outputLayer.Contains(outLayerId))
                    {
                        outputLayer.Add(outLayerId);
                    }
                }
                inLayerId++;
            }

            //Initialize the players
            red.position.x = red.spawnPoint.x;
            red.position.y = red.spawnPoint.y;
			red.botBrain = new NeuralNetwork(tempEdges, MapBlock.numOfStates * WIDTH_IN_TILES * HEIGHT_IN_TILES + (int)ControlCommand.FINAL_UNUSED, outputLayer);

			blue.position.x = blue.spawnPoint.x;
			blue.position.y = blue.spawnPoint.y;
			blue.botBrain = new NeuralNetwork(tempEdges, MapBlock.numOfStates * WIDTH_IN_TILES * HEIGHT_IN_TILES + (int)ControlCommand.FINAL_UNUSED, outputLayer);

            red.calcDegreeAndDistanceToTarget(blue.position.x, blue.position.y);
            blue.calcDegreeAndDistanceToTarget(red.position.x, red.position.y);

            List<double> redInput = new List<double> { red.distanceToTarget, red.degreeToTarget };
            List<double> blueInput = new List<double> { blue.distanceToTarget, blue.degreeToTarget };

            red.botBrain.feedForward(redInput);
            blue.botBrain.feedForward(blueInput);
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
			int x = splitContainer1.Panel2.Width / WIDTH_IN_TILES;
			int y = splitContainer1.Panel2.Height / HEIGHT_IN_TILES;

			xMapBlock = 0;
			yMapBlock = 0;

			//Iterate through the terrainMapN string created from the JSON file
			offset.x = 0;		//reset offsets to 0 to ensure correct
			offset.y = 0;       //starting position

			for (int index = 0; index < mapBlocks.Length; ++index)
			{
				//Check to see if you've moved beyond the width of the panel
				if(offset.x >= WIDTH_IN_TILES * x)
				{
					offset.x = 0;
					offset.y += y;
				}

				//Check to see if you've gone beyond the height of the panel
				if(offset.y >= HEIGHT_IN_TILES * y)
				{
					offset.y = 0;
				}

				if (xMapBlock >= WIDTH_IN_TILES)
				{
					xMapBlock = 0;
					++yMapBlock;
				}

				if (yMapBlock >= HEIGHT_IN_TILES)
				{
					yMapBlock = 0;
				}

                Brush brush = null;
				switch(mapBlocks[xMapBlock, yMapBlock].color)
				{
					case Color.Black:
                        brush = new SolidBrush(System.Drawing.Color.Black);
                        mapBlocks[xMapBlock, yMapBlock].updateStates();
						break;
					case Color.Gray:
                        brush = new SolidBrush(System.Drawing.Color.Gray);
                        break;
					case Color.Red:
                        brush = new SolidBrush(System.Drawing.Color.White);
                        mapBlocks[xMapBlock, yMapBlock].updateStates();
						break;
					case Color.Blue:
                        brush = new SolidBrush(System.Drawing.Color.White);
						mapBlocks[xMapBlock, yMapBlock].updateStates();
						break;
				}
                graphics.FillRectangle(brush, offset.x, offset.y, x, y);
                brush.Dispose();

                //Increment the offset
                offset.x += x;
				++xMapBlock;
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
			graphics.DrawImage(red.bodyOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
			graphics.DrawImage(red.turretOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
			graphics.DrawImage(blue.bodyOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
			graphics.DrawImage(blue.turretOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
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
			for(int i = 0; i < red.missiles.Length; ++i)
			{
				if (red.missiles[i].isActive)
				{
					graphics.DrawImage(red.missiles[i].missileOriented, new Point(red.missiles[i].position.x * Missile.xScale, red.missiles[i].position.y * Missile.yScale));
				}
				else if (red.missiles[i].isContact)
				{
					graphics.DrawImage(red.missiles[i].scaleExplosion(), new Point(red.missiles[i].position.x * Missile.xScale, red.missiles[i].position.y * Missile.yScale));
					red.missiles[i].isContact = false;
				}
			}
			for (int i = 0; i < blue.missiles.Length; ++i)
			{
				if (blue.missiles[i].isActive)
				{
					graphics.DrawImage(blue.missiles[i].missileOriented, new Point(blue.missiles[i].position.x * Missile.xScale, blue.missiles[i].position.y * Missile.yScale));
				}
				else if (blue.missiles[i].isContact)
				{
					graphics.DrawImage(blue.missiles[i].scaleExplosion(), new Point(blue.missiles[i].position.x * Missile.xScale, blue.missiles[i].position.y * Missile.yScale));
					blue.missiles[i].isContact = false;
				}
			}
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
					break;
				case 9:         //Blue Player Spawn
					mapBlocks[xMapBlock, yMapBlock] = new MapBlock(false, true, false, false, true, xMapBlock, yMapBlock, Color.Blue, Player.xScale, Player.yScale);
					blue.spawnPoint.x = xMapBlock;
					blue.spawnPoint.y = yMapBlock;
					break;
			}
			mapBlocks[xMapBlock, yMapBlock].updateStates();

			++xMapBlock;
			if(xMapBlock >= WIDTH_IN_TILES)
			{
				xMapBlock = 0;
				++yMapBlock;
			}
			if(yMapBlock >= HEIGHT_IN_TILES)
			{
				yMapBlock = 0;
			}
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
			currentTime = DateTime.Now;
			elapsed = currentTime - startTime;

			//Determine and assign the Pixel:GameTile ratio and assign it to
			//the static Player and Missile class variables xScale and yScale.
			Player.xScale = splitContainer1.Panel2.Width / WIDTH_IN_TILES;
			Player.yScale = splitContainer1.Panel2.Height / HEIGHT_IN_TILES;
			Missile.xScale = Player.xScale;
			Missile.yScale = Player.yScale;

			//Update players and missiles
			red.updatePlayer(mapBlocks);
			blue.updatePlayer(mapBlocks);
			for (int i = 0; i < red.missiles.Length; ++i)
			{
				red.missiles[i].updateMissile(mapBlocks);
			}
			for (int i = 0; i < red.missiles.Length; ++i)
			{
				blue.missiles[i].updateMissile(mapBlocks);
			}
            red.calcDegreeAndDistanceToTarget(blue.position.x, blue.position.y);
            blue.calcDegreeAndDistanceToTarget(red.position.x, red.position.y);
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
        private void Render()
		{
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
			Render();
		}
	}
}

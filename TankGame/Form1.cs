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
			GameLoop();
			Application.Idle += HandleApplicationIdle;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Handler Name:	HandleApplicationIdle
		/// Parameters:		Included but not used
		/// Returns:		None
		/// Description:	When the Application.Idle event is triggered, this
		///					handler is called. This acts as a simple Game Loop timer
		///	Last Modified:	19 September 2019
		///	Modification:	Initialized handler and added comments
		/// </summary>
		void HandleApplicationIdle(object sender, EventArgs e)
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
								0, 0, isRedHumanPlaying);
		Player blue = new Player("Resources/images/Blue_TankBody.png", "Resources/images/Turret.png", 
								0, 0, isBlueHumanPlaying);

		//Define useful variables
		const int WIDTH_IN_TILES = 25;		//Number of tiles in the width of the world
		const int HEIGHT_IN_TILES = 25;		//Number of tiles in the height of the world
		int xMapBlock;						//Used as iterator in mapBlocks[]
		int yMapBlock;						//Used as iterator in mapBlocks[]
		MapBlock[,] mapBlocks = new MapBlock[WIDTH_IN_TILES, HEIGHT_IN_TILES];//The map in gametiles
		Position offset = new Position();   //Position offset in pixels for use when drawing the bitmap
		Bitmap bitmap;                      //The map in pixels
		Bitmap bufferMap;
		bool[] mapState = new bool[WIDTH_IN_TILES * HEIGHT_IN_TILES];
		int mapStateIterator = 0;

		//END GLOBALS------------------------------------------------------------------------------------

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	InitializeData()
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Iterates through the JSON map file and defines the starting
		///					points for each entity.
		///	Last Modified:	19 September 2019
		///	Modification:	Added comments
		/// </summary>
		private void InitializeData()
		{
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
				++mapStateIterator;
			}
			red.position.x = red.spawnPoint.x;
			red.position.y = red.spawnPoint.y;

			blue.position.x = blue.spawnPoint.x;
			blue.position.y = blue.spawnPoint.y;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddBackGround
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Determines the definition of each pixel in the bitmap
		///					based on the values found in the mapBlocks array of
		///					MapBlock objects and adds them to the world bitmap
		///	Last Modified:	19 September 2019
		///	Modification:	Added comments and optimized memory usage
		/// </summary>
		private void AddBackground()
		{
			//Proportional number of pixels that represent the pixel per game tile
			int x = splitContainer1.Panel2.Width / WIDTH_IN_TILES;
			int y = splitContainer1.Panel2.Height / HEIGHT_IN_TILES;

			//Bitmap to be used
			if(bitmap != null)
			{
				bitmap.Dispose();
			}
			bitmap = new Bitmap(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);

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

				switch(mapBlocks[xMapBlock, yMapBlock].color)
				{
					case Color.Black:
						for (int y_index = 0 + offset.y; y_index < y + offset.y; ++y_index)
						{
							for (int x_index = 0 + offset.x; x_index < x + offset.x; ++x_index)
							{
								bitmap.SetPixel(x_index, y_index, System.Drawing.Color.Black);
							}
						}
						break;
					case Color.Gray:
						for (int y_index = 0 + offset.y; y_index < y + offset.y; ++y_index)
						{
							for (int x_index = 0 + offset.x; x_index < x + offset.x; ++x_index)
							{
								bitmap.SetPixel(x_index, y_index, System.Drawing.Color.Gray);
							}
						}
						break;
					case Color.Red:
						for (int y_index = 0 + offset.y; y_index < y + offset.y; ++y_index)
						{
							for (int x_index = 0 + offset.x; x_index < x + offset.x; ++x_index)
							{
								bitmap.SetPixel(x_index, y_index, System.Drawing.Color.White);
							}
						}
						break;
					case Color.Blue:
						for (int y_index = 0 + offset.y; y_index < y + offset.y; ++y_index)
						{
							for (int x_index = 0 + offset.x; x_index < x + offset.x; ++x_index)
							{
								bitmap.SetPixel(x_index, y_index, System.Drawing.Color.White);
							}
						}
						break;
					default:
						break;
				}

				//Increment the offset
				offset.x += x;
				++xMapBlock;
			}
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddPlayers
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Adds the player images to the existing bitmap
		/// Last Modified:	19 September 2019
		/// Modification:	Added comments and resolved memory issues
		/// </summary>
		private void AddPlayers()
		{
			//Temporary bitmap to be used for manipulation
			Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);

			using (Graphics gr = Graphics.FromImage(bm))
			{
				gr.DrawImage(bitmap, new Point(0, 0));
				gr.DrawImage(red.bodyOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
				gr.DrawImage(red.turretOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
				gr.DrawImage(blue.bodyOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
				gr.DrawImage(blue.turretOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
			}

			//Remove previous map definition
			bitmap.Dispose();

			//Assign new map definition
			bitmap = bm;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	AddMissiles
		/// Parameters:		None	
		/// Returns:		None
		/// Description:	Adds the missile images to the existing bitmap.
		/// Last Modified:	19 September 2019
		/// Modification:	Creation and implementation of function/comments
		/// </summary>
		private void AddMissiles()
		{
			//Temporary bitmap to be used for manipulation
			Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);

			using (Graphics gr = Graphics.FromImage(bm))
			{
				gr.DrawImage(bitmap, new Point(0, 0));
				for(int i = 0; i < red.missiles.Length; ++i)
				{
					if (red.missiles[i].isActive)
					{
						gr.DrawImage(red.missiles[i].missileOriented, new Point(red.missiles[i].position.x * Missile.xScale, red.missiles[i].position.y * Missile.yScale));
					}
					else if (red.missiles[i].isContact)
					{
						gr.DrawImage(red.missiles[i].scaleExplosion(), new Point(red.missiles[i].position.x * Missile.xScale, red.missiles[i].position.y * Missile.yScale));
						red.missiles[i].isContact = false;
					}
				}
				for (int i = 0; i < blue.missiles.Length; ++i)
				{
					if (blue.missiles[i].isActive)
					{
						gr.DrawImage(blue.missiles[i].missileOriented, new Point(blue.missiles[i].position.x * Missile.xScale, blue.missiles[i].position.y * Missile.yScale));
					}
					else if (blue.missiles[i].isContact)
					{
						gr.DrawImage(blue.missiles[i].scaleExplosion(), new Point(blue.missiles[i].position.x * Missile.xScale, blue.missiles[i].position.y * Missile.yScale));
						blue.missiles[i].isContact = false;
					}
				}
			}

			//Remove previous map definition
			bitmap.Dispose();

			//Assign new map definition
			bitmap = bm;
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
				default:
					return;
			}

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
		}

		private void Render()
		{
			AddBackground();
			AddPlayers();
			AddMissiles();
			bufferMap = bitmap;
			splitContainer1.Panel2.BackgroundImage = bufferMap;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	GameLoop
		/// Parameters:		None
		/// Returns:		None
		/// Description:	The typical game loop found in games. While not defined
		///					as "Render()" and "Update()", the steps found within 
		///					serve the same purpose.
		/// Last Modified:	19 September 2019
		/// Modification:	Added comments.
		/// </summary>
		public void GameLoop()
		{
			Update();
			Render();
		}
	}
}

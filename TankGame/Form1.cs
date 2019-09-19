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
			Focus();
			InitializeData();
			GameLoop();
			Application.Idle += HandleApplicationIdle;
		}

		void HandleApplicationIdle(object sender, EventArgs e)
		{
			GameLoop();
		}

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
			GameLoop();
			return true;
		}

		//Globals
		const bool isRedHumanPlaying = true;
		const bool isBlueHumanPlaying = false;
		float rating = 1;
		string terrainMapN = File.ReadAllText("Resources/Maps/TerrainMaps/TerrainMap1.JSON"); //Read the file into a single string for easier manipulation
		Player red = new Player("Resources/images/Red_TankBody.png", "Resources/images/Turret.png", 0, 0, isRedHumanPlaying);
		Player blue = new Player("Resources/images/Blue_TankBody.png", "Resources/images/Turret.png", 0, 0, isBlueHumanPlaying);
		const int WIDTH_IN_TILES = 25;		//Number of tiles in the width of the world
		const int HEIGHT_IN_TILES = 25;     //Number of tiles in the height of the world
		int xMapBlock;
		int yMapBlock;
		MapBlock[,] mapBlocks = new MapBlock[WIDTH_IN_TILES, HEIGHT_IN_TILES];
		Position offset = new Position();   //Position offset for use when drawing the bitmap
		Bitmap bitmap;

		Timer timer1 = new Timer();
		TimeSpan elapsed;
		DateTime startTime = DateTime.Now;

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
			}
			red.position.x = red.spawnPoint.x;
			red.position.y = red.spawnPoint.y;

			blue.position.x = blue.spawnPoint.x;
			blue.position.y = blue.spawnPoint.y;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	DrawBackGround
		/// Parameters:		None
		/// Returns:		None
		/// Description:	Intereprets JSON file and translates data into a bitmap for the background,
		///					then draws this bitmap to the background.
		/// </summary>
		private void DrawBackground()
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
			//Draw the newly created bitmap to the panel
			splitContainer1.Panel2.BackgroundImage = bitmap;
		}

		private void DrawPlayers()
		{
			Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);

			using (Graphics gr = Graphics.FromImage(bm))
			{
				gr.DrawImage(bitmap, new Point(0, 0));
				gr.DrawImage(red.bodyOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
				gr.DrawImage(red.turretOriented, new Point(red.position.x * Player.xScale, red.position.y * Player.yScale));
				gr.DrawImage(blue.bodyOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
				gr.DrawImage(blue.turretOriented, new Point(blue.position.x * Player.xScale, blue.position.y * Player.yScale));
			}
			bitmap.Dispose();
			bitmap = bm;
		}

		private void DrawMissiles()
		{
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
			bitmap.Dispose();
			bitmap = bm;
		}

		/// <summary>
		/// Developer:		Anthony Harris
		/// Function Name:	TileType
		/// Parameters:		int typeSpecifier
		///						Use:	Integer value representative of the type of game tile being added
		///								to the bitmap. Ex: wall, floor.
		///					ref Bitmap map
		///						Use:	Reference to the preceding bitmap that will be drawn to the
		///								background. Will be altered in accordance to the typeSpecifier.
		///					int offset.x
		///						Use:	Integer value representing the offset in the x-direction where the
		///								unit of measurement is in pixels.
		///					int offset.y
		///						Use:	Integer value representing the offset in the y-direction where the
		///								unit of measurement is in pixels.
		/// Returns:		None
		/// Description:	Manipulates the bitmap representation of the game world and defines the game
		///					tiles.
		/// </summary>
		private void TileType(int typeSpecifier/*ref Bitmap map*/)
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

		private float CalcRating()
		{
			return 0;
		}


		public void GameLoop()
		{
			//Proportional number of pixels that represent the pixel per game tile
			Player.xScale = splitContainer1.Panel2.Width / WIDTH_IN_TILES;
			Player.yScale = splitContainer1.Panel2.Height / HEIGHT_IN_TILES;
			Missile.xScale = Player.xScale;
			Missile.yScale = Player.yScale;
			red.updatePlayer(mapBlocks);
			blue.updatePlayer(mapBlocks);
			for(int i = 0; i < red.missiles.Length; ++i)
			{
				red.missiles[i].updateMissile(mapBlocks);
			}
			for (int i = 0; i < red.missiles.Length; ++i)
			{
				blue.missiles[i].updateMissile(mapBlocks);
			}
			DrawBackground();
			DrawPlayers();
			DrawMissiles();
			splitContainer1.Panel2.BackgroundImage = bitmap;
		}
	}
}

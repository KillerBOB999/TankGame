using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
	public class MapBlock
	{
		public bool isOccupied = false;
		public bool isFloor = false;
		public bool isWall = false;
		public bool isRedSpawn = false;
		public bool isBlueSpawn = false;
		public Position position = new Position();          //In game tiles
		public static int xRatio;
		public static int yRatio;

		public Color color;

		public MapBlock(bool isOcc, bool isFlo, bool isWal, bool isRed, bool isBlu, int xPos, int yPos, Color col, int xRat, int yRat)
		{
			isOccupied = isOcc;
			isFloor = isFlo;
			isWall = isWal;
			isRedSpawn = isRed;
			isBlueSpawn = isBlu;
			position.x = xPos;
			position.y = yPos;
			color = col;
			xRatio = xRat;
			yRatio = yRat;
		}
	}
}

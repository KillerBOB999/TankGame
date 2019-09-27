using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TankGame
{
	public class MapBlock
	{
		public static int xRatio;
		public static int yRatio;

		public bool isOccupied = false;
		public bool isOccRed = false;
		public bool isOccBlue = false;
		public bool isOccMissile = false;
		public bool isFloor = false;
		public bool isWall = false;
		public bool isRedSpawn = false;
		public bool isBlueSpawn = false;

		public static int numOfStates = 2;
		public WallState wallState;
		public FloorState floorState;

		public Position position = new Position();          //In game tiles

		public Color color;

		public enum WallState
		{
			floor, wall
		}
		public enum FloorState
		{
			none, missile, red, blue
		}

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

			isOccRed = isRed;
			isOccBlue = isBlu;
		}

		public void updateStates()
		{
			if (isFloor)
			{
				wallState = WallState.floor;
				if (isOccRed)
				{
					floorState = FloorState.red;
				}
				else if (isOccBlue)
				{
					floorState = FloorState.blue;
				}
				else if (isOccMissile)
				{
					floorState = FloorState.missile;
				}
			}
			else
			{
				wallState = WallState.wall;
				floorState = FloorState.none;
			}
		}
	}
}

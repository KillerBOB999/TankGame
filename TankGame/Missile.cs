using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TankGame
{
	public class Missile
	{
		public static Bitmap missileBase = new Bitmap(Image.FromFile("Resources/images/BaseRocket.png"));
		public Bitmap missileOriented;
		public bool isActive = false;
		public static int xScale = 1;
		public static int yScale = 1;
		public Position position = new Position();
		public Velocity velocity = new Velocity();
		public Orientation missileOrientation = new Orientation();

		public Bitmap findOrientedImage(Bitmap baseImage, Orientation orientation)
		{
			switch (orientation)
			{
				case Orientation.North:
					return baseImage;
				case Orientation.East:
					baseImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
					return baseImage;
				case Orientation.South:
					baseImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
					return baseImage;
				case Orientation.West:
					baseImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
					return baseImage;
				default:
					return baseImage;
			}
		}

		public void updateMissile(MapBlock[,] map)
		{
			if (isActive)
			{
				Position newPosition = new Position(position.x + velocity.x, position.y + velocity.y);

				if (!map[newPosition.x, newPosition.y].isWall && !map[newPosition.x, newPosition.y].isOccupied)
				{
					position.x = newPosition.x;
					position.y = newPosition.y;
				}
				else
				{
					isActive = false;
				}

				missileOriented = findOrientedImage(scaleMissile(), missileOrientation);
			}
		}

		public Bitmap scaleMissile()
		{
			return new Bitmap(missileBase, new Size(xScale, yScale));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TankGame
{
	/// <summary>
	/// Developer:		Anthony Harris
	/// Class Name:		Missile
	/// Description:	Represents the missiles shot by the tanks
	///	Last Modified:	01 October 2019
	///	Modification:	Added relative path functionality
	/// </summary>
	public class Missile
	{
		public static Bitmap missileBase = new Bitmap(Image.FromFile("Resources/images/BaseRocket.png"));
		public static Bitmap explosion = new Bitmap(Image.FromFile("Resources/images/BaseExplosion.png"));
		public Bitmap missileOriented;
		public Bitmap missileScaled;
		public Bitmap explosionScaled;
		public bool isActive = false;
		public bool isContact = false;
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

		public void fireMissile(Orientation turretOrientation, Position tankPosition)
		{
			isActive = true;
			missileOrientation = turretOrientation;
			switch (missileOrientation)
			{
				case Orientation.North:
					velocity.x = 0;
					velocity.y = -1;	
					break;
				case Orientation.East:
					velocity.x = 1;
					velocity.y = 0;
					break;
				case Orientation.South:
					velocity.x = 0;
					velocity.y = 1;
					break;
				case Orientation.West:
					velocity.x = -1;
					velocity.y = 0;
					break;
			}
			position.x = tankPosition.x;
			position.y = tankPosition.y;
		}

		public void updateMissile(MapBlock[,] map)
		{
			if (isActive)
			{
				Position newPosition = new Position(position.x + velocity.x, position.y + velocity.y);

				map[position.x, position.y].isOccupied = false;
				map[position.x, position.y].isOccMissile = false;
				if (!map[newPosition.x, newPosition.y].isWall)
				{
					position.x = newPosition.x;
					position.y = newPosition.y;
					if(map[position.x, position.y].isOccupied)
					{
						isActive = false;
						isContact = true;
					}
				}
				else
				{
					isActive = false;
					isContact = true;
				}
				if (isActive)
				{
					map[newPosition.x, newPosition.y].isOccupied = true;
					map[newPosition.x, newPosition.y].isOccMissile = true;
				}

				missileOriented = findOrientedImage(scaleMissile(), missileOrientation);
				newPosition = null;
			}
		}

		public Bitmap scaleMissile()
		{
			if(missileScaled != null) {
				missileScaled.Dispose();
			}
			missileScaled = new Bitmap(missileBase, new Size(xScale, yScale));
			return missileScaled;
		}

		public Bitmap scaleExplosion()
		{
			if(explosionScaled != null)
			{
				explosionScaled.Dispose();
			}
			explosionScaled = new Bitmap(explosion, new Size(xScale, yScale));
			return explosionScaled;
		}
	}
}

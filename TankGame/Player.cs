using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TankGame
{
	public class Player
	{
		public bool isHuman;
		public Bitmap bodyBase;
		public Bitmap turretBase;
		public Bitmap bodyOriented;
		public Bitmap turretOriented;
		public static int xScale = 1;
		public static int yScale = 1;
		public Position spawnPoint = new Position();
		public Position position = new Position();
		public Velocity velocity = new Velocity();
		public Orientation tankOrientation = new Orientation();
		public Orientation turretOrientation = new Orientation();
		public Missile[] missiles = { new Missile(), new Missile() };

		//Constructor
		public Player(string tankBody, string tankTurret, Orientation bodyOrientation, Orientation gunOrientation, bool isHum)
		{
			bodyBase = new Bitmap(Image.FromFile(tankBody));
			turretBase = new Bitmap(Image.FromFile(tankTurret));
			tankOrientation = bodyOrientation;
			turretOrientation = gunOrientation;
			bodyOriented = findOrientedImage(bodyBase, bodyOrientation);
			turretOriented = findOrientedImage(turretBase, turretOrientation);
			isHuman = isHum;
		}

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

		public void keyController(Keys pressedKey)
		{
			switch (pressedKey)
			{
				case Keys.Up:
					playerController(ControlCommand.Up, ControlCommand.NONE);
					return;
				case Keys.Right:
					playerController(ControlCommand.Right, ControlCommand.NONE);
					return;
				case Keys.Down:
					playerController(ControlCommand.Down, ControlCommand.NONE);
					return;
				case Keys.Left:
					playerController(ControlCommand.Left, ControlCommand.NONE);
					return;
				case Keys.W:
					playerController(ControlCommand.NONE, ControlCommand.Up);
					return;
				case Keys.D:
					playerController(ControlCommand.NONE, ControlCommand.Right);
					return;
				case Keys.S:
					playerController(ControlCommand.NONE, ControlCommand.Down);
					return;
				case Keys.A:
					playerController(ControlCommand.NONE, ControlCommand.Left);
					return;
				case Keys.Space:
					playerController(ControlCommand.NONE, ControlCommand.Space);
					return;
			}
		}

		public void playerController(ControlCommand tankCommand, ControlCommand turretCommand)
		{
			switch (tankCommand)
			{
				case ControlCommand.NONE:
					break;
				case ControlCommand.Up:
					velocity.x = 0;
					velocity.y = -1;
					tankOrientation = Orientation.North;
					return;
				case ControlCommand.Right:
					velocity.x = 1;
					velocity.y = 0;
					tankOrientation = Orientation.East;
					return;
				case ControlCommand.Down:
					velocity.x = 0;
					velocity.y = 1;
					tankOrientation = Orientation.South;
					return;
				case ControlCommand.Left:
					velocity.x = -1;
					velocity.y = 0;
					tankOrientation = Orientation.West;
					return;
			}

			switch (turretCommand)
			{
				case ControlCommand.NONE:
					break;
				case ControlCommand.Up:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.North;
					return;
				case ControlCommand.Right:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.East;
					return;
				case ControlCommand.Down:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.South;
					return;
				case ControlCommand.Left:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.West;
					return;
				// TODO: FINISH MISSILE IMPLEMENTATION
				case ControlCommand.Space:
					if (missiles[0].isActive)
					{
						missiles[1].isActive = true;
					}
					else
					{
						missiles[0].isActive = true;
					}
					return;
			}

			return;
		}

		public void updatePlayer(MapBlock[,] map)
		{
			Position newPosition = new Position(position.x + velocity.x, position.y + velocity.y);

			if(map[newPosition.x, newPosition.y].isFloor)
			{
				position.x = newPosition.x;
				position.y = newPosition.y;
			}

			bodyOriented = findOrientedImage(scaleBody(), tankOrientation);
			turretOriented = findOrientedImage(scaleTurret(), turretOrientation);
		}

		public Bitmap scaleBody()
		{
			return new Bitmap(bodyBase, new Size(xScale, yScale));
		}

		public Bitmap scaleTurret()
		{
			return new Bitmap(turretBase, new Size(xScale, yScale));
		}
	}
}

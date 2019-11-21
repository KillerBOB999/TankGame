using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using TankGame.Entities;

namespace TankGame
{
	public class Player
	{
        public Organism organism = new Organism();
		public List<double> botInput = new List<double>() { 0, 0, 0, 0 };
		public static double baseFitness = 1000;
		public double winBonus = 0;
		public int numOfMoves = 0;
		public int numOfMissilesFired = 0;

		public bool isHuman;
		public bool isRedPlayer;
		public bool isAlive = true;
		public Bitmap bodyBase;
		public Bitmap turretBase;
		public Bitmap bodyOriented;
		public Bitmap turretOriented;
		public Bitmap bodyScaled;
		public Bitmap turretScaled;
		public static int xScale = 1;
		public static int yScale = 1;
		public Position spawnPoint = new Position();
		public Position position = new Position();
		public Velocity velocity = new Velocity();
		public Orientation tankOrientation = new Orientation();
		public Orientation turretOrientation = new Orientation();
		public Missile[] missiles = { new Missile(), new Missile() };

        //Temporary inputs to simple net
        public static int NUM_OF_INPUTS = 4;
        public double distanceToTarget;
        public double degreeToTarget;
        public double distanceToNearestMissile;
        public double degreeToNearestMissile;
        

		//Constructor
		public Player(string tankBody, string tankTurret, Orientation bodyOrientation, Orientation gunOrientation, bool isHum, bool isRed)
		{
			bodyBase = new Bitmap(Image.FromFile(tankBody));
			turretBase = new Bitmap(Image.FromFile(tankTurret));
			tankOrientation = bodyOrientation;
			turretOrientation = gunOrientation;
			bodyOriented = findOrientedImage(bodyBase, bodyOrientation);
			turretOriented = findOrientedImage(turretBase, turretOrientation);
			isHuman = isHum;
			isRedPlayer = isRed;

			if (!isHuman)
			{
				organism.botBrain = new NeuralNetwork();
			}
		}

		public Player(string tankBody, string tankTurret, Orientation bodyOrientation, Orientation gunOrientation, bool isHum, bool isRed, int brainID, NeuralNetwork brain)
		{
			bodyBase = new Bitmap(Image.FromFile(tankBody));
			turretBase = new Bitmap(Image.FromFile(tankTurret));
			tankOrientation = bodyOrientation;
			turretOrientation = gunOrientation;
			bodyOriented = findOrientedImage(bodyBase, bodyOrientation);
			turretOriented = findOrientedImage(turretBase, turretOrientation);
			isHuman = isHum;
			isRedPlayer = isRed;
            organism.botBrain = brain;
            organism.botBrainID = brainID;
		}

		public Player(Player inPlayer)
		{
			bodyBase = inPlayer.bodyBase;
			turretBase = inPlayer.turretBase;
			tankOrientation = inPlayer.tankOrientation;
			turretOrientation = inPlayer.turretOrientation;
			bodyOriented = findOrientedImage(bodyBase, inPlayer.tankOrientation);
			turretOriented = findOrientedImage(turretBase, turretOrientation);
		}

		public void reset(List<double> maxes)
		{
			position.x = spawnPoint.x;
			position.y = spawnPoint.y;
			velocity.x = 0;
			velocity.y = 0;
			tankOrientation = Orientation.North;
			turretOrientation = Orientation.North;
			numOfMissilesFired = 0;
			numOfMoves = 0;
            winBonus = 0;
            distanceToNearestMissile = maxes[2];
            degreeToNearestMissile = maxes[3];
			for (int i = 0; i < missiles.Length; ++i)
			{
				missiles[i].isActive = false;
				missiles[i].isContact = false;
			}
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

        public void calcDegreeAndDistanceToTarget(Player target, List<Double>inputMaxes)
        {
            bool isTank = true;
            bool isActiveMissile = false;
            int nearestMissileID = -1;
            int deltaX;
            int deltaY;
            
            deltaX = position.x - target.position.x;
            deltaY = position.y - target.position.y;

            distanceToTarget = calcDistance(deltaX, deltaY);
            degreeToTarget = calcDegree(deltaX, deltaY, isTank, isActiveMissile);
            isTank = false;
            
            for(int i = 0; i < target.missiles.Length; ++i)
            {
                if (target.missiles[i].isActive)
                {
                    isActiveMissile = true;
                    nearestMissileID = i;
                    deltaX = position.x - target.missiles[i].position.x;
                    deltaY = position.y - target.missiles[i].position.y;
                    double tempDistance = calcDistance(deltaX, deltaY);
                    if(tempDistance < distanceToNearestMissile)
                    {
                        distanceToNearestMissile = tempDistance;
                    }
                }
            }
            deltaX = 0;
            deltaY = 0;
            if (isActiveMissile)
            {
                deltaX = position.x - target.missiles[nearestMissileID].position.x;
                deltaY = position.y - target.missiles[nearestMissileID].position.y;
            }
            degreeToNearestMissile = calcDegree(deltaX, deltaY, isTank, isActiveMissile);
            botInput[0] = distanceToTarget;
			botInput[1] = degreeToTarget;
            botInput[2] = distanceToNearestMissile;
            botInput[3] = degreeToNearestMissile;
			standardizeValues(inputMaxes);
        }

        public double calcDistance(int deltaX, int deltaY)
        {
            return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
        }

        public double calcDegree(int deltaX, int deltaY, bool isTank, bool isMissile)
        {
            if (deltaX == 0 && deltaY == 0)
            {
                return -2 * Math.PI;
            }
            if (deltaX == 0)
            {
                if (deltaY > 0)
                {
                    return Math.PI;
                }
                else
                {
                    return -Math.PI;
                }
            }
            else
            {
                if (isTank && !isMissile)
                {
                    switch (turretOrientation)
                    {
                        case Orientation.North:
                            deltaY--;
                            break;
                        case Orientation.East:
                            deltaX++;
                            break;
                        case Orientation.South:
                            deltaY++;
                            break;
                        case Orientation.West:
                            deltaX--;
                            break;
                    }
                }
                return Math.Atan((double)deltaY / (double)deltaX);
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
					break;
				case ControlCommand.Right:
					velocity.x = 1;
					velocity.y = 0;
					tankOrientation = Orientation.East;
					break;
				case ControlCommand.Down:
					velocity.x = 0;
					velocity.y = 1;
					tankOrientation = Orientation.South;
					break;
				case ControlCommand.Left:
					velocity.x = -1;
					velocity.y = 0;
					tankOrientation = Orientation.West;
					break;
			}
			if (tankCommand != ControlCommand.NONE)
			{
				++numOfMoves;
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
					break;
				case ControlCommand.Right:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.East;
					break;
				case ControlCommand.Down:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.South;
					break;
				case ControlCommand.Left:
					velocity.x = 0;
					velocity.y = 0;
					turretOrientation = Orientation.West;
					break;
				case ControlCommand.Space:
					if (missiles[0].isActive && !missiles[1].isActive)
					{
						missiles[1].fireMissile(turretOrientation, position);
						++numOfMissilesFired;
					}
					else if(!missiles[0].isActive)
					{
						missiles[0].fireMissile(turretOrientation, position);
						++numOfMissilesFired;
					}
					return;
			}
			if (turretCommand != ControlCommand.NONE)
			{
				++numOfMoves;
			}
			return;
		}

		public void updatePlayer(MapBlock[,] map)
		{
			Position newPosition = new Position(position.x + velocity.x, position.y + velocity.y);

			if(map[newPosition.x, newPosition.y].isFloor && !map[newPosition.x, newPosition.y].isOccupied)
			{
				map[position.x, position.y].isOccupied = false;
				if (isRedPlayer)
				{
					map[position.x, position.y].isOccRed = false;
				}
				else
				{
					map[position.x, position.y].isOccBlue = false;
				}
				position.x = newPosition.x;
				position.y = newPosition.y;
				map[position.x, position.y].isOccupied = true;
				if (isRedPlayer)
				{
					map[position.x, position.y].isOccRed = true;
				}
				else
				{
					map[position.x, position.y].isOccBlue = true;
				}
			}

			velocity.x = 0;
			velocity.y = 0;
		}

		public static void renderPlayer(MapBlock[,] map, int width, int height)
		{
			//Determine and assign the Pixel:GameTile ratio and assign it to
			//the static Player and Missile class variables xScale and yScale.
			Player.xScale = width / Game.WIDTH_IN_TILES;
			Player.yScale = height / Game.HEIGHT_IN_TILES;
			Missile.xScale = Player.xScale;
			Missile.yScale = Player.yScale;
		}

		public Bitmap scaleBody()
		{
			if(bodyScaled != null)
			{
				bodyScaled.Dispose();
			}
			bodyScaled = new Bitmap(bodyBase, new Size(xScale, yScale));
			return bodyScaled;
		}

		public Bitmap scaleTurret()
		{
			if (turretScaled != null)
			{
				turretScaled.Dispose();
			}
			turretScaled = new Bitmap(turretBase, new Size(xScale, yScale));
			return turretScaled;
		}

		public void calcFitness(Player target, TimeSpan elapsed)
		{
            if (target.distanceToNearestMissile != 0)
            {
                organism.fitness = winBonus + (100 / target.distanceToNearestMissile) + (baseFitness / (numOfMoves + numOfMissilesFired + elapsed.TotalSeconds));
            }
            else
            {
                organism.fitness = winBonus + (baseFitness / (numOfMoves + numOfMissilesFired + elapsed.TotalSeconds));
            }
        }

        public void calcFitness(Player target, int numIterations)
        {
			int fireScale = 3;
            if (target.distanceToNearestMissile != 0)
            {
                organism.fitness = winBonus + ((100 * fireScale) / target.distanceToNearestMissile) + (baseFitness / (numOfMissilesFired + numIterations)) - target.organism.fitness / 2;
            }
            else
            {
                organism.fitness = winBonus + (baseFitness / (numOfMoves + numOfMissilesFired + numIterations)) - target.organism.fitness / 2;
            }
        }

        public void standardizeValues(List<double>inputMaxes)
		{
            for (int i = 0; i < inputMaxes.Count; ++i)
            {
                botInput[i] = botInput[i] / inputMaxes[i];
            }
		}
	}
}

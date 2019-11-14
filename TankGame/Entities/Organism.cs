using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.Entities
{
    public class Organism
    {
        public NeuralNetwork botBrain;
        public int botBrainID;
        public double fitness;
        static public int nextBrainID = 0;

        public Organism()
        {

        }
        public Organism(NeuralNetwork NN, int ID)
        {
            botBrain = NN;
            botBrainID = ID;
        }

        public Organism(NeuralNetwork NN, int ID, double fitnessScore)
        {
            botBrain = NN;
            botBrainID = ID;
            fitness = fitnessScore;
        }
    }
}

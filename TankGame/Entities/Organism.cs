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
        public int brainID;

        public Organism(NeuralNetwork NN, int ID)
        {
            botBrain = NN;
            brainID = ID;
        }
    }
}

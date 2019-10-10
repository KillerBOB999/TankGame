using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    public class Edge
	{
        public int inNeuronID;
        public int outNeuronID;
        public double weight;
        public double bias;

		public Edge(int inNeuron, int outNeuron, double w, double b)
		{
			inNeuronID = inNeuron;
			outNeuronID = outNeuron;
			weight = w;
			bias = b;
		}

        public Edge(Edge edge)
        {
            inNeuronID = edge.inNeuronID;
            outNeuronID = edge.outNeuronID;
            weight = edge.weight;
            bias = edge.bias;
        }
	}
}

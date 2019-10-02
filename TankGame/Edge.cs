using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    public class Edge
	{
        int inNeuronID;
		int outNeuronID;
		float weight;
		float bias;

		public Edge(int inNeuron, int outNeuron, float w, float b)
		{
			inNeuronID = inNeuron;
			outNeuronID = outNeuron;
			weight = w;
			bias = b;
		}
	}
}

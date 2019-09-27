using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
	public class NeuralNetwork
	{
		List<Edge> edges;
		int nextID;
		List<int> currentInputLayerIDs;

		public NeuralNetwork(List<Edge> e, int nID, List<int> inputLayerIDs)
		{
			edges = e;
			nextID = nID;
			currentInputLayerIDs = inputLayerIDs;
		}

		public NeuralNetwork() { }
	}
}

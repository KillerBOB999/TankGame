using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
	public class NeuralNetwork
	{
		public List<Edge> edges;
		public int nextID;
        public List<int> outputLayerIDs;
        public List<int> topologicalOrdering;
        public static int sizeOfInputLayer;
        public static int sizeOfOutputLayer;
        
		public NeuralNetwork(List<Edge> e, int nID, List<int> outLayerIDs)
		{
			edges = e;
			nextID = nID;
            outputLayerIDs = outLayerIDs;
            topologicalOrdering = new List<int>();
            for(int i = 0; i < nextID; ++i)
            {
                topologicalOrdering.Add(i);
            }
		}

		public NeuralNetwork() { }

        public Dictionary<int, double> feedForward(List<double>inputActivationValues)
        {
            sizeOfInputLayer = inputActivationValues.Count;
            int nodeID = 1;
            Dictionary<int, double> activationValues = new Dictionary<int, double>();
            Dictionary<int, double> currentProgress = new Dictionary<int, double>();
            Dictionary<int, double> outputLayer = new Dictionary<int, double>();

            foreach (var inputValue in inputActivationValues)
            {
                activationValues.Add(nodeID, inputValue);
                ++nodeID;
            }
            
            for(int i = 0; i < topologicalOrdering.Count; ++i)
            {
                foreach(Edge edge in edges)
                {
                    if(edge.inNeuronID == topologicalOrdering[i])
                    {
                        if (!currentProgress.ContainsKey(edge.outNeuronID))
                        {
                            currentProgress.Add(edge.outNeuronID, edge.weight * activationValues[edge.inNeuronID] + edge.bias);
                        }
                        else
                        {
                            currentProgress[edge.outNeuronID] += edge.weight * activationValues[edge.inNeuronID] + edge.bias;
                        }
                    }
                }
            }

            sizeOfOutputLayer = outputLayer.Count;
            return outputLayer;
        }
	}
}

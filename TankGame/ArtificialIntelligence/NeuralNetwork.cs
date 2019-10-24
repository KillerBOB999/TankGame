using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
	public class NeuralNetwork
	{
        public Dictionary<int, Dictionary<int, Edge>> edgeDict;
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
            updateEdgeDict(edges);
		}

		public NeuralNetwork() { }
        public NeuralNetwork(NeuralNetwork neuralNetwork)
        {
            edges = new List<Edge>(neuralNetwork.edges);
            nextID = neuralNetwork.nextID;
            outputLayerIDs = new List<int>(neuralNetwork.outputLayerIDs);
            topologicalOrdering = new List<int>(neuralNetwork.topologicalOrdering);
            updateEdgeDict(edges);
        }

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

            for (int i = 0; i < outputLayerIDs.Count(); ++i)
            {
                outputLayer.Add(outputLayerIDs[i], currentProgress[outputLayerIDs[i]]);
            }

            sizeOfOutputLayer = outputLayer.Count();
            return outputLayer;
        }

        public void updateEdgeDict(List<Edge> e)
        {
            if (edgeDict != null && edgeDict.ContainsKey(1)) { 
                edgeDict.Clear();
            }
            foreach (Edge edge in e)
            {
                if (edgeDict == null)
                {
                    edgeDict = new Dictionary<int, Dictionary<int, Edge>>() 
                    { 
                        { 
                            edge.inNeuronID, new Dictionary<int, Edge>() 
                            {
                                { 
                                    edge.outNeuronID, edge 
                                } 
                            } 
                        } 
                    };
                }
                if (!edgeDict.ContainsKey(edge.inNeuronID))
                {
                    edgeDict.Add(edge.inNeuronID, new Dictionary<int, Edge> { { edge.outNeuronID, edge } });
                }
                else
                {
                    if (!edgeDict[edge.inNeuronID].ContainsKey(edge.outNeuronID))
                    {
                        edgeDict[edge.inNeuronID].Add(edge.outNeuronID, edge);
                    }
                }
            }
        }

		public static double sigmoid(double numeratorfactor, double exponentFactor, double inputValue)
		{
			return numeratorfactor / (1 + Math.Pow(Math.E, -exponentFactor * inputValue));
		}
	}
}

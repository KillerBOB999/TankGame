using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame.ArtificialIntelligence.NEAT
{
    static class Mutatinator
    {
        enum TypesOfMutation
        {
            FIRST_UNUSED, AddNewNode, AddNewEdge, RemoveExistingEdge, ChangeExistingWeight, ChangeExistingBias, LAST_UNUSED
        };
        static Random rng = new Random();
        public static void mutate(NeuralNetwork neuralNetwork)
        {
            TypesOfMutation mutationChoice = (TypesOfMutation)rng.Next((int)TypesOfMutation.FIRST_UNUSED + 1, (int)TypesOfMutation.LAST_UNUSED - 1);
            switch (mutationChoice)
            {
                case TypesOfMutation.AddNewNode:
                    addNode(neuralNetwork);
                    break;
                case TypesOfMutation.AddNewEdge:
                    int attemptedInputNode = rng.Next(1, neuralNetwork.nextID - 1);
                    int attemptedOutputNode = rng.Next(NeuralNetwork.sizeOfInputLayer + 1, neuralNetwork.nextID - 1);

                    if(canAddEdge(neuralNetwork, attemptedInputNode, attemptedOutputNode))
                    {
                        addEdge(neuralNetwork, attemptedInputNode, attemptedOutputNode);
                    }

                    break;
                case TypesOfMutation.RemoveExistingEdge:
                    removeEdge(neuralNetwork, rng.Next(0, neuralNetwork.edges.Count - 1));
                    break;
                case TypesOfMutation.ChangeExistingWeight:
                    changeWeight(neuralNetwork);
                    break;
                case TypesOfMutation.ChangeExistingBias:
                    changeBias(neuralNetwork);
                    break;
            }
        }

        private static bool canAddEdge(NeuralNetwork neuralNetwork, int attemptedInputNode, int attemptedOutputNode)
        {
            if (!neuralNetwork.outputLayerIDs.Contains(attemptedInputNode))
            {
                if (neuralNetwork.topologicalOrdering.IndexOf(attemptedInputNode) <
                    neuralNetwork.topologicalOrdering.IndexOf(attemptedOutputNode))
                {
                    return true;
                }
            }
            return false;
        }

        private static void addNode(NeuralNetwork neuralNetwork)
        {
            int indexOfEdge = rng.Next(1, neuralNetwork.edges.Count - 1);
            Edge oldEdge = new Edge(neuralNetwork.edges[indexOfEdge]);

            removeEdge(neuralNetwork, indexOfEdge);
            addEdge(neuralNetwork, oldEdge.inNeuronID, neuralNetwork.nextID);
            addEdge(neuralNetwork, neuralNetwork.nextID, oldEdge.outNeuronID);
            ++neuralNetwork.nextID;

            for(int i = 0; i < neuralNetwork.topologicalOrdering.Count; ++i)
            {
                if(neuralNetwork.topologicalOrdering[i] == neuralNetwork.edges[indexOfEdge].inNeuronID)
                {
                    neuralNetwork.topologicalOrdering.Insert(i + 1, neuralNetwork.edges[indexOfEdge].outNeuronID);
                }
            }
        }

        private static void addEdge(NeuralNetwork neuralNetwork, int from, int to)
        {
            neuralNetwork.edges.Add(new Edge(from, to, 1, 0));
        }

        private static void removeEdge(NeuralNetwork neuralNetwork, int index)
        {
            neuralNetwork.edges.RemoveAt(index);
        }

        private static void changeWeight(NeuralNetwork neuralNetwork)
        {
            double randomDouble = rng.NextDouble();
            int addIf1 = rng.Next(0, 1);
            int indexOfEdge = rng.Next(1, neuralNetwork.nextID);

            if(addIf1 == 1)
            {
                neuralNetwork.edges[indexOfEdge].weight += randomDouble;
            }
            else
            {
                neuralNetwork.edges[indexOfEdge].weight -= randomDouble;
            }
        }

        private static void changeBias(NeuralNetwork neuralNetwork)
        {
            double randomDouble = rng.NextDouble();
            int addIf1 = rng.Next(0, 1);
            int indexOfEdge = rng.Next(1, neuralNetwork.nextID);

            if (addIf1 == 1)
            {
                neuralNetwork.edges[indexOfEdge].bias += randomDouble;
            }
            else
            {
                neuralNetwork.edges[indexOfEdge].bias -= randomDouble;
            }
        }

        private static NeuralNetwork cross(NeuralNetwork mama, NeuralNetwork papa)
        {
            double papaContribution = 0.6;
            double mamaContribution = 0.4;
            NeuralNetwork baby = new NeuralNetwork(papa);

            foreach (Edge mamaEdge in mama.edges)
            {
                foreach (Edge babyEdge in baby.edges)
                {
                    if (mamaEdge.inNeuronID == babyEdge.inNeuronID && mamaEdge.outNeuronID == babyEdge.outNeuronID)
                    {
                        babyEdge.weight = papaContribution * babyEdge.weight + mamaContribution * mamaEdge.weight;
                        babyEdge.bias = papaContribution * babyEdge.bias + mamaContribution * mamaEdge.bias;
                    }
                }
            }

            baby.updateEdgeDict(baby.edges);
            return baby;
        }
    }
}

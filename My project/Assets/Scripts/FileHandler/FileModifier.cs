
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace Assets.Scripts.FileHandler
{
    public class FileModifier
    {
        public string filePath = "Assets/Scripts/FileHandler/";

        public void AddTextFile(string text)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);
                fileContent += text;
                File.WriteAllText(filePath, fileContent);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        public void WriteTextFile(NeuralNet net)
        {
            try
            {
                string fileContent = "";

                for (int i = 0; i < net.GetLayerCount(); i++)
                {
                    // first the weights after that the biases
                    float[,] weights = net.GetLayerIndexWeights(i);
                    for (int j = 0; j < weights.GetLength(0); j++)
                    {
                        for (int k = 0; k < weights.GetLength(1); k++)
                        {
                            fileContent += weights[j, k] + " ";
                        }
                        fileContent += "\n";
                    }
                    float[] biases = net.GetLayerIndexBiases(i);
                    for (int j = 0; j < biases.Length; j++)
                    {
                        fileContent += biases[j] + " ";
                    }
                    fileContent += "\n";
                }

                File.WriteAllText(filePath, fileContent);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        public bool WriteBinFile(NeuralNet net, string fileName)
        {

            try
            {
                using (FileStream fileStream = new FileStream(filePath + fileName, FileMode.Create)) // FileMode.Create overwrites existing files
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        NTools tool = new NTools();
                        NetworkData b = tool.SerializeNetwork(net);
                        formatter.Serialize(fileStream, b);
                        
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error Writing binary:" + e.Message);
            }
            return false;
        }


        public NeuralNet ReadNet(NeuralNet net, string fileName)
        {
            NeuralNet newNet = net;
            NTools tool = new NTools();
            try
            {
                if (File.Exists(filePath)) {
                    using (FileStream fileStream = new FileStream(filePath + fileName, FileMode.Open))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        tool.DeserializeNetwork(newNet, (NetworkData)binaryFormatter.Deserialize(fileStream));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("didnt work");
                Debug.LogException(e);
            }

            return newNet;
        }
    }
}

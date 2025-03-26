using Unity.VisualScripting;
using UnityEngine;

public class NTools : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public double[,] MultiplyMatrices(double[,] matrix1, double[,] matrix2)
    {
        double[,] result = new double[matrix1.GetLength(0), matrix2.GetLength(1)];
        for (int i = 0; i < matrix1.GetLength(0); i++)
        {
            for (int j = 0; j < matrix2.GetLength(1); j++)
            {
                for (int k = 0; k < matrix1.GetLength(1); k++)
                {
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            }
        }
        return result;
    }

    public double MultiplyMatrices(double[] matrix1, double[] matrix2)
    {
        double result = 0;
        for (int i = 0; i < matrix1.Length; i++)
        {
            result += matrix1[i] * matrix2[i];
        }
        return result;
    }
}

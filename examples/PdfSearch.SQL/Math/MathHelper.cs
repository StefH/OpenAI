using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;

namespace PdfSearch.SQL.Math;

internal static class MathHelper
{
    public static (Vector<float> QuestionVector, double QuestionMagnitude) GetQuestionVectorData(byte[] question)
    {
        var questionVector = Vector<float>.Build.DenseOfArray(MemoryMarshal.Cast<byte, float>(question).ToArray());
        return (questionVector, questionVector.L2Norm());
    }

    public static double CalculateCosineSimilarity(Vector<float> questionVector, double questionMagnitude, Vector<float> vecB)
    {
        if (questionVector.Count != vecB.Count)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        var dotProduct = questionVector.DotProduct(vecB);
        var magnitudeB = vecB.L2Norm();

        if (questionMagnitude == 0 || magnitudeB == 0)
        {
            throw new ArgumentException("One or both of the vectors have zero magnitude.");
        }

        return dotProduct / (questionMagnitude * magnitudeB);
    }
}
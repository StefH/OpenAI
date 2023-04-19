using MathNet.Numerics.LinearAlgebra;

var v1 = new float[] { 0,27, 0,37, 0,47 };
var v2 = new float[] { 0,29, 0,39, 0,49 };
var s1 = CalculateCosineSimilarity(v1, v2);
Console.WriteLine(s1);

var s2 = CalculateCosineSimilarityUsingMathNet(v1, v2);
Console.WriteLine(s2);

static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
{
    if (vectorA.Length != vectorB.Length)
    {
        throw new ArgumentException("Vectors must have the same length.");
    }

    float dotProduct = 0.0f;
    float magnitudeA = 0.0f;
    float magnitudeB = 0.0f;

    for (int i = 0; i < vectorA.Length; i++)
    {
        dotProduct += vectorA[i] * vectorB[i];
        magnitudeA += vectorA[i] * vectorA[i];
        magnitudeB += vectorB[i] * vectorB[i];
    }

    magnitudeA = (float)Math.Sqrt(magnitudeA);
    magnitudeB = (float)Math.Sqrt(magnitudeB);

    if (magnitudeA == 0 || magnitudeB == 0)
    {
        throw new ArgumentException("One or both of the vectors have zero magnitude.");
    }

    return dotProduct / (magnitudeA * magnitudeB);
}

static double CalculateCosineSimilarityUsingMathNet(float[] vectorA, float[] vectorB)
{
    if (vectorA.Length != vectorB.Length)
    {
        throw new ArgumentException("Vectors must have the same length.");
    }

    var vecA = Vector<float>.Build.DenseOfArray(vectorA);
    var vecB = Vector<float>.Build.DenseOfArray(vectorB);

    var dotProduct = vecA.DotProduct(vecB);
    var magnitudeA = vecA.L2Norm();
    var magnitudeB = vecB.L2Norm();

    if (magnitudeA == 0 || magnitudeB == 0)
    {
        throw new ArgumentException("One or both of the vectors have zero magnitude.");
    }

    return dotProduct / (magnitudeA * magnitudeB);
}
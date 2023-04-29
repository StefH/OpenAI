using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using OpenAI_API;

var gptEncoding = SharpToken.GptEncoding.GetEncoding("cl100k_base");

var openAIAPI = new OpenAIAPI(new APIAuthentication(Environment.GetEnvironmentVariable("OpenAIAPI_Key"), Environment.GetEnvironmentVariable("OpenAIAPI_Org")));

var v1 = await openAIAPI.Embeddings.WithRetry(api => api.GetEmbeddingsAsync("cat"));
//var json1 = JsonSerializer.Serialize(v1);
//File.WriteAllText("cat.json", json1);

var v2 = await openAIAPI.Embeddings.WithRetry(api => api.GetEmbeddingsAsync("kitten"));
//var json2 = JsonSerializer.Serialize(v2);
//File.WriteAllText("kitten.json", json2);

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
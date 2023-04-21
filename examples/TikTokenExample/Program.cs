using System.Text.RegularExpressions;
using SharpToken;

var alice = File.ReadAllText("alice.txt");
var words = Regex.Split(alice.Trim(), @"\s+");
Console.WriteLine("Words = {0}", words.Length);

var encoding1 = GptEncoding.GetEncoding("cl100k_base");
Console.WriteLine("Tokens for cl100k_base = {0}", encoding1.Encode(alice).Count);

var encoding2 = GptEncoding.GetEncodingForModel("gpt-4");
Console.WriteLine("Tokens for gpt-4 = {0}", encoding2.Encode(alice).Count);
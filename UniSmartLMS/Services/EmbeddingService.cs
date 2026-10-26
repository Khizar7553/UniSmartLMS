using System.Numerics.Tensors;

namespace UniSmartLMS.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            await Task.Delay(1);

            var embedding = new float[384];
            var lowerText = text.ToLower();

            for (int i = 0; i < lowerText.Length - 2 && i < 500; i++)
            {
                var trigram = lowerText.Substring(i, Math.Min(3, lowerText.Length - i));
                var hash = Math.Abs(trigram.GetHashCode()) % 384;
                embedding[hash] += 1.0f;
            }

            var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
            if (magnitude > 0)
            {
                for (int i = 0; i < embedding.Length; i++)
                    embedding[i] /= magnitude;
            }

            return embedding;
        }

        public float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;
            return (float)TensorPrimitives.CosineSimilarity(a, b);
        }

        public async Task<List<float[]>> BatchGenerateEmbeddingsAsync(List<string> texts)
        {
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                results.Add(await GenerateEmbeddingAsync(text));
            }
            return results;
        }
    }
}
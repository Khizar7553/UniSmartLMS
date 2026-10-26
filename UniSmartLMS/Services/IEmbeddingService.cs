namespace UniSmartLMS.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        float CosineSimilarity(float[] a, float[] b);
        Task<List<float[]>> BatchGenerateEmbeddingsAsync(List<string> texts);
    }
}
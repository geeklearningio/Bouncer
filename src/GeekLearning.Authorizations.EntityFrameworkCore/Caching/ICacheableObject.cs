namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using Newtonsoft.Json;

    public interface ICacheableObject
    {
        [JsonIgnore]
        string CacheKey { get; }
    }
}

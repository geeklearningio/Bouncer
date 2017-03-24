namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using System;

    public interface ICacheableObject
    {
        string CacheKey { get; }

        DateTime CacheValuesDateTime { get; set; }
    }
}

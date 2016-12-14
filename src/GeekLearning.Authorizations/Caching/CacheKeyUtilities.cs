namespace GeekLearning.Authorizations.Caching
{
    using System;

    public static class CacheKeyUtilities
    {
        public static string GetCacheKey(Guid principalId)
        {
            return $"{nameof(Model.RightsResult)}-{principalId}";
        }
    }
}

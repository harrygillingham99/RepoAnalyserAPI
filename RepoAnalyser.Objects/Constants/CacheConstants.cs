using System;

namespace RepoAnalyser.Objects.Constants
{
    public static class CacheConstants
    {
        public static DateTimeOffset DefaultCacheExpiry = DateTimeOffset.Now.AddHours(1);
    }
}

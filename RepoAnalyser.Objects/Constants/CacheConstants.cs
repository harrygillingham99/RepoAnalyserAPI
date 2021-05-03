using System;

namespace RepoAnalyser.Objects.Constants
{
    public static class CacheConstants
    {
        public static DateTimeOffset DefaultCacheExpiry = DateTimeOffset.Now.AddHours(1);

        //set it low for testing
        public static TimeSpan DefaultSlidingCacheExpiry = new TimeSpan(0, 1, 0);
    }
}

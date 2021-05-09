using System;
using System.Diagnostics;

namespace RepoAnalyser.Objects.Constants
{
    public static class CacheConstants
    {
        public static DateTimeOffset DefaultCacheExpiry = DateTimeOffset.Now.AddHours(1);

        //set it low for testing
        public static TimeSpan DefaultSlidingCacheExpiry = Debugger.IsAttached ? new TimeSpan(0, 0, 30) : new TimeSpan(0, 5, 0);
    }
}

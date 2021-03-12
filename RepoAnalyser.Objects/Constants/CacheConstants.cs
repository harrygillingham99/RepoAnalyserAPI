using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects.Constants
{
    public static class CacheConstants
    {
        public static DateTimeOffset DefaultCacheExpiry = DateTimeOffset.Now.AddHours(1);
    }
}

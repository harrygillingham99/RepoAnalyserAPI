using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects
{
    public class GitHubSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string AppName { get; set; }
        public string FrontEndRedirectUrl { get; set; }
    }
}

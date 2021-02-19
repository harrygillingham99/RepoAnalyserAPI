using System;
using System.Collections.Generic;
using System.Text;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserInfoResult
    {
        public User User { get; set; }
        public string LoginRedirectUrl { get; set; }
    }
}

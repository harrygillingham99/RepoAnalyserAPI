using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserActivity
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public IEnumerable<Activity> Events { get; set; }
    }
}
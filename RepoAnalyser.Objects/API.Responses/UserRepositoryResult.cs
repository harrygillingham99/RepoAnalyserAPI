using System;
using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserRepositoryResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PullUrl { get; set; }

        public bool Private { get; set; }
        public bool Template { get; set; }
        public IEnumerable<Collaborator> Collaborators { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public struct Collaborator
    {
        public Collaborator(string name, string avatarUrl)
        {
            Name = name ?? "Unknown User";
            AvatarUrl = avatarUrl ?? "Unknown";
        }

        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
}
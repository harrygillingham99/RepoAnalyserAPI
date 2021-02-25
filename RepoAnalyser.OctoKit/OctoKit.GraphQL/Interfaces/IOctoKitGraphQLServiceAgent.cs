﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoAnalyser.Services.OctoKit.GraphQL.Interfaces
{
    public interface IOctoKitGraphQlServiceAgent
    {
        Task<IEnumerable<Repo>> GetRepositories(string token);
    }
}
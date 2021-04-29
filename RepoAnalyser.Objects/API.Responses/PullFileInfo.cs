using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects.API.Responses
{
    public class PullFileInfo
    {
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int CommitsThatIncludeFile { get; set; }
    }
}

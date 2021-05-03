using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects.API.Responses
{
    public class AddedRemoved
    {
        public AddedRemoved(int added, int removed)
        {
            Added = added;
            Removed = removed;
        }
        public int Added { get; set; }
        public int Removed { get; set; }
    }
}

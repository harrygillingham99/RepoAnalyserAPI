using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RepoAnalyser.Objects.Responses
{
    public class NotFoundResponse : Response
    {
        public Dictionary<string,string> BadProperties { get; set; }
    }
}

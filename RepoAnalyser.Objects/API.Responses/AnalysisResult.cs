using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class AnalysisResult {

        public AnalysisResults Result { get; set; } 
        public string GendarmeReportDirectory { get; set; }
        public IDictionary<string, string> CodeOwners { get; set; }
        public IDictionary<string, int> Complexities { get; set; }

    }
}
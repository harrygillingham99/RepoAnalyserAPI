namespace RepoAnalyser.Objects.API.Responses
{
    public class PullFileInfo
    {
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int CommitsThatIncludeFile { get; set; }
    }
}

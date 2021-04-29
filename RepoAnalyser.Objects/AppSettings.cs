namespace RepoAnalyser.Objects
{
    public class AppSettings
    {
        public string DatabaseConnectionString { get; set; }
        public bool RequestLogging { get; set; }

        public string WorkingDirectory { get; set; }
        public string ServerUser { get; set; }
        public string ServerPassword { get; set; }
    }
    
}

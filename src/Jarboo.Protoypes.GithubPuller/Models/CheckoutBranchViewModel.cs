namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class CheckoutBranchViewModel
    {
        public string[] BuildFilePaths { get; set; }
        public string OutputDirectoryName { get; set; }

        public string RepositoryName { get; set; }

        public string Branch { get; set; }

        public string RepositoryPath { get; set; }
    }
}
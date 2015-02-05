using System.Collections.Generic;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class RepositoriesViewModel
    {
        public IList<Repository> Repositories { get; set; }
    }
}
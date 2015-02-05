using System.Collections.Generic;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class DetailViewModel
    {
        public Repository Repository { get; set; }

        public IList<Branch> Branches { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class RepositoryViewModel
    {
        public string CloneUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string DefaultBranch { get; set; }
        public string Description { get; set; }
        public bool Fork { get; set; }
        public int ForksCount { get; set; }
        public string FullName { get; set; }
        public string GitUrl { get; set; }
        public bool HasDownloads { get; set; }
        public bool HasIssues { get; set; }
        public bool HasWiki { get; set; }
        public string Homepage { get; set; }
        public string HtmlUrl { get; set; }
        public int Id { get; set; }
        public string Language { get; set; }
        public string MirrorUrl { get; set; }
        public string Name { get; set; }
        public int OpenIssuesCount { get; set; }
        //public User Organization { get; set; }
        public UserViewModel Owner { get; set; }
        //public Repository Parent { get; set; }
        //public RepositoryPermissions Permissions { get; set; }
        public bool Private { get; set; }
        public DateTimeOffset? PushedAt { get; set; }
        //public Repository Source { get; set; }
        public string SshUrl { get; set; }
        public int StargazersCount { get; set; }
        public int SubscribersCount { get; set; }
        public string SvnUrl { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Url { get; set; }
        public int WatchersCount { get; set; }
    }
}

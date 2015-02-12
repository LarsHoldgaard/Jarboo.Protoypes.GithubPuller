using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Jarboo.Protoypes.GithubPuller.Services
{
    public class RepositoryService
    {
        private readonly string _githubUsername = ConfigurationManager.AppSettings["GithubUsername"];
        private readonly string _githubPassword = ConfigurationManager.AppSettings["GithubPassword"];
        private readonly CredentialsHandler _credHandler;

        public RepositoryService()
        {
            var creds = new UsernamePasswordCredentials
            {
                Username = _githubUsername,
                Password = _githubPassword
            };
            _credHandler = (_, __, cred) => creds;
        }

        public string Clone(string path, string url, string branch)
        {
            return LibGit2Sharp.Repository.Clone(url, path, new CloneOptions
            {
                BranchName = branch,
                Checkout = true,
                CredentialsProvider = _credHandler
            });
        }

        public void RemoveRepositoryFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var subdirectory in Directory.EnumerateDirectories(path))
            {
                RemoveRepositoryFolder(subdirectory);
            }
            foreach (var fileName in Directory.EnumerateFiles(path))
            {
                var fileInfo = new FileInfo(fileName) {Attributes = FileAttributes.Normal};
                fileInfo.Delete();
            }
            Directory.Delete(path);
        }


        public string[] FindBuildFiles(string repositoryPath, string pattern)
        {
            var parent = Directory.GetParent(repositoryPath).Parent;
            string searchPattern = string.IsNullOrEmpty(pattern) ? "*.sln" : pattern;
            FileInfo[] files = parent.GetFiles(searchPattern, SearchOption.AllDirectories);

            if (!files.Any())
            {
                return null;
            }

            return files.Select(file => Path.Combine(file.DirectoryName, file.Name)).ToArray();
        }
    }
}
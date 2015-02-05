using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Jarboo.Protoypes.GithubPuller.Attributes;
using Jarboo.Protoypes.GithubPuller.Models;
using LibGit2Sharp;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Web.Administration;
using NLog;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    [CustomAuthorize]
    public class ProjectController : BaseController
    {
        private readonly Lazy<GitHubClient> _gitHubClient;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ProjectController()
        {
            _gitHubClient = new Lazy<GitHubClient>(() => new GitHubClient(CurrentConnection));
        }

        public async Task<ActionResult> Index()
        {
            var repositories = await _gitHubClient.Value.Repository.GetAllForCurrent();
            var model = new RepositoriesViewModel
            {
                Repositories = repositories.ToList()
            };
            return View(model);
        }

        public async Task<ActionResult> Detail(string owner, string name)
        {
            var repositoryTask = _gitHubClient.Value.Repository.Get(owner, name);
            var branchesTask = _gitHubClient.Value.Repository.GetAllBranches(owner, name);

            await Task.WhenAll(repositoryTask, branchesTask);

            var model = new DetailViewModel
            {
                Branches = branchesTask.Result.ToList(),
                Repository = repositoryTask.Result
            };

            return View(model);
        }

        public async Task<ActionResult> Branch(string owner, string repositoryName, string name)
        {
            bool result = true;
            var basePath = Server.MapPath(ConfigurationManager.AppSettings["DownloadPath"]);
            try
            {
                var repo = await _gitHubClient.Value.Repository.Get(owner, repositoryName);

                var path = Path.Combine(basePath, DateTime.Now.Ticks + repositoryName);

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                var repositoryPath = Repository.Clone(repo.CloneUrl, path, new CloneOptions { BranchName = name, Checkout = true });
                
                var solutionFile = FindSolutionFile(repositoryPath);

                _logger.Debug("Solution file: {0}", solutionFile);

                string outputPath = Server.MapPath(Path.Combine(ConfigurationManager.AppSettings["BuildPath"], Guid.NewGuid().ToString()));
                string solutionName = solutionFile.Replace(".sln", ""); //extracting solution name
                string packagePath = Path.Combine(outputPath, "_PublishedWebsites", solutionName);

                _logger.Debug("Package path: {0}", packagePath);

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                Build(solutionFile, outputPath, null);

                CreateApplication(packagePath, repositoryName + "-" + name, ConfigurationManager.AppSettings["DeployApplication"]);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                result = false;
            }

            return View(result);
        }

        private string FindSolutionFile(string path)
        {
            var parent = Directory.GetParent(path).Parent;
            FileInfo[] files = parent.GetFiles("*.sln", SearchOption.AllDirectories);

            if (!files.Any())
            {
                return null;
            }

            var solutionFile = files.First();
            return Path.Combine(solutionFile.DirectoryName, solutionFile.Name);
        }

        private void CreateApplication(string path, string name, string root)
        {
            name = name.StartsWith("/") ? name : "/" + name;
            using (var server = new ServerManager())
            {
                Site site = server.Sites.First(w => w.Name == root);

                var existingApplications = site.Applications.Where(a => a.Path == name);

                //removing sites with this name if they exist
                foreach (var existing in existingApplications)
                {
                    site.Applications.Remove(existing);
                }

                server.CommitChanges();
                
                site.Applications.Add(name, path);

                server.CommitChanges();

            }
        }

        private void Build(string solutionPath, string outputPath, string[] targets, string configuration = "Release", string platform = "Any CPU")
        {
            _logger.Debug("Start building");
            if (targets == null || !targets.Any())
            {
                targets = new[] { "Build" };
            }
            _logger.Debug("Restoring packages");
            NuGetPlus.SolutionManagement.RestorePackages(solutionPath);
            _logger.Debug("Packages restored");

            var projectCollection = new ProjectCollection();

            var properties = new Dictionary<string, string> 
            { 
                { "Configuration", configuration }, 
                { "Platform", platform }, 
                {"DeployOnBuild", "true"},
                {"OutputPath", outputPath}
            };
            var buildRequestData = new BuildRequestData(solutionPath, properties, null, targets, null);

            var buildParameters = new BuildParameters(projectCollection);

            var buildRequest = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);
            var isSuccess = buildRequest.OverallResult == BuildResultCode.Success;

            _logger.Debug("Build succes: {0}", isSuccess);

            if (!isSuccess)
            {
                foreach (var res in buildRequest.ResultsByTarget)
                {
                    _logger.Debug("Result key: {0}", res.Key);
                    _logger.Debug("REsult code: {0}", res.Value.ResultCode);
                    _logger.Debug("Result exception: ");
                    _logger.Debug(res.Value.Exception);

                }
            }
        }
    }
}
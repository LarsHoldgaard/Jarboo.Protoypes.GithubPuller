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
using NuGetPlus;
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

                var repositoryPath = Repository.Clone(repo.CloneUrl, path, new CloneOptions { BranchName = name, Checkout = true });
                
                var solutionFilePath = FindSolutionPath(repositoryPath);

                _logger.Debug("Solution file: {0}", solutionFilePath);

                string outputPath = Server.MapPath(Path.Combine(ConfigurationManager.AppSettings["BuildPath"], Guid.NewGuid().ToString()));
                string solutionName = Path.GetFileNameWithoutExtension(solutionFilePath); //extracting solution name
                _logger.Debug("Solution file name: {0}", solutionName);
                
                Build(solutionFilePath, outputPath, null);

                string packagePath = Path.Combine(outputPath, "_PublishedWebsites", solutionName);
                _logger.Debug("Package path: {0}", packagePath);

                CreateApplication(packagePath, repositoryName + "." + name, ConfigurationManager.AppSettings["DeployApplication"]);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                result = false;
            }

            return View(result);
        }

        private string FindSolutionPath(string path)
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

        private void Build(string solutionPath, string outputPath, string[] targets, string configuration = "Release", string platform = "Any CPU")
        {
            _logger.Debug("Start building");
            if (targets == null || !targets.Any())
            {
                targets = new[] { "Build" };
            }
            _logger.Debug("Restoring packages");
            _logger.Debug("Solution path: {0}", solutionPath);

            try
            {
                NuGetPlus.SolutionManagement.RestorePackages(solutionPath);
            }
            catch (Exception e)
            {
                _logger.Debug("Error when running restore command");
                _logger.Debug(e);
            }

            var packages = NuGetPlus.SolutionManagement.GetRestorePackages(solutionPath);
            foreach (var package in packages)
            {
                _logger.Debug("Package {0}, id: {1}, version {2}", package.Item1.Item, package.Item2.Id, package.Item2.Version);
                RepositoryManagement.RestorePackage a = new NuGetPlus.RepositoryManagement.RestorePackage(package.Item2.Id, package.Item2.Version);
            }

            _logger.Debug("Packages restored");

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

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

        private void CreateApplication(string path, string name, string root)
        {
            name = name.StartsWith("/") ? name : "/" + name;
            name = name.Replace(".", "-");
            using (var server = new ServerManager())
            {
                _logger.Debug("Getting site root: {0}", root);
                Site site = server.Sites.First(w => w.Name == root);

                var existingApplications = site.Applications.Where(a => a.Path == name).ToList();
                _logger.Debug("Existing applications for {0}: {1}", name, existingApplications.Count());

                //removing sites with this name if they exist
                foreach (var existing in existingApplications)
                {
                    site.Applications.Remove(existing);
                }

//                server.CommitChanges();

                _logger.Debug("Adding application {0}, path: {1}", name, path);
                site.Applications.Add(name, path);

                server.CommitChanges();

                _logger.Debug("Application added: {0}", name);
            }
        }
    }
}
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Jarboo.Protoypes.GithubPuller.Attributes;
using Jarboo.Protoypes.GithubPuller.Models;
using Jarboo.Protoypes.GithubPuller.Services;
using NLog;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    [CustomAuthorize]
    public class ProjectController : BaseController
    {
        private readonly Lazy<GitHubClient> _gitHubClient;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly RepositoryService _repositoryService;
        private readonly BuildService _buildService;
        private readonly DeployService _deployService;

        public ProjectController()
        {
            _gitHubClient = new Lazy<GitHubClient>(() => new GitHubClient(CurrentConnection));
            _repositoryService = new RepositoryService();
            _buildService = new BuildService();
            _deployService = new DeployService();
        }

        public async Task<ActionResult> Index()
        {
            
            var repositoriesTask = _gitHubClient.Value.Repository.GetAllForCurrent();
            var organizationTask = _gitHubClient.Value.Organization.GetAllForCurrent();

            await Task.WhenAll(repositoriesTask, organizationTask);

            var organizationRepos = organizationTask.Result.Select(s => _gitHubClient.Value.Repository.GetAllForOrg(s.Login)).ToArray();

            await Task.WhenAll(organizationRepos);

            var repositories = repositoriesTask.Result.ToList();
            repositories.AddRange(organizationRepos.SelectMany(s => s.Result).ToList());
            var model = new RepositoriesViewModel
            {
                Repositories = repositories
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

        public ActionResult UpdateDependencies()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateDependencies(string url, string branch)
        {
            var path = Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["DownloadPath"]), "Saxo.Dependencies");

            _repositoryService.RemoveRepositoryFolder(path);

            _repositoryService.Clone(path, url, branch);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> CheckoutBranch(string owner, string repositoryName, string branch, string pattern = "*.build")
        {
            var basePath = Server.MapPath(ConfigurationManager.AppSettings["DownloadPath"]);
            string resultDirectory = DateTime.Now.Ticks + repositoryName;
            var path = Path.Combine(basePath, resultDirectory);

            var repository = await _gitHubClient.Value.Repository.Get(owner, repositoryName);
            var repositoryPath = _repositoryService.Clone(path, repository.CloneUrl, branch);

            string[] filePaths = _repositoryService.FindBuildFiles(repositoryPath, pattern);

            var model = new CheckoutBranchViewModel
            {
                BuildFilePaths = filePaths,
                OutputDirectoryName = resultDirectory,
                RepositoryName = repositoryName,
                Branch = branch,
                RepositoryPath = repositoryPath
            };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Build(string buildFilePath, string outputDirectoryName, string repositoryName, string branch, string repositoryPath)
        {
            bool result;
            string buildOutput = string.Empty;
            
            try
            {
                string outputPath = Path.Combine(Server.MapPath(ConfigurationManager.AppSettings["BuildPath"]), outputDirectoryName);

                buildOutput = _buildService.Build(buildFilePath, outputPath);

                result = buildOutput.Contains("0 Error(s)");

                _repositoryService.RemoveRepositoryFolder(repositoryPath);

                if (result)
                {
                    var sitePath = _deployService.DeployApplication(repositoryName + "." + branch, outputPath);

                    ViewBag.SitePath = ConfigurationManager.AppSettings["DeployApplication"] + sitePath;
                }

            }
            catch (Exception e)
            {
                _logger.Error(e);
                result = false;
            }

            ViewBag.BuildOutput = buildOutput;
            return View(result);
        }
    }
}
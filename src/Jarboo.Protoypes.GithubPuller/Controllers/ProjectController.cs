using Jarboo.Protoypes.GithubPuller.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    public class ProjectController : AuthenticationController
    {
        public async Task<ActionResult> Index()
        {
            var gitClient = new GitHubClient(this.CurrentConnection);
            ApiConnection apiConnection = new ApiConnection(this.CurrentConnection);
            var repositories = await gitClient.Repository.GetAllForCurrent();
            var model = new RepositoriesViewModel();
            model.Repositories = repositories.Select(Create).ToList();
            return View(model);
        }

        public async Task<ActionResult> Detail(string owner, string name)
        {
            var gitClient = new GitHubClient(this.CurrentConnection);
            ApiConnection apiConnection = new ApiConnection(this.CurrentConnection);
            var client = new RepositoriesClient(apiConnection);
            var repository = await gitClient.Repository.Get(owner, name);
            var model = Create(repository);
            var branches = await gitClient.Repository.GetAllBranches(owner, name);
            return View(model);
        }

        private RepositoryViewModel Create(Repository repo)
        {
            var model = new RepositoryViewModel();
            model.CloneUrl = repo.CloneUrl;
            model.CreatedAt = repo.CreatedAt;
            model.DefaultBranch = repo.DefaultBranch;
            model.Description = repo.Description;
            model.Fork = repo.Fork;
            model.ForksCount = repo.ForksCount;
            model.FullName = repo.FullName;
            model.GitUrl = repo.GitUrl;
            model.HasDownloads = repo.HasDownloads;
            model.HasIssues = repo.HasIssues;
            model.HasWiki = repo.HasWiki;
            model.Homepage = repo.Homepage;
            model.HtmlUrl = repo.HtmlUrl;
            model.Id = repo.Id;
            model.Language = repo.Language;
            model.MirrorUrl = repo.MirrorUrl;
            model.Name = repo.Name;
            model.OpenIssuesCount = repo.OpenIssuesCount;
            model.Private = repo.Private;
            model.PushedAt = repo.PushedAt;
            model.SshUrl = repo.SshUrl;
            model.StargazersCount = repo.StargazersCount;
            model.SubscribersCount = repo.SubscribersCount;
            model.SvnUrl = repo.SvnUrl;
            model.UpdatedAt = repo.UpdatedAt;
            model.Url = repo.Url;
            model.WatchersCount = repo.WatchersCount;
            model.Owner = new UserViewModel()
            {
                Id = repo.Owner.Id,
                Name = repo.Owner.Name,
                Login = repo.Owner.Login,
                Email = repo.Owner.Email;
            };
            return model;
        }
    }
}
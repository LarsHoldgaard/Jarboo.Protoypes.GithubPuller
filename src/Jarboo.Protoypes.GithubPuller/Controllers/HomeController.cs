using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Jarboo.Protoypes.GithubPuller.Models;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(string returnUrl)
        {
            if (CurrentUser != null && CurrentConnection != null)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Project");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            var connection = new Connection(new ProductHeaderValue("Jarboo.Protoypes.GithubPuller", "1.0"))
            {
                Credentials = new Credentials(model.Username, model.Password)
            };

            var gitClient = new GitHubClient(connection);
            try
            {
                User user = await gitClient.User.Get(model.Username);

                CurrentUser = user;
                CurrentConnection = connection;

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Project");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Invalid username or password!";
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            CurrentUser = null;
            CurrentConnection = null;
            return RedirectToAction("Index");
        }
    }
}
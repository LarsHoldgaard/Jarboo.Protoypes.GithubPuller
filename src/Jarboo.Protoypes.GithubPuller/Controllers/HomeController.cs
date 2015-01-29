using Jarboo.Protoypes.GithubPuller.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            var connection = new Octokit.Connection(new Octokit.ProductHeaderValue("Jarboo.Protoypes.GithubPuller", "1.0"))
            {
                Credentials = new Credentials(model.Username, model.Password)
            };

            var gitClient = new GitHubClient(connection);
            try
            {
                User user = await gitClient.User.Get(model.Username);
                FormsAuthentication.SetAuthCookie(user.Name, true);
                Session[Constants.Session.CurrentUser] = user;
                Session[Constants.Session.CurrentConnection] = connection;
                return RedirectToAction("Index", "Project");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Invalid username or password!";
            }

            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session[Constants.Session.CurrentUser] = null;
            Session[Constants.Session.CurrentConnection] = null;
            return RedirectToAction("Index");
        }
    }
}
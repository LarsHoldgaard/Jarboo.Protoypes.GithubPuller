using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Jarboo.Protoypes.GithubPuller.Models;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    public class HomeController : BaseController
    {
        private readonly string _clientId = ConfigurationManager.AppSettings["GithubClientId"];
        private readonly string _clientSecret = ConfigurationManager.AppSettings["GithubClientSecret"];


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

            string csrf = Membership.GeneratePassword(24, 1);
            Session[Constants.Session.State] = csrf;

            // 1. Redirect users to request GitHub access
            var request = new OauthLoginRequest(_clientId)
            {
                Scopes = { "user", "notifications" },
                State = csrf
            };

            var client = new GitHubClient(new ProductHeaderValue("Jarboo.Protoypes.GithubPuller", "1.0"));

            var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);
            return Redirect(oauthLoginUrl.ToString());/*

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);*/
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

        public async Task<ActionResult> Callback(string code, string state)
        {
            if (!string.IsNullOrEmpty(code))
            {
                var expectedState = Session[Constants.Session.State].ToString();

                if (state != expectedState)
                {
                    return RedirectToActionPermanent("Index");
                }

                Session[Constants.Session.State] = null;

                var client = new GitHubClient(new ProductHeaderValue("Jarboo.Protoypes.GithubPuller"));

                var token = await client.Oauth.CreateAccessToken(new OauthTokenRequest(_clientId, _clientSecret, code));

                var credentials = new Credentials(token.AccessToken);

                CurrentConnection = new Connection(new ProductHeaderValue("Jarboo.Protoypes.GithubPuller", "1.0"))
                {
                    Credentials = credentials
                };

                client = new GitHubClient(CurrentConnection);
                CurrentUser = await client.User.Current();
            }

            return RedirectToAction("Index", "Project");
        }
    }
}
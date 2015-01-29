using Jarboo.Protoypes.GithubPuller.Helpers;
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
    [Authorize]
    public class AuthenticationController : Controller
    {
        protected User CurrentUser = null;
        protected Connection CurrentConnection = null;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session[Constants.Session.CurrentUser] == null)
            {
                FormsAuthentication.SignOut();
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl + "?returnUrl=" +
                            filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.RawUrl));
            }

            this.CurrentUser = (User)Session[Constants.Session.CurrentUser];
            this.CurrentConnection = (Connection)Session[Constants.Session.CurrentConnection];
            filterContext.HttpContext.Items[Constants.Session.CurrentUser] = this.CurrentUser;
            base.OnActionExecuting(filterContext);
        }
    }
}
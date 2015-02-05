using System.Web.Mvc;
using Octokit;

namespace Jarboo.Protoypes.GithubPuller.Controllers
{
    
    public class BaseController : Controller
    {
        protected User CurrentUser
        {
            get { return (User) Session[Constants.Session.CurrentUser]; }
            set { Session[Constants.Session.CurrentUser] = value; }
        }


        protected Connection CurrentConnection
        {
            get
            {
                if (Session[Constants.Session.CurrentConnection] != null)
                {
                    return (Connection)Session[Constants.Session.CurrentConnection];
                }

                return null;
            }
            set { Session[Constants.Session.CurrentConnection] = value; }
        }
    }
}
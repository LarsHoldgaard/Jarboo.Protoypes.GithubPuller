using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Jarboo.Protoypes.GithubPuller.Helpers
{
    public class SecurityHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SecurityHelper));
        private static Random random = new Random();
        private static String passwordChars = "0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ!@#$*";

        public static String GeneratePassword(int numberOfChars)
        {
            StringBuilder sb = new StringBuilder(numberOfChars);
            for (int i = 0; i < numberOfChars; i++)
            {
                sb.Append(passwordChars[random.Next(passwordChars.Length)]);
            }
            return sb.ToString();
        }

        public static Octokit.User CurrentUser
        {
            get
            {
                HttpContext currentContext = HttpContext.Current;
                if (currentContext == null) return null;
                if (!currentContext.User.Identity.IsAuthenticated) return null;

                if (currentContext.Items[Constants.Session.CurrentUser] == null) return null;

                return (Octokit.User)currentContext.Items[Constants.Session.CurrentUser];
            }
        }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }   
    }
}
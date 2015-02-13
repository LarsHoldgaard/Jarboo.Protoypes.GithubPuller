using System.Linq;
using Microsoft.Web.Administration;
using NLog;

namespace Jarboo.Protoypes.GithubPuller.Services
{
    public class DeployService
    {
        private readonly string _rootApplication = System.Configuration.ConfigurationManager.AppSettings["DeployApplication"];
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string DeployApplication(string name, string path)
        {
            name = name.StartsWith("/") ? name : "/" + name;
            name = name.Replace(".", "").Replace(@"/", "");
            _logger.Debug("Output path: {0}", path);
            _logger.Debug("App name: {0}", name);
            using (var server = new ServerManager())
            {
                Site site = server.Sites.First(w => w.Name == _rootApplication);

                var existingApplications = site.Applications.Where(a => a.Path == name).ToList();

                //removing sites with this name if they exist
                foreach (var existing in existingApplications)
                {
                    site.Applications.Remove(existing);
                }

                site.Applications.Add(name, path);

                server.CommitChanges();
            }

            return name;
        }
    }
}
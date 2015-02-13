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
            name = name.Replace(".", "").Replace(@"/", "").Replace("-", "").ToLowerInvariant();
            name = "/" + name;
//            path = path + @"\";
//            path = @"D:\sites\githubpuller.jarboo.com\Downloads\635594332589268911Saxo.Websites.Tools\Web";
            _logger.Debug("Output path: {0}", path);
            _logger.Debug("App name: {0}", name);
            using (var server = new ServerManager())
            {
                Site site = server.Sites.First(w => w.Name == _rootApplication);

                _logger.Debug("Site found: {0}", site.Name);
                var existingApplications = site.Applications.Where(a => a.Path == name).ToList();

                _logger.Debug("Existing applications: {0}", existingApplications.Count);
                //removing sites with this name if they exist
                foreach (var existing in existingApplications)
                {
                    site.Applications.Remove(existing);
                }

                var app = site.Applications.Add(name, path);
                app.ApplicationPoolName = _rootApplication;

                server.CommitChanges();
            }

            return name;
        }
    }
}
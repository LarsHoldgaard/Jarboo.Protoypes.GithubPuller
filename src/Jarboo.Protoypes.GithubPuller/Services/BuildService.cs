using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using NLog;
using NuGetPlus;

namespace Jarboo.Protoypes.GithubPuller.Services
{
    public class BuildService
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Build(string buildFilePath, string outputPath, string configuration = "Release", string platform = "Any CPU")
        {
            /*bool restorePackages = false;

            if (restorePackages)
            {
                try
                {
                    NuGetPlus.SolutionManagement.RestorePackages(solutionPath);
                }
                catch (Exception e)
                {
                    _logger.Debug("Error when running restore command");
                    _logger.Debug(e);
                }

                _logger.Debug("Get restore packages");
                var packages = NuGetPlus.SolutionManagement.GetRestorePackages(solutionPath);
                foreach (var package in packages)
                {
                    _logger.Debug("Package {0}, id: {1}, version {2}", package.Item1.Item, package.Item2.Id, package.Item2.Version);
                    new RepositoryManagement.RestorePackage(package.Item2.Id, package.Item2.Version);
                }
            }

            _logger.Debug("Packages restored");*/

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe")
                {
                    Arguments =
                        string.Format(
                            @"""{2}"" /P:Configuration={0} /p:DeployOnBuild=True /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:TargetFolder=""{{1}}"" /p:TargetWebFolder=""{1}PrecompileWeb"" /p:DeleteExistingFiles=True /p:publishUrl=""{1}"" /P:Platform=""{3}""",
                            configuration, outputPath + @"\\", buildFilePath, platform),
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            p.Start();
            string buildOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return buildOutput;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.HostViewAdapters;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using Microsoft.ApplicationInsights;
using System.IO;
using System.AddIn.Hosting;
using System.Reflection;
using System.Deployment.Application;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensions
{
    public class ExtensionManager
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _tc;
        private readonly string _addInRoot;

        public ExtensionManager(ILogger logger, TelemetryClient tc)
        {
            _logger = logger;
            _tc = tc;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var s4nPath = Path.Combine(appDataPath, "ScratchForDotNet");
            _addInRoot = Path.Combine(s4nPath, "Extensibility");
        }

        public void Initialize()
        {
            EnsureDirectoryStructure();
            UpdatePipeline();
        }

        private void UpdatePipeline()
        {
            var warnings = AddInStore.Update(_addInRoot);
            var tokens = AddInStore.FindAddIns(typeof(IFirmwareConfiguration), _addInRoot);
        }

        private void EnsureDirectoryStructure()
        {
            var path = Path.Combine(_addInRoot, "AddIns");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(_addInRoot, "AddInSideAdapters");
            CopyTree("AddInSideAdapters", path);

            path = Path.Combine(_addInRoot, "AddInViews");
            CopyTree("AddInViews", path);

            path = Path.Combine(_addInRoot, "Contracts");
            CopyTree("Contracts", path);

            path = Path.Combine(_addInRoot, "HostSideAdapters");
            CopyTree("HostSideAdapters", path);
        }

        private void CopyTree(string assetDir, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            string dataDir;
            if (ApplicationDeployment.IsNetworkDeployed)
                dataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
            else
                dataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var source = Path.Combine(dataDir, @"Assets\Files", assetDir);

            var files = Directory.GetFiles(source);
            foreach (var file in files)
            {
                var destFile = Path.Combine(destination, Path.GetFileName(file));
                var fCopy = false;
                if (File.Exists(destFile))
                {
                    var sourceTS = File.GetCreationTimeUtc(file);
                    var destTS = File.GetCreationTimeUtc(destFile);
                    if (sourceTS > destTS)
                        fCopy = true;
                }
                else
                    fCopy = true;

                if (fCopy)
                    File.Copy(file, destFile, true);
            }
        }
    }
}

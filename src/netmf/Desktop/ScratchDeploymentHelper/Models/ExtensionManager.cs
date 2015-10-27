//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//-------------------------------------------------------------------------
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;

using PervasiveDigital.Scratch.DeploymentHelper.Extensibility;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.Common;
using System.Net;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class ExtensionManager
    {
        private readonly ILogger _logger;
        private readonly FirmwareManager _fwmgr;
        private readonly TelemetryClient _tc;
        private readonly string _cacheRoot;
        private readonly string _addInRoot;
        private readonly string _addInsDirectory;

        public ExtensionManager(ILogger logger, FirmwareManager fwmgr, TelemetryClient tc)
        {
            _logger = logger;
            _fwmgr = fwmgr;
            _tc = tc;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var s4nPath = Path.Combine(appDataPath, "ScratchForDotNet");
            _addInRoot = Path.Combine(s4nPath, "Extensibility");
            _cacheRoot = Path.Combine(s4nPath, "Cache");
            _addInsDirectory = Path.Combine(_addInRoot, "AddIns");
        }

        public async Task Initialize()
        {
            EnsureDirectoryStructure();
            await UpdatePlugins();
        }

        public async Task UpdatePlugins()
        {
            var files = new HashSet<string>();
            if (_fwmgr.Images != null)
            {
                foreach (var item in _fwmgr.Images)
                {
                    if (!string.IsNullOrEmpty(item.DriverSource) &&
                        !files.Contains(item.DriverSource))
                    {
                        files.Add(item.DriverSource);
                    }
                    if (!string.IsNullOrEmpty(item.ConfigurationExtensionSource) &&
                        !files.Contains(item.ConfigurationExtensionSource))
                    {
                        files.Add(item.ConfigurationExtensionSource);
                    }
                }
                if (Settings.Default.OnlineDataUpdates)
                    await this.UpdatePluginsFromInternet(files);
                else
                    this.UpdatePluginsFromInstallation(files);
                UpdatePipeline();
            }
        }

        private void UpdatePluginsFromInstallation(IEnumerable<string> fileNames)
        {
            string sourcePath;

            if (ApplicationDeployment.IsNetworkDeployed)
                sourcePath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            else
                sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var file in fileNames)
            {
                var sourceFilePath = Path.Combine(sourcePath,  @"Assets\Installation", Constants.ScratchExtensionsPath, file);
                var destPath = Path.Combine(_cacheRoot, file);

                var sourceLastWrite = File.GetLastWriteTimeUtc(sourceFilePath);
                var destLastWrite = File.GetLastWriteTimeUtc(destPath);

                if (destLastWrite < sourceLastWrite)
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);

                    File.Copy(sourceFilePath, destPath, true);

                    // Unzip into the correct location
                    var dest = Path.Combine(_addInsDirectory, Path.GetFileNameWithoutExtension(file));
                    if (Directory.Exists(dest))
                    {
                        Directory.Delete(dest, true);
                    }
                    Directory.CreateDirectory(dest);

                    ZipFile.ExtractToDirectory(destPath, dest);

                }
            }
        }

        private async Task UpdatePluginsFromInternet(IEnumerable<string> fileNames)
        {
            foreach (var file in fileNames)
            {
                string cachePath = null;
                try
                {
                    cachePath = Path.Combine(_cacheRoot, file);
                    var lastWrite = File.GetLastWriteTimeUtc(cachePath);

                    var source = Constants.S4NHost + Constants.ScratchExtensionsPath + file;

                    var tempFile = Path.GetTempFileName();

                    var req = (HttpWebRequest)WebRequest.Create(source);
                    req.IfModifiedSince = lastWrite;
                    var buffer = new byte[4096];
                    using (var response = (HttpWebResponse)await req.GetResponseAsync())
                    {
                        var output = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                        using (var stream = response.GetResponseStream())
                        {
                            int count = 0;
                            do
                            {
                                count = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (count > 0)
                                    output.Write(buffer, 0, count);
                            } while (count > 0);
                        }
                        output.Close();
                    }

                    if (File.Exists(cachePath))
                        File.Delete(cachePath);
                    File.Move(tempFile, cachePath);

                    // Unzip into the correct location
                    var dest = Path.Combine(_addInsDirectory, Path.GetFileNameWithoutExtension(file));
                    if (Directory.Exists(dest))
                    {
                        Directory.Delete(dest, true);
                    }
                    Directory.CreateDirectory(dest);

                    ZipFile.ExtractToDirectory(cachePath, dest);

                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        var status = ((HttpWebResponse)wex.Response).StatusCode;
                        if (status == HttpStatusCode.NotModified)
                        {
                            // no big deal - we have the latest copy
                        }
                        else
                        {
                            // error - file not updated
                            _tc.TrackException(wex);
                        }
                    }
                    else
                    {
                        // error - file not updated
                        _tc.TrackException(wex);
                    }
                }
                catch (Exception ex)
                {
                    // log it, but keep going
                    _tc.TrackException(ex);
                    // Make sure we don't retain a cached copy of something that did not install
                    if (!string.IsNullOrEmpty(cachePath))
                    {
                        try
                        {
                            File.Delete(cachePath);
                        }
                        catch { }
                    }
                }
            }
        }

        private void UpdatePipeline()
        {
            var warnings = AddInStore.Update(_addInRoot);
            var tokens = AddInStore.FindAddIns(typeof(IFirmwareConfiguration), _addInRoot);
        }

        public IDriver GetDriverForImage(FirmwareImage image)
        {
            IDriver result = null;

            if (image != null)
            {
                var tokens = AddInStore.FindAddIns(typeof(IDriver), _addInRoot);
                var driverToken = tokens.FirstOrDefault(x => x.Name == image.DriverName);
                if (driverToken != null)
                {
                    result = driverToken.Activate<IDriver>(AddInSecurityLevel.FullTrust);
                    // use AddInController.GetAddInController(result) to manage/unload/etc
                }
            }
            return result;
        }

        private void EnsureDirectoryStructure()
        {
            if (!Directory.Exists(_addInRoot))
                Directory.CreateDirectory(_addInRoot);
            if (!Directory.Exists(_cacheRoot))
                Directory.CreateDirectory(_cacheRoot);
            if (!Directory.Exists(_addInsDirectory))
                Directory.CreateDirectory(_addInsDirectory);

            var path = Path.Combine(_addInRoot, "AddInSideAdapters");
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
